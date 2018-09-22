// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A user who has granted proxy access permissions to the current user.
    /// </summary>
    [Serializable]
    public class ProxySubject
    {
        private readonly string _id;
        private readonly List<ProxyAccessPermission> _permissions = new List<ProxyAccessPermission>();

        /// <summary>
        /// Person ID of the proxy subject
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Proxy subject's full name.
        /// </summary>
        public string FullName { get; set; }

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
        /// Proxy access permissions granted to, and potentially revoked from, the proxy user
        /// </summary>
        public ReadOnlyCollection<ProxyAccessPermission> Permissions { get; private set; }

        /// <summary>
        /// Flag indicating whether or not reauthorization is currently needed
        /// </summary>
        public bool ReauthorizationIsNeeded { get { return Permissions.Any(p => p.ReauthorizationDate.HasValue && p.ReauthorizationDate.Value <= DateTime.Today); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyUser" /> class
        /// </summary>
        /// <param name="id">ID of the proxy user</param>
        /// <exception cref="System.ArgumentNullException">id</exception>
        public ProxySubject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            _id = id;

            Permissions = _permissions.AsReadOnly();
        }

        /// <summary>
        /// Add a proxy permission
        /// </summary>
        /// <param name="permission">Proxy permission workflow to be added</param>
        public void AddPermission(ProxyAccessPermission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException("permission", "A permission must be supplied.");
            }
            if (permission.ProxySubjectId != _id)
            {
                throw new ArgumentException("Permission's Proxy Subject ID must match user's ID.", "permission");
            }

            _permissions.Add(permission);
        }
    }
}
