// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A user with proxy access granted or revoked to one or more workflows for one or more proxy subject users
    /// </summary>
    public class ProxyUser
    {
        /// <summary>
        /// ID of the proxy user
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Date on which proxy access permissions were most recently granted or revoked
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Proxy access permissions granted to, and potentially revoked from, the proxy user
        /// </summary>
        public IEnumerable<ProxyAccessPermission> Permissions { get; set; }
    }
}
