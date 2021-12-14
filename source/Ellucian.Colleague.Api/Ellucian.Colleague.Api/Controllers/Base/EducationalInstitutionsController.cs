// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

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
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Educational Institutions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class EducationalInstitutionsController : BaseCompressedApiController
    {
        private readonly IEducationalInstitutionsService _educationalInstitutionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EducationalInstitutionsController class.
        /// </summary>
        /// <param name="educationalInstitutionsService">Service of type <see cref="IEducationalInstitutionsService">IEducationalInstitutionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public EducationalInstitutionsController(IEducationalInstitutionsService educationalInstitutionsService, ILogger logger)
        {
            _educationalInstitutionsService = educationalInstitutionsService;
            _logger = logger;
        }


        /// <summary>
        /// Return Educational-Institutions 
        /// </summary>
        /// <param name="page">paging information</param>
        /// <param name="type">Type of Educational-Institution ex:"secondary" or "postSecondary"</param>
        /// <param name="criteria">criteria</param>
        /// <returns>List of EducationalInstitutions <see cref="Dtos.EducationalInstitution"/> objects representing matching educationalInstitutions</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.EducationalInstitution))]
        [ValidateQueryStringFilter(new string[] { "type", "credentials.type", "credentials.value" }, false, true)]
        public async Task<IHttpActionResult> GetEducationalInstitutionsAsync(Paging page, [FromUri] string type = "", QueryStringFilter criteria = null)
        {
           
            var bypassCache = false;
            if (type == null || type == "null")
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request);
            }

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                _educationalInstitutionsService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var criteriaObject = GetFilterObject<Dtos.EducationalInstitution>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request);



                Dtos.EnumProperties.EducationalInstitutionType? typeFilter = null;

                if (!string.IsNullOrEmpty(type))
                {
                    switch (type.ToLower())
                    {
                        case "postsecondaryschool":
                            typeFilter = EducationalInstitutionType.PostSecondarySchool;
                            break;
                        case "secondaryschool":
                            typeFilter = EducationalInstitutionType.SecondarySchool;
                            break;
                        default:
                            return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request); ;
                    }
                }
                else if (criteriaObject.Type != EducationalInstitutionType.NotSet)
                {

                    switch (criteriaObject.Type)
                    {
                        case EducationalInstitutionType.PostSecondarySchool:
                            typeFilter = EducationalInstitutionType.PostSecondarySchool;
                            break;
                        case EducationalInstitutionType.SecondarySchool:
                            typeFilter = EducationalInstitutionType.SecondarySchool;
                            break;
                        default:
                            return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request);
                    }
                }

                //we need to validate the credentials
                if (criteriaObject.Credentials != null && criteriaObject.Credentials.Any())
                {
                    if (criteriaObject.Credentials.Count() > 1)
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request);
                    }
                    var credential = criteriaObject.Credentials.FirstOrDefault();

                    if ((credential.Type != null) || !string.IsNullOrEmpty(credential.Value))
                    {
                        if (!string.IsNullOrEmpty(credential.Type.ToString()) && string.IsNullOrEmpty(credential.Value))
                        {
                            throw new ArgumentException("credentialValue", "credentialValue is required when requesting a credentialType");
                        }
                        if (string.IsNullOrEmpty(credential.Type.ToString()) && !string.IsNullOrEmpty(credential.Value))
                        {
                            throw new ArgumentException("credentialType", "credentialType is required when requesting a credentialValue");
                        }
                    }
                    if (credential.Type != Credential3Type.ColleaguePersonId)
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(new List<Dtos.EducationalInstitution>(), page, 0, this.Request);
                    }
                }

                var pageOfItems = await _educationalInstitutionsService.GetEducationalInstitutionsByTypeAsync(page.Offset, page.Limit, criteriaObject, typeFilter, bypassCache);

                AddEthosContextProperties(
                    await _educationalInstitutionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _educationalInstitutionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.EducationalInstitution>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) an Educational-Institution-Unit using a GUID
        /// </summary>
        /// <param name="id">GUID to desired educationalInstitution</param>
        /// <returns>An EducationalInstitutions object <see cref="Dtos.EducationalInstitution"/> in DataModel format</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter, PermissionsFilter(BasePermissionCodes.ViewEducationalInstitution)]
        public async Task<Dtos.EducationalInstitution> GetEducationalInstitutionsByGuidAsync(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                _educationalInstitutionsService.ValidatePermissions(GetPermissionsMetaData());
                AddEthosContextProperties(
                    await _educationalInstitutionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _educationalInstitutionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _educationalInstitutionsService.GetEducationalInstitutionByGuidAsync(id);
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
        /// Create (POST) a new Educational-Institution
        /// </summary>
        /// <param name="educationalInstitution">DTO of the new educationalInstitutionUnits</param>
        /// <returns>A educationalInstitutionUnits object <see cref="Dtos.EducationalInstitution"/> in Data Model format</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.EducationalInstitution> PostEducationalInstitutionsAsync([FromBody] Dtos.EducationalInstitution educationalInstitution)
        {
            //Post is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }

        /// <summary>
        /// Update (PUT) an existing Educational-Institution
        /// </summary>
        /// <param name="id">GUID of the EducationalInstitutions to update</param>
        /// <param name="educationalInstitution">DTO of the updated EducationalInstitutions</param>
        /// <returns>A EducationalInstitutions object <see cref="Dtos.EducationalInstitution"/> in Data Model format</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.EducationalInstitution> PutEducationalInstitutionsAsync([FromUri] string id, [FromBody] Dtos.EducationalInstitution educationalInstitution)
        {
            //Put is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }

        /// <summary>
        /// Delete (DELETE) a Educational-Institution
        /// </summary>
        /// <param name="id">GUID to desired EducationalInstitutions</param>
        [HttpDelete, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task DeleteEducationalInstitutionByGuidAsync(string id)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);

        }
    }
}