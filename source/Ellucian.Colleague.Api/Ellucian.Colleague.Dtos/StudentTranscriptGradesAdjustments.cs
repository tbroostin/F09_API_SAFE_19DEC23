//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The grade adjustments submitted for the student transcript. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StudentTranscriptGradesAdjustments : BaseModel2
    {
        /// <summary>
        /// The details of the adjustments to the student transcript.
        /// </summary>

        [JsonProperty("detail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StudentTranscriptGradesAdjustmentsDetail Detail { get; set; }

    }
}
