/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Expose Human Resources Institution Positions data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class InstitutionPositionsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IInstitutionPositionService _institutionPositionService;

        /// <summary>
        /// InstitutionPositionsController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="institutionPositionService"></param>
        public InstitutionPositionsController(ILogger logger, IInstitutionPositionService institutionPositionService)
        {
            this._logger = logger;
            this._institutionPositionService = institutionPositionService;
        }


        /// <summary>
        /// Retrieves an Institution Positions by ID.
        /// </summary>
        /// <returns>An <see cref="Dtos.InstitutionPosition">InstitutionPosition</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<InstitutionPosition> GetInstitutionPositionsByGuidAsync(string guid)
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
                //AddDataPrivacyContextProperty((await _institutionPositionService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                 await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));
                return await _institutionPositionService.GetInstitutionPositionByGuidAsync(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Institution Positions by ID. (v11)
        /// </summary>
        /// <returns>An <see cref="Dtos.InstitutionPosition">InstitutionPosition</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<InstitutionPosition> GetInstitutionPositionsByGuid2Async(string guid)
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
                //AddDataPrivacyContextProperty((await _institutionPositionService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _institutionPositionService.GetInstitutionPositionByGuid2Async(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Retrieves an Institution Positions by ID. (v12)
        /// </summary>
        /// <returns>An <see cref="Dtos.InstitutionPosition">InstitutionPosition</see>object.</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<InstitutionPosition2> GetInstitutionPositionsByGuid3Async(string guid)
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
                //AddDataPrivacyContextProperty((await _institutionPositionService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _institutionPositionService.GetInstitutionPositionByGuid3Async(guid, bypassCache);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
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
        /// Return a list of InstitutionPosition objects based on selection criteria.
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="campus">The physical location of the institution position</param>
        /// <param name="status">The status of the position (e.g. active, frozen, cancelled, inactive)</param>
        /// <param name="bargainingUnit">The group or union associated with the position</param>
        /// <param name="reportsToPosition">The position to which this position reports</param>
        /// <param name="exemptionType">An indicator if the position is exempt or non-exempt</param>
        /// <param name="compensationType">The type of compensation awarded (e.g. salary, wages, etc.)</param>
        /// <param name="startOn">The date when the position is first available</param>
        /// <param name="endOn">The date when the position is last available</param>
        /// <returns>List of InstitutionPositions <see cref="Dtos.InstitutionPosition"/> objects representing matching Institution Positions</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        [ValidateQueryStringFilter(new string[] { "campus", "status", "bargainingUnit", "reportsToPosition", "exemptionType", 
           "compensationType","startOn", "endOn" }, false, true)]
        [EedmResponseFilter]
        public async Task<IHttpActionResult> GetInstitutionPositionsAsync(Paging page, [FromUri] string campus = "", [FromUri] string status = "", [FromUri] string bargainingUnit = "",
            [FromUri] string reportsToPosition = "", [FromUri] string exemptionType = "", [FromUri] string compensationType = "", [FromUri] string startOn = "", [FromUri] string endOn = "") 
        {

            string criteria = string.Concat(campus, status, bargainingUnit, reportsToPosition, exemptionType, compensationType, startOn, endOn);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(new List<Dtos.InstitutionPosition>(), page, 0, this.Request);
            }

            if (campus == null || status == null || bargainingUnit == null || reportsToPosition == null || exemptionType == null ||
                compensationType == null || startOn == null || endOn ==null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(new List<Dtos.InstitutionPosition>(), page, 0, this.Request);
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
                if (!string.IsNullOrEmpty(status))
                {
                    if (!status.Equals("active") && !status.Equals("frozen") && !status.Equals("cancelled") && !status.Equals("inactive"))
                    {
                        throw new Exception(
                            string.Format("{0} is an invalid enumeration value", status));
                    }

                    if (status.Equals("frozen"))
                    {
                        throw new Exception(string.Concat("The filter status frozen is not supported in Colleague"));
                    }
                }

                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _institutionPositionService.GetInstitutionPositionsAsync(page.Offset, page.Limit, campus, status,
                            bargainingUnit, reportsToPosition, exemptionType, compensationType, startOn, endOn, bypassCache);

                AddEthosContextProperties(
                 await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));
               
                return new PagedHttpActionResult<IEnumerable<InstitutionPosition>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Return a list of InstitutionPositions objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - InstitutionPosition page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>List of InstitutionPosition <see cref="Dtos.InstitutionPosition"/> objects representing matching institution positions</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionPosition))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetInstitutionPositions2Async(Paging page, QueryStringFilter criteria)
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

                string campus = string.Empty, startOn = string.Empty, endOn = string.Empty, bargainingUnit = string.Empty,
                    exemptionType = string.Empty, compensationType = string.Empty, status = string.Empty, keyword = string.Empty;
                List<string> reportsToPositions = null;

                var criteriaObj = GetFilterObject<Dtos.InstitutionPosition>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(new List<Dtos.InstitutionPosition>(), page, 0, this.Request);

                if (criteriaObj != null)
                {
                    campus = ((criteriaObj.Campus != null)
                       && (!string.IsNullOrEmpty(criteriaObj.Campus.Id)))
                        ? criteriaObj.Campus.Id : string.Empty;

                    status = ((criteriaObj.Status != null) && (criteriaObj.Status != Dtos.EnumProperties.PositionStatus.NotSet))
                       ? criteriaObj.Status.ToString().ToLower() : string.Empty;
                    if (status == "frozen")
                        return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(new List<Dtos.InstitutionPosition>(), page, 0, this.Request);

                    bargainingUnit = ((criteriaObj.BargainingUnit != null)
                        && (!string.IsNullOrEmpty(criteriaObj.BargainingUnit.Id)))
                        ? criteriaObj.BargainingUnit.Id : string.Empty;

                    if ((criteriaObj.ReportsTo != null) && (criteriaObj.ReportsTo.Any()))
                    {
                        var reportsTo = new List<string>();
                        foreach (var report in criteriaObj.ReportsTo)
                        {
                            if ((report != null) && (report.Postition != null))
                            {
                                reportsTo.Add(report.Postition.Id);
                            }
                        }
                        reportsToPositions = reportsTo;
                    }
                    exemptionType = ((criteriaObj.ExemptionType != null) && (criteriaObj.ExemptionType != Dtos.EnumProperties.ExemptionType.NotSet))
                        ? criteriaObj.ExemptionType.ToString().ToLower() : string.Empty;
                    compensationType = ((criteriaObj.Compensation != null) &&
                        (criteriaObj.Compensation.Type != Dtos.EnumProperties.CompensationType.NotSet))
                        ? criteriaObj.Compensation.Type.ToString().ToLower() : string.Empty;
                    startOn = criteriaObj.StartOn != null && criteriaObj.StartOn != default(DateTime)
                        ? criteriaObj.StartOn.ToShortDateString() : string.Empty;
                    endOn = criteriaObj.EndOn != null && criteriaObj.EndOn != default(DateTime)
                        ? Convert.ToDateTime(criteriaObj.EndOn).ToShortDateString() : string.Empty;
                }
                var pageOfItems = await _institutionPositionService.GetInstitutionPositions2Async(page.Offset, page.Limit, campus, status,
                            bargainingUnit, reportsToPositions, exemptionType, compensationType, startOn, endOn, bypassCache);

                AddEthosContextProperties(
                await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (JsonReaderException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (JsonSerializationException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Return a list of InstitutionPositions objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - InstitutionPosition page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <returns>List of InstitutionPosition <see cref="Dtos.InstitutionPosition"/> objects representing matching institution positions</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.InstitutionPosition2))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetInstitutionPositions3Async(Paging page, QueryStringFilter criteria)
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
               
                string campus = string.Empty, startOn = string.Empty, endOn = string.Empty, bargainingUnit = string.Empty,
                    exemptionType = string.Empty, compensationType = string.Empty, status = string.Empty, keyword = string.Empty;
                List<string> reportsToPositions = null;

                var criteriaObj = GetFilterObject<Dtos.InstitutionPosition2>(_logger, "criteria");
                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition2>>(new List<Dtos.InstitutionPosition2>(), page, 0, this.Request);

                if (criteriaObj != null)
                {
                    campus = ((criteriaObj.Campus != null)
                       && (!string.IsNullOrEmpty(criteriaObj.Campus.Id)))
                        ? criteriaObj.Campus.Id : string.Empty;

                    status = ((criteriaObj.Status != null) && (criteriaObj.Status != Dtos.EnumProperties.PositionStatus.NotSet))
                       ? criteriaObj.Status.ToString().ToLower() : string.Empty;
                    if (status == "frozen")
                        return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition>>(new List<Dtos.InstitutionPosition>(), page, 0, this.Request);

                    bargainingUnit = ((criteriaObj.BargainingUnit != null)
                        && (!string.IsNullOrEmpty(criteriaObj.BargainingUnit.Id)))
                        ? criteriaObj.BargainingUnit.Id : string.Empty;

                    if ((criteriaObj.ReportsTo != null) && (criteriaObj.ReportsTo.Any()))
                    {
                        var reportsTo = new List<string>();
                        foreach (var report in criteriaObj.ReportsTo)
                        {
                            if ((report != null) && (report.Postition != null))
                            {
                                reportsTo.Add(report.Postition.Id);
                            }
                        }
                        reportsToPositions = reportsTo;
                    }
                    exemptionType = ((criteriaObj.ExemptionType != null) && (criteriaObj.ExemptionType != Dtos.EnumProperties.ExemptionType.NotSet))
                        ? criteriaObj.ExemptionType.ToString().ToLower() : string.Empty;
                    compensationType = ((criteriaObj.Compensation != null) &&
                        (criteriaObj.Compensation.Type != Dtos.EnumProperties.CompensationType.NotSet))
                        ? criteriaObj.Compensation.Type.ToString().ToLower() : string.Empty;
                    startOn = criteriaObj.StartOn != null && criteriaObj.StartOn != default(DateTime)
                        ? criteriaObj.StartOn.ToShortDateString() : string.Empty;
                    endOn = criteriaObj.EndOn != null && criteriaObj.EndOn != default(DateTime)
                        ? Convert.ToDateTime(criteriaObj.EndOn).ToShortDateString() : string.Empty;
                }
             
                var pageOfItems = await _institutionPositionService.GetInstitutionPositions3Async(page.Offset, page.Limit, campus, status,
                            bargainingUnit, reportsToPositions, exemptionType, compensationType, startOn, endOn, bypassCache);

                AddEthosContextProperties(
                await _institutionPositionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _institutionPositionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.InstitutionPosition2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (JsonReaderException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (JsonSerializationException e)
            {
                _logger.Error(e.ToString());

                throw CreateHttpResponseException(new IntegrationApiException("Deserialization Error",
                    IntegrationApiUtility.GetDefaultApiError("Error parsing JSON section search request.")));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
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
        /// Creates a Institution Position.
        /// </summary>
        /// <param name="institutionPosition"><see cref="Dtos.InstitutionPosition">InstitutionPosition</see> to create</param>
        /// <returns>Newly created <see cref="Dtos.InstitutionPosition">InstitutionPosition</see></returns>
        [HttpPost]
        public async Task<Dtos.InstitutionPosition> CreateInstitutionPositionsAsync([FromBody] Dtos.InstitutionPosition institutionPosition)
        {
            //Create is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Updates an Institution Position.
        /// </summary>
        /// <param name="guid">Guid of the Institution Position to update</param>
        /// <param name="institutionPosition"><see cref="Dtos.InstitutionPosition">InstitutionPosition</see> to create</param>
        /// <returns>Updated <see cref="Dtos.InstitutionPosition">InstitutionPosition</see></returns>
        [HttpPut]
        public async Task<Dtos.InstitutionPosition> UpdateInstitutionPositionsAsync([FromUri] string guid, [FromBody] Dtos.InstitutionPosition institutionPosition)
        {

            //Update is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Deletes an Institution Positions.
        /// </summary>
        /// <param name="guid">Guid of the Institution Position to be deleted</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DefaultDeleteInstitutionPositions(string guid)
        {
            //Delete is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }   
    }
}