// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Authentication Scheme
    /// </summary>
    [Serializable]
    public class AuthenticationScheme
    {
        /// <summary>
        /// Authentication Scheme Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Authentication Scheme constructor
        /// </summary>
        /// <param name="code">Authentication Scheme code</param>
        public AuthenticationScheme(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code", "Code is required to create an authentication scheme.");
            }

            Code = code;
        }
    }
}
