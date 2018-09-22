// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// Information about an Interest
    /// </summary>
    [DataContract]
    public class Interest : CodeItem2
    {
        /// <summary>
        /// An area of interests
        /// </summary>
        [DataMember(Name = "area", EmitDefaultValue = false)]
        public GuidObject2 Area { get; set; }
    }
}