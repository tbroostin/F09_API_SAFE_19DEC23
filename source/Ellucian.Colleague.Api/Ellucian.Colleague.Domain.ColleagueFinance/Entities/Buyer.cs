// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Buyer entity
    /// </summary>
    [Serializable]
    public class Buyer
    {
        /// <summary>
        /// ID of Staff record
        /// </summary>
        public string RecordKey { get; set; }

        /// <summary>
        /// GUID for Staff record
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// First and Last name of Staff
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// PersonId of Staff record
        /// </summary>
        public string PersonGuid { get; set; }

        /// <summary>
        /// Status of Staff record
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The Add date of the staff record
        /// </summary>
        public DateTime? StartOn { get; set; }

        /// <summary>
        /// The Change Date of the Staff record
        /// </summary>
        public DateTime? EndOn { get; set; }

    }
}
