// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonIdentityDocuments
    {
        /// <summary>
        /// Colleague Person ID
        /// </summary>
        public string PersonId { get; private set; }
        /// <summary>
        /// Unique identifier for Person record
        /// </summary>
        public string PersonGuid { get; set; }
        /// <summary>
        /// Document number
        /// </summary>
        public string Number { get; private set; }
        /// <summary>
        /// Document Country (ISO Alpha-3)
        /// </summary>
        public string Country { get; set; }
        /// <summary>
        /// Document Region (ISO Region Codes)
        /// </summary>
        public string Region { get; set; }
        /// <summary>
        /// License expires on this date
        /// </summary>
        public DateTime? ExpireDate { get; set; }
        /// <summary>
        /// Doc Type
        /// </summary>
        public string DocType { get; set; }

        public PersonIdentityDocuments(string personId, string number, DateTime? expiresOn = null)
        {
            //"$NEW.PERSON"
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "person ID must be specified");
            }
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentNullException("number", "document number must be specified");
            }
            PersonId = personId;
            Number = number;
            ExpireDate = expiresOn;
        }
    }
}
