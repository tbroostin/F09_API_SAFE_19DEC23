//Copyright 2017-2021 Ellucian Company L.P. and its affiliates

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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to StudentAptitudeAssessments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAptitudeAssessmentsController : BaseCompressedApiController
    {
        private readonly IStudentAptitudeAssessmentsService _studentAptitudeAssessmentsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAptitudeAssessmentsController class.
        /// </summary>
        /// <param name="studentAptitudeAssessmentsService">Service of type <see cref="IStudentAptitudeAssessmentsService">IStudentAptitudeAssessmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public StudentAptitudeAssessmentsController(IStudentAptitudeAssessmentsService studentAptitudeAssessmentsService, ILogger logger)
        {
            _studentAptitudeAssessmentsService = studentAptitudeAssessmentsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetStudentAptitudeAssessmentsAsync(Paging page)
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
                    page = new Paging(200, 0);
                }

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAptitudeAssessments)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        public async Task<IHttpActionResult> GetStudentAptitudeAssessments2Async(Paging page, QueryStringFilter criteria)
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
                    page = new Paging(200, 0);
                }
                var criteriaObj = GetFilterObject<Dtos.StudentAptitudeAssessments>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(new List<Dtos.StudentAptitudeAssessments>(), page, 0, this.Request);

                string studentFilter = (criteriaObj != null && criteriaObj.Student != null ? criteriaObj.Student.Id : "");

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessments2Async(studentFilter, page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAptitudeAssessments>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Return all studentAptitudeAssessments
        /// </summary>
        /// <returns>List of StudentAptitudeAssessments <see cref="Dtos.StudentAptitudeAssessments"/> objects representing matching studentAptitudeAssessments</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        [QueryStringFilterFilter("personFilter", typeof(Dtos.Filters.PersonFilterFilter2))]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentAptitudeAssessments2)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        public async Task<IHttpActionResult> GetStudentAptitudeAssessments3Async(Paging page, QueryStringFilter criteria, QueryStringFilter personFilter)
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
                    page = new Paging(200, 0);
                }

                string personFilterValue = string.Empty;
                var personFilterObj = GetFilterObject<Dtos.Filters.PersonFilterFilter2>(_logger, "personFilter");
                if ((personFilterObj != null) && (personFilterObj.personFilter != null))
                {
                    personFilterValue = personFilterObj.personFilter.Id;
                }

                var criteriaObj = GetFilterObject<Dtos.StudentAptitudeAssessments2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentAptitudeAssessments2>>(new List<Dtos.StudentAptitudeAssessments2>(), page, 0, this.Request);

                string studentFilter = (criteriaObj != null && criteriaObj.Student != null ? criteriaObj.Student.Id : "");
                string assessmentFilter = (criteriaObj != null && criteriaObj.Assessment != null ? criteriaObj.Assessment.Id : "");

                var pageOfItems = await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessments3Async(studentFilter,
                    assessmentFilter, personFilterValue, page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentAptitudeAssessments2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuidAsync(string guid)
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
                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuidAsync(guid);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentAptitudeAssessments> GetStudentAptitudeAssessmentsByGuid2Async(string guid)
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
                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid2Async(guid);
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
        /// Read (GET) a studentAptitudeAssessments using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentAptitudeAssessments2> GetStudentAptitudeAssessmentsByGuid3Async(string guid)
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
                AddEthosContextProperties(
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { guid }));

                return await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid3Async(guid);
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
        /// Update (PUT) an existing studentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.StudentAptitudeAssessments> PutStudentAptitudeAssessmentsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing StudentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A StudentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.StudentAptitudeAssessments> PutStudentAptitudeAssessments2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            //if (string.IsNullOrEmpty(guid))
            //{
            //    throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
            //        IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            //}
            //if (studentAptitudeAssessments == null)
            //{
            //    throw CreateHttpResponseException(new IntegrationApiException("Null studentAptitudeAssessments argument",
            //        IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            //}
            //if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            //{
            //    throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            //}
            //if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
            //{
            //    studentAptitudeAssessments.Id = guid.ToLowerInvariant();
            //}
            //else if (!string.Equals(guid, studentAptitudeAssessments.Id, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
            //        IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            //}
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                throw CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }

            try
            {
                // call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // merge and update the assessment
                var assessment = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessmentsAsync(
                    await PerformPartialPayloadMerge(studentAptitudeAssessments, async () => await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid2Async(guid, true),
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                    _logger));

                // store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing StudentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID of the studentAptitudeAssessments to update</param>
        /// <param name="studentAptitudeAssessments">DTO of the updated studentAptitudeAssessments</param>
        /// <returns>A StudentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentAptitudeAssessments2> PutStudentAptitudeAssessments3Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                  IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (studentAptitudeAssessments == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null studentAptitudeAssessments argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            //if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            //{
            //    throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            //}
            //if (string.IsNullOrEmpty(studentAptitudeAssessments.Id))
            //{
            //    studentAptitudeAssessments.Id = guid.ToLowerInvariant();
            //}
            //else if (!string.Equals(guid, studentAptitudeAssessments.Id, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
            //        IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            //}

            //validations to occur before partial put
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                throw CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }

            try
            {
                // call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                // merge and update the assessment
                var assessment = await _studentAptitudeAssessmentsService.UpdateStudentAptitudeAssessments2Async(
                    await PerformPartialPayloadMerge(studentAptitudeAssessments, async () => await _studentAptitudeAssessmentsService.GetStudentAptitudeAssessmentsByGuid3Async(guid, true),
                    await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                    _logger));

                // store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
        
        /// <summary>
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.StudentAptitudeAssessments> PostStudentAptitudeAssessmentsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentAptitudeAssessments> PostStudentAptitudeAssessments2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid studentAptitudeAssessments.", HttpStatusCode.BadRequest);
            }
           
            if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
                throw CreateHttpResponseException(new IntegrationApiException("Null source id",
                    IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            if (studentAptitudeAssessments.SpecialCircumstances != null)
            {
                foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
                {
                    if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
                    {
                        throw CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
                            IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
                    }
                }
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the assessment
                var assessment = await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessmentsAsync(studentAptitudeAssessments);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;

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
        /// Create (POST) a new studentAptitudeAssessments
        /// </summary>
        /// <param name="studentAptitudeAssessments">DTO of the new studentAptitudeAssessments</param>
        /// <returns>A studentAptitudeAssessments object <see cref="Dtos.StudentAptitudeAssessments"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Dtos.StudentAptitudeAssessments2> PostStudentAptitudeAssessments3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentAptitudeAssessments2 studentAptitudeAssessments)
        {
            if (studentAptitudeAssessments == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid studentAptitudeAssessments.", HttpStatusCode.BadRequest);
            }
            //if (studentAptitudeAssessments.Id != Guid.Empty.ToString())
            //{
            //    throw CreateHttpResponseException(new IntegrationApiException("Null guid must be supplied to create operation",
            //        IntegrationApiUtility.GetDefaultApiError("Null guid must be supplied to create operation")));
            //}
            //if (studentAptitudeAssessments.Source != null && (studentAptitudeAssessments.Source.Id == string.Empty || studentAptitudeAssessments.Source.Id == Guid.Empty.ToString()))
            //    throw CreateHttpResponseException(new IntegrationApiException("Null source id",
            //        IntegrationApiUtility.GetDefaultApiError("Source id cannot be empty.")));
            //if (studentAptitudeAssessments.SpecialCircumstances != null)
            //{
            //    foreach (var circ in studentAptitudeAssessments.SpecialCircumstances)
            //    {
            //        if ((string.IsNullOrEmpty(circ.Id)) || (circ.Id == Guid.Empty.ToString()))
            //        {
            //            throw CreateHttpResponseException(new IntegrationApiException("Null special circumstances id",
            //                IntegrationApiUtility.GetDefaultApiError("Special circumstances id cannot be empty.")));
            //        }
            //    }
            //}
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _studentAptitudeAssessmentsService.ImportExtendedEthosData(await ExtractExtendedData(await _studentAptitudeAssessmentsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the assessment
                var assessment = await _studentAptitudeAssessmentsService.CreateStudentAptitudeAssessments2Async(studentAptitudeAssessments);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _studentAptitudeAssessmentsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _studentAptitudeAssessmentsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { assessment.Id }));

                return assessment;
                
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
        /// Delete (DELETE) a studentAptitudeAssessments
        /// </summary>
        /// <param name="guid">GUID to desired studentAptitudeAssessments</param>
        [HttpDelete]
        public async Task DeleteStudentAptitudeAssessmentsAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null studentAptitudeAssessments id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                await _studentAptitudeAssessmentsService.DeleteStudentAptitudeAssessmentAsync(guid);
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
    }
}