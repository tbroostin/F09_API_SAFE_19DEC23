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
    /// Provides access to Prospect Source data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ProspectSourcesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// Initializes a new instance of the ProspectSourcesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        public ProspectSourcesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Retrieves all Prospect Sources.
        /// </summary>
        /// <returns>All <see cref="ProspectSource">Prospect Source codes and descriptions.</see></returns>
        public IEnumerable<ProspectSource> Get()
        {
            var ProspectSourceCollection = _referenceDataRepository.ProspectSources;

            // Get the right adapter for the type mapping
            var ProspectSourceDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.ProspectSource, ProspectSource>();

            // Map the ProspectSource entity to the program DTO
            var ProspectSourceDtoCollection = new List<ProspectSource>();
            foreach (var ProspectSource in ProspectSourceCollection)
            {
                ProspectSourceDtoCollection.Add(ProspectSourceDtoAdapter.MapToType(ProspectSource));
            }

            return ProspectSourceDtoCollection;
        }
    }
}
