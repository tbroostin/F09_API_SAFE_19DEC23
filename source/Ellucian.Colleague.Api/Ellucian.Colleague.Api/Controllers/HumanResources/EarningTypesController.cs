/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Expose Human Resources Earnings Types data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EarningTypesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IEarningsTypeRepository earningsTypeRepository;
        private readonly IEarningTypesService _earningTypesService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// EarningsTypesController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="earningsTypeRepository"></param>
        /// <param name="earningTypesService">Service of type <see cref="IEarningTypesService">IEarningTypesService</see></param>
        public EarningTypesController(ILogger logger, IAdapterRegistry adapterRegistry, IEarningsTypeRepository earningsTypeRepository, IEarningTypesService earningTypesService)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.earningsTypeRepository = earningsTypeRepository;
            _earningTypesService = earningTypesService;
        }

        /// <summary>
        /// Gets a list of earnings types. An earnings type is an identifier for wages or leave associated with an employment position.   
        /// The returned list should contain all active and inactive earn types available for an institution
        /// </summary>
        /// <returns>A List of earnings type objects</returns>
        [HttpGet]
        public async Task<IEnumerable<EarningsType>> GetEarningsTypesAsync()
        {
            logger.Debug("********* Start - Process to get List of Earning Types - Start *********");
            try
            {
                var earningsTypeEntities = await earningsTypeRepository.GetEarningsTypesAsync();
                var entityToDtoAdapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.EarningsType, EarningsType>();
                logger.Debug("********* End - Process to get List of Earning Types - End *********");
                return earningsTypeEntities.Select(earningsType => entityToDtoAdapter.MapToType(earningsType));
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return all earningTypes
        /// </summary>
        /// <returns>List of EarningTypes <see cref="Dtos.EarningTypes"/> objects representing matching earningTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EarningTypes>> GetEarningTypesAsync()
        {
            logger.Debug("********* Start - Process to get List of Earning Types - Start*********");
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                logger.Debug(string.Format("Calling GetEarningTypesAsync service method with BypassCache {0}",bypassCache));
                var items = await _earningTypesService.GetEarningTypesAsync(bypassCache);

                AddEthosContextProperties(await _earningTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _earningTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));
                logger.Debug("********* End - Process to get List of Earning Types - End*********");
                return items;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a earningTypes using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.EarningTypes> GetEarningTypesByGuidAsync(string guid)
        {
            logger.Debug("********* Start - Process to Get earning types by guid - Start *********");
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                logger.Debug("GUID cannot be null or empty");
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                logger.Debug(string.Format("Calling GetEarningTypesByGuidAsync service method with guid {0}",guid));
                var earningType = await _earningTypesService.GetEarningTypesByGuidAsync(guid);

                if (earningType != null)
                {

                    AddEthosContextProperties(await _earningTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _earningTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { earningType.Id }));
                }
                logger.Debug("********* End - Process to Get earning types by guid - End *********");
                return earningType;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new earningTypes
        /// </summary>
        /// <param name="earningTypes">DTO of the new earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.EarningTypes> PostEarningTypesAsync([FromBody] Dtos.EarningTypes earningTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing earningTypes
        /// </summary>
        /// <param name="guid">GUID of the earningTypes to update</param>
        /// <param name="earningTypes">DTO of the updated earningTypes</param>
        /// <returns>A earningTypes object <see cref="Dtos.EarningTypes"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.EarningTypes> PutEarningTypesAsync([FromUri] string guid, [FromBody] Dtos.EarningTypes earningTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a earningTypes
        /// </summary>
        /// <param name="guid">GUID to desired earningTypes</param>
        [HttpDelete]
        public async Task DeleteEarningTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
