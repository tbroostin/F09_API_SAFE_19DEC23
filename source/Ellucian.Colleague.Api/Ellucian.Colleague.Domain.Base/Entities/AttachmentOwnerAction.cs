// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Actions the owner can take on attachments they own in the collection
    /// </summary>
    [Serializable]
    public enum AttachmentOwnerAction
    {
        /// <summary>
        /// Update
        /// </summary>
        Update,

        /// <summary>
        /// Delete
        /// </summary>
        Delete        
    }
}