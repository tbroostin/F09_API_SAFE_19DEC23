// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// Status for adding a course section to a student's list of preferred course sections
    /// </summary>
    [Serializable]
    public enum PortalStudentPreferredCourseSectionUpdateStatus
    {
        /// <summary>
        /// Course section was successfully added to the student's list of preferred course sections
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Course section was not added to the student's list of preferred course sections
        /// </summary>
        Error = 1
    }
}
