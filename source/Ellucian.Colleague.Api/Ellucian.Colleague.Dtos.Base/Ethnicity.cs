// Copyright 2013-2020 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Information about ethnicities
    /// </summary>
    public class Ethnicity
    {
        /// <summary>
        /// Unique system Id for this ethnicity
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this ethnicity
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Ethnicity type for the ethnicity
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public EthnicityType Type { get; set; }

    }
}