﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A designation of a student's progress, based on the number of completed courses
    /// </summary>
    [DataContract]
    public class PlaceDtoProperty
    {
        /// <summary>
        /// The country where the address is located
        /// </summary> 
        [DataMember(Name = "country")]
        public AddressCountry Country { get; set; }

    }
}