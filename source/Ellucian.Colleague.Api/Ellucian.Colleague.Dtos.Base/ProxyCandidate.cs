// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Contains the demographic information needed to identify a proxy, a list of possible match candidates, and the granted permissions
    /// </summary>
    public class ProxyCandidate
    {

        /// <summary>
        /// The unique identifier of the proxy candidate.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The grantor of the privileges.
        /// </summary>
        public string ProxySubject { get; set; }

        /// <summary>
        /// The relationship of the candidate to the grantor.
        /// </summary>
        public string RelationType { get; set; }

        /// <summary>
        /// The list of granted permissions.
        /// </summary>
        public IEnumerable<string> GrantedPermissions { get; set; }

        /// <summary>
        /// Candidate's name prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Candidate's given name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Candidate's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Candidate's family name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Candidate's name suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Candidate's email address.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Candidate's phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Candidate's phone extension
        /// </summary>
        public string PhoneExtension { get; set; }

        /// <summary>
        /// Candidate's birth date
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Candidate's gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The type of the candidate's email address.
        /// </summary>
        public string EmailType { get; set; }

        /// <summary>
        /// The type of the candidate's phone number
        /// </summary>
        public string PhoneType { get; set; }

        /// <summary>
        /// Candidate's previous given name
        /// </summary>
        public string FormerFirstName { get; set; }

        /// <summary>
        /// Candidate's previous middle name
        /// </summary>
        public string FormerMiddleName { get; set; }

        /// <summary>
        /// Candidate's previous family name
        /// </summary>
        public string FormerLastName { get; set; }

        /// <summary>
        /// Candidate's government identifier
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// The list of possible matches for this candidate.
        /// </summary>
        public IEnumerable<PersonMatchResult> ProxyMatchResults { get; set; }
    }
}
