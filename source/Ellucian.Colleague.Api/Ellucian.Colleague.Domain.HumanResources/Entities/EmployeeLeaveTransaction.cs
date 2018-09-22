/* Copyright 2018 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Represents an amount of leave that an employee earned or used (or an adjustment made by an HR administrator)
    /// on a specific date
    /// </summary>
    [Serializable]
    public class EmployeeLeaveTransaction : IComparable<EmployeeLeaveTransaction>, IComparer<EmployeeLeaveTransaction>, IEqualityComparer<EmployeeLeaveTransaction>
    {
        /// <summary>
        /// The database Id of the transaction
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The Id of the LeavePlan Definition
        /// </summary>
        public string LeavePlanDefinitionId { get; private set; }

        /// <summary>
        /// The Id of the EmployeeLeavePlan
        /// </summary>
        public string EmployeeLeavePlanId { get; private set; }

        /// <summary>
        /// The amount of leave to be added or subtracted from the employee leave plan balance.
        /// The amount entered may be either positive or negative. If positive, it is added to the total; if negative, it is subtracted from the total.
        /// If the Type is Earned the amount is expected to be positive, but negative amounts are accepted.
        /// If the Type is Used the amount is expected to be negative, but positive amounts are accepted.
        /// If the Type is Adjusted, the amount may be either positive or negative.
        /// </summary>
        public decimal TransactionHours { get; private set; }

        /// <summary>
        /// The date the transaction occurred, or will occur.
        /// </summary>
        public DateTimeOffset Date { get; private set; }

        /// <summary>
        /// The type of transation, Earned, Used, Adjusted. 
        /// </summary>
        public LeaveTransactionType Type { get; private set; }

        /// <summary>
        /// The balance of the employee leave plan after this transaction is applied - forwarding to the next transaction
        /// </summary>
        public decimal ForwardingBalance { get; private set; }

        /// <summary>
        /// Create a new EmployeeLeaveTransaction
        /// </summary>
        public EmployeeLeaveTransaction(int id,
            string leavePlanDefinitionId,
            string employeeLeavePlanId,
            decimal transactionHours,
            DateTimeOffset date,
            LeaveTransactionType type,
            decimal forwardingBalance)
        {
            
            if (string.IsNullOrWhiteSpace(leavePlanDefinitionId))
            {
                throw new ArgumentNullException("leavePlanDefinitionId");
            }
            if (string.IsNullOrWhiteSpace(employeeLeavePlanId))
            {
                throw new ArgumentNullException("employeeLeavePlanId");
            }

            Id = id;
            LeavePlanDefinitionId = leavePlanDefinitionId;
            EmployeeLeavePlanId = employeeLeavePlanId;
            TransactionHours = transactionHours;
            Date = date;
            Type = type;
            ForwardingBalance = forwardingBalance;

        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var transaction = obj as EmployeeLeaveTransaction;

            return this.Id == transaction.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", Id, LeavePlanDefinitionId, TransactionHours, Date.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns>A value that indicates the relative order of EmployeeLeaveTransaction objects. 
        /// Less than 0: this object is less than other.
        /// 0: this object is equal to the other.
        /// Greater than 0: this object greater than other
        /// </returns>
        public int CompareTo(EmployeeLeaveTransaction other)
        {
            return Id.CompareTo(other.Id);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>A value that indicates the relative order of EmployeeLeaveTransaction objects. 
        /// Less than 0: x object is less than y.
        /// 0: x object is equal to the y.
        /// Greater than 0: x object greater than y</returns>
        public int Compare(EmployeeLeaveTransaction x, EmployeeLeaveTransaction y)
        {
            return x.CompareTo(y);
        }

        /*IEqualityComparer implementation*/
        public bool Equals(EmployeeLeaveTransaction x, EmployeeLeaveTransaction y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(EmployeeLeaveTransaction obj)
        {
            return obj.GetHashCode();
        }
    }
}
