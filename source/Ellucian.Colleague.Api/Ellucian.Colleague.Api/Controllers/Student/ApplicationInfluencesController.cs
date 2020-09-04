// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to application influence data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class ApplicationInfluencesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAdmissionApplicationInfluencesService _admissionApplicationInfluencesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the ApplicationInfluencesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="admissionApplicationInfluencesService">Service of type <see cref="IAdmissionApplicationInfluencesService">IAdmissionApplicationInfluencesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public ApplicationInfluencesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository,
            IAdmissionApplicationInfluencesService admissionApplicationInfluencesService, ILogger logger)
        {
            _admissionApplicationInfluencesService = admissionApplicationInfluencesService;
            _logger = logger;

            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
        }


        /// <summary>
        /// Return all admissionApplicationInfluences
        /// </summary>
        /// <returns>List of AdmissionApplicationInfluences <see cref="Dtos.AdmissionApplicationInfluences"/> objects representing matching admissionApplicationInfluences</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationInfluences>> GetAdmissionApplicationInfluencesAsync()
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
                var admissionApplicationInfluences = await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesAsync(bypassCache);

                if (admissionApplicationInfluences != null && admissionApplicationInfluences.Any())
                {
                    AddEthosContextProperties(await _admissionApplicationInfluencesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                              await _admissionApplicationInfluencesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              admissionApplicationInfluences.Select(a => a.Id).ToList()));
                }
                return admissionApplicationInfluences;
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Read (GET) a admissionApplicationInfluences using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationInfluences</param>
        /// <returns>A admissionApplicationInfluences object <see cref="Dtos.AdmissionApplicationInfluences"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AdmissionApplicationInfluences> GetAdmissionApplicationInfluencesByGuidAsync(string guid)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                AddEthosContextProperties(
                   await _admissionApplicationInfluencesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _admissionApplicationInfluencesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _admissionApplicationInfluencesService.GetAdmissionApplicationInfluencesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new admissionApplicationInfluences
        /// </summary>
        /// <param name="admissionApplicationInfluences">DTO of the new admissionApplicationInfluences</param>
        /// <returns>A admissionApplicationInfluences object <see cref="Dtos.AdmissionApplicationInfluences"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AdmissionApplicationInfluences> PostAdmissionApplicationInfluencesAsync([FromBody] Dtos.AdmissionApplicationInfluences admissionApplicationInfluences)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing admissionApplicationInfluences
        /// </summary>
        /// <param name="guid">GUID of the admissionApplicationInfluences to update</param>
        /// <param name="admissionApplicationInfluences">DTO of the updated admissionApplicationInfluences</param>
        /// <returns>A admissionApplicationInfluences object <see cref="Dtos.AdmissionApplicationInfluences"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AdmissionApplicationInfluences> PutAdmissionApplicationInfluencesAsync([FromUri] string guid, [FromBody] Dtos.AdmissionApplicationInfluences admissionApplicationInfluences)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a admissionApplicationInfluences
        /// </summary>
        /// <param name="guid">GUID to desired admissionApplicationInfluences</param>
        [HttpDelete]
        public async Task DeleteAdmissionApplicationInfluencesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Retrieves all Application Influences.
        /// </summary>
        /// <returns>All <see cref="ApplicationInfluence">Application Influence</see> codes and descriptions.</returns>
        public async Task<IEnumerable<ApplicationInfluence>> GetAsync()
        {
            var applicationInfluenceCollection = await _referenceDataRepository.GetApplicationInfluencesAsync(false);

            // Get the right adapter for the type mapping
            var applicationInfluenceDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ApplicationInfluence, ApplicationInfluence>();

            // Map the ApplicationInfluence entity to the program DTO
            var applicationInfluenceDtoCollection = new List<ApplicationInfluence>();
            foreach (var applicationInfluence in applicationInfluenceCollection)
            {
                applicationInfluenceDtoCollection.Add(applicationInfluenceDtoAdapter.MapToType(applicationInfluence));
            }

            return applicationInfluenceDtoCollection;
        }
    }
}