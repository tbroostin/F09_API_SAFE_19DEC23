//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// AltIdTypes
    /// </summary>
    [Serializable]
    public class AltIdTypes : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AltIdTypes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AltIdTypes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}