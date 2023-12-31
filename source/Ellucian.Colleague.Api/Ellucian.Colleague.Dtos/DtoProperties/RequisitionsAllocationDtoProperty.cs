﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// The discount applied to the line item (e.g. trade/volume discounts).
    /// </summary>
    [DataContract]
    public class RequisitionsAllocationDtoProperty
    {
        /// <summary>
        /// The portion (or percentage) of the line item amount or quantity allocated to the accounting string. 
        /// </summary>
        [DataMember(Name = "allocated", EmitDefaultValue = false)]
        public RequisitionsAllocatedDtoProperty Allocated { get; set; }


        /// <summary>
        /// The tax amount associated with the accounting string - overrides the distributed line amounts, if specified.
        /// </summary>
        [DataMember(Name = "taxAmount", EmitDefaultValue = false)]
        public Amount2DtoProperty TaxAmount { get; set; }


        /// <summary>
        /// Additional charges applied to the accounting string (e.g. freight) - overrides the distributed line item amounts
        /// </summary>
        [DataMember(Name = "additionalAmount", EmitDefaultValue = false)]
        public Amount2DtoProperty AdditionalAmount { get; set; }

        /// <summary>
        /// The discount amount associated with the accounting string - overrides the distributed line amounts
        /// </summary>
        [DataMember(Name = "discountAmount", EmitDefaultValue = false)]
        public Amount2DtoProperty DiscountAmount { get; set; }

    }
}