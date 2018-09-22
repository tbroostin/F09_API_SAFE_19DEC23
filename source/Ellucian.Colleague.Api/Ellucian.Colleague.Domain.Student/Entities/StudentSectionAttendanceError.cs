// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Attendance for a specific class in a specific class meeting of a section
    /// </summary>
    [Serializable]
    public class StudentSectionAttendanceError
    {
        private string _StudentCourseSectionId;
        /// <summary>
        /// Id of the student course section record this attendance is associated with
        /// </summary>
        public string StudentCourseSectionId { get { return _StudentCourseSectionId; } }

        /// <summary>
        /// Indicates the reason the associated student section attendance failed update. 
        /// </summary>
        public string ErrorMessage { get; set; }

       
        public StudentSectionAttendanceError(string studentCourseSectionId )
        {
            if (string.IsNullOrEmpty(studentCourseSectionId))
            {
                throw new ArgumentNullException("studentCourseSectionId", "A studentCourseSectionId is required for a student section attendance error");
            }
            _StudentCourseSectionId = studentCourseSectionId;
        }
        
    }
}
