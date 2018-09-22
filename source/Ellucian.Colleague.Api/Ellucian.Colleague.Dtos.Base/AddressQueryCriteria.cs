// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Used to query addresses for specific keys or persons.
    /// </summary>
    public class AddressQueryCriteria
    {
        /// <summary>
        /// Specific Address Keys to retrieve
        /// </summary>
        public IEnumerable<string> AddressIds { get; set; }
        /// <summary>
        /// Retrieve Addresses for these people
        /// </summary>
        public IEnumerable<string> PersonIds { get; set; }
    }
}
