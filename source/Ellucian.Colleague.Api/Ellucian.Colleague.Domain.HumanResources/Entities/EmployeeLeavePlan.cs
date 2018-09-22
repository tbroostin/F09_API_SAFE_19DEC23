/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
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
        /// The start date of the current (as of today) plan year.
        /// The start date is built by using the PlanYearStartMonth and PlanYearStartDay and then deriving the year
        /// based on today's date. 
        /// </summary>
        public DateTime CurrentPlanYearStartDate
        {
            get
            {
                //most plan years start January 1 (1/1). start with today's year, and the specified plan month and day.       
                var planYearStartDate = new DateTime(DateTime.Today.Year, PlanYearStartMonth, PlanYearStartDay);

                //if the plan year starts July 1 (7/1), and today is before July 1 (5/1 for instance),
                //the plan year start date is July 1 in the year previous to today's year
                if (DateTime.Today < planYearStartDate)
                {
                    planYearStartDate = planYearStartDate.AddYears(-1);
                }

                return planYearStartDate;
            }
        }

        /// <summary>
        /// The last transaction of the prior plan year (as of today). The ForwardingBalance of this
        /// transaction is the starting balance of the current plan year.
        /// </summary>
        public virtual EmployeeLeaveTransaction PriorPlanYearEndTransaction
        {
            get
            {
                var planYearStartDate = CurrentPlanYearStartDate;
                foreach (var transaction in SortedLeaveTransactions.Reverse())
                {
                    if (transaction.Date > planYearStartDate)
                    {
                        continue;
                    }
                    if (transaction.Date == planYearStartDate && transaction.Type == LeaveTransactionType.Adjusted)
                    {
                        return transaction;
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
        /// The index of PriorPlanYearEndTransaction in SortedLeaveTransactions.
        /// If PriorPlanYearEndTransaction is null, this method will return -1;
        /// </summary>
        public virtual int PriorPlanYearEndTransactionIndex
        {
            get
            {
                var endTransaction = PriorPlanYearEndTransaction;
                if (endTransaction == null)
                {
                    return -1;
                }
                var index = leaveTransactions.BinarySearch(endTransaction);
                if (index < 0)
                {                
                    return -1;//this shouldn't happen
                }
                return index; //index of the prior plan year end transaction 
            }
        }

        /// <summary>
        /// The starting balance for this leave plan of the current plan year (as of today).
        /// </summary>
        public virtual decimal CurrentPlanYearStartingBalance
        {
            get
            {
                var startTransaction = PriorPlanYearEndTransaction;
                if (startTransaction == null)
                {
                    return 0;
                }
                return startTransaction.ForwardingBalance;
            }
        }

        /// <summary>
        /// The number of earned hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearEarnedHours
        {
            get
            {
                var startingTransactionIndex = PriorPlanYearEndTransactionIndex + 1; //index of the first transaction in the plan year
                return SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => trans.Type == LeaveTransactionType.Earned) 
                    .Sum(trans => trans.TransactionHours);
            }
        }

        /// <summary>
        /// The number of used hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearUsedHours
        {
            get
            {
                var startingTransactionIndex = PriorPlanYearEndTransactionIndex + 1; //index of the first transaction in the plan year
                return SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => trans.Type == LeaveTransactionType.Used)
                    .Sum(trans => trans.TransactionHours);
            }
        }

        /// <summary>
        /// The number of adjusted hours for this leave plan in the current plan year (as of today)
        /// </summary>
        public virtual decimal CurrentPlanYearAdjustedHours
        {
            get
            {
                var startingTransactionIndex = PriorPlanYearEndTransactionIndex + 1; //index of the first transaction in the plan year
                return SortedLeaveTransactions
                    .Skip(startingTransactionIndex)
                    .Where(trans => trans.Type == LeaveTransactionType.Adjusted)
                    .Sum(trans => trans.TransactionHours);
            }
        }

        /// <summary>
        /// The balance (StartingBalance + Earned - Used + Adjusted) of this leave plan in the current plan year (as of today)
        /// </summary>
        public decimal CurrentPlanYearBalance
        {
            get
            {
                return CurrentPlanYearStartingBalance +
                    CurrentPlanYearEarnedHours + 
                    CurrentPlanYearUsedHours + 
                    CurrentPlanYearAdjustedHours;
            }
        }

        /// <summary>
        /// The Leave Transactions associated to this plan, sorted by AddDate then by TransactionId;
        /// </summary>
        public virtual ReadOnlyCollection<EmployeeLeaveTransaction> SortedLeaveTransactions
        {
            get
            {
                return leaveTransactions.AsReadOnly();
            }
        }

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
        /// <param name="allowNegativeBalance"></param>
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
            bool allowNegativeBalance = false)
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

            if (string.IsNullOrWhiteSpace(earningsTypeId))
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
            
            leaveTransactions = new List<EmployeeLeaveTransaction>();


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
