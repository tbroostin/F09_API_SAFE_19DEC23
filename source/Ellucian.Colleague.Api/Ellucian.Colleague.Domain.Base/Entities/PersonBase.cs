// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Class for Base PERSON records
    /// </summary>
    [Serializable]
    public class PersonBase
    {
        #region Properties

        // Required fields

        private readonly string _LastName;

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get { return _LastName; } }

        // Non-required fields
        
        private string _Id;
        public string Id
        {
            get { return _Id; }
            set
            {
                if (string.IsNullOrEmpty(_Id))
                {
                    _Id = value;
                }
                else
                {
                    throw new InvalidOperationException("Person Id cannot be changed");
                }
            }
        }

        /// <summary>
        /// GUID for the person; not required, but cannot be changed once assigned.
        /// </summary>
        private string _Guid;
        public string Guid
        {
            get { return _Guid; }
            set
            {
                if (string.IsNullOrEmpty(_Guid))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _Guid = value.ToLowerInvariant();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Cannot change value of Guid.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        /// <value>
        /// The person's prefix.
        /// </value>
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        /// <value>
        /// The person's middle name.
        /// </value>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the birth/maiden last name.
        /// </summary>
        /// <value>
        /// The birth/maiden last name.
        /// </value>
        public string BirthNameLast { get; set; }

        /// <summary>
        /// Gets or sets the birth/maiden first name.
        /// </summary>
        /// <value>
        /// The birth/maiden first name.
        /// </value>
        public string BirthNameFirst { get; set; }

        /// <summary>
        /// Gets or sets the birth/maiden middle name.
        /// </summary>
        /// <value>
        /// The person's birth middle name.
        /// </value>
        public string BirthNameMiddle { get; set; }

        /// <summary>
        /// Gets or sets the chosen last name.
        /// </summary>
        /// <value>
        /// The chosen last name.
        /// </value>
        public string ChosenLastName { get; set; }

        /// <summary>
        /// Gets or sets the chosen first name.
        /// </summary>
        /// <value>
        /// The chosen first name.
        /// </value>
        public string ChosenFirstName { get; set; }

        /// <summary>
        /// Gets or sets the chosen middle name.
        /// </summary>
        /// <value>
        /// The person's chosen middle name.
        /// </value>
        public string ChosenMiddleName { get; set; }

        /// <summary>
        /// Name to use for this person if a "mail label" type of name is requested as the display name 
        /// in the Name Hierarchy calculation of name.  This will override the normal formal name default of Prefix First MI Last Suffix.
        /// </summary>
        public string MailLabelNameOverride { get; set; }
        
        /// <summary>
        /// Name to use for this person if a "preferred" type of name is requested as the display name 
        /// in the Name Hierarchy calculation of name.  This will override the normal formal name default of Prefix First MI Last Suffix.
        /// </summary>
        public string PreferredNameOverride { get; set; }

        /// <summary>
        /// List of formatted names by type for a person if special formatted names were entered for the person.
        /// Used in the Name Hierarchy calculation of names
        /// </summary>
        private List<PersonFormattedName> _FormattedNames = new List<PersonFormattedName>();
        public List<PersonFormattedName> FormattedNames { get { return _FormattedNames; } }

        /// <summary>
        /// Ethnicities
        /// </summary>
        public List<EthnicOrigin> Ethnicities { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>
        /// The person's suffix.
        /// </value>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the nickname.
        /// </summary>
        /// <value>
        /// The person's nickname.
        /// </value>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        /// <value>
        /// The person's gender.
        /// </value>
        public string Gender { get; set; }

        /// <summary>
        /// Gets or sets the personal pronoun code.
        /// </summary>
        /// <value>
        /// The personal pronoun code.
        /// </value>
        public string PersonalPronounCode { get; set; }

        /// <summary>
        /// Gets or sets the gender identity code.
        /// </summary>
        /// <value>
        /// The gender identity code.
        /// </value>
        public string GenderIdentityCode { get; set; }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The person's birth date.
        /// </value>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the government ID.
        /// </summary>
        /// <value>
        /// The person's government ID.
        /// </value>
        public string GovernmentId { get; set; }

        /// <summary>
        /// Gets or sets the deceased date.
        /// </summary>
        /// <value>
        /// The person's deceased date.
        /// </value>
        public DateTime? DeceasedDate { get; set; }

        /// <summary>
        /// Gets or sets the martial state.
        /// </summary>
        /// <value>
        /// The person's marital state.
        /// </value>
        public MaritalState? MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the marital status.
        /// </summary>
        /// <value>
        /// The person's marital status.
        /// </value>
        public string MaritalStatusCode { get; set; }

        /// <summary>
        /// Gets or sets list of Races
        /// </summary>
        public List<string> RaceCodes { get; set; }

        /// <summary>
        /// Gets or sets list of Ethnicities
        /// </summary>
        public List<string> EthnicCodes { get; set; }

        /// <summary>
        /// Gets or sets the name of the preferred.
        /// </summary>
        /// <value>
        /// The name of the preferred.
        /// </value>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets corp indicator
        /// </summary>
        public string PersonCorpIndicator { get; set; }

        // <summary>
        /// Gets or sets military status.
        /// </summary>
        public string MilitaryStatus { get; set; }

        /// <summary>
        /// A type of a visa
        /// </summary>
        public string VisaType { get; set; }

        /// <summary>
        /// Privacy status code
        /// </summary>
        private readonly string _privacyStatusCode;
        public string PrivacyStatusCode { get { return _privacyStatusCode; } }

        /// <summary>
        /// All Email Addresses for the person. 
        /// </summary>
        private List<EmailAddress> _EmailAddresses = new List<EmailAddress>();
        public List<EmailAddress> EmailAddresses { get { return _EmailAddresses; } }

        /// <summary>
        /// All alternate IDs for a person. 
        /// </summary>
        private List<PersonAlt> _PersonAltIds = new List<PersonAlt>();
        public List<PersonAlt> PersonAltIds { get { return _PersonAltIds; } }

        // Date time stamp of last change to person data (checked as version control before updates)
        public DateTimeOffset LastChangedDateTime { get; set; }

        /// <summary>
        /// Name that should be used when displaying a person's name on reports and forms.
        /// This property is based on a Name Address Hierarcy and will be null if none is provided.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }

        /// <summary>
        /// Person has either a deceased status, either verified or unverified. This may be true even if
        /// the DeceasedDate is not set.
        /// </summary>
        public bool IsDeceased { get; set; }
        
        /// <summary>
        /// Primary language of a person
        /// </summary>
        public string PrimaryLanguage { get; set; }

        /// <summary>
        /// Secondary languages of a person
        /// </summary>
        public List<string> SecondaryLanguages { get; set; }

        #endregion
        
        /// <summary>
        /// Add a formatted name to the person
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="formattedName">name</param>
        public void AddFormattedName(string type, string formattedName)
        {
            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentNullException("type");
            }
            if (string.IsNullOrEmpty(formattedName))
            {
                throw new ArgumentNullException("formattedName");
            }
            if (_FormattedNames.Where(f => f.Type == type).Count() == 0)
            {
                _FormattedNames.Add(new PersonFormattedName(type, formattedName));
            }
        }

        #region Email Address Methods

        /// <summary>
        /// Used to add email addresses to EmailAddresses.
        /// Can't have the same type twice, or more than one item marked preferred. 
        /// But can have the same email address twice.
        /// </summary>
        /// <param name="emailAddress">Email address to add.</param>
        public void AddEmailAddress(EmailAddress emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException("emailAddress", "Email address must be specified");
            }
            if (_EmailAddresses.Where(f => f.TypeCode == emailAddress.TypeCode).Count() > 0)
            {
                throw new ArgumentException("There can only be one email address of type " + emailAddress.TypeCode);
            }
            if ((emailAddress.IsPreferred == true) && (_EmailAddresses.Where(f => f.IsPreferred == true).Count() > 0))
            {
                throw new ArgumentException("There can only be one preferred email address");
            }
            _EmailAddresses.Add(emailAddress);
        }

        /// <summary>
        /// The email or emails for the person of a certain type. If a type is not specified, an empty list of strings is returned.  
        /// </summary>
        /// <param name="emailTypeCode">Type of emails that should be returned.</param>
        /// <returns>A list of strings for each email address that matches that type.</returns>
        public IEnumerable<string> GetEmailAddresses(string emailTypeCode)
        {
            List<string> personEmails = new List<String>();
            if (!string.IsNullOrEmpty(emailTypeCode))
            {
                var emails = _EmailAddresses.Where(em => em.TypeCode == emailTypeCode).Select(e => e.Value);
                if (emails != null && emails.Count() > 0)
                {
                    personEmails.AddRange(emails);
                }
            }
            return personEmails;
        }

        /// <summary>
        /// The person's preferred email address 
        /// This will be the first email address with the preferred email flag set to true.
        /// If none of the emails are preferred we will default to the first email in the list, even if it isn't flagged as a preferred.
        /// </summary>
        public EmailAddress PreferredEmailAddress
        {
            get
            {
                if (EmailAddresses.Count() > 0)
                {
                    EmailAddress preferredEmail = EmailAddresses.Where(email => email.IsPreferred).FirstOrDefault();
                    if (preferredEmail == null)
                    {
                        preferredEmail = EmailAddresses.FirstOrDefault();
                    }
                    return preferredEmail;
                }
                return null;
            }
        }

        #endregion

        #region Person Alt Id Methods

        /// <summary>
        /// Used to add person alternate Ids to a person
        /// </summary>
        /// <param name="personAlt"><see cref="PersonAlt">Person Alt</see></param>
        public void AddPersonAlt(PersonAlt personAlt)
        {
            if (personAlt == null)
            {
                throw new ArgumentNullException("personAlt");
            }
            if (_PersonAltIds.Where(f => f.Equals(personAlt)).Count() > 0)
            {
                throw new ArgumentException("Person Alt ID already exists in this list");
            }
            _PersonAltIds.Add(personAlt);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a PersonBase domain object
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        /// <exception cref="System.ArgumentNullException">
        /// personId
        /// or
        /// lastName
        /// </exception>
        public PersonBase(string personId, string lastName, string privacyStatusCode = null)
            {
            if (String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentNullException("lastName");
            }
            _Id = personId;
            _LastName = lastName;
            _privacyStatusCode = privacyStatusCode;
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
            PersonBase other = obj as PersonBase;
            if (other == null)
            {
                return false;
            }
            return other.Id.Equals(Id);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
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
