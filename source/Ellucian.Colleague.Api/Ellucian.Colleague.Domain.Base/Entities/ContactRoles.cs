// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Source Context
    /// </summary>
    [Serializable]
    public class ContactRoles : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public ContactRoles(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}