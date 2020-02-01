// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Reset password token request
    /// </summary>
    public class PasswordResetTokenRequest
    {
        /// <summary>
        /// User ID (user name) subject to the password reset
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email of the user subject to the password reset
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
