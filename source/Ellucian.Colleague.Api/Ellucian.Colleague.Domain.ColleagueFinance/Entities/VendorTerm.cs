// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Vendor Term
    /// </summary>
    [Serializable]
    public class VendorTerm : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="VendorTerm"/> class.
        /// </summary>
        public VendorTerm(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
