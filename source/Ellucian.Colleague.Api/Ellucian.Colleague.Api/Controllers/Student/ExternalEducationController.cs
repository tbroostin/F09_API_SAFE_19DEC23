// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for External Education
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ExternalEducationController : BaseCompressedApiController
    {
        private readonly IExternalEducationService _externalEducationService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the External Education Controller class.
        /// </summary>
        /// <param name="externalEducationService">Service of type <see cref="IExternalEducationService">IExternalEducationService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public ExternalEducationController(IExternalEducationService externalEducationService, ILogger logger)
        {
            _externalEducationService = externalEducationService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all External Education
        /// </summary>
        /// <returns>All <see cref="Dtos.ExternalEducation">External Education.</see></returns>
        [HttpGet, PermissionsFilter(StudentPermissionCodes.ViewExternalEducation)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "person" }, false, true)]
        public async Task<IHttpActionResult> GetExternalEducationsAsync(Paging page, [FromUri] string person = "")
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
                _externalEducationService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                if (person == null)
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.ExternalEducation>>(new List<Dtos.ExternalEducation>(), page, 0, this.Request);
                }

                var pageOfItems = await _externalEducationService.GetExternalEducationsAsync(page.Offset, page.Limit, bypassCache, person);
                AddEthosContextProperties(
                    await _externalEducationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.ExternalEducation>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }        
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves an External Education by GUID.
        /// </summary>
        /// <returns>A <see cref="Dtos.ExternalEducation">External Education.</see></returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.ViewExternalEducation)]
        public async Task<Dtos.ExternalEducation> GetExternalEducationByGuidAsync(string guid)
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
                _externalEducationService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _externalEducationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _externalEducationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _externalEducationService.GetExternalEducationByGuidAsync(guid);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }        
        }

        /// <summary>        
        /// Creates an External Education
        /// </summary>
        /// <param name="externalEducation"><see cref="Dtos.ExternalEducation">ExternalEducation</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.ExternalEducation">ExternalEducation</see></returns>
        [HttpPost]
        public async Task <Dtos.ExternalEducation> PostExternalEducationAsync([FromBody] Dtos.ExternalEducation externalEducation)
        {
            //Create is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>        
        /// Updates an External Education.
        /// </summary>
        /// <param name="id">Id of the External Education to update</param>
        /// <param name="externalEducation"><see cref="Dtos.ExternalEducation">ExternalEducation</see> to create</param>
        /// <returns>Updated <see cref="Dtos.ExternalEducation">ExternalEducation</see></returns>
        [HttpPut]
        public async Task<Dtos.ExternalEducation> PutExternalEducationAsync([FromUri] string id, [FromBody] Dtos.ExternalEducation externalEducation)
        {
            //Update is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing External Education
        /// </summary>
        /// <param name="id">Id of the External Education to delete</param>
        [HttpDelete]
        public async Task DeleteExternalEducationAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but EEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}