using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PhoneNumber
    {
        // Required fields

        private readonly string _personId;
        public string PersonId { get { return _personId; } }

        // Non-required fields

        private readonly ICollection<Phone> _phoneNumbers;
        public ICollection<Phone> PhoneNumbers { get { return _phoneNumbers; } }

        public PhoneNumber(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            _personId = personId;
            _phoneNumbers = new List<Phone>();
        }

        public void AddPhone(Phone phone)
        {
            if (phone == null)
            {
                throw new ArgumentNullException("phone", "Phone must be specified");
            }
            if (_phoneNumbers.Where(f => f.Equals(phone)).Count() > 0)
            {
                throw new ArgumentException("Phone number already exists in this list");
            }
            _phoneNumbers.Add(phone);
        }
    }
}
