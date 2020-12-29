//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// IntgPersonEmerTypes
    /// </summary>
    [Serializable]
    public class IntgPersonEmerTypes : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="IntgPersonEmerTypes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public IntgPersonEmerTypes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}