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
    /// Provides access to PersonBeneficiaries
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonBeneficiariesController : BaseCompressedApiController
    {
        private readonly IPersonBeneficiariesService _personBeneficiariesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PersonBeneficiariesController class.
        /// </summary>
        /// <param name="personBeneficiariesService">Service of type <see cref="IPersonBeneficiariesService">IPersonBeneficiariesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public PersonBeneficiariesController(IPersonBeneficiariesService personBeneficiariesService, ILogger logger)
        {
            _personBeneficiariesService = personBeneficiariesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all personBeneficiaries
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of PersonBeneficiaries <see cref="Dtos.PersonBeneficiaries"/> objects representing matching personBeneficiaries</returns>
        [HttpGet, EedmResponseFilter]

        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetPersonBeneficiariesAsync(Paging page)
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
                AddDataPrivacyContextProperty((await _personBeneficiariesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                var pageOfItems = await _personBeneficiariesService.GetPersonBeneficiariesAsync(page.Offset, page.Limit, bypassCache);
                return new PagedHttpActionResult<IEnumerable<Dtos.PersonBeneficiaries>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a personBeneficiaries using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired personBeneficiaries</param>
        /// <returns>A personBeneficiaries object <see cref="Dtos.PersonBeneficiaries"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.PersonBeneficiaries> GetPersonBeneficiariesByGuidAsync(string guid)
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
                AddDataPrivacyContextProperty((await _personBeneficiariesService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                return await _personBeneficiariesService.GetPersonBeneficiariesByGuidAsync(guid);
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
        /// Create (POST) a new personBeneficiaries
        /// </summary>
        /// <param name="personBeneficiaries">DTO of the new personBeneficiaries</param>
        /// <returns>A personBeneficiaries object <see cref="Dtos.PersonBeneficiaries"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.PersonBeneficiaries> PostPersonBeneficiariesAsync([FromBody] Dtos.PersonBeneficiaries personBeneficiaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing personBeneficiaries
        /// </summary>
        /// <param name="guid">GUID of the personBeneficiaries to update</param>
        /// <param name="personBeneficiaries">DTO of the updated personBeneficiaries</param>
        /// <returns>A personBeneficiaries object <see cref="Dtos.PersonBeneficiaries"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.PersonBeneficiaries> PutPersonBeneficiariesAsync([FromUri] string guid, [FromBody] Dtos.PersonBeneficiaries personBeneficiaries)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a personBeneficiaries
        /// </summary>
        /// <param name="guid">GUID to desired personBeneficiaries</param>
        [HttpDelete]
        public async Task DeletePersonBeneficiariesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}