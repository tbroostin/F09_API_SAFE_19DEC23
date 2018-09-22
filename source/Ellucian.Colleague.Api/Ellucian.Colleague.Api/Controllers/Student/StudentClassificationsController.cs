// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.Transcripts;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using StudentClassification = Ellucian.Colleague.Dtos.StudentClassification;


namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Accesses Student classification data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentClassificationsController : BaseCompressedApiController
    {
        private readonly IStudentService _studentService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentsClassificationsController class.
        /// </summary>
        /// <param name="adapterRegistry">adapterRegistry</param>
        /// <param name="studentService">studentService</param>
        /// <param name="logger">logger</param>
        public StudentClassificationsController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger)
        {
            _studentService = studentService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Gets all student classification
        /// </summary>
        /// <returns></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<StudentClassification>> GetStudentClassificationsAsync()
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var classificationEntities = await _studentService.GetAllStudentClassificationsAsync(bypassCache);

                AddEthosContextProperties(
                        await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            classificationEntities.Select(sc => sc.Id).ToList()));

                return classificationEntities;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// Retrieves a student classification by id.
        /// </summary>
        /// <param name="id">Id of students classification to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentClassification">student classification.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<StudentClassification> GetStudentClassificationByIdAsync(string id)
        {
            bool bypassCache = false;
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
                        await _studentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                        await _studentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                            new List<string>() { id }));

                return await _studentService.GetStudentClassificationByGuidAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <summary>
        /// POST student classification
        /// </summary>
        /// <param name="studentClassification"></param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.StudentClassification">student Classification.</see></returns>
        [HttpPost]
        public async Task<StudentClassification> PostStudentClassificationAsync([FromBody] StudentClassification studentClassification)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// PUT student classification
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentClassification"></param>
        /// <returns>Dtos.StudentsClassification</returns>
        [HttpPut]
        public async Task<StudentClassification> PutStudentClassificationAsync([FromUri] string id, [FromBody] StudentClassification studentClassification)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete student classification
        /// </summary>
        /// <param name="id">id</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteStudentClassificationAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}