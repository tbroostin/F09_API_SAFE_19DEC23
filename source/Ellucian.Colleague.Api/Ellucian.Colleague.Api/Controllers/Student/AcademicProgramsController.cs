// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicProgram data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicProgramsController : BaseCompressedApiController
    {
        private readonly IAcademicProgramService _academicProgramService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicProgramController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="academicProgramService">Repository of type <see cref="IAcademicProgramService">IAcademicPeriodService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AcademicProgramsController(IAdapterRegistry adapterRegistry, IAcademicProgramService academicProgramService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _academicProgramService = academicProgramService;
            _logger = logger;
        }

        /// <summary>
        /// Get all academicPrograms.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>
        public async Task<IEnumerable<AcademicProgram>> GetAsync()
        {
            try
            {
                return await _academicProgramService.GetAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Retrieves an academic program by GUID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicProgram">AcademicProgram</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicProgram2> GetAcademicProgramByIdV6Async(string id)
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
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuidV6Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Get all academicPrograms for HeDM version 6.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicProgram2>> GetAcademicProgramsV6Async()
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

                var items = await _academicProgramService.GetAcademicProgramsV6Async(bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Retrieves an academic program by GUID in V10 format.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicProgram">AcademicProgram</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicProgram3> GetAcademicProgramById3Async(string id)
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
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuid3Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Get all academicPrograms for HeDM version 10.
        /// </summary>
        /// <returns>List of <see cref="AcademicProgram">AcademicProgram</see> data.</returns>        
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.AcademicProgram3>> GetAcademicPrograms3Async()
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

                var items = await _academicProgramService.GetAcademicPrograms3Async(bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EeDM</remarks>
        /// <summary>
        /// Read (GET) a academicPrograms using a GUID
        /// </summary>
        /// <param name="id">GUID to desired academicPrograms</param>
        /// <returns>A academicPrograms object <see cref="Dtos.AcademicProgram4"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AcademicProgram4> GetAcademicProgramById4Async(string id)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _academicProgramService.GetAcademicProgramByGuid4Async(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        /// <summary>
        /// Return all academicPrograms
        /// </summary>
        /// <returns>List of AcademicPrograms <see cref="Dtos.AcademicProgram4"/> objects representing matching academicPrograms</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("academicCatalog", typeof(Dtos.Filters.AcademicCatalogFilter))]
        public async Task<IEnumerable<Dtos.AcademicProgram4>> GetAcademicPrograms4Async(QueryStringFilter academicCatalog)
        {
            var academicCatalogId = string.Empty;
            bool bypassCache = false;

            try
            {
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }              

                var academicCatalogFilter = GetFilterObject<Dtos.Filters.AcademicCatalogFilter>(_logger, "academicCatalog");

                if (CheckForEmptyFilterParameters())
                    return new List<Dtos.AcademicProgram4>(new List<Dtos.AcademicProgram4>());

                if ((academicCatalogFilter != null) && (academicCatalogFilter.AcademicCatalog != null)
                        && (!string.IsNullOrEmpty(academicCatalogFilter.AcademicCatalog.Id)))
                {
                    academicCatalogId = academicCatalogFilter.AcademicCatalog.Id;
                }
                var items = await _academicProgramService.GetAcademicPrograms4Async( academicCatalogId, bypassCache);

                AddEthosContextProperties(
                  await _academicProgramService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _academicProgramService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      items.Select(i => i.Id).Distinct().ToList()));

                return items;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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

        #region PUT/POST
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a AcademicProgram.
        /// </summary>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicProgram">AcademicProgram</see></returns>
        [HttpPost]
        public Dtos.AcademicProgram PostAcademicProgram([FromBody] Dtos.AcademicProgram academicProgram)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Academic Program.
        /// </summary>
        /// <param name="id">Id of the Academic Program to update</param>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicProgram">AcademicProgram</see></returns>
        [HttpPut]
        public Dtos.AcademicProgram PutAcademicProgram([FromUri] string id, [FromBody] Dtos.AcademicProgram academicProgram)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Creates a AcademicProgram.
        /// </summary>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicProgram3">AcademicProgram</see></returns>
        [HttpPost]
        public Dtos.AcademicProgram PostAcademicProgram3([FromBody] Dtos.AcademicProgram3 academicProgram)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Updates a Academic Program.
        /// </summary>
        /// <param name="id">Id of the Academic Program to update</param>
        /// <param name="academicProgram"><see cref="Dtos.AcademicProgram">AcademicProgram</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicProgra3m">AcademicProgram</see></returns>
        [HttpPut]
        public Dtos.AcademicProgram PutAcademicProgram3([FromUri] string id, [FromBody] Dtos.AcademicProgram3 academicProgram)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Delete (DELETE) an existing Academic Program
        /// </summary>
        /// <param name="id">Id of the Academic Program to delete</param>
        [HttpDelete]
        public void DeleteAcademicProgram([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
