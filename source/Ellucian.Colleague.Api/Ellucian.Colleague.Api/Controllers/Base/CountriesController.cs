// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching country codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class CountriesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;


        /// <summary>
        /// Initializes a new instance of the CountriesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public CountriesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets information for all Country codes
        /// </summary>
        /// <returns>List of Country Dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.Country>> GetAsync()
        {
            try
            {
                var countryDtoCollection = new List<Ellucian.Colleague.Dtos.Base.Country>();
                var countryCollection = await referenceDataRepository.GetCountryCodesAsync(false);
                // Get the right adapter for the type mapping
                var countryDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Ellucian.Colleague.Dtos.Base.Country>();
                // Map the grade entity to the grade DTO
                if (countryCollection != null && countryCollection.Count() > 0)
                {
                    foreach (var country in countryCollection)
                    {
                        countryDtoCollection.Add(countryDtoAdapter.MapToType(country));
                    }
                }

                return countryDtoCollection;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Countries.");
            }


        }
    }
}