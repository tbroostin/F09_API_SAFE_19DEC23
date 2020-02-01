//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonMatchRequestInitiation
    {
        public PersonMatchRequestInitiation()
        {
            AlternateIds = new List<PersonAlt>();
        }

        public string Guid { get; set; }

        public string RecordKey { get; set; }

        public string PersonId { get; set; }

        public string Originator { get; set; }

        public string RequestType { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string BirthFirstName { get; set; }

        public string BirthMiddleName { get; set; }

        public string BirthLastName { get; set; }

        public string ChosenFirstName { get; set; }

        public string ChosenMiddleName { get; set; }

        public string ChosenLastName { get; set; }

        public List<PersonMatchRequestInitiationOtherNames> OtherNames { get; private set; }

        public string Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string TaxId { get; set; }

        public List<PersonAlt> AlternateIds { get; private set; }

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

        #region Other Names Methods

        /// <summary>
        /// Used to add person alternate Ids to a person
        /// </summary>
        /// <param name="personAlt"><see cref="PersonAlt">Person Alt</see></param>
        public void AddOtherName(string first, string middle, string last)
        {
            if (string.IsNullOrEmpty(last))
            {
                throw new ArgumentNullException("last", "Last Name is required when adding other names.");
            }
            var otherName = new PersonMatchRequestInitiationOtherNames(first, middle, last);
            if (OtherNames.Where(f => f.Equals(otherName)).Count() > 0)
            {
                throw new ArgumentException("Other Name already exists in the Other Names list.");
            }
            OtherNames.Add(otherName);
        }

        #endregion

        #region Alternate Ids Methods

        /// <summary>
        /// Used to add person alternate Ids to a person
        /// </summary>
        /// <param name="personAlt"><see cref="PersonAlt">Person Alt</see></param>
        public void AddAlternateId(string id, string type)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Alternate Id is required when adding alternate ID information.");
            }
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type", "Alternate Id type is required when adding alternate ID information.");
            }
            var personAlt = new PersonAlt(id, type);
            if (AlternateIds.Where(f => f.Equals(personAlt)).Count() > 0)
            {
                throw new ArgumentException("Alternate Id and type already exists in the Alternate Ids list.");
            }
            AlternateIds.Add(personAlt);
        }

        #endregion

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
            PersonMatchRequestInitiation other = obj as PersonMatchRequestInitiation;
            if (other == null)
            {
                return false;
            }
            return other.RecordKey.Equals(RecordKey);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return RecordKey.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return RecordKey;
        }

        #endregion
    }
}
