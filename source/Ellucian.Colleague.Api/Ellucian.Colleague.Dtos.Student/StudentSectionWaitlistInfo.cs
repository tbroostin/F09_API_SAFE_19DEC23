// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Section waitlist information for the student along with the rank, rating and boolean values to decide on the display of the fields
    /// </summary>
  
    public class StudentSectionWaitlistInfo
    {
        /// <summary>
        /// Holds the data with respect to rank, rating, student id, section id and status date of the student and section
        /// </summary>
        public SectionWaitlistStudent SectionWaitlistStudent { get; set;}


        /// <summary>
        /// Holds the data with respect to the visibility of the rank and the rating fields
        /// </summary>
        public SectionWaitlistConfig SectionWaitlistConfig { get; set; }

    }
}
