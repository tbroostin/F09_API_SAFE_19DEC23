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
    /// Provides access to Suffix data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class SuffixesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the SuffixController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        public SuffixesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Suffixes.
        /// </summary>
        /// <returns>All <see cref="Suffix">Suffix codes and descriptions.</see></returns>
        public IEnumerable<Suffix> Get()
        {
            var suffixCollection = _referenceDataRepository.Suffixes;

            // Get the right adapter for the type mapping
            var suffixDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Suffix, Suffix>();

            // Map the suffix entity to the program DTO
            var suffixDtoCollection = new List<Suffix>();
            foreach (var suffix in suffixCollection)
            {
                suffixDtoCollection.Add(suffixDtoAdapter.MapToType(suffix));
            }

            return suffixDtoCollection;
        }
    }
}
