// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Commodity Code is used to search for an commodity code
    /// </summary>
    [Serializable]
    public class ProcurementCommodityCode: CodeItem
    {
        public string FixedAssetsFlag { get; set; }
        public bool DefaultDescFlag { get; set; }
        public decimal? Price { get; set; }
        public List<string> TaxCodes { get; set; }

        /// <summary>
        /// Commodity Code constructor
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public ProcurementCommodityCode(string code, string description)
            : base(code, description)
        {
            // no additional work to do
        }
    }
}
