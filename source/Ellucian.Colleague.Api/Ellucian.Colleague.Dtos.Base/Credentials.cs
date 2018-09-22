using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// User credentials
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// User's login id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; }
    }
}
