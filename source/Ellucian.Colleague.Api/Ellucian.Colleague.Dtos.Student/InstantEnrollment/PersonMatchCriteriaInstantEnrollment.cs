// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// The person attributes that will be matched to existing person records in Colleague, using the 
    /// ELF duplicate criteria configured for Instant Enrollment person matching.
    /// </summary>
    public class PersonMatchCriteriaInstantEnrollment
    {
        /// <summary>
        /// First name of person to find. The standard capitalization processing that Colleague applies to names upon input will be 
        /// applied to this name before matching with existing persons.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of person to find. The standard capitalization processing that Colleague applies to names upon input will be 
        /// applied to this name before matching with existing persons.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Middle name of person to find. The standard capitalization processing that Colleague applies to names upon input will be 
        /// applied to this name before matching with existing persons.
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
        public IEnumerable<string> AddressLines{ get; set; }

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
    }
}
