// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor named query
    /// </summary>
    public class VendorDetail
    {
        /// <summary>
        /// VendorDetail - Guid for vendor who can be a person, organization or institution. We added this to do a query request 
        /// without having to identify if the vendor is an organization, person, or institution.
        /// </summary>        
        [JsonProperty("vendorDetail")]
        [FilterProperty("vendorDetail")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 vendorDetail { get; set; }
    }
}