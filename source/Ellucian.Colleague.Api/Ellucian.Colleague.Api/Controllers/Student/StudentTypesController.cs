// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

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
using System.Linq;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to StudentType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTypesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public StudentTypesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }
        /// <summary>
        /// Get all studentTypes.
        /// </summary>
        /// <returns>List of <see cref="StudentType">StudentType</see> data.</returns>
        public async Task<IEnumerable<StudentType>> GetAsync()
        {
            var studentTypeCollection = await _referenceDataRepository.GetStudentTypesAsync(false);

            // Get the right adapter for the type mapping
            var studentTypeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentType, StudentType>();

            // Map the student type entity to the student type DTO
            var studentTypeDtoCollection = new List<StudentType>();
            foreach (var studentType in studentTypeCollection)
            {
                studentTypeDtoCollection.Add(studentTypeDtoAdapter.MapToType(studentType));
            }

            return studentTypeDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all student types.
        /// </summary>
        /// <returns>All <see cref="StudentType">StudentTypes.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.StudentType>> GetStudentTypesAsync()
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
                var items = await _curriculumService.GetStudentTypesAsync(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an student type by ID.
        /// </summary>
        /// <returns>A <see cref="StudentType">StudentType.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.StudentType> GetStudentTypeByIdAsync(string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("Student type id is required.");
                }

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _curriculumService.GetStudentTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a StudentType.
        /// </summary>
        /// <param name="studentType"><see cref="StudentType">StudentType</see> to update</param>
        /// <returns>Newly updated <see cref="StudentType">StudentType</see></returns>
        [HttpPut]
        public async Task<Dtos.StudentType> PutStudentTypeAsync([FromBody] Dtos.StudentType studentType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a StudentType.
        /// </summary>
        /// <param name="studentType"><see cref="StudentType">StudentType</see> to create</param>
        /// <returns>Newly created <see cref="StudentType">StudentType</see></returns>
        [HttpPost]
        public async Task<Dtos.StudentType> PostStudentTypeAsync([FromBody] Dtos.StudentType studentType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing StudentType
        /// </summary>
        /// <param name="id">Id of the StudentType to delete</param>
        [HttpDelete]
        public async Task DeleteStudentTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}