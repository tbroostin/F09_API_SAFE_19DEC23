// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Honor presented to a student based on academic achievement
    /// </summary>
    [DataContract]
    public class OtherHonor : CodeItem2
    {
        /// <summary>
        /// A type of academic honor
        /// </summary>
        [DataMember(Name = "type")]
        public AcademicHonorType AcademicHonorType { get; set; }

    }
}
