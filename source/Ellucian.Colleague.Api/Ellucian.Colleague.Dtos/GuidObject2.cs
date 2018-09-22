// Copyright 2014 Ellucian Company L.P. and its affiliates
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A GUID container
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class GuidObject2
    {
        /// <summary>
        /// Globally unique Identifier (GUID)
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Constructor for GuidObject
        /// </summary>
        [JsonConstructor]
        public GuidObject2()
        {
        }

        /// <summary>
        /// Constructor for GuidObject
        /// </summary>
        /// <param name="id">Globally unique identifier</param>
        public GuidObject2(string id)
        {
            Id = id;
        }
    }
}