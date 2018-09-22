// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A user who grants a proxy user permissions to act on his/her behalf.
    /// </summary>
    public class ProxySubject
    {
        /// <summary>
        /// Person ID of the proxy subject
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Proxy subject's full name.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Date on which proxy access permissions were most recently granted or revoked
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Proxy access permissions granted to, and potentially revoked from, the proxy user
        /// </summary>
        public IEnumerable<ProxyAccessPermission> Permissions { get; set; }

        /// <summary>
        /// Flag indicating whether or not reauthorization is currently needed
        /// </summary>
        public bool ReauthorizationIsNeeded { get; set; }
    }
}
