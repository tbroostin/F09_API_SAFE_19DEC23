// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Prefix data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PrefixesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the PrefixController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        public PrefixesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Prefixes.
        /// </summary>
        /// <returns>All <see cref="Prefix">Prefix codes and descriptions.</see></returns>
        public IEnumerable<Prefix> Get()
        {
            var prefixCollection = _referenceDataRepository.Prefixes;

            // Get the right adapter for the type mapping
            var prefixDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Prefix, Prefix>();

            // Map the prefix entity to the program DTO
            var prefixDtoCollection = new List<Prefix>();
            foreach (var prefix in prefixCollection)
            {
                prefixDtoCollection.Add(prefixDtoAdapter.MapToType(prefix));
            }

            return prefixDtoCollection;
        }
    }
}
