/* Copyright 2014-2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A class to encapsulate a person's emergency contacts and related emergency information.
    /// </summary>
    [Serializable]
    public class EmergencyInformation
    {

        // Note: The setters of the properties in the domain entity include code to verify length limits. Yes, these edits are 
        // also in the Colleague transaction and in the self-service code, but should be here in addition. 
        // If the length ever changes, this code will need to change also.

        // The hospital preference text has a maximum length of 50 characters.
        int maximumHospitalPreferenceLineLength = 50;


        /// <summary>
        /// The person's ID.
        /// </summary>
        private readonly string personId;

        /// <summary>
        /// Publicly accessible getter for the person ID.
        /// </summary>
        public string PersonId { get { return personId; } }

        /// <summary>
        /// The list of emergency contacts for this person.
        /// </summary>
        private readonly List<EmergencyContact> emergencyContacts = new List<EmergencyContact>();

        /// <summary>
        /// The public getter for the Emergency Contacts.
        /// </summary>
        public ReadOnlyCollection<EmergencyContact> EmergencyContacts { get; private set; }

        /// <summary>
        /// Free-form text information about a person's insurance. This can be multi-line, where the user
        /// has formatted it similarly to what appears on the insurance card. Line breaks will be maintained
        /// as entered by the user (as much as possible). Each line is expected to be 40 characters or less. 
        /// Any line that exceeds 40 characters will be broken into multiple lines of 40 characters or less.
        /// </summary>
        // It was recommended in peer review that this field be validated for maximum length as part of the
        // "set" and an error be thrown if the text was too long. However, the Colleague transaction will 
        // break the lines as needed in order to have each line fit within the length limit, so it was 
        // decided NOT to enforce length limits here.
        public string InsuranceInformation { get; set; }        

        /// <summary>
        /// Free-form text containing self-disclosed additional information that would be needed in case of emergency.
        /// This can be multi-line, and the line breaks will be maintained as entered by the user (as much as possible).
        /// Each line of text is expected to be 74 characters or less. Any line that exceeds 74 characters will be 
        /// broken into multiple lines of 74 characters or less.
        /// </summary>
        // It was recommended in peer review that this field be validated for maximum length as part of the
        // "set" and an error be thrown if the text was too long. However, the Colleague transaction will 
        // break the lines as needed in order to have each line fit within the length limit, so it was 
        // decided NOT to enforce length limits here.
        public string AdditionalInformation { get; set; } 

        /// <summary>
        /// A person's hospital preference. 
        /// </summary>
        private string hospitalPreference;

        /// <summary>
        /// Free-form text information about a person's hospital preference. The maximum length
        /// is 50 characters.
        /// <exception cref="ArgumentException">Hospital preference must contain no more than 50 characters.</exception>
        /// </summary>
        public string HospitalPreference
        {
            get
            {
                return hospitalPreference;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the hospital preference text that was provided does not
                    // exceed the maximum length. (This is a single value input - no line breaks.)
                    if (value.Length > maximumHospitalPreferenceLineLength)
                    {
                        throw new ArgumentException("Hospital preference must contain no more than " + maximumHospitalPreferenceLineLength + " characters.");
                    }
                }
                hospitalPreference = value;
            }
        }

        /// <summary>
        /// The list of health conditions for this person.
        /// </summary>
        private readonly List<string> healthConditions = new List<string>();

        /// <summary>
        /// The public getter for the Health Conditions.
        /// </summary>
        public ReadOnlyCollection<string> HealthConditions { get; private set; }

        /// <summary>
        /// The date the emergency information was last confirmed as accurate and current.
        /// </summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>
        /// The opt out flag based on whether the user has chosen not to provide emergency contact info.
        /// </summary>
        public bool OptOut { get; set; }

        /// <summary>
        /// Constructor for an EmergencyInformation object.
        /// </summary>
        /// <param name="personId">Pass the person's ID.</param>
        /// <exception cref="ArgumentNullException">Person ID is required.</exception>
        public EmergencyInformation(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "personId is required in order to instantiate an EmergencyInformation object");
            }

            this.personId = personId;
            this.EmergencyContacts = emergencyContacts.AsReadOnly();

            this.AdditionalInformation = string.Empty;
            this.InsuranceInformation = string.Empty;
            this.HospitalPreference = string.Empty;

            this.HealthConditions = healthConditions.AsReadOnly();

            this.ConfirmedDate = null;
            this.OptOut = false;
        }

        /// <summary>
        /// Add an emergencyContact object, which contains the data about whom to contact in case of an emergency (name, phone numbers, etc.).
        /// </summary>
        /// <param name="emergencyContact">Pass in an emergencyContact object.</param>
        public void AddEmergencyContact(EmergencyContact emergencyContact)
        {
            if (emergencyContact == null)
            {
                throw new ArgumentNullException("emergencyContact", "Cannot add a null emergency contact to a person's emergency information");
            }
            this.emergencyContacts.Add(emergencyContact);
        }



        /// <summary>
        /// Add a healthCondition, which is a code indicating a health condition for this
        /// person that may be important to be aware of in case of an emergency.
        /// </summary>
        /// <param name="healthCondition">Pass in a code for a healthCondition.</param>
        public void AddHealthCondition(string healthCondition)
        {
            if (string.IsNullOrEmpty(healthCondition))
            {
                throw new ArgumentNullException("healthCondition", "Cannot add a null health condition to a person's emergency information");
            }
            this.healthConditions.Add(healthCondition);
        }
    }
}
