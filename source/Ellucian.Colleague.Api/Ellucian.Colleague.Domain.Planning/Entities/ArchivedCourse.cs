// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    [Serializable]
    public class ArchivedCourse
    {
        private string _CourseId;
        /// <summary>
        /// The course ID
        /// </summary>
        public string CourseId { get { return _CourseId; } }
        /// <summary>
        /// Term of the course or section, if any
        /// </summary>
        public string TermCode { get; set; }
        /// <summary>
        /// Id of the section, if planned or registered
        /// </summary>
        public string SectionId { get; set; }
        /// <summary>
        /// Number of Credits for the course or section
        /// </summary>
        public decimal? Credits { get; set; }
        /// <summary>
        /// Number of CEUs for the course or section
        /// </summary>
        public decimal? ContinuingEducationUnits { get; set; }
        /// <summary>
        /// Name of the course or section at time of archive
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Title of the course or section at time of archive
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// String conversion of course approval status
        /// </summary>
        public string ApprovalStatus { get; set; }
        /// <summary>
        /// ID of person who approved the course
        /// </summary>
        public string ApprovedBy { get; set; }
        /// <summary>
        /// Date/time course approved
        /// </summary>
        public DateTimeOffset? ApprovalDate { get; set; }
        /// <summary>
        /// Boolean indicating whether this course was on the course plan
        /// </summary>
        public bool IsPlanned { get; set; }
        /// <summary>
        /// Registration status of this course/section, based on client's local academic credit values
        /// </summary>
        public string RegistrationStatus { get; set; }
        /// <summary>
        /// Boolean indicating whether this course was registered
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                return !string.IsNullOrEmpty(RegistrationStatus);
            }
        }
        /// <summary>
        /// Indicator, from academic credit, whether the grade is a withdraw grade
        /// </summary>
        public bool HasWithdrawGrade { get; set; }
        /// <summary>
        /// Id of the person who added this course to the plan
        /// </summary> 
        public string AddedBy { get; set; }
        /// <summary>
        /// Date/time course was added to the plan
        /// </summary>
        public DateTimeOffset? AddedOn { get; set; }

        public ArchivedCourse(string courseId)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId", "Archive Course must have a Course Id.");
            }
            _CourseId = courseId;
            IsPlanned = false;
        }
    }
}
