// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Contains the fields required to create a new proxy user
    /// </summary>
    [Serializable]
    public class PersonProxyUser: PersonBase
    {
        /// <summary>
        /// The list of phone numbers for this person
        /// </summary>
        public ReadOnlyCollection<Phone> Phones { get; private set; }

        /// <summary>
        /// The list of former names for this person
        /// </summary>
        public ReadOnlyCollection<PersonName> FormerNames { get; private set; }

        /// <summary>
        /// Constructs a <see cref="PersonProxyUser"/>
        /// </summary>
        /// <param name="id">The identifier for the person.  Optional.</param>
        /// <param name="firstName">The given name of the person.  Mandatory.</param>
        /// <param name="lastName">The family name of the person.  Mandatory.</param>
        /// <param name="emails">The list of email addresses for the person.  Optional.</param>
        /// <param name="phones">The list of phone numbers for the person.  Optional.</param>
        /// <param name="names">The list of former names for the person.  Optional.</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        public PersonProxyUser(string id, string firstName, string lastName, IEnumerable<EmailAddress> emails, IEnumerable<Phone> phones, IEnumerable<PersonName> names, string privacyStatusCode = null)
            : base(id, lastName, privacyStatusCode)
        {
            if (string.IsNullOrEmpty(firstName))
            {
                throw new ArgumentNullException("firstName");
            }
            if (emails == null)
            {
                throw new ArgumentNullException("email");
            }
            if (!emails.Any())
            {
                throw new ArgumentException("List must contain at least one email address", "email");
            }
            FirstName = firstName;
            Phones = new ReadOnlyCollection<Phone>(phones == null? new List<Phone>() : phones.ToList());
            FormerNames = new ReadOnlyCollection<PersonName>(names == null? new List<PersonName>() : names.ToList());
            foreach (EmailAddress x in emails)
            {
                this.AddEmailAddress(x);
            }
        }
    }
}
