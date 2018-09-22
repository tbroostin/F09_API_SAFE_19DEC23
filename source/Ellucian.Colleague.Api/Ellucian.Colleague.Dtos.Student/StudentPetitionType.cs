// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Type of student petition - faculty consent or regular student petition
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StudentPetitionType
    {
        /// <summary>
        /// Student Petition
        /// </summary>
        StudentPetition,
        /// <summary>
        /// Faculty Consent
        /// </summary>
        FacultyConsent
    }
}

