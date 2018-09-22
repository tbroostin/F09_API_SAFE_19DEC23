// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Controller for Admission Populations
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AdmissionPopulationsController : BaseCompressedApiController
    {
        private readonly IAdmissionPopulationsService _admissionPopulationsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AdmissionPopulationController class.
        /// </summary>
        /// <param name="admissionPopulationsService">Service of type <see cref="IAdmissionPopulationsService">IAdmissionPopulationsService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public AdmissionPopulationsController(IAdmissionPopulationsService admissionPopulationsService, ILogger logger)
        {
            _admissionPopulationsService = admissionPopulationsService;
            _logger = logger;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Admission Populations
        /// </summary>
        /// <returns>All <see cref="Dtos.AdmissionPopulations">AdmissionPopulations.</see></returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionPopulations>> GetAdmissionPopulationsAsync()
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
                var pageOfItems = await _admissionPopulationsService.GetAdmissionPopulationsAsync(bypassCache);

                AddEthosContextProperties(
                  await _admissionPopulationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionPopulationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Select(i => i.Id).Distinct().ToList()));

                return pageOfItems;
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
        /// Retrieves an Admission Population by ID.
        /// </summary>
        /// <returns>A <see cref="Dtos.AdmissionPopulations">AdmissionPopulations.</see></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Ellucian.Colleague.Dtos.AdmissionPopulations> GetAdmissionPopulationByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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
                  await _admissionPopulationsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _admissionPopulationsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      new List<string>() { id }));

                return await _admissionPopulationsService.GetAdmissionPopulationsByGuidAsync(id);
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
        /// Updates an AdmissionPopulations.
        /// </summary>
        /// <param name="admissionPopulations"><see cref="Dtos.AdmissionPopulations">AdmissionPopulations</see> to update</param>
        /// <returns>Newly updated <see cref="Dtos.AdmissionPopulations">AdmissionPopulations</see></returns>
        [HttpPut]
        public Dtos.AdmissionPopulations PutAdmissionPopulation([FromBody] Dtos.AdmissionPopulations admissionPopulations)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Creates a AdmissionPopulations.
        /// </summary>
        /// <param name="admissionPopulations"><see cref="Dtos.AdmissionPopulations">AdmissionPopulations</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AdmissionPopulations">AdmissionPopulations</see></returns>
        [HttpPost]
        public Dtos.AdmissionPopulations PostAdmissionPopulation([FromBody] Dtos.AdmissionPopulations admissionPopulations)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AdmissionPopulations
        /// </summary>
        /// <param name="id">Id of the AdmissionPopulations to delete</param>
        [HttpDelete]
        public Dtos.AdmissionPopulations DeleteAdmissionPopulation(string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
