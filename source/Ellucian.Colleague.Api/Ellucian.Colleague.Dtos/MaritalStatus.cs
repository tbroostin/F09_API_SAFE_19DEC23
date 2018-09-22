// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// The condition of being married or unmarried.
    /// </summary>
    [DataContract]
    public class MaritalStatus : CodeItem
    {
        /// <summary>
        /// The type of marital status.
        /// </summary>
        [DataMember(Name = "parentCategory")]
        public MaritalStatusType? StatusType { get; set; }
    }
}