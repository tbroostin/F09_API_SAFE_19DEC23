// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Authentication Scheme Repository
    /// </summary>
    public interface IAuthenticationSchemeRepository
    {
        /// <summary>
        /// Gets the authentication scheme associated with the given username.
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>The authentication scheme the username is subject to. Null if the username does not have a defined authentication scheme.</returns>
        Task<AuthenticationScheme> GetAuthenticationSchemeAsync(string username);
    }
}
