// Copyright 2019 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Enumeration of Encryption Key statuses
    /// </summary>
    public enum EncrKeyStatus
    {
        /// <summary>
        /// Active, the key can be used
        /// </summary>
        Active,

        /// <summary>
        /// Inactive, the key cannot be used
        /// </summary>
        Inactive
    }
}