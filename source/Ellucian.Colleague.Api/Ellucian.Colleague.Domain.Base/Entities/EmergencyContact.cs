/* Copyright 2014 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// An emergency contact for a person.
    /// </summary>
    [Serializable]
    public class EmergencyContact
    {
        // Note: The setters of the properties in the domain entity include code to verify length limits. Yes, these edits are 
        // also in the Colleague transaction and in the self-service code, but should be here in addition. 
        // If the length ever changes, this code will need to change also.

        // All phone numbers for an emergency contact have a maximum length of 20 characters.
        int maximumPhoneLength = 20;

        // The relationship text has a maximum length of 25 characters.
        int maximumRelationshipLength = 25;

        // The contact address has a maximum length of 75 characters.
        int maximumAddressLength = 75;

        // The contact name has a maximum length of 57 characters.
        int maximumContactNameLength = 57;


        /// <summary>
        /// The contact's name, as a simple string.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// Public getter for the contact's name.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// The contact's relationship to the person.
        /// </summary>
        private string relationship;

        /// <summary>
        /// The contact's relationship to the person, as a simple free-form text string with maximum
        /// length of 25 characters.
        /// <exception cref="ArgumentException">Relationship must contain no more than 25 characters.</exception>
        /// </summary>
        public string Relationship 
        {
            get
            {
                return relationship;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the relationship text that was provided does not
                    // exceed the maximum length.
                    if (value.Length > maximumRelationshipLength)
                    {
                        throw new ArgumentException("Relationship must contain no more than " + maximumRelationshipLength + " characters.");
                    }
                }
                relationship = value;
            }
        }

        /// <summary>
        /// The contact's daytime phone.
        /// </summary>
        private string daytimePhone;

        /// <summary>
        /// The contact's daytime phone, as a free-form text string with maximum length of 20 characters. 
        /// This might include an extension, for example, or text to indicate whether this is a cell phone 
        /// or a work phone.
        /// <exception cref="ArgumentException">Daytime phone must contain no more than 20 characters.</exception>
        /// </summary>
        public string DaytimePhone 
        {
            get
            {
                return daytimePhone;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the phone number that was provided does not exceed
                    // the maximum length.
                    if (value.Length > maximumPhoneLength)
                    {
                        throw new ArgumentException("Daytime phone must contain no more than " + maximumPhoneLength + " characters.");
                    }
                }
                daytimePhone = value;
            }
        }

        /// <summary>
        /// The contact's evening phone.
        /// </summary>
        private string eveningPhone;

        /// <summary>
        /// The contact's evening phone, as a free-form text string with maximum length of 20 characters. 
        /// This might include, for example, text to indicate whether this is a cell phone or a work phone 
        /// or a home phone.
        /// <exception cref="ArgumentException">Evening phone must contain no more than 20 characters.</exception>
        /// </summary>
        public string EveningPhone 
        {
            get
            {
                return eveningPhone;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the phone number that was provided does not exceed
                    // the maximum length.
                    if (value.Length > maximumPhoneLength)
                    {
                        throw new ArgumentException("Evening phone must contain no more than " + maximumPhoneLength + " characters.");
                    }
                }
                eveningPhone = value;
            }
        }

        /// <summary>
        /// The contact's other phone.
        /// </summary>
        private string otherPhone;

        /// <summary>
        /// The contact's other phone, as a free-form text string with maximum length of 20 characters. 
        /// This might include, for example, text to indicate whether this is a cell phone or a home phone.
        /// <exception cref="ArgumentException">Other phone must contain no more than 20 characters.</exception>
        /// </summary>
        public string OtherPhone 
        {
            get
            {
                return otherPhone;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the phone number that was provided does not exceed
                    // the maximum length.
                    if (value.Length > maximumPhoneLength)
                    {
                        throw new ArgumentException("Other phone must contain no more than " + maximumPhoneLength + " characters.");
                    }
                }
                otherPhone = value;
            }
        }

        /// <summary>
        /// The earliest date when emergency notifications related to this person should use this contact.
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Contact should be notified if missing (boolean).
        /// </summary>
        public bool IsMissingPersonContact { get; set; }

        /// <summary>
        /// Contact should be notified in case of emergency (boolean).
        /// </summary>
        public bool IsEmergencyContact { get; set; }

        /// <summary>
        /// The contact's address.
        /// </summary>
        private string address;

        /// <summary>
        /// The contact's address, as a simple free-form text string with maximum length of 75 characters.
        /// <exception cref="ArgumentException">Contact address must contain no more than 75 characters.</exception>
        /// </summary>
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Verify that the length of the address text that was provided does not exceed
                    // the maximum length.
                    if (value.Length > maximumAddressLength)
                    {
                        throw new ArgumentException("Contact address must contain no more than " + maximumAddressLength + " characters.");
                    }
                }
                address = value;
            }
        }

        /// <summary>
        /// Constructor for an emergency contact.
        /// </summary>
        /// <param name="name">Pass the name of the emergency contact, in freeform text, maximum 57 characters.</param>
        /// <exception cref="ArgumentNullException">Contact name is a required field.</exception>
        /// <exception cref="ArgumentException">Contact name must contain no more than 57 characters.</exception>
        public EmergencyContact(string name) 
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name", "Contact name is a required field.");
            }
            else
            {
                // We have a contact name. Now make sure it does not exceed the maximum length.
                if (name.Length > maximumContactNameLength)
                {
                    throw new ArgumentException("Contact name must contain no more than " + maximumContactNameLength + " characters.");
                }
            }

            this.name = name;

            Relationship  = string.Empty;
            DaytimePhone  = string.Empty;
            EveningPhone  = string.Empty;
            OtherPhone    = string.Empty;
            EffectiveDate = null;
            IsEmergencyContact     = true;    // Default is that the contact is notified in case of emergency. 
            IsMissingPersonContact = false;   // Default is that the contact is not notified if the person is missing.
            Address = string.Empty;
        }
    }
}
