// Copyright 2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// Provides access to retrieve Student Planning configuration data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class PlanningConfigurationController : BaseCompressedApiController
    {
        private readonly IPlanningConfigurationRepository _planningConfigurationRepository;
        private readonly ILogger _logger;
        private readonly IAdapterRegistry _adapterRegistry;

        /// <summary>
        /// AdvisorsController constructor
        /// </summary>
        /// <param name="planningConfigurationRepository">Interface to the planning configuration repository</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="adapterRegistry">Adapter registry</param>
        public PlanningConfigurationController(IPlanningConfigurationRepository planningConfigurationRepository, ILogger logger, IAdapterRegistry adapterRegistry)
        {
            _planningConfigurationRepository = planningConfigurationRepository;
            _logger = logger;
            _adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Retrieves the configuration information for Student Planning.
        /// </summary>
        /// <returns>The <see cref="PlanningConfiguration">Student Planning configuration</see> data</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the required setup is not complete</exception>
        public async System.Threading.Tasks.Task<PlanningConfiguration> GetPlanningConfigurationAsync()
        {
            PlanningConfiguration configurationDto = null;
            try
            {
                var configurationEntity = await _planningConfigurationRepository.GetPlanningConfigurationAsync();
                var adapter = _adapterRegistry.GetAdapter<Domain.Planning.Entities.PlanningConfiguration, PlanningConfiguration>();
                configurationDto = adapter.MapToType(configurationEntity);
                return configurationDto;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving planning configuration";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }

            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}