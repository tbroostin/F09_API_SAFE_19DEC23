// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Authentication Scheme Service
    /// </summary>
    [RegisterType]
    public class AuthenticationSchemeService : BaseCoordinationService, IAuthenticationSchemeService
    {
        /// <summary>
        /// Authentication Scheme Repository
        /// </summary>
        private IAuthenticationSchemeRepository authenticationSchemeRepository;

        /// <summary>
        /// Authentication Scheme Service constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        /// <param name="authenticationSchemeRepository"></param>
        public AuthenticationSchemeService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger, IAuthenticationSchemeRepository authenticationSchemeRepository) : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.authenticationSchemeRepository = authenticationSchemeRepository;
        }

        /// <summary>
        /// Gets the authentication scheme for the given username
        /// </summary>
        /// <param name="username">User name</param>
        /// <returns>The authentication scheme associated with the username. Null if the user did not have an authentication scheme defined.</returns>
        public async Task<Dtos.Base.AuthenticationScheme> GetAuthenticationSchemeAsync(string username)
        {
            var authenticationSchemeEntity = await authenticationSchemeRepository.GetAuthenticationSchemeAsync(username);
            var authenticationSchemeAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.AuthenticationScheme, Dtos.Base.AuthenticationScheme>();
            var authenticationSchemeDto = authenticationSchemeAdapter.MapToType(authenticationSchemeEntity);
            return authenticationSchemeDto;
        }
    }
}
