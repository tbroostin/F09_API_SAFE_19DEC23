// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is a Vendor search result
    /// </summary>
    public class VendorSearchResult
    {
        /// <summary>
        /// The vendor id
        /// </summary>
        /// 
        public string VendorId { get; set; }

        /// <summary>
        /// The vendor name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// The vendor address
        /// </summary>
        public string VendorAddress { get; set; }

    }
}
