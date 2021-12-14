// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student.Requirements;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a course in the curriculum, e.g. MATH-101
    /// </summary>
    public class Course2
    {
        /// <summary>
        /// Unique system Id of the course
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Subject code for the course (MATH)
        /// </summary>
        public string SubjectCode { get; set; }

        /// <summary>
        /// Number of the course (101)
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// (Optional) Minimum credits for the course
        /// </summary>
        public decimal? MinimumCredits { get; set; }

        /// <summary>
        /// (Optional) Maximum credits for the course
        /// </summary>
        public decimal? MaximumCredits { get; set; }

        /// <summary>
        /// When there is a minimum and a maximum number of credits, an increment may also be provided to limit valid
        /// credits the course can carry between the minimum and maximum. If Minimum is 1 and maximum is 7 and increment
        /// is 2 then the course can be taken for 1, 3, 5, or 7 credits only.
        /// </summary>
        public decimal? VariableCreditIncrement { get; set; }

        /// <summary>
        /// Continuing Education Units for the course
        /// </summary>
        public decimal? Ceus { get; set; }

        /// <summary>
        /// Title of the course
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// (optional) Text description of the course
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// (optional) Code indicating the term sesion cycle in which this course is typically offered. If not provided and, if there are no cycle restrictions by location, then the course is offered anytime.
        /// </summary>
        public string TermSessionCycle { get; set; }

        /// <summary>
        /// (optional) Code indicating the term yearly cycle in which this course is typically offered. If not provided and, if there are no cycle restrictions by location, then the course is offered anytime.
        /// </summary>
        public string TermYearlyCycle { get; set; }

        /// <summary>
        /// (Optional) Used when the course is not offered every year to describe which years it is offered (every other year).
        /// </summary>
        public string YearsOffered { get; set; }

        /// <summary>
        /// (optional) Free form text description indicating the terms in which this course is offered (Spring only).
        /// </summary>
        public string TermsOffered { get; set; }

        /// <summary>
        /// Codes of the locations where this course is normally offered. 
        /// </summary>
        public IEnumerable<string> LocationCodes { get; set; }

        /// <summary>
        /// Indicates that this course is a pseudo course.
        /// </summary>
        public bool IsPseudoCourse { get; set; }

        /// <summary>
        /// List of Requisites associated to this course and will include requisites the must be taken concurrently as well as previously.
        /// </summary>
        public IEnumerable<Requisite> Requisites { get; set; }

        /// <summary>
        /// List of Ids of courses that are equated to this course.
        /// </summary>
        public IEnumerable<string> EquatedCourseIds { get; set; }

        /// <summary>
        /// Session and yearly cycle restrictions by location for the course.
        /// </summary>
        public IEnumerable<LocationCycleRestriction> LocationCycleRestrictions { get; set; }

        /// <summary>
        /// Immediately verify grades
        /// </summary>
        public bool? VerifyGrades { get; set; }

        /// <summary>
        /// Indicates whether the drop roster should be displayed for the course
        /// </summary>
        public bool ShowDropRoster { get; set; }
    }
}
