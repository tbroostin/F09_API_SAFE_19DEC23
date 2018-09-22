// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Add Authorization related to a specific section.
    /// </summary>
    public class AddAuthorization
    {
        /// <summary>
        /// Unique Id for the add authorization
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Section Id associated to this add authorization
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Generated Add Authorization Code (optional alternate Id for the authorization)
        /// that can be used by a student to assign themselves to an authorization.
        /// </summary>
        public string AddAuthorizationCode { get; set; }

        /// <summary>
        /// Student Id assigned to this add authorization
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Date and time the student was assigned to this authorization
        /// </summary>
        public DateTimeOffset? AssignedTime { get; set; }

        /// <summary>
        /// Who assigned this authorization to the student (could be student themselves or the faculty of the section)
        /// </summary>
        public string AssignedBy { get; set; }

        /// <summary>
        /// Indicates if this add authorization has been revoked
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Id of person who revoked the add authorization
        /// </summary>
        public string RevokedBy { get; set; }

        /// <summary>
        /// Date and time that the authorization was revoked
        /// </summary>
        public DateTimeOffset? RevokedTime { get; set; }
    }
}