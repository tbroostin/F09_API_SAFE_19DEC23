// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Commodity Code is used to search for an commodity code
    /// </summary>
    [Serializable]
    public class CommodityCode : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityCode"/> class.
        /// </summary>
        public CommodityCode(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
