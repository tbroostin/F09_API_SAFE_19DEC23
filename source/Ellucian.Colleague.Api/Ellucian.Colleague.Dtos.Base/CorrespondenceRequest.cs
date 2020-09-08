// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Correspondence Request DTO
    /// </summary>
    public class CorrespondenceRequest
    {
        /// <summary>
        /// The person id
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The correspondence request code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The correspondence request instance uniquely identifies the associated 
        /// correspondence request
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// The status type of the correspondence request
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CorrespondenceRequestStatus Status { get; set; }

        /// <summary>
        /// Correspondence request status description
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// The date that the status of this CorrespondenceRequest was updated.
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// The correspondence request's due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// The correspondence request's assign date. Needed to match up to correct item in the database along with Code and person.
        /// </summary>
        public DateTime? AssignDate { get; set; }
    }
}
