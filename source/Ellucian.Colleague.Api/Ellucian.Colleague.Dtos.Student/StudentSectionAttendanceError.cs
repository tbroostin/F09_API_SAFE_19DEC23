// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Indicates the student section attendance item that failed on update and why.
    /// </summary>
    public class StudentSectionAttendanceError
    {
        /// <summary>
        /// Student Section Attendance that produced the error (required)
        /// </summary>
        public string StudentCourseSectionId { get; set; }

        /// <summary>
        /// The Error Message produced
        /// </summary>
        public string ErrorMessage { get; set; }




    }
}
