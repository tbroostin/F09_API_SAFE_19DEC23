//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The valid list of languages. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Languages : BaseModel2
    {
        /// <summary>
        /// The full name of the language.
        /// </summary>

        [JsonProperty("title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// The code used to identify the language.
        /// </summary>

        [JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// The ISO code used to identify the language.
        /// </summary>

        [JsonProperty("isoCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ISOCode { get; set; }

    }
}
