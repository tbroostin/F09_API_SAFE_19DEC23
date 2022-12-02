// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Entities;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Dmi.Runtime;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Finance.Repositories
{
    [RegisterType]
    public class AccountActivityRepository : BaseColleagueRepository, IAccountActivityRepository
    {
        public const string PastPeriod = FinanceTimeframeCodes.PastPeriod;
        public const string CurrentPeriod = FinanceTimeframeCodes.CurrentPeriod;
        public const string FuturePeriod = FinanceTimeframeCodes.FuturePeriod;
        public const string NonTerm = FinanceTimeframeCodes.NonTerm;
        public const string Mnemonic = "SFPAY";
        public const string TuitionBySectionType = "TUIBS";
        public const string TuitionByTotalType = "TUIBT";
        public const string FeeType = "FEES";
        public const string OtherType = "OTHER";
        public const string RoomAndBoardType = "RB";

        // Variable to store Colleague valcode table
        private IEnumerable<DaysOfWeek> _days = null;

        // Temporarily cache values in case of subsequent calls to this repository on the same request
        // For example, a call to GetAccountPeriods() may be followed by a call to GetNonTermAccountPeriod().
        // Without this stopgap measure, GetNonTermAccountPeriod() would invoke the CTX again just to get
        // a single AccountPeriod.
        private IEnumerable<AccountPeriod> _cachedPeriods;
        private AccountPeriod _cachedNonTermPeriod;
        private bool _executedTransaction;

        public AccountActivityRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        public IEnumerable<AccountPeriod> GetAccountPeriods(string studentId)
        {
            try
            {
                if (!_executedTransaction)
                {
                    PopulateLocalData(studentId);
                }

                IEnumerable<AccountPeriod> accountPeriods = _cachedPeriods;

                return accountPeriods;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        public AccountPeriod GetNonTermAccountPeriod(string studentId)
        {
            try
            {
                if (!_executedTransaction)
                {
                    PopulateLocalData(studentId);
                }

                AccountPeriod nonTermPeriod = _cachedNonTermPeriod;

                return nonTermPeriod;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        [Obsolete("Obsolete as of API version 1.8, use GetTermActivityForStudent2 instead")]
        public DetailedAccountPeriod GetTermActivityForStudent(string TermId, string StudentId)
        {
            // Get the data from cache or get it and cache it for 1 minute
            return GetOrAddToCache("GetTermActivity-" + StudentId + "-" + TermId,
                () => ExecuteActivityByTermAdminCTX(TermId, StudentId), 1);
        }

        public DetailedAccountPeriod GetTermActivityForStudent2(string termId, string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            try
            {
                // Get the data from cache or get it and cache it for 1 minute
                return GetOrAddToCache("GetTermActivity-" + studentId + "-" + termId,
                    () => ExecuteActivityByTermAdminCTX2(termId, studentId), 1);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        [Obsolete("Obsolete as of API version 1.8, use GetPeriodActivityForStudent2 instead")]
        public DetailedAccountPeriod GetPeriodActivityForStudent(IEnumerable<string> TermIds, DateTime? StartDate, DateTime? EndDate, string StudentId)
        {
            PeriodType period;
            if (StartDate == null || StartDate.Value.Date.Equals(DateTime.MinValue.Date))
            {
                period = PeriodType.Past;
            }
            else if (EndDate == null || EndDate.Value.Date.Equals(DateTime.MaxValue.Date))
            {
                period = PeriodType.Future;
            }
            else
            {
                period = PeriodType.Current;
            }
            return GetOrAddToCache("GetPeriodActivity-" + StudentId + "-" + period.ToString(),
                () => ExecuteActivityByPeriodAdminCTX(TermIds, StartDate, EndDate, StudentId), 1);
        }

        public DetailedAccountPeriod GetPeriodActivityForStudent2(IEnumerable<string> termIds, DateTime? startDate, DateTime? endDate, string studentId)
        {
            if (termIds == null || termIds.Count() == 0)
            {
                throw new ArgumentNullException("termIds");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            PeriodType period;
            if (startDate == null || startDate.Value.Date.Equals(DateTime.MinValue.Date))
            {
                period = PeriodType.Past;
            }
            else if (endDate == null || endDate.Value.Date.Equals(DateTime.MaxValue.Date))
            {
                period = PeriodType.Future;
            }
            else
            {
                period = PeriodType.Current;
            }
            try
            {
                return GetOrAddToCache("GetPeriodActivity-" + studentId + "-" + period.ToString(),
                    () => ExecuteActivityByPeriodAdminCTX2(termIds, startDate, endDate, studentId), 1);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <param name="awardId">award id</param>
        /// <param name="awardCategory">award category</param>
        /// <returns>StudentAwardDisbursementInfo entity</returns>
        public async Task<StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync(string studentId, string awardYearCode, string awardId, TIVAwardCategory awardCategory)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }

            try
            {
                string criteria = studentId + "*" + awardId;
                string fileName = "";

                //Create student award disbursement information entity with existing data
                StudentAwardDisbursementInfo disbInfo = new StudentAwardDisbursementInfo(studentId, awardId, awardYearCode);

                //Get separate disbursements information from SL.ACYR, TC.ACYR/DATE.AWARD, or PELL.ACYR based on the award category
                if (awardCategory == TIVAwardCategory.Loan)
                {
                    fileName = "SL." + awardYearCode;
                    var loanData = await DataReader.ReadRecordAsync<SlAcyr>(fileName, criteria);
                    if (loanData == null)
                    {
                        string message = string.Format("Could not locate an SL.ACYR file with specified criteria: {0}, {1}", fileName, criteria);
                        LogDataError("SL.ACYR", fileName, null, null, message);
                        throw new KeyNotFoundException(message);
                    }
                    foreach (var disb in loanData.SlLoanDisbEntityAssociation)
                    {
                        disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.SlAntDisbTermAssocMember, disb.SlAntDisbDateAssocMember, disb.SlActDisbAmtAssocMember, disb.SlInitDisbDtAssocMember));
                    }
                }
                else if (awardCategory == TIVAwardCategory.Teach)
                {
                    fileName = "TC." + awardYearCode;
                    var teachAwardData = await DataReader.ReadRecordAsync<TcAcyr>(fileName, criteria);
                    if (teachAwardData == null)
                    {
                        string message = string.Format("Could not locate an TC.ACYR file with specified criteria: {0}, {1}", fileName, criteria);
                        LogDataError("TC.ACYR", fileName, null, null, message);
                        throw new KeyNotFoundException(message);
                    }
                    string tcDateAwardId = teachAwardData.TcDateAwardId;
                    if (!string.IsNullOrEmpty(tcDateAwardId))
                    {
                        var dateAwardData = await DataReader.ReadRecordAsync<DateAward>(tcDateAwardId);
                        if (dateAwardData == null)
                        {
                            string message = string.Format("Could not locate an DATE.AWARD file with specified criteria: {0}", tcDateAwardId);
                            LogDataError("DATE.AWARD", tcDateAwardId, null, null, message);
                            throw new KeyNotFoundException(message);
                        }
                        var tcDisbData = await DataReader.BulkReadRecordAsync<DateAwardDisb>(dateAwardData.DawDateAwardDisbIds.ToArray());
                        if (tcDisbData == null || !tcDisbData.Any())
                        {
                            string message = "Could not locate an DATE.AWARD.DISB records with specified ids";
                            LogDataError("DATE.AWARD.DISB", tcDateAwardId, null, null, message);
                            throw new KeyNotFoundException(message);
                        }
                        foreach (var disb in tcDisbData)
                        {
                            disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.DawdAwardPeriod, disb.DawdDate, disb.DawdXmitAmount, disb.DawdInitialXmitDate));
                        }
                    }
                }
                else if (awardCategory == TIVAwardCategory.Pell)
                {
                    fileName = "PELL." + awardYearCode;
                    var pellData = await DataReader.ReadRecordAsync<PellAcyr>(fileName, criteria);
                    if (pellData == null)
                    {
                        string message = string.Format("Could not locate an PELL.ACYR file with specified criteria: {0}, {1}", fileName, criteria);
                        LogDataError("PELL.ACYR", fileName, null, null, message);
                        throw new KeyNotFoundException(message);
                    }
                    foreach (var disb in pellData.PellDisbsEntityAssociation)
                    {
                        disbInfo.AwardDisbursements.Add(new StudentAwardDisbursement(disb.PellDisbAwardPeriodsAssocMember, disb.PellDisbDatesAssocMember, disb.PellActDisbAmountsAssocMember, disb.PellInitDisbDatesAssocMember));
                    }
                }
                return disbInfo;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

        }

        private void PopulateLocalData(string studentId)
        {
            try
            {
                var periods = ExecuteAccountPeriodsAdminCTX(studentId);

                var accountPeriods = periods.Where(x => x.Id != NonTerm);

                _cachedPeriods = accountPeriods;

                var nonTermPeriod = periods.Where(x => x.Id == NonTerm).FirstOrDefault(); ;

                _cachedNonTermPeriod = nonTermPeriod;

                _executedTransaction = true;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, csee.Message);
                throw;
            }
        }

        #region Colleague Transactions

        private IEnumerable<AccountPeriod> ExecuteAccountPeriodsAdminCTX(string studentId)
        {
            // Build outgoing Colleague Transaction
            StudentFinActPeriodsAdminRequest colleagueTxRequest = new StudentFinActPeriodsAdminRequest();
            colleagueTxRequest.PersonId = studentId;

            StudentFinActPeriodsAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinActPeriodsAdminRequest, StudentFinActPeriodsAdminResponse>(colleagueTxRequest);

            var periods = new List<AccountPeriod>();

            foreach (var period in colleagueTxResponse.FinancialPeriods)
            {
                AccountPeriod newPeriod = new AccountPeriod()
                {
                    Balance = period.PeriodBalances,
                    Description = period.PeriodDescs,
                    Id = period.Periods
                };

                // treat non-term a little differently
                if (period.Periods.Equals(NonTerm))
                {
                    periods.Add(newPeriod);
                }
                else
                {
                    // populate any related terms
                    string prdToProcess = period.Periods;
                    SelectPeriodRelatedTerms(colleagueTxResponse.PeriodRelatedTerms.ToList<PeriodRelatedTerms>(), prdToProcess, newPeriod);

                    // populate the period date(s) based on the period, and the dates passed in
                    switch (prdToProcess)
                    {
                        case PastPeriod:
                            newPeriod.EndDate = colleagueTxResponse.CurBeginDate;
                            break;
                        case CurrentPeriod:
                            newPeriod.StartDate = colleagueTxResponse.CurBeginDate;
                            newPeriod.EndDate = colleagueTxResponse.CurEndDate;
                            break;
                        case FuturePeriod:
                            newPeriod.StartDate = colleagueTxResponse.CurEndDate;
                            break;
                    }
                    periods.Add(newPeriod);
                }
            }

            return periods;
        }

        [Obsolete("Obsolete as of API version 1.8, use ExecuteActivityByTermAdminCTX2 instead")]
        private DetailedAccountPeriod ExecuteActivityByTermAdminCTX(string TermId, string StudentId)
        {
            var period = new DetailedAccountPeriod();
            
            // Build outgoing Colleague Transaction
            StudentFinancialActivityAdminRequest colleagueTxRequest = new StudentFinancialActivityAdminRequest();
            colleagueTxRequest.PersonId = StudentId;

            // Pass in the Term ID
            var inboundTerm = new InboundTerms();
            inboundTerm.TermsId = TermId;
            colleagueTxRequest.InboundTerms = new List<InboundTerms>() { inboundTerm };
            colleagueTxRequest.StartDate = null;
            colleagueTxRequest.EndDate = null;

            StudentFinancialActivityAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(colleagueTxRequest);

            // Create all of the expected categories
            ChargesCategory charges = new ChargesCategory();
            DepositCategory deposits = new DepositCategory();
            FinancialAidCategory finAid = new FinancialAidCategory();
            PaymentCategory payments = new PaymentCategory();
            PaymentPlanCategory paymentPlans = new PaymentPlanCategory();
            RefundCategory refunds = new RefundCategory();
            SponsorshipCategory sponsorships = new SponsorshipCategory();
            StudentPaymentCategory studentPayments = new StudentPaymentCategory();


            // Build the categories
            BuildChargeTypes(charges, colleagueTxResponse.FinancialActivityChargeGroups.ToList<FinancialActivityChargeGroups>(), colleagueTxResponse.AllChargeGroups.ToList<AllChargeGroups>());
            BuildDeposits(deposits, colleagueTxResponse.FinancialActivityDeposits.ToList<FinancialActivityDeposits>());
            BuildRefunds(refunds, colleagueTxResponse.FinancialActivityRefunds.ToList<FinancialActivityRefunds>());
            BuildPaymentPlans(paymentPlans, colleagueTxResponse.PaymentPlans.ToList<PaymentPlans>(), colleagueTxResponse.PlanSchedules.ToList<PlanSchedules>());
            BuildFinancialAid(finAid, colleagueTxResponse.AnticipatedFinancialAid.ToList<AnticipatedFinancialAid>(), colleagueTxResponse.AnticipatedAidTerms.ToList<AnticipatedAidTerms>());
            BuildSponsorships(sponsorships, colleagueTxResponse.FinancialActivitySponsorPayments.ToList<FinancialActivitySponsorPayments>());
            BuildStudentPayments(studentPayments, colleagueTxResponse.FinancialActivityStudentPayments.ToList<FinancialActivityStudentPayments>());

            // Assign the categories to the account period
            period.Charges = charges;
            period.Deposits = deposits;
            period.FinancialAid = finAid;
            period.Payments = payments;
            period.PaymentPlans = paymentPlans;
            period.Refunds = refunds;
            period.Sponsorships = sponsorships;
            period.StudentPayments = studentPayments;

            return period;
        }

        private DetailedAccountPeriod ExecuteActivityByTermAdminCTX2(string TermId, string StudentId)
        {
            var period = new DetailedAccountPeriod();

            // Build outgoing Colleague Transaction
            StudentFinancialActivityAdminRequest colleagueTxRequest = new StudentFinancialActivityAdminRequest();
            colleagueTxRequest.PersonId = StudentId;

            // Pass in the Term ID
            var inboundTerm = new InboundTerms();
            inboundTerm.TermsId = TermId;
            colleagueTxRequest.InboundTerms = new List<InboundTerms>() { inboundTerm };
            colleagueTxRequest.StartDate = null;
            colleagueTxRequest.EndDate = null;
            
            StudentFinancialActivityAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(colleagueTxRequest);

            // Create all of the expected categories
            ChargesCategory charges = new ChargesCategory();
            DepositCategory deposits = new DepositCategory();
            FinancialAidCategory finAid = new FinancialAidCategory();
            PaymentCategory payments = new PaymentCategory();
            PaymentPlanCategory paymentPlans = new PaymentPlanCategory();
            RefundCategory refunds = new RefundCategory();
            SponsorshipCategory sponsorships = new SponsorshipCategory();
            StudentPaymentCategory studentPayments = new StudentPaymentCategory();


            // Build the categories
            BuildChargeTypes(charges, colleagueTxResponse.FinancialActivityChargeGroups.ToList<FinancialActivityChargeGroups>(), colleagueTxResponse.AllChargeGroups.ToList<AllChargeGroups>());
            BuildDeposits(deposits, colleagueTxResponse.FinancialActivityDeposits.ToList<FinancialActivityDeposits>());
            BuildRefunds(refunds, colleagueTxResponse.FinancialActivityRefunds.ToList<FinancialActivityRefunds>());
            BuildPaymentPlans(paymentPlans, colleagueTxResponse.PaymentPlans.ToList<PaymentPlans>(), colleagueTxResponse.PlanSchedules.ToList<PlanSchedules>());
            BuildFinancialAid(finAid, colleagueTxResponse.AnticipatedFinancialAid.ToList<AnticipatedFinancialAid>(), colleagueTxResponse.AnticipatedAidTerms.ToList<AnticipatedAidTerms>());
            BuildSponsorships(sponsorships, colleagueTxResponse.FinancialActivitySponsorPayments.ToList<FinancialActivitySponsorPayments>());
            BuildStudentPayments2(studentPayments, colleagueTxResponse.FinancialActivityStudentPayments.ToList<FinancialActivityStudentPayments>());

            // Assign the categories to the account period
            period.Charges = charges;
            period.Deposits = deposits;
            period.FinancialAid = finAid;
            period.Payments = payments;
            period.PaymentPlans = paymentPlans;
            period.Refunds = refunds;
            period.Sponsorships = sponsorships;
            period.StudentPayments = studentPayments;

            return period;
        }

        [Obsolete("Obsolete as of API version 1.8, use ExecuteActivityByPeriodAdminCTX2 instead")]
        private DetailedAccountPeriod ExecuteActivityByPeriodAdminCTX(IEnumerable<string> TermIds, DateTime? StartDate, DateTime? EndDate, string StudentId)
        {
            var period = new DetailedAccountPeriod();
            
            // Build outgoing Colleague Transaction
            StudentFinancialActivityAdminRequest colleagueTxRequest = new StudentFinancialActivityAdminRequest();
            colleagueTxRequest.PersonId = StudentId;

            // Pass in the Term ID
            var inboundTermList = from term in TermIds
                                  select new InboundTerms()
                                  {
                                      TermsId = term
                                  };

            colleagueTxRequest.InboundTerms = new List<InboundTerms>();
            colleagueTxRequest.InboundTerms.AddRange(inboundTermList);
            colleagueTxRequest.StartDate = StartDate;
            colleagueTxRequest.EndDate = EndDate;

            StudentFinancialActivityAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(colleagueTxRequest);

            // Create all of the expected categories
            ChargesCategory charges = new ChargesCategory();
            DepositCategory deposits = new DepositCategory();
            FinancialAidCategory finAid = new FinancialAidCategory();
            PaymentCategory payments = new PaymentCategory();
            PaymentPlanCategory paymentPlans = new PaymentPlanCategory();
            RefundCategory refunds = new RefundCategory();
            SponsorshipCategory sponsorships = new SponsorshipCategory();
            StudentPaymentCategory studentPayments = new StudentPaymentCategory();

            // Build the categories
            BuildChargeTypes(charges, colleagueTxResponse.FinancialActivityChargeGroups.ToList<FinancialActivityChargeGroups>(), colleagueTxResponse.AllChargeGroups.ToList<AllChargeGroups>());
            BuildDeposits(deposits, colleagueTxResponse.FinancialActivityDeposits.ToList<FinancialActivityDeposits>());
            BuildRefunds(refunds, colleagueTxResponse.FinancialActivityRefunds.ToList<FinancialActivityRefunds>());
            BuildPaymentPlans(paymentPlans, colleagueTxResponse.PaymentPlans.ToList<PaymentPlans>(), colleagueTxResponse.PlanSchedules.ToList<PlanSchedules>());
            BuildFinancialAid(finAid, colleagueTxResponse.AnticipatedFinancialAid.ToList<AnticipatedFinancialAid>(), colleagueTxResponse.AnticipatedAidTerms.ToList<AnticipatedAidTerms>());
            BuildSponsorships(sponsorships, colleagueTxResponse.FinancialActivitySponsorPayments.ToList<FinancialActivitySponsorPayments>());
            BuildStudentPayments(studentPayments, colleagueTxResponse.FinancialActivityStudentPayments.ToList<FinancialActivityStudentPayments>());

            // Assign the categories to the account period
            period.Charges = charges;
            period.Deposits = deposits;
            period.FinancialAid = finAid;
            period.Payments = payments;
            period.PaymentPlans = paymentPlans;
            period.Refunds = refunds;
            period.Sponsorships = sponsorships;
            period.StudentPayments = studentPayments;

            return period;
        }

        private DetailedAccountPeriod ExecuteActivityByPeriodAdminCTX2(IEnumerable<string> TermIds, DateTime? StartDate, DateTime? EndDate, string StudentId)
        {
            var period = new DetailedAccountPeriod();

            // Build outgoing Colleague Transaction
            StudentFinancialActivityAdminRequest colleagueTxRequest = new StudentFinancialActivityAdminRequest();
            colleagueTxRequest.PersonId = StudentId;

            // Pass in the Term ID
            var inboundTermList = from term in TermIds
                                  select new InboundTerms()
                                  {
                                      TermsId = term
                                  };

            colleagueTxRequest.InboundTerms = new List<InboundTerms>();
            colleagueTxRequest.InboundTerms.AddRange(inboundTermList);
            colleagueTxRequest.StartDate = StartDate;
            colleagueTxRequest.EndDate = EndDate;

            StudentFinancialActivityAdminResponse colleagueTxResponse = transactionInvoker.Execute<StudentFinancialActivityAdminRequest, StudentFinancialActivityAdminResponse>(colleagueTxRequest);
                        
            // Create all of the expected categories
            ChargesCategory charges = new ChargesCategory();
            DepositCategory deposits = new DepositCategory();
            FinancialAidCategory finAid = new FinancialAidCategory();
            PaymentCategory payments = new PaymentCategory();
            PaymentPlanCategory paymentPlans = new PaymentPlanCategory();
            RefundCategory refunds = new RefundCategory();
            SponsorshipCategory sponsorships = new SponsorshipCategory();
            StudentPaymentCategory studentPayments = new StudentPaymentCategory();

            // Build the categories
            BuildChargeTypes(charges, colleagueTxResponse.FinancialActivityChargeGroups.ToList<FinancialActivityChargeGroups>(), colleagueTxResponse.AllChargeGroups.ToList<AllChargeGroups>());
            BuildDeposits(deposits, colleagueTxResponse.FinancialActivityDeposits.ToList<FinancialActivityDeposits>());
            BuildRefunds(refunds, colleagueTxResponse.FinancialActivityRefunds.ToList<FinancialActivityRefunds>());
            BuildPaymentPlans(paymentPlans, colleagueTxResponse.PaymentPlans.ToList<PaymentPlans>(), colleagueTxResponse.PlanSchedules.ToList<PlanSchedules>());
            BuildFinancialAid(finAid, colleagueTxResponse.AnticipatedFinancialAid.ToList<AnticipatedFinancialAid>(), colleagueTxResponse.AnticipatedAidTerms.ToList<AnticipatedAidTerms>());
            BuildSponsorships(sponsorships, colleagueTxResponse.FinancialActivitySponsorPayments.ToList<FinancialActivitySponsorPayments>());
            BuildStudentPayments2(studentPayments, colleagueTxResponse.FinancialActivityStudentPayments.ToList<FinancialActivityStudentPayments>());

            // Assign the categories to the account period
            period.Charges = charges;
            period.Deposits = deposits;
            period.FinancialAid = finAid;
            period.Payments = payments;
            period.PaymentPlans = paymentPlans;
            period.Refunds = refunds;
            period.Sponsorships = sponsorships;
            period.StudentPayments = studentPayments;

            return period;
        }

        #endregion

        /// <summary>
        /// This method finds the related terms for a given period (PAST/CUR/FTR), and uses them to populate a BasicAccountPeriod object. 
        /// </summary>
        /// <param name="originalPeriodRelatedTerms">the complete list of all related terms for all periods</param>
        /// <param name="prdToProcess">the period of interest (PAST/CUR/FTR)</param>
        /// <param name="newPeriod">the BasicAccountPeriod object we are building</param>
        /// 
        private void SelectPeriodRelatedTerms(List<PeriodRelatedTerms> originalPeriodRelatedTerms, string prdToProcess, AccountPeriod newPeriod)
        {
            var relatedTerms = from periodRelatedTerm in originalPeriodRelatedTerms
                               where periodRelatedTerm.RelatedTermPeriods != null && periodRelatedTerm.RelatedTermPeriods.Equals(prdToProcess)
                               select periodRelatedTerm;

            // If we found some related terms for this period, populate the new period with the term names.
            if (relatedTerms != null)
            {
                newPeriod.AssociatedPeriods = new List<string>();
                foreach (PeriodRelatedTerms term in relatedTerms)
                {
                    newPeriod.AssociatedPeriods.Add(term.RelatedTerms);
                }
            }
        }

        #region Build Charge Types

        /// <summary>
        /// This method builds the charge type groups and preserves their display order.
        /// </summary>
        /// <param name="charges">charges category</param>
        /// <param name="groups">grouped charge data</param>
        public void BuildChargeTypes(ChargesCategory charges, List<FinancialActivityChargeGroups> groups, List<AllChargeGroups> chargeGroupMetadata)
        {
            // Group the charge group types by name
            Dictionary<string, NamedType> chargeGroupTypes = new Dictionary<string, NamedType>();

            #region Build Charge Groups from Metadata

            // Make sure the charge group metadata is not empty
            if (chargeGroupMetadata != null && chargeGroupMetadata.Count > 0)
            {
                // Iterate over the list of charge group types
                foreach (AllChargeGroups chargeGroup in chargeGroupMetadata)
                {
                    // Create a new Tuition By Section type
                    if (chargeGroup.AllChargeGroupTypes.Equals(TuitionBySectionType))
                    {
                        TuitionBySectionType tuitionBySectionType = new TuitionBySectionType()
                        {
                            DisplayOrder = chargeGroup.AllChargeGroupOrders ?? 0,
                            Name = chargeGroup.AllChargeGroupNames,
                            SectionCharges = new List<ActivityTuitionItem>()
                        };

                        chargeGroupTypes.Add(chargeGroup.AllChargeGroupNames, tuitionBySectionType);
                    }
                    // Create a new Fee type
                    else if (chargeGroup.AllChargeGroupTypes.Equals(FeeType))
                    {
                        FeeType feeType = new FeeType()
                        {
                            DisplayOrder = chargeGroup.AllChargeGroupOrders ?? 0,
                            Name = chargeGroup.AllChargeGroupNames,
                            FeeCharges = new List<ActivityDateTermItem>()
                        };

                        chargeGroupTypes.Add(chargeGroup.AllChargeGroupNames, feeType);
                    }
                    // Create a new Other type
                    else if (chargeGroup.AllChargeGroupTypes.Equals(OtherType))
                    {
                        OtherType otherType = new OtherType()
                        {
                            DisplayOrder = chargeGroup.AllChargeGroupOrders ?? 0,
                            Name = chargeGroup.AllChargeGroupNames,
                            OtherCharges = new List<ActivityDateTermItem>()
                        };

                        chargeGroupTypes.Add(chargeGroup.AllChargeGroupNames, otherType);
                    }
                    // Create a new Room and Board type
                    else if (chargeGroup.AllChargeGroupTypes.Equals(RoomAndBoardType))
                    {
                        RoomAndBoardType roomAndBoardType = new RoomAndBoardType()
                        {
                            DisplayOrder = chargeGroup.AllChargeGroupOrders ?? 0,
                            Name = chargeGroup.AllChargeGroupNames,
                            RoomAndBoardCharges = new List<ActivityRoomAndBoardItem>()
                        };

                        chargeGroupTypes.Add(chargeGroup.AllChargeGroupNames, roomAndBoardType);
                    }
                    // Create a new Tuition By Total type
                    else if (chargeGroup.AllChargeGroupTypes.Equals(TuitionByTotalType))
                    {
                        TuitionByTotalType tuitionByTotalType = new TuitionByTotalType()
                        {
                            DisplayOrder = chargeGroup.AllChargeGroupOrders ?? 0,
                            Name = chargeGroup.AllChargeGroupNames,
                            TotalCharges = new List<ActivityTuitionItem>()
                        };

                        chargeGroupTypes.Add(chargeGroup.AllChargeGroupNames, tuitionByTotalType);
                    }
                }
            }

            #endregion

            #region Sort Charge Groups

            // Make sure the group items are not empty
            if (groups != null && groups.Count > 0)
            {
                // Iterate over each group item
                foreach (FinancialActivityChargeGroups groupItem in groups)
                {
                    // The charge group name is guaranteed to return 
                    NamedType associatedChargeGroup = chargeGroupTypes[groupItem.ChrgGroupName];

                    if (associatedChargeGroup is OtherType)
                    {
                        ActivityDateTermItem item = new ActivityDateTermItem();
                        item.Amount = groupItem.Amount;
                        item.Date = groupItem.Date;
                        item.Description = groupItem.Description;
                        item.Id = groupItem.InvoiceNumber;
                        item.TermId = groupItem.Term;

                        (associatedChargeGroup as OtherType).OtherCharges.Add(item);
                    }
                    else if (associatedChargeGroup is TuitionBySectionType)
                    {
                        ActivityTuitionItem item = new ActivityTuitionItem();
                        item.Amount = groupItem.Amount;
                        item.Description = groupItem.CourseTitle;
                        item.Id = groupItem.Section;
                        item.TermId = groupItem.Term;
                        item.Classroom = groupItem.Classroom;
                        item.Credits = groupItem.Credits;
                        item.BillingCredits = groupItem.BillingCredits;
                        item.Ceus = groupItem.Ceus;
                        item.Days = ConvertMeetingDays(groupItem.Days);
                        item.Instructor = groupItem.Instructor;
                        item.Status = groupItem.Status;
                        //item.Times = groupItem.Times;
                        item.StartTime = GetStartTime(groupItem.Times);
                        item.EndTime = GetEndTime(groupItem.Times);

                        (associatedChargeGroup as TuitionBySectionType).SectionCharges.Add(item);
                    }
                    else if (associatedChargeGroup is RoomAndBoardType)
                    {
                        ActivityRoomAndBoardItem item = new ActivityRoomAndBoardItem();
                        item.Amount = groupItem.Amount;
                        item.Description = groupItem.Description;
                        item.Id = groupItem.InvoiceNumber;
                        item.TermId = groupItem.Term;
                        item.Amount = groupItem.Amount ?? 0;
                        item.Date = groupItem.Date;
                        item.Room = groupItem.BuildingRoom;

                        (associatedChargeGroup as RoomAndBoardType).RoomAndBoardCharges.Add(item);
                    }
                    else if (associatedChargeGroup is FeeType)
                    {
                        ActivityDateTermItem item = new ActivityDateTermItem();
                        item.Amount = groupItem.Amount;
                        item.Description = groupItem.Description;
                        item.Id = groupItem.InvoiceNumber;
                        item.TermId = groupItem.Term;
                        item.Amount = groupItem.Amount ?? 0;
                        item.Date = groupItem.Date;

                        (associatedChargeGroup as FeeType).FeeCharges.Add(item);
                    }
                    else if (associatedChargeGroup is TuitionByTotalType)
                    {
                        ActivityTuitionItem item = new ActivityTuitionItem();
                        item.Amount = groupItem.Amount;
                        item.Description = groupItem.CourseTitle;
                        item.Id = groupItem.Section;
                        item.TermId = groupItem.Term;
                        item.Classroom = groupItem.Classroom;
                        item.Credits = groupItem.Credits;
                        item.BillingCredits = groupItem.BillingCredits;
                        item.Ceus = groupItem.Ceus;
                        item.Days = ConvertMeetingDays(groupItem.Days);
                        item.Instructor = groupItem.Instructor;
                        item.Status = groupItem.Status;
                        //item.Times = groupItem.Times;
                        item.StartTime = GetStartTime(groupItem.Times);
                        item.EndTime = GetEndTime(groupItem.Times);

                        (associatedChargeGroup as TuitionByTotalType).TotalCharges.Add(item);
                    }
                }
            }

            #endregion


            if (chargeGroupTypes != null && chargeGroupTypes.Values != null && chargeGroupTypes.Values.Count > 0)
            {
                foreach (NamedType type in chargeGroupTypes.Values)
                {
                    if (type is FeeType)
                    {
                        charges.FeeGroups.Add(type as FeeType);
                    }
                    else if (type is OtherType)
                    {
                        // The Miscellaneous grouping will always display as the 99th item
                        if (type.DisplayOrder == 99)
                        {
                            charges.Miscellaneous = type as OtherType;
                        }
                        else
                        {
                            charges.OtherGroups.Add(type as OtherType);
                        }
                    }
                    else if (type is TuitionBySectionType)
                    {
                        charges.TuitionBySectionGroups.Add(type as TuitionBySectionType);
                    }
                    else if (type is TuitionByTotalType)
                    {
                        charges.TuitionByTotalGroups.Add(type as TuitionByTotalType);
                    }
                    else if (type is RoomAndBoardType)
                    {
                        charges.RoomAndBoardGroups.Add(type as RoomAndBoardType);
                    }
                }
            }
        }

        #endregion

        #region Build Deposits

        /// <summary>
        /// This method builds the deposit type items. 
        /// </summary>
        /// <param name="deposits">deposits category</param>
        /// <param name="groups">grouped deposit data</param>
        public void BuildDeposits(DepositCategory deposits, List<FinancialActivityDeposits> groups)
        {
            if (groups != null)
            {
                deposits.Deposits = new List<ActivityRemainingAmountItem>();

                foreach (FinancialActivityDeposits group in groups)
                {
                    ActivityRemainingAmountItem item = new ActivityRemainingAmountItem();
                    item.Amount = (decimal?)group.DepositAmts ?? 0;
                    item.Date = group.DepositDates;
                    item.Description = group.DepositTypeDescs;
                    item.Id = group.DepositIds;
                    item.PaidAmount = (decimal?)group.DepositAppliedAmts ?? 0;
                    item.RemainingAmount = (decimal?)group.DepositBalance ?? 0;
                    item.TermId = group.DepositTerms;
                    item.RefundAmount = (decimal?)group.DepositRefundAmts ?? 0;
                    item.OtherAmount = (decimal?)group.DepositOtherAmts ?? 0;
                    item.ReceiptId = group.DepositRcptId;

                    deposits.Deposits.Add(item);
                }
            }
        }

        #endregion

        #region Build Refunds

        /// <summary>
        /// This method builds the refund type items. 
        /// </summary>
        /// <param name="refunds">refund category</param>
        /// <param name="groups">grouped refund data</param>
        public void BuildRefunds(RefundCategory refunds, List<FinancialActivityRefunds> groups)
        {
            if (groups != null)
            {
                refunds.Refunds = new List<ActivityPaymentMethodItem>();

                foreach (FinancialActivityRefunds group in groups)
                {
                    ActivityPaymentMethodItem item = new ActivityPaymentMethodItem();
                    item.Amount = (decimal?)group.RefundAmount ?? 0;
                    item.Date = group.RefundDate;
                    item.Description = group.RefundDescription;
                    item.Id = group.RefundVoucherId;
                    item.TermId = group.RefundTerms;
                    item.Method = group.RefundPayMethod;
                    item.CheckDate = group.RefundCheckDate;
                    item.CheckNumber = group.RefundCheckNo;
                    item.CreditCardLastFourDigits = group.RefundCcLast4;
                    item.Status = GetRefundVoucherStatus(group.RefundStatus);
                    item.StatusDate = group.RefundStatusDate.GetValueOrDefault();
                    item.TransactionNumber = group.RefundTransNo;

                    refunds.Refunds.Add(item);
                }
            }
        }

        private RefundVoucherStatus GetRefundVoucherStatus(string source)
        {
            RefundVoucherStatus status = RefundVoucherStatus.Unknown;
            switch(source)
            {
                case "Cancelled":
                    status = RefundVoucherStatus.Cancelled;
                    break;
                case "InProgress":
                    status = RefundVoucherStatus.InProgress;
                    break;
                case "NotApproved":
                    status = RefundVoucherStatus.NotApproved;
                    break;
                case "Outstanding":
                    status = RefundVoucherStatus.Outstanding;
                    break;
                case "Paid":
                    status = RefundVoucherStatus.Paid;
                    break;
                case "Reconciled":
                    status = RefundVoucherStatus.Reconciled;
                    break;
                case "Voided":
                    status = RefundVoucherStatus.Voided;
                    break;
            }
            return status;
        }

        #endregion

        #region Build Payment Plans

        /// <summary>
        /// This method builds the payment plan type items. 
        /// </summary>
        /// <param name="paymentPlans">payment plans</param>
        /// <param name="paymentPlanGroups">payment plan groups</param>
        /// <param name="planSchedules">payment plan schedules</param>
        public void BuildPaymentPlans(PaymentPlanCategory paymentPlans, List<PaymentPlans> paymentPlanGroups, List<PlanSchedules> planSchedules)
        {
            if (paymentPlanGroups != null)
            {
                paymentPlans.PaymentPlans = new List<ActivityPaymentPlanDetailsItem>();

                foreach (PaymentPlans planGroup in paymentPlanGroups)
                {
                    ActivityPaymentPlanDetailsItem item = new ActivityPaymentPlanDetailsItem();
                    item.Amount = (decimal?)planGroup.PlanAmtDue ?? 0;
                    item.CurrentBalance = (decimal?)planGroup.PlanBalance ?? 0;
                    item.Id = planGroup.PlanId;
                    item.OriginalAmount = (decimal?)planGroup.PlanOrigAmount ?? 0;
                    item.TermId = planGroup.PlanTerm;
                    item.Type = planGroup.PlanType;
                    item.PaymentPlanApproval = planGroup.PlanApprovalId;

                    var plannedPayments = from schedule in planSchedules
                                          where schedule.SchedulePlanId != null && schedule.SchedulePlanId.Equals(planGroup.PlanId)
                                          select schedule;
                    if (plannedPayments != null)
                    {
                        foreach (var payment in plannedPayments)
                        {
                            ActivityPaymentPlanScheduleItem scheduleItem = new ActivityPaymentPlanScheduleItem();
                            scheduleItem.Amount = payment.ScheduleAmtDue ?? 0;
                            scheduleItem.AmountPaid = payment.ScheduleAmtPaid ?? 0;
                            scheduleItem.Id = payment.SchedulePlanId;
                            scheduleItem.Date = payment.ScheduleDueDate;
                            scheduleItem.LateCharge = payment.ScheduleLateCharge ?? 0;
                            scheduleItem.NetAmountDue = payment.ScheduleNetAmtDue ?? 0;
                            scheduleItem.SetupCharge = payment.ScheduleSetupCharge ?? 0;
                            scheduleItem.DatePaid = payment.ScheduleDatePaid;

                            item.PaymentPlanSchedules.Add(scheduleItem);
                        }
                    }

                    paymentPlans.PaymentPlans.Add(item);
                }
            }
        }

        #endregion

        #region Build Financial Aid

        public void BuildFinancialAid(FinancialAidCategory financialAid, List<AnticipatedFinancialAid> anticipatedFinAid, List<AnticipatedAidTerms> anticipatedAidTerms)
        {
            if (anticipatedFinAid != null)
            {
                financialAid.AnticipatedAid = new List<ActivityFinancialAidItem>();

                foreach (AnticipatedFinancialAid finAidItem in anticipatedFinAid)
                {
                    ActivityFinancialAidItem newItem = new ActivityFinancialAidItem();
                    newItem.AwardAmount = finAidItem.FaAwardAmt;
                    newItem.AwardDescription = finAidItem.FaAwardDesc;
                    newItem.Comments = finAidItem.FaAwardComments;
                    newItem.IneligibleAmount = finAidItem.FaAwardIneligAmt;
                    newItem.LoanFee = finAidItem.FaAwardLoanFee;
                    newItem.OtherTermAmount = finAidItem.FaAwardOtherAmt;
                    newItem.PeriodAward = finAidItem.FaAwardPeriodAward;
                    newItem.TransmitAwardExcess = finAidItem.FaAwardTransmitExcessInd;

                    if (anticipatedAidTerms != null)
                    {
                        IEnumerable<AnticipatedAidTerms> awardItems = from aidTerms in anticipatedAidTerms
                                                                       where (!string.IsNullOrEmpty(aidTerms.FaTermsPeriodAward) && aidTerms.FaTermsPeriodAward.Equals(newItem.PeriodAward))
                                                                       select aidTerms;

                        if (awardItems != null)
                        {
                            newItem.AwardTerms = new List<ActivityFinancialAidTerm>();
                            foreach (AnticipatedAidTerms item in awardItems)
                            {
                                ActivityFinancialAidTerm finAidTerm = new ActivityFinancialAidTerm();
                                finAidTerm.AnticipatedAmount = item.FaTermsAntAmt;
                                finAidTerm.AwardTerm = item.FaTermsAwardTerm;
                                finAidTerm.DisbursedAmount = item.FaTermsDisbAmt;

                                newItem.AwardTerms.Add(finAidTerm);
                            }
                        }
                    }

                    newItem.IneligibilityReasons.AddRange(ConvertSubvaluedStringToList(finAidItem.FaAwardEligibilityMsgs));
                    financialAid.AnticipatedAid.Add(newItem);
                }
            }
        }

        /// <summary>
        /// Takes in a string with subvalues and converts it into a list strings where each
        /// string is a subvalue
        /// </summary>
        /// <param name="stringToConvert">a string to be converted</param>
        /// <returns>created list of subvalued strings</returns>
        private IEnumerable<string> ConvertSubvaluedStringToList(string stringToConvert)
        {
            List<string> convertedList = new List<string>();
            if (!string.IsNullOrEmpty(stringToConvert))
            {
                char sm = Convert.ToChar(DynamicArray.SM);
                var msgs = stringToConvert.Split(sm);
                foreach(var msg in msgs)
                {
                    convertedList.Add(msg);
                }
            }
            return convertedList;
        }

        #endregion

        #region Build Sponsorships
        /// <summary>
        /// 2012.03.27 - SCR 36161 - AJK
        /// This method builds the sponsorship type items. 
        /// </summary>
        /// <param name="sponsorships">sponsorship category</param>
        /// <param name="groups">grouped sponsorship data</param>
        public void BuildSponsorships(SponsorshipCategory sponsorships, List<FinancialActivitySponsorPayments> groups)
        {
            if (groups != null)
            {
                sponsorships.SponsorItems = new List<ActivitySponsorPaymentItem>();

                foreach (FinancialActivitySponsorPayments group in groups)
                {
                    ActivitySponsorPaymentItem item = new ActivitySponsorPaymentItem();
                    item.Id = group.SponPayId;
                    item.Date = group.SponPayDate;
                    item.TermId = group.SponPayTerm;
                    item.Sponsorship = group.SponPaySponsor;
                    item.Amount = group.SponPayAmount;

                    sponsorships.SponsorItems.Add(item);
                }
            }
        }
        #endregion

        #region Build Student Payments
        /// <summary>
        /// 2012.04.13 - SCR 36161 - AJK
        /// This method builds the list of student payment items.
        /// </summary>
        /// <param name="sponsorships">sponsorship category</param>
        /// <param name="groups">grouped sponsorship data</param>
        [Obsolete("Obsolete as of API version 1.8, use BuildStudentPayments2 instead")]
        public void BuildStudentPayments(StudentPaymentCategory studentPayments, List<FinancialActivityStudentPayments> studentPaymentGroups)
        {
            if (studentPaymentGroups != null)
            {
                studentPayments.StudentPayments = new List<ActivityPaymentPaidItem>();

                foreach (FinancialActivityStudentPayments group in studentPaymentGroups)
                {
                    ActivityPaymentPaidItem item = new ActivityPaymentPaidItem();
                    item.Amount = (decimal?)group.StuPayAmount ?? 0;
                    item.Date = group.StuPayRcptDate;
                    item.Description = group.StuPayDescription;
                    item.Id = group.StuPayRcptNo;
                    item.TermId = group.StuPayTerm;
                    item.Method = group.StuPayMethod;
                    item.ReferenceNumber = group.StuPayRefNo;

                    studentPayments.StudentPayments.Add(item);
                }
            }
        }

        /// <summary>
        /// Builds the list of student payment items.
        /// </summary>
        /// <param name="sponsorships">sponsorship category</param>
        /// <param name="groups">grouped sponsorship data</param>
        public void BuildStudentPayments2(StudentPaymentCategory studentPayments, List<FinancialActivityStudentPayments> studentPaymentGroups)
        {
            if (studentPaymentGroups != null)
            {
                studentPayments.StudentPayments = new List<ActivityPaymentPaidItem>();

                foreach (FinancialActivityStudentPayments group in studentPaymentGroups)
                {
                    studentPayments.StudentPayments.Add(new ActivityPaymentPaidItem()
                    {
                        Amount = (decimal?)group.StuPayAmount ?? 0,
                        Date = group.StuPayRcptDate,
                        Description = group.StuPayDescription,
                        Id = group.StuPayRcptId,
                        TermId = group.StuPayTerm,
                        Method = group.StuPayMethod,
                        ReferenceNumber = group.StuPayRefNo,
                        ReceiptNumber = group.StuPayRcptNo
                    });
                }
            }
        }

        // End SCR 36161
        #endregion

        #region Convert time string into start and end times

        private string GetStartTime(string times)
        {
            if (String.IsNullOrEmpty(times))
            {
                return String.Empty;
            }
            string[] time = times.Split('-');
            return (String.IsNullOrEmpty(time[0])) ? String.Empty : time[0];
        }

        private string GetEndTime(string times)
        {
            if (String.IsNullOrEmpty(times))
            {
                return String.Empty;
            }
            string[] time = times.Split('-');
            return (time.Length < 2 || String.IsNullOrEmpty(time[1])) ? String.Empty : time[1];
        }

        #endregion

        #region Convert days of week string to List<DayOfWeek>

        private List<DayOfWeek> ConvertMeetingDays(string days)
        {
            var daysList = new List<DayOfWeek>();
            if (String.IsNullOrEmpty(days)) return daysList;
            if (_days == null)
            {
                _days = GetDaysOfWeek();
            }

            // Parse the string of days using the days-of-week table.  Since the string can contain values
            // of 1 or 2 characters, convert the 2 character codes, then the 1 character codes.

            for (int len = 2; len > 0; len--)
            {
                foreach (var day in _days)
                {
                    if (day.Code.Length == len)
                    {
                        days = days.Replace(day.Code, day.Day);
                    }
                }
            }

            // Now, build the list of days using the modified string
            if (days.IndexOf('0') >= 0)
            {
                daysList.Add(DayOfWeek.Sunday);
            }
            if (days.IndexOf('1') >= 0)
            {
                daysList.Add(DayOfWeek.Monday);
            }
            if (days.IndexOf('2') >= 0)
            {
                daysList.Add(DayOfWeek.Tuesday);
            }
            if (days.IndexOf('3') >= 0)
            {
                daysList.Add(DayOfWeek.Wednesday);
            }
            if (days.IndexOf('4') >= 0)
            {
                daysList.Add(DayOfWeek.Thursday);
            }
            if (days.IndexOf('5') >= 0)
            {
                daysList.Add(DayOfWeek.Friday);
            }
            if (days.IndexOf('6') >= 0)
            {
                daysList.Add(DayOfWeek.Saturday);
            }

            return daysList;
        }

        private IEnumerable<DaysOfWeek> GetDaysOfWeek()
        {
            var days = new List<DaysOfWeek>();
            var daysOfWeek = DataReader.ReadRecord<Ellucian.Data.Colleague.DataContracts.ApplValcodes>("ST.VALCODES", "DAYS.OF.WEEK");

            if (daysOfWeek == null)
            {
                // Record not found - Use default values
                days.Add(new DaysOfWeek("SU", "Sunday", "0"));
                days.Add(new DaysOfWeek("M", "Monday", "1"));
                days.Add(new DaysOfWeek("T", "Tuesday", "2"));
                days.Add(new DaysOfWeek("W", "Wednesday", "3"));
                days.Add(new DaysOfWeek("TH", "Thursday", "4"));
                days.Add(new DaysOfWeek("F", "Friday", "5"));
                days.Add(new DaysOfWeek("S", "Saturday", "6"));
            }
            else
            {
                foreach (var day in daysOfWeek.ValsEntityAssociation)
                {
                    days.Add(new DaysOfWeek(day.ValInternalCodeAssocMember, day.ValExternalRepresentationAssocMember, day.ValActionCode1AssocMember));
                }
            }

            return days;
        }

        // Private class for Colleague DAYS.OF.WEEK table
        private class DaysOfWeek
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public string Day { get; set; }

            public DaysOfWeek(string code, string description, string day)
            {
                Code = code;
                Description = description;
                Day = day;
            }
        }

        #endregion

    }
}
