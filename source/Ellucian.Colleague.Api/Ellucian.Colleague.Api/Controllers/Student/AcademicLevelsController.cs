// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net;
using System.Linq;
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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to AcademicLevels
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicLevelsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// AcademicLevelsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AcademicLevelsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Academic Levels.
        /// </summary>
        /// <returns>All <see cref="AcademicLevel">Academic Level</see> codes and descriptions.</returns>
        public async Task<IEnumerable<AcademicLevel>> GetAsync()
        {
            try
            {
                var academicLevelCollection = await _studentReferenceDataRepository.GetAcademicLevelsAsync();

                // Get the right adapter for the type mapping
                var academicLevelDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicLevel, AcademicLevel>();

                // Map the academiclevel entity to the program DTO
                var academicLevelDtoCollection = new List<AcademicLevel>();
                foreach (var academicLevel in academicLevelCollection)
                {
                    academicLevelDtoCollection.Add(academicLevelDtoAdapter.MapToType(academicLevel));
                }

                return academicLevelDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving academic levels";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString() + ex.StackTrace);
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 5</remarks>
        /// <summary>
        /// Retrieves all academic levels.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevels.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel2>> GetAcademicLevels3Async()
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

                var items = await _curriculumService.GetAcademicLevels2Async(bypassCache);

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        items.Select(i => i.Id).ToList()));

                return items;
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 5</remarks>
        /// <summary>
        /// Retrieves an academic level by ID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.AcademicLevel2> GetAcademicLevelById3Async(string id)
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
                    throw new ArgumentNullException("Academic Level id is required.");
                }

                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));

                return await _curriculumService.GetAcademicLevelById2Async(id);
            }
            catch (PermissionsException e)
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
        /// Updates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to update</param>
        /// <returns>Newly updated <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPut]
        public async Task<Dtos.AcademicLevel2> PutAcademicLevelsAsync([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to create</param>
        /// <returns>Newly created <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPost]
        public async Task<Dtos.AcademicLevel2> PostAcademicLevelsAsync([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to update</param>
        /// <returns>Newly updated <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPut]
        public async Task<Dtos.AcademicLevel2> PutAcademicLevels2Async([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a AcademicLevel.
        /// </summary>
        /// <param name="academicLevel"><see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see> to create</param>
        /// <returns>Newly created <see cref="Ellucian.Colleague.Dtos.AcademicLevel2">AcademicLevel</see></returns>
        [HttpPost]
        public async Task<Dtos.AcademicLevel2> PostAcademicLevels2Async([FromBody] Dtos.AcademicLevel2 academicLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicLevel
        /// </summary>
        /// <param name="id">Id of the AcademicLevel to delete</param>
        [HttpDelete]
        public async Task DeleteAcademicLevels2Async(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
