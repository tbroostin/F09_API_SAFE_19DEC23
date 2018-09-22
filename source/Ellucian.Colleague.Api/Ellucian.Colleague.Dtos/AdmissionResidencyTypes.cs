using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Admission applicants types of residency 
    /// </summary>
    [DataContract]
    public class AdmissionResidencyTypes : CodeItem2
    {
        /// <summary>
        /// The human-readable name of a resource.
        /// </summary>
        [DataMember(Name = "code", EmitDefaultValue = false)]
        new public string Code { get; set; } 
    }      
}          
