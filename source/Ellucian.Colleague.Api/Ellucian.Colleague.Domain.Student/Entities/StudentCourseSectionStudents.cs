// Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This is a student roster by section key and student key.
    /// </summary>
    [Serializable]
    public class StudentCourseSectionStudents
    {
        /// <summary>
        /// Course Section key for building rosters
        /// </summary>
        public string CourseSectionIds { get; set; }
        /// <summary>
        /// Student Key for building rosters
        /// </summary>
        public string StudentIds { get; set; }
    }
}
