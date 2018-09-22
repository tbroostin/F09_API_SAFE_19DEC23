﻿// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Role data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class RolesController : BaseCompressedApiController
    {
        private readonly IRoleRepository roleRepository;
        private readonly IAdapterRegistry adapterRegistry;

        /// <summary>
        /// RolesController constructor
        /// </summary>
        /// <param name="roleRepository">Repository of type <see cref="IRoleRepository">IRoleRepository</see></param>
        /// <param name="adapterRegistry">Adapter of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        public RolesController(IRoleRepository roleRepository, IAdapterRegistry adapterRegistry)
        {
            if (roleRepository == null)
            {
                throw new ArgumentNullException("roleRepository");
            }
            this.roleRepository = roleRepository;

            if (adapterRegistry == null)
            {
                throw new ArgumentNullException("adapterRegistry");
            }
            this.adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Retrieves all Roles.
        /// </summary>
        /// <returns>All <see cref="Role">user Roles defined in Colleague.</see></returns>
        public async Task<IEnumerable<Dtos.Base.Role>> GetRolesAsync()
        {
            var domainRoles = await roleRepository.GetRolesAsync();
            var adapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Entities.Role, Dtos.Base.Role>();
            var dtos = new List<Dtos.Base.Role>();
            foreach (var role in domainRoles)
            {
                dtos.Add(adapter.MapToType(role));
            }
            return dtos;
        }
    }
}
