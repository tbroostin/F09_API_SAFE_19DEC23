// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonUserName
    {
        /// <summary>
        /// Person's key
        /// </summary>
        public string PersonUserNameId { get; private set; }

        /// <summary>
        /// Persons user id
        /// </summary>
        public string UserName { get; set; }

        public PersonUserName(string id, string userName)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "PersonUserName id must be specified.");
            }
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("userName", "Person user name must be specified.");
            }

            PersonUserNameId = id;
            UserName = userName;
        }
    }
}
