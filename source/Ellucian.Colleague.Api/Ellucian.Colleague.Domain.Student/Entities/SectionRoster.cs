// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Collection of registered student IDs for a course section
    /// </summary>
    [Serializable]
    public class SectionRoster
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

        private List<string> _facultyIds = new List<string>();
        /// <summary>
        /// IDs for faculty assigned to the course section
        /// </summary>
        public ReadOnlyCollection<string> FacultyIds { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SectionRoster"/> class
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        public SectionRoster(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section roster without a course section ID.");
            }

            _sectionId = sectionId;
            StudentIds = _studentIds.AsReadOnly();
            FacultyIds = _facultyIds.AsReadOnly();
        }

        /// <summary>
        /// Add a student ID to the <see cref="SectionRoster"/>
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

        /// <summary>
        /// Add a faculty ID to the <see cref="SectionRoster"/>
        /// </summary>
        /// <param name="facultyId">Faculty ID to add</param>
        public void AddFacultyId(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "A faculty ID must be specified.");
            }
            if (!FacultyIds.Contains(facultyId))
            {
                _facultyIds.Add(facultyId);
            }
        }
    }
}
