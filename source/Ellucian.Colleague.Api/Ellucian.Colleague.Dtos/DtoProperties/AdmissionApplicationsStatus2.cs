﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The statuses of the application through the admission process stages.
    /// </summary>
    [DataContract]
    public class AdmissionApplicationsStatus2
    {
        /// <summary>
        /// The top level category for the status of the application.
        /// </summary>

        [DataMember(Name = "type", EmitDefaultValue = false)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AdmissionApplicationsStatusType2 AdmissionApplicationsStatusType { get; set; }

        /// <summary>
        /// The extension of the top level type category.
        /// </summary>

        [DataMember(Name = "detail", EmitDefaultValue = false)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public GuidObject2 AdmissionApplicationsStatusDetail { get; set; }

        /// <summary>
        /// The date when the status was assigned.
        /// </summary>

        [DataMember(Name = "startOn", EmitDefaultValue = false)]
        public DateTime AdmissionApplicationsStatusStartOn { get; set; }
    }
}
