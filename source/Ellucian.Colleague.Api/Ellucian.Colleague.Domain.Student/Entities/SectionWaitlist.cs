// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Collection of waitlisted student IDs for a course section
    /// </summary>
    [Serializable]
    public class SectionWaitlist
    {
        private string _sectionId;

        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get { return _sectionId; } }

        private List<string> _studentIds = new List<string>();

        /// <summary>
        /// IDs for students registered in the course section
        /// </summary>
        public ReadOnlyCollection<string> StudentIds { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SectionWaitlist"/> class
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        public SectionWaitlist(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a course section ID.");
            }

            _sectionId = sectionId;
            StudentIds = _studentIds.AsReadOnly();
        }

        /// <summary>
        /// Add a student ID to the <see cref="SectionWaitlist"/>
        /// </summary>
        /// <param name="studentId">Student ID to add</param>
        public void AddStudentId(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID must be specified.");
            }
            if (!StudentIds.Contains(studentId))
            {
                _studentIds.Add(studentId);
            }
        }
    }
}
