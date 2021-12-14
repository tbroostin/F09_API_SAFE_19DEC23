// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.DegreePlans
{
    /// <summary>
    /// A PlannedCourse is a unit that groups a course with it optionally scheduled section in the 
    /// dictionary TermCourses above
    /// </summary>
    [Serializable]
    public class PlannedCourse
    {
        private string _CourseId;
        private string _SectionId;
        private GradingType _GradingType;
        private WaitlistStatus _WaitlistedStatus;
        private string _AddedBy;
        private DateTimeOffset? _AddedOn;
        private string _CoursePlaceholderId;

        #region Constructor

        /// <summary>
        /// Planned course constructor
        /// </summary>
        /// <param name="course">ID of course being planned. Required when CoursePlaceholder is empty</param>
        /// <param name="section">ID of section being planned. Optional</param>
        /// <param name="gradingType">Grading type, defaults to Graded</param>
        /// <param name="status">Waitlist status, defaults to Not Waitlisted</param>
        /// <param name="addedBy">Added by, defaults to null</param>
        /// <param name="addedOn">Added on, defaults to null</param>
        /// <param name="coursePlaceholder">ID of course placeholder being planned. Required when Course is empty</param>
        public PlannedCourse(string course, string section = null, GradingType gradingType = GradingType.Graded,
            WaitlistStatus status = WaitlistStatus.NotWaitlisted,
            string addedBy = null, DateTimeOffset? addedOn = null, string coursePlaceholder = null)
        {
            if (string.IsNullOrEmpty(course) && string.IsNullOrEmpty(coursePlaceholder))
            {
                throw new ArgumentNullException("course", "Planned Course requires either a CourseId or a CoursePlaceholderId");
            }

            _CourseId = course;
            _SectionId = section;
            _GradingType = gradingType;
            _WaitlistedStatus = status;
            _AddedBy = addedBy;
            _AddedOn = addedOn;
            _CoursePlaceholderId = coursePlaceholder;
        }

        #endregion

        #region Required properties

        /// <summary>
        /// The course ID
        /// </summary>
        public string CourseId { get { return _CourseId; } }

        /// <summary>
        /// The section ID
        /// </summary>
        public string SectionId { get { return _SectionId; } }

        /// <summary>
        /// Type of the grading
        /// </summary>
        public GradingType GradingType { get { return _GradingType; } }

        /// <summary>
        /// The waitlisted status
        /// </summary>
        public WaitlistStatus WaitlistedStatus { get { return _WaitlistedStatus; } }

        /// <summary>
        /// System ID of the user who added this course to the plan.  Private because it should not be updateable.
        /// </summary>
        public string AddedBy { get { return _AddedBy; } }

        /// <summary>
        /// Timestamp to track when this course was added to the plan.  Private because it should not be updateable.
        /// </summary>
        public DateTimeOffset? AddedOn { get { return _AddedOn; } }

        /// <summary>
        /// The course placholder ID
        /// </summary>
        public string CoursePlaceholderId { get { return _CoursePlaceholderId; } }
        #endregion

        #region Optional Properties

        /// <summary>
        /// A nullable boolean indicating whether this course is protected from change/deletion by the student. Only and advisor with adequate
        /// permissions can update/change a protected course or add/remove the protected flag.
        /// A null value is the same as a false but we are allowing nulls to manage older versions of the solution and 
        /// different ways the data is stored in the database.
        /// </summary>
        public bool? IsProtected { get; set; }


        // DegreePlanService does validation of this data against the section credit ranges.
        /// <summary>
        /// Gets or sets the credits.
        /// </summary>
        public decimal? Credits { get; set; }

        private List<PlannedCourseWarning> _Warnings = new List<PlannedCourseWarning>();
        /// <summary>
        /// Validation warnings for this course/section
        /// </summary>
        public List<PlannedCourseWarning> Warnings
        {
            get { return _Warnings; }
        }
        #endregion

        /// <summary>
        /// Clear all warnings from this planned course
        /// </summary>
        public void ClearWarnings()
        {
            _Warnings = new List<PlannedCourseWarning>();
        }

        /// <summary>
        /// Add a warning to the enumerable list of warnings for this planned course
        /// </summary>
        /// <param name="warning"></param>
        public void AddWarning(PlannedCourseWarning warning)
        {
            if (warning != null)
            {
                var warnings = _Warnings.ToList();
                warnings.Add(warning);
                _Warnings = warnings;
            }
        }

        /// <summary>
        /// PlannedCourses are equal if their course ids and section ids match, or if their course ids match and section ids are both null or
        /// if their course ids and section ids are both null and course placeholder ids match.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PlannedCourse other = obj as PlannedCourse;
            if (other == null)
            {
                return false;
            }

            return other.CourseId == this.CourseId && 
                other.SectionId == this.SectionId &&
                other.CoursePlaceholderId == this.CoursePlaceholderId &&
                other.GradingType == this.GradingType &&
                other.Credits == this.Credits;
        }

        public override int GetHashCode()
        {
            return (CourseId + SectionId + CoursePlaceholderId).GetHashCode();
        }

        public override string ToString()
        {
            string sect = " SectionId: " + (SectionId ?? "null");
            string placeholder = " CoursePlaceholderId: " + (CoursePlaceholderId ?? "null");
            return "Planned Course: CourseId: " + CourseId + sect + placeholder;
        }
    }
}
