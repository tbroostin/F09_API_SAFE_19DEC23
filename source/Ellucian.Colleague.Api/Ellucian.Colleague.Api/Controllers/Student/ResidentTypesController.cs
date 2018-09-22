// Copyright 2016 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to ResidentType data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ResidentTypesController : BaseCompressedApiController
    {
        private readonly IStudentService _studentService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ResidentTypesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentService">Service of type <see cref="IStudentService">IStudentService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public ResidentTypesController(IAdapterRegistry adapterRegistry, IStudentService studentService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentService = studentService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves all resident types.
        /// </summary>
        /// <returns>All <see cref="ResidentType">ResidentTypes.</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ResidentType>> GetResidentTypesAsync()
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
                return await _studentService.GetResidentTypesAsync(bypassCache);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM Version 6</remarks>
        /// <summary>
        /// Retrieves an resident type by ID.
        /// </summary>
        /// <returns>A <see cref="ResidentType">ResidentType.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.ResidentType> GetResidentTypeByIdAsync(string id)
        {
            try
            {
                return await _studentService.GetResidentTypeByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <summary>
        /// Updates a ResidentType.
        /// </summary>
        /// <param name="residentType"><see cref="ResidentType">ResidentType</see> to update</param>
        /// <returns>Newly updated <see cref="ResidentType">ResidentType</see></returns>
        [HttpPut]
        public async Task<Dtos.ResidentType> PutResidentTypeAsync([FromBody] Dtos.ResidentType residentType)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Creates a ResidentType.
        /// </summary>
        /// <param name="residentType"><see cref="ResidentType">ResidentType</see> to create</param>
        /// <returns>Newly created <see cref="ResidentType">ResidentType</see></returns>
        [HttpPost]
        public async Task<Dtos.ResidentType> PostResidentTypeAsync([FromBody] Dtos.ResidentType residentType)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing ResidentType
        /// </summary>
        /// <param name="id">Id of the ResidentType to delete</param>
        [HttpDelete]
        public async Task DeleteResidentTypeAsync(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}


