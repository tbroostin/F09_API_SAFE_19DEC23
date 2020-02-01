// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment collection statuses
    /// </summary>
    [Serializable]
    public enum AttachmentCollectionStatus
    {
        /// <summary>
        /// Active attachment collection
        /// </summary>
        Active,

        /// <summary>
        /// Inactive attachment collection
        /// </summary>
        Inactive
    }
}