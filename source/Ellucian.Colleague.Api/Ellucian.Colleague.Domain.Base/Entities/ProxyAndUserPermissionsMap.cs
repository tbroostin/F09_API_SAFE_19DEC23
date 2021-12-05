// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Mapping a person's proxy permissions with their user permissions.
/// </summary>
namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class ProxyAndUserPermissionsMap
    {
        /// <summary>
        /// The ID of the proxy and uder access permissions mapping record.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// User Permission
        /// </summary>
        public string UserPermission { get { return userPermission; } }
        private readonly string userPermission;

        /// <summary>
        /// Proxy Access Permission Code
        /// </summary>
        public string ProxyAccessPermission { get { return proxyAccessPermission; } }
        private readonly string proxyAccessPermission;

        /// <summary>
        /// Parameterized contructor to instantiate a ProxyAndUserPermissionsMap object.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userPermission"></param>
        /// <param name="proxyAccessPermission"></param>
        public ProxyAndUserPermissionsMap(string id, string userPermission, string proxyAccessPermission)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrWhiteSpace(userPermission))
            {
                throw new ArgumentNullException("userPermission");
            }
            if (string.IsNullOrWhiteSpace(proxyAccessPermission))
            {
                throw new ArgumentNullException("proxyAccessPermission");
            }
            this.id = id;
            this.userPermission = userPermission;
            this.proxyAccessPermission = proxyAccessPermission;
        }
    }
}
