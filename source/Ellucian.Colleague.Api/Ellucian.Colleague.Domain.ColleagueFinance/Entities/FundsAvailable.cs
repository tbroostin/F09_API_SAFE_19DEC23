// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Accounting String is used to search for an account number + project number and by valid on date
    /// </summary>
    [Serializable]
    public class FundsAvailable
    {
        /// <summary>
        /// Account String
        /// </summary>
        public string AccountString { get; private set; }

        /// <summary>
        /// Project Number
        /// </summary>
        public string ProjectNumber { get; set; }

        public DateTime? BalanceOn { get; set; }

        /// <summary>
        /// Amount to be aded to the expenses for total expenses calculations
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Total budget
        /// </summary>
        public decimal TotalBudget { get; set; }

        /// <summary>
        /// Total expenses
        /// </summary>
        public decimal TotalExpenses { get; set; }

        /// <summary>
        /// All project budget values
        /// </summary>
        public List<decimal?> ProjectBudgets { get; set; }

        /// <summary>
        /// All project actual memo values
        /// </summary>
        public List<decimal?> ProjectActualMemos { get; set; }

        /// <summary>
        /// All project actual posted values
        /// </summary>
        public List<decimal?> ProjectActualPosted { get; set; }

        /// <summary>
        /// All project encumbrance memo values
        /// </summary>
        public List<decimal?> ProjectEncumbranceMemos { get; set; }

        /// <summary>
        /// All project encumbrance posted values
        /// </summary>
        public List<decimal?> ProjectEncumbrancePosted { get; set; }

        /// <summary>
        /// All project requisition memo values
        /// </summary>
        public List<decimal?> ProjectRequisitionMemos { get; set; }

        /// <summary>
        /// All project period start dates
        /// </summary>
        public List<DateTime?> ProjectStartDates { get; set; }

        /// <summary>
        /// All project period end dates
        /// </summary>
        public List<DateTime?> ProjectEndDates { get; set; }

        /// <summary>
        /// The projects current status.
        /// </summary>
        public string ProjectStatus { get; set; }
        
        /// <summary>
        ///  Item ID
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Available Status
        /// </summary>
        public FundsAvailableStatus AvailableStatus { get; set; }

        /// <summary>
        /// Submitted By
        /// </summary>
        public string SubmittedBy { get; set; }

        /// <summary>
        /// Transaction Date
        /// </summary>
        public DateTimeOffset? TransactionDate { get; set; }
        public string CurrencyCode { get; set; }
        public string Sequence { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="accountString"></param>
        /// <param name="amount"></param>
        /// <param name="projectNumber"></param>
        /// <param name="balanceOn"></param>
        /// <param name="submittedByValue"></param>
        public FundsAvailable(string accountString)
        {
            if (string.IsNullOrEmpty(accountString))
            {
                throw new ArgumentNullException("accountString", "Accouting String is a required field.");
            }            
            AccountString = accountString;
        }
    }
}
