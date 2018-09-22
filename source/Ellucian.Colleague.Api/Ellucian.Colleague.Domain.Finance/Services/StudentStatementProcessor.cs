// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Services;

namespace Ellucian.Colleague.Domain.Finance.Services
{
    /// <summary>
    /// Helper class that assists in generating Student Statements
    /// </summary>
    public class StudentStatementProcessor
    {
        private readonly string _timeframeId;
        private readonly ActivityDisplay _activityDisplay;
        private readonly PaymentDisplay _paymentDisplay;
        private decimal _pastPeriodBalance;
        private decimal _currentPeriodBalance;
        private decimal _futurePeriodBalance;
        private decimal _previousBalance;
        private decimal _futureBalance;
        private decimal _otherBalance;
        private List<DepositType> _depositTypes;
        private List<Term> _terms;
        private List<AccountPeriod> _termPeriods;
        private List<FinancialPeriod> _financialPeriods;

        /// <summary>
        /// Constructor for the StudentStatementProcessor
        /// </summary>
        /// <param name="activityDisplay">Account activity display method</param>
        /// <param name="paymentDisplay">Make a Payment display method</param>
        /// <param name="depositTypes">Collection of deposit types</param>
        /// <param name="terms">Collection of terms</param>
        /// <param name="termPeriods">Account activity term periods</param>
        /// <param name="financialPeriods">Collection of financial periods</param>
        public StudentStatementProcessor(string timeframeId, ActivityDisplay activityDisplay, PaymentDisplay paymentDisplay, IEnumerable<DepositType> depositTypes,
            IEnumerable<Term> terms, IEnumerable<AccountPeriod> termPeriods, IEnumerable<FinancialPeriod> financialPeriods)
        {
            if (string.IsNullOrEmpty(timeframeId))
            {
                throw new ArgumentNullException("timeframeId", "Term/Period ID cannot be null or empty.");
            }
            if (terms == null || terms.Count() == 0)
            {
                throw new ArgumentNullException("terms", "Terms cannot be null or empty.");
            }
            if (termPeriods == null)
            {
                throw new ArgumentNullException("termPeriods", "Term Periods cannot be null.");
            }
            if (financialPeriods == null)
            {
                throw new ArgumentNullException("financialPeriods", "Financial Periods cannot be null.");
            }

            _timeframeId = timeframeId;
            _activityDisplay = activityDisplay;
            _paymentDisplay = paymentDisplay;
            _depositTypes = (depositTypes != null) ? depositTypes.ToList() : new List<DepositType>();
            _terms = terms.ToList();
            _termPeriods = termPeriods.ToList();
            _financialPeriods = financialPeriods.ToList();

            CalculatePeriodBalances();
        }

        /// <summary>
        /// Filters and updates deposits due for a group of terms for display on Student Statements
        /// </summary>
        /// <param name="depositsDue">Collection of deposits due to be updated</param>
        /// <param name="termIds">Collection of IDs of terms for which to filter deposits due</param>
        /// <param name="startDate">Date on/after which deposits due for the student will be included on the statement (Past/Current/Future mode only)</param>
        /// <param name="endDate">Date on/before which deposits due for the student will be included on the statement (Past/Current/Future mode only)</param>
        /// <returns>Collection of filtered, statement-ready DepositDue entities</returns>
        public IEnumerable<DepositDue> FilterAndUpdateDepositsDue(IEnumerable<DepositDue> depositsDue, IEnumerable<string> termIds,
            DateTime? startDate, DateTime? endDate)
        {
            if (depositsDue == null || depositsDue.Count() == 0)
            {
                return depositsDue;
            }
            if (termIds == null || termIds.Count() == 0)
            {
                throw new ArgumentNullException("termIds", "Term IDs cannot be null or empty.");
            }
            if (termIds.Count() > 1 && !startDate.HasValue && !endDate.HasValue)
            {
                throw new ArgumentException("termIds", "Start and/or End Date must be supplied if more than one term ID is supplied.");
            }

            var filteredDepositsDue = new List<DepositDue>();
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    var termId = termIds.First();
                    var compareTerm = termId;
                    if (termId != null && termId == FinanceTimeframeCodes.NonTerm)
                    {
                        compareTerm = String.Empty;
                    }
                    filteredDepositsDue.AddRange(depositsDue.Where(dd => TermPeriodProcessor.AreTermIdsEqual(dd.TermId, compareTerm)).OrderBy(dd => dd.SortOrder));
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    filteredDepositsDue.AddRange(depositsDue.Where(dd => TermPeriodProcessor.IsInPeriod(dd.TermId, dd.DueDate, termIds, startDate, endDate)).OrderBy(dd => dd.SortOrder));
                    break;
            }

            // More than one term ID supplied - Past/Current/Future mode
            PopulateDepositDueDescriptions(filteredDepositsDue);
            return filteredDepositsDue;
        }

