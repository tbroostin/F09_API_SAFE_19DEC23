// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor named query
    /// </summary>
    public class PersonFilterFilter
    {
        /// <summary>
        /// instructor
        /// </summary>        
        [DataMember(Name = "personFilter", EmitDefaultValue = false)]
        [FilterProperty("personFilter")]
        public string personFilterId { get; set; }
    }
}