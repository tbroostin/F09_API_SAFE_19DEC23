// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Grade with only fields required by Pilot
    /// </summary>
    public class PilotGrade
    {
        /// <summary>
        /// Reference back to the student record necessary when processing several students
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Section this grade relates to.
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Grade position
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Letter grade
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Final or Midterm
        /// </summary>
        public string GradeType { get; set; }

        /// <summary>
        /// Grade priority
        /// </summary>
        public decimal? GradeValue { get; set; }
    }
}
