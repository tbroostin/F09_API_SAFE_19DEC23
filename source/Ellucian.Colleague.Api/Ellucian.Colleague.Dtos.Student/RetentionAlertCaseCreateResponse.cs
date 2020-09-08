// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// The response received from a Retention Alert case creation
    /// </summary>
    public class RetentionAlertCaseCreateResponse
    {
        /// <summary>
        ///  Key of the case record
        /// </summary>
        public string CaseId { get; set; }

        /// <summary>
        /// Current status code for the case
        /// </summary>
        public string CaseStatus { get; set; }

        /// <summary>
        /// Case items key
        /// </summary>
        public string CaseItemsId { get; set; }

        /// <summary>
        /// List of unique roles to which the case is assigned.
        /// </summary>
        public IEnumerable<string> OwnerRoles { get; set; }

        /// <summary>
        /// List of unique role titles to which the case is assigned.
        /// </summary>
        public IEnumerable<string> OwnerRoleTitles { get; set; }

        /// <summary>
        /// List of unique PERSON IDs to whom this case is specifically assigned.
        /// </summary>
        public IEnumerable<string> OwnerIds { get; set; }

        /// <summary>
        /// List of unique PERSON Namess to whom this case is specifically assigned.
        /// </summary>
        public IEnumerable<string> OwnerNames { get; set; }

        /// <summary>
        /// Error messages
        /// </summary>
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}
