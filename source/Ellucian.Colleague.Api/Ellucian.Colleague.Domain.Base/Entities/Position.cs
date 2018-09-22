//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Positions
    /// </summary>
    [Serializable]
    public class Positions : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Positions"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Positions(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}