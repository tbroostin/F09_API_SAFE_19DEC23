/* Copyright 2014 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// This class contains emergency contact information for a person.
    /// </summary>
    public class EmergencyContact
    {

        /// <summary>
        /// Name of the contact person, in freeform text. Maximum length is 57 characters.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Relationship of the contact to the person. Maximum length is 25 characters. Freeform text.
        /// </summary>
        public string Relationship { get; set; }

        /// <summary>
        /// Daytime phone number of the contact. Can include text. No formatting is applied. Maximum length is 20 characters.
        /// </summary>
        public string DaytimePhone { get; set; }

        /// <summary>
        /// Evening phone number of the contact. Can include text. No formatting is applied. Maximum length is 20 characters.
        /// </summary>
        public string EveningPhone { get; set; }

        /// <summary>
        /// Other phone number of the contact. Can include text. No formatting is applied. Maximum length is 20 characters.
        /// </summary>
        public string OtherPhone { get; set; }

        /// <summary>
        /// Beginning date when this contact is to be used.
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Indicates whether the contact should be notified if this person is missing.
        /// </summary>
        public bool IsMissingPersonContact { get; set; }

        /// <summary>
        /// Indicates whether the contact should be notified in case of emergency.
        /// </summary>
        public bool IsEmergencyContact { get; set; }

        /// <summary>
        /// Address of the contact. Maximum length is 75 characters. Freeform text.
        /// </summary>
        public string Address { get; set; }

    }
}
