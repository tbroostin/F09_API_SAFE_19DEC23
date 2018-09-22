// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration properties related to organizational relationships
    /// </summary>
    public class OrganizationalRelationshipConfiguration
    {
        /// <summary>
        /// Defines the mapping between the types and the valcodes for those types
        /// </summary>
        public IDictionary<OrganizationalRelationshipType, List<string>> RelationshipTypeCodeMapping { get; set; }
    }
}
