﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Newtonsoft.Json;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Credential DTO Property
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AdmissionClassificationDtoProperty
    {
        /// <summary>
        /// The student's admission category (e.g. first-time, transfer, returning, freshman)
        /// </summary>
        [JsonProperty("admissionCategory", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public GuidObject2 AdmissionCategory { get; set; }

    }
}
