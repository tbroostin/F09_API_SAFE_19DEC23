// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information needed to create a new add authorization for a student in a section.
    /// </summary>
    public class AddAuthorizationInput
    {

        /// <summary>
        /// Section Id for the new add authorization
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Student Id being granted the add authorization
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Date and time the student was granted authorization
        /// </summary>
        public DateTimeOffset? AssignedTime { get; set; }

        /// <summary>
        /// Person Id who granted the authorization to the student 
        /// </summary>
        public string AssignedBy { get; set; }
    }
}