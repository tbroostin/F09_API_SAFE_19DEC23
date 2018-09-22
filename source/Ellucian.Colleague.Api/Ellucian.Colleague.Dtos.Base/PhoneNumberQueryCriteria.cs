// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Used to query phone numbers from person keys.
    /// </summary>
    public class PhoneNumberQueryCriteria
    {
        /// <summary>
        /// List of Person Keys to get Phone Numbers for
        /// </summary>
        public IEnumerable<string> PersonIds { get; set; }
    }
}
