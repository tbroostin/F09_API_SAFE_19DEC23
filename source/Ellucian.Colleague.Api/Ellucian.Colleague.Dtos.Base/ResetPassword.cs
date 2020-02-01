// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Reset password
    /// </summary>
    public class ResetPassword
    {
        /// <summary>
        /// Id of user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Password Reset Token
        /// </summary>
        public string ResetToken { get; set; }

        /// <summary>
        /// User's new password
        /// </summary>
        public string NewPassword { get; set; }
    }
}
