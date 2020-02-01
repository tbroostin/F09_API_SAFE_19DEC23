// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Actions the identity can take on attachments in the collection
    /// </summary>
    [Serializable]
    public enum AttachmentAction
    {
        /// <summary>
        /// View
        /// </summary>
        View,

        /// <summary>
        /// Create
        /// </summary>
        Create,

        /// <summary>
        /// Delete
        /// </summary>
        Delete,

        /// <summary>
        /// Update
        /// </summary>
        Update
    }
}