//Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A single email address with associated type and preference flag
    /// </summary>
    public class EmailAddress
    {
        /// <summary>
        /// An email address
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The type code associated with this email address
        /// </summary>
        public string TypeCode { get; set; }
        /// <summary>
        /// Boolean indicates if this is the preferred email address
        /// </summary>
        public bool IsPreferred { get; set; }
    }
}
