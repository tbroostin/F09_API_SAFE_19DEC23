﻿// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.

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
    /// Provides access to Grade Scheme data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class GradeSchemesController : BaseCompressedApiController
    {
        private readonly IGradeSchemeService _gradeSchemeService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the GradeSchemesController class.
        /// </summary>
        /// <param name="gradeSchemeService">Service of type <see cref="IGradeSchemeService">IGradeSchemeService</see></param>
        /// <param name="logger">Interface to logger</param>
        public GradeSchemesController(IGradeSchemeService gradeSchemeService, ILogger logger)
        {
            _gradeSchemeService = gradeSchemeService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all grade schemes.
        /// </summary>
        /// <returns>All GradeScheme objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme>> GetGradeSchemesAsync()
        {
            try
            {
                return await _gradeSchemeService.GetGradeSchemesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves a grade scheme by GUID.
        /// </summary>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GradeScheme">GradeScheme.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.GradeScheme> GetGradeSchemeByGuidAsync(string guid)
        {
            try
            {
                return await _gradeSchemeService.GetGradeSchemeByGuidAsync(guid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all grade schemes.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All GradeScheme objects.</returns>

        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme2>> GetGradeSchemes2Async()
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
                var gradeSchemeDtos = await _gradeSchemeService.GetGradeSchemes2Async(bypassCache);
                AddEthosContextProperties(
                    await _gradeSchemeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _gradeSchemeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        gradeSchemeDtos.Select(i => i.Id).ToList()));
                return gradeSchemeDtos;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves a grade scheme by ID.
        /// </summary>
        /// <param name="id">Id of Grade Scheme to retrieve</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.GradeScheme2">GradeScheme.</see></returns>
        [EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.GradeScheme2> GetGradeSchemeByIdAsync(string id)
        {
            try
            {
                AddEthosContextProperties(
                   await _gradeSchemeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo()),
                   await _gradeSchemeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _gradeSchemeService.GetGradeSchemeByIdAsync(id);                 
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a GradeScheme.
        /// </summary>
        /// <param name="gradeScheme"><see cref="Dtos.GradeScheme2">GradeScheme</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.GradeScheme2">GradeScheme</see></returns>
        [HttpPost]
        public async Task<Dtos.GradeScheme2> PostGradeSchemeAsync([FromBody] Dtos.GradeScheme2 gradeScheme)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Grade Scheme.
        /// </summary>
        /// <param name="id">Id of the Grade Scheme to update</param>
        /// <param name="gradeScheme"><see cref="Dtos.GradeScheme2">GradeScheme</see> to create</param>
        /// <returns>Updated <see cref="Dtos.GradeScheme2">GradeScheme</see></returns>
        [HttpPut]
        public async Task<Dtos.GradeScheme2> PutGradeSchemeAsync([FromUri] string id, [FromBody] Dtos.GradeScheme2 gradeScheme)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Grade Scheme
        /// </summary>
        /// <param name="id">Id of the Grade Scheme to delete</param>
        [HttpDelete]
        public async Task DeleteGradeSchemeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


    }
}
