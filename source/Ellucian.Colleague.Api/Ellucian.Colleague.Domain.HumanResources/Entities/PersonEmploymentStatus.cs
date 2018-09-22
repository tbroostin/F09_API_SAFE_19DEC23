/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// PersonPosition object describes how a person holds a position.
    /// </summary>
    [Serializable]
    public class PersonEmploymentStatus
    {
        /// <summary>
        /// The identifier of this record
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The identifier of the associated person
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// The identifier of the associated position, which is primary
        /// </summary>
        public string PrimaryPositionId { get; set; }
        /// <summary>
        /// The start date of this person status record
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The identifier of the associated position
        /// </summary>
        public string PersonPositionId { get; set; }
        /// <summary>
        /// The end date of this person status record
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Creates an instance of a PersonStatus
        /// </summary>
        /// <param name="id"></param>
        /// <param name="PersonId"></param>
        /// <param name="primaryPositionId"></param>
        /// <param name="personPositionId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public PersonEmploymentStatus(string id, string personId, string primaryPositionId, string personPositionId, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(id)) { throw new ArgumentNullException("id"); }
            if (string.IsNullOrWhiteSpace(personId)) { throw new ArgumentNullException("personId"); }
            if (string.IsNullOrWhiteSpace(primaryPositionId)) { throw new ArgumentNullException("primaryPositionId"); }
            if (string.IsNullOrWhiteSpace(personPositionId)) { throw new ArgumentNullException("personPositionId"); }
            if (!startDate.HasValue) { throw new ApplicationException("Start Date should have value"); }

            this.Id = id;
            this.PersonId = personId;
            this.PrimaryPositionId = primaryPositionId;
            this.PersonPositionId = personPositionId;
            this.StartDate = startDate.Value;
            this.EndDate = endDate;
        }

        /// <summary>
        /// Two PersonEmploymentStatuses are equal when their Ids are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var personEmploymentStatus = obj as PersonEmploymentStatus;

            return personEmploymentStatus.Id == this.Id;
        }

        /// <summary>
        /// Hashcode representation of object (Id)
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
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
