// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.Linq;
using System.Net.Http;
using System.Web.Http.ModelBinding;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for requisitions
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class RequisitionsController : BaseCompressedApiController
    {
        private readonly IRequisitionService requisitionService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the RequisitionsController object
        /// </summary>
        /// <param name="requisitionService">Requisition service object</param>
        /// <param name="logger">Logger object</param>
        public RequisitionsController(IRequisitionService requisitionService, ILogger logger)
        {
            this.requisitionService = requisitionService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified requisition
        /// </summary>
        /// <param name="requisitionId">ID of the requested requisition</param>
        /// <returns>Requisition DTO</returns>
        /// <accessComments>
        /// Requires permission VIEW.REQUISITION, and requires access to at least one of the
        /// general ledger numbers on the requisition line items.
        /// </accessComments>
        public async Task<Requisition> GetRequisitionAsync(string requisitionId)
        {
            if (string.IsNullOrEmpty(requisitionId))
            {
                string message = "A Requisition ID must be specified.";
                logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }

            try
            {
                var requisition = await requisitionService.GetRequisitionAsync(requisitionId);
                return requisition;
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to get the requisition.", HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfex)
            {
                logger.Error(knfex, knfex.Message);
                throw CreateHttpResponseException("Record not found.", HttpStatusCode.NotFound);
            }
            catch (ApplicationException aex)
            {
                logger.Error(aex, aex.Message);
                throw CreateHttpResponseException("Invalid data in record.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException("Unable to get the requisition.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return all requisitions
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <returns>List of Requisitions <see cref="Dtos.Requisitions"/> objects representing matching requisitions</returns>
        [HttpGet, EedmResponseFilter]            
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetRequisitionsAsync(Paging page)
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
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await requisitionService.GetRequisitionsAsync(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                    await requisitionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Requisitions>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
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
        /// Read (GET) a requisitions using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired requisitions</param>
        /// <returns>A requisitions object <see cref="Dtos.Requisitions"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Requisitions> GetRequisitionsByGuidAsync(string guid)
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
                    await requisitionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await requisitionService.GetRequisitionsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
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
        /// Update (PUT) an existing Requisitions
        /// </summary>
        /// <param name="guid">GUID of the requisitions to update</param>
        /// <param name="requisitions">DTO of the updated requisitions</param>
        /// <returns>A Requisitions object <see cref="Dtos.Requisitions"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Requisitions> PutRequisitionsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Requisitions requisitions)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null guid argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (requisitions == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null requisitions argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(requisitions.Id))
            {
                requisitions.Id = guid.ToLowerInvariant();
            }
            else if (!string.Equals(guid, requisitions.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await requisitionService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await requisitionService.ImportExtendedEthosData(await ExtractExtendedData(await requisitionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var requisitionReturn = await requisitionService.UpdateRequisitionsAsync(
                    await PerformPartialPayloadMerge(requisitions, async () => await requisitionService.GetRequisitionsByGuidAsync(guid, true),
                    dpList, logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return requisitionReturn;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
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
            catch (ConfigurationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new requisitions
        /// </summary>
        /// <param name="requisitions">DTO of the new requisitions</param>
        /// <returns>A requisitions object <see cref="Dtos.Requisitions"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Requisitions> PostRequisitionsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.Requisitions requisitions)
        {
            if (requisitions == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid requisitions.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(requisitions.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null requisitions id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await requisitionService.ImportExtendedEthosData(await ExtractExtendedData(await requisitionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var requisitionReturn = await requisitionService.CreateRequisitionsAsync(requisitions);

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await requisitionService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await requisitionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { requisitionReturn.Id }));

                return requisitionReturn;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a requisitions
        /// </summary>
        /// <param name="guid">GUID to desired requisitions</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteRequisitionsAsync([FromUri] string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("id", "guid is a required for delete");
                }
                await requisitionService.DeleteRequisitionAsync(guid);
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
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}