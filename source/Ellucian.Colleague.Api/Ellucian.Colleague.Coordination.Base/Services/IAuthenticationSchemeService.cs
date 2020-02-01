// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Authentication Scheme Service
    /// </summary>
    public interface IAuthenticationSchemeService
    {
        /// <summary>
        /// Gets the authentication scheme for the given username
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>The authentication scheme associated with the username. Null if the user did not have an authentication scheme defined.</returns>
        Task<AuthenticationScheme> GetAuthenticationSchemeAsync(string username);
    }
}
