// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// User ID Recovery Request
    /// </summary>
    public class UserIdRecoveryRequest
    {
        /// <summary>
        /// First name of the user to recover
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user to recover
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// An email address of the user to recover
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
