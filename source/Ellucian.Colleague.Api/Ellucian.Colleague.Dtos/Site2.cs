// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// A physical location within an organization.
    /// </summary>
    [DataContract]
    public class Site2 : CodeItem2
    {
        /// <summary>
        /// Globally unique identifier for associated organization
        /// </summary>
        [DataMember(Name = "organization", EmitDefaultValue = false)]
        public GuidObject2 OrganizationGuid { get; set; }
    }
}