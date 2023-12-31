﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A collection of people organized together into a community or other social,
    /// commercial or political structure for the purposes of education.
    /// </summary>
    [DataContract]
    public class Ethnicity2 : CodeItem2
    {
        /// <summary>
        /// Properties required for governmental or other reporting
        /// </summary>
        [DataMember(Name = "reporting", EmitDefaultValue = false)]
        public List<EthnicityReporting> EthnicityReporting { get; set; }
    }
}