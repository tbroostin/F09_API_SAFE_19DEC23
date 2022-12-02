// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class PaymentPlanRepository : BaseColleagueRepository, IPaymentPlanRepository
    {
        private readonly string _colleagueTimeZone;

        public PaymentPlanRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _colleagueTimeZone = settings.ColleagueTimeZone;
            // Override the default cache value - unless otherwise specified, only cache data for 1 minute
            CacheTimeout = 1;
        }

        /// <summary>
        /// List of all payment plan templates
        /// </summary>
        public IEnumerable<PaymentPlanTemplate> PaymentPlanTemplates
        {
            get
            {
                return GetOrAddToCache("PaymentPlanTemplates",
                    () =>
                    {
                        List<PaymentPlanTemplate> paymentPlanTemplateEntities = new List<PaymentPlanTemplate>();
                        Collection<PayPlanTemplates> payPlanTemplates = DataReader.BulkReadRecord<PayPlanTemplates>(string.Empty);

                        if (payPlanTemplates != null && payPlanTemplates.Count > 0)
                        {
                            foreach (var ppt in payPlanTemplates)
                            {
                                try
                                {
                                    paymentPlanTemplateEntities.Add(BuildPaymentPlanTemplate(ppt));
                                }
                                catch (Exception ex)
                                {
                                    LogDataError("PAY.PLAN.TEMPLATES", ppt.Recordkey, ppt, ex);
                                }
                            }
                        }
                        return paymentPlanTemplateEntities;
                    }, Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// Get a specified payment plan template
        /// </summary>
        /// <param name="templateId">ID of the payment plan template</param>
        /// <returns>A PaymentPlanTemplate entity</returns>
        public PaymentPlanTemplate GetTemplate(string templateId)
        {
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template ID cannot be null/empty.");
            }
            IEnumerable<PaymentPlanTemplate> paymentPlanTemplates = PaymentPlanTemplates;
            PaymentPlanTemplate paymentPlanTemplate = paymentPlanTemplates.Where(payPlanTemplate => payPlanTemplate.Id == templateId).FirstOrDefault();
            if (paymentPlanTemplate == null)
            {
                throw new KeyNotFoundException("Payment plan template with ID " + templateId + " not found.  Either this template does not exist or the PAY.PLAN.TEMPLATES record is invalid.");
            }

            return paymentPlanTemplate;
        }

        /// <summary>
        /// Get a specified payment plan
        /// </summary>
        /// <param name="paymentPlanId">ID of the payment plan</param>
        /// <returns>A PaymentPlan entity</returns>
        public PaymentPlan GetPaymentPlan(string paymentPlanId)
        {
            if (string.IsNullOrEmpty(paymentPlanId))
            {
                throw new ArgumentNullException("paymentPlanId", "Plan ID must be specified.");
            }

            ArPayPlans plan = DataReader.ReadRecord<ArPayPlans>(paymentPlanId);
            if (plan == null)
            {
                throw new KeyNotFoundException("Payment Plan ID " + paymentPlanId + " is not valid.");
            }

            var scheduledPayments = new Collection<ArPayPlanItems>();
            if (plan.ArplPayPlanItems != null && plan.ArplPayPlanItems.Count > 0)
            {
                scheduledPayments = DataReader.BulkReadRecord<ArPayPlanItems>(plan.ArplPayPlanItems.ToArray());
                if (plan.ArplPayPlanItems.Count != scheduledPayments.Count)
                {
                    var invalidIds = plan.ArplPayPlanItems.Except(scheduledPayments.Select(x => x.Recordkey));
                    throw new KeyNotFoundException("Plan " + paymentPlanId + " has missing plan items: " + string.Join(", ", invalidIds));
                }
            }

            var planInvoiceItems = new Collection<ArInvoiceItems>();
            if (plan.ArplInvoiceItems != null && plan.ArplInvoiceItems.Count > 0)
            {
                planInvoiceItems = DataReader.BulkReadRecord<ArInvoiceItems>(plan.ArplInvoiceItems.ToArray());
                if (plan.ArplInvoiceItems.Count != planInvoiceItems.Count)
                {
                    var invalidIds = plan.ArplInvoiceItems.Except(planInvoiceItems.Select(x => x.Recordkey));
                    throw new KeyNotFoundException("Plan " + paymentPlanId + " has missing invoice items: " + string.Join(", ", invalidIds));
                }
            }

            var payInvoiceItems = new Collection<ArPayInvoiceItems>();
            if (planInvoiceItems.Count > 0)
            {
                var keys = planInvoiceItems.Select(x => plan.Recordkey + "*" + x.Recordkey).ToArray();
                payInvoiceItems = DataReader.BulkReadRecord<ArPayInvoiceItems>(keys);
            }

            return BuildPaymentPlan(plan, scheduledPayments, planInvoiceItems, payInvoiceItems);
        }

        /// <summary>
        /// Get the payment plan schedule dates using a custom frequency subroutine
        /// </summary>
        /// <param name="personId">ID of the person for whom the plan dates are being calculated</param>
        /// <param name="receivableType">Receivable Type of the plan for which dates are being calculated</param>
        /// <param name="termId">ID of the term for which plan dates are being calculated</param>
        /// <param name="templateId">ID of the template used in creating the payment plan</param>
        /// <param name="downPaymentDate">Date on which down payment is due</param>
        /// <param name="firstPaymentDate">Date on which first payment (after down payment) is due</param>
        /// <param name="planId">Optional ID of the payment plan</param>
        /// <returns>A list of dates</returns>
        public IEnumerable<DateTime?> GetPlanCustomScheduleDates(string personId, string receivableType, string termId, string templateId, DateTime? downPaymentDate, DateTime firstPaymentDate, string planId = null)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID must be provided to get plan custom schedule dates.");
            }
            if (string.IsNullOrEmpty(receivableType))
            {
                throw new ArgumentNullException("receivableType", "Receivable Type must be provided to get plan custom schedule dates.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "Term ID must be provided to get plan custom schedule dates.");
            }
            if (string.IsNullOrEmpty(templateId))
            {
                throw new ArgumentNullException("templateId", "Template ID must be provided to get plan custom schedule dates.");
            }

            // Build the transaction request
            var request = new GetPlanCustomScheduleDatesRequest()
            {
                PersonId = personId,
                ArType = receivableType,
                TermId = termId,
                TemplateId = templateId,
                DownPmtDate = downPaymentDate,
                FirstPmtDate = firstPaymentDate,
                PlanId = planId,
            };

            try
            {
                // Execute the transaction
                var response = transactionInvoker.Execute<GetPlanCustomScheduleDatesRequest, GetPlanCustomScheduleDatesResponse>(request);

                // Check for errors from the response
                if (!String.IsNullOrEmpty(response.ErrorMsg))
                {
                    throw new InvalidOperationException("Error getting plan custom schedule dates: " + response.ErrorMsg);
                }

                // Build the list of plan custom schedule dates in order: (1) Down Payment Date, (2) First Payment Date, (3) any additional dates
                List<DateTime?> scheduleDates = new List<DateTime?>();
                scheduleDates.Add(response.DownPmtDate);
                scheduleDates.Add(response.FirstPmtDate);
                scheduleDates.AddRange(response.OutAddnlPmtDates);

                return scheduleDates;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

        }

        /// <summary>
        /// Creates a payment plan terms approval
        /// </summary>
        /// <param name="acceptance">Payment Plan approval information</param>
        /// <returns>Updated Payment Plan approval information</returns>
        public PaymentPlanApproval ApprovePaymentPlanTerms(PaymentPlanTermsAcceptance acceptance)
        {
            if (acceptance == null)
            {
                throw new ArgumentNullException("acceptance", "Payment Plan terms acceptance must be provided.");
            }

            // Process the transaction to create the data needed
            var request = new ApprovePaymentPlanTermsRequest()
            {
                StudentId = acceptance.StudentId,
                StudentName = acceptance.StudentName,
                PaymentControlId = acceptance.PaymentControlId,
                AcknowledgementDate = acceptance.AcknowledgementDateTime.Date,
                AcknowledgementTime = acceptance.AcknowledgementDateTime.ToLocalDateTime(_colleagueTimeZone),
                PlanTemplateId = acceptance.ProposedPlan.TemplateId,
                PlanReceivableType = acceptance.ProposedPlan.ReceivableTypeCode,
                PlanTerm = acceptance.ProposedPlan.TermId,
                PlanFirstPaymentDate = acceptance.ProposedPlan.FirstDueDate,
                PlanFrequency = PutPlanFrequency(acceptance.ProposedPlan.Frequency),
                PlanNumberOfPayments = acceptance.ProposedPlan.NumberOfPayments,
                PlanGraceDays = acceptance.ProposedPlan.GraceDays,
                PlanSetupChargeAmount = acceptance.ProposedPlan.TotalSetupChargeAmount,
                PlanSetupChargePercent = acceptance.ProposedPlan.SetupPercentage,
                PlanTotalAmount = acceptance.ProposedPlan.CurrentAmount,
                PlanLateChargeAmount = acceptance.ProposedPlan.LateChargeAmount,
                PlanLateChargePercent = acceptance.ProposedPlan.LateChargePercentage,
                PlanDownPaymentPercent = acceptance.ProposedPlan.DownPaymentPercentage,
                PlanDownPaymentAmount = acceptance.DownPaymentAmount,
                PlanDownPaymentDate = acceptance.DownPaymentDate,
                AcknowledgementText = acceptance.AcknowledgementText.ToList(),
                TermsText = acceptance.TermsText.ToList(),
                ApprovalUserid = acceptance.ApprovalUserId,
                ApprovalDate = acceptance.ApprovalReceived.Date,
                ApprovalTime = acceptance.ApprovalReceived.ToLocalDateTime(_colleagueTimeZone),
                ScheduledPayments = new List<ScheduledPayments>(),
                PlanInvoiceItems = new List<PlanInvoiceItems>(),
                IpcRegApprovalsId = acceptance.RegistrationApprovalId
            };

            if (acceptance.ProposedPlan.ScheduledPayments != null && acceptance.ProposedPlan.ScheduledPayments.Count() > 0)
            {
                foreach (var payment in acceptance.ProposedPlan.ScheduledPayments)
                {
                    request.ScheduledPayments.Add(new ScheduledPayments() { ScheduledPaymentDates = payment.DueDate, ScheduledPaymentAmounts = payment.Amount });
                }
            }

            if (acceptance.ProposedPlan.PlanCharges != null && acceptance.ProposedPlan.PlanCharges.Count() > 0)
            {
                foreach (var planCharge in acceptance.ProposedPlan.PlanCharges)
                {
                    request.PlanInvoiceItems.Add(new PlanInvoiceItems()
                        {
                            PlanInvoiceItemIds = planCharge.Charge.Id,
                            PlanInvoiceItemBalances = planCharge.Charge.Amount,
                            PlanInvoiceItemPlanAmounts = planCharge.Amount,
                            PlanInvoices = planCharge.Charge.InvoiceId
                        });
                }
            }

            try
            {
                var response = transactionInvoker.Execute<ApprovePaymentPlanTermsRequest, ApprovePaymentPlanTermsResponse>(request);

                if (!String.IsNullOrEmpty(response.ErrorMsg))
                {
                    throw new InvalidOperationException(response.ErrorMsg);
                }

                // Create the new object so to be returned
                var planApproval = GetPaymentPlanApproval(response.PayPlanApprovalsId);

                return planApproval;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a payment plan approval
        /// </summary>
        /// <param name="approvalId">ID of plan approval</param>
        /// <returns>The payment plan approval information</returns>
        public PaymentPlanApproval GetPaymentPlanApproval(string approvalId)
        {
            if (String.IsNullOrEmpty(approvalId))
            {
                throw new ArgumentNullException("approvalId", "Approval response ID is required.");
            }

            try
            {
                PayPlanApprovals planApproval = DataReader.ReadRecord<PayPlanApprovals>(approvalId);

                if (planApproval == null)
                {
                    throw new KeyNotFoundException("Specified approval response ID is invalid: " + approvalId);
                }
                //var payPlan = GetPaymentPlan(planApproval.PpaPayPlan);

                // We need the acknowledgement date and time as a DateTimeOffset
                DateTimeOffset ackDateTime = planApproval.PpaAckTime.ToPointInTimeDateTimeOffset(planApproval.PpaAckDate, _colleagueTimeZone).GetValueOrDefault();

                var planSchedule = new List<PlanSchedule>();
                foreach (var item in planApproval.PpaSchedulesEntityAssociation)
                {
                    planSchedule.Add(new PlanSchedule(item.PpaDueDateAssocMember.GetValueOrDefault(), item.PpaDueAmountAssocMember.GetValueOrDefault()));
                }
                // TODO JTM - If this is where the failure is occurring for some reason, use the following code that should work the same
                //for (int i = 0; i < planApproval.PpaDueDate.Count; i++)
                //{
                //    planSchedule.Add(new PlanSchedule(planApproval.PpaDueDate[i].GetValueOrDefault(), planApproval.PpaDueAmount[i].GetValueOrDefault());
                //}


                // TODO: JTM/JPM2 - Add the student/person ID to PAY.PLAN.APPROVALS, then it should go in the constructors 
                // of both PaymentPlanTermsAcceptance and PaymentPlanApproval, if it isn't already there.


                // Create the entity and return it
                var payPlanApproval = new PaymentPlanApproval(planApproval.Recordkey, planApproval.PpaStudent, planApproval.PpaStudentName, ackDateTime, planApproval.PpaTemplateId,
                    planApproval.PpaPayPlan, planApproval.PpaTermsApprDocResponse, planApproval.PpaTotalPlanAmt.GetValueOrDefault(),
                    planSchedule)
                {
                    PaymentControlId = planApproval.PpaIpcRegistration,
                    AcknowledgementDocumentId = planApproval.PpaAckApprovalDocument,
                    DownPaymentAmount = planApproval.PpaDownPaymentAmt.GetValueOrDefault(),
                    DownPaymentDate = planApproval.PpaDownPaymentDate.GetValueOrDefault(),
                    SetupChargeAmount = planApproval.PpaSetupChargeAmt.GetValueOrDefault(),
                    Frequency = ConvertToFrequencyEntity(planApproval.PpaFrequency),
                    NumberOfPayments = planApproval.PpaNumberOfPayments.GetValueOrDefault(),
                    GraceDays = planApproval.PpaGraceDays.GetValueOrDefault(),
                    LateChargeAmount = planApproval.PpaLateChargeAmt.GetValueOrDefault(),
                    LateChargePercentage = planApproval.PpaLateChargePct.GetValueOrDefault(),
                };

                return payPlanApproval;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a proposed payment plan for a given person, term, receivable type, amount, and first payment date
        /// </summary>
        /// <param name="personId">Proposed plan owner ID</param>
        /// <param name="termId">Billing term ID</param>
        /// <param name="receivableTypeCode">Receivable Type Code</param>
        /// <param name="paymentPlanTemplateId">Payment Plan template ID</param>
        /// <param name="firstPaymentDate">Date on which first scheduled payment will be due</param>
        /// <param name="planAmount">Maximum total payment plan charges</param>       
        /// <returns>Proposed payment plan</returns>
        public async Task<PaymentPlan> GetProposedPaymentPlanAsync(string personId, string termId,
            string receivableTypeCode, string paymentPlanTemplateId, DateTime firstPaymentDate, decimal planAmount)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person ID must be supplied.");
            }
            if (string.IsNullOrEmpty(termId))
            {
                throw new ArgumentNullException("termId", "A term ID must be supplied.");
            }
            if (string.IsNullOrEmpty(receivableTypeCode))
            {
                throw new ArgumentNullException("receivableTypeCode", "A receivable type code must be supplied.");
            }
            if (string.IsNullOrEmpty(paymentPlanTemplateId))
            {
                throw new ArgumentNullException("paymentPlanTemplateId", "A payment plan template ID must be supplied.");
            }

            var request = new GetProposedPaymentPlanInfoRequest()
            {
                InArType = receivableTypeCode,
                InTermId = termId,
                InPersonId = personId,
                InPayPlanTemplateId = paymentPlanTemplateId,
                InFirstPmtDate = firstPaymentDate,
                InPlanAmt = planAmount
            };
            List<Charge> planCharges = new List<Charge>();
            List<Invoice> planInvoices = new List<Invoice>();
            try
            {
                var response = await transactionInvoker.ExecuteAsync<GetProposedPaymentPlanInfoRequest, GetProposedPaymentPlanInfoResponse>(request);
                if (response != null)
                {
                    if (response.OutErrorMessages != null && response.OutErrorMessages.Any())
                    {
                        response.OutErrorMessages.ForEach(error => logger.Error(error));
                        throw new ApplicationException("Transaction GET.PROPOSED.PAYMENT.PLAN.INFO had one or more errors: " + string.Join(Environment.NewLine, response.OutErrorMessages));
                    }
                    ArPayPlans planData = new ArPayPlans()
                    {
                        Recordkey = response.OutPlanId,
                        ArplAmt = response.OutPlanAmount,
                        ArplPayPlanTemplate = paymentPlanTemplateId,
                        ArplPersonId = personId,
                        ArplArType = receivableTypeCode,
                        ArplTerm = termId,
                        ArplOrigAmt = response.OutPlanAmount,
                        ArplFirstDueDate = firstPaymentDate,
                        ArplFrequency = response.OutPlanFrequency,
                        ArplNoPayments = response.OutPlanPmtsCount,
                        ArplChargeAmt = response.OutPlanSetupChgAmt,
                        ArplChargePct = response.OutPlanSetupChgPct,
                        ArplDownPayPct = response.OutPlanDownPayPct,
                        ArplGraceNoDays = response.OutPlanGraceDays,
                        ArplLateChargeAmt = response.OutPlanLateChgAmt,
                        ArplPlanLateChrgPct = response.OutPlanLateChgPct
                    };
                    Collection<ArPayPlanItems> scheduledPaymentData = new Collection<ArPayPlanItems>();
                    Collection<ArInvoiceItems> invoiceItemsData = new Collection<ArInvoiceItems>();
                    Collection<ArPayInvoiceItems> payInvoiceItemsData = new Collection<ArPayInvoiceItems>();

                    if (response.ProposedScheduledPayments != null && response.ProposedScheduledPayments.Any())
                    {
                        response.ProposedScheduledPayments.ForEach(sp => scheduledPaymentData.Add(new ArPayPlanItems()
                        {
                            Recordkey = sp.OutSchedPmtIds,
                            ArpliAmt = sp.OutSchedPmtAmts,
                            ArpliDueDate = sp.OutSchedPmtDueDates,
                            ArpliPayPlan = response.OutPlanId
                        }));
                    }

                    if (response.ProposedInvoiceItems != null && response.ProposedInvoiceItems.Any())
                    {
                        response.ProposedInvoiceItems.ForEach(sp => invoiceItemsData.Add(new ArInvoiceItems()
                        {
                            Recordkey = sp.OutArInvoiceItemIds,
                            InviInvoice = sp.OutArInvoiceItemInvoiceIds,
                            InviDesc = sp.OutArInvoiceItemDescs,
                            InviArCode = sp.OutArInvoiceItemArCodes,
                            InviExtChargeAmt = sp.OutArInvoiceItemChargeAmts,
                            InviExtCrAmt = sp.OutArInvoiceItemCreditAmts
                        }));
                    }

                    if (response.ProposedPaymentPlanCharges != null && response.ProposedPaymentPlanCharges.Any())
                    {
                        response.ProposedPaymentPlanCharges.ForEach(sp => payInvoiceItemsData.Add(new ArPayInvoiceItems()
                        {
                            Recordkey = sp.OutPlanItemInvItemIds,
                            ArpliiAmt = sp.OutPlanItemAmounts,
                            ArpliiSetupInvoiceFlag = sp.OutPlanItemSetupChgFlags,
                            ArpliiAllocationFlag = sp.OutPlanItemAutoModFlags,
                        }));
                    }

                    return BuildPaymentPlan(planData, scheduledPaymentData, invoiceItemsData, payInvoiceItemsData);
                }
                else
                {
                    throw new ApplicationException("Transaction GET.PROPOSED.PAYMENT.PLAN.INFO did not return any data.");
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Could not build proposed payment plan.", ex);
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Build a payment plan template
        /// </summary>
        /// <param name="source">An <see cref="PayPlanTemplates"/> object</param>
        /// <returns>A PaymentPlanTemplate object</returns>
        private PaymentPlanTemplate BuildPaymentPlanTemplate(PayPlanTemplates source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source PayPlanTemplates record must be provided.");
            }

            var template = new PaymentPlanTemplate(source.Recordkey,
                source.PptDescription,
                (source.PptActiveFlag == "Y"),
                ConvertToFrequencyEntity(source.PptFrequency),
                source.PptNoPayments.GetValueOrDefault(),
                source.PptMinPlanAmt.GetValueOrDefault(),
                source.PptMaxPlanAmt,
                source.PptPayFrequencySubr)
            {
                TermsAndConditionsDocumentId = source.PptTermsAndConditionsDoc,
                SetupChargeAmount = source.PptChargeAmt.GetValueOrDefault(),
                SetupChargePercentage = source.PptChargePct.GetValueOrDefault(),
                DownPaymentPercentage = source.PptDownPayPct.GetValueOrDefault(),
                DaysUntilDownPaymentIsDue = source.PptDownPayNoDays.GetValueOrDefault(),
                GraceDays = source.PptGraceNoDays.GetValueOrDefault(),
                LateChargeAmount = source.PptLateChargeAmt.GetValueOrDefault(),
                LateChargePercentage = source.PptLateChrgPct.GetValueOrDefault(),
                IncludeSetupChargeInFirstPayment = (source.PptPrepaySetupFlag == "Y"),
                SubtractAnticipatedFinancialAid = (source.PptSubtractAnticipatedFa == "Y"),
                CalculatePlanAmountAutomatically = (source.PptCalcAmtFlag == "Y"),
                ModifyPlanAutomatically = (source.PptRebillModifyFlag == "Y"),
            };

            // Allowed AR Types
            if (source.PptAllowedArTypes != null && source.PptAllowedArTypes.Count > 0)
            {
                foreach (var allowedArType in source.PptAllowedArTypes)
                {
                    template.AddAllowedReceivableTypeCode(allowedArType);
                }
            }

            // Invoice Exclusion Rules
            if (source.PptInvoiceExclRules != null && source.PptInvoiceExclRules.Count > 0)
            {
                foreach (var invoiceExclusionRuleId in source.PptInvoiceExclRules)
                {
                    template.AddInvoiceExclusionRuleId(invoiceExclusionRuleId);
                }
            }

            // Include AR Codes
            if (source.PptIncludeArCodes != null && source.PptIncludeArCodes.Count > 0)
            {
                foreach (var includedArCode in source.PptIncludeArCodes)
                {
                    template.AddIncludedChargeCode(includedArCode);
                }
            }

            // Exclude AR Codes
            if (source.PptExcludeArCodes != null && source.PptExcludeArCodes.Count > 0)
            {
                foreach (var excludedArCode in source.PptExcludeArCodes)
                {
                    template.AddExcludedChargeCode(excludedArCode);
                }
            }

            return template;
        }

        /// <summary>
        /// Builds a payment plan
        /// </summary>
        /// <param name="source">An <see cref="ArPayPlans"/> object</param>
        /// <returns>A PaymentPlan object</returns>
        private PaymentPlan BuildPaymentPlan(ArPayPlans source, Collection<ArPayPlanItems> schedules, Collection<ArInvoiceItems> invoiceItems,
            Collection<ArPayInvoiceItems> planInvoiceItems)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source ArPayPlans record must be provided.");
            }

            // Plan Statuses
            var planStatuses = new List<PlanStatus>();
            if (source.ArplStatusesEntityAssociation != null && source.ArplStatusesEntityAssociation.Count > 0)
            {
                foreach (var planStatus in source.ArplStatusesEntityAssociation)
                {
                    planStatuses.Add(new PlanStatus(ConvertToPlanStatusEntity(planStatus.ArplStatusAssocMember), planStatus.ArplStatusDateAssocMember.GetValueOrDefault()));
                }
            }

            var scheduledPayments = new List<ScheduledPayment>();
            if (schedules != null && schedules.Count > 0)
            {
                foreach (var item in schedules)
                {
                    scheduledPayments.Add(BuildScheduledPayment(item));
                }
            }

            var planCharges = new List<PlanCharge>();
            if (planInvoiceItems != null && planInvoiceItems.Count > 0)
            {
                foreach (var item in planInvoiceItems)
                {
                    var keys = item.Recordkey.Split('*');
                    string chargeId = keys.Length > 1 ? keys[1] : null;
                    var charge = chargeId == null ? null : invoiceItems.FirstOrDefault(x => x.Recordkey == chargeId);
                    planCharges.Add(BuildPlanCharge(item, charge));
                }
            }

            var plan = new PaymentPlan(source.Recordkey,
                source.ArplPayPlanTemplate,
                source.ArplPersonId,
                source.ArplArType,
                source.ArplTerm,
                source.ArplOrigAmt.GetValueOrDefault(),
                source.ArplFirstDueDate.GetValueOrDefault(),
                planStatuses,
                scheduledPayments,
                planCharges)
            {
                CurrentAmount = source.ArplAmt.GetValueOrDefault(),
                Frequency = ConvertToFrequencyEntity(source.ArplFrequency),
                NumberOfPayments = source.ArplNoPayments.GetValueOrDefault(),
                SetupAmount = source.ArplChargeAmt.GetValueOrDefault(),
                SetupPercentage = source.ArplChargePct.GetValueOrDefault(),
                DownPaymentPercentage = source.ArplDownPayPct.GetValueOrDefault(),
                GraceDays = source.ArplGraceNoDays.GetValueOrDefault(),
                LateChargeAmount = source.ArplLateChargeAmt.GetValueOrDefault(),
                LateChargePercentage = source.ArplPlanLateChrgPct.GetValueOrDefault(),
            };

            return plan;
        }

        /// <summary>
        /// Builds a scheduled payment
        /// </summary>
        /// <param name="source">An <see cref="ArPayPlanItems"/> object</param>
        /// <returns>A ScheduledPayment object</returns>
        private ScheduledPayment BuildScheduledPayment(ArPayPlanItems source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source ArPayPlanItems record must be provided.");
            }

            var scheduledPayment = new ScheduledPayment(source.Recordkey,
                source.ArpliPayPlan,
                source.ArpliAmt.GetValueOrDefault(),
                source.ArpliDueDate.GetValueOrDefault(),
                source.ArpliAmountPaid.GetValueOrDefault(),
                source.ArpliDatePaid.GetValueOrDefault());

            return scheduledPayment;
        }

        /// <summary>
        /// Builds a plan charge
        /// </summary>
        /// <param name="arPayInvoiceItem">An <see cref="ArPayInvoiceItems"/> object</param>
        /// <returns>A PlanCharge object</returns>
        private PlanCharge BuildPlanCharge(ArPayInvoiceItems source, ArInvoiceItems invoiceItem)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A source ArPayInvoiceItems record must be provided.");
            }
            if (invoiceItem == null)
            {
                throw new ArgumentNullException("invoiceItem", "A source invoiceItems record must be provided.");
            }

            var splitKey = source.Recordkey.Split('*');

            if (splitKey.Count() != 2)
            {
                throw new ArgumentException("Record Key is not valid.", "source.RecordKey");
            }

            var planCharge = new PlanCharge(
                splitKey[0],
                BuildCharge(invoiceItem),
                source.ArpliiAmt.GetValueOrDefault(),
                (source.ArpliiSetupInvoiceFlag == "Y"),
                (source.ArpliiAllocationFlag == "Y")
                );

            return planCharge;
        }

        /// <summary>
        /// Converts a payment plan status string to the appropriate enumeration value
        /// </summary>
        /// <param name="frequency">Payment Plan Status</param>
        /// <returns>Payment Plan Status Type enumeration value</returns>
        private PlanStatusType ConvertToPlanStatusEntity(string planStatus)
        {
            if (string.IsNullOrEmpty(planStatus))
            {
                throw new ArgumentNullException("planStatus", "Plan Status must be defined.");
            }
            switch (planStatus.ToUpper())
            {
                case "O":
                    return PlanStatusType.Open;
                case "P":
                    return PlanStatusType.Paid;
                case "C":
                    return PlanStatusType.Cancelled;
                default:
                    throw new ArgumentOutOfRangeException("planStatus", "Plan Status " + planStatus + " is not valid.");
            }
        }

        /// <summary>
        /// Converts a payment plan frequency string to the appropriate enumeration value
        /// </summary>
        /// <param name="frequency">Payment Plan Frequency</param>
        /// <returns>Payment Plan Frequency enumeration value</returns>
        private PlanFrequency ConvertToFrequencyEntity(string frequency)
        {
            if (string.IsNullOrEmpty(frequency))
            {
                throw new ArgumentNullException("frequency", "Payment Plan Template must have a frequency specified.");
            }

            switch (frequency.ToUpper())
            {
                case "W":
                    return PlanFrequency.Weekly;
                case "B":
                    return PlanFrequency.Biweekly;
                case "M":
                    return PlanFrequency.Monthly;
                case "Y":
                    return PlanFrequency.Yearly;
                case "C":
                    return PlanFrequency.Custom;
                default:
                    throw new ArgumentOutOfRangeException("frequency", "Frequency " + frequency + " is not valid.");
            }
        }

        /// <summary>
        /// Convert the PlanFrequency into the corresponding Colleague payment frequency
        /// </summary>
        /// <param name="sourceStatus">The PlanFrequency</param>
        /// <returns>Colleague payment frequency code</returns>
        private string PutPlanFrequency(PlanFrequency frequency)
        {
            string code = null;
            switch (frequency)
            {
                case PlanFrequency.Weekly:
                    code = "W";
                    break;
                case PlanFrequency.Biweekly:
                    code = "B";
                    break;
                case PlanFrequency.Monthly:
                    code = "M";
                    break;
                case PlanFrequency.Yearly:
                    code = "Y";
                    break;
                case PlanFrequency.Custom:
                    code = "C";
                    break;
            }

            return code;
        }

        /// <summary>
        /// Build a charge entity from a Colleague invoice item
        /// </summary>
        /// <param name="item">Colleague AR.INVOICE.ITEMS record</param>
        /// <returns>Charge entity</returns>
        private Charge BuildCharge(ArInvoiceItems item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "An invoice item cannot be null");
            }

            var charge = new Charge(item.Recordkey, item.InviInvoice, item.InviDesc.Split(DmiString._VM), item.InviArCode,
                item.InviExtChargeAmt.GetValueOrDefault() - item.InviExtCrAmt.GetValueOrDefault());

            // Calculate the total tax, if any
            decimal tax = 0;
            if (item.InviArCodeTaxDistrs != null && item.InviArCodeTaxDistrs.Count > 0)
            {
                Collection<ArCodeTaxGlDistr> taxes = DataReader.BulkReadRecord<ArCodeTaxGlDistr>(item.InviArCodeTaxDistrs.ToArray());
                foreach (var taxItem in taxes)
                {
                    tax += taxItem.ArctdGlTaxAmt.GetValueOrDefault();
                }
            }
            charge.TaxAmount = tax;

            // Add any associated payment plan IDs to charges
            if (item.InviArPayPlans != null && item.InviArPayPlans.Count > 0)
            {
                foreach (var planId in item.InviArPayPlans)
                {
                    charge.AddPaymentPlan(planId);
                }
            }

            return charge;
        }

        #endregion
    }
}
