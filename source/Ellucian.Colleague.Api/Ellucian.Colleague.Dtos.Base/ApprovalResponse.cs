// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An approval response
    /// </summary>
    public class ApprovalResponse
    {
        /// <summary>
        /// Internal ID of the approval response
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Internal ID of the approval document that is being approved 
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// ID of the person to whom the approval applies
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Login ID of the user who gave the approval
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Date and time at which the approval was given
        /// </summary>
        public DateTimeOffset Received { get; set; }

        /// <summary>
        /// Status of the approval
        /// </summary>
        public bool IsApproved { get; set; }
    }
}
