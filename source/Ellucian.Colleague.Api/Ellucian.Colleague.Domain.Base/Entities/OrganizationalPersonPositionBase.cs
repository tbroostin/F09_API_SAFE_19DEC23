// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Organizational person position base class
    /// </summary>
    [Serializable]
    public class OrganizationalPersonPositionBase
    {
        /// <summary>
        /// The unique Id of the person position. 
        /// </summary>
        public string Id { get { return id; } }
        private string id;

        /// <summary>
        /// The Id for the position
        /// </summary>
        public string PositionId { get { return positionId; } }
        private string positionId;

        /// <summary>
        /// The Title for this position
        /// </summary>
        public string PositionTitle { get { return positionTitle; } }
        private string positionTitle;

        /// <summary>
        /// Id of the person assigned to the position
        /// </summary>
        public string PersonId { get { return personId; } }
        private string personId { get; set; }

        /// <summary>
        /// Start Date of the organizational position
        /// </summary>
        public DateTime? StartDate { get { return startDate; } }
        private DateTime? startDate;

        /// <summary>
        /// End Date of the organizational position
        /// </summary>
        public DateTime? EndDate { get { return endDate; } }
        private DateTime? endDate;

        /// <summary>
        /// Returns the status of the Organizational Person Position: Current, Past, Future, or Unknown
        /// </summary>
        public OrganizationalPersonPositionStatus Status
        {
            get
            {
                return OrganizationalStructureService.GetOrganizationalPersonPositionStatus(StartDate, EndDate);
            }
        }

        /// <summary>
        /// Initializes an existing base organizational position.
        /// </summary>
        /// <param name="id">The organizational person position id</param>
        /// <param name="personId">The person id</param>
        /// <param name="positionId">The position role id</param>
        /// <param name="positionTitle">The position role title</param>
        /// <param name="startDate">The start date of this organizational person position</param>
        /// <param name="endDate">The end date of this organizational person position</param>
        public OrganizationalPersonPositionBase(string id, string personId, string positionId, string positionTitle, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }
            if (string.IsNullOrEmpty(positionTitle))
            {
                throw new ArgumentNullException("positionTitle");
            }
            this.id = id;
            this.personId = personId;
            this.positionId = positionId;
            this.positionTitle = positionTitle;
            this.startDate = startDate;
            this.endDate = endDate;
        }

    }
}
