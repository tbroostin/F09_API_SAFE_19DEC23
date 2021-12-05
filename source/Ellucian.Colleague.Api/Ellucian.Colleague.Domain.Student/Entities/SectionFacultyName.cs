// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Entity that only carries faculty names
    /// </summary>
    [Serializable]

    public class SectionFacultyName
    {
        private readonly string _FacultyId;
        /// <summary>
        /// Id of the book
        /// </summary>
        public string FacultyId { get { return _FacultyId; } }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        /// <summary>
        /// This is the name the faculty member has indicated should be used in student-facing software.
        /// This is not guaranteed to have a value - it is up to the consumer of this entity to determine which
        /// name is appropriate to use based on context.
        /// </summary>
        public string ProfessionalName { get; set; }

        /// <summary>
        /// Name that should be used when displaying a person's name on reports and forms.
        /// This property is based on a Name Address Hierarcy and will be null if none is provided.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }


        /// <summary>
        /// Section Book constructor
        /// </summary>
        /// <param name="requirementStatus">enumeration value</param>
        public SectionFacultyName(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyId", "Faculty Id must be provided");
            }
            _FacultyId = facultyId;

        }
    }
}

