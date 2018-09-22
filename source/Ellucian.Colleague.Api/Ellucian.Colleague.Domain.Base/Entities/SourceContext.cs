// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Source Context
    /// </summary>
    [Serializable]
    public class SourceContext : GuidCodeItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocialMediaType"/> class.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public SourceContext(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}