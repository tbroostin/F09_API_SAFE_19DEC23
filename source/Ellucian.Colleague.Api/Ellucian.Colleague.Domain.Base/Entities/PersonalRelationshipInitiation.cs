//Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonalRelationshipInitiation
    {
        public PersonalRelationshipInitiation()
        {
            AddressLines = new List<string>();
        }

        public string PersonMatchRequestGuid { get; set; }

        public string PersonMatchRequestId { get; set; }

        public string PersonId { get; set; }

        public string Originator { get; set; }

        public string RequestType { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        public string AddressType { get; set; }

        public List<string> AddressLines { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Locality { get; set; }

        public string State { get; set; }

        public string Region { get; set; }

        public string County { get; set; }

        public string SubRegion { get; set; }

        public string Zip { get; set; }

        public string PostalCode { get; set; }

        public string DeliveryPoint { get; set; }

        public string CarrierRoute { get; set; }

        public string CorrectionDigit { get; set; }

        public string PhoneType { get; set; }

        public string Phone { get; set; }

        public string EmailType { get; set; }

        public string Email { get; set; }

        /// <summary>
        /// Uniqueidentifier
        /// </summary>
        public string RelationshipGuid { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string RelationshipId { get; set; }

        /// <summary>
        /// The other person in the relationship (e.g., the student's parent)
        /// </summary>
        public string RelatedPersonId { get; set; }

        /// <summary>
        /// The code defining the relationship
        /// </summary>
        public string RelationshipType { get; set; }

        /// <summary>
        /// The code defining the relationship
        /// </summary>
        public string InverseRelationshipType { get; set; }

        /// <summary>
        /// The date the relationship began
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The date the relationship ended (can be future)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///  points to personal-relationship-statuses
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///  points to comments from RELATION
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Guid of subject person
        /// </summary>
        public string SubjectPersonGuid { get; set; }

        /// <summary>
        /// Guid of related person
        /// </summary>
        public string RelatedPersonGuid { get; set; }

        #region Override methods

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" }, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            PersonalRelationshipInitiation other = obj as PersonalRelationshipInitiation;
            if (other == null)
            {
                return false;
            }
            return other.RelationshipId.Equals(RelationshipId);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return RelationshipId.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return RelationshipId;
        }

        #endregion
    }
}
