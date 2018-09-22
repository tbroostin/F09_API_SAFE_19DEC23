//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The details associated with an existing vendor 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ExistingVendorDetailsDtoProperty
    {
        /// <summary>
        /// The details associated with an existing vendor.
        /// </summary>
        [JsonProperty("vendor", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 Vendor { get; set; }

        /// <summary>
        /// The details of an alternative stored address
        /// </summary>
        [JsonProperty("alternativeVendorAddress", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AlternativeVendorAddress { get; set; }


    }
}