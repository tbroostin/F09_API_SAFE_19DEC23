﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.Converters;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Financial award details by period.
    /// </summary>
    [DataContract]
    public class StudentAwardStatusDtoProperty
    {
        /// <summary>
        /// The state of the award.
        /// </summary>
        [DataMember(Name = "state", EmitDefaultValue = false)]
        public Dtos.EnumProperties.StudentAwardStatus State { get; set; }

        /// <summary>
        /// Effective date of the state.
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "stateOn", EmitDefaultValue = false)]
        public DateTime? StateOn { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public StudentAwardStatusDtoProperty()
        {
        }
    }
}