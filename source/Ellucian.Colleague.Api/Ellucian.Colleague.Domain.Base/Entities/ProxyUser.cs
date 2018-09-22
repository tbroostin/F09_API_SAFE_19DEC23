// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A user with proxy access granted or revoked to one or more workflows for one or more proxy subject users
    /// </summary>
    [Serializable]
    public class ProxyUser
    {
        private readonly string _id;
        private readonly List<ProxyAccessPermission> _permissions = new List<ProxyAccessPermission>();

        /// <summary>
        /// ID of the proxy user
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Proxy access permissions granted to, and potentially revoked from, the proxy user
        /// </summary>
        public ReadOnlyCollection<ProxyAccessPermission> Permissions { get; private set; }

        /// <summary>
        /// Date on which proxy access permissions were most recently granted or revoked
        /// </summary>
        public DateTime? EffectiveDate 
        { 
            get
            {
                if (Permissions.Any())
                {
                    return Permissions.Select(p => p.EffectiveDate).Max();
                }
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyUser"/> class
        /// </summary>
        /// <param name="id">ID of the proxy user</param>
        public ProxyUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            _id = id;

            Permissions = _permissions.AsReadOnly();
        }

        /// <summary>
        /// Add a proxy workflow to the proxy workflow group
        /// </summary>
        /// <param name="permission">Proxy workflow to be added</param>
        public void AddPermission(ProxyAccessPermission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission", "A permission must be supplied.");
            }
            if (permission.ProxyUserId != _id)
            {
                throw new ArgumentException("Permission's Proxy User ID must match user's ID.");
            }

            _permissions.Add(permission);
        }
    }
}
