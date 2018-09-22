// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Services;
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Representation of a relationship between organizational members
    /// </summary>
    [Serializable]
    public class OrganizationalRelationship
    {
        /// <summary>
        /// The unique Id of the organizational relationship. May be empty for a new relationship.
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
        /// The Id of the organizational person position
        /// </summary>
        public string OrganizationalPersonPositionId { get { return organizationalPersonPositionId; } }
        private string organizationalPersonPositionId;

        /// <summary>
        /// The person Id of the organizational person position
        /// </summary>
        public string PersonId { get { return personId; } }
        private string personId;

        /// <summary>
        /// The position Id of the organizational person position
        /// </summary>
        public string PositionId { get { return positionId; } }
        private string positionId;

        /// <summary>
        /// The Position Title of the organizational person position
        /// </summary>
        public string PositionTitle { get { return positionTitle; } }
        private string positionTitle;

        /// <summary>
        /// Organizational person position start date
        /// </summary>
        public DateTime? OrganizationalPersonPositionStartDate { get { return organizationalPersonPositionStartDate; } }
        private DateTime? organizationalPersonPositionStartDate;


        /// <summary>
        /// Organizational person position end date
        /// </summary>
        public DateTime? OrganizationalPersonPositionEndDate { get { return organizationalPersonPositionEndDate; } }
        private DateTime? organizationalPersonPositionEndDate;

        /// <summary>
        /// Returns the status of the Organizational Person Position
        /// </summary>
        public OrganizationalPersonPositionStatus OrganizationalPersonPositionStatus
        {
            get
            {
                return OrganizationalStructureService.GetOrganizationalPersonPositionStatus(OrganizationalPersonPositionStartDate, OrganizationalPersonPositionEndDate);
            }
        }

        /// <summary>
        /// Id of the related organizational person position
        /// </summary>
        public string RelatedOrganizationalPersonPositionId { get { return relatedOrganizationalPersonPositionId; } }
        private string relatedOrganizationalPersonPositionId;

        /// <summary>
        /// The person Id of the related organizational person position
        /// </summary>
        public string RelatedPersonId { get { return relatedPersonId; } }
        private string relatedPersonId;

        /// <summary>
        /// The position Id of the related organizational person position
        /// </summary>
        public string RelatedPositionId { get { return relatedPositionId; } }
        private string relatedPositionId;

        /// <summary>
        /// The Position Title of the related organizational person position
        /// </summary>
        public string RelatedPositionTitle { get { return relatedPositionTitle; } }
        private string relatedPositionTitle;

        /// <summary>
        /// Related organizational person position start date
        /// </summary>
        public DateTime? RelatedOrganizationalPersonPositionStartDate { get { return relatedOrganizationalPersonPositionStartDate; } }
        private DateTime? relatedOrganizationalPersonPositionStartDate;

        /// <summary>
        /// Related organizational person position end date
        /// </summary>
        public DateTime? RelatedOrganizationalPersonPositionEndDate { get { return relatedOrganizationalPersonPositionEndDate; } }
        private DateTime? relatedOrganizationalPersonPositionEndDate;

        /// <summary>
        /// Returns the status of the Related Organizational Person Position
        /// </summary>
        public OrganizationalPersonPositionStatus RelatedOrganizationalPersonPositionStatus
        {
            get
            {
                return OrganizationalStructureService.GetOrganizationalPersonPositionStatus(RelatedOrganizationalPersonPositionStartDate, RelatedOrganizationalPersonPositionEndDate);
            }
        }

        /// <summary>
        /// The nature of the relationship, as locally defined. May be org chart, or approvals, for example.
        /// </summary>
        public string Category { get { return category; } }
        private string category;

        /// <summary>
        /// Initializes a new organizational relationship.
        /// </summary>
        /// <param name="id">Id of the relationship</param>
        /// <param name="orgPersonPositionId">The primary organizational person position Id of the relationship</param>
        /// <param name="personId"> person Id</param>
        /// <param name="positionId"> position Id</param>
        /// <param name="positionTitle"> position title</param>
        /// <param name="orgPersonPositionStartDate">organizational person position start date</param>
        /// <param name="orgPersonPositionEndDate">organizational person position end date</param>
        /// <param name="relatedOrgPersonPositionId">The related org person position Id</param>
        /// <param name="relatedPersonId">related person Id</param>
        /// <param name="relatedPositionId">related position Id</param>
        /// <param name="relatedPositionTitle">related position title</param>
        /// <param name="relatedStartDate">related organizational person position start date</param>
        /// <param name="relatedEndDate">related organizational person position end date</param>
        /// <param name="category">The category of the relationship</param>
        public OrganizationalRelationship(string id, string orgPersonPositionId, string personId, string positionId, string positionTitle, DateTime? orgPersonPositionStartDate, DateTime? orgPersonPositionEndDate, string relatedOrgPersonPositionId, string relatedPersonId, string relatedPositionId, string relatedPositionTitle, DateTime? relatedStartDate, DateTime? relatedEndDate, string category)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(orgPersonPositionId))
            {
                throw new ArgumentNullException("orgPersonPositionId", "Organizational person position ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(relatedOrgPersonPositionId))
            {
                throw new ArgumentNullException("relatedOrgPersonPositionId", "Related organizational person position ID is required for organizational relationship.");
            }
            if (orgPersonPositionId == relatedOrgPersonPositionId)
            {
                throw new ArgumentException("Org Person position id and Related Org Person Position Id cannot be the same");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId", "Position ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(positionTitle))
            {
                throw new ArgumentNullException("positionTitle", "Position title is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(relatedPersonId))
            {
                throw new ArgumentNullException("relatedPersonId", "Related person ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(relatedPositionId))
            {
                throw new ArgumentNullException("relatedPositionId", "Related position ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(relatedPositionTitle))
            {
                throw new ArgumentNullException("relatedPositionTitle", "Related position title is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentNullException("category", "Category is required for organizational relationship.");
            }
            this.id = id;
            this.organizationalPersonPositionId = orgPersonPositionId;
            this.personId = personId;
            this.positionId = positionId;
            this.positionTitle = positionTitle;
            this.organizationalPersonPositionStartDate = orgPersonPositionStartDate;
            this.organizationalPersonPositionEndDate = orgPersonPositionEndDate;
            this.relatedOrganizationalPersonPositionId = relatedOrgPersonPositionId;
            this.relatedPersonId = relatedPersonId;
            this.relatedPositionId = relatedPositionId;
            this.relatedPositionTitle = relatedPositionTitle;
            this.relatedOrganizationalPersonPositionStartDate = relatedStartDate;
            this.relatedOrganizationalPersonPositionEndDate = relatedEndDate;
            this.category = category;
        }

        /// <summary>
        /// Initializes a new organizational relationship.
        /// </summary>
        /// <param name="id">Id of the relationship</param>
        /// <param name="orgPersonPositionId">The primary organizational person position Id of the relationship</param>
        /// <param name="relatedOrgPersonPositionId">The related org person position Id</param>
        /// <param name="category">The category of the relationship</param>
        public OrganizationalRelationship(string id, string orgPersonPositionId, string relatedOrgPersonPositionId, string category)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id", "ID cannot be null for organizational relationship.");
            }
            if (string.IsNullOrEmpty(orgPersonPositionId))
            {
                throw new ArgumentNullException("orgPersonPositionId", "Organizational person position ID is required for organizational relationship.");
            }
            if (string.IsNullOrEmpty(relatedOrgPersonPositionId))
            {
                throw new ArgumentNullException("relatedOrgPersonPositionId", "Related organizational person position ID is required for organizational relationship.");
            }
            if (orgPersonPositionId == relatedOrgPersonPositionId)
            {
                throw new ArgumentException("Org Person position id and Related Org Person Position Id cannot be the same");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category", "Category cannot be null for organizational relationship.");
            }
            this.id = id;
            this.organizationalPersonPositionId = orgPersonPositionId;
            this.relatedOrganizationalPersonPositionId = relatedOrgPersonPositionId;
            this.category = category;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            OrganizationalRelationship other = obj as OrganizationalRelationship;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id)
                && other.OrganizationalPersonPositionId.Equals(OrganizationalPersonPositionId)
                && other.RelatedOrganizationalPersonPositionId.Equals(RelatedOrganizationalPersonPositionId)
                && other.Category.Equals(Category);
        }

        public override int GetHashCode()
        {
            return (this.Id).GetHashCode();
        }

    }
}
