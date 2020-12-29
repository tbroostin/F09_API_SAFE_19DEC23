// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// academicCatalog named query
    /// </summary>
    public class PersonalRelationships2Filter
    {
        /// <summary>
        /// person
        /// </summary>        
        [JsonProperty("person")]
        [FilterProperty("person")]
        public GuidObject2 person { get; set; }

        /// <summary>
        /// person
        /// </summary>        
        [JsonProperty("relationshipType")]
        [FilterProperty("relationshipType")]
        public GuidObject2 relationshipType { get; set; }

        /// <summary>
        /// person Filter - Guid for SAVE.LIST.PARMS which contains a savedlist of person IDs
        /// </summary>        
        [JsonProperty("personFilter")]
        [FilterProperty("personFilter")]
        [JsonConverter(typeof(GuidObject2FilterConverter))]
        public GuidObject2 personFilter { get; set; }
    }
}
