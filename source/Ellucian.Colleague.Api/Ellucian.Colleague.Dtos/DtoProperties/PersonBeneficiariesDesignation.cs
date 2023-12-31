﻿//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.EnumProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The amount or percentage of the allocation of the benefit to the beneficiary.
    /// </summary>
    [DataContract]
    public class PersonBeneficiariesDesignation
    {
        /// <summary>
        /// The percentage of the allocation of the benefit to the beneficiary.
        /// </summary>

        [DataMember(Name = "percentage", EmitDefaultValue = false)]
        public decimal Percentage { get; set; }

    }
}
