//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// IntgVendorAddressUsages
    /// </summary>
    [Serializable]
    public class IntgVendorAddressUsages : GuidCodeItem
    {
       
        /// <summary>
        /// Initializes a new instance of the <see cref="IntgVendorAddressUsages"/> class.
        /// </summary>
        /// <param name="guid">The Unique Identifier</param>
        /// <param name="code">The code.</param>
        /// <param name="description">The description.</param>
        public IntgVendorAddressUsages(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}