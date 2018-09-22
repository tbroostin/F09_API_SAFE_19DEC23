//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A list of records showing the total loans disbursed to a student 
    /// from a specific school. This is retrieved from NSLDS imported records.
    /// This exists as a subset of the StudentLoanSummary object.
    /// </summary>
    public class StudentLoanHistory
    {
        /// <summary>
        /// The school's OPE (Office of Postsecondary Education) Id, where the student borrowed money
        /// </summary>
        public string OpeId { get; set; }

        /// <summary>
        /// The total amount of loans from the school
        /// </summary>
        public int TotalLoanAmount { get; set; }
    }
}
