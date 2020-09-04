// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is a Vendor and commodity code association
    /// </summary>
    public class VendorCommodity
    {
        /// <summary>
        /// The id
        /// </summary>
        /// 
        public string Id { get; set; }

        /// <summary>
        /// standard price of commodity for vendor
        /// </summary>
        public decimal? StdPrice { get; set; }

        /// <summary>
        /// standard price date of commodity for vendor
        /// </summary>
        public DateTime? StdPriceDate { get; set; }

    }
}
