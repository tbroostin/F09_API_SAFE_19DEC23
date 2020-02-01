// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment statuses
    /// </summary>
    [Serializable]
    public enum AttachmentStatus
    {
        /// <summary>
        /// Active attachment
        /// </summary>
        Active,

        /// <summary>
        /// Attachment was deleted
        /// </summary>
        Deleted
    }
}