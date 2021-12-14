//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A StudentLoanSummary object contains lifetime summary data about a student's loans.
    /// </summary>
    public class StudentLoanSummary
    {
        /// <summary>
        /// The Student's Colleague PERSON id
        /// 
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Entrance Interview date for a Subsidized or Unsubsidized Stafford Loan
        /// If null, the student has not completed an entrance interview for Subsidized or Unsubsidized Stafford Loans
        /// </summary>
        public DateTime? DirectLoanEntranceInterviewDate { get; set; }

        /// <summary>
        /// Entrance interview date for a Graduate PLUS Loan
        /// If null, the student has not completed an entrance interview for Subsidized or Graduate PLUS Loans
        /// </summary>
        public DateTime? GraduatePlusLoanEntranceInterviewDate { get; set; }

        /// <summary>
        /// Expiration date of the Master Promissory Note (MPN) for a Subsidized or Unsubsidized loan
        /// If null, the student has not completed an MPN for Subsidized or Unsubsidized loans.
        /// </summary>
        public DateTime? DirectLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Expiration date of the Master Promissory Note (MPN) for a Parent PLUS or Graduate PLUS loan
        /// If null, the student has not completed an MPN for Parent PLUS or Graduate PLUS loans
        /// </summary>
        public DateTime? PlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Placeholder
        /// </summary>
        public DateTime? GraduatePlusLoanMpnExpirationDate { get; set; }

        /// <summary>
        /// Aggregate amount of all student loan debt - this amount can be higher than the sum of all 
        /// StudentLoanHistory amounts
        /// </summary>
        public int StudentLoanCombinedTotalAmount { get; set; }

        /// <summary>
        /// Flag that denotes whether the Informed Borrower checklist item is completed.
        /// </summary>
        public List<InformedBorrowerItem> InformedBorrowerItem { get; set; }

        /// <summary>
        /// List of PLUS Loan Item objects
        /// </summary>
        public List<PlusLoanItem> PlusLoanItems { get; set; }

        /// <summary>
        /// Placeholder
        /// </summary>
        public List<PlusApplicationItem> PlusApplicationItems { get; set; }

        /// <summary>
        /// placeholder
        /// </summary>
        public List<PlusMpnItem> PlusMpnItems { get; set; }

        /// <summary>
        /// A list of loan totals from other schools for this student.
        /// If empty, a student has no loan history
        /// </summary>
        public List<StudentLoanHistory> StudentLoanHistory { get; set; }
    }
}