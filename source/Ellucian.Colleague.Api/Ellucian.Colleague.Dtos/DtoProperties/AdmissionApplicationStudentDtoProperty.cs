﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;
using Newtonsoft.Json;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The residency type of the applicant as specified in the student system.
    /// </summary>
    [DataContract]
    public class AdmissionApplicationStudentDtoProperty
    {
        /// <summary>
        /// The person applying for the admission.
        /// </summary>
        [DataMember(Name = "applicant", EmitDefaultValue = false)]
        public GuidObject2 Student { get; set; }
    }
}