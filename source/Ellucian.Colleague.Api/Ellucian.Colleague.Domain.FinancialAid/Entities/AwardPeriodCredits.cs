/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardPeriodCredits entity contains a list of CourseCreditAssociation objects for a specific student/award year/award period combination
    /// </summary>
    [Serializable]
    public class AwardPeriodCredits
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom these credits belong
        /// </summary>
        public string StudentId { get { return _StudentId; } }
        private readonly string _StudentId;

        /// <summary>
        /// The Award Year these credits belong to
        /// </summary>
        public string AwardYear { get { return _AwardYear; } }
        private readonly string _AwardYear;

        /// <summary>
        /// The Award Period that these credits belong to
        /// </summary>
        public string AwardPeriod { get { return _AwardPeriod; } }
        private readonly string _AwardPeriod;

        /// <summary>
        /// The Award Period description 
        /// </summary>
        public string AwardPeriodDescription { get; set; }

        /// <summary>
        /// The student's program for this award period
        /// </summary>
        public string ProgramName { get; set; }
        /// <summary>
        /// Flag indicating if degree audit is active for this award period
        /// </summary>
        public bool DegreeAuditActive { get; set; }

        /// <summary>
        /// List off CourseCreditAssociation records for each award period
        /// </summary>
        public List<CourseCreditAssociation> Coursework {get;set;}
        

        /// <summary>
        /// Create a new AwardPeriodCredits object
        /// </summary>
        /// <param name="awardYear">Required: The awardYear to which these credits apply</param>
        /// <param name="studentId">Required: The Colleague PERSON id of the student to whom these credits apply</param>
        /// <param name="awardPeriodId">Required: The award period ID to which these credits apply</param>
        /// <exception cref="ArgumentNullException">Thrown if any required arguments are null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the campusBasedOriginalAmount argument is less than zero.</exception>
        public AwardPeriodCredits(
            string studentId,
            string awardYear,
            string awardPeriodId)
        {
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardPeriodId))
            {
                throw new ArgumentNullException("awardPeriodId");
            }

            _AwardYear = awardYear;
            _StudentId = studentId;
            _AwardPeriod = awardPeriodId;

            //Do not need to create an empty list of CourseCreditAssociation objects as we will add them directly after this constructor
        }
    }
}
