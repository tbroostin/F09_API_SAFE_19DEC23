// Copyright 2019 Ellucian Company L.P. and its affiliates.


namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Config setting for the display of Rank and Rating of a student for the waitlisted section.
    /// This DTO is considered generic and used for both Faculty and student.
    /// </summary>
    public class SectionWaitlistConfig
    {
        /// <summary>
        /// Boolean value to set the display of Rank
        /// </summary>
        public bool ShowRank { get; set; }

        /// <summary>
        /// Boolean value to set the display of Rating
        /// </summary>
        public bool ShowRating { get; set; }

        /// <summary>
        /// Holds the section waitlist enroll no of days
        /// </summary>
        public int? NoOfDaysToEnroll { get; set; }
    }
}

