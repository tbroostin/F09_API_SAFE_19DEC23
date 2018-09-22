// Copyright 2016 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.Resources;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for Admission Application Types
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionApplicationTypesController : BaseCompressedApiController
    {
        private readonly IAdmissionApplicationTypesService _admissionApplicationTypesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionApplicationTypeController class.
        /// </summary>
        /// <param name="admissionApplicationTypesService">Service of type <see cref="IAdmissionApplicationTypesService">IAdmissionApplicationTypesService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AdmissionApplicationTypesController(IAdmissionApplicationTypesService admissionApplicationTypesService, ILogger logger)
        {
            _admissionApplicationTypesService = admissionApplicationTypesService;
            _logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Admission Application Types
        /// </summary>
        /// <returns>All <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes.</see></returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationTypes>> GetAdmissionApplicationTypesAsync()
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
                return await _admissionApplicationTypesService.GetAdmissionApplicationTypesAsync(bypassCache);
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

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an Admission Application Type by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes.</see></returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationTypes> GetAdmissionApplicationTypeByIdAsync(string id)
        {
            try
            {
                return await _admissionApplicationTypesService.GetAdmissionApplicationTypesByGuidAsync(id);
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
        /// Updates an AdmissionApplicationTypes.
        /// </summary>
        /// <param name="admissionApplicationTypes"><see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see></returns>
        [HttpPut]
        public Dtos.AdmissionApplicationTypes PutAdmissionApplicationType([FromBody] Dtos.AdmissionApplicationTypes admissionApplicationTypes)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a AdmissionApplicationTypes.
        /// </summary>
        /// <param name="admissionApplicationTypes"><see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AdmissionApplicationTypes">AdmissionApplicationTypes</see></returns>
        [HttpPost]
        public Dtos.AdmissionApplicationTypes PostAdmissionApplicationType([FromBody] Dtos.AdmissionApplicationTypes admissionApplicationTypes)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AdmissionApplicationTypes
        /// </summary>
        /// <param name="id">Id of the AdmissionApplicationTypes to delete</param>
        [HttpDelete]
        public Dtos.AdmissionApplicationTypes DeleteAdmissionApplicationType(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
