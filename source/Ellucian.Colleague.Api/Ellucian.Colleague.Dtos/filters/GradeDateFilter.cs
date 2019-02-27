// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// Grade Date named query
    /// </summary>
    public class GradeDateFilter
    {
        /// <summary>
        /// Grade Date
        /// </summary>        
        [DataMember(Name = "gradeDate", EmitDefaultValue = false)]
        [FilterProperty("gradeDate")]
        public DateTime? GradeDate { get; set; }
    }
}
