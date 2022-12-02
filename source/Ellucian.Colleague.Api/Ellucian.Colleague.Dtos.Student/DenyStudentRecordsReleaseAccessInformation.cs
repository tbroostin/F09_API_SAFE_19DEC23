﻿// Copyright 2022 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Deny access to student records release information
    /// </summary> 
    public class DenyStudentRecordsReleaseAccessInformation
    {
        /// <summary>
        /// Flag indicates whether student can deny access to all its student records release
        /// </summary>
        public bool DenyAccessToAll { get; set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }
    }
}
