/* Copyright 2014-2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This is emergency information for a person.
    /// </summary>
    public class EmergencyInformation
    {
        /// <summary>
        /// This is the ID of the person.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// This is the name of the person.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// This is the information about one point of contact for a person, which could be someone to contact in case of
        /// emergency or someone to notify if the person is missing (or both). It includes the contact's name, phone numbers, 
        /// relationship, effective date, and indicators of whether to contact for an emergency or if missing.
        /// <see cref="EmergencyContact"/>
        /// </summary>
        public List<EmergencyContact> EmergencyContacts { get; set; }

        /// <summary>
        /// Free-form text information about a person's insurance. This can be multi-line, where the user
        /// has formatted it similarly to what appears on the insurance card. Line breaks will be maintained
        /// as entered by the user (as much as possible). Each line is expected to be 40 characters or less. 
        /// Any line that exceeds 40 characters will be broken into multiple lines of 40 characters or less.
        /// </summary>
        public string InsuranceInformation { get; set; }

        /// <summary>
        /// Free-form text information about a person's hospital preference. Maximum length is 50 characters.
        /// </summary>
        public string HospitalPreference { get; set; }

        /// <summary>
        /// The public getter for the Health Conditions.
        /// </summary>
        public List<string> HealthConditions { get; set; }

        /// <summary>
        /// Date when the person last confirmed this information to be accurate and current.
        /// </summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>
        /// The opt out flag based on whether the user has chosen not to provide emergency contact info.
        /// </summary>
        public bool OptOut { get; set; }

        /// <summary>
        /// Free-form text containing self-disclosed additional information that would be needed in case of emergency.
        /// This can be multi-line, and the line breaks will be maintained as entered by the user (as much as possible).
        /// Each line of text is expected to be 74 characters or less. Any line that exceeds 74 characters will be 
        /// broken into multiple lines of 74 characters or less.
        /// </summary>
        public string AdditionalInformation { get; set; }

        /// <summary>
        /// Privacy status code
        /// </summary>
        public string PrivacyStatusCode { get; set; }

    }
}
