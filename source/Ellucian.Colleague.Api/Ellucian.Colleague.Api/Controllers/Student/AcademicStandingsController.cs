// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Academic Standing Code data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicStandingsController : BaseCompressedApiController
    {
        private readonly IAcademicStandingsService _academicStandingsService;
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicStandingsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="academicStandingsService">Service of type <see cref="IAcademicStandingsService">IAcademicStandingsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AcademicStandingsController(IAdapterRegistry adapterRegistry, IAcademicStandingsService academicStandingsService, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _academicStandingsService = academicStandingsService;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all of the Academic Standings Codes
        /// </summary>
        /// <returns>All <see cref="AcademicStanding">AcademicStandings</see></returns>
        public async Task<IEnumerable<AcademicStanding>> GetAsync()
        {
            var academicStandingCollection =await  _referenceDataRepository.GetAcademicStandingsAsync();

            // Get the right adapter for the type mapping
            var academicStandingDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding, AcademicStanding>();

            // Map the AdvisorType entity to the program DTO
            var academicStandingDtoCollection = new List<AcademicStanding>();
            foreach (var academicStanding in academicStandingCollection)
            {
                academicStandingDtoCollection.Add(academicStandingDtoAdapter.MapToType(academicStanding));
            }

            return academicStandingDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves all accounting codes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All accounting codes objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AcademicStanding>> GetAcademicStandingsAsync()
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
                return await _academicStandingsService.GetAcademicStandingsAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Retrieves a accounting code by ID.
        /// </summary>
        /// <param name="id">Id of accounting code to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.AcademicStanding">accounting code.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicStanding> GetAcademicStandingByIdAsync(string id)
        {
            try
            {
                return await _academicStandingsService.GetAcademicStandingByIdAsync(id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Creates a AcademicStanding.
        /// </summary>
        /// <param name="academicStanding"><see cref="Dtos.AcademicStanding">AcademicStanding</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicStanding">AcademicStanding</see></returns>
        [HttpPost]
        public async Task<Dtos.AcademicStanding> PostAcademicStandingAsync([FromBody] Dtos.AcademicStanding academicStanding)
        {
            //Create is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Updates a accounting code.
        /// </summary>
        /// <param name="id">Id of the AcademicStanding to update</param>
        /// <param name="academicStanding"><see cref="Dtos.AcademicStanding">AcademicStanding</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicStanding">AcademicStanding</see></returns>
        [HttpPut]
        public async Task<Dtos.AcademicStanding> PutAcademicStandingAsync([FromUri] string id, [FromBody] Dtos.AcademicStanding academicStanding)
        {
            //Update is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing academicStanding
        /// </summary>
        /// <param name="id">Id of the academicStanding to delete</param>
        [HttpDelete]
        public async Task DeleteAcademicStandingAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HEDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
