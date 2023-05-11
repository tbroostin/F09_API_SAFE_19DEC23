// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PilotPhoneNumber
    {
        // Required fields

        private readonly string _personId;
        public string PersonId { get { return _personId; } }

        // Non-required fields

        public string PrimaryPhoneNumber { get; set; }
        public string SmsPhoneNumber { get; set; }

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
