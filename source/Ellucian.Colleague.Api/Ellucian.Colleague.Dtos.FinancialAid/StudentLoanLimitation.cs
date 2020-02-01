/*Copyright 2014-2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// A StudentLoanLimitation describes the parameters within which a student can request changes to their loans.
    /// Clients should check the StudentAwards against these limitations before using the PUT StudentAward or PUT StudentAwardPackage.
    /// Those endpoints will throw exceptions if the requested changes are beyond the limits set in this object.
    /// </summary>
    public class StudentLoanLimitation
    {
        /// <summary>
        /// The AwardYear that these loan limits apply to.
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// The StudentId that these loan limits apply to
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The maximum Subsidized loan amount that the student is eligible to borrow. The sum
        /// of all subsidized loan awards must be less than or equal to this amount.
        /// </summary>
        public int SubsidizedMaximumAmount { get; set; }

        /// <summary>
        /// The maximum Unsubsidized loan amount that the student is eligible to borrow. The sum
        /// of all unsubsidized loan awards must be less than or equal to this amount.
        /// </summary>
        public int UnsubsidizedMaximumAmount { get; set; }

        /// <summary>
        /// The maximum GradPLUS loan amount that the student is eligible to borrow. It is calculated
        /// based on the unmet need amount. The sum of all graduate PLUS loans must be less than or equal to this amount.
        /// </summary>
        public int GradPlusMaximumAmount { get; set; }

        /// <summary>
        /// Flag indicating whether the loan max amounts should be suppressed for the year on the student level
        /// </summary>
        public bool SuppressStudentMaximumAmounts { get; set; }
    }
}
