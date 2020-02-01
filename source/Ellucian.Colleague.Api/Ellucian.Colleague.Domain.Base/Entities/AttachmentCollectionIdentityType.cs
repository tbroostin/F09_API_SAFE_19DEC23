// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// File attachment collection user types
    /// </summary>
    [Serializable]
    public enum AttachmentCollectionIdentityType
    {
        /// <summary>
        /// Role identity
        /// </summary>
        Role,

        /// <summary>
        /// User identity
        /// </summary>
        User,
    }
}