/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
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
    /// Exposes FinancialAidOffice and FinancialAidConfiguration Data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidOfficesController : BaseCompressedApiController
    {
        private readonly IStudentFinancialAidOfficeService financialAidOfficeService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidOfficesController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidOfficeService">FinancialAidOfficeService</param>
        /// <param name="logger">Logger</param>
        public StudentFinancialAidOfficesController(IAdapterRegistry adapterRegistry, IStudentFinancialAidOfficeService financialAidOfficeService, ILogger logger)
        {
            this.financialAidOfficeService = financialAidOfficeService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }        

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all financial aid offices.
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <returns>All FinancialAidOffice objects.</returns>
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter, HttpGet]
        public async Task<IEnumerable<Dtos.FinancialAidOffice>> GetEedmFinancialAidOfficesAsync()
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
                var items = await financialAidOfficeService.GetFinancialAidOfficesAsync(bypassCache);

                AddEthosContextProperties(await financialAidOfficeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await financialAidOfficeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return items;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves an Financial Aid Offices by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see>object.</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.FinancialAidOffice> GetFinancialAidOfficeByGuidAsync(string guid)
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
                var faOffice = await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync(guid, bypassCache);

                if (faOffice != null)
                {

                    AddEthosContextProperties(await financialAidOfficeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await financialAidOfficeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { faOffice.Id }));
                }

                return faOffice;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Creates a Financial Aid Office.
        /// </summary>
        /// <param name="financialAidOffice"><see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see></returns>
        [HttpPost]
        public async Task<Dtos.FinancialAidOffice> PostFinancialAidOfficeAsync([FromBody] Dtos.FinancialAidOffice financialAidOffice)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates a Financial Aid Office.
        /// </summary>
        /// <param name="guid">Id of the Financial Aid Office to update</param>
        /// <param name="financialAidOffice"><see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see> to create</param>
        /// <returns>Updated <see cref="Dtos.FinancialAidOffice">FinancialAidOffice</see></returns>
        [HttpPut]
        public async Task<Dtos.FinancialAidOffice> PutFinancialAidOfficeAsync([FromUri] string guid, [FromBody] Dtos.FinancialAidOffice financialAidOffice)
        {

            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes a Financial Aid Office.
        /// </summary>
        /// <param name="guid">ID of the Financial Aid Office to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteFinancialAidOfficeAsync(string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}