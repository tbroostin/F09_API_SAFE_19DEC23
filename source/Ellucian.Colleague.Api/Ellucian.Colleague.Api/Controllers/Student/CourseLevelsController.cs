// Copyright 2012-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using System.Linq;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Course Level data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class CourseLevelsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CourseLevelsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public CourseLevelsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Course Levels.
        /// </summary>
        /// <returns>All <see cref="CourseLevel">Course Level</see> codes and descriptions.</returns>
        public async Task<IEnumerable<CourseLevel>> GetAsync()
        {
            var courseLevelCollection = await _referenceDataRepository.GetCourseLevelsAsync();

            // Get the right adapter for the type mapping
            var courseLevelDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseLevel, CourseLevel>();

            // Map the courselevel entity to the program DTO
            var courseLevelDtoCollection = new List<CourseLevel>();
            foreach (var courseLevel in courseLevelCollection)
            {
                courseLevelDtoCollection.Add(courseLevelDtoAdapter.MapToType(courseLevel));
            }

            return courseLevelDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves all course levels.
        /// </summary>
        /// <returns>All <see cref="CourseLevel2">CourseLevels.</see></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CourseLevel2>> GetCourseLevels2Async()
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
                var courseLevels = await _curriculumService.GetCourseLevels2Async(bypassCache);

                if (courseLevels != null && courseLevels.Any())
                {
                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              courseLevels.Select(a => a.Id).ToList()));
                }
                return courseLevels;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Retrieves a course level by ID.
        /// </summary>
        /// <returns>A <see cref="CourseLevel2">CourseLevel.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.CourseLevel2> GetCourseLevelById2Async(string id)
        {
            try
            {
                AddEthosContextProperties(
                    await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                    await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                return await _curriculumService.GetCourseLevelById2Async(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

         /// <summary>
        /// Creates a Course Level.
        /// </summary>
        /// <param name="courseLevel"><see cref="CourseLevel2">CourseLevel</see> to create</param>
        /// <returns>Newly created <see cref="CourseLevel2">InstructionalMethod</see></returns>
        [HttpPost]
        public async Task<Dtos.CourseLevel2> PostCourseLevelsAsync([FromBody] Dtos.CourseLevel2 courseLevel)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Course Level.
        /// </summary>
        /// <param name="id">Id of the Course Level to update</param>
        /// <param name="courseLevel"><see cref="CourseLevel2">CourseLevel</see> to create</param>
        /// <returns>Updated <see cref="CourseLevel">CourseLevel</see></returns>
        [HttpPut]
        public async Task<Dtos.CourseLevel2> PutCourseLevelsAsync([FromUri] string id, [FromBody] Dtos.CourseLevel2 courseLevel)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Course Level.
        /// </summary>
        /// <param name="id">Id of the Course Level to delete</param>
        [HttpDelete]
        public async Task DeleteCourseLevelsAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
