// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// A deposit due
    /// </summary>
    public class DepositDue
    {
        /// <summary>
        /// ID of a deposit due
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the student who owes the deposit due
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Amount of the deposit due
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Type of deposit that is due
        /// </summary>
        public string DepositType { get; set; }

        /// <summary>
        /// Date by which the deposit is due
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// (Optional) Term for which the deposit is due
        /// </summary>
        public string TermId { get; set; }

        /// <summary>
        /// List of <see cref="Deposit">Deposits</see> paid against this deposit due
        /// </summary>
        public IEnumerable<Deposit> Deposits { get; set; }

        /// <summary>
        /// Distribution against which the deposit due is paid
        /// </summary>
        public string Distribution { get; set; }

        /// <summary>
        /// Balance owed on the deposit due
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Indicates whether the deposit due is overdue
        /// </summary>
        public bool Overdue { get; set; }

        /// <summary>
        /// Amount still due on the deposit due (same as Balance)
        /// </summary>
        public decimal AmountDue
        { 
            get { return Balance; }
            set { Balance = value; }
        }

        /// <summary>
        /// Amount paid towards the deposit due
        /// </summary>
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// The description of the type of deposit due.
        /// </summary>
        public string DepositTypeDescription { get; set; }

        /// <summary>
        /// The description of the term to which a deposit made would apply.
        /// </summary>
        public string TermDescription { get; set; }

        /// <summary>
        /// Get the sort order for a deposit due:
        /// By due date, then by deposit type, then by ID
        /// </summary>
        public string SortOrder { get; set; }
    }
}
