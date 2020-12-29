// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment
{
    /// <summary>
    /// The person attributes that will be sent while doing registration for the person in Instant Enrollment. 
    /// </summary>
    [Serializable]
    public class InstantEnrollmentPersonDemographic
    {
        /// <summary>
        /// First name of person
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Last name of person
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Middle name of person
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The prefix of the person
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the person
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The gender of the person
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The birthdate of the person
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// The email address of the person
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Address lines of the person 
        /// </summary>
        public IEnumerable<string> AddressLines { get; set; }

        /// <summary>
        /// Address city of the peron
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Address State of the person
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Address zip code of the person 
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Address county code of the person 
        /// </summary>
        public string CountyCode { get; set; }

        /// <summary>
        /// Address country code of the person. Only be specified if not the home country of the institution.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Country code of the citizenship country of the person 
        /// </summary>
        public string CitizenshipCountryCode { get; set; }

        /// <summary>
        /// Creates a new PersonDemographicInstantEnrollment
        /// </summary>
        /// 
        /// <summary>
        /// The phone numbers of the person.
        /// </summary>
        public IEnumerable<Phone> PersonPhones { get; set; }

        private readonly List<string> _ethnicGroups;
        /// <summary>
        /// The list of ethnic groups to which the student belongs.
        /// </summary>
        public ReadOnlyCollection<string> EthnicGroups { get; private set; }

        private readonly List<string> _racialGroups;
        /// <summary>
        /// The list of racial groups to which the student belongs. 
        /// </summary>
        public ReadOnlyCollection<string> RacialGroups { get; private set; }

        /// <summary>
        /// Government identification number; e.g. Social Security Number, Social Insurance Number
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// Create a new <see cref="InstantEnrollmentPersonDemographic"/> object
        /// </summary>
        /// <param name="firstName">Required; student's first name</param>
        /// <param name="lastName">Required; student's last name</param>
        public InstantEnrollmentPersonDemographic(string firstName, string lastName)
        {
            // First and Last names are required at a minimum
            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName","First and Last name are required for person");
            }

            if (string.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName","First and Last name are required for person");
            }

            FirstName = firstName;
            LastName = lastName;

            AddressLines = new List<string>();
            PersonPhones = new List<Phone>();
            _ethnicGroups = new List<string>();
            _racialGroups = new List<string>();
            RacialGroups = _racialGroups.AsReadOnly();
            EthnicGroups = _ethnicGroups.AsReadOnly();
        }

        /// <summary>
        /// Add a racial group to the <see cref="InstantEnrollmentPersonDemographic"/>
        /// </summary>
        /// <param name="ethnicGroup">Racial group to add</param>
        public void AddRacialGroup(string racialGroup)
        {
            if (racialGroup != null && !_racialGroups.Contains(racialGroup))
            {
                _racialGroups.Add(racialGroup);
            }
        }

        /// <summary>
        /// Add an ethnic group to the <see cref="InstantEnrollmentPersonDemographic"/>
        /// </summary>
        /// <param name="ethnicGroup">Ethnic group to add</param>
        public void AddEthnicGroup(string ethnicGroup)
        {
            if (ethnicGroup != null && !_ethnicGroups.Contains(ethnicGroup))
            {
                _ethnicGroups.Add(ethnicGroup);
            }
        }
    }
}
