//Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The list of valid admission application status types 
    /// </summary>
    [DataContract]
    public class AdmissionDecisionType2 : CodeItem2
    {
        /// <summary>
        /// Admission application status type category
        /// </summary>
        [DataMember(Name = "decisionCategories", EmitDefaultValue = false)]
        public IEnumerable<AdmissionApplicationStatusTypesCategory2?> Category { get; set; }         
     }      
}          
