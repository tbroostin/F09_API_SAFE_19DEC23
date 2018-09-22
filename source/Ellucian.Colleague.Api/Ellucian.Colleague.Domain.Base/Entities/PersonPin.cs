// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonPin
    {
        /// <summary>
        /// Person's key
        /// </summary>
        public string PersonId { get; private set; }

        /// <summary>
        /// Persons user id
        /// </summary>
        public string PersonPinUserId { get; set; }

        public PersonPin(string id, string personUserId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Person id must be specified.");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("personUserId", "Person user id must be specified.");
            }

            PersonId = id;
            PersonPinUserId = personUserId;
        }
    }
}
