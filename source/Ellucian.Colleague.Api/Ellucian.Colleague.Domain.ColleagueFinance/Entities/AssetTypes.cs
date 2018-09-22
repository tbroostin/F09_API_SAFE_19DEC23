//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// AssetTypes
    /// </summary>
    [Serializable]
    public class AssetTypes : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetTypes"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public AssetTypes(string guid, string code, string description)
            : base(guid, code, description)
        {
        }

        public string AstpCalcMethod { get; set; }
        public decimal? AstpSalvagePoint { get; set; }
        public int? AstpUsefulLife { get; set; }
    }
}