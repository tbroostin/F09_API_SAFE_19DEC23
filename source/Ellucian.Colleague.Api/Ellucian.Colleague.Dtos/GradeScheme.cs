﻿// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A specification for how a grade should be calculated.
    /// </summary>
    [DataContract]
    public class GradeScheme : CodeItem
    {
        /// <summary>
        /// The date on which a record becomes valid.
        /// </summary>
        [DataMember(Name = "effectiveStartDate")]
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// The date after which a record is no longer valid.
        /// </summary>
        [DataMember(Name = "effectiveEndDate")]
        public DateTime? EffectiveEndDate { get; set; }
    }
}