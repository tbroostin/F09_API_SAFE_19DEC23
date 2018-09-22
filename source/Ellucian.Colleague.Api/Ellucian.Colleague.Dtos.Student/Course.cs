using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a course in the curriculum, e.g. MATH-101
    /// </summary>
    public class Course
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
        /// (optional) Free form text description indicating the terms in which this course is offered (Spring only).
        /// </summary>
        public string TermsOffered { get; set; }

        /// <summary>
        /// (Optional) Used when the course is not offered every year to describe which years it is offered (every other year).
        /// </summary>
        public string YearsOffered { get; set; }

        /// <summary>
        /// Codes of the locations where this course is normally offered. 
        /// </summary>
        public IEnumerable<string> LocationCodes { get; set; }

        /// <summary>
        /// <see cref="Corequisite">Corequisite</see> contains a course Id and an indicator whether this course corequisite is required or not.
        /// </summary>
        public IEnumerable<Corequisite> Corequisites { get; set; }

        /// <summary>
        /// Id for the requirement which specifies prerequisites for this course.
        /// </summary>
        public string Prerequisites { get; set; }

        /// <summary>
        /// Indicates that this course is a pseudo course.
        /// </summary>
        public bool IsPseudoCourse { get; set; }
        //
        // Added fields for Student Success Project (SRM)
        //
        /// <summary>
        /// Federal Course Classification code
        /// </summary>
        public string FederalCourseClassification { get; set; }
        /// <summary>
        /// List of Local Government Course Classification codes
        /// </summary>
        public IEnumerable<string> LocalCourseClassifications { get; set; }
        /// <summary>
        /// Status of the course (Active, Inactive, etc.)
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Course Name, such as MATH-100
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of Department Codes for this course
        /// </summary>
        public IEnumerable<string> DepartmentCodes { get; set; }
        /// <summary>
        /// The Academic Level code for this course
        /// </summary>
        public string AcademicLevelCode { get; set; }
        /// <summary>
        /// List of Course Level Codes for this course
        /// </summary>
        public IEnumerable<string> CourseLevelCodes { get; set; }
        /// <summary>
        /// List of Course Type codes for this course
        /// </summary>
        public IEnumerable<string> CourseTypeCodes { get; set; }
        /// <summary>
        /// List of Instructional Method codes for this course
        /// </summary>
        public IEnumerable<string> InstructionalMethodCodes { get; set; }
    }
}
