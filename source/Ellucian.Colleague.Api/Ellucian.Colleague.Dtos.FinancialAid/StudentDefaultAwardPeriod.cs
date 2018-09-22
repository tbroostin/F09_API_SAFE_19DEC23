//Copyright 2015 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Provides a list of award periods to use for Requesting a Loan. It will contain
    /// either the SA.TERMS from Colleague or terms from an Attendance Pattern determined from the rules. 
    /// </summary>
    public class StudentDefaultAwardPeriod
    {
        /// <summary>
        /// The student Id associated to this record
        /// </summary>
        public string StudentId { get; set; }
        
        /// <summary>
        /// The year for this list of award periods
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// List of award periods for this year
        /// </summary>
        public List<string> DefaultAwardPeriods { get; set; }
    }
}
