// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

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
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Instructional Method data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructionalMethodsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly ICurriculumService _curriculumService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the InstructionalMethodsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="curriculumService">Service of type <see cref="ICurriculumService">ICurriculumService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public InstructionalMethodsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ICurriculumService curriculumService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _curriculumService = curriculumService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Instructional Methods.
        /// </summary>
        /// <returns>All <see cref="InstructionalMethod">Instructional Method</see> codes and descriptions.</returns>
        public async Task<IEnumerable<InstructionalMethod>> GetAsync()
        {
            var instructionalMethodCollection = await _referenceDataRepository.GetInstructionalMethodsAsync();

            // Get the right adapter for the type mapping
            var instructionalMethodDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.InstructionalMethod, InstructionalMethod>();

            // Map the instructional method entity to the instructional method DTO
            var instructionalMethodDtoCollection = new List<InstructionalMethod>();
            foreach (var instrMethod in instructionalMethodCollection)
            {
                instructionalMethodDtoCollection.Add(instructionalMethodDtoAdapter.MapToType(instrMethod));
            }

            return instructionalMethodDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Retrieves all instructional methods.
        /// </summary>
        /// <returns>All <see cref="Dtos.InstructionalMethod2">InstructionalMethods.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod2>> GetInstructionalMethods2Async()
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
                var items = await _curriculumService.GetInstructionalMethods2Async(bypassCache);

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

        /// <remarks>FOR USE WITH ELLUCIAN HEDM version 4</remarks>
        /// <summary>
        /// Retrieves an instructional method by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.InstructionalMethod2">InstructionalMethod.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.InstructionalMethod2> GetInstructionalMethodById2Async(string id)
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
                var building = await _curriculumService.GetInstructionalMethodById2Async(id);

                if (building != null)
                {

                    AddEthosContextProperties(await _curriculumService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _curriculumService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { building.Id }));
                }

                return building;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Creates a Instructional Method.
        /// </summary>
        /// <param name="instructionalMethod"><see cref="Dtos.InstructionalMethod2">InstructionalMethod</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.InstructionalMethod2">InstructionalMethod</see></returns>
        [HttpPost]
        public async Task<Dtos.InstructionalMethod2> PostInstructionalMethodsAsync([FromBody] Dtos.InstructionalMethod2 instructionalMethod)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a Instructional Method.
        /// </summary>
        /// <param name="id">Id of the Instructional Method to update</param>
        /// <param name="instructionalMethod"><see cref="Dtos.InstructionalMethod2">InstructionalMethod</see> to create</param>
        /// <returns>Updated <see cref="Dtos.InstructionalMethod2">InstructionalMethod</see></returns>
        [HttpPut]
        public async Task<Dtos.InstructionalMethod2> PutInstructionalMethodsAsync([FromUri] string id, [FromBody] Dtos.InstructionalMethod2 instructionalMethod)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing Instructional Method
        /// </summary>
        /// <param name="id">Id of the Instructional Method to delete</param>
        [HttpDelete]
        public async Task DeleteInstructionalMethodsAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}

