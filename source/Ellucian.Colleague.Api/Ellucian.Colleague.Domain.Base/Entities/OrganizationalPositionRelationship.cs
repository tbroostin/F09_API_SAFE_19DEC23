// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Organizational Position
    /// </summary>
    [Serializable]
    public class OrganizationalPositionRelationship
    {
        /// <summary>
        /// The unique Id of the organizational position relationship. May be empty for a new relationship.
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = value;
                }
                else
                {
                    throw new ArgumentException("Id cannot be changed");
                }
            }
        }
        private string id;

        /// <summary>
        /// The Id of the organizational position
        /// </summary>
        public string OrganizationalPositionId { get; private set; }

        /// <summary>
        /// The title of the organizational position
        /// </summary>
        public string OrganizationalPositionTitle { get; private set; }

        /// <summary>
        /// The Id of the related organizational position
        /// </summary>
        public string RelatedOrganizationalPositionId { get; private set; }

        /// <summary>
        /// The title of the related organizational position
        /// </summary>
        public string RelatedOrganizationalPositionTitle { get; private set; }

        /// <summary>
        /// The nature of the relationship, as locally defined.
        /// </summary>
        public string RelationshipCategory { get; private set; }

        /// <summary>
        /// Collection of organizational person positions in the primary position
        /// </summary>
        public IEnumerable<OrganizationalPersonPositionBase> OrganizationalPersonPositions { get { return organizationalPersonPositions.AsReadOnly(); } }
        private readonly List<OrganizationalPersonPositionBase> organizationalPersonPositions;

        /// <summary>
        /// Collection of organizational person positions in the related position
        /// </summary>
        public IEnumerable<OrganizationalPersonPositionBase> RelatedOrganizationalPersonPositions { get { return relatedOrganizationalPersonPositions.AsReadOnly(); } }
        private readonly List<OrganizationalPersonPositionBase> relatedOrganizationalPersonPositions;

        /// <summary>
        /// Adds organizational person positions to the collection of constituents in the primary position
        /// </summary>
        /// <param name="organizationalPersonPositions">Organizational person positions</param>
        public void AddOrganizationalPersonPositions(IEnumerable<OrganizationalPersonPositionBase> organizationalPersonPositions)
        {
            if (organizationalPersonPositions != null)
            {
                this.organizationalPersonPositions.AddRange(organizationalPersonPositions);
            }

        }

        /// <summary>
        /// Adds organizational person positions to the collection of constituents in the related position
        /// </summary>
        /// <param name="relatedOrganizationalPersonPositions">Related organizational person positions</param>
        public void AddRelatedOrganizationalPersonPositions(IEnumerable<OrganizationalPersonPositionBase> relatedOrganizationalPersonPositions)
        {
            if (relatedOrganizationalPersonPositions != null)
            {
                this.relatedOrganizationalPersonPositions.AddRange(relatedOrganizationalPersonPositions);
            }

        }

        /// <summary>
        /// Initializes a new organizational position relationship
        /// </summary>
        /// <param name="id">Id of the position relationship</param>
        /// <param name="orgPositionId">Id of the organizational position</param>
        /// <param name="orgPositionTitle">Title of the organizational position</param>
        /// <param name="relatedOrgPositionId">Id of the related organizational position</param>
        /// <param name="relatedOrgPositionTitle">Title of the related organizational position</param>
        /// <param name="category">The category of the relationship</param>
        public OrganizationalPositionRelationship(string id, string orgPositionId, string orgPositionTitle, string relatedOrgPositionId, string relatedOrgPositionTitle, string category)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "ID cannot be null for organizational position relationship.");
            }
            if (string.IsNullOrEmpty(orgPositionId))
            {
                throw new ArgumentNullException("orgPositionId", "Organizational position ID is required for organizational position relationship.");
            }
            if (string.IsNullOrEmpty(relatedOrgPositionId))
            {
                throw new ArgumentNullException("relatedOrgPositionId", "Related organizational position ID is required for organizational position relationship.");
            }
            if (orgPositionId == relatedOrgPositionId)
            {
                throw new ArgumentException("Org position ID and related org position ID cannot be the same");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category", "Category cannot be null for organizational position relationship.");
            }
            Id = id;
            OrganizationalPositionId = orgPositionId;
            OrganizationalPositionTitle = orgPositionTitle;
            RelatedOrganizationalPositionId = relatedOrgPositionId;
            RelatedOrganizationalPositionTitle = relatedOrgPositionTitle;
            RelationshipCategory = category;
            organizationalPersonPositions = new List<OrganizationalPersonPositionBase>();
            relatedOrganizationalPersonPositions = new List<OrganizationalPersonPositionBase>();
        }
    }

}
