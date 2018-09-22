// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonVisa
    {
        /// <summary>
        /// Unique identifier of Foreign Person record
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// Colleague Person ID
        /// </summary>
        public string PersonId { get; private set; }
        /// <summary>
        /// Unique identifier for Person record
        /// </summary>
        public string PersonGuid { get; set; }
        /// <summary>
        /// The visa type identified on a foreign person
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// The visa number or identifier
        /// </summary>
        public string VisaNumber { get; set; }
        /// <summary>
        /// Date that the visa was requested
        /// </summary>
        public DateTime? RequestDate { get; set; }
        /// <summary>
        /// Date that the visa was issued
        /// </summary>
        public DateTime? IssueDate { get; set; }
        /// <summary>
        /// Date that the visa expires
        /// </summary>
        public DateTime? ExpireDate { get; set; }
        /// <summary>
        /// Date in which the foreign person entered the country
        /// </summary>
        public DateTimeOffset? EntryDate { get; set; }
        /// <summary>
        /// The country that issued the visa
        /// </summary>
        public string IssuingCountry { get; set; }

        public PersonVisa(string personId, string visaType)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "person ID must be specified");
            }
            if (string.IsNullOrEmpty(visaType))
            {
                throw new ArgumentNullException("visaType", "visa type must be specified");
            }
            PersonId = personId;
            Type = visaType;
        }
    }
}
