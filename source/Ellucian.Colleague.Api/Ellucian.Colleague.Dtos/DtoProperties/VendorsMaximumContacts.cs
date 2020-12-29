//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The contacts associated with a vendor. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class VendorsMaximumContacts
    {


        /// <summary>
        /// The detail of the vendor contact.
        /// </summary>

        [JsonProperty("vendorContact", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 VendorContact { get; set; }

        /// <summary>
        /// The type of relationship between the vendor and the contact.
        /// </summary>

        [JsonProperty("relationshipType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 RelationshipType { get; set; }

        /// <summary>
        /// The person who is the vendor contact.
        /// </summary>

        [JsonProperty("person", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public VendorContactsPerson Person { get; set; }

        /// <summary>
        /// The phone type, number and extension of the contact person.
        /// </summary>

        [JsonProperty("phones", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<VendorContactsPhones> Phones { get; set; }


    }
}

