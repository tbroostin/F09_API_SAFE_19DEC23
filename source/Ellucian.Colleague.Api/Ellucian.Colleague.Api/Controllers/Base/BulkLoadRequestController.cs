//Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Http.Models;

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

            Guid guidOutput;
            if (!Guid.TryParse(bulkLoadRequestDto.RequestorTrackingId, out guidOutput))
            {
                throw CreateHttpResponseException(new IntegrationApiException("RequestorTrackingId",
                   IntegrationApiUtility.GetDefaultApiError("Must provide a valid GUID for RequestorTrackingId.")));
            }
            if (!Guid.TryParse(bulkLoadRequestDto.ApplicationId, out guidOutput))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ApplicationId",
                   IntegrationApiUtility.GetDefaultApiError("Must provide a valid GUID for ApplicationId.")));
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
        /// <param name="criteria">filter</param>
        /// <param name="guid">guid</param>
        /// <returns>The requested <see cref="BulkLoadGet">object</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.BulkLoadGet)), FilteringFilter(IgnoreFiltering = true)]
        public async Task<Dtos.BulkLoadGet> GetBulkLoadRequestStatusAsync(QueryStringFilter criteria, [FromUri] string guid = null)
        {

            var filter = GetFilterObject<Dtos.BulkLoadGet>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null requestorTrackingId",
                    IntegrationApiUtility.GetDefaultApiError("requestorTrackingId is a required filter.")));
            }

            var id = filter.RequestorTrackingId;

            // if the filter is empty, do we have a guid in the URL?
            if (string.IsNullOrEmpty(id))
            {
                id = guid;
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null requestorTrackingId",
                    IntegrationApiUtility.GetDefaultApiError("requestorTrackingId is a required property.")));
            }

            var routeData = ActionContext.Request.GetRouteData();
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

            try
            {
                return await _bulkLoadRequestService.GetBulkLoadRequestStatus(GetRouteResourceName(), id, permissionCode);
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