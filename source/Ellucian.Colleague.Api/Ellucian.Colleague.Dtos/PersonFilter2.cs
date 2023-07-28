// Copyright 2023 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.DtoProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a person filter
    /// </summary>
    [DataContract]
    //public class PersonFilter2 : FilterCodeItem
    public class PersonFilter2
    {
        /// <summary>
        /// <see cref="PersonNameDtoProperty">Names</see> of the person
        /// </summary>
        [JsonProperty("personIds", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> PersonIds { get; set; }
    }
}