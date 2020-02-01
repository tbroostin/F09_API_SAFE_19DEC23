// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration for session and account settings
    /// </summary>
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
