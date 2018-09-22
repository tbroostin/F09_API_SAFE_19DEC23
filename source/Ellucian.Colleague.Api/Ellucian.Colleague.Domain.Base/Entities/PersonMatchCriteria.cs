// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// The literal values used for person lookup
    /// </summary>
    [Serializable]
    public class PersonMatchCriteria
    {
        /// <summary>
        /// Identifier of the record specifying the match criteria to use for this lookup
        /// </summary>
        public string MatchCriteriaIdentifier;

        /// <summary>
        /// List of possible names for the person to find
        /// </summary>
        public IEnumerable<PersonName> MatchNames { get; private set; }

        /// <summary>
        /// A government identifier for the person to find
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// The gender of the person to find
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The birthdate of the person to find
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// The email address of the person to find
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The phone number of the person to find
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// The phone extension of the person to find
        /// </summary>
        public string PhoneExtension { get; set; }

        /// <summary>
        /// The prefix of the person to find
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the person to find
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Creates a new PersonMatchCriteria
        /// </summary>
        /// <param name="criteria">The identifier of the record containing the match criteria</param>
        public PersonMatchCriteria(string criteria, IEnumerable<PersonName> names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            if (!names.Any())
            {
                throw new ArgumentException("At least one name must be provided for searching.", "names");
            }
            if (string.IsNullOrEmpty(criteria))
            {
                throw new ArgumentNullException("Search criteria record must be specified", "criteria");
            }

            MatchCriteriaIdentifier = criteria;
            MatchNames = new List<PersonName>(names);
        }

    }
}
