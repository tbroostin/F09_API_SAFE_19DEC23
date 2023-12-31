﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Types of holds on student records
    /// </summary>
    [DataContract]
    public class PersonHoldType :CodeItem2
    {
        /// <summary>
        /// A global category of student hold types
        /// </summary>
        [DataMember(Name = "category")]
        public PersonHoldCategoryTypes Category { get; set; }
    }
}
