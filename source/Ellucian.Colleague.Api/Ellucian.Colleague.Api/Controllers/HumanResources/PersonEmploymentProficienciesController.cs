//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
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

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonEmploymentProficiencies
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonEmploymentProficienciesController : BaseCompressedApiController
    {
        private readonly IPersonEmploymentProficienciesService _personEmploymentProficienciesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonEmploymentProficienciesController class.
        /// </summary>
        /// <param name="personEmploymentProficienciesService">Service of type <see cref="IPersonEmploymentProficienciesService">IPersonEmploymentProficienciesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonEmploymentProficienciesController(ILogger logger, IPersonEmploymentProficienciesService personEmploymentProficienciesService )
        {
            _personEmploymentProficienciesService = personEmploymentProficienciesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personEmploymentProficiencies
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of PersonEmploymentProficiencies <see cref="Dtos.PersonEmploymentProficiencies"/> objects representing matching personEmploymentProficiencies</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetPersonEmploymentProficienciesAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _personEmploymentProficienciesService.GetPersonEmploymentProficienciesAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                   await _personEmploymentProficienciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _personEmploymentProficienciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.PersonEmploymentProficiencies>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a personEmploymentProficiencies using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonEmploymentProficiencies> GetPersonEmploymentProficienciesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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
                AddEthosContextProperties(
                  await _personEmploymentProficienciesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _personEmploymentProficienciesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { guid }));

                return await _personEmploymentProficienciesService.GetPersonEmploymentProficienciesByGuidAsync(guid);
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
        /// Create (POST) a new personEmploymentProficiencies
        /// </summary>
        /// <param name="personEmploymentProficiencies">DTO of the new personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonEmploymentProficiencies> PostPersonEmploymentProficienciesAsync([FromBody] Dtos.PersonEmploymentProficiencies personEmploymentProficiencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personEmploymentProficiencies
        /// </summary>
        /// <param name="guid">GUID of the personEmploymentProficiencies to update</param>
        /// <param name="personEmploymentProficiencies">DTO of the updated personEmploymentProficiencies</param>
        /// <returns>A personEmploymentProficiencies object <see cref="Dtos.PersonEmploymentProficiencies"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonEmploymentProficiencies> PutPersonEmploymentProficienciesAsync([FromUri] string guid, [FromBody] Dtos.PersonEmploymentProficiencies personEmploymentProficiencies)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personEmploymentProficiencies
        /// </summary>
        /// <param name="guid">GUID to desired personEmploymentProficiencies</param>
        [HttpDelete]
        public async Task DeletePersonEmploymentProficienciesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}