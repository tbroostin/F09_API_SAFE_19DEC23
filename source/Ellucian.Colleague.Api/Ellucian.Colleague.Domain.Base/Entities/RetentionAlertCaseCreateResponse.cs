// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
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
        public List<string> OwnerRoles { get; set; }

        /// <summary>
        /// List of unique role titles to which the case is assigned.
        /// </summary>
        public List<string> OwnerRoleTitles { get; set; }

        /// <summary>
        /// List of unique PERSON IDs to whom this case is specifically assigned.
        /// </summary>
        public List<string> OwnerIds { get; set; }

        /// <summary>
        /// List of unique PERSON Namess to whom this case is specifically assigned.
        /// </summary>
        public List<string> OwnerNames { get; set; }

        /// <summary>
        /// Error messages
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        public RetentionAlertCaseCreateResponse()
        {
            OwnerRoles = new List<string>();
            OwnerRoleTitles = new List<string>();
            OwnerIds = new List<string>();
            ErrorMessages = new List<string>();
            OwnerNames = new List<string>();
        }
    }
}
