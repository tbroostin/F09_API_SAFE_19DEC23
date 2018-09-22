using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Delivered security permission code. Permission codes are delivered and are assigned to users to enforce data security.
    /// Permissions are assigned to roles, then roles are assigned to users to enforce security.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Code that is used to determine whether user has permission to access certain functions
        /// </summary>
        public string Code { get; set; }
    }
}
