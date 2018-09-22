// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Converters;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Academic Period 
    /// </summary>
    [DataContract]
    public class AcademicPeriod2 : CodeItem2
    {
        /// <summary>
        /// The <see cref="AcademicPeriodCategory">Academic Period category</see>
        /// </summary>
        [DataMember(Name = "category", EmitDefaultValue = false)]
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


    }
}
