// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Filters;
using System.Linq;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicCatalog data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class AcademicCatalogController : BaseCompressedApiController
    {
         private readonly IAcademicCatalogService _academicCatalogService; 
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicCatalogController class.
        /// </summary>
         /// <param name="academicCatalogService">Service of type <see cref="IAcademicCatalogService">IAcademicCatalogService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AcademicCatalogController(IAcademicCatalogService academicCatalogService, ILogger logger)
        {

            _academicCatalogService = academicCatalogService;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves all Academic Catalogs.
        /// </summary>
        /// <returns>All <see cref="Dtos.AcademicCatalog">AcademicCatalog</see>objects.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true), EedmResponseFilter]
        public async Task<IEnumerable<Dtos.AcademicCatalog2>> GetAcademicCatalogs2Async()
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
                var items = await _academicCatalogService.GetAcademicCatalogs2Async(bypassCache);

                AddEthosContextProperties(await _academicCatalogService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _academicCatalogService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              items.Select(a => a.Id).ToList()));

                return items;
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(ex));
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Retrieves an academic catalog by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.AcademicCatalog">AcademicCatalog</see>object.</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [EedmResponseFilter]
        public async Task<Dtos.AcademicCatalog2> GetAcademicCatalogById2Async(string id)
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
                var item = await _academicCatalogService.GetAcademicCatalogByGuid2Async(id);

                if(item != null)
                {
                    AddEthosContextProperties(await _academicCatalogService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _academicCatalogService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { item.Id }));
                }

                return item;
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN SS</remarks>
        /// <summary>
        /// Retrieves all Academic Catalogs.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>All <see cref="Catalog">Catalog</see>objects.</returns>
        public async Task<IEnumerable<Catalog>> GetAllAcademicCatalogsAsync()
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
                IEnumerable<Catalog> cats = await _academicCatalogService.GetAllAcademicCatalogsAsync(bypassCache);
                return cats;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                var message = "Unable to retrieve academic catalog data.  See Logging for more details.  Exception thrown: " + ex.Message;
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>        
        /// Creates a AcademicCatalog.
        /// </summary>
        /// <param name="academicCatalog"><see cref="Dtos.AcademicCatalog">AcademicCatalog</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.AcademicCatalog">AcademicCatalog</see></returns>
        [HttpPost]
        public Dtos.AcademicCatalog PostAcademicCatalogs([FromBody] Dtos.AcademicCatalog academicCatalog)
        {
            //Create is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Updates a AcademicCatalog.
        /// </summary>
        /// <param name="id">Id of the AcademicCatalog to update</param>
        /// <param name="academicCatalog"><see cref="Dtos.AcademicCatalog">AcademicCatalog</see> to create</param>
        /// <returns>Updated <see cref="Dtos.AcademicCatalog">AcademicCatalog</see></returns>
        [HttpPut]
        public Dtos.AcademicCatalog PutAcademicCatalogs([FromUri] string id, [FromBody] Dtos.AcademicCatalog academicCatalog)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Delete (DELETE) an existing AcademicCatalog
        /// </summary>
        /// <param name="id">Id of the AcademicCatalog to delete</param>
        [HttpDelete]
        public void DeleteAcademicCatalogs([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}
