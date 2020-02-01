// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to SectionRegistrationStatuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionRegistrationStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// SectionRegistrationStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public SectionRegistrationStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all SectionRegistrationStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2">SectionRegistrationStatus.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2>> GetSectionRegistrationStatuses2Async()
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
                return await _curriculumService.GetSectionRegistrationStatuses2Async(bypassCache);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a SectionRegistrationStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired SectionRegistrationStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2">SectionRegistrationStatus.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem2> GetSectionRegistrationStatusById2Async(string id)
        {
            try
            {
                return await _curriculumService.GetSectionRegistrationStatusById2Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH EEDM</remarks>
        /// <summary>
        /// Retrieves all SectionRegistrationStatuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3">SectionRegistrationStatus.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3>> GetSectionRegistrationStatuses3Async()
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
                return await _curriculumService.GetSectionRegistrationStatuses3Async(bypassCache);
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

        /// <remarks>FOR USE WITH EEDM</remarks>
        /// <summary>
        /// Retrieves a SectionRegistrationStatus by ID.
        /// </summary>
        /// <param name="id">ID to desired SectionRegistrationStatus</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3">SectionRegistrationStatus.</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        public async Task<Ellucian.Colleague.Dtos.SectionRegistrationStatusItem3> GetSectionRegistrationStatusById3Async(string id)
        {
            try
            {
                return await _curriculumService.GetSectionRegistrationStatusById3Async(id);
            }
            catch (IntegrationApiException e)
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
        /// Updates a SectionRegistrationStatus.
        /// </summary>
        /// <param name="sectionRegistrationStatus"><see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see> to update</param>
        /// <returns>Newly updated <see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see></returns>
        [HttpPut]
        public async Task<Dtos.SectionRegistrationStatusItem2> PutSectionRegistrationStatusesAsync([FromBody] Dtos.SectionRegistrationStatusItem2 sectionRegistrationStatus)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a SectionRegistrationStatus.
        /// </summary>
        /// <param name="sectionRegistrationStatus"><see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see> to create</param>
        /// <returns>Newly created <see cref="SectionRegistrationStatusItem2">SectionRegistrationStatus</see></returns>
        [HttpPost]
        public async Task<Dtos.SectionRegistrationStatusItem2> PostSectionRegistrationStatusesAsync([FromBody] Dtos.SectionRegistrationStatusItem2 sectionRegistrationStatus)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing SectionRegistrationStatus.
        /// </summary>
        /// <param name="id">Id of the SectioinRegistrationStatus to delete</param>
        [HttpDelete]
        public async Task DeleteSectionRegistrationStatusesAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
