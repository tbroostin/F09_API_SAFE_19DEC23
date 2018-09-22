// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonDriverLicense
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
        /// License number
        /// </summary>
        public string LicenseNumber { get; private set; }
        /// <summary>
        /// Drivers license state
        /// </summary>
        public string IssuingState { get; set; }
        /// <summary>
        /// License expires on this date
        /// </summary>
        public DateTime? ExpireDate { get; set; }
        /// <summary>
        /// Doc Type
        /// </summary>
        public string DocType { get; set; }

        public PersonDriverLicense(string personId, string licenseNumber, DateTime? expiresOn = null)
        {
            //"$NEW.PERSON"
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "person ID must be specified");
            }
            if (string.IsNullOrEmpty(licenseNumber))
            {
                throw new ArgumentNullException("licenseNumber", "licenseNumber must be specified");
            }
            PersonId = personId;
            LicenseNumber = licenseNumber;
            ExpireDate = expiresOn;
        }
    }
}
