// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Contains the demographic information needed to identify a proxy, a list of possible match candidates, and the granted permissions
    /// </summary>
    [Serializable]
    public class ProxyCandidate
    {

        /// <summary>
        /// The unique identifier of the proxy candidate.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The grantor of the privileges.  Required.
        /// </summary>
        public string ProxySubject { get; private set; }

        /// <summary>
        /// The relationship of the candidate to the grantor.  Required.
        /// </summary>
        public string RelationType { get; private set; }

        /// <summary>
        /// The list of granted permissions.  Required.
        /// </summary>
        public IEnumerable<string> GrantedPermissions { get; private set; }

        /// <summary>
        /// Candidate's name prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Candidate's given name.  Required.
        /// </summary>
        public string FirstName { get; private set; }

        /// <summary>
        /// Candidate's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Candidate's family name.  Required.
        /// </summary>
        public string LastName { get; private set; }

        /// <summary>
        /// Candidate's name suffix
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Candidate's email address.  Required
        /// </summary>
        public string EmailAddress { get; private set; }

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
        /// The list of possible matches for this candidate.  Required.
        /// </summary>
        public IEnumerable<PersonMatchResult> ProxyMatchResults { get; private set; }

        public ProxyCandidate(string subject, string relationship, IEnumerable<string> perms, string first, string last, string email, IEnumerable<PersonMatchResult> matches)
        {
            if (string.IsNullOrEmpty(subject))
            {
                throw new ArgumentNullException("subject");
            }
            if (string.IsNullOrEmpty(relationship))
            {
                throw new ArgumentNullException("relationship");
            }
            if (perms == null || !perms.Any()){
                throw new ArgumentNullException("perms");
            }
            if (string.IsNullOrEmpty(first))
            {
                throw new ArgumentNullException("first");
            }
            if (string.IsNullOrEmpty(last))
            {
                throw new ArgumentNullException("last");
            }
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email");
            }
            if (matches == null || !matches.Any()){
                throw new ArgumentNullException("matches");
            }

            ProxySubject = subject;
            RelationType = relationship;
            GrantedPermissions = perms;
            FirstName = first;
            LastName = last;
            EmailAddress = email;
            ProxyMatchResults = matches;
        }
    }
}
