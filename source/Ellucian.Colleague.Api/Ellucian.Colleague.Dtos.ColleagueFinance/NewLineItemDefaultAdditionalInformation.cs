// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Default line item information required for modifying requition.
    /// </summary>
    public class NewLineItemDefaultAdditionalInformation
    {
        /// <summary>
        /// List of default Line Item tax codes.
        /// </summary>
        public List<string> TaxCodes { get; set; }

        /// <summary>
        /// Default tax form for the line item.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Default box number for the line item.
        /// </summary>
        public string BoxNo { get; set; }

        /// <summary>
        /// Default state for the line item.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Standard price for the line item.
        /// </summary>
        public decimal? StdPrice { get; set; }

        /// <summary>
        /// Commodity code description.
        /// </summary>
        public string CommodityCode { get; set; }

        /// <summary>
        /// Commodity code description.
        /// </summary>
        public string CommodityCodeDesc { get; set; }

        /// <summary>
        /// Fixed asset flag.
        /// </summary>
        public string FixedAssetFlag { get; set; }
    }
}
