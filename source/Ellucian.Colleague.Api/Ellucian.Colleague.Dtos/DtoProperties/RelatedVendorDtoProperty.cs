// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The vendor assigned to receive payment for this vendor or the parent vendor.
    /// </summary>
    [DataContract]
    public class RelatedVendorDtoProperty
    {
        /// <summary>
        ///  The type of related vendor.
        /// </summary>
        [DataMember(Name = "type")]
        [FilterProperty("criteria")]
        public Ellucian.Colleague.Dtos.EnumProperties.VendorType? Type { get; set; }

        /// <summary>
        /// The related vendor
        /// </summary>
        [DataMember(Name = "vendor")]
        [FilterProperty("criteria")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 Vendor { get; set; }
    }
}