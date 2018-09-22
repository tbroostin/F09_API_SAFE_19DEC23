// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// There may be any number of instances of a specific position in an organization since several different
    /// persons may be assigned to the same position title. This entity represents one such instance
    /// of a position, including its relationships to other organizational person positions. This entity includes a 
    /// person ID when currently assigned to a person.
    /// </summary>
    public class OrganizationalPersonPosition : OrganizationalPersonPositionBase
    {
        /// <summary>
        /// List of relationships involving this organizational person position
        /// </summary>
        public IEnumerable<OrganizationalRelationship> Relationships { get; set; }

        /// <summary>
        /// List of organizational position relationships involving this organizational person position
        /// </summary>
        public IEnumerable<OrganizationalPositionRelationship> PositionRelationships { get; set; }
    }
}
