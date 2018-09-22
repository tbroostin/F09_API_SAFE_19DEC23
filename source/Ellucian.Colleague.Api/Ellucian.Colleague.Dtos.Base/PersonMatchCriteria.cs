// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The literal values used for person lookup
    /// </summary>
    public class PersonMatchCriteria
    {
        /// <summary>
        /// Identifier of the record specifying the match criteria to use for this lookup
        /// </summary>
        public string MatchCriteriaIdentifier { get; set; }

        /// <summary>
        /// List of possible names for the person to find
        /// </summary>
        public IEnumerable<PersonName> MatchNames { get; set; }

        /// <summary>
        /// A government identifier for the person to find
        /// </summary>
        public string GovernmentId { get; set; }

        /// <summary>
        /// The gender of the person to find
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// The birthdate of the person to find
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// The email address of the person to find
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The phone number of the person to find
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// The phone extension of the person to find
        /// </summary>
        public string PhoneExtension { get; set; }

        /// <summary>
        /// The prefix of the person to find
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The suffix of the person to find
        /// </summary>
        public string Suffix { get; set; }
    }
}
