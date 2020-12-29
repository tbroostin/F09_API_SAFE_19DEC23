// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// The literal values used for person lookup
    /// </summary>
    [Serializable]
    public class PersonMatchCriteriaInstantEnrollment
    {
        /// <summary>
        /// First name of person to find
        /// </summary>
        public string FirstName { get; private set; }
        /// <summary>
        /// Last name of person to find
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Middle name of person to find
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The prefix of the person to find
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the person to find
        /// </summary>
        public string Suffix { get; set; }

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
        /// Address lines of the person to find
        /// </summary>
        public IEnumerable<string> AddressLines { get; set; }

        /// <summary>
        /// Address city of the person to find
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Address State of the person to find
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Address zip code of the person to find
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Address county code of the person to find
        /// </summary>
        public string CountyCode { get; set; }

        /// <summary>
        /// Address country code of the person to find. Should only be specified if not the home country of the institution.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Country code of the citizenship country of the person to find
        /// </summary>
        public string CitizenshipCountryCode { get; set; }

        /// <summary>
        /// Government identification number; e.g. Social Security Number, Social Insurance Number
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// Creates a new PersonMatchCriteriaInstantEnrollment
        /// </summary>
        public PersonMatchCriteriaInstantEnrollment(string firstName, string lastName)
        {
            // First and Last names are required at a minimum
            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("First and Last name are required for person matching", "firstName");
            }

            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("First and Last name are required for person matching", "lastName");
            }

            FirstName = firstName;
            LastName = lastName;
            AddressLines = new List<string>();
        }

    }
}
