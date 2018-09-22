// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities 
{
    [Serializable]
    public class Faculty //: Person
    {
        private readonly string _Id;
        public string Id { get { return _Id; } }

        private readonly string _LastName;
        public string LastName { get { return _LastName; } }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }

        public Faculty(string id, string lastName) {
            if (String.IsNullOrEmpty(id)) {
                throw new ArgumentNullException("id");
            }
            if (String.IsNullOrEmpty(lastName)) {
                throw new ArgumentNullException("lastName");
            }
            _Id = id;
            _LastName = lastName;
        }

        /// <summary>
        /// This is the name the faculty member has indicated should be used in student-facing software.
        /// This is not guaranteed to have a value - it is up to the consumer of this entity to determine which
        /// name is appropriate to use based on context.
        /// </summary>
        public string ProfessionalName { get; set; }

        /// <summary>
        /// All current Personal and Address Phone numbers specific to this person.
        /// </summary>
        private List<Phone> _PersonalPhones = new List<Phone>();

        /// <summary>
        /// All Addresses with a FAC designation in special processing 2 of the validation table.
        /// </summary>
        private List<Address> _Addresses = new List<Address>();

        /// <summary>
        /// All Email Addresses for the faculty as a person. 
        /// </summary>
        private List<EmailAddress> _PersonEmailAddresses = new List<EmailAddress>();

        public void AddPhone(Phone phone) {
            if (phone == null) {
                throw new ArgumentNullException("phone", "Phone must be specified");
            }
            if (_PersonalPhones.Where(f => f.Equals(phone)).Count() > 0) {
                throw new ArgumentException("Phone number already exists in this list");
            }
            _PersonalPhones.Add(phone);
        }

        public void AddEmailAddress(EmailAddress emailAddress) {
            if (emailAddress == null) {
                throw new ArgumentNullException("emailAddress", "Email address must be specified");
            }
            if (_PersonEmailAddresses.Where(f => f.Equals(emailAddress)).Count() > 0) {
                throw new ArgumentException("Email address already exists in this list");
            }
            _PersonEmailAddresses.Add(emailAddress);
        }

        public void AddAddress(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address", "Address must be specified");
            }
            if (_Addresses.Where(a => a.Equals(address)).Count() > 0)
            {
                throw new ArgumentException("Address already exists in this list");
            }
            _Addresses.Add(address);
        }

        /// <summary>
        /// The email or emails for the faculty that are designated as "faculty type" emails and that can be publicly displayed for
        /// such things as section details in the course catalog
        /// </summary>
        /// <param name="facultyEmailTypeCode"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFacultyEmailAddresses(string facultyEmailTypeCode) {
            List<string> facultyEmails = new List<String>();
            if (!string.IsNullOrEmpty(facultyEmailTypeCode)) {
                var emails = _PersonEmailAddresses.Where(em => em.TypeCode == facultyEmailTypeCode).Select(e => e.Value);
                if (emails != null && emails.Count() > 0) {
                    facultyEmails.AddRange(emails);
                }
            }
            return facultyEmails;
        }

        /// <summary>
        /// The phone numbers for the faculty that are designated as "faculty type" phone numbers and that can be publicly displayed.
        /// </summary>
        /// <param name="facultyPhoneTypeCode"></param>
        /// <returns></returns>
        public IEnumerable<Phone> GetFacultyPhones(string facultyPhoneTypeCode) {
            // Includes any phone number that has the specific type. Combines info in personal phones and address phones.
            List<Phone> facultyPhones = new List<Phone>();
            if (!string.IsNullOrEmpty(facultyPhoneTypeCode)) {
                var fromPersonal = _PersonalPhones.Where(pp => pp.TypeCode == facultyPhoneTypeCode);
                if (fromPersonal != null && fromPersonal.Count() > 0) {
                    facultyPhones.AddRange(fromPersonal);
                }
            }
            return facultyPhones;
        }

        /// <summary>
        /// The address for the faculty that are designated as "faculty type" addresses and that can be publicly displayed.
        /// </summary>
        /// <param name="facultyAddressTypeCode">Address Type which has FAC designation</param>
        /// <returns></returns>
        public IEnumerable<Address> GetFacultyAddresses(string facultyAddressTypeCode)
        {
            List<Address> facultyAddresses = new List<Address>();
            if (!string.IsNullOrEmpty(facultyAddressTypeCode))
            {
                var fromAddress = _Addresses.Where(pp => pp.Type == facultyAddressTypeCode);
                if (fromAddress != null && fromAddress.Count() > 0)
                {
                    facultyAddresses.AddRange(fromAddress);
                }
            }
            return facultyAddresses;
        }
    }
}
