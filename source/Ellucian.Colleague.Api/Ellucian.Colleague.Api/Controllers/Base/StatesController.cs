// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides an API controller for fetching state and province codes.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class StatesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;


        /// <summary>
        /// Initializes a new instance of the StatesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StatesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Gets information for all State and Province Codes 
        /// </summary>
        /// <returns>List of State Dtos</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.State>> GetAsync()
        {
            try
            {
                var stateDtoCollection = new List<Ellucian.Colleague.Dtos.Base.State>();
                var stateCollection = await referenceDataRepository.GetStateCodesAsync();
                // Get the right adapter for the type mapping
                var stateDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.State, Ellucian.Colleague.Dtos.Base.State>();
                // Map the code and description to the Dto
                if (stateCollection != null && stateCollection.Count() > 0)
                {
                    foreach (var state in stateCollection)
                    {
                        stateDtoCollection.Add(stateDtoAdapter.MapToType(state));
                    }
                }

                return stateDtoCollection;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve States.");
            }


        }
    }
}