using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Change password object
    /// </summary>
    public class ChangePassword
    {
        /// <summary>
        /// Id of user
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// User's old password
        /// </summary>
        public string OldPassword { get; set; }
        /// <summary>
        /// User's new password
        /// </summary>
        public string NewPassword { get; set; }
    }
}
