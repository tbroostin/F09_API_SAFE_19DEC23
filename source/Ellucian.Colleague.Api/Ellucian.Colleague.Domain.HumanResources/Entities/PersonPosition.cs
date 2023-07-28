/* Copyright 2016-2023 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PersonPosition object describes how a person holds a position.
    /// </summary>
    [Serializable]
    public class PersonPosition
    {
        /// <summary>
        /// The database ID of the PersonPosition
        /// The ID will be empty if this entity is a Non-Employee Position as noted by the NonEmployeePosition field
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The PERSON Id
        /// </summary>
        public string PersonId { get { return personId; } }
        private readonly string personId;

        /// <summary>
        /// The PositionId. <see cref="Position"/>
        /// </summary>
        public string PositionId { get { return positionId; } }
        private readonly string positionId;

        /// <summary>
        /// The Id of the person's supervisor for this position
        /// </summary>
        public string SupervisorId { get; set; }

        /// <summary>
        /// The Id of the person's alternate supervisor for this position
        /// </summary>
        public string AlternateSupervisorId { get; set; }

        /// <summary>
        /// Contains the list of supervisor Ids (if any) assigned to the supervisory position defined for this position
        /// </summary>
        public List<string> PositionLevelSupervisorIds { get; set; }

        /// <summary>
        /// The date this person begins in this position.
        /// </summary>
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                if (EndDate.HasValue && EndDate.Value < value)
                {
                    throw new ArgumentOutOfRangeException("Start Date cannot be after the EndDate");
                }
                startDate = value;
            }
        }
        private DateTime startDate;

        /// <summary>
        /// The date this person ends being in this position.
        /// </summary>
        public DateTime? EndDate
        {
            get { return endDate; }
            set
            {
                if (value.HasValue && value.Value < StartDate)
                {
                    throw new ArgumentOutOfRangeException("End Date cannot be before Start Date");
                }
                endDate = value;
            }
        }
        private DateTime? endDate;

        /// <summary>
        /// The date this person's position is migrated from WA to SS
        /// </summary>
        public DateTime? MigrationDate { get; set; }

        /// <summary>
        /// The end date of the last PayPeriod the employee entered time for in Web Advisor
        /// </summary>
        public DateTime? LastWebTimeEntryPayPeriodEndDate { get; set; }

        /// <summary>
        /// bool that states whether this PersonPosition is a Non-Employee Position
        /// The Id of this entity will be empty because the Non-Employee Position record comes from HRPER and not PERPOS
        /// </summary>
        public bool NonEmployeePosition { get { return nonEmployeePosition; } }
        private readonly bool nonEmployeePosition;

        /// <summary>
        /// Contains the list of work schedule items for this person's position. Each WorkScheduleItem represents a day
        /// of the week with a corresponding unit (in hours) which together form a work schedule for this person's position.
        /// </summary>
        public List<WorkScheduleItem> WorkScheduleItems { get; set; }

        /// <summary>
        /// <summary>
        /// Decimal field that represents the full-time equivalent (FTE) value of the employee in the position
        /// </summary>
        public Decimal? FullTimeEquivalent { get { return fullTimeEquivalent; } set { fullTimeEquivalent = value; } }
        private Decimal? fullTimeEquivalent;

        /// <summary>
        /// Create a PersonPosition object
        /// </summary>
        /// <param name="id">The Id of the PersonPosition</param>
        /// <param name="personId">The Colleague PERSON id of the person in this position</param>
        /// <param name="positionId">The Id of the Position assigned to this person</param>
        /// <param name="startDate">The date on which the person begins this position</param>
        /// <param name="fullTimeEquivalent">Full-time equivalent (FTE) value of the employee in the position</param>
        public PersonPosition(string id, string personId, string positionId, DateTime startDate, Decimal? fullTimeEquivalent)
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

            this.id = id;
            this.personId = personId;
            this.positionId = positionId;
            this.startDate = startDate;
            this.nonEmployeePosition = false;
            this.fullTimeEquivalent = fullTimeEquivalent;
          }

        /// <summary>
        /// A person position that can only be created for a Non-Employee Position
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="positionId"></param>
        public PersonPosition(string personId, string positionId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }
            if (string.IsNullOrEmpty(positionId))
            {
                throw new ArgumentNullException("positionId");
            }

            this.personId = personId;
            this.positionId = positionId;
            this.nonEmployeePosition = true;
        }

        /// <summary>
        /// Two PersonPositions are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var personPosition = obj as PersonPosition;

            if (string.IsNullOrWhiteSpace(personPosition.Id) && string.IsNullOrWhiteSpace(this.Id))
            {
                return personPosition.PositionId == this.PositionId &&
                     personPosition.PersonId == this.PersonId;
            }

            return personPosition.Id == this.Id;
        }

        /// <summary>
        /// Hashcode representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return PositionId.GetHashCode() ^ PersonId.GetHashCode();
        }

        /// <summary>
        /// String representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id;
        }
    }
}
