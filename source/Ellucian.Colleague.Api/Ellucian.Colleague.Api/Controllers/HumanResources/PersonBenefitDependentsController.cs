//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to PersonBenefitDependents
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonBenefitDependentsController : BaseCompressedApiController
    {
        private readonly IPersonBenefitDependentsService _personBenefitDependentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonBenefitDependentsController class.
        /// </summary>
        /// <param name="personBenefitDependentsService">Service of type <see cref="IPersonBenefitDependentsService">IPersonBenefitDependentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonBenefitDependentsController(IPersonBenefitDependentsService personBenefitDependentsService, ILogger logger)
        {
            _personBenefitDependentsService = personBenefitDependentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personBenefitDependents
        /// </summary>
        /// <returns>List of PersonBenefitDependents <see cref="Dtos.PersonBenefitDependents"/> objects representing matching personBenefitDependents</returns>
        [HttpGet, EedmResponseFilter]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPersonBenefitDependentsAsync(Paging page)
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
                AddDataPrivacyContextProperty((await _personBenefitDependentsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var pageOfItems = await _personBenefitDependentsService.GetPersonBenefitDependentsAsync(page.Offset, page.Limit, bypassCache);
                return new PagedHttpActionResult<IEnumerable<Dtos.PersonBenefitDependents>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a personBenefitDependents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonBenefitDependents> GetPersonBenefitDependentsByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _personBenefitDependentsService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _personBenefitDependentsService.GetPersonBenefitDependentsByGuidAsync(guid);
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
        /// Create (POST) a new personBenefitDependents
        /// </summary>
        /// <param name="personBenefitDependents">DTO of the new personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonBenefitDependents> PostPersonBenefitDependentsAsync([FromBody] Dtos.PersonBenefitDependents personBenefitDependents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personBenefitDependents
        /// </summary>
        /// <param name="guid">GUID of the personBenefitDependents to update</param>
        /// <param name="personBenefitDependents">DTO of the updated personBenefitDependents</param>
        /// <returns>A personBenefitDependents object <see cref="Dtos.PersonBenefitDependents"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonBenefitDependents> PutPersonBenefitDependentsAsync([FromUri] string guid, [FromBody] Dtos.PersonBenefitDependents personBenefitDependents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personBenefitDependents
        /// </summary>
        /// <param name="guid">GUID to desired personBenefitDependents</param>
        [HttpDelete]
        public async Task DeletePersonBenefitDependentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}