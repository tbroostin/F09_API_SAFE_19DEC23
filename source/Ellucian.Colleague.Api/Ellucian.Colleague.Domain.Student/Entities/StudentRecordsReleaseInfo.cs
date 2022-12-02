// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student Records Release information for a student
    /// </summary>
    [Serializable]
    public class StudentRecordsReleaseInfo
    {

        /// <summary>
        /// Student Records Release Information Unique Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Relationship First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Relationship Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Relationship PIN
        /// </summary>
        public string PIN { get; set; }

        /// <summary>
        /// Relationship type
        /// </summary>
        public string RelationType { get; set; }

        /// <summary>
        /// List of access areas to which access is given
        /// </summary>
        public List<string> AccessAreas { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///  End Date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Boolean value to indicate if the consent is given
        /// </summary>
        public bool IsConsentGiven { get; set; }
    }
}
