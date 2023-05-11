/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    [Serializable]
    public class EmployeeLeavePlan
    {
        /// <summary>
        /// The database Id of the Employee Leave Plan
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The Colleague PERSON id of the Employee
        /// </summary>
        public string EmployeeId { get; private set; }

        /// <summary>
        /// The start date of this employee leave plan
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The end date of this employee leave plan. If null, leave plan has no end date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The database id of the LeavePlan definition
        /// </summary>
        public string LeavePlanId { get; private set; }

        /// <summary>
        /// The description of the LeavePlan definition
        /// </summary>
        public string LeavePlanDescription { get; private set; }

        /// <summary>
        /// The start date of the LeavePlan definition
        /// </summary>
        public DateTime LeavePlanStartDate { get; private set; }

        /// <summary>
        /// The end date of the LeavePlan definition. If null, leave plan definition has no end date
        /// </summary>
        public DateTime? LeavePlanEndDate { get; private set; }

        /// <summary>
        /// The end date of the probationary period for this employee's leave plan
        /// </summary>
        public DateTime LeaveAllowedDate { get; set; }

        /// <summary>
        /// The leave balance as of the last pay period
        /// </summary>
        public decimal PriorPeriodLeaveBalance { get; private set; }

        /// <summary>
        /// Indicates if this leave plan allows a negative balance
        /// </summary>
        public bool AllowNegativeBalance { get; private set; }

        /// <summary>
        /// The category of the leave plan based on the leave plan definition's leave type. 
        /// </summary>
        public LeaveTypeCategory LeavePlanTypeCategory { get; private set; }

        /// <summary>
        /// The database Id of the EarningsType associated to this Leave Plan. When the employee takes this type of leave,
        /// they track their leave hours to this earnings type
        /// </summary>
        public string EarningsTypeId { get; private set; }

        /// <summary>
        /// The description of the EarningsType identified by the EarningsTypeId. Also see the /earnings-types endpoint for more
        /// details about Earnings Types.
        /// </summary>
        public string EarningsTypeDescription { get; private set; }

        /// <summary>
        /// The Month (1-12) that begins the plan year.
        /// </summary>
        public int PlanYearStartMonth { get; private set; }

        /// <summary>
        /// The day (1-the number of days in the month) that begin the plan year
        /// </summary>
        public int PlanYearStartDay { get; private set; }

        /// <summary>
        /// Accrual Rate
        /// </summary>
        public Decimal? AccrualRate { get; private set; }

        /// <summary>
        /// Accrual Limit
        /// </summary>
        public Decimal? AccrualLimit { get; private set; }

        /// <summary>
        /// Accrual Maximum Carry Over
        /// </summary>
        public Decimal? AccrualMaxCarryOver { get; private set; }

        /// <summary>
        /// Accrual Maximum Roll Over
        /// </summary>
        public decimal? AccrualMaxRollOver { get; private set; }

        /// <summary>
        /// Accrual Method
        /// </summary>
        public string AccrualMethod { get; private set; }

        /// <summary>
        /// Indicates if this is a leave reporting plan
        /// </summary>
        public bool IsLeaveReportingPlan { get; private set; }

        /// <summary>
        /// List of all Earning Type IDs associated with a Employee Leave Plan
        /// </summary>
        public IEnumerable<string> EarningTypeIDList { get; private set; }

        /// <summary
        /// Optional date 
        /// </summary
        public DateTime? LatestCarryoverDate { get; private set; }

        /// <summary>
        /// The start date of the current (as of today) plan year.
        /// The start date is built by using the PlanYearStartMonth and PlanYearStartDay and then deriving the year
        /// based on today's date. 
        /// </summary>
        public DateTime CurrentPlanYearStartDate {
            get {
                // default the current plan year start date to 1/1 of the current year
                var planYearStartDate = new DateTime(DateTime.Today.Year, 1, 1);

                // If the Perlv plan has a latest start balanace update date set via LCOV- use it
                if (LatestCarryoverDate.HasValue)
                {
                    planYearStartDate = LatestCarryoverDate.Value;
                }
                else
                {
                    // set the planYearStartDate to use the day and month with the current year if PlanYearStartDate is defined
                    if (IsPlanYearStartDateDefined)
                    {
                        //most plan years start January 1 (1/1). start with today's year, and the specified plan month and day.       
                        planYearStartDate = new DateTime(DateTime.Today.Year, PlanYearStartMonth, PlanYearStartDay);
                    }

                    //if the plan year starts July 1 (7/1), and today is before July 1 (5/1 for instance),
                    //the plan year start date is July 1 in the year previous to today's year
                    if (DateTime.Today < planYearStartDate)
                    {
                        planYearStartDate = planYearStartDate.AddYears(-1);
                    }
                }
                return planYearStartDate;
            }
        }

        /// <summary>
        /// Calculates the current plan years end date
        /// Effectively sets the end date to 364 days after the current plan year start date
        /// </summary>
        public DateTime CurrentPlanYearEndDate {
            get {
                return CurrentPlanYearStartDate.AddYears(1).AddDays(-1);
            }
        }

        /// <summary>
        /// The last transaction of the prior plan year (as of today). The ForwardingBalance of this
        /// transaction is the starting balance of the current plan year.
        /// </summary>
        public virtual EmployeeLeaveTransaction PriorPlanYearEndTransaction {
            get {
                var planYearStartDate = CurrentPlanYearStartDate;
                foreach (var transaction in SortedLeaveTransactions.Reverse())
                {
                    if (transaction.Date > planYearStartDate)
                    {
                        continue;
                    }
                    // if there is a starting balance record found, do not look for any other trx's on the current plan year start date
                    // this prevents a B and J record from conflicting scenario
                    if (StartingBalanceTransaction == null)
                    {
                        if (transaction.Date == planYearStartDate && transaction.Type == LeaveTransactionType.Adjusted)
                        {
                            return transaction;
                        }
                    }
                    if (transaction.Date < planYearStartDate)
                    {
                        return transaction;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The index of PriorPlanYearEndTransaction in leaveTransactions.
        /// If PriorPlanYearEndTransaction is null, this method will return -1;
        /// This is for the "old method"
        /// </summary>
        public virtual int PriorPlanYearEndTransactionIndex {
            get {
                var endTransaction = PriorPlanYearEndTransaction;
                if (endTransaction == null)
                {
                    return -1;
                }
                // Sort leave transactions by date then by id
                leaveTransactions = leaveTransactions.OrderBy(slt => slt.Date)
                    .ThenBy(slt => slt.Id)
                    .ToList();

                var index = leaveTransactions.BinarySearch(endTransaction);
                if (index < 0)
                {
                    return -1;//this shouldn't happen
                }
                return index; //index of the prior plan year end transaction 
            }
        }

        /// <summary>
        /// The index of PriorPlanYearEndTransaction in SortedLeaveTransactions.
        /// If PriorPlanYearEndTransaction is null, this method will return -1;
        /// This is for the "old method"
        /// </summary>
        public int SortedPriorPlanYearEndIndex {
            get {
                var endTransaction = PriorPlanYearEndTransaction;
                if (endTransaction == null)
                {
                    return -1;
                }
                var index = SortedLeaveTransactions.IndexOf(endTransaction);
                if (index < 0)
                {
                    return -1;//this shouldn't happen
                }
                return index; //index of the prior plan year end transaction 
            }
        }
        /// <summary>
        /// Attempts to get the "B" record on the PERLV.LATEST.START.BAL.UPDT (if set)
        /// returns null if not found
        /// </summary>

        public virtual EmployeeLeaveTransaction StartingBalanceTransaction {
            get {
                var startingBalanceTransaction = SortedLeaveTransactions
                    .Where(pld => pld.Date == CurrentPlanYearStartDate && pld.Type == LeaveTransactionType.StartingBalance)
                    .FirstOrDefault();
                return startingBalanceTransaction;
            }
        }

        /// <summary>
        /// The Current Balance from the B record, if it exists
        /// </summary>
        public virtual decimal RetroAvailableBalance {
            get {
                decimal retroAvailableBalance = 0;
                if (StartingBalanceTransaction != null)
                {
                    // add the forwarding balance from the starting balance transaction to the starting balance
                    retroAvailableBalance = StartingBalanceTransaction.CurrentBalance;
                }
                return retroAvailableBalance;
            }
        }

        /// <summary>
        /// The starting balance for this leave plan of the current plan year (as of today).
        /// </summary>
        public virtual decimal CurrentPlanYearStartingBalance {
            get {
                decimal startingBalance = 0; // of no cases below are met, 0 is returned for the starting balance
                var startingTransactionIndex = SortedPriorPlanYearEndIndex + 1;

                var startingBalanceAdjustments = SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(pld => pld.Date == CurrentPlanYearStartDate)
                    .Where(pld => pld.Type == LeaveTransactionType.StartingBalanceAdjustment);

                // if there ia starting balance transaction ("B" transaction on PERLV.LATEST.START.BAL.UPDT date) use the NEW METHOD
                if (StartingBalanceTransaction != null)
                {
                    // add the forwarding balance from the starting balance transaction to the starting balance
                    startingBalance += StartingBalanceTransaction.ForwardingBalance;
                    startingBalance += startingBalanceAdjustments.Sum(trans => trans.TransactionHours);
                }
                // no starting balance transaction - OLD WAY, check J record or latest record in prior plan year
                else
                {
                    // and there is a prior plan year end transaction on the current plan year start date.
                    if (PriorPlanYearEndTransaction != null)
                    {
                        // filter out the transactions that are already included in the starting balance
                        startingBalanceAdjustments = startingBalanceAdjustments.Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);

                        // add the starting balance to the prior plan year end transactions forwarding balance
                        startingBalance += PriorPlanYearEndTransaction.ForwardingBalance;
                        PriorYearForwardingBalanceHours = PriorPlanYearEndTransaction.ForwardingBalance;

                        // IF the PriorPlanYearEndTransaction is NOT a J record:     
                        if (PriorPlanYearEndTransaction.Type != LeaveTransactionType.Adjusted)
                        {
                            // sum of all (S)tarting balance adjustment transactions on the same date.
                            startingBalance += startingBalanceAdjustments.Sum(trans => trans.TransactionHours);
                        }
                    }
                    // else - there is no prior plan year end transaction but there are S records on CurrentPlanYearStartDate
                    else
                    {
                        // sum of all (S)tarting balance adjustment transactions on the same date.
                        startingBalance += startingBalanceAdjustments.Sum(trans => trans.TransactionHours);
                    }
                }
                return startingBalance;
            }
        }

        /// <summary>
        /// The number of earned hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearEarnedHours {
            get {
                var startingTransactionIndex = SortedPriorPlanYearEndIndex + 1; //index of the first transaction in the plan year
                var currentPlanYearEarnedTransactions = SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => trans.Type == LeaveTransactionType.Earned &&
                        trans.Date >= CurrentPlanYearStartDate);

                // if there is a "B" record - USE NEW METHOD
                if (StartingBalanceTransaction != null)
                {
                    // use logic above
                }
                else if (PriorPlanYearEndTransaction != null)
                {
                    // filter out the transactions that are already included in the starting balance
                    currentPlanYearEarnedTransactions = currentPlanYearEarnedTransactions.Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);
                    // check for retro transactions that aren't included in the prior plan year end record already.
                    var retroEarnedTransactions = SortedLeaveTransactions
                        .Where(trans => trans.Type == LeaveTransactionType.Earned)
                        .Where(trans => trans.Date < CurrentPlanYearStartDate)
                        .Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);
                    // add them to the curernt plan year earned transactions
                    currentPlanYearEarnedTransactions = currentPlanYearEarnedTransactions.Union(retroEarnedTransactions);
                }
                return currentPlanYearEarnedTransactions.Sum(trans => trans.TransactionHours); ;
            }
        }

        /// <summary>
        /// The number of used hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearUsedHours {
            get {
                var startingTransactionIndex = SortedPriorPlanYearEndIndex + 1; //index of the first transaction in the plan year
                var currentPlanYearUsedTransactions = SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => (trans.Type == LeaveTransactionType.Used || trans.Type == LeaveTransactionType.LeaveReporting) &&
                        trans.Date >= CurrentPlanYearStartDate);

                // if there is a "B" record - USE NEW METHOD
                if (StartingBalanceTransaction != null)
                {
                    // use logic above
                }
                // if there isn't a "B" record
                // need to include retro transactions that were added after 
                else if (PriorPlanYearEndTransaction != null)
                {
                    // filter out the transactions that are already included in the starting balance
                    currentPlanYearUsedTransactions = currentPlanYearUsedTransactions.Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);
                    // check for retro transactions that aren't included in the prior plan year end record already.
                    var retroUsedTransactions = SortedLeaveTransactions
                        .Where(trans => trans.Type == LeaveTransactionType.Used || trans.Type == LeaveTransactionType.LeaveReporting)
                        .Where(trans => trans.Date < CurrentPlanYearStartDate)
                        .Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);
                    // add them to the current plan year used transactions
                    currentPlanYearUsedTransactions = currentPlanYearUsedTransactions.Union(retroUsedTransactions);
                }
                return currentPlanYearUsedTransactions.Sum(trans => trans.TransactionHours);
            }
        }

        /// <summary>
        /// The number of adjusted hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearAdjustedHours {
            get {
                var startingTransactionIndex = SortedPriorPlanYearEndIndex + 1; //index of the first transaction in the plan year
                // Take J, C, R records after starting transaction
                var currentPlanYearAdjustedTransactions = SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => (trans.Type == LeaveTransactionType.Adjusted || trans.Type == LeaveTransactionType.MidYearBalanceAdjustment || trans.Type == LeaveTransactionType.Rollover) &&
                        trans.Date >= CurrentPlanYearStartDate);

                // if there is a "B" record - USE NEW METHOD
                if (StartingBalanceTransaction != null)
                {
                    // use logic above
                }
                else if (PriorPlanYearEndTransaction != null)
                {
                    // filter out the transactions that are already included in the starting balance
                    currentPlanYearAdjustedTransactions = currentPlanYearAdjustedTransactions.Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);
                    // gather retro transactions that aren't included in the prior plan year end record already.
                    var retroAdjustedTransactions = SortedLeaveTransactions
                        .Where(trans => trans.Type == LeaveTransactionType.Adjusted || trans.Type == LeaveTransactionType.MidYearBalanceAdjustment || trans.Type == LeaveTransactionType.Rollover)
                        .Where(trans => trans.Date < CurrentPlanYearStartDate)
                        .Where(trans => trans.Id > PriorPlanYearEndTransaction.Id);

                    // add the retro transactions into the current plan year adjusted transactions
                    currentPlanYearAdjustedTransactions = currentPlanYearAdjustedTransactions.Union(retroAdjustedTransactions);
                }
                return currentPlanYearAdjustedTransactions.Sum(trans => trans.TransactionHours);
            }
        }

        /// <summary>
        /// The balance (StartingBalance + Earned - Used + Adjusted) of this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearBalance {
            get {
                return CurrentPlanYearStartingBalance +
                    CurrentPlanYearEarnedHours +
                    CurrentPlanYearUsedHours +
                    CurrentPlanYearAdjustedHours;
            }
        }

        /// <summary>
        /// The Leave Transactions associated to this plan, sorted by AddDate then by TransactionId;
        /// </summary>
        public virtual ReadOnlyCollection<EmployeeLeaveTransaction> SortedLeaveTransactions {
            get {
                // sort leave transactions by date then by id
                List<EmployeeLeaveTransaction> sortedLeaveTransactions = leaveTransactions
                    .OrderBy(slt => slt.Date)
                    .ThenBy(slt => slt.Id)
                    .ToList();
                return sortedLeaveTransactions.AsReadOnly();
            }
        }

        /// <summary>
        /// Indicates whether or not this leave plan has a plan year start date defined in LEAD form. 
        /// </summary>
        public bool IsPlanYearStartDateDefined { get; private set; }

        private List<EmployeeLeaveTransaction> leaveTransactions;

        /// <summary>
        /// Add a leave transaction to this plan in sorted order.
        /// The transaction will only be added if it matches this plan's Id and LeavePlanId.
        /// </summary>
        /// <param name="transaction"></param>
        public void AddLeaveTransaction(EmployeeLeaveTransaction transaction)
        {
            if (canAddTransaction(transaction))
            {
                var index = leaveTransactions.BinarySearch(transaction);
                if (index < 0)
                {
                    leaveTransactions.Insert(~index, transaction);
                }
                leaveTransactions.Sort();
            }
        }

        /// <summary>
        /// Add a range of leave transactions to this plan. Only transactions that match this plan's Id and LeavePlanId will be added.
        /// This method ensure all transactions are in sorted order.
        /// </summary>
        /// <param name="transactions"></param>
        public void AddLeaveTransactionRange(params EmployeeLeaveTransaction[] transactions)
        {
            var filteredTransactions = transactions.Distinct().Where(t => canAddTransaction(t));
            leaveTransactions.AddRange(filteredTransactions);
            leaveTransactions.Sort();
        }

        private bool canAddTransaction(EmployeeLeaveTransaction transaction)
        {
            var canAdd = transaction.EmployeeLeavePlanId == Id && //transaction must be assigned to this employee leave plan
                transaction.LeavePlanDefinitionId == LeavePlanId && //transaction must be assigned to this leave plan definition
                !leaveTransactions.Contains(transaction); //transaction must not already been in the list.
            return canAdd;
        }

        /// <summary>
        /// List of all Leave Transactions associated with a Employee Leave Plan
        /// </summary>
        public IEnumerable<EmployeeLeaveTransaction> EmployeeLeaveTransactions {
            get {
                var startingTransactionIndex = PriorPlanYearEndTransactionIndex + 1; //index of the first transaction in the plan year
                return SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => trans.Date.DateTime >= CurrentPlanYearStartDate);
            }
        }

        /// <summary>
        /// PriorYearEndTrnsaction's forwarding balance hours.
        /// </summary>
        public decimal? PriorYearForwardingBalanceHours { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="employeeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="leavePlanId"></param>
        /// <param name="leavePlanDescription"></param>
        /// <param name="leavePlanStartDate"></param>
        /// <param name="leavePlanEndDate"></param>
        /// <param name="leavePlanTypeCategory"></param>
        /// <param name="earningsTypeId"></param>
        /// <param name="earningsTypeDescription"></param>
        /// <param name="leaveAllowedDate"></param>
        /// <param name="priorPeriodLeaveBalance"></param>
        /// <param name="planYearStartMonth"></param>
        /// <param name="planYearStartDay"></param>
        /// <param name="isLeaveReportingPlan"></param>
        /// <param name="earningTypeIDList"></param>
        /// <param name="accrualRate"></param>
        /// <param name="accrualLimit"></param>
        /// <param name="accrualMaxCarryOver"></param>
        /// <param name="accrualMaxRollOver"></param>
        /// <param name="accrualMethod"></param>
        /// <param name="isPlanYearStartDateDefined"></param>
        /// <param name="perLeaveStartBalUpdt"></param>
        /// <param name="allowNegativeBalance"></param>
        /// <param name="includeLeavePlansWithNoEarningsTypes"></param>
        public EmployeeLeavePlan(string id,
            string employeeId,
            DateTime startDate,
            DateTime? endDate,
            string leavePlanId,
            string leavePlanDescription,
            DateTime leavePlanStartDate,
            DateTime? leavePlanEndDate,
            LeaveTypeCategory leavePlanTypeCategory,
            string earningsTypeId,
            string earningsTypeDescription,
            DateTime leaveAllowedDate,
            decimal priorPeriodLeaveBalance,
            int planYearStartMonth,
            int planYearStartDay,
            bool isLeaveReportingPlan,
            IEnumerable<string> earningTypeIDList,
            decimal? accrualRate,
            decimal? accrualLimit,
            decimal? accrualMaxCarryOver,
            decimal? accrualMaxRollOver,
            string accrualMethod,
            bool isPlanYearStartDateDefined,
            DateTime? latestCarryoverDate,
            bool allowNegativeBalance = false,
            bool includeLeavePlansWithNoEarningsTypes = false
            )
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("Id");
            }

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException("EmployeeId");
            }

            if (string.IsNullOrWhiteSpace(leavePlanId))
            {
                throw new ArgumentNullException("LeavePlanId");
            }

            // SS Leave Balance page doesn't mandate the earnigs type to be associated with a leave plan.
            if (string.IsNullOrWhiteSpace(earningsTypeId) && !includeLeavePlansWithNoEarningsTypes)
            {
                throw new ArgumentNullException("EarningsTypeId");
            }

            if (endDate.HasValue && endDate.Value < startDate)
            {
                throw new ArgumentOutOfRangeException("endDate", "endDate cannot be before startDate");
            }

            if (leavePlanEndDate.HasValue && leavePlanEndDate.Value < leavePlanStartDate)
            {
                throw new ArgumentOutOfRangeException("leavePlanEndDate", "leavePlanEndDate cannot be before leavePlanStartDate");
            }

            if (planYearStartMonth < 1 || planYearStartMonth > 12)
            {
                throw new ArgumentOutOfRangeException("planYearStartMonth", "planYearStartMonth must be between 1 and 12");
            }

            if (planYearStartDay < 1 || planYearStartDay > 31)
            {
                throw new ArgumentOutOfRangeException("planYearStartDay", "playYearStartDay must be between 1 and the end of the month");
            }

            // SS Leave Balance page doesn't mandate the earnigs type to be associated with a leave plan.
            if ((earningTypeIDList == null || !earningTypeIDList.Any()) && !includeLeavePlansWithNoEarningsTypes)
            {
                throw new ArgumentNullException("earningTypeIDList");
            }

            try
            {
                //try to create a dateTime using today's year, the start month and start day
                new DateTime(DateTime.Today.Year, planYearStartMonth, planYearStartDay);
            }
            catch (ArgumentOutOfRangeException aore)
            {
                throw new ArgumentOutOfRangeException("planYearStartDay", aore);
            }

            this.id = id;
            EmployeeId = employeeId;
            StartDate = startDate;
            EndDate = endDate;
            LeavePlanId = leavePlanId;
            LeavePlanDescription = leavePlanDescription;
            LeavePlanStartDate = leavePlanStartDate;
            LeavePlanEndDate = leavePlanEndDate;
            LeavePlanTypeCategory = leavePlanTypeCategory;
            EarningsTypeId = earningsTypeId;
            EarningsTypeDescription = earningsTypeDescription;
            LeaveAllowedDate = leaveAllowedDate;
            PriorPeriodLeaveBalance = priorPeriodLeaveBalance;
            AllowNegativeBalance = allowNegativeBalance;
            PlanYearStartMonth = planYearStartMonth;
            PlanYearStartDay = planYearStartDay;
            EarningTypeIDList = earningTypeIDList;
            IsLeaveReportingPlan = isLeaveReportingPlan;
            leaveTransactions = new List<EmployeeLeaveTransaction>();
            AccrualRate = accrualRate;
            AccrualLimit = accrualLimit;
            AccrualMaxCarryOver = accrualMaxCarryOver;
            AccrualMaxRollOver = accrualMaxRollOver;
            AccrualMethod = accrualMethod;
            IsPlanYearStartDateDefined = isPlanYearStartDateDefined;
            LatestCarryoverDate = latestCarryoverDate;
        }

        /// <summary>
        /// Two leave plans are equal when their ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var employeeLeavePlan = obj as EmployeeLeavePlan;
            return employeeLeavePlan.Id == Id;
        }

        /// <summary>
        /// Hashcode for EmployeeLeavePlan Id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// String representation of the EmployeeLeavePlan
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return LeavePlanTypeCategory.ToString() + "-" + Id + "-" + EmployeeId;
        }
    }
}
