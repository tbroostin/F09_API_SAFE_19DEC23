// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class RegistrationBillingRepository : BaseColleagueRepository, IRegistrationBillingRepository
    {
        private Domain.Finance.Entities.ImmediatePaymentControl _ipcConfig;
        private const string _Mnemonic = "SFIPC";
        private readonly string _colleagueTimeZone;

        public RegistrationBillingRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = settings.ColleagueTimeZone;

            // Get the IPC configuration
            _ipcConfig = GetImmediatePaymentControl();

            // Override the default cache value - unless otherwise specified, only cache data for 1 minute
            CacheTimeout = 1;
        }

        /// <summary>
        /// Get all the registration payment controls for a specified student
        /// </summary>
        /// <param name="studentId">Student</param>
        /// <returns>List of registration payment controls</returns>
        public IEnumerable<RegistrationPaymentControl> GetStudentPaymentControls(string studentId)
        {
            // First, see if IPC is enabled.  If not, return an empty list.
            if (!_ipcConfig.IsEnabled)
            {
                return new List<RegistrationPaymentControl>();
            }

            if (String.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID must be specified.");
            }

            try
            {
                var controls = new List<RegistrationPaymentControl>();

                string criteria = String.Format("IPCR.STUDENT EQ '{0}'", studentId);

                var ids = DataReader.Select("IPC.REGISTRATION", criteria).ToList();
                foreach (var id in ids)
                {
                    RefreshPaymentControl(id);
                    var ipcr = DataReader.ReadRecord<IpcRegistration>(id);
                    controls.Add(BuildPaymentControl(ipcr));
                }
                return controls;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a specified registration payment control
        /// </summary>
        /// <param name="id">Registration payment control ID</param>
        /// <returns>Registration payment control</returns>
        public RegistrationPaymentControl GetPaymentControl(string id)
        {
            // First, see if IPC is enabled.  If not, return a null value.
            if (!_ipcConfig.IsEnabled)
            {
                return null;
            }

            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Registration payment control ID must be specified.");
            }

            RefreshPaymentControl(id);

            IpcRegistration reg = DataReader.ReadRecord<IpcRegistration>(id);
            if (reg == null)
            {
                throw new KeyNotFoundException("Registration payment control ID " + id + " is not valid.");
            }

            return BuildPaymentControl(reg);
        }

        /// <summary>
        /// Get a specified registration billing
        /// </summary>
        /// <param name="id">Registration billing ID</param>
        /// <returns>Registration billing</returns>
        public RegistrationBilling GetRegistrationBilling(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Registration billing ID must be specified.");
            }

            RegArPostings regArPosting = DataReader.ReadRecord<RegArPostings>(id);

            if (regArPosting == null)
            {
                throw new ArgumentOutOfRangeException("id", "Registration billing ID " + id + " is not valid.");
            }

            var billing = new RegistrationBilling(regArPosting.Recordkey, regArPosting.RgarStudent, regArPosting.RgarArType,
                regArPosting.RgarBillingStartDate, regArPosting.RgarBillingEndDate, regArPosting.RgarInvoice,
                GetRegistrationBillingItems(regArPosting.RgarRegArPostingItems))
            {
                TermId = regArPosting.RgarTerm,
                AdjustmentId = regArPosting.RgarAdjByRegArPosting
            };

            return billing;
        }

        /// <summary>
        /// Get a list of registration billing items
        /// </summary>
        /// <param name="ids">List of item IDs</param>
        /// <returns>List of registration billing items</returns>
        public IEnumerable<RegistrationBillingItem> GetRegistrationBillingItems(IEnumerable<string> ids)
        {
            if (ids == null || ids.Count() == 0)
            {
                throw new ArgumentNullException("ids", "Billing item ids must be specified.");
            }

            Collection<RegArPostingItems> items = DataReader.BulkReadRecord<RegArPostingItems>(ids.ToArray());
            if (items == null || items.Count != ids.Count())
            {
                throw new ArgumentOutOfRangeException("ids", "Failed to retrieve all billing items from the database.");
            }

            Collection<StudentCourseSec> scs = DataReader.BulkReadRecord<StudentCourseSec>(items.Select(x => x.RgariStudentCourseSec).ToArray());
            if (scs == null || scs.Count != items.Count)
            {
                throw new ArgumentOutOfRangeException("ids", "Failed to retrieve all student course sections from the database.");
            }

            var billItems = new List<RegistrationBillingItem>();
            foreach (var item in items)
            {
                billItems.Add(new RegistrationBillingItem(item.Recordkey, scs.Select(x => x.ScsStudentAcadCred).FirstOrDefault()));
            }

            return billItems;
        }

        /// <summary>
        /// Update an existing registration payment control record
        /// </summary>
        /// <param name="regPmtControl">Registration payment control to update</param>
        /// <returns>Updated registration payment control</returns>
        public RegistrationPaymentControl UpdatePaymentControl(RegistrationPaymentControl regPmtControl)
        {
            if (regPmtControl == null)
            {
                throw new ArgumentNullException("regPmtControl", "Cannot update a null Registration Payment Control.");
            }

            try
            {
                // Determine the reg payment status
                string status = PutPaymentStatus(regPmtControl.PaymentStatus);

                // Execute the UpdateRegistrationPaymentControl transaction
                var request = new UpdateRegistrationPaymentControlRequest()
                {
                    PaymentIds = regPmtControl.Payments.ToList(),
                    PaymentControlId = regPmtControl.Id,
                    PaymentStatus = status
                };
                var response = transactionInvoker.Execute<UpdateRegistrationPaymentControlRequest, UpdateRegistrationPaymentControlResponse>(request);

                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    // Update failed
                    logger.Error(response.ErrorMessage);
                    throw new InvalidOperationException(response.ErrorMessage);
                }

                return GetPaymentControl(regPmtControl.Id);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a registration approval and updates a registration payment control with the newly-created approval
        /// </summary>
        /// <param name="acceptance">Registration approval information</param>
        /// <returns>Updated registration approval information</returns>
        public RegistrationTermsApproval ApproveRegistrationTerms(PaymentTermsAcceptance acceptance)
        {
            if (acceptance == null)
            {
                throw new ArgumentNullException("acceptance", "Payment terms acceptance cannot be null");
            }

            // Process the transaction to create the data needed
            var request = new ApproveRegistrationTermsRequest()
                {
                    StudentId = acceptance.StudentId,
                    PaymentControlId = acceptance.PaymentControlId,
                    AcknowledgementDate = acceptance.AcknowledgementDateTime.ToLocalDateTime(_colleagueTimeZone),
                    AcknowledgementTime = acceptance.AcknowledgementDateTime.ToLocalDateTime(_colleagueTimeZone),
                    InvoiceIds = acceptance.InvoiceIds.ToList(),
                    SectionIds = acceptance.SectionIds.ToList(),
                    AcknowledgementText = acceptance.AcknowledgementText.ToList(),
                    TermsText = acceptance.TermsText.ToList(),
                    ApprovalUserid = acceptance.ApprovalUserId,
                    ApprovalDate = acceptance.ApprovalReceived.ToLocalDateTime(_colleagueTimeZone),
                    ApprovalTime = acceptance.ApprovalReceived.ToLocalDateTime(_colleagueTimeZone)
                };

            try
            {
                var response = transactionInvoker.Execute<ApproveRegistrationTermsRequest, ApproveRegistrationTermsResponse>(request);

                if (!String.IsNullOrEmpty(response.ErrorMessage))
                {
                    throw new InvalidOperationException(response.ErrorMessage);
                }

                // Return the new approval
                return GetTermsApproval(response.RegistrationApprovalId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets all of the payment requirements for a given term
        /// </summary>
        /// <param name="termId">ID of the term for which to get payment requirements</param>
        /// <returns>A list of payment requirements for the term</returns>
        public IEnumerable<PaymentRequirement> GetPaymentRequirements(string termId)
        {
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "A Term must be provided");
            }

            string criteria = String.Format("IPCP.TERM EQ '{0}' BY IPCP.RULE.EVAL.ORDER", termId);
            Collection<IpcPaymentReqs> ipcps = DataReader.BulkReadRecord<IpcPaymentReqs>(criteria);

            var paymentRequirements = new List<PaymentRequirement>();
            if (ipcps == null || ipcps.Count() == 0)
            {
                // No payment requirements defined for the term - return an empty list
                return paymentRequirements;
            }

            foreach (var ipcp in ipcps)
            {
                paymentRequirements.Add(BuildPaymentRequirement(ipcp));
            }

            return paymentRequirements;
        }

        /// <summary>
        /// Evaluate a student's payment requirements
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="paymentRequirements">List of payment requirements</param>
        /// <returns>The payment requirement applicable to this student</returns>
        //public PaymentRequirement EvaluatePaymentRequirements(string studentId, IEnumerable<PaymentRequirement> paymentRequirements)
        //{
        //    if(string.IsNullOrEmpty(studentId))
        //    {
        //        throw new ArgumentNullException("studentId","A Student ID must be provided");
        //    }

        //    if (paymentRequirements == null || paymentRequirements.Count() == 0)
        //    {
        //        throw new ArgumentNullException("paymentRequirements", "At least one payment requirement must be provided.");
        //    }

        //    List<string> ruleIds = new List<string>();
        //    List<RuleResult> ruleResults = new List<RuleResult>();
        //    AccountHolder acctHolder = _arRepository.GetAccountHolder(studentId);

        //    // Set the payment requirement to the default payment requirement for the term (no associated rule)
        //    PaymentRequirement paymentReq = paymentRequirements.First(x => x.ProcessingOrder == 0);

        //    // Evaluate each payment requirement with a rule to see if the Account Holder passes the rule
        //    foreach (var pmtReq in paymentRequirements)
        //    {
        //        // Ignore the default payment requirement; it has no associated rule
        //        if (!string.IsNullOrEmpty(pmtReq.EligibilityRuleId))
        //        {
        //            Rule<AccountHolder> acctHldrRule = new Rule<AccountHolder>(pmtReq.EligibilityRuleId);
        //            List<RuleRequest<AccountHolder>> acctHldrRequests = new List<RuleRequest<AccountHolder>>() { new RuleRequest<AccountHolder>(acctHldrRule, acctHolder) };
        //            ruleResults = _ruleRepository.Execute(acctHldrRequests).ToList();

        //            // If the Account Holder passes the rule, use the associated Payment Requirement
        //            if (ruleResults[0].Passed)
        //            {
        //                return pmtReq;
        //            }
        //        }
        //    }
        //    return paymentReq;
        //}

        /// <summary>
        /// Start the registration payment process
        /// </summary>
        /// <param name="payment">Payment information</param>
        /// <returns>Payment provider information</returns>
        public PaymentProvider StartRegistrationPayment(Payment payment)
        {
            if (payment == null)
            {
                throw new ArgumentNullException("payment", "Cannot start a null payment.");
            }

            // Execute the StartStudentPayment transaction
            var request = new StartStudentPaymentRequest() 
                {
                    InPersonId = payment.PersonId,
                    InMnemonic = _Mnemonic,
                    InReturnUrl = payment.ReturnUrl,
                    InDistribution = payment.Distribution,
                    InPayMethod = payment.PayMethod,
                    InAmtToPay = payment.AmountToPay,
                    InProviderAcct = payment.ProviderAccount,
                    InConvFee = payment.ConvenienceFee,
                    InConvFeeAmt = payment.ConvenienceFeeAmount,
                    InConvFeeGlNo = payment.ConvenienceFeeGeneralLedgerNumber,
                    InSfipcReturnUrl = payment.ReturnToOriginUrl
                };

            // Build the list of payment items on the request
            foreach (PaymentItem paymentItem in payment.PaymentItems)
            {
                request.InPayments.Add(new InPayments()
                    {
                        InPmtAmts = paymentItem.PaymentAmount,
                        InPmtDescs = paymentItem.Description,
                        InPmtArTypes = paymentItem.AccountType,
                        InPmtTerms = paymentItem.Term,
                        InPmtInvoices = paymentItem.InvoiceId,
                        InPmtPlans = paymentItem.PaymentPlanId,
                        InPmtOverdues = paymentItem.Overdue,
                        InPmtDepositsDue = paymentItem.DepositDueId,
                        InSfipcRegControlId = paymentItem.PaymentControlId,
                        InSfipcPmtComplete = paymentItem.PaymentComplete ? "Y" : "N",
                    }
                );
            }

            try
            {
                StartStudentPaymentResponse response = transactionInvoker.Execute<StartStudentPaymentRequest, StartStudentPaymentResponse>(request);

                if (!String.IsNullOrEmpty(response.OutErrorMsg))
                {
                    // Payment post failed
                    logger.Error(response.OutErrorMsg);
                    throw new InvalidOperationException(response.OutErrorMsg);
                }

                PaymentProvider pmtProvider = new PaymentProvider();
                pmtProvider.RedirectUrl = response.OutStartUrl;

                return pmtProvider;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

        }

        /// <summary>
        /// Get a registration approval
        /// </summary>
        /// <param name="id">ID of approval</param>
        /// <returns>The registration approval information</returns>
        public RegistrationTermsApproval GetTermsApproval(string id)
        {
            
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Approval ID is required.");
            }

            IpcRegApprovals regApproval = DataReader.ReadRecord<IpcRegApprovals>(id);
            if (regApproval == null)
            {
                throw new KeyNotFoundException("Specified ID is invalid: " + id);
            }

            // Get other data elements
            var payControl = GetPaymentControl(regApproval.IpcraRegistration);
            DateTimeOffset ackDateTime = regApproval.IpcraAckTime.ToPointInTimeDateTimeOffset(regApproval.IpcraAckDate, _colleagueTimeZone).GetValueOrDefault();
            // Create the entity and return it
            var approval = new RegistrationTermsApproval(id, payControl.StudentId, ackDateTime, payControl.Id, payControl.RegisteredSectionIds,
                payControl.InvoiceIds, regApproval.IpcraTermsResponse)
                {
                    AcknowledgementDocumentId = regApproval.IpcraAckDocument
                };

            return approval;
        }

        #region Private methods
        /// <summary>
        /// Refreshes the active list if SCS records for the Payment Control record
        /// </summary>
        /// <param name="id">The Payment Control record of interest</param>
        private void RefreshPaymentControl(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Registration payment control ID must be specified.");
            }

            // Execute the RefreshIpcRegistration transaction
            var request = new RefreshIpcRegistrationRequest()
            {
                IpcRegistrationId = id,
            };
            try
            {
                var response = transactionInvoker.Execute<RefreshIpcRegistrationRequest, RefreshIpcRegistrationResponse>(request);

                if (response.ErrorMessages != null && response.ErrorMessages.Count > 0)
                {
                    // Update failed
                    logger.Error(response.ErrorMessages.ToString());
                    throw new InvalidOperationException(String.Join("\n", response.ErrorMessages));
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

        }

        /// <summary>
        /// Build a registration payment control
        /// </summary>
        /// <param name="source">An <see cref="IpcRegistration"/> object</param>
        /// <returns>A RegistrationPaymentControl object</returns>
        private RegistrationPaymentControl BuildPaymentControl(IpcRegistration source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source IpcRegistration record must be provided.");
            }

            Collection<StudentCourseSec> scsList = null;
            if (source.IpcrActiveScs != null && source.IpcrActiveScs.Count > 0)
            {
                scsList = DataReader.BulkReadRecord<StudentCourseSec>(source.IpcrActiveScs.ToArray());
                if (scsList == null || scsList.Count != source.IpcrActiveScs.Count)
                {
                    throw new ArgumentOutOfRangeException("source", "Failed to retrieve all active sections for ID " + source.Recordkey);
                }
            }

            var control = new RegistrationPaymentControl(source.Recordkey, source.IpcrStudent, source.IpcrTerm, GetPaymentStatus(source.IpcrPayStatus))
                {
                    PaymentPlanId = source.IpcrArPayPlan
                };

            if (scsList != null && scsList.Count > 0)
            {
                foreach (var scs in scsList)
                {
                    control.AddRegisteredSection(scs.ScsCourseSection);
                    control.AddAcademicCredit(scs.ScsStudentAcadCred);
                }
            }

            if (source.IpcrRegInvoices != null && source.IpcrRegInvoices.Count > 0)
            {
                foreach (var inv in source.IpcrRegInvoices)
                {
                    control.AddInvoice(inv);
                }
            }

            if (source.IpcrPayments != null && source.IpcrPayments.Count > 0)
            {
                foreach (var pmt in source.IpcrPayments)
                {
                    control.AddPayment(pmt);
                }
            }

            string criteria = String.Format("IPCRA.REGISTRATION EQ '{0}' BY.DSND IPCRA.APPROVAL.DATE BY.DSND IPCRA.APPROVAL.TIME", control.Id);
            string[] approvalIds = DataReader.Select("IPC.REG.APPROVALS", criteria);
            if (approvalIds != null && approvalIds.Length > 0)
            {
                control.LastTermsApprovalId = approvalIds[0];
            }

            string planCriteria = String.Format("PPA.IPC.REGISTRATION EQ '{0}' AND PPA.PLAN.CURRENT.STATUS EQ 'O''P' BY.DSND PPA.APPROVAL.DATE BY.DSND PPA.APPROVAL.TIME", control.Id);
            string[] planApprovalIds = DataReader.Select("PAY.PLAN.APPROVALS", planCriteria);
            if (planApprovalIds != null && planApprovalIds.Length > 0)
            {
                control.LastPlanApprovalId = planApprovalIds[0];
            }

            return control;
        }

        /// <summary>
        /// Convert the Colleague payment status into the corresponding RegistrationPaymentStatus
        /// </summary>
        /// <param name="sourceStatus">Colleague payment status</param>
        /// <returns>The RegistrationPaymentStatus</returns>
        private RegistrationPaymentStatus GetPaymentStatus(string sourceStatus)
        {
            // Determine the reg payment status
            RegistrationPaymentStatus status = RegistrationPaymentStatus.New;
            if (!String.IsNullOrEmpty(sourceStatus))
            {
                switch (sourceStatus)
                {
                    case "NEW":
                        status = RegistrationPaymentStatus.New;
                        break;
                    case "ACCEPT":
                        status = RegistrationPaymentStatus.Accepted;
                        break;
                    case "COMPLETE":
                        status = RegistrationPaymentStatus.Complete;
                        break;
                    case "ERROR":
                        status = RegistrationPaymentStatus.Error;
                        break;
                }
            }

            return status;
        }

        /// <summary>
        /// Convert the RegistrationPaymentStatus into the corresponding Colleague payment status
        /// </summary>
        /// <param name="sourceStatus">The RegistrationPaymentStatus</param>
        /// <returns>Colleague payment status code</returns>
        private string PutPaymentStatus(RegistrationPaymentStatus sourceStatus)
        {
            string status = null;
            switch (sourceStatus)
            {
                case RegistrationPaymentStatus.New:
                    status = "NEW";
                    break;
                case RegistrationPaymentStatus.Accepted:
                    status = "ACCEPT";
                    break;
                case RegistrationPaymentStatus.Complete:
                    status = "COMPLETE";
                    break;
                case RegistrationPaymentStatus.Error:
                    status = "ERROR";
                    break;
            }

            return status;
        }

        /// <summary>
        /// Build a PaymentRequirement object
        /// </summary>
        /// <param name="source">A <see cref="IpcPaymentReqs"/> object</param>
        /// <returns>The PaymentRequirement object</returns>
        private PaymentRequirement BuildPaymentRequirement(IpcPaymentReqs source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source IpcPaymentReqs record must be provided.");
            }
            
            List<PaymentDeferralOption> deferralOptions = new List<PaymentDeferralOption>();
            
            // Build the list of deferral options for the payment requirement
            if (source.IpcpDeferralsEntityAssociation != null && source.IpcpDeferralsEntityAssociation.Count() > 0)
            {
                foreach (var defOption in source.IpcpDeferralsEntityAssociation)
                {
                    deferralOptions.Add(new PaymentDeferralOption(defOption.IpcpDeferEffectiveStartAssocMember.Value, 
                        defOption.IpcpDeferEffectiveEndAssocMember, 
                        (defOption.IpcpDeferPctAssocMember == null ? 0 : defOption.IpcpDeferPctAssocMember.Value)));
                }
            }

            List<PaymentPlanOption> paymentPlanOptions = new List<PaymentPlanOption>();

            // Build the list of payment plan options for the payment requirement
            if (source.IpcpPayPlansEntityAssociation != null && source.IpcpPayPlansEntityAssociation.Count > 0)
            {
                foreach (var payPlanOption in source.IpcpPayPlansEntityAssociation)
                {
                    paymentPlanOptions.Add(new PaymentPlanOption(payPlanOption.IpcpPlanEffectiveStartAssocMember.Value, 
                        payPlanOption.IpcpPlanEffectiveEndAssocMember.Value, 
                        payPlanOption.IpcpPayPlanTemplateAssocMember, 
                        payPlanOption.IpcpPlanStartDateAssocMember.GetValueOrDefault()));
                }
            }

            // Build the payment requirement
            var pmtReq = new PaymentRequirement(source.Recordkey, source.IpcpTerm, source.IpcpEligibilityRule, (int)source.IpcpRuleEvalOrder.Value, deferralOptions, paymentPlanOptions);
            return pmtReq;
        }

        /// <summary>
        /// Get the control information for Immediate Payment
        /// </summary>
        /// <returns>Immediate Payment configuration</returns>
        private Domain.Finance.Entities.ImmediatePaymentControl GetImmediatePaymentControl()
        {
            return GetOrAddToCache<Domain.Finance.Entities.ImmediatePaymentControl>("ImmediatePaymentControl",
                () =>
                {
                    var ipc = DataReader.ReadRecord<DataContracts.ImmediatePaymentControl>("ST.PARMS", "IMMEDIATE.PAYMENT.CONTROL");
                    if (ipc == null)
                    {
                        // IPC is not set up - just return the enabled flag as false
                        return new Domain.Finance.Entities.ImmediatePaymentControl(false);
                    }

                    bool isEnabled = String.IsNullOrEmpty(ipc.IpcEnabled) ? false : ipc.IpcEnabled == "Y";
                    var immediatePaymentControl = new Domain.Finance.Entities.ImmediatePaymentControl(isEnabled)
                    {
                        RegistrationAcknowledgementDocumentId = ipc.IpcRegAcknowledgementDoc,
                        TermsAndConditionsDocumentId = ipc.IpcTermsAndConditionsDoc,
                        DeferralAcknowledgementDocumentId = ipc.IpcDeferralDoc
                    };

                    return immediatePaymentControl;
                });
        }

        #endregion

    }
}
