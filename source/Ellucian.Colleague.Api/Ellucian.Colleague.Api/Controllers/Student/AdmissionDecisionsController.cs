//Copyright 2017-19 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AdmissionDecisions
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionDecisionsController : BaseCompressedApiController
    {
        private readonly IAdmissionDecisionsService _admissionDecisionsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionDecisionsController class.
        /// </summary>
        /// <param name="admissionDecisionsService">Service of type <see cref="IAdmissionDecisionsService">IAdmissionDecisionsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AdmissionDecisionsController(IAdmissionDecisionsService admissionDecisionsService, ILogger logger)
        {
            _admissionDecisionsService = admissionDecisionsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all admissionDecisions
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="personFilter">Selection from SaveListParms definition or person-filters</param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions})]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AdmissionDecisions)), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetAdmissionDecisionsAsync(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if (personFilterObj != null)
                {
                    if (personFilterObj.personFilter != null)
                    {
                        personFilterValue = personFilterObj.personFilter.Id;
                    }
                }

                var admissionDecision = GetFilterObject<Dtos.AdmissionDecisions>(_logger, "criteria");
                var filterQualifiers = GetFilterQualifiers(_logger);

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionDecisions>>(new List<Dtos.AdmissionDecisions>(), page, 0, this.Request);
                
                string applicationId = string.Empty; DateTimeOffset decidedOn = DateTime.MinValue;
                if (admissionDecision != null)
                {
                    applicationId = admissionDecision.Application != null ? admissionDecision.Application.Id : string.Empty;
                    if (admissionDecision.DecidedOn != DateTime.MinValue)
                    {
                        decidedOn = admissionDecision.DecidedOn;
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }
                var pageOfItems = await _admissionDecisionsService.GetAdmissionDecisionsAsync(page.Offset, page.Limit, 
                    applicationId, decidedOn, filterQualifiers, personFilterValue, bypassCache);

                AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AdmissionDecisions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a admissionDecisions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions})]

        public async Task<Dtos.AdmissionDecisions> GetAdmissionDecisionsByGuidAsync(string guid)
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
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                var admissionDecision = await _admissionDecisionsService.GetAdmissionDecisionsByGuidAsync(guid);

                if (admissionDecision != null)
                {

                    AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { admissionDecision.Id }));
                }


                return admissionDecision;
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
            catch (ArgumentNullException e)
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
        /// Create (POST) a new admissionDecisions
        /// </summary>
        /// <param name="admissionDecisions">DTO of the new admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.UpdateAdmissionDecisions)]
        public async Task<Dtos.AdmissionDecisions> PostAdmissionDecisionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.AdmissionDecisions admissionDecisions)
        {
            if (admissionDecisions == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid admission decision.", HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(admissionDecisions.Id))
            {
                throw CreateHttpResponseException("Id is required.");
            }

            if (admissionDecisions.Id != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("admissionDecisionsDto", "On a post you can not define a GUID.");
            }

            try
            {
                _admissionDecisionsService.ValidatePermissions(GetPermissionsMetaData());
                ValidateAdmissionDecisions(admissionDecisions);
                //call import extend method that needs the extracted extension data and the config
                await _admissionDecisionsService.ImportExtendedEthosData(await ExtractExtendedData(await _admissionDecisionsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the admission decision
                var admissionDecisionReturn = await _admissionDecisionsService.CreateAdmissionDecisionAsync(admissionDecisions);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _admissionDecisionsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _admissionDecisionsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { admissionDecisionReturn.Id }));

                return admissionDecisionReturn;
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
            catch (ArgumentNullException e)
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

        private void ValidateAdmissionDecisions(AdmissionDecisions admissionDecisions)
        {

            if (admissionDecisions.Application == null)
            {
                throw new ArgumentNullException("application", "Application is required.");
            }

            if (admissionDecisions.Application != null && string.IsNullOrEmpty(admissionDecisions.Application.Id))
            {
                throw new ArgumentNullException("application.id", "Application id is required.");
            }

            if (admissionDecisions.DecisionType == null)
            {
                throw new ArgumentNullException("decisionType", "Decision type is required.");
            }

            if (admissionDecisions.DecisionType != null && string.IsNullOrEmpty(admissionDecisions.DecisionType.Id))
            {
                throw new ArgumentNullException("decisionType.id", "Decision type id is required.");
            }

            if (admissionDecisions.DecidedOn == null || admissionDecisions.DecidedOn.Equals(default(DateTime)))
            {
                throw new ArgumentNullException("decidedOn", "Decided on is required.");
            }
        }

        /// <summary>
        /// Update (PUT) an existing admissionDecisions
        /// </summary>
        /// <param name="guid">GUID of the admissionDecisions to update</param>
        /// <param name="admissionDecisions">DTO of the updated admissionDecisions</param>
        /// <returns>A admissionDecisions object <see cref="Dtos.AdmissionDecisions"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.AdmissionDecisions> PutAdmissionDecisionsAsync([FromUri] string guid, [FromBody] Dtos.AdmissionDecisions admissionDecisions)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException("PUT",
                IntegrationApiUtility.GetDefaultApiError("Admission decision cannot be updated. Use POST to submit a new admission decision.")));

        }

        /// <summary>
        /// Delete (DELETE) a admissionDecisions
        /// </summary>
        /// <param name="guid">GUID to desired admissionDecisions</param>
        [HttpDelete]
        public async Task DeleteAdmissionDecisionsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}