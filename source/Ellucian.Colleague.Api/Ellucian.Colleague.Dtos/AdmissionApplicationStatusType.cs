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
    public class AdmissionApplicationStatusType : CodeItem2
    {
        /// <summary>
        /// Admission application status type category
        /// </summary>
        [DataMember(Name = "category")]
        public AdmissionApplicationStatusTypesCategory Category { get; set; }         
     }      
}          
