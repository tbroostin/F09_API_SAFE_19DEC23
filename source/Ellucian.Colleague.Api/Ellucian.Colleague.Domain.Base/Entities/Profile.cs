// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class Profile : PersonBase
    {
        /// <summary>
        /// List of addresses for this person
        /// </summary>
        public ReadOnlyCollection<Address> Addresses { get; set; }
        private readonly List<Address> _addresses = new List<Address>();

        /// <summary>
        /// List of phones for this person
        /// </summary>
        public ReadOnlyCollection<Phone> Phones { get; private set; }
        private readonly List<Phone> _phones = new List<Phone>();

        /// <summary>
        /// Date of most recent address confirmation 
        /// </summary>
        public DateTimeOffset? AddressConfirmationDateTime { get ; set; }

        /// <summary>
        /// Date of most recent email address confirmation 
        /// </summary>
        public DateTimeOffset? EmailAddressConfirmationDateTime { get; set; }

        /// <summary>
        /// Date of most recent phone confirmation 
        /// </summary>
        public DateTimeOffset? PhoneConfirmationDateTime { get; set; }

        public Profile(string personId, string lastName)
            : base(personId, lastName)
        {
            Addresses = _addresses.AsReadOnly();
            Phones = _phones.AsReadOnly();
        }

        public void AddPhone(Phone phone)
        {
            if (phone == null)
            {
                throw new ArgumentNullException("phone", "Phone must be specified");
            }
            if (_phones.Where(f => f.Equals(phone)).Count() > 0)
            {
                throw new ArgumentException("Phone number already exists in this list");
            }
            _phones.Add(phone);
        }

        public void AddAddress(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address", "Address must be specified");
            }
            if (_addresses.Where(a => a.Equals(address)).Count() > 0)
            {
                throw new ArgumentException("Address already exists in this list");
            }
            _addresses.Add(address);
        }
    }
}
