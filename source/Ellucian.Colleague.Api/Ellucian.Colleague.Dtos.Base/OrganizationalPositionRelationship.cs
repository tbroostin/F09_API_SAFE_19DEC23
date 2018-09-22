// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Defines a relationship between two organizational positions
    /// </summary>
    public class OrganizationalPositionRelationship
    {
        /// <summary>
        /// The unique Id of the organizational position relationship. May be empty for a new relationship.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Id of the organizational position
        /// </summary>
        public string OrganizationalPositionId { get; set; }

        /// <summary>
        /// The title of the organizational position
        /// </summary>
        public string OrganizationalPositionTitle { get; set; }

        /// <summary>
        /// The Id of the related organizational position
        /// </summary>
        public string RelatedOrganizationalPositionId { get; set; }

        /// <summary>
        /// The title of the related organizational position
        /// </summary>
        public string RelatedOrganizationalPositionTitle { get; set; }

        /// <summary>
        /// The nature of the relationship, as locally defined.
        /// </summary>
        public string RelationshipCategory { get; set; }

        /// <summary>
        /// Collection of organizational person positions in the primary position
        /// </summary>
        public IEnumerable<OrganizationalPersonPositionBase> OrganizationalPersonPositions { get; set; }

        /// <summary>
        /// Collection of organizational person positions in the related position
        /// </summary>
        public IEnumerable<OrganizationalPersonPositionBase> RelatedOrganizationalPersonPositions { get; set; }
    }
}
