// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.InstantEnrollment
{
    /// <summary>
    /// The person attributes that will be sent while doing registration for the person in Instant Enrollment. 
    /// </summary>
    public class InstantEnrollmentPersonDemographic
    {
        /// <summary>
        /// First name of person.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of person.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Middle name of person.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The prefix of the person.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the person.
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// The gender of the person.
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The birthdate of the person.
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// The email address of the person.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Address lines of the person.
        /// </summary>
        public List<string> AddressLines{ get; set; }

        /// <summary>
        /// Address city of the person.
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Address State of the person.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Address zip code of the person.
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Address county code of the person.
        /// </summary>
        public string CountyCode { get; set; }

        /// <summary>
        /// Address country code of the person.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Country code of the citizenship country of the person.
        /// </summary>
        public string CitizenshipCountryCode { get; set; }

        /// <summary>
        /// The phone numbers of the person.
        /// </summary>
        public List<Phone> PersonPhones { get; set; }

        /// <summary>
        /// The list of ethnic groups to which the student belongs.
        /// </summary>
        public List<string> EthnicGroups { get; set; }

        /// <summary>
        /// The list of racial groups to which the student belongs. 
        /// </summary>
        public List<string> RacialGroups { get; set; }

        /// <summary>
        /// Government identification number; e.g. Social Security Number, Social Insurance Number
        /// </summary>
        public string GovernmentId { get; set; }
    }
}
