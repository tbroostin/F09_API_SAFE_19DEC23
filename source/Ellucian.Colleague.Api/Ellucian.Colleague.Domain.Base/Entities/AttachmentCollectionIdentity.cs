// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment collection identity
    /// </summary>
    [Serializable]
    public class AttachmentCollectionIdentity
    {
        /// <summary>
        /// ID of the identity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Type of identity
        /// </summary>
        public AttachmentCollectionIdentityType Type { get; private set; }

        /// <summary>
        /// List of actions the identity can take on attachments in the collection
        /// </summary>
        public IEnumerable<AttachmentAction> Actions { get; set; }

        /// <summary>
        /// Create a file attachment identity
        /// </summary>
        /// <param name="id">Identity ID</param>
        /// <param name="type">Type of identity</param>
        /// <param name="actions">List of actions the identity can take in the collection</param>
        public AttachmentCollectionIdentity(string id, AttachmentCollectionIdentityType type, IEnumerable<AttachmentAction> actions)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            if (actions == null || !actions.Any())
                throw new ArgumentNullException("actions");

            Id = id;
            Type = type;
            Actions = actions;
        }
    }
}