using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Role used to control user access to menus and pages in self-service configuration. 
    /// Permissions associated with the role allow for finer-grained security within the functions 
    /// that this role is allowed access to. Roles are assigned to the users of the system.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Unique (free-form) identification of this role (must match value in configuration)
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// <see cref="Permission">Permissions</see> associated with this role
        /// </summary>
        public IEnumerable<Permission> Permissions { get; set; }
    }
}
