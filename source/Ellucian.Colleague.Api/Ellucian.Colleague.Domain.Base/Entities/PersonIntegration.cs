// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonIntegration : Person
    {
        /// <summary>
        /// Visa information for a person
        /// </summary>
        public PersonVisa Visa { get; set; }
        /// <summary>
        /// Passport information for a person
        /// </summary>
        public PersonPassport Passport { get; set; }
        /// <summary>
        /// Drivers License information for a person
        /// </summary>
        public PersonDriverLicense DriverLicense { get; set; }
        /// <summary>
        /// Other Identity Documents for a person
        /// </summary>
        public List<PersonIdentityDocuments> IdentityDocuments { get; set; }
        /// <summary>
        /// Language ISO codes of a person
        ///
        /// This property is used only for Put/Post of persons.  It is used because the UPDATE.PERSON.INTEGRATION CTX
        /// was already written to take in Language ISO codes and convert to Colleague Language codes.
        /// (Otherwise, the Put/post would instead convert/map to PrimaryLanguage and SecondaryLanguages in PersonBase entity.)
        /// 
        /// </summary>
        public List<PersonLanguage> Languages { get; private set; }
        /// <summary>
        /// Alien or Citizenship Status of a person
        /// </summary>
        public string AlienStatus { get; set; }
        /// <summary>
        /// Denomination of a person
        /// </summary>
        public string Religion { get; set; }
        /// <summary>
        /// Country of birth for a person
        /// </summary>
        public string BirthCountry { get; set; }
        /// <summary>
        /// The list of former names (name history) for this person
        /// </summary>
        public List<PersonName> FormerNames { get; set; }
        /// <summary>
        /// Person's Citizenship Country Code
        /// </summary>
        public string Citizenship { get; set; }
        /// <summary>
        /// Person's Interest Codes
        /// </summary>
        public List<string> Interests { get; set; }
        /// <summary>
        /// Privacy Status of a person
        /// </summary>
        public PrivacyStatusType PrivacyStatus { get; set; }
        /// <summary>
        /// Privacy Status Code override from readonly item in base person
        /// </summary>
        public string PrivacyStatusCode { get; set; }
        /// <summary>
        /// Preferred Name Type of a person
        /// </summary>
        public string PreferredNameType { get; set; }
        /// <summary>
        /// Addresses for a person
        /// </summary>
        public List<Address> Addresses { get; set; }
        /// <summary>
        /// Phone numbers for a person
        /// </summary>
        public List<Phone> Phones { get; private set; }
        /// <summary>
        /// Social Media used by this person
        /// </summary>
        public List<SocialMedia> SocialMedia { get; private set; }
        /// <summary>
        /// Roles a person may have within the context of the data
        /// </summary>
        public List<PersonRole> Roles { get; private set; }
        /// <summary>
        /// Single Ethnicity where Person has multiple
        /// </summary>
        public string EthnicityCode { get; set; }

        public List<string> ProfessionalAbbreviations { get; set; }

        public PersonIntegration(string id, string lastName)
            : base(id, lastName)
        {
            Languages = new List<PersonLanguage>();
            FormerNames = new List<PersonName>();
            Interests = new List<string>();
            Phones = new List<Phone>();
            SocialMedia = new List<SocialMedia>();
            Roles = new List<PersonRole>();
            ProfessionalAbbreviations = new List<string>();
        }

        public void AddPersonLanguage(Entities.PersonLanguage personLanguage)
        {
            if (personLanguage == null)
            {
                throw new ArgumentNullException("personLanguage", "Person Language Object must be specified");
            }
            if ((personLanguage.IsPrimary == true) && (Languages.Where(f => f.IsPrimary == true).Count() > 0))
            {
                throw new ArgumentException("There can only be one primary language");
            }
            if (!Languages.Contains(personLanguage))
            {
                Languages.Add(personLanguage);
            }
        }

        public void AddPhone(Entities.Phone personPhone)
        {
            if (personPhone == null)
            {
                throw new ArgumentNullException("personPhones", "Person Phone Object must be specified");
            }
            if (!Phones.Contains(personPhone))
            {
                Phones.Add(personPhone);
            }
        }

        public void AddSocialMedia(Entities.SocialMedia personSocialMedia)
        {
            if (personSocialMedia == null)
            {
                throw new ArgumentNullException("personSocialMedia", "Person Social Media Object must be specified");
            }
            if (!SocialMedia.Contains(personSocialMedia))
            {
                SocialMedia.Add(personSocialMedia);
            }
        }

        public void AddRole(Entities.PersonRole personRole)
        {
            if (personRole == null)
            {
                throw new ArgumentNullException("personRoles", "Person Role Object must be specified");
            }
            if (!Roles.Contains(personRole))
            {
                Roles.Add(personRole);
            }
        }
    }
}
