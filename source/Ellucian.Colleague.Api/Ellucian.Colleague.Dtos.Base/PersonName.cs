// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// The name components of a person
    /// </summary>
    public class PersonName
    {
        /// <summary>
        /// The person's given name.  Required.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// The person's middle name
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// The person's family name.  Required.
        /// </summary>
        public string FamilyName { get; set; }
    }
}
