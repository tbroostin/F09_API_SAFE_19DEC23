// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicPeriodEnrollmentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicPeriodEnrollmentStatusesController : BaseCompressedApiController
    {
        //private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// AcademicPeriodEnrollmentStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AcademicPeriodEnrollmentStatusesController(IAdapterRegistry adapterRegistry, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            //_studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves all AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus">EnrollmentStatus.</see></returns>
        /// 
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus>> GetAcademicPeriodEnrollmentStatusesAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var academicPeriodEnrollmentStatus = await _curriculumService.GetAcademicPeriodEnrollmentStatusesAsync(bypassCache);

                if (academicPeriodEnrollmentStatus != null && academicPeriodEnrollmentStatus.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              academicPeriodEnrollmentStatus.Select(a => a.Id).ToList()));
                }

                return academicPeriodEnrollmentStatus;                
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Retrieves a AcademicPeriodEnrollmentStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired AcademicPeriodEnrollmentStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        /// 
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.AcademicPeriodEnrollmentStatus> GetAcademicPeriodEnrollmentStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetAcademicPeriodEnrollmentStatusByGuidAsync(id);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Updates a AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="id"><see cref="id">id</see></param>
        /// <param name="academicPeriodEnrollmentStatus"><see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.AcademicPeriodEnrollmentStatus> PutAcademicPeriodEnrollmentStatusAsync([FromUri] string id, [FromBody] Dtos.AcademicPeriodEnrollmentStatus academicPeriodEnrollmentStatus)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="academicPeriodEnrollmentStatus"><see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see> to create</param>
        /// <returns>Newly created <see cref="AcademicPeriodEnrollmentStatus">AcademicPeriodEnrollmentStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.AcademicPeriodEnrollmentStatus> PostAcademicPeriodEnrollmentStatusAsync([FromBody] Dtos.AcademicPeriodEnrollmentStatus academicPeriodEnrollmentStatus)
        {
            //Update is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicPeriodEnrollmentStatus.
        /// </summary>
        /// <param name="id">Id of the AcademicPeriodEnrollmentStatus to delete</param>
        [HttpDelete]
        public async Task DeleteAcademicPeriodEnrollmentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
