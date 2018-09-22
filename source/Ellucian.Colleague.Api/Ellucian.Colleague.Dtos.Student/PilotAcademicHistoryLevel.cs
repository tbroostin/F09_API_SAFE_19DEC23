// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student Academic History by Academic Level with only fields required by Pilot
    /// </summary>
    public class PilotAcademicHistoryLevel
    {
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Academic Level code for all credit and totals within this object
        /// </summary>
        public string AcademicLevelCode { get; set; }

        /// <summary>
        /// Total credits earned for the academic level 
        /// </summary>
        public decimal TotalCreditsEarned { get; set; }

        /// <summary>
        /// Overall GPA based on all term and non-term credit, can be null if no credits completed.
        /// </summary>
        public decimal? CumulativeGradePointAverage { get; set; }

        /// <summary>
        /// Overall Transfer GPA based on all term and non-term credit, can be null if no credits completed.
        /// </summary>
        public decimal? TransferGradePointAverage { get; set; }

        /// <summary>
        /// First Term Enrolled at this academic level
        /// </summary>
        public string FirstTermEnrolled { get; set; }
       
        /// <summary>
        /// List of student sections representing the student's registration in sections.
        /// </summary>
        public List<PilotStudentSection> StudentSections {get; set;}
    }
}
