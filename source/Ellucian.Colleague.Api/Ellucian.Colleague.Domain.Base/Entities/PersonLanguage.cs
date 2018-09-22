// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonLanguage
    {
        public string PersonId { get; private set; }
        /// <summary>
        /// Unique identifier for Person record
        /// </summary>
        public string PersonGuid { get; set; }
        /// <summary>
        /// Language code defining the person's language
        /// </summary>
        public string Code { get; private set; }
        /// <summary>
        /// Language preference of either Primary or Secondary
        /// </summary>
        public LanguagePreference Preference { get; set; }
        /// <summary>
        /// Flag to indicate if a specific language is primary.  Only one primary is allowed.
        /// </summary>
        public bool IsPrimary { get { return (Preference == LanguagePreference.Primary); } }

        public PersonLanguage(string personId, string code, Entities.LanguagePreference preference = Entities.LanguagePreference.Secondary)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "person ID must be specified");
            }
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "language code must be specified");
            }
            PersonId = personId;
            Code = code;
            Preference = preference;
        }
    }
}
