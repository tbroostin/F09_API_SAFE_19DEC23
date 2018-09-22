// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Active on named query
    /// </summary>
    public class ActiveOnFilter
    {
        /// <summary>
        /// activeOn
        /// </summary>        
        [DataMember(Name = "activeOn", EmitDefaultValue = false)]
        [FilterProperty("activeOn")]
        public DateTime? ActiveOn { get; set; }
    }
}
