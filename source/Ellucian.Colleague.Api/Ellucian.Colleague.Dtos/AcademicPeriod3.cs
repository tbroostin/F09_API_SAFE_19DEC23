// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Academic Period 
    /// </summary>
    [DataContract]
    public class AcademicPeriod3 : CodeItem2
    {
        /// <summary>
        /// The <see cref="AcademicPeriodCategory2">Academic Period category</see>
        /// </summary>
        [DataMember(Name = "category")]
        public AcademicPeriodCategory2 Category { get; set; }

        /// <summary>
        /// Start
        /// </summary>
        [DataMember(Name = "startOn", EmitDefaultValue= false)]
        public DateTimeOffset? Start { get; set; }


        /// <summary>
        /// End
        /// </summary>
        [DataMember(Name = "endOn", EmitDefaultValue = false)]
        public DateTimeOffset? End { get; set; }
      
        /// <summary>
        /// Registration Status
        /// </summary>
        [DataMember(Name = "registration", EmitDefaultValue = false)]
        [FilterProperty("criteria")]
        public TermRegistrationStatus? RegistrationStatus { get; set; }

        /// <summary>
        /// Census Dates
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(DateOnlyConverter), DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "censusDates", EmitDefaultValue = false)]
        public List<DateTime?> CensusDates { get; set; }
     }
 }
