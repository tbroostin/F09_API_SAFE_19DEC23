// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Linq;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to Academic Disciplines data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicDisciplinesController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAcademicDisciplineService _academicDisciplineService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicDisciplinesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicDisciplineService">Service of type <see cref="IAcademicDisciplineService">IAcademicDisciplineService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AcademicDisciplinesController(IAdapterRegistry adapterRegistry, IAcademicDisciplineService academicDisciplineService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _academicDisciplineService = academicDisciplineService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM V15 (Same as V10, but adds filter and named query) </remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines.  
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline3">AcademicDiscipline </see>objects.</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AcademicDiscipline3))]
        [QueryStringFilterFilter("majorStatus", typeof(Dtos.Filters.MajorStatusFilter))]
        public async Task<IEnumerable<Dtos.AcademicDiscipline3>> GetAcademicDisciplines3Async(QueryStringFilter criteria, QueryStringFilter majorStatus)
        {
            Dtos.EnumProperties.MajorStatus status = Dtos.EnumProperties.MajorStatus.NotSet;
            string type = "";


            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (CheckForEmptyFilterParameters())
            {
                throw CreateHttpResponseException(new IntegrationApiException("Empty filter parameter supplied."), HttpStatusCode.BadRequest);
            }
            
            try
            {
                // Check for discipline type filter
                var criteriaObj = GetFilterObject<Dtos.AcademicDiscipline3>(_logger, "criteria");

                if (criteriaObj != null)
                {
                    if (criteriaObj.Type == AcademicDisciplineType2.Major) type = "major";
                    if (criteriaObj.Type == AcademicDisciplineType2.Minor) type = "minor";
                    if (criteriaObj.Type == AcademicDisciplineType2.Concentration) type = "concentration";
                }

                // Check for named query majorStatus
                var majorStatusObj = GetFilterObject<Dtos.Filters.MajorStatusFilter>(_logger, "majorStatus");
                if (majorStatusObj != null)
                {
                    status = majorStatusObj.MajorStatus;
                }

                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplines3Async(status, type, bypassCache);

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return disciplineDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
                        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM V7, V10</remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines. 
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline2">AcademicDiscipline </see>objects.</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicDiscipline2>> GetAcademicDisciplines2Async()
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplines2Async(bypassCache);

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return disciplineDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM V6</remarks>
        /// <summary>
        /// Retrieves all Academic Disciplines.
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicDiscipline">AcademicDiscipline </see>objects.</returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicDiscipline>> GetAcademicDisciplinesAsync()
        {

            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDtos = await _academicDisciplineService.GetAcademicDisciplinesAsync(bypassCache);
                var x = disciplineDtos.First().Id;

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          disciplineDtos.Select(dd => dd.Id).ToList()));
                return disciplineDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic discipline by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicDiscipline2">AcademicDiscipline2 </see>object.</returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.AcademicDiscipline2> GetAcademicDiscipline2ByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                var disciplineDto = await _academicDisciplineService.GetAcademicDiscipline2ByGuidAsync(id);
                if (disciplineDto == null)
                {
                    throw new KeyNotFoundException("Academic Discipline not found for GUID " + id);
                }

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { disciplineDto.Id }));
                return disciplineDto;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic discipline by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicDiscipline">AcademicDiscipline </see>object.</returns>
        /// 
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.AcademicDiscipline> GetAcademicDisciplineByIdAsync(string id)
        {
            bool bypassCache = false;
            if (Request != null && Request.Headers != null && Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                var disciplineDto = await _academicDisciplineService.GetAcademicDisciplineByGuidAsync(id);
                if (disciplineDto == null)
                {
                    throw new KeyNotFoundException("Academic Discipline not found for GUID " + id);
                }

                AddEthosContextProperties(await _academicDisciplineService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                          await _academicDisciplineService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                          new List<string>() { disciplineDto.Id }));
                return disciplineDto;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }



        /// <summary>        
        /// Creates a AcademicDiscipline.
        /// </summary>
        /// <param name="academicDiscipline"><see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see></returns>
        [HttpPost]
        public Dtos.AcademicDiscipline PostAcademicDisciplines([FromBody] Dtos.AcademicDiscipline academicDiscipline)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicDiscipline.
        /// </summary>
        /// <param name="id">Id of the AcademicDiscipline to update</param>
        /// <param name="academicDiscipline"><see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicDiscipline">AcademicDiscipline</see></returns>
        [HttpPut]
        public Dtos.AcademicDiscipline PutAcademicDisciplines([FromUri] string id, [FromBody] Dtos.AcademicDiscipline academicDiscipline)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicDiscipline
        /// </summary>
        /// <param name="id">Id of the AcademicDiscipline to delete</param>
        [HttpDelete]
        public void DeleteAcademicDisciplines([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
