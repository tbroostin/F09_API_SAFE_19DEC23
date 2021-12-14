// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Mapping a person's proxy permissions with their user permissions.
    /// </summary>
    public class ProxyAndUserPermissionsMap
    {
        /// <summary>
        /// The ID of the proxy and uder access permissions mapping record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User Permission
        /// </summary>
        public string UserPermission { get; set; }

        /// <summary>
        /// Proxy Access Permission Code
        /// </summary>
        public string ProxyAccessPermission { get; set; }
    }
}
