// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to EnrollmentStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EnrollmentStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// EnrollmentStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public EnrollmentStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all EnrollmentStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.EnrollmentStatus">EnrollmentStatus.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EnrollmentStatus>> GetEnrollmentStatusesAsync()
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
                return await _curriculumService.GetEnrollmentStatusesAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a EnrollmentStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired EnrollmentStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.EnrollmentStatus">EnrollmentStatus.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.EnrollmentStatus> GetEnrollmentStatusByIdAsync(string id)
        {
            try
            {
                return await _curriculumService.GetEnrollmentStatusByGuidAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a EnrollmentStatus.
        /// </summary>
        /// <param name="enrollmentStatus"><see cref="EnrollmentStatus">EnrollmentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="EnrollmentStatus">EnrollmentStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.EnrollmentStatus> PutEnrollmentStatusAsync([FromBody] Dtos.EnrollmentStatus enrollmentStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a EnrollmentStatus.
        /// </summary>
        /// <param name="enrollmentStatus"><see cref="EnrollmentStatus">EnrollmentStatus</see> to create</param>
        /// <returns>Newly created <see cref="EnrollmentStatus">EnrollmentStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.EnrollmentStatus> PostEnrollmentStatusAsync([FromBody] Dtos.EnrollmentStatus enrollmentStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing EnrollmentStatus.
        /// </summary>
        /// <param name="id">Id of the EnrollmentStatus to delete</param>
        [HttpDelete]
        public async Task DeleteEnrollmentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
