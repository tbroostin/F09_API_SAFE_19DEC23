﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about a citizenship status
    /// </summary>
    [DataContract]
    public class CitizenshipStatus : CodeItem2
    {
        /// <summary>
        /// The <see cref="CitizenshipStatusType">entity type</see> for the personal relationship types
        /// </summary>
        [DataMember(Name = "category")]
        public CitizenshipStatusType citizenshipStatusType { get; set; }
    }
}