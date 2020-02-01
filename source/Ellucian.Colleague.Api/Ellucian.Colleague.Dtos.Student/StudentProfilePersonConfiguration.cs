// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Profile persona configuration
    /// </summary>
    public class StudentProfilePersonConfiguration
    {
        /// <summary>
        ///  Show academic programs
        /// </summary>
        public bool ShowAcadamicPrograms { get; set; }

        /// <summary>
        /// Show Phone number
        /// </summary>
        public bool ShowPhone { get; set; }

        /// <summary>
        /// Show Student Address
        /// </summary>
        public bool ShowAddress { get; set; }

        /// <summary>
        /// Show Student Anticipated Completion date
        /// </summary>
        public bool ShowAnticipatedCompletionDate { get; set; }

        /// <summary>
        /// Show Student Acadamic Level Standing
        /// </summary>
        public bool ShowAcadLevelStanding { get; set; }

        /// <summary>
        /// Show Advisor Details
        /// </summary>
        public bool ShowAdvisorDetails { get; set; }

        /// <summary>
        /// Show Advisor Office Hours
        /// </summary>
        public bool ShowAdvisorOfficeHours { get; set; }
    }
}
