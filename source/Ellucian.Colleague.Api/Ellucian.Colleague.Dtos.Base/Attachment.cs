// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// ID of the attachment
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The attachment's display name (e.g. 2017 Tax Info.pdf)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Content type (e.g. application/pdf; utf-8)
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Size, in bytes, of the attachment
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Status of the attachment (e.g. active, deleted)
        /// </summary>
        public AttachmentStatus Status { get; set; }

        /// <summary>
        /// Owner/Colleague person ID of the attachment
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Date when the attachment was created
        /// </summary>
        public DateTimeOffset? CreatedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who created the attachment
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date when the attachment's properties or content was modified
        /// </summary>
        public DateTimeOffset? ModifiedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who modified the attachment
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Date when the attachment was deleted
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }

        /// <summary>
        /// Colleague person ID of who deleted the attachment
        /// </summary>
        public string DeletedBy { get; set; }

        /// <summary>
        /// Attachment collection Id for which this attachment belongs
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Free form text tag
        /// </summary>
        public string TagOne { get; set; }
    }
}