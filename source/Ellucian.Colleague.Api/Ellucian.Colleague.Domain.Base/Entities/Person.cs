// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Class for PERSON records
    /// </summary>
    [Serializable]
    public class Person : PersonBase
    {
        /// <summary>
        /// Gets or sets the preferred address.
        /// </summary>
        /// <value>
        /// The preferred address.
        /// </value>
        public List<string> PreferredAddress { get; set; }


        #region Constructors

        /// <summary>
        /// Create a Person domain object
        /// </summary>
        /// <param name="personId">Person ID</param>
        /// <param name="lastName">Last Name</param>
        /// <param name="privacyStatusCode">Privacy status code</param>
        /// <exception cref="System.ArgumentNullException">
        /// personId
        /// or
        /// lastName
        /// </exception>
        public Person(string personId, string lastName, string privacyStatusCode = null)
            : base(personId, lastName, privacyStatusCode)
            {
            PreferredAddress = new List<string>();
        }

        #endregion
    }
}
