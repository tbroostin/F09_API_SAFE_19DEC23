// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
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
    public class AccountDueRepository : BaseColleagueRepository, IAccountDueRepository
    {
        public const string PastPeriod = "PAST";
        public const string CurrentPeriod = "CUR";
        public const string FuturePeriod = "FTR";
        public const string Mnemonic = "SFPAY";

        private List<string> _masterTermList;
        private FinanceConfigurationRepository _configurationRepository;
        private IEnumerable<FinancialPeriod> _periods = null;
        private readonly string _colleagueTimeZone;


        public AccountDueRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            _configurationRepository = new FinanceConfigurationRepository(cacheProvider, transactionFactory, logger, settings);
            if (settings != null)
            {
                _colleagueTimeZone = settings.ColleagueTimeZone;
            }
        }

        public AccountDue Get(string studentId)
        {
            try
            {
                return ExecutePaymentsDueByTermAdminCTX(studentId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        public AccountDuePeriod GetPeriods(string studentId)
        {
            try
            {
                return ExecutePaymentsDueByPeriodAdminCTX(studentId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        public ElectronicCheckPayer GetCheckPayerInformation(string personId)
        {
            try
            {
                return ExecuteCheckPayerInformationCTX(personId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        public ElectronicCheckProcessingResult ProcessCheck(Payment paymentDetails)
        {
            try
            {
                return ExecuteProcessCheckCTX(paymentDetails);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        #region Colleague Transactions

        private AccountDuePeriod ExecutePaymentsDueByPeriodAdminCTX(string studentId)
        {
            StudentFinPaymentsDueAdminRequest colleagueTxRequest = new StudentFinPaymentsDueAdminRequest();
            colleagueTxRequest.PersonId = studentId;

            try
            {
                StudentFinPaymentsDueAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinPaymentsDueAdminRequest, StudentFinPaymentsDueAdminResponse>(colleagueTxRequest);

                List<PaymentsDue> paymentsDue = colleagueTxResponse.PaymentsDue.ToList<PaymentsDue>();
                List<PaymentPlansDue> paymentPlansDue = colleagueTxResponse.PaymentPlansDue.ToList<PaymentPlansDue>();

                if (_masterTermList == null)
                {
                    _masterTermList = (colleagueTxResponse.TermList != null) ? colleagueTxResponse.TermList : new List<string>();

                    // Reverse the list so terms appear in chronological order
                    _masterTermList.Reverse();
                }

                var accountDuePeriod = new AccountDuePeriod();

                BuildPaymentsDueByPeriod(studentId, accountDuePeriod, paymentsDue, paymentPlansDue);

                // Return name
                accountDuePeriod.PersonName = colleagueTxResponse.PersonName;

                return accountDuePeriod;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private AccountDue ExecutePaymentsDueByTermAdminCTX(string studentId)
        {
            StudentFinPaymentsDueAdminRequest colleagueTxRequest = new StudentFinPaymentsDueAdminRequest();
            colleagueTxRequest.PersonId = studentId;

            try
            {
                StudentFinPaymentsDueAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinPaymentsDueAdminRequest, StudentFinPaymentsDueAdminResponse>(colleagueTxRequest);

                List<PaymentsDue> paymentsDue = colleagueTxResponse.PaymentsDue.ToList<PaymentsDue>();
                List<PaymentPlansDue> paymentPlansDue = colleagueTxResponse.PaymentPlansDue.ToList<PaymentPlansDue>();

                if (_masterTermList == null)
                {
                    _masterTermList = (colleagueTxResponse.TermList != null) ? colleagueTxResponse.TermList : new List<string>();

                    // Reverse the list so terms appear in chronological order
                    _masterTermList.Reverse();
                }

                var accountDue = new AccountDue();

                BuildPaymentsDueByTerm(studentId, accountDue, paymentsDue, paymentPlansDue);

                // Return name
                accountDue.PersonName = colleagueTxResponse.PersonName;

                return accountDue;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private ElectronicCheckPayer ExecuteCheckPayerInformationCTX(string personId = "")
        {
            GetEcheckPayerRequest colleagueTxRequest = new GetEcheckPayerRequest();
            colleagueTxRequest.InPersonId = personId;

            try
            {
                GetEcheckPayerResponse colleagueTxResponse = transactionInvoker.Execute<GetEcheckPayerRequest, GetEcheckPayerResponse>(colleagueTxRequest);

                // Create the data model
                ElectronicCheckPayer payerInfo = new ElectronicCheckPayer();

                // Populate the check payer information
                payerInfo.City = colleagueTxResponse.OutCity;
                payerInfo.Country = colleagueTxResponse.OutCountry;
                payerInfo.Email = colleagueTxResponse.OutEmail;
                payerInfo.FirstName = colleagueTxResponse.OutFirstName;
                payerInfo.LastName = colleagueTxResponse.OutLastName;
                payerInfo.MiddleName = colleagueTxResponse.OutMiddleName;
                payerInfo.PostalCode = colleagueTxResponse.OutPostalCode;
                payerInfo.State = colleagueTxResponse.OutState;
                payerInfo.Street = colleagueTxResponse.OutStreet;
                payerInfo.Telephone = colleagueTxResponse.OutTelephone;

                return payerInfo;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        private ElectronicCheckProcessingResult ExecuteProcessCheckCTX(Payment paymentDetails)
        {
            // Build outgoing Colleague Transaction
            ProcessECheckRequest colleagueTxRequest = new ProcessECheckRequest();
            colleagueTxRequest.InPersonId = paymentDetails.PersonId;
            colleagueTxRequest.InPayerId = string.IsNullOrEmpty(paymentDetails.PayerId) ? paymentDetails.PersonId : paymentDetails.PayerId;
            colleagueTxRequest.InMnemonic = Mnemonic;
            colleagueTxRequest.InDistribution = paymentDetails.Distribution;
            colleagueTxRequest.InPayMethod = paymentDetails.PayMethod;
            colleagueTxRequest.InAmtToPay = paymentDetails.AmountToPay;
            colleagueTxRequest.InProviderAcct = paymentDetails.ProviderAccount;
            colleagueTxRequest.InConvFee = paymentDetails.ConvenienceFee;
            colleagueTxRequest.InConvFeeAmt = paymentDetails.ConvenienceFeeAmount;
            colleagueTxRequest.InConvFeeGlNo = paymentDetails.ConvenienceFeeGeneralLedgerNumber;

            colleagueTxRequest.InFirstName = paymentDetails.CheckDetails.FirstName;
            colleagueTxRequest.InLastName = paymentDetails.CheckDetails.LastName;

            colleagueTxRequest.InAbaNumber = paymentDetails.CheckDetails.AbaRoutingNumber;
            colleagueTxRequest.InAccountNumber = paymentDetails.CheckDetails.BankAccountNumber;
            colleagueTxRequest.InCheckNumber = paymentDetails.CheckDetails.CheckNumber;
            colleagueTxRequest.InDriversLicense = paymentDetails.CheckDetails.DriversLicenseNumber;
            colleagueTxRequest.InDriversLicenseState = paymentDetails.CheckDetails.DriversLicenseState;
            colleagueTxRequest.InPostalCode = paymentDetails.CheckDetails.ZipCode;
            colleagueTxRequest.InEmail = paymentDetails.CheckDetails.EmailAddress;
            colleagueTxRequest.InCity = paymentDetails.CheckDetails.City;
            colleagueTxRequest.InState = paymentDetails.CheckDetails.State;
            colleagueTxRequest.InStreet = paymentDetails.CheckDetails.BillingAddress1;
            colleagueTxRequest.InStreet += "\n";
            colleagueTxRequest.InStreet += paymentDetails.CheckDetails.BillingAddress2;

            colleagueTxRequest.InboundPayments = new List<InboundPayments>();

            foreach (PaymentItem paymentItem in paymentDetails.PaymentItems)
            {
                colleagueTxRequest.InboundPayments.Add(
                    new InboundPayments()
                    {
                        InPmtAmts = paymentItem.PaymentAmount,
                        InPmtDescs = paymentItem.Description,
                        InPmtArTypes = paymentItem.AccountType,
                        InPmtTerms = paymentItem.Term,
                        InPmtInvoices = paymentItem.InvoiceId,
                        InPmtPlans = paymentItem.PaymentPlanId,
                        InPmtOverdues = paymentItem.Overdue,
                        InPmtDepositsDue = paymentItem.DepositDueId,
                        InSfipcPmtComplete = paymentItem.PaymentComplete ? "Y" : "N",
                        InSfipcRegControlId = paymentItem.PaymentControlId,
                    }
                );
            }

            try
            {
                ProcessECheckResponse colleagueTxResponse = transactionInvoker.Execute<ProcessECheckRequest, ProcessECheckResponse>(colleagueTxRequest);

                // Create the data model
                ElectronicCheckProcessingResult result = new ElectronicCheckProcessingResult();

                // Cash receipt is available with successful payment
                result.CashReceiptsId = colleagueTxResponse.OutCashRcptsId;

                // Capture processing error message
                result.ErrorMessage = colleagueTxResponse.OutErrorMsg;

                return result;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        #endregion

        #region Create Payment Due Item From PaymentsDue

        /// <summary>
        /// This method determines a specific payment due object and creates it.
        /// </summary>
        /// <param name="dueItem">payments due item</param>
        /// <returns>base payment due item or null</returns>
        private AccountsReceivableDueItem CreatePaymentDueItem(PaymentsDue dueItem)
        {
            AccountsReceivableDueItem convertedDueItem = null;

            // For an ArTypeDueItem, check the ArTypes property
            if (!string.IsNullOrEmpty(dueItem.ArTypes) && !string.IsNullOrEmpty(dueItem.ArTypeDescs))
            {
                AccountsReceivableDueItem arTypeItem = new AccountsReceivableDueItem();
                arTypeItem.AmountDue = dueItem.ArTypeBals;
                arTypeItem.Description = dueItem.ArTypeDescs;
                arTypeItem.AccountDescription = dueItem.ArTypeDescs;
                arTypeItem.Overdue = dueItem.ArTypeOverdue;
                arTypeItem.Term = dueItem.RelatedTerms;
                arTypeItem.TermDescription = dueItem.RelatedTermDescs;
                arTypeItem.AccountType = dueItem.ArTypes;
                arTypeItem.DueDate = dueItem.ArTypeDueDates;
                if (dueItem.ArTypeDueDates.HasValue)
                {
                    arTypeItem.DueDateOffset = dueItem.ArTypeDueDates.ToPointInTimeDateTimeOffset(dueItem.ArTypeDueDates, _colleagueTimeZone).GetValueOrDefault();
                }
                arTypeItem.Period = dueItem.Periods;
                arTypeItem.PeriodDescription = dueItem.PeriodDescs;
                arTypeItem.Distribution = dueItem.ArTypeDist;
                convertedDueItem = arTypeItem;
            }
            // For an InvoiceDueItem, check the Invoices property.
            // Non-term invoices only.
            else if (!string.IsNullOrEmpty(dueItem.Invoices) || dueItem.InvoiceBals != null)
            {
                InvoiceDueItem invoiceItem = new InvoiceDueItem();
                invoiceItem.InvoiceId = dueItem.Invoices;
                invoiceItem.AmountDue = dueItem.InvoiceBals;
                invoiceItem.Description = dueItem.InvoiceDescs;
                invoiceItem.DueDate = dueItem.InvoicesDueDate;
                if (dueItem.InvoicesDueDate.HasValue)
                {
                    invoiceItem.DueDateOffset = dueItem.InvoicesDueDate.ToPointInTimeDateTimeOffset(dueItem.InvoicesDueDate, _colleagueTimeZone).GetValueOrDefault();
                }
                invoiceItem.Overdue = dueItem.InvoicesOverdue;

                if (!dueItem.RelatedTerms.Equals("NON-TERM"))
                {
                    invoiceItem.Term = dueItem.RelatedTerms;
                    invoiceItem.TermDescription = dueItem.RelatedTermDescs;
                }

                invoiceItem.Distribution = dueItem.InvoiceDist;
                invoiceItem.AccountType = dueItem.InvoiceArTypes;
                convertedDueItem = invoiceItem;
            }

            return convertedDueItem;
        }

        #endregion

        #region Create Payment Due Item From PaymentPlansDue

        /// <summary>
        /// This method determines a specific payment plans due object and creates it.
        /// </summary>
        /// <param name="dueItem">payment plans due item</param>
        /// <returns>base payment due item or null</returns>
        private AccountsReceivableDueItem CreatePaymentDueItem(PaymentPlansDue dueItem)
        {
            AccountsReceivableDueItem convertedDueItem = null;

            // For an PaymentPlanDueItem, check the PaymentPlansIds property
            if (!string.IsNullOrEmpty(dueItem.PaymentPlanIds))
            {
                // PaymentPlanDueItem instance is created. These are the same as a PaymentDueItem, but in a separate group
                // because more than one payment plan scheduled item can be due per AR Type/Term. These will be combined with PaymentDueItem
                // in the view for display.
                PaymentPlanDueItem payplanItem = new PaymentPlanDueItem();
                payplanItem.AmountDue = (decimal?)dueItem.PaymentPlanUnpaidAmts;
                payplanItem.Description = dueItem.PaymentPlanDescs;
                payplanItem.DueDate = dueItem.PaymentPlanDueDates;
                if (dueItem.PaymentPlanDueDates.HasValue)
                {
                    payplanItem.DueDateOffset = dueItem.PaymentPlanDueDates.ToPointInTimeDateTimeOffset(dueItem.PaymentPlanDueDates, _colleagueTimeZone).GetValueOrDefault();
                }
                payplanItem.Overdue = dueItem.PaymentPlanOverdue;
                payplanItem.PaymentPlanCurrent = dueItem.PaymentPlanCurrent;
                payplanItem.PaymentPlanId = dueItem.PaymentPlanIds;
                payplanItem.DueDate = dueItem.PaymentPlanDueDates;
                payplanItem.AccountType = dueItem.PaymentPlanArTypes;
                payplanItem.AccountDescription = dueItem.PaymentPlanArTypeDescs;
                payplanItem.Distribution = dueItem.PaymentPlanDist;
                payplanItem.UnpaidAmount = dueItem.PaymentPlanUnpaidAmts;

                if (!dueItem.PaymentPlanTerms.Equals("NON-TERM"))
                {
                    payplanItem.Term = dueItem.PaymentPlanTerms;
                    payplanItem.TermDescription = dueItem.PaymentPlanTermDescs;
                }

                convertedDueItem = payplanItem;
            }

            return convertedDueItem;
        }

        #endregion

        #region Payments Due By Term

        /// <summary>
        /// This method builds the payments due items and adds them to the data model
        /// </summary>
        /// <param name="termDataModel">data model to populate</param>
        /// <param name="paymentsDue">list of payments due</param>
        /// <param name="paymentPlansDue">list of payment plans</param>
        public void BuildPaymentsDueByTerm(string id, AccountDue accountDue, List<PaymentsDue> paymentsDue, List<PaymentPlansDue> paymentPlansDue)
        {
            // Create a dictionary to capture the unique Terms
            Dictionary<string, AccountTerm> termGroups = new Dictionary<string, AccountTerm>();

            #region Process Payments Due

            // Iterate over the Colleague response - PaymentsDue
            foreach (PaymentsDue paymentItem in paymentsDue)
            {
                // Create the term object if it does not exist
                if (!termGroups.ContainsKey(paymentItem.RelatedTerms))
                {
                    AccountTerm newTermGroup = new AccountTerm();
                    newTermGroup.TermId = paymentItem.RelatedTerms;
                    newTermGroup.Description = paymentItem.RelatedTermDescs;
                    termGroups.Add(paymentItem.RelatedTerms, newTermGroup);
                }

                // Create a specific payment due item, but cast it back to a base object. 
                AccountsReceivableDueItem dueItem = CreatePaymentDueItem(paymentItem);

                // Make sure it was an expected item
                if (dueItem != null)
                {
                    // The dictionary is guaranteed to contain a term object, so add the new item
                    termGroups[paymentItem.RelatedTerms].AccountDetails.Add(dueItem);
                }
            }

            #endregion

            #region Process Payment Plans

            // Iterate over the Colleague response - PaymentPlansDue
            foreach (PaymentPlansDue paymentplanItem in paymentPlansDue)
            {
                // Create the term object if it does not exist
                if (!termGroups.ContainsKey(paymentplanItem.PaymentPlanTerms))
                {
                    AccountTerm newTermGroup = new AccountTerm();
                    newTermGroup.TermId = paymentplanItem.PaymentPlanTerms;
                    newTermGroup.Description = paymentplanItem.PaymentPlanTermDescs;
                    termGroups.Add(paymentplanItem.PaymentPlanTerms, newTermGroup);
                }

                // Create a specific payment due item, but cast it back to a base object. 
                AccountsReceivableDueItem dueItem = CreatePaymentDueItem(paymentplanItem);

                // Make sure it was an expected item
                if (dueItem != null)
                {
                    // The dictionary is guaranteed to contain a term object, so add the new item
                    termGroups[paymentplanItem.PaymentPlanTerms].AccountDetails.Add(dueItem);
                }
            }

            #endregion

            #region Calculate Term Total

            // Iterate over the terms
            foreach (AccountTerm group in termGroups.Values)
            {
                // Perform a summation of the AmountDue fields. If the values turn out to be null, use 0 instead
                group.Amount = group.AccountDetails.Sum(x => x.AmountDue) ?? 0;
            }

            #endregion

            #region Order Terms Based on Master List

            // Order the term groups before adding them to the model
            List<AccountTerm> orderedTermGroups = new List<AccountTerm>();

            // Find the Terms from Payments Due
            var paymentDueTermGroupIds = paymentsDue.GroupBy(x => x.RelatedTerms).Select(y => y.Key);

            // Find the Terms from Payment Plans
            var paymentPlanTermGroupIds = paymentPlansDue.GroupBy(x => x.PaymentPlanTerms).Select(y => y.Key);

            // Combine the two lists into one
            var combinedTermGroupIds = paymentPlanTermGroupIds.Union(paymentPlanTermGroupIds);

            // Take the difference of both against the master term list to find extra values, like Non-Term
            // This also handles an unplanned situation where multiple term groups are omitted from the master list
            var extraTerms = paymentDueTermGroupIds.Except(_masterTermList).Union(paymentPlanTermGroupIds.Except(_masterTermList));

            // Add the extra values after the master list
            var newTermIdList = _masterTermList.Concat(extraTerms);

            // Iterate over the new term list 
            foreach (var orderedTerm in newTermIdList)
            {
                // Only add term groups that have been processed
                if (termGroups.ContainsKey(orderedTerm))
                {
                    orderedTermGroups.Add(termGroups[orderedTerm]);
                }
            }

            #endregion

            // Add the terms to the data model
            //termDataModel.Groups.AddRange(orderedTermGroups);

            accountDue.AccountTerms.AddRange(orderedTermGroups);
            accountDue.PersonId = id;
        }

        #endregion

        #region Payments Due By Period

        public void BuildPaymentsDueByPeriod(string id, AccountDuePeriod accountDuePeriod, List<PaymentsDue> paymentsDue, List<PaymentPlansDue> paymentPlansDue)
        {
            #region Past Period

            // Build the Past Period
            AccountDue pastDueByTerm = new AccountDue();
            pastDueByTerm.PersonId = id;

            //  Find the Payments Due for the Past Period
            List<PaymentsDue> pastPaymentsDue = SelectPaymentsDue(paymentsDue.ToList<PaymentsDue>(), PastPeriod);

            //  Find the Payment Plans for the Past Period
            List<PaymentPlansDue> pastPaymentPlansDue = SelectPaymentPlansDue(paymentPlansDue.ToList<PaymentPlansDue>(), PastPeriod);

            // Build the terms for the Past Period
            BuildPaymentsDueByTerm(id, pastDueByTerm, pastPaymentsDue, pastPaymentPlansDue);

            // Determine the end date for the Past Period
            pastDueByTerm.EndDate = SelectEndDate(paymentsDue.ToList<PaymentsDue>(), paymentPlansDue.ToList<PaymentPlansDue>(), PastPeriod, 1.0f);

            // Assign the Past Period
            accountDuePeriod.Past = pastDueByTerm;

            #endregion

            #region Current Period

            // Build the Current Period
            AccountDue currentDueByTerm = new AccountDue();
            currentDueByTerm.PersonId = id;

            //  Find the Payments Due for the Current Period
            List<PaymentsDue> currentPaymentsDue = SelectPaymentsDue(paymentsDue.ToList<PaymentsDue>(), CurrentPeriod);

            //  Find the Payment Plans for the Current Period
            List<PaymentPlansDue> currentPaymentPlansDue = SelectPaymentPlansDue(paymentPlansDue.ToList<PaymentPlansDue>(), CurrentPeriod);

            // Build the terms for the Current Period
            BuildPaymentsDueByTerm(id, currentDueByTerm, currentPaymentsDue, currentPaymentPlansDue);

            // Determine the start date for the Current Period
            currentDueByTerm.StartDate = SelectStartDate(paymentsDue.ToList<PaymentsDue>(), paymentPlansDue.ToList<PaymentPlansDue>(), CurrentPeriod, 0);

            // Determine the end date for the Current Period
            currentDueByTerm.EndDate = SelectEndDate(paymentsDue.ToList<PaymentsDue>(), paymentPlansDue.ToList<PaymentPlansDue>(), CurrentPeriod, 0);

            // Assign the Current Period
            accountDuePeriod.Current = currentDueByTerm;

            #endregion

            #region Future Period

            // Build the Future Period
            AccountDue futureDueByTerm = new AccountDue();
            futureDueByTerm.PersonId = id;

            //  Find the Payments Due for the Future Period
            List<PaymentsDue> futurePaymentsDue = SelectPaymentsDue(paymentsDue.ToList<PaymentsDue>(), FuturePeriod);

            //  Find the Payment Plans for the Future Period
            List<PaymentPlansDue> futurePaymentPlansDue = SelectPaymentPlansDue(paymentPlansDue.ToList<PaymentPlansDue>(), FuturePeriod);

            // Build the terms for the Future Period
            BuildPaymentsDueByTerm(id, futureDueByTerm, futurePaymentsDue, futurePaymentPlansDue);

            // Determine the start date for the Future Period
            futureDueByTerm.StartDate = SelectStartDate(paymentsDue.ToList<PaymentsDue>(), paymentPlansDue.ToList<PaymentPlansDue>(), FuturePeriod, -1.0f);

            // Assign the Future Period
            accountDuePeriod.Future = futureDueByTerm;

            #endregion
        
            // Get the financial period dates if we don't already have them
            if (_periods == null)
            {
                _periods = _configurationRepository.GetFinancialPeriods();
            }
            // Fill in missing period dates
            if (accountDuePeriod.Past.EndDate == null)
            {
                accountDuePeriod.Past.EndDate = _periods.Where(x => x.Type == PeriodType.Past).First().End;
            }
            if (accountDuePeriod.Current.StartDate == null)
            {
                accountDuePeriod.Current.StartDate = _periods.Where(x => x.Type == PeriodType.Current).First().Start;
            }
            if (accountDuePeriod.Current.EndDate == null)
            {
                accountDuePeriod.Current.EndDate = _periods.Where(x => x.Type == PeriodType.Current).First().End;
            }
            if (accountDuePeriod.Future.StartDate == null)
            {
                accountDuePeriod.Future.StartDate = _periods.Where(x => x.Type == PeriodType.Future).First().Start;
            }
        }

        #endregion

        #region Select End Date

        /// <summary>
        /// This method determines an end date for a period from the available lists of PaymentsDue and PaymentPlansDue items. The found date is offset
        /// by the specified amount. 
        /// </summary>
        /// <param name="PaymentsDue">list of payments due</param>
        /// <param name="PaymentPlansDue">list of payment plans</param>
        /// <param name="Period">period type</param>
        /// <param name="Offset">difference from the found end date</param>
        /// <returns>offset end date or null</returns>
        private DateTime? SelectEndDate(List<PaymentsDue> PaymentsDue, List<PaymentPlansDue> PaymentPlansDue, string Period, float Offset)
        {
            // Find the end date for the specified period in payments due
            var paymentsDueEndDates = from paymentDue in PaymentsDue
                                      where paymentDue.Periods != null && paymentDue.Periods.Equals(Period) && paymentDue.PeriodEndDates != null
                                      select paymentDue.PeriodEndDates.Value;

            // Find the end date for the specified period in payment plans
            var paymentPlanEndDates = from paymentPlan in PaymentPlansDue
                                      where paymentPlan.PaymentPlanPeriods != null && paymentPlan.PaymentPlanPeriods.Equals(Period) && paymentPlan.PaymentPlanPeriodEndDates != null
                                      select paymentPlan.PaymentPlanPeriodEndDates.Value;

            // Inline logic to determine the end date:
            // 1) Attempt to find the end date from payments due
            // 2) If that result is null (no date found), then evaluate the payment plans
            // 3) If that result is null, then allow the null value
            DateTime? nullableEndDate = FindDateOrNull(paymentsDueEndDates);

            if (nullableEndDate == null)
            {
                nullableEndDate = FindDateOrNull(paymentPlanEndDates);
            }

            // If no date was found, then the offset will not be applied
            if (Offset > 0 && nullableEndDate != null)
            {
                nullableEndDate = nullableEndDate.Value.AddDays(Offset);
            }

            return nullableEndDate;
        }

        #endregion

        #region Select Start Date

        /// <summary>
        /// This method determines a start date for a period from the available lists of PaymentsDue and PaymentPlansDue items. The found date is offset
        /// by the specified amount. 
        /// </summary>
        /// <param name="PaymentsDue">list of payments due</param>
        /// <param name="PaymentPlansDue">list of payment plans</param>
        /// <param name="Period">period type</param>
        /// <param name="Offset">difference from the found end date</param>
        /// <returns>offset start date or null</returns>
        private DateTime? SelectStartDate(List<PaymentsDue> PaymentsDue, List<PaymentPlansDue> PaymentPlansDue, string Period, float Offset)
        {
            // Find the start date for the specified period in payments due
            var paymentsDueStartDates = from paymentDue in PaymentsDue
                                        where paymentDue.Periods != null && paymentDue.Periods.Equals(Period) && paymentDue.PeriodBeginDates != null
                                        select paymentDue.PeriodBeginDates.Value;

            // Find the start date for the specified period in payment plans
            var paymentPlanStartDates = from paymentPlan in PaymentPlansDue
                                        where paymentPlan.PaymentPlanPeriods != null && paymentPlan.PaymentPlanPeriods.Equals(Period) && paymentPlan.PaymentPlanPeriodBeginDates != null
                                        select paymentPlan.PaymentPlanPeriodBeginDates.Value;

            // Inline logic to determine the end date:
            // 1) Attempt to find the end date from payments due
            // 2) If that result is null (no date found), then evaluate the payment plans
            // 3) If that result is null, then allow the null value
            DateTime? nullableStartDate = FindDateOrNull(paymentsDueStartDates);

            if( nullableStartDate == null ) {
                nullableStartDate = FindDateOrNull(paymentPlanStartDates);
            }

            // If no date was found, then the offset will not be applied
            if (Offset < 0 && nullableStartDate != null)
            {
                nullableStartDate = nullableStartDate.Value.AddDays(Offset);
            }

            return nullableStartDate;
        }

        #endregion

        #region Select Payments Due for Period

        /// <summary>
        /// This method finds the associated PaymentsDue items for the specified Period.
        /// </summary>
        /// <param name="PaymentsDue">list of payments due</param>
        /// <param name="Period">period type</param>
        /// <returns>empty list or list of payments due items</returns>
        private List<PaymentsDue> SelectPaymentsDue(List<PaymentsDue> PaymentsDue, string Period)
        {
            // Find the payments due for the specified period
            var paymentDueItems = from paymentDue in PaymentsDue
                                  where paymentDue.Periods != null && paymentDue.Periods.Equals(Period)
                                  select paymentDue;

            // Guarantee the list is not null
            List<PaymentsDue> paymentsDueList = new List<PaymentsDue>();

            if (paymentDueItems != null)
            {
                // Pass out the list of found items
                paymentsDueList = paymentDueItems.ToList();
            }

            return paymentsDueList;
        }

        #endregion

        #region Select Payment Plans for Period

        /// <summary>
        /// This method finds the associated PaymentPlansDue items for the specified Period.
        /// </summary>
        /// <param name="PaymentPlansDue">list of payment plans</param>
        /// <param name="Period">period type</param>
        /// <returns>empty list or list of payment plan items</returns>
        private List<PaymentPlansDue> SelectPaymentPlansDue(List<PaymentPlansDue> PaymentPlansDue, string Period)
        {
            // Find the payment plans for the specified period
            var paymentPlansDueItems = from paymentPlanDue in PaymentPlansDue
                                       where paymentPlanDue.PaymentPlanPeriods != null && paymentPlanDue.PaymentPlanPeriods.Equals(Period)
                                       select paymentPlanDue;

            // Guarantee the list is not null
            List<PaymentPlansDue> paymentPlansDueList = new List<PaymentPlansDue>();

            if (paymentPlansDueItems != null)
            {
                // Pass out the list of found items
                paymentPlansDueList = paymentPlansDueItems.ToList();
            }

            return paymentPlansDueList;
        }

        #endregion

        #region Find Date or Null Function

        // Determines if the enumeration contains a valid DateTime value or returns null
        private DateTime? FindDateOrNull(IEnumerable<DateTime> dateCollection)
        {
            
            Nullable<DateTime> firstDate = null;
            if(dateCollection != null && dateCollection.Count() > 0) {
                DateTime firstValue = dateCollection.First();
                firstDate = new Nullable<DateTime>(firstValue);
            }

            return firstDate;
        }

        #endregion
    }
}
