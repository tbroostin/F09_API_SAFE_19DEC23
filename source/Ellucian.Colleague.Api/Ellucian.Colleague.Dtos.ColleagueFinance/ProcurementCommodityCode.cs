// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Commodity Code 
    /// </summary>
    public class ProcurementCommodityCode
    {
        /// <summary>
        /// Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fixed Asstes flag
        /// </summary>
        public string FixedAssetsFlag { get; set; }

        /// <summary>
        /// Default description flag
        /// </summary>
        public bool DefaultDescFlag { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Tax codes
        /// </summary>
        public List<string> TaxCodes { get; set; }
    }
}
