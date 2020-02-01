// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment collection
    /// </summary>
    public class AttachmentCollection
    {
        /// <summary>
        /// ID of the attachment collection
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The attachment collection's display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of this attachment collection's purpose
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Colleague person Id who created/owns this collection - the owner is the collection admin
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Collections's status (e.g. active, inactive)
        /// </summary>
        public AttachmentCollectionStatus Status { get; set; }

        /// <summary>
        /// List of actions the attachment owner can take upon attachments they own in this collection
        /// </summary>
        public IEnumerable<AttachmentOwnerAction> AttachmentOwnerActions { get; set; }

        /// <summary>
        /// List of allowed content types in this collection
        /// </summary>
        public IEnumerable<string> AllowedContentTypes { get; set; }

        /// <summary>
        /// Max size, in bytes, of attachments that can be created in this collection
        /// </summary>
        public long? MaxAttachmentSize { get; set; }

        /// <summary>
        /// List of individual users and actions they can take upon attachments in this collection
        /// </summary>
        public IEnumerable<AttachmentCollectionIdentity> Users { get; set; }

        /// <summary>
        /// List of individual roles and actions they can take upon attachments in this collection
        /// </summary>
        public IEnumerable<AttachmentCollectionIdentity> Roles { get; set; }

        /// <summary>
        /// ISO-8601 duration, example: 5 years would be 'P5Y'
        /// </summary>
        public string RetentionDuration { get; set; }

        /// <summary>
        /// ID of the encryption key used to encrypt the attachment content keys in this collection
        /// </summary>
        public string EncryptionKeyId { get; set; }
    }
}