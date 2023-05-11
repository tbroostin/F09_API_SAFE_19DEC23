/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AwardYearCredits entity contains a list of AwardPeriodCredits objects for a specific student/award year combination
    /// </summary>
    [Serializable]
    public class AwardYearCredits
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
        /// Flag indicating whether or not there are any credits in this award year
        /// </summary>
        public bool ContainsCourseCredits { get; set; }

        /// <summary>
        /// List off AwardPeriodCredits objects for the student/award year 
        /// </summary>
        public List<AwardPeriodCredits> AwardPeriodCoursework {get;set;}
        

        /// <summary>
        /// Create a new AwardYearCredits object
        /// </summary>
        /// <param name="awardYear">Required: The awardYear to which these credits apply</param>
        /// <param name="studentId">Required: The Colleague PERSON id of the student to whom these credits apply </param>
        /// <exception cref="ArgumentNullException">Thrown if any required arguments are null or empty</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the campusBasedOriginalAmount argument is less than zero.</exception>
        public AwardYearCredits(
            string studentId,
            string awardYear)
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
            //Initialize an empty list of AwardPeriodCredits objects so that we can add to them as necessary after constructing 
            AwardPeriodCoursework = new List<AwardPeriodCredits>();
            ContainsCourseCredits = false;
        }
    }
}
