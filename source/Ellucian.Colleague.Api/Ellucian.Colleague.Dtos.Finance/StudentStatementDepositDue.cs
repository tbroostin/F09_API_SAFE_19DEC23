// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Student deposit due for display in the Student Statement
    /// </summary>
    public class StudentStatementDepositDue
    {
        /// <summary>
        /// ID of a deposit due
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date by which the deposit is due
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Balance owed on the deposit due
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// Amount still due on the deposit due (same as Balance)
        /// </summary>
        public decimal AmountDue { get; set; }

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
    }
}
