// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Commodity Unit Type is used to search for an commodity unit type
    /// </summary>
    [Serializable]
    public class CommodityUnitType : GuidCodeItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CommodityUnitType"/> class.
        /// </summary>
        public CommodityUnitType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
