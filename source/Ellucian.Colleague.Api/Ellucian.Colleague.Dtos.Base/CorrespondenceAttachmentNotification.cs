// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This DTO carries information that is used optionally for status change and back office notification
    /// whenever a self-service user uploads a new attachments associated to a correspondence request.
    /// </summary>
    public class CorrespondenceAttachmentNotification
    {
        /// <summary>
        /// The person id (required)
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// The communication code associated to the correspondence request that received the attachment (required)
        /// </summary>
        public string CommunicationCode { get; set; }
        /// <summary>
        /// The assign date associated to the correspondence request - required, cannot be null.
        /// </summary>
        public DateTime? AssignDate { get; set; }
        /// <summary>
        /// The instance associated to the correspondence request (optional)
        /// </summary>
        public string Instance { get; set; }
    }

}
