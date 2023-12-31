﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.DtoProperties
{
    /// <summary>
    /// Various amounts involved in a financial aid award.
    /// </summary>
    [DataContract]
    public class StudentAwardAmountDtoProperty
    {
        /// <summary>
        /// Amount that was originally offered for the award period.
        /// </summary>
        [DataMember(Name = "originalOffered", EmitDefaultValue = false)]
        public AmountDtoProperty OriginalOffered { get; set; }

        /// <summary>
        /// Total amount offered for the award period.
        /// </summary>
        [DataMember(Name = "offered", EmitDefaultValue = false)]
        public AmountDtoProperty Offered { get; set; }

        /// <summary>
        /// Total amount accepted for the award period.
        /// </summary>
        [DataMember(Name = "accepted", EmitDefaultValue = false)]
        public AmountDtoProperty Accepted { get; set; }

        /// <summary>
        /// The amount declined for the award period
        /// </summary>
        [DataMember(Name = "declined", EmitDefaultValue = false)]
        public AmountDtoProperty Declined { get; set; }

        /// <summary>
        /// The amount canceled or rejected for the award period
        /// </summary>
        [DataMember(Name = "rejected", EmitDefaultValue = false)]
        public AmountDtoProperty Rejected { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        [JsonConstructor]
        public StudentAwardAmountDtoProperty()
        {
        }
    }
}