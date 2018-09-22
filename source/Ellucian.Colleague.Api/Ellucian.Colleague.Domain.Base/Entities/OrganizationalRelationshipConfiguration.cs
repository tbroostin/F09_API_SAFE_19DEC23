// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Configuration properties related to organizational relationships
    /// </summary>
    [Serializable]
    public class OrganizationalRelationshipConfiguration
    {
        /// <summary>
        /// Defines the mapping between the special processing codes and the valcodes with the special processing codes
        /// </summary>
        public IDictionary<OrganizationalRelationshipType, List<string>> RelationshipTypeCodeMapping { get; set; }

        /// <summary>
        /// Constructor for Organizational Relationship Configuration
        /// </summary>
        public OrganizationalRelationshipConfiguration()
        {
            RelationshipTypeCodeMapping = new Dictionary<OrganizationalRelationshipType, List<string>>();
        }
    }
}
