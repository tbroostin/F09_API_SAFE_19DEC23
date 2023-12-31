﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about an phone types
    /// </summary>
    [DataContract]
    public class PhoneType2 : CodeItem2
    {
        /// <summary>
        /// The <see cref="PhoneTypeList">type</see> of phone
        /// </summary>
        [DataMember(Name = "phoneType")]
        public PhoneTypeList PhoneTypeList { get; set; }
    }
}