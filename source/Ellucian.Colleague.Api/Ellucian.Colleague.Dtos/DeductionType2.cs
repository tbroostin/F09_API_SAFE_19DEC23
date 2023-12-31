﻿using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a deduction type
    /// </summary>
    [DataContract]
    public class DeductionType2 : CodeItem2
    {
        /// <summary>
        /// The global identifier for the Category.
        /// </summary>
        [DataMember(Name = "category", EmitDefaultValue = false)]
        public GuidObject2 Category { get; set; }

        /// <summary>
        /// The global identifier for the Cost Calculation Method.
        /// </summary>
        [DataMember(Name = "costCalculationMethod")]
        public GuidObject2 CostCalculationMethod { get; set; }

        /// <summary>
        /// The withholding frequency associated with the deduction type.
        /// </summary>
        [DataMember(Name = "withholdingFrequency", EmitDefaultValue = false)]
        public DeductionTypeWithholdingFrequencyDtoProperty WithholdingFrequency { get; set; }

        /// <summary>
        /// An indicator if the deduction should be applied before or after taxes are withheld.
        /// </summary>
        [DataMember(Name = "taxApplication", EmitDefaultValue = false)]
        public DeductionTypeTaxApplicationType? TaxApplication { get; set; }

    }
}