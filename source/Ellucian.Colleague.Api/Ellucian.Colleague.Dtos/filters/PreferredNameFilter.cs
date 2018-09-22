// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Instructor named query
    /// </summary>
    public class PreferredNameFilter
    {
        /// <summary>
        /// instructor
        /// </summary>        
        [DataMember(Name = "preferredName", EmitDefaultValue = false)]
        [FilterProperty("preferredName")]
        public string PreferredName { get; set; }
    }
}