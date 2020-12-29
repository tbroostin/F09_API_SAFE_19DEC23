// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// File attachment collection identity
    /// </summary>
    public class AttachmentCollectionIdentity
    {
        /// <summary>
        /// ID of the identity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of identity
        /// </summary>
        public AttachmentCollectionIdentityType Type { get; set; }

        /// <summary>
        /// List of actions the identity can take on attachments in the collection
        /// </summary>
        public IEnumerable<AttachmentAction> Actions { get; set; }
    }
}