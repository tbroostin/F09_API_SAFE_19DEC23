//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Routes;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to BulkLoadRequest
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class BulkLoadRequestController : BaseCompressedApiController
    {
        private readonly IBulkLoadRequestService _bulkLoadRequestService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BulkLoadRequestController class.
        /// </summary>
        /// <param name="bulkLoadRequestService">Service of type <see cref="IBulkLoadRequestService">IBulkLoadRequestService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BulkLoadRequestController(IBulkLoadRequestService bulkLoadRequestService, ILogger logger)
        {
            _bulkLoadRequestService = bulkLoadRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Create (POST) a new BulkLoadRequest
        /// </summary>
        /// <param name="bulkLoadRequestDto">DTO of the new BulkLoadRequest</param>
        /// <returns>A BulkLoadRequest object <see cref="Dtos.BulkLoadRequest"/> in HeDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.BulkLoadRequest> PostBulkLoadRequestAsync(Dtos.BulkLoadRequest bulkLoadRequestDto)
        {
            if (bulkLoadRequestDto == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid BulkLoadRequest.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(bulkLoadRequestDto.RequestorTrackingId))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null BulkLoadRequest id",
                    IntegrationApiUtility.GetDefaultApiError("RequestorId is a required property.")));
            }

            if (string.IsNullOrEmpty(bulkLoadRequestDto.ApplicationId))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null ApplicationId",
                    IntegrationApiUtility.GetDefaultApiError("ApplicationId is a required property.")));
            }

            try
            {
                var ethosRouteInfo = GetEthosResourceRouteInfo();
                bulkLoadRequestDto.ResourceName = ethosRouteInfo.ResourceName;
                
                var routeData = ActionContext.Request.GetRouteData();
                if (string.IsNullOrEmpty(bulkLoadRequestDto.Representation) || bulkLoadRequestDto.Representation.Contains("application/json"))
                {
                    object objBulkRepresentationValue;
                    routeData.Route.Defaults.TryGetValue("bulkRepresentation", out objBulkRepresentationValue);
                    if (objBulkRepresentationValue != null)
                    {
                        bulkLoadRequestDto.Representation = objBulkRepresentationValue.ToString();
                    }
                    else
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("Null Default Bulk Representation",
                            IntegrationApiUtility.GetDefaultApiError("Default Bulk Representation is not set on route.")));
                    }
                }

                string permissionCode = string.Empty;
                object objPermissionCodeValue;
                routeData.Route.Defaults.TryGetValue("permissionCode", out objPermissionCodeValue);
                if (objPermissionCodeValue != null)
                {
                    permissionCode = objPermissionCodeValue.ToString();
                }
                else
                {
                    throw CreateHttpResponseException(new IntegrationApiException("Null PermissionCode",
                        IntegrationApiUtility.GetDefaultApiError("PermissionCode is not set on route.")));
                }

                return await _bulkLoadRequestService.CreateBulkLoadRequestAsync(bulkLoadRequestDto, permissionCode);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ConfigurationException e)
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
        ///  Get the status of the bulk request
        /// </summary>
        /// <param name="id">Guid of the bulk request to get</param>
        /// <returns>The requested <see cref="BulkLoadGet">Person</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        public async Task<Dtos.BulkLoadGet> GetBulkLoadRequestStatusAsync(string id)
        {
            try
            {
                return await _bulkLoadRequestService.GetBulkLoadRequestStatus(id);
            }
            catch (PermissionsException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
    }
}