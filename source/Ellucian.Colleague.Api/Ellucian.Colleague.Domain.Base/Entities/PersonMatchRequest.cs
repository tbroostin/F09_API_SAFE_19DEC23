//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonMatchRequest
    {
        public PersonMatchRequest()
        {
            Outcomes = new List<PersonMatchRequestOutcomes>();
        }

        public string Guid { get; set; }

        public string RecordKey { get; set; }
        
        public string PersonId { get; set; }

        public string Originator { get; set; }

        public List<PersonMatchRequestOutcomes> Outcomes { get; private set; }

        public void AddPersonMatchRequestOutcomes(PersonMatchRequestOutcomes personMatchRequestOutcome)
        {
            if (personMatchRequestOutcome == null)
            {
                throw new ArgumentNullException("personLanguage", "Person Language Object must be specified");
            }
            if (Outcomes == null)
            {
                Outcomes = new List<PersonMatchRequestOutcomes>();
            }
            if (!Outcomes.Contains(personMatchRequestOutcome))
            {
                Outcomes.Add(personMatchRequestOutcome);
            }
        }
    }
}
