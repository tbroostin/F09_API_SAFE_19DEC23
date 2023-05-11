// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// There may be any number of instances of a specific position in an organization since several different
    /// persons may be assigned to the same position title. This entity represents one such instance
    /// of a position, including its relationships to other organizational position. This entity includes a 
    /// person ID when currently assigned to a person.
    /// </summary>
    [Serializable]
    public class OrganizationalPersonPosition : OrganizationalPersonPositionBase
    {
        /// <summary>
        /// List of relationships for this organizational position
        /// </summary>
        public ReadOnlyCollection<OrganizationalRelationship> Relationships { get; private set; }
        private readonly List<OrganizationalRelationship> relationships = new List<OrganizationalRelationship>();

        public ReadOnlyCollection<OrganizationalPositionRelationship> PositionRelationships { get; private set; }
        private readonly List<OrganizationalPositionRelationship> positionRelationships = new List<OrganizationalPositionRelationship>();


        /// <summary>
        /// Initializes an existing base organizational position.
        /// </summary>
        /// <param name="id">The organizational person position id</param>
        /// <param name="personId">The person id</param>
        /// <param name="positionId">The position role id</param>
        /// <param name="positionTitle">The position role title</param>
        /// <param name="startDate">The start date of this organizational person position</param>
        /// <param name="endDate">The end date of this organizational person position</param>
        public OrganizationalPersonPosition(string id, string personId, string positionId, string positionTitle, DateTime? startDate, DateTime? endDate)
            : base(id, personId, positionId, positionTitle, startDate, endDate)
        {
            Relationships = relationships.AsReadOnly();
            PositionRelationships = positionRelationships.AsReadOnly();
        }

        /// <summary>
        /// Adds a known relationship to this organizational person position. 
        /// </summary>
        /// <param name="relationship">Relationship to add</param>
        public void AddRelationship(OrganizationalRelationship relationship)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException("relationship", "Relationship cannot be null");
            }
            this.ValidateRelationship(relationship);
            this.relationships.Add(relationship);
        }

        /// <summary>
        /// Make sure that this relationship category does not already exist for the person
        /// and that this relationship involves this person.
        /// </summary>
        /// <param name="relationship"></param>
        private void ValidateRelationship(OrganizationalRelationship relationship)
        {
            if (relationship.OrganizationalPersonPositionId != this.Id && relationship.RelatedOrganizationalPersonPositionId != this.Id)
            {
                throw new ArgumentException("Cannot add a relationship that does not represent a relationship with this person position.", "relationship");
            }

            var existingRelationship = Relationships.Where(r => r.Category == relationship.Category && r.OrganizationalPersonPositionId == this.Id && relationship.OrganizationalPersonPositionId == this.Id).FirstOrDefault();
            if (existingRelationship != null)
            {
                throw new ArgumentException("Found duplicate relationship: category " + relationship.Category + " for position " + this.Id);
            }
        }

        /// <summary>
        /// Adds a position relationship to this organizational person position. 
        /// </summary>
        /// <param name="positionRelationship">Position relationship to add</param>
        public void AddPositionRelationship(OrganizationalPositionRelationship positionRelationship)
        {
            if (positionRelationship == null)
            {
                throw new ArgumentNullException("positionRelationship", "Relationship cannot be null");
            }
            ValidatePositionRelationship(positionRelationship);
            positionRelationships.Add(positionRelationship);
        }

        /// <summary>
        /// Make sure this position relationship involves this person position
        /// </summary>
        /// <param name="positionRelationship">The position relationship to validate</param>
        private void ValidatePositionRelationship(OrganizationalPositionRelationship positionRelationship)
        {
            if (positionRelationship.OrganizationalPositionId != this.PositionId && positionRelationship.RelatedOrganizationalPositionId != this.PositionId)
            {
                throw new ArgumentException("Cannot add a position relationship that does not represent a relationship with this person position.", "positionRelationship");
            }
        }
    }
}
