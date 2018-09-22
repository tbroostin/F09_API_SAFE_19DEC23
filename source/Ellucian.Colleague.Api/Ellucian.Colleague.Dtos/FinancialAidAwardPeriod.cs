﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Financial Aid Award Period
    /// </summary>
    [DataContract]
    public class FinancialAidAwardPeriod : CodeItem2
    {
        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status", EmitDefaultValue = false)]
        public string Status { get; set; }

        /// <summary>
        /// Start
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "startOn", EmitDefaultValue= false)]
        public DateTime? Start { get; set; }


        /// <summary>
        /// End
        /// </summary>
        [JsonConverter(typeof(DateOnlyConverter))]
        [DataMember(Name = "endOn", EmitDefaultValue = false)]
        public DateTime? End { get; set; }


    }
}
