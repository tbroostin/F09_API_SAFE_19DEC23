﻿// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Dtos.Attributes;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.Filters
{
    /// <summary>
    /// A code item filter
    /// </summary>
    [DataContract]
    public class CodeItemFilter
    {
        /// <summary>
        /// A shortened or contracted form of a word or phrase, used to represent the whole. Abbreviations are not assumed to be unique.
        /// </summary>
        [DataMember(Name = "code", EmitDefaultValue = false)]
        [FilterProperty( "criteria" )]
        public string Code { get; set; }
    }
}