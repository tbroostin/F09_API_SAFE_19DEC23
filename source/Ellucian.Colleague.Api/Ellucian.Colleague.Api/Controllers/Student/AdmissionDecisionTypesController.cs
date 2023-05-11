// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
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
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionDecisionTypes
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionDecisionTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionDecisionTypesService _admissionDecisionTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionDecisionTypesController class.
        /// </summary>
        /// <param name="admissionDecisionTypesService">Service of type <see cref="IAdmissionDecisionTypesService">IAdmissionDecisionTypesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionDecisionTypesController(IAdmissionDecisionTypesService admissionDecisionTypesService, ILogger logger)
        {
            _admissionDecisionTypesService = admissionDecisionTypesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionDecisionTypes v11.
        /// </summary>
        /// <returns>List of AdmissionDecisionTypes <see cref="Dtos.AdmissionDecisionType"/> objects representing matching admissionDecisionTypes</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionType2>> GetAdmissionDecisionTypes2Async()
        {
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
                var items = await _admissionDecisionTypesService.GetAdmissionDecisionTypesAsync(bypassCache);

                if (items != null && items.Any())
                {
                    AddEthosContextProperties(await _admissionDecisionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _admissionDecisionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(a => a.Id).ToList()));
                }

                return items;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Admission Decision Types
        /// </summary>
        /// <returns>All <see cref="Dtos.AdmissionDecisionTypes">AdmissionDecisionTypes.</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionTypes>> GetAdmissionDecisionTypesAsync()
        {
            return new List<AdmissionDecisionTypes>();
        }

        /// <summary>
        /// Read (GET) a admissionDecisionTypes using a GUID v11.
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisionTypes</param>
        /// <returns>A admissionDecisionTypes object <see cref="Dtos.AdmissionDecisionType"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionDecisionType2> GetAdmissionDecisionTypeByGuid2Async(string guid)
        {
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
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            
            try
            {
                var item = await _admissionDecisionTypesService.GetAdmissionDecisionTypesByGuidAsync(guid, bypassCache);

                if (item != null)
                {
                    AddEthosContextProperties(await _admissionDecisionTypesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                      await _admissionDecisionTypesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { item.Id }));
                }

                return item;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieve (GET) an existing Admission Decision Type
        /// </summary>
        /// <param name="guid">GUID of the admissionDecisionType to get</param>
        /// <returns>A admissionDecisionType object <see cref="Dtos.AdmissionDecisionTypes"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AdmissionDecisionTypes> GetAdmissionDecisionTypeByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No admission decision type was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Create (POST) a new admissionDecisionTypes
        /// </summary>
        /// <param name="admissionDecisionTypes">DTO of the new admissionDecisionTypes</param>
        /// <returns>A admissionDecisionTypes object <see cref="Dtos.AdmissionDecisionType"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionDecisionType2> PostAdmissionDecisionTypesAsync([FromBody] Dtos.AdmissionDecisionType2 admissionDecisionTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionDecisionTypes
        /// </summary>
        /// <param name="guid">GUID of the admissionDecisionTypes to update</param>
        /// <param name="admissionDecisionTypes">DTO of the updated admissionDecisionTypes</param>
        /// <returns>A admissionDecisionTypes object <see cref="Dtos.AdmissionDecisionType"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionDecisionType2> PutAdmissionDecisionTypesAsync([FromUri] string guid, [FromBody] Dtos.AdmissionDecisionType2 admissionDecisionTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionDecisionTypes
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisionTypes</param>
        [HttpDelete]
        public async Task DeleteAdmissionDecisionTypesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}
