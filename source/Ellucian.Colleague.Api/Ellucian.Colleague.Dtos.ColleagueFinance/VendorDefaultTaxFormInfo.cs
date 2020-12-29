// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is a Vendor default tax form info
    /// </summary>
    public class VendorDefaultTaxFormInfo
    {
        /// <summary>
        /// The vendor id
        /// </summary>
        /// 
        public string VendorId { get; set; }

        /// <summary>
        /// Default Tax form.
        /// </summary>
        public string TaxForm { get; set; }

        /// <summary>
        /// Default Tax form box code.
        /// </summary>
        public string TaxFormBoxCode { get; set; }

        /// <summary>
        /// Default Tax form state / location.
        /// </summary>
        public string TaxFormState { get; set; }

    }
}