        /// <summary>
        /// Get the term IDs associated with a given timeframe
        /// </summary>
        /// <returns>Collection of term IDs associated with the supplied timeframe</returns>
        public IEnumerable<string> GetTermIdsForTimeframe()
        {
            List<string> termIds = new List<string>();
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByPeriod:
                    {
                        switch (_timeframeId)
                        {
                            case FinanceTimeframeCodes.PastPeriod:
                                var pastTerms = _terms.Where(t => t.FinancialPeriod == PeriodType.Past).ToList();
                                if (pastTerms != null)
                                {
                                    termIds.AddRange(pastTerms.Select(t => t.Code));
                                }
                                break;
                            case FinanceTimeframeCodes.CurrentPeriod:
                                var currentTerms = _terms.Where(t => t.FinancialPeriod == PeriodType.Current).ToList();
                                if (currentTerms != null)
                                {
                                    termIds.AddRange(currentTerms.Select(t => t.Code));
                                }
                                break;
                            case FinanceTimeframeCodes.FuturePeriod:
                                var futureTerms = _terms.Where(t => t.FinancialPeriod == PeriodType.Future).ToList();
                                if (futureTerms != null)
                                {
                                    termIds.AddRange(futureTerms.Select(t => t.Code));
                                }
                                break;
                            default:
                                termIds.Add(_timeframeId);
                                break;
                        }
                        break;
                    }
                case ActivityDisplay.DisplayByTerm:
                    {
                        termIds.Add(_timeframeId);
                        break;
                    }
            }
            return termIds;
        }

        /// <summary>
        /// Calculate the due date for a collection of account terms
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <param name="depositsDue">Collection of deposits due</param>
        /// <returns>Due date for a collection of account terms</returns>
        public DateTime? CalculateDueDate(IEnumerable<AccountTerm> accountTerms, IEnumerable<DepositDue> depositsDue)
        {
            List<DateTime?> itemDueDates = new List<DateTime?>();
            DateTime? dueDate = null;
            if (accountTerms != null && accountTerms.Any())
            {

                foreach (var accountTerm in accountTerms)
                {
                    if (accountTerm.AccountDetails != null && accountTerm.AccountDetails.Count > 0)
                    {
                        itemDueDates.AddRange(accountTerm.AccountDetails.Where(ad => (ad.AmountDue ?? 0) > 0).Select(ad => ad.DueDate));
                    }
                }
            }
            if (depositsDue != null && depositsDue.Any())
            {
                itemDueDates.AddRange(depositsDue.Where(dd => dd.Balance > 0).Select(d => (DateTime?)d.DueDate));
            }

            if (!itemDueDates.Any())
            { 
                return dueDate; 
            }
            
            itemDueDates.RemoveAll(d => d.Value < DateTime.Today);
            return itemDueDates.Min();
        }

        /// <summary>
        /// Calculate the previous balance for the specified timeframe
        /// </summary>
        /// <returns>Previous balance for the specified timeframe</returns>
        public decimal CalculatePreviousBalance()
        {
            foreach (AccountPeriod period in _termPeriods)
            {
                switch (_activityDisplay)
                {
                    case ActivityDisplay.DisplayByTerm:
                        if (_timeframeId != FinanceTimeframeCodes.NonTerm)
                        {
                            if (String.Compare(TermPeriodProcessor.GetTermSortOrder(period.Id, _terms), TermPeriodProcessor.GetTermSortOrder(_timeframeId, _terms)) < 0)
                            {
                                _previousBalance += period.Balance ?? 0;
                            }
                        }
                        break;

                    case ActivityDisplay.DisplayByPeriod:
                        if (_timeframeId == period.Id && period.Id == FinanceTimeframeCodes.PastPeriod)
                        {
                            _previousBalance = 0;
                            break;
                        }
                        else if (_timeframeId == period.Id && period.Id == FinanceTimeframeCodes.CurrentPeriod)
                        {
                            _previousBalance = _pastPeriodBalance;
                            break;
                        }
                        else
                        {
                            if (_timeframeId == period.Id && period.Id == FinanceTimeframeCodes.FuturePeriod)
                            {
                                _previousBalance = _pastPeriodBalance + _currentPeriodBalance;
                                break;
                            }
                        }
                        break;
                }
            }
            return _previousBalance;
        }

        /// <summary>
        /// Calculate the future balance for the specified timeframe
        /// </summary>
        /// <returns>Future balance for the specified timeframe</returns>
        public decimal CalculateFutureBalance()
        {
            foreach (AccountPeriod period in _termPeriods)
            {
                switch (_activityDisplay)
                {
                    case ActivityDisplay.DisplayByTerm:
                        if (_timeframeId == period.Id)
                        {
                            break;
                        }
                        else
                        {
                            if (string.Compare(TermPeriodProcessor.GetTermSortOrder(period.Id, _terms),
                                TermPeriodProcessor.GetTermSortOrder(_timeframeId, _terms)) > 0)
                            {
                                _futureBalance += period.Balance ?? 0;
                            }
                        }
                        break;
                    case ActivityDisplay.DisplayByPeriod:
                        switch (_timeframeId)
                        {
                            case FinanceTimeframeCodes.PastPeriod:
                                return _currentPeriodBalance + _futurePeriodBalance;
                            case FinanceTimeframeCodes.CurrentPeriod:
                                return _futurePeriodBalance;
                            default:
                                return 0;
                        }
                }
            }
            return _futureBalance;
        }

        /// <summary>
        /// Calculate the other balance for the specified timeframe
        /// </summary>
        /// <param name="nonTermAccountPeriod">Account periods</param>
        /// <returns>Other balance for the specified timeframe</returns>
        public decimal CalculateOtherBalance(AccountPeriod nonTermAccountPeriod)
        {
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    if (nonTermAccountPeriod != null)
                    {
                        _otherBalance = nonTermAccountPeriod.Balance ?? 0;
                        switch (_timeframeId)
                        {
                            case (FinanceTimeframeCodes.NonTerm):
                                _otherBalance = _termPeriods.Sum(tp => tp.Balance ?? 0);
                                _previousBalance = 0;
                                _futureBalance = 0;
                                break;
                            default:
                                _otherBalance = nonTermAccountPeriod.Balance ?? 0;
                                break;
                        }
                    }
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    _otherBalance = 0m;
                    break;
            }
            return _otherBalance;
        }

        /// <summary>
        /// Calculates the current balance for a student statement
        /// </summary>
        /// <param name="accountDetails">Detailed accounts receivable information for the student for a term or period</param>
        /// <returns>The current balance for a student statement</returns>
        public decimal CalculateCurrentBalance(DetailedAccountPeriod accountDetails)
        {
            decimal totalAmountDue = 0m;
            if (accountDetails == null)
            {
                return totalAmountDue;
            }
            var totalCharges = 0m;
            if (accountDetails.Charges != null)
            {
                if (accountDetails.Charges.FeeGroups != null && accountDetails.Charges.FeeGroups.Count > 0)
                {
                    totalCharges += accountDetails.Charges.FeeGroups.SelectMany(fg => fg.FeeCharges).Sum(fc => fc.Amount ?? 0);
                }
                if (accountDetails.Charges.Miscellaneous != null && accountDetails.Charges.Miscellaneous.OtherCharges != null
                    && accountDetails.Charges.Miscellaneous.OtherCharges.Count > 0)
                {
                    totalCharges += accountDetails.Charges.Miscellaneous.OtherCharges.Sum(oc => oc.Amount ?? 0);
                }
                if (accountDetails.Charges.OtherGroups != null && accountDetails.Charges.OtherGroups.Count > 0)
                {
                    totalCharges += accountDetails.Charges.OtherGroups.SelectMany(og => og.OtherCharges).Sum(oc => oc.Amount ?? 0);
                }
                if (accountDetails.Charges.RoomAndBoardGroups != null && accountDetails.Charges.RoomAndBoardGroups.Count > 0)
                {
                    totalCharges += accountDetails.Charges.RoomAndBoardGroups.SelectMany(rbg => rbg.RoomAndBoardCharges).Sum(rbc => rbc.Amount ?? 0);
                }
                if (accountDetails.Charges.TuitionBySectionGroups != null && accountDetails.Charges.TuitionBySectionGroups.Count > 0)
                {
                    totalCharges += accountDetails.Charges.TuitionBySectionGroups.SelectMany(tsg => tsg.SectionCharges).Sum(sc => sc.Amount ?? 0);
                }
                if (accountDetails.Charges.TuitionByTotalGroups != null && accountDetails.Charges.TuitionByTotalGroups.Count > 0)
                {
                    totalCharges += accountDetails.Charges.TuitionByTotalGroups.SelectMany(ttg => ttg.TotalCharges).Sum(tc => tc.Amount ?? 0);
                }
            }
            var totalStudentPayments = 0m;
            if (accountDetails.StudentPayments != null)
            {
                if (accountDetails.StudentPayments.StudentPayments != null && accountDetails.StudentPayments.StudentPayments.Count > 0)
                {
                    totalStudentPayments += accountDetails.StudentPayments.StudentPayments.Sum(sp => sp.Amount ?? 0);
                }
            }
            var totalDeposits = 0m;
            if (accountDetails.Deposits != null)
            {
                if (accountDetails.Deposits.Deposits != null && accountDetails.Deposits.Deposits.Count > 0)
                {
                    totalDeposits += accountDetails.Deposits.Deposits.Sum(d => d.PaidAmount ?? 0)
                        + accountDetails.Deposits.Deposits.Sum(x => x.RemainingAmount ?? 0);
                }
            }
            var totalSponsorships = 0m;
            if (accountDetails.Sponsorships != null)
            {
                if (accountDetails.Sponsorships.SponsorItems != null && accountDetails.Sponsorships.SponsorItems.Count > 0)
                {
                    totalSponsorships += accountDetails.Sponsorships.SponsorItems.Sum(si => si.Amount ?? 0);
                }
            }
            var totalFinancialAid = 0m;
            if (accountDetails.FinancialAid != null)
            {
                if (accountDetails.FinancialAid.AnticipatedAid != null && accountDetails.FinancialAid.AnticipatedAid.Count > 0)
                {
                    totalFinancialAid += accountDetails.FinancialAid.AnticipatedAid.Sum(aa => aa.AwardTerms.Sum(at => at.AnticipatedAmount ?? 0));
                    totalFinancialAid += accountDetails.FinancialAid.AnticipatedAid.Sum(aa => aa.AwardTerms.Sum(at => at.DisbursedAmount ?? 0));
                }
                if (accountDetails.FinancialAid.DisbursedAid != null && accountDetails.FinancialAid.DisbursedAid.Count > 0)
                {
                    totalFinancialAid += accountDetails.FinancialAid.DisbursedAid.Sum(aa => aa.Amount ?? 0);
                }
            }
            var totalRefunds = 0m;
            if (accountDetails.Refunds != null)
            {
                if (accountDetails.Refunds.Refunds != null && accountDetails.Refunds.Refunds.Count > 0)
                {
                    totalRefunds += accountDetails.Refunds.Refunds.Sum(r => r.Amount ?? 0);
                }
            }
            totalAmountDue = totalCharges - totalStudentPayments - totalDeposits - totalSponsorships - totalFinancialAid + totalRefunds;
            return totalAmountDue;
        }

        /// <summary>
        /// Calculates the total amount due for a student statement
        /// </summary>
        /// <param name="previousBalance">Previous balance for timeframe</param>
        /// <param name="currentBalance">Current balance for timeframe</param>
        /// <param name="payPlanAdjustments">Total of deferred payment plan amounts for timeframe</param>
        /// <param name="currentDepositsDue">Total of deposits due currently due for timeframe</param>
        /// <param name="futureOverdueAmounts">Past due amounts in future terms/periods</param>
        /// <returns>The total amount due for a student statement</returns>
        public decimal CalculateTotalAmountDue(decimal previousBalance, decimal currentBalance, decimal payPlanAdjustments,
            decimal currentDepositsDue, decimal futureOverdueAmounts)
        {
            return previousBalance + currentBalance - payPlanAdjustments + currentDepositsDue + futureOverdueAmounts;
        }

        /// <summary>
        /// Builds the summary portion of a student statement
        /// </summary>
        /// <param name="accountDetails">Detailed accounts receivable information for the student for a term or period</param>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <param name="depositsDue">Collection of deposits due</param>
        /// <param name="endDate">Date on which the student statement's reported accounts receivable activity starts</param>
        /// <param name="endDate">Date on which the student statement's reported accounts receivable activity ends</param>
        /// <returns>A student statement summary</returns>
        public StudentStatementSummary BuildStatementSummary(DetailedAccountPeriod accountDetails,
            IEnumerable<AccountTerm> accountTerms, decimal currentDepositsDue, DateTime? startDate, DateTime? endDate)
        {
            if (accountDetails == null)
            {
                return null;
            }

            List<ActivityTermItem> chargeInformation = new List<ActivityTermItem>();
            List<ActivityTermItem> nonChargeInformation = new List<ActivityTermItem>();

            if (accountDetails.Charges != null)
            {
                //Get charges for Tuition By Section
                ActivityTermItem tuitionBySectionCharge = new ActivityTermItem() { Description = "Tuition by Section" };
                if (accountDetails.Charges.TuitionBySectionGroups != null)
                {
                    tuitionBySectionCharge.Amount = accountDetails.Charges.TuitionBySectionGroups.Sum(x => x.SectionCharges.Sum(y => y.Amount ?? 0));
                }

                //Get charges for Tuition By Total
                ActivityTermItem tuitionByTotalCharge = new ActivityTermItem { Description = "Tuition by Total" };
                if (accountDetails.Charges.TuitionByTotalGroups != null)
                {
                    tuitionByTotalCharge.Amount = accountDetails.Charges.TuitionByTotalGroups.Sum(x => x.TotalCharges.Sum(y => y.Amount ?? 0));
                }

                //Get charges for Fees
                ActivityTermItem feeCharge = new ActivityTermItem() { Description = "Fees" };
                if (accountDetails.Charges.FeeGroups != null)
                {
                    feeCharge.Amount = accountDetails.Charges.FeeGroups.Sum(x => x.FeeCharges.Sum(y => y.Amount ?? 0));
                }

                //Get charges for Room and Board
                ActivityTermItem roomCharge = new ActivityTermItem() { Description = "Room & Board" };
                if (accountDetails.Charges.RoomAndBoardGroups != null)
                {
                    roomCharge.Amount = accountDetails.Charges.RoomAndBoardGroups.Sum(x => x.RoomAndBoardCharges.Sum(y => y.Amount ?? 0));
                }

                //Get charges for Miscellaneous/Other
                ActivityTermItem miscCharge = new ActivityTermItem() { Description = "Miscellaneous Charges", Amount = 0m };
                if (accountDetails.Charges.Miscellaneous != null)
                {
                    if (accountDetails.Charges.Miscellaneous.OtherCharges != null)
                    {
                        miscCharge.Amount += accountDetails.Charges.Miscellaneous.OtherCharges.Sum(x => x.Amount ?? 0);
                    }
                }

                if (accountDetails.Charges.OtherGroups != null)
                {
                    miscCharge.Amount += accountDetails.Charges.OtherGroups.Sum(x => x.OtherCharges.Sum(y => y.Amount ?? 0));
                }

                //Add charge groups to the collection
                chargeInformation.Add(tuitionBySectionCharge);
                chargeInformation.Add(tuitionByTotalCharge);
                chargeInformation.Add(feeCharge);
                chargeInformation.Add(roomCharge);
                chargeInformation.Add(miscCharge);
            }

            // Student Payments
            if (accountDetails.StudentPayments != null && accountDetails.StudentPayments.StudentPayments != null && accountDetails.StudentPayments.StudentPayments.Count != 0)
            {
                ActivityTermItem studentPaymentItem = new ActivityTermItem()
                {
                    Id = "-",
                    Description = "Student Payments",
                    TermId = string.Empty,
                    Amount = accountDetails.StudentPayments.StudentPayments.Sum(x => x.Amount ?? 0)
                };
                nonChargeInformation.Add(studentPaymentItem);
            }

            // Financial Aid
            if (accountDetails.FinancialAid != null)
            {
                if ((accountDetails.FinancialAid.AnticipatedAid != null && accountDetails.FinancialAid.AnticipatedAid.Count > 0)
                    || (accountDetails.FinancialAid.DisbursedAid != null && accountDetails.FinancialAid.DisbursedAid.Count > 0))
                {
                    ActivityTermItem financialAidItem = new ActivityTermItem()
                    {
                        Id = "-",
                        Description = "Financial Aid",
                        TermId = string.Empty,
                        Amount = 0m
                    };
                    if (accountDetails.FinancialAid.AnticipatedAid != null)
                    {
                        financialAidItem.Amount += accountDetails.FinancialAid.AnticipatedAid.Sum(x => x.AwardTerms.Sum(y => y.AnticipatedAmount ?? 0));
                        financialAidItem.Amount += accountDetails.FinancialAid.AnticipatedAid.Sum(x => x.AwardTerms.Sum(y => y.DisbursedAmount ?? 0));
                    }
                    if (accountDetails.FinancialAid.DisbursedAid != null)
                    {
                        financialAidItem.Amount += accountDetails.FinancialAid.DisbursedAid.Sum(x => x.Amount ?? 0);
                    }
                    nonChargeInformation.Add(financialAidItem);
                }
            }

            // Sponsorships
            if (accountDetails.Sponsorships != null && accountDetails.Sponsorships.SponsorItems != null && accountDetails.Sponsorships.SponsorItems.Count != 0)
            {
                ActivityTermItem sponsorItem = new ActivityTermItem()
                {
                    Id = "-",
                    Description = "Sponsorships",
                    TermId = string.Empty,
                    Amount = accountDetails.Sponsorships.SponsorItems.Sum(x => x.Amount ?? 0)
                };
                nonChargeInformation.Add(sponsorItem);
            }

            // Deposits
            if (accountDetails.Deposits != null && accountDetails.Deposits.Deposits != null && accountDetails.Deposits.Deposits.Count != 0)
            {
                ActivityTermItem depositItem = new ActivityTermItem()
                {
                    Id = "-",
                    Description = "Deposits",
                    TermId = string.Empty,
                    Amount = accountDetails.Deposits.Deposits.Sum(x => x.PaidAmount ?? 0) +
                        accountDetails.Deposits.Deposits.Sum(x => x.RemainingAmount ?? 0)
                };
                nonChargeInformation.Add(depositItem);
            }

            // Refunds
            if (accountDetails.Refunds != null && accountDetails.Refunds.Refunds != null && accountDetails.Refunds.Refunds.Count != 0)
            {
                ActivityTermItem refundItem = new ActivityTermItem()
                {
                    Id = "+",
                    Description = "Refunds",
                    TermId = string.Empty,
                    Amount = accountDetails.Refunds.Refunds.Sum(x => x.Amount ?? 0)
                };
                nonChargeInformation.Add(refundItem);
            }

            decimal payPlanAdjustments = 0m;
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    payPlanAdjustments = CalculatePaymentPlanAdjustments(accountTerms);
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    PeriodType? period = null;
                    switch (_timeframeId)
                    {
                        case FinanceTimeframeCodes.PastPeriod:
                            period = PeriodType.Past;
                            break;
                        case FinanceTimeframeCodes.CurrentPeriod:
                            period = PeriodType.Current;
                            break;
                        case FinanceTimeframeCodes.FuturePeriod:
                            period = PeriodType.Future;
                            break;
                    }
                    accountTerms = accountTerms.Where(at => TermPeriodProcessor.GetTermPeriod(at.TermId, _terms) == period).ToList();
                    payPlanAdjustments = CalculatePaymentPlanAdjustments(accountTerms);
                    break;
            }

            return new StudentStatementSummary(chargeInformation, nonChargeInformation, payPlanAdjustments, currentDepositsDue)
            {
                SummaryDateRange = GetSummaryHeaderDateRange(startDate, endDate),
                TimeframeDescription = GetTimeframeDescription()
            };
        }

        /// <summary>
        /// Builds a description of the previous balance
        /// </summary>
        /// <param name="startDate">Date on which the student statement's reported accounts receivable activity begins</param>
        /// <returns>A description of the previous balance</returns>
        public string BuildPreviousBalanceDescription(DateTime? startDate)
        {
            string previousBalanceDescription = string.Empty;
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    var term = _terms.Where(t => t.Code == _timeframeId).FirstOrDefault();
                    var termDesc = term != null ? term.Description : string.Empty;
                    previousBalanceDescription = string.Format("Charges before {0}", termDesc);
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    var startDateString = startDate != null ? startDate.Value.ToShortDateString() : String.Empty;
                    previousBalanceDescription = string.Format("Charges before {0}", startDateString);
                    break;
            }
            return previousBalanceDescription;
        }

        /// <summary>
        /// Builds a description of the future balance
        /// </summary>
        /// <param name="endDate">Date on which the student statement's reported accounts receivable activity ends</param>
        /// <returns>A description of the future balance</returns>
        public string BuildFutureBalanceDescription(DateTime? endDate)
        {
            string futureBalanceDescription = string.Empty;
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    var term = _terms.Where(t => t.Code == _timeframeId).FirstOrDefault();
                    var termDesc = term != null ? term.Description : string.Empty;
                    futureBalanceDescription = string.Format("Charges after {0}", termDesc);
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    var endDateString = endDate != null ? endDate.Value.ToShortDateString() : String.Empty;
                    futureBalanceDescription = string.Format("Charges after {0}", endDateString);
                    break;
            }
            return futureBalanceDescription;
        }

        /// <summary>
        /// Gets the summary header date range
        /// </summary>
        /// <param name="startDate">Date on which the student statement's reported accounts receivable activity starts</param>
        /// <param name="endDate">Date on which the student statement's reported accounts receivable activity ends</param>
        /// <returns>The summary header date range</returns>
        public string GetSummaryHeaderDateRange(DateTime? startDate, DateTime? endDate)
        {
            string summaryTermDate = string.Empty;

            if (_activityDisplay == ActivityDisplay.DisplayByPeriod)
            {
                switch (_timeframeId)
                {
                    case FinanceTimeframeCodes.PastPeriod:
                        summaryTermDate = (endDate != null) ? " (before " + endDate.Value.AddDays(1).ToShortDateString() + ")" : summaryTermDate;
                        break;
                    case FinanceTimeframeCodes.FuturePeriod:
                        summaryTermDate = (startDate != null) ? " (after " + startDate.Value.AddDays(-1).ToShortDateString() + ")" : summaryTermDate;
                        break;
                    default:
                        summaryTermDate = (startDate != null && endDate != null) ? " (" + startDate.Value.ToShortDateString() + " to " + endDate.Value.Date.ToShortDateString() + ")" : summaryTermDate;
                        break;
                }
            }
            return summaryTermDate;
        }

        /// <summary>
        /// Sort and consolidate account details for statement display
        /// </summary>
        /// <param name="accountDetails">Detailed accounts receivable information for the student for a term or period</param>
        public void SortAndConsolidateAccountDetails(DetailedAccountPeriod accountDetails)
        {
            if (accountDetails != null)
            {
                if (accountDetails.Charges != null)
                {
                    if (accountDetails.Charges.TuitionBySectionGroups != null && accountDetails.Charges.TuitionBySectionGroups.Count > 0)
                    {
                        if (accountDetails.Charges.TuitionBySectionGroups.Count > 1)
                        {
                            // Consolidate all TUIBS charges into a single group, 
                            // remove all other groups, then sort items in the remaining group by term sort order, then by course ID (i.e. ACCT-100-01)
                            accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges = new List<ActivityTuitionItem>(accountDetails.Charges.
                                TuitionBySectionGroups.SelectMany(tbs => tbs.SectionCharges));
                            accountDetails.Charges.TuitionBySectionGroups.RemoveRange(1, accountDetails.Charges.TuitionBySectionGroups.Count - 1);
                        }
                        accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges = accountDetails.Charges.TuitionBySectionGroups[0].
                            SectionCharges.OrderBy(sc => TermPeriodProcessor.GetTermSortOrder(TermPeriodProcessor.GetTermIdForTermDescription(sc.TermId, _terms), _terms)).ThenBy(sc => sc.Id).ToList();
                        var otherTuitionActivityIndex = accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges.FindIndex(tbs => tbs.Id == "Other Tuition Activity"
                            && string.IsNullOrEmpty(tbs.Description));
                        if (otherTuitionActivityIndex >= 0)
                        {
                            var item = accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges[otherTuitionActivityIndex];
                            accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges.Remove(item);
                            accountDetails.Charges.TuitionBySectionGroups[0].SectionCharges.Add(item);
                        }
                    }
                    if (accountDetails.Charges.TuitionByTotalGroups != null && accountDetails.Charges.TuitionByTotalGroups.Count > 0)
                    {
                        if (accountDetails.Charges.TuitionByTotalGroups.Count > 1)
                        {
                            // Consolidate all TUIBT charges into a single group, 
                            // remove all other groups, then sort items in the remaining group by term sort order, then by course ID (i.e. ACCT-100-01)
                            accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges = new List<ActivityTuitionItem>(accountDetails.Charges.
                                TuitionByTotalGroups.SelectMany(tbt => tbt.TotalCharges));
                            accountDetails.Charges.TuitionByTotalGroups.RemoveRange(1, accountDetails.Charges.TuitionByTotalGroups.Count - 1);
                        }
                        accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges = accountDetails.Charges.TuitionByTotalGroups[0].
                            TotalCharges.OrderBy(tc => TermPeriodProcessor.GetTermSortOrder(TermPeriodProcessor.GetTermIdForTermDescription(tc.TermId, _terms), _terms)).ThenBy(tc => tc.Id).ToList();
                        var otherTuitionActivityIndex = accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges.FindIndex(tbt => tbt.Id == "Other Tuition Activity"
                            && string.IsNullOrEmpty(tbt.Description));
                        if (otherTuitionActivityIndex >= 0)
                        {
                            var item = accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges[otherTuitionActivityIndex];
                            accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges.Remove(item);
                            accountDetails.Charges.TuitionByTotalGroups[0].TotalCharges.Add(item);
                        }
                    }

                    if (accountDetails.Charges.FeeGroups != null && accountDetails.Charges.FeeGroups.Count > 0)
                    {
                        if (accountDetails.Charges.FeeGroups.Count > 1)
                        {
                            // Consolidate all FEES charges into a single group, 
                            // remove all other groups, then sort items in the remaining group by term sort order
                            accountDetails.Charges.FeeGroups[0].FeeCharges = new List<ActivityDateTermItem>(accountDetails.Charges.
                                FeeGroups.SelectMany(f => f.FeeCharges));
                            accountDetails.Charges.FeeGroups.RemoveRange(1, accountDetails.Charges.FeeGroups.Count - 1);
                        }
                        accountDetails.Charges.FeeGroups[0].FeeCharges = accountDetails.Charges.FeeGroups[0].
                            FeeCharges.OrderBy(tc => TermPeriodProcessor.GetTermSortOrder(tc.TermId, _terms)).ToList();
                    }

                    if (accountDetails.Charges.RoomAndBoardGroups != null && accountDetails.Charges.RoomAndBoardGroups.Count > 0)
                    {
                        if (accountDetails.Charges.RoomAndBoardGroups.Count > 1)
                        {
                            // Consolidate all ROOMS charges into a single group, 
                            // remove all other groups, then sort items in the remaining group by term sort order
                            accountDetails.Charges.RoomAndBoardGroups[0].RoomAndBoardCharges = new List<ActivityRoomAndBoardItem>(accountDetails.Charges.
                                RoomAndBoardGroups.SelectMany(rb => rb.RoomAndBoardCharges));
                            accountDetails.Charges.RoomAndBoardGroups.RemoveRange(1, accountDetails.Charges.RoomAndBoardGroups.Count - 1);
                        }
                        accountDetails.Charges.RoomAndBoardGroups[0].RoomAndBoardCharges = accountDetails.Charges.RoomAndBoardGroups[0].
                            RoomAndBoardCharges.OrderBy(tc => TermPeriodProcessor.GetTermSortOrder(tc.TermId, _terms)).ToList();
                    }

                    var otherCharges = new List<ActivityDateTermItem>();
                    if (accountDetails.Charges.OtherGroups != null && accountDetails.Charges.OtherGroups.Count > 0)
                    {
                        otherCharges.AddRange(accountDetails.Charges.OtherGroups.SelectMany(o => o.OtherCharges));
                    }
                    if (accountDetails.Charges.Miscellaneous != null
                        && accountDetails.Charges.Miscellaneous.OtherCharges != null
                        && accountDetails.Charges.Miscellaneous.OtherCharges.Count > 0)
                    {
                        otherCharges.AddRange(accountDetails.Charges.Miscellaneous.OtherCharges);
                        accountDetails.Charges.Miscellaneous.OtherCharges.Clear();
                    }

                    if (accountDetails.Charges.OtherGroups != null && accountDetails.Charges.OtherGroups.Count > 0)
                    {
                        if (accountDetails.Charges.OtherGroups.Count > 1)
                        {
                            accountDetails.Charges.OtherGroups.RemoveRange(1, accountDetails.Charges.OtherGroups.Count - 1);
                        }
                        accountDetails.Charges.OtherGroups[0].OtherCharges = otherCharges.OrderBy(tc => TermPeriodProcessor.GetTermSortOrder(tc.TermId, _terms)).ToList();
                    }
                    else
                    {
                        accountDetails.Charges.OtherGroups = new List<OtherType>() { new OtherType() { OtherCharges = otherCharges } };
                    }
                }
            }
        }


        /// <summary>
        /// Calculates the sum of all payment plan scheduled payments that are not overdue or the next payment due on a given plan for a given timeframe
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <returns>The sum of all payment plan scheduled payments that are not overdue or the next payment due on a given plan for a given timeframe</returns>
        public decimal CalculatePaymentPlanAdjustments(IEnumerable<AccountTerm> accountTerms)
        {
            decimal paymentPlanAdjustments = 0;
            if (accountTerms == null || accountTerms.Count() == 0)
            {
                return paymentPlanAdjustments;
            }

            if (_activityDisplay == ActivityDisplay.DisplayByTerm)
            {
                accountTerms = accountTerms.Where(at => at.TermId == _timeframeId);
            }
            else
            {
                accountTerms = accountTerms.Where(at => TermPeriodProcessor.GetTermPeriod(at.TermId, _terms) == FinancialPeriodProcessor.GetPeriodType(_timeframeId)
                    || at.TermId == FinanceTimeframeCodes.NonTerm);
            }

            return CalculatePaymentPlanAdjustmentsForAccountTerms(accountTerms);
        }

        /// <summary>
        /// Calculates the sum of all deposits due that are currently due for a given timeframe
        /// </summary>
        /// <param name="depositsDue">Collection of all deposits due for account holder</param>
        /// <returns>The sum of all deposits due that are currently due for a given timeframe</returns>
        public decimal CalculateCurrentDepositsDue(IEnumerable<DepositDue> depositsDue)
        {
            decimal currentDepositsDue = 0;
            if (depositsDue == null || depositsDue.Count() == 0)
            {
                return currentDepositsDue;
            }
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    currentDepositsDue += _timeframeId == FinanceTimeframeCodes.NonTerm ?
                        depositsDue.Where(dd => String.IsNullOrEmpty(dd.TermId) || dd.Overdue).Sum(dd => dd.Balance)
                        : depositsDue.Where(dd => (string.IsNullOrEmpty(dd.TermId) && dd.Overdue) || string.Compare(TermPeriodProcessor.GetTermSortOrder(dd.TermId, _terms),
                                TermPeriodProcessor.GetTermSortOrder(_timeframeId, _terms)) <= 0 && dd.TermId != "").Sum(dd => dd.Balance);
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    FinancialPeriod period = null;
                    DateTime? startDate = null;
                    DateTime? endDate = null;
                    switch (_timeframeId)
                    {
                        case FinanceTimeframeCodes.PastPeriod:
                            period = _financialPeriods.Where(fp => fp.Type == PeriodType.Past).FirstOrDefault();
                            startDate = period != null ? (DateTime?)period.Start : null;
                            endDate = period != null ? (DateTime?)period.End : null;
                            currentDepositsDue += depositsDue.Where(dd => TermPeriodProcessor.GetTermPeriod(dd.TermId, _terms) == PeriodType.Past ||
                                TermPeriodProcessor.IsDateInRange(dd.DueDate, startDate, endDate)).Sum(dd => dd.Balance);
                            break;
                        case FinanceTimeframeCodes.CurrentPeriod:
                            period = _financialPeriods.Where(fp => fp.Type == PeriodType.Current).FirstOrDefault();
                            endDate = period != null ? (DateTime?)period.End : null;
                            currentDepositsDue += depositsDue.Where(dd => TermPeriodProcessor.GetTermPeriod(dd.TermId, _terms) != PeriodType.Future ||
                                TermPeriodProcessor.IsDateInRange(dd.DueDate, startDate, endDate)).Sum(dd => dd.Balance);
                            break;
                        case FinanceTimeframeCodes.FuturePeriod:
                            currentDepositsDue += depositsDue.Sum(dd => dd.Balance);
                            break;
                    }
                    break;
            }
            return currentDepositsDue;
        } 

        /// <summary>
        /// Set refund reference numbers and status dates for statement display
        /// </summary>
        /// <param name="refunds">Collection of refunds</param>
        public void UpdateRefundDatesAndReferenceNumbers(IEnumerable<ActivityPaymentMethodItem> refunds)
        {
            if (refunds != null)
            {
                foreach (var refund in refunds)
                {
                    // Set the refund reference number
                    switch (refund.Status)
                    {
                        case RefundVoucherStatus.Outstanding:
                        case RefundVoucherStatus.NotApproved:
                            refund.CheckNumber = "In Progress";
                            break;
                        case RefundVoucherStatus.Paid:
                        case RefundVoucherStatus.Reconciled:
                            if (string.IsNullOrEmpty(refund.CheckNumber))
                            {
                                refund.CheckNumber = refund.CreditCardLastFourDigits;
                            }
                            break;
                        default:
                            refund.CheckNumber = string.Empty;
                            break;
                    }

                    // Set the refund status date using the check date (if supplied)
                    if (refund.CheckDate.HasValue)
                    {
                        refund.StatusDate = refund.CheckDate.GetValueOrDefault();
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the portion of the total balance that is past due
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <param name="depositsDue">Collection of deposits due</param>
        /// <returns>The past due portion of the total balance</returns>
        public decimal CalculateOverdueAmount(IEnumerable<AccountTerm> accountTerms, IEnumerable<DepositDue> depositsDue)
        {
            decimal overdueAmount = 0m;
            if ((accountTerms == null || accountTerms.Count() == 0) && (depositsDue == null || depositsDue.Count() == 0))
            {
                return overdueAmount;
            }

            if (accountTerms != null)
            {
                foreach (var accountTerm in accountTerms)
                {
                    if (accountTerm.AccountDetails != null && accountTerm.AccountDetails.Count > 0)
                    {
                        overdueAmount += accountTerm.AccountDetails.Where(ad => (ad.DueDate.HasValue && ad.DueDate < DateTime.Today)).Sum(ad => ad.AmountDue ?? 0);
                    }
                }
            }

            if (depositsDue != null)
            {
                overdueAmount += depositsDue.Where(d => d.DueDate < DateTime.Today).Sum(d => d.Balance);
            }

            return overdueAmount;
        }

        /// <summary>
        /// Calculate the portion of the total balance that is past due that is in a future term/period
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <returns>The past due portion of the total balance that is in a future term/period</returns>
        public decimal CalculateFutureOverdueAmounts(IEnumerable<AccountTerm> accountTerms)
        {
            decimal futureOverdueAmount = 0m;
            if (accountTerms == null || accountTerms.Count() == 0)
            {
                return futureOverdueAmount;
            }

            List<AccountTerm> futureTerms = new List<AccountTerm>();
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByTerm:
                    futureTerms = accountTerms.Where(dd => string.Compare(TermPeriodProcessor.GetTermSortOrder(dd.TermId, _terms),
                        TermPeriodProcessor.GetTermSortOrder(_timeframeId, _terms)) > 0 && dd.TermId != "").ToList();
                    break;
                case ActivityDisplay.DisplayByPeriod:
                    switch (_timeframeId)
                    {
                        case FinanceTimeframeCodes.PastPeriod:
                            futureTerms = accountTerms.Where(at => TermPeriodProcessor.GetTermPeriod(at.TermId, _terms) != PeriodType.Past).ToList();
                            break;
                        case FinanceTimeframeCodes.CurrentPeriod:
                            futureTerms = accountTerms.Where(at => TermPeriodProcessor.GetTermPeriod(at.TermId, _terms) == PeriodType.Future).ToList();
                            break;
                        case FinanceTimeframeCodes.FuturePeriod:
                            break;
                    }
                    break;
            }

            if (futureTerms.Any())
            {
                foreach (var accountTerm in futureTerms)
                {
                    if (accountTerm.AccountDetails != null && accountTerm.AccountDetails.Count > 0)
                    {
                        futureOverdueAmount += accountTerm.AccountDetails.Where(ad => (ad.DueDate.HasValue && ad.DueDate < DateTime.Today)).Sum(ad => ad.AmountDue ?? 0);
                    }
                }
            }
            return futureOverdueAmount;
        }

        /// <summary>
        /// Calculate the portion of the total balance that is currently due
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <returns>The currently due portion of the total balance</returns>
        public decimal CalculateCurrentAmountDue(IEnumerable<AccountTerm> accountTerms)
        {
            decimal currentDueAmount = 0m;
            if (accountTerms == null || accountTerms.Count() == 0)
            {
                return currentDueAmount;
            }

            foreach (var accountTerm in accountTerms)
            {
                if (accountTerm.AccountDetails != null && accountTerm.AccountDetails.Count > 0)
                {
                    currentDueAmount += accountTerm.AccountDetails.Where(ad => (ad.DueDate.HasValue && ad.DueDate >= DateTime.Today)).Sum(ad => ad.AmountDue ?? 0);
                }
            }
            return currentDueAmount;
        }

        /// <summary>
        /// Gets the description of the term or period for a statement
        /// </summary>
        /// <returns>The description of the term or period for a statement</returns>
        private string GetTimeframeDescription()
        {
            switch (_timeframeId)
            {
                case FinanceTimeframeCodes.PastPeriod:
                    return "Past";
                case FinanceTimeframeCodes.CurrentPeriod:
                    return "Current";
                case FinanceTimeframeCodes.FuturePeriod:
                    return "Future";
                case FinanceTimeframeCodes.NonTerm:
                    return "Non-Term";
                default:
                    var term = _terms.Where(t => t.Code == _timeframeId).FirstOrDefault();
                    return term.Description;
            }
        }

        /// <summary>
        /// Populates deposit due deposit type descriptions and term descriptions for display on Student Statements
        /// </summary>
        /// <param name="depositsDue">Collection of deposits due to be updated</param>
        private void PopulateDepositDueDescriptions(IEnumerable<DepositDue> depositsDue)
        {
            if (depositsDue == null || depositsDue.Count() == 0)
            {
                return;
            }
            foreach (var depositDue in depositsDue)
            {
                if (_depositTypes != null && _depositTypes.Count() > 0)
                {
                    var depositType = _depositTypes.Where(dt => dt.Code == depositDue.DepositType).FirstOrDefault();
                    depositDue.DepositTypeDescription = depositType != null ? depositType.Description : depositDue.DepositTypeDescription;
                }
                var term = _terms.Where(t => t.Code == depositDue.TermId).FirstOrDefault();
                depositDue.TermDescription = term != null ? term.Description : depositDue.TermDescription;
            }
        }

        /// <summary>
        /// Calculate the balances of the three PCF periods.
        /// </summary>
        /// <param name="periods">Account activity term periods</param>
        private void CalculatePeriodBalances()
        {
            switch (_activityDisplay)
            {
                case ActivityDisplay.DisplayByPeriod:
                    foreach (AccountPeriod period in _termPeriods)
                    {
                        switch (period.Id)
                        {
                            case FinanceTimeframeCodes.PastPeriod:
                                _pastPeriodBalance = period.Balance ?? 0;
                                break;
                            case FinanceTimeframeCodes.CurrentPeriod:
                                _currentPeriodBalance = period.Balance ?? 0;
                                break;
                            case FinanceTimeframeCodes.FuturePeriod:
                                _futurePeriodBalance = period.Balance ?? 0;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case ActivityDisplay.DisplayByTerm:
                    return;
            }
        }

        /// <summary>
        /// Calculates the sum of all payment plan scheduled payments that are not overdue or the next payment due on a given plan for a collection of account terms
        /// </summary>
        /// <param name="accountTerms">Collection of account terms</param>
        /// <returns>The sum of all payment plan scheduled payments that are not overdue or the next payment due on a given plan for a collection of account terms</returns>
        private decimal CalculatePaymentPlanAdjustmentsForAccountTerms(IEnumerable<AccountTerm> accountTerms)
        {
            decimal paymentPlanAdjustments = 0m;
            foreach (var accountTerm in accountTerms)
            {
                if (accountTerm.AccountDetails != null && accountTerm.AccountDetails.Count > 0)
                {
                    // Remove all overdue and non-plan items so that only non-overdue payment plan items remain
                    var itemsToInspect = accountTerm.AccountDetails.Where(item => item is PaymentPlanDueItem && !item.Overdue).ToList();
                    if (itemsToInspect.Any())
                    {
                        // Convert the remaining items to plan items, then sort by payment plan ID, then by due date
                        List<PaymentPlanDueItem> payPlanDueItems = new List<PaymentPlanDueItem>();
                        var planItems = itemsToInspect.ConvertAll
                            (ad => ad as PaymentPlanDueItem).OrderBy(ad => ad.PaymentPlanId).ThenBy(ad => ad.DueDate);
                        if (planItems != null)
                        {
                            payPlanDueItems.AddRange(planItems);
                        }

                        var paymentPlanIds = payPlanDueItems.Select(ppdi => ppdi.PaymentPlanId).Distinct().ToList();
                        if (paymentPlanIds != null && paymentPlanIds.Count > 0)
                        {
                            foreach (var planId in paymentPlanIds)
                            {
                                var scheduledPayments = payPlanDueItems.Where(ppdi => ppdi.PaymentPlanId == planId).ToList();
                                if (scheduledPayments != null && scheduledPayments.Count > 0)
                                {
                                    scheduledPayments.RemoveAt(0);
                                    if (scheduledPayments.Count > 0)
                                    {
                                        paymentPlanAdjustments += scheduledPayments.Sum(sp => sp.AmountDue ?? 0);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return paymentPlanAdjustments;
        }
    }
}
