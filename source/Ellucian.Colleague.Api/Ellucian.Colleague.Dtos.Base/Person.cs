/* Copyright 2014-2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A base of any person-type entities, such as Student or Advisor
    /// </summary>
    public class Person
    {
        /// <summary>
        ///  Unique system ID of this person
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Person's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Person's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Person's middle name
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Person's birth/maiden last name
        /// </summary>
        public string BirthNameLast { get; set; }
        /// <summary>
        /// Person's birth/maiden first name
        /// </summary>
        public string BirthNameFirst { get; set; }
        /// <summary>
        /// Person's birth/maiden middle name
        /// </summary>
        public string BirthNameMiddle { get; set; }
        /// <summary>
        /// Calculated preferred name, based on this person's stated preference
        /// </summary>
        public string PreferredName { get; set; }
        /// <summary>
        /// Address of this person, chosen from all addresses on file based on person's stated preference
        /// </summary>
        public IEnumerable<string> PreferredAddress { get; set; }
        /// <summary>
        /// Prefixes
        /// </summary>
        public string Prefix { get; set; }
        /// <summary>
        /// Suffixes
        /// </summary>
        public string Suffix { get; set; }
        /// <summary>
        /// Gender (Male or Female)
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Date of Birth
        /// </summary>
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// Social Security or similar Government Id
        /// </summary>
        public string GovernmentId { get; set; }
        /// <summary>
        /// List of Races
        /// </summary>
        public List<string> RaceCodes { get; set; }
        /// <summary>
        /// List of Ethnicities
        /// </summary>
        public List<string> EthnicCodes { get; set; }
        /// <summary>
        /// Specific Ethnicities (Unknown, HispanicOrLatino, Asian, BlackOrAfricanAmerican, 
        /// NativeHawaiianOrOtherPacificIslander or White)
        /// </summary>
        public List<EthnicOrigin> Ethnicities { get; set; }
        /// <summary>
        /// Marital Status of the person (Single, Married, Divorced, Widowed)
        /// </summary>
        public MaritalState? MaritalStatus { get; set; }

        /// <summary>
        /// Privacy status code
        /// </summary>
        public string PrivacyStatusCode { get; set; }

        /// <summary>
        /// Personal Pronoun Code indicating person's perferred manner of address
        /// </summary>
        public string PersonalPronounCode { get; set; }
    }
}
