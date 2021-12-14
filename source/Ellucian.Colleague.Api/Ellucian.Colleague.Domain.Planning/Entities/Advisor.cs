// Copyright 2013-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Planning.Entities
{
    [Serializable]
    public class Advisor
    {
        // Required fields

        private readonly string _Id;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public string Id { get { return _Id; } }

        private readonly string _LastName;

        /// <summary>
        /// Gets the last name.
        /// </summary>
        public string LastName { get { return _LastName; } }

        // Non-required fields

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the name of the middle.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// All Email Addresses for the advisor 
        /// </summary>
        private List<EmailAddress> _EmailAddresses = new List<EmailAddress>();
        public List<EmailAddress> EmailAddresses { get { return _EmailAddresses; } }

        /// <summary>
        /// Name that should be used when displaying a person's name on reports and forms.
        /// This property is based on a Name Address Hierarcy and will be null if none is provided.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }

        /// <summary>
        /// Used to add email addresses to EmailAddresses 
        /// </summary>
        /// <param name="emailAddress">Email address to add.</param>
        public void AddEmailAddress(EmailAddress emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress", "Email address must be specified");
            }
            if (_EmailAddresses.Where(f => f.Equals(emailAddress)).Count() > 0)
            {
                throw new ArgumentException("Email address already exists in this list");
            }
            _EmailAddresses.Add(emailAddress);
        }

        /// <summary>
        /// Return the emails of a certain type. If a type is not specified, an empty list of strings is returned.  
        /// </summary>
        /// <param name="emailTypeCode">Type of emails that should be returned.</param>
        /// <returns>A list of strings for each email address that matches that type.</returns>
        public IEnumerable<string> GetEmailAddresses(string emailTypeCode)
        {
            List<string> advisorEmails = new List<String>();
            if (!string.IsNullOrEmpty(emailTypeCode))
            {
                var emails = _EmailAddresses.Where(em => em.TypeCode == emailTypeCode).Select(e => e.Value);
                if (emails != null && emails.Count() > 0)
                {
                    advisorEmails.AddRange(emails);
                }
            }
            return advisorEmails;
        }

        /// <summary>
        /// Name built from provided last, first, middle names. Used in place of PreferredName
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// All CURRENT advisees assigned to this advisor
        /// </summary>
        private List<string> _Advisees = new List<string>();
        public List<string> Advisees { get { return _Advisees; } }

        /// <summary>
        /// Add an advisee to this advisor's list of advisees.
        /// </summary>
        /// <param name="adviseeId"></param>
        public void AddAdvisee(string adviseeId)
        {
            if (adviseeId == null)
            {
                throw new ArgumentNullException("adviseeId", "Advisee cannot be null");
            }
            if (Advisees.Where(a => a.Equals(adviseeId)).Count() > 0)
            {
                throw new ArgumentException("Advisee already exists in this list");
            }
            _Advisees.Add(adviseeId);
        }

        /// <summary>
        /// Indicates whether this advisor is currently active. 
        /// </summary>
        public bool IsActive { get; set; }

        #region Constructor

        /// <summary>
        /// Create an Advisor domain object
        /// </summary>
        /// <param name="id">Advisor ID</param>
        /// <param name="lastName">Last Name</param>
        public Advisor(string id, string lastName)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("personId");
            }
            if (String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            _Id = id;
            _LastName = lastName;
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
            Advisor other = obj as Advisor;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        /// <summary>
        /// Returns a HashCode for this instance.
        /// </summary>
        /// <returns>
        /// A HashCode for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Id;
        }

        #endregion
    }
}
