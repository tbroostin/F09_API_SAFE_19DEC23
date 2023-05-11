// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Data.Colleague.Exceptions;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Frequency Codes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class FrequencyCodesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FrequencyCodesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">ReferenceDataRepository</see></param>
        /// <param name="logger">Interface to Logger</param>
        public FrequencyCodesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all of the Frequency Codes.
        /// </summary>
        /// <returns>All <see cref="FrequencyCode">FrequencyCodes</see></returns>
        public IEnumerable<FrequencyCode> Get()
        {
            try
            {
                var frequencyCodeCollection = _referenceDataRepository.FrequencyCodes;

                // Get the right adapter for the type mapping
                var frequencyCodeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.FrequencyCode, FrequencyCode>();

                // Map the courselevel entity to the program DTO
                var frequencyCodeDtoCollection = new List<FrequencyCode>();
                foreach (var frequencyCode in frequencyCodeCollection)
                {
                    frequencyCodeDtoCollection.Add(frequencyCodeDtoAdapter.MapToType(frequencyCode));
                }

                return frequencyCodeDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                string message = "Your previous session has expired and is no longer valid.";
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
        }
    }
}
