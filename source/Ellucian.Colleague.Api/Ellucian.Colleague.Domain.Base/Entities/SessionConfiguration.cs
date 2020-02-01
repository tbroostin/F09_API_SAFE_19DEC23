// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Configuration for session and account settings
    /// </summary>
    [Serializable]
    public class SessionConfiguration
    {
        /// <summary>
        /// Flag indicating whether or not username recovery is enabled
        /// </summary>
        public bool UsernameRecoveryEnabled { get; set; }

        /// <summary>
        /// Flag indicating whether or not password reset is enabled
        /// </summary>
        public bool PasswordResetEnabled { get; set; }
    }
}