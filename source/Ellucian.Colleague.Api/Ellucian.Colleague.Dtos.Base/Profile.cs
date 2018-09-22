// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Profile information for a person
    /// </summary>
    public class Profile
    {
        /// <summary>
        ///  Unique system ID of this person
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Person's last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Person's first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Calculated preferred name, based on this person's stated preference
        /// </summary>
        public string PreferredName { get; set; }
        /// <summary>
        /// Date of Birth
        /// </summary>
        public System.DateTime? BirthDate { get; set; }
        /// <summary>
        /// Preferred email address of student
        /// </summary>
        public string PreferredEmailAddress { get; set; }

        /// <summary>
        /// List of addresses for this person
        /// </summary>
        public List<Address> Addresses { get; set; }

        /// <summary>
        /// List of email addresses for this person
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }

        /// <summary>
        /// List of phones for this person
        /// </summary>
        public List<Phone> Phones { get; set; }


        /// <summary>
        /// Date of most recent address confirmation 
        /// </summary>
        public DateTimeOffset? AddressConfirmationDateTime { get; set; }

        /// <summary>
        /// Date of most recent email address confirmation 
        /// </summary>
        public DateTimeOffset? EmailAddressConfirmationDateTime { get; set; }

        /// <summary>
        /// Date of most recent phone confirmation 
        /// </summary>
        public DateTimeOffset? PhoneConfirmationDateTime { get; set; }

        /// <summary>
        /// Date time stamp of last change to person data (used for version control at update)
        /// </summary>
        public DateTimeOffset LastChangedDateTime { get; set; }

        /// <summary>
        /// Chosen First Name
        /// </summary>
        public string ChosenFirstName { get; set; }

        /// <summary>
        /// Chosen Middle Name
        /// </summary>
        public string ChosenMiddleName { get; set; }

        /// <summary>
        /// Chosen Last Name
        /// </summary>
        public string ChosenLastName { get; set; }

        /// <summary>
        /// Personal pronoun code
        /// </summary>
        public string PersonalPronounCode { get; set; }

        /// <summary>
        /// Boolean indicates if person is deceased.
        /// </summary>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gender Identity Code
        /// </summary>
        public string GenderIdentityCode { get; set; }

        /// <summary>
        /// Nickname
        /// </summary>
        public string Nickname { get; set; }
    }
}
