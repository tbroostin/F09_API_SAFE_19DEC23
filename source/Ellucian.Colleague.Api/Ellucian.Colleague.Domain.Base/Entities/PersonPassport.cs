// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonPassport
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
        /// Passport number
        /// </summary>
        public string PassportNumber { get; private set; }
        /// <summary>
        /// Issuing Country
        /// </summary>
        public string IssuingCountry { get; set; }
        /// <summary>
        /// Doc Type
        /// </summary>
        public string DocType { get; set; }
        /// <summary>
        /// Expiration Date of the passport
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        public PersonPassport(string personId, string passportNumber)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "person ID must be specified");
            }
            if (string.IsNullOrEmpty(passportNumber))
            {
                throw new ArgumentNullException("passportNumber", "passportNumber must be specified");
            }
            PersonId = personId;
            PassportNumber = passportNumber;
        }
    }
}
