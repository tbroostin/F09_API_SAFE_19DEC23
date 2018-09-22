//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Classifications
    /// </summary>
    [Serializable]
    public class Classifications : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Classifications"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public Classifications(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}