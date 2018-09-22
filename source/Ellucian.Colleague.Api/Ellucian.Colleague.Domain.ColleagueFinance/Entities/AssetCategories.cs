//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// AssetCategories
    /// </summary>
    [Serializable]
    public class AssetCategories : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCategories"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AssetCategories(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}