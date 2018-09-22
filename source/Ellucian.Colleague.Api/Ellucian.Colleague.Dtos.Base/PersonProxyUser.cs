//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Information about a proxy user
    /// </summary>
    public class PersonProxyUser
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
        /// List of person's phone numbers
        /// </summary>
        public List<Phone> Phones { get; set; }

        /// <summary>
        /// List of person's email addresses
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// List of person's former names
        /// </summary>
        public List<PersonName> FormerNames { get; set; }

        /// <summary>
        /// Privacy status code
        /// </summary>
        public string PrivacyStatusCode { get; set; }
    }
}
