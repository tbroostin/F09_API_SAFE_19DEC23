// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Profile information required for proxy
    /// </summary>
    public class PersonProxyDetails
    {
        /// <summary>
        /// Unique system ID of this person.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Person's preferred name
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// The email address that is to be used for proxy communication
        /// </summary>
        public string ProxyEmailAddress { get; set; }

    }
}
