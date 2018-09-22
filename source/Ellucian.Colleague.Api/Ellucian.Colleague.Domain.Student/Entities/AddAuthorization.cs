// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Add Authorization information for a particular section.
    /// </summary>
    [Serializable]
    public class AddAuthorization
    {
        /// <summary>
        /// A unique identifier for the add authorization.
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string _id;

        /// <summary>
        /// The ID of the section for which add authorization belongs. (Required)
        /// </summary>
        public string SectionId { get { return _sectionId; } }
        private string _sectionId;

        /// <summary>
        /// Generated add authorization code to be used in paper system. (Optional)
        /// </summary>
        public string AddAuthorizationCode { get; set; }


        /// <summary>
        /// Student Id assigned to this add authorization
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Date & time the student was assigned to this authorization
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
        /// Faculty Id who revoked the add authorization
        /// </summary>
        public string RevokedBy { get; set; }

        /// <summary>
        /// Date and time that the authorization was revoked
        /// </summary>
        public DateTimeOffset? RevokedTime { get; set; }

        /// <summary>
        /// Constructor for an add authorization 
        /// </summary>
        /// <param name="id">Unique Id of the authorization.</param>
        /// <param name="sectionId">Id of the section, required</param>
        public AddAuthorization(string id, string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Section ID is required");
            }    
            _id = id;
            _sectionId = sectionId;
            IsRevoked = false;
        }

        
    }
}
