//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The vendor associated with the blanket purchase order. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class BlanketPurchaseOrdersVendor
    {
        /// <summary>
        /// The details associated with an existing vendor.
        /// </summary>

        [JsonProperty("existingVendor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PurchaseOrdersExistingVendorDtoProperty ExistingVendor { get; set; }

        /// <summary>
        /// The details associated with an undefined vendor or an override to an existing vendor's information.
        /// </summary>

        [JsonProperty("manualVendorDetails", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ManualVendorDetailsDtoProperty ManualVendorDetails { get; set; }

    }
}