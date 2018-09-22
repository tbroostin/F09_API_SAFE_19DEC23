// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The condition of being married or unmarried.
    /// </summary>
    [DataContract]
    public class MaritalStatus2 : CodeItem2
    {
        /// <summary>
        /// The type of marital status.
        /// </summary>
        [DataMember(Name = "maritalCategory")]
        public MaritalStatusType2? StatusType { get; set; }
    }
}