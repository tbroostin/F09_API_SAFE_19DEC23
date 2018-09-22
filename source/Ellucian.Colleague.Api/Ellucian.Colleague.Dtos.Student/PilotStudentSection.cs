// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Student's registration for a section, as used by Pilot.
    /// </summary>
    public class PilotStudentSection
    {
        /// <summary>
        /// Number of credits the section/course is worth.
        /// </summary>
        public decimal Credits { get; set; }
        /// <summary>
        /// Date the section ends.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Whether the section is being taken for pass/fail credit.
        /// </summary>
        public bool PassFail { get; set; }
        /// <summary>
        /// The name of the course.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The id of the section
        /// </summary>
        public string Section { get; set; }
        /// <summary>
        /// The date the section started.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// The status of the course registration.
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// The date of the status.
        /// </summary>
        public DateTime? StatusDate { get; set; }
        /// <summary>
        /// The student this registration is for.
        /// </summary>
        public string Student { get; set; }
        /// <summary>
        /// The term this section is in.
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// The academic level this section is being taken for.
        /// </summary>
        public string AcademicLevel { get; set; }
        /// <summary>
        /// Location from the student course section.
        /// </summary>
        public string Campus { get; set; }
    }
}
