/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// AwardYearCredits object contains a list of AwardPeriodCredits objects for a specific student/award year combination
    /// </summary>
    public class AwardYearCredits
    {
        /// <summary>
        /// Colleague PERSON id of the student to whom these credits belong
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// The Award Year these credits belong to
        /// </summary>
        public string AwardYear { get; set; }

        /// <summary>
        /// Flag indicating whether or not there are any credits in this award year
        /// </summary>
        public bool ContainsCourseCredits { get; set; }

        /// <summary>
        /// List off AwardPeriodCredits objects for the student/award year 
        /// </summary>
        public System.Collections.Generic.List<AwardPeriodCredits> AwardPeriodCoursework { get; set; }
    }
}
