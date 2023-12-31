﻿// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about an email type
    /// </summary>
    [DataContract]
    public class EmailType : CodeItem2
    {
        /// <summary>
        /// The <see cref="EmailTypeList">type</see> of email address type for this entity
        /// </summary>
        [DataMember(Name = "emailType")]
        public EmailTypeList EmailTypeList { get; set; }
      
    }
}