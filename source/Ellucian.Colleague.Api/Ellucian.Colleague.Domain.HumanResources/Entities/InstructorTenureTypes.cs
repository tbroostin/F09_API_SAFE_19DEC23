using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// TenureTypes
    /// </summary>
    [Serializable]
    public class TenureTypes : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TenureTypes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public TenureTypes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}