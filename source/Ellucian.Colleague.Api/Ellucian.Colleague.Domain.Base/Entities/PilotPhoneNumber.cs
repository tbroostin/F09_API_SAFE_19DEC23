using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PilotPhoneNumber
    {
        // Required fields

        private readonly string _personId;
        public string PersonId { get { return _personId; } }

        // Non-required fields

        public string PrimaryPhoneNumber;
        public string SmsPhoneNumber;

        public PilotPhoneNumber(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            _personId = personId;
        }
    }
}
