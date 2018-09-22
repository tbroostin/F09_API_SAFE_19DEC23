//Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// The student loan limitation class defines the maximum and minimum limits
    /// for a student's loans for a specific year. If a student requests a change
    /// to their loan amounts. That change must be checked against these limits.
    /// </summary>
    [Serializable]
    public class StudentLoanLimitation
    {
        private readonly string _AwardYear;
        private readonly string _StudentId;

        /// <summary>
        /// The AwardYear that these limits apply to
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }

        /// <summary>
        /// The student id that these limits apply to.
        /// </summary>
        public string StudentId { get { return _StudentId; } }

        /// <summary>
        /// The maximum Subsidized loan amount that the student is eligible to borrow. The sum
        /// of all subsidized loan awards must be less than this amount.
        /// </summary>
        public int SubsidizedMaximumAmount { get; set; }

        /// <summary>
        /// The maximum Unsubsidized loan amount that the student is eligible to borrow. The sum
        /// of all unsubsidized loan awards must be less than this amount.
        /// </summary>
        public int UnsubsidizedMaximumAmount { get; set; }

        /// <summary>
        /// The maximum GraduatePlus loan amount that the student is eligible to borrow. It is calculated
        /// based on the unmet need amount
        /// </summary>
        public int GradPlusMaximumAmount { get; set; }

        /// <summary>
        /// Constructor for StudentLoanLimitations requires the award year and student id.
        /// The limits are initialized to $0.
        /// </summary>
        /// <param name="awardYear">The year these limits apply to</param>
        /// <param name="studentId">The student these limits apply to</param>
        /// <exception cref="ArgumentNullException">Thrown when the awardYear or studentId arguments are null or empty</exception>
        public StudentLoanLimitation(string awardYear, string studentId)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            _AwardYear = awardYear;
            _StudentId = studentId;

            SubsidizedMaximumAmount = 0;
            UnsubsidizedMaximumAmount = 0;

        }
    }
}
