// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using System;
using Ellucian.Colleague.Dtos;
using System.Net;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to StudentStatus data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public StudentStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all student statuses.
        /// </summary>
        /// <returns>All <see cref="StudentStatus">StudentStatuses.</see></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentStatus>> GetStudentStatusesAsync()
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
                var studentStatuses = await _curriculumService.GetStudentStatusesAsync(bypassCache);

                if (studentStatuses != null && studentStatuses.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              studentStatuses.Select(a => a.Id).ToList()));
                }

                return studentStatuses;                
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an student status by ID.
        /// </summary>
        /// <returns>A <see cref="StudentStatus">StudentStatus.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.StudentStatus> GetStudentStatusByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetStudentStatusByIdAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e, string.Format("No student status was found for guid '{0}'.", id));
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a StudentStatus.
        /// </summary>
        /// <param name="studentStatus"><see cref="StudentStatus">StudentStatus</see> to update</param>
        /// <returns>Newly updated <see cref="StudentStatus">StudentStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.StudentStatus> PutStudentStatusAsync([FromBody] Dtos.StudentStatus studentStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a StudentStatus.
        /// </summary>
        /// <param name="studentStatus"><see cref="StudentStatus">StudentStatus</see> to create</param>
        /// <returns>Newly created <see cref="StudentStatus">StudentStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.StudentStatus> PostStudentStatusAsync([FromBody] Dtos.StudentStatus studentStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing StudentStatus
        /// </summary>
        /// <param name="id">Id of the StudentStatus to delete</param>
        [HttpDelete]
        public async Task DeleteStudentStatusAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}