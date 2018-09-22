// Copyright 2014-2015  Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about an "other" location at which an instructional event occurs
    /// </summary>
    [JsonObject("other")]
    public class InstructionalOtherLocation : InstructionalLocation
    {
        /// <summary>
        /// Location type
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public InstructionalLocationType LocationType { get; set; }

        /// <summary>
        /// A text description of some other location
        /// </summary>
        [JsonProperty("other")]
        public string OtherLocation { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public InstructionalOtherLocation()
        {
            LocationType = Dtos.InstructionalLocationType.InstructionalOtherLocation;
        }
    }
}