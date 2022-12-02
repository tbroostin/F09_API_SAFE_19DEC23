// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// 
    /// </summary>

    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class LoadPeriodsController : BaseCompressedApiController
    {
        private readonly ILoadPeriodService _loadPeriodService;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the LoadPeriodsController class.
        /// </summary>
        /// <param name="loadPeriodService">Service of type <see cref="ILoadPeriodService">ILoadPeriodService</see></param>
        /// <param name="logger">Interface to logger</param>
        public LoadPeriodsController(ILoadPeriodService loadPeriodService, ILogger logger)
        {
            _loadPeriodService = loadPeriodService;
            _logger = logger;
        }

        /// <summary>
        /// Query Load Periods
        /// </summary>
        /// <param name="loadPeriodQueryCriteria">Load Period Query Criteria</param>
        /// <returns>Load periods</returns>
        [HttpPost]
        public async Task<IEnumerable<LoadPeriod>> QueryLoadPeriodsAsync(LoadPeriodQueryCriteria loadPeriodQueryCriteria)
        {
            if(loadPeriodQueryCriteria == null)
            {
                //Uncaught exceptions will return a 500 error = bad, this returns 400 which is default
                throw CreateHttpResponseException("Load period query criteria required to query load period");
            }

            //Controllers only talk in DTOs so do not have to specify in var name
            try
            {
                var loadPeriods = await _loadPeriodService.GetLoadPeriodsByIdsAsync(loadPeriodQueryCriteria.Ids);
                return loadPeriods;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to retrieve load periods");
                throw CreateHttpResponseException("Failed to retrieve load periods");
            }
            
        }
    }
}
