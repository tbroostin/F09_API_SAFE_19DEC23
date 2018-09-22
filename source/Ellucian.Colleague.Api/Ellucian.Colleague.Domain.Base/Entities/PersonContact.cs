// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// A class to encapsulate a person's contacts and related information.
    /// </summary>
    [Serializable]
    public class PersonContact
    {
        public string PersonContactGuid { get; private set; }

        public string PersonContactRecordKey { get; private set; }

        public string SubjectPersonId { get; private set; }

        public IEnumerable<PersonContactDetails> PersonContactDetails { get; set; }

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recordKey"></param>
        /// <param name="personId"></param>
        public PersonContact(string id, string recordKey, string personId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id is required");
            }
            if (string.IsNullOrEmpty(recordKey))
            {
                throw new ArgumentNullException("recordKey is required");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId is required");
            }
            PersonContactGuid = id;
            PersonContactRecordKey = recordKey;
            SubjectPersonId = personId;
        }
    }

    [Serializable]
    public class PersonContactDetails
    {
        public string ContactName { get; set; }

        public string ContactFlag { get; set; }

        public string MissingContactFlag { get; set; }

        public string ContactAddresses { get; set; }

        public string DaytimePhone { get; set; }

        public string EveningPhone { get; set; }

        public string OtherPhone { get; set; }

        public string Relationship { get; set; }
    }
}
