//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
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
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to HousingAssignments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class HousingAssignmentController : BaseCompressedApiController
    {
        private readonly IHousingAssignmentService _housingAssignmentService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the HousingAssignmentsController class.
        /// </summary>
        /// <param name="housingAssignmentService">Service of type <see cref="IHousingAssignmentService">IHousingAssignmentsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public HousingAssignmentController(IHousingAssignmentService housingAssignmentService, ILogger logger)
        {
            _housingAssignmentService = housingAssignmentService;
            _logger = logger;
        }


        #region 16.0.0

        /// <summary>
        /// Return all housingAssignments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of HousingAssignments <see cref="Dtos.HousingAssignment2"/> objects representing matching housingAssignments</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.HousingAssignment2))]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetHousingAssignments2Async(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.HousingAssignment2>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.HousingAssignment2>>(new List<Dtos.HousingAssignment2>(), page, 0, this.Request);
            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _housingAssignmentService.GetHousingAssignments2Async(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.HousingAssignment2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a housingAssignment using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignment</param>
        /// <returns>A housingAssignment object <see cref="Dtos.HousingAssignment2"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        public async Task<Dtos.HousingAssignment2> GetHousingAssignmentByGuid2Async(string guid)
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
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                var housingAssignment = await _housingAssignmentService.GetHousingAssignmentByGuid2Async(guid);

                if (housingAssignment != null)
                {

                    AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { housingAssignment.Id }));
                }

                return housingAssignment;

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
        /// Update (PUT) an existing housingAssignment
        /// </summary>
        /// <param name="guid">GUID of the housingAssignments to update</param>
        /// <param name="housingAssignment">DTO of the updated housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        public async Task<Dtos.HousingAssignment2> PutHousingAssignment2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment2 housingAssignment)
        {
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            if (housingAssignment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the id on the url is not a nil one
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid id value",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation.")));
            }

            //make sure the id in the body and on the url match
            if (!string.Equals(guid, housingAssignment.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }

            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                housingAssignment.Id = guid.ToLowerInvariant();
            }

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var housingAssignmentReturn = await _housingAssignmentService.UpdateHousingAssignment2Async(guid,
                    await PerformPartialPayloadMerge(housingAssignment, async () => await _housingAssignmentService.GetHousingAssignmentByGuid2Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return housingAssignmentReturn;

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Create (POST) a new housingAssignment
        /// </summary>
        /// <param name="housingAssignment">DTO of the new housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        public async Task<Dtos.HousingAssignment2> PostHousingAssignment2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment2 housingAssignment)
        {
            if (housingAssignment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the housingRequest object has an Id as it is required
            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }

            if (housingAssignment.Id != Guid.Empty.ToString())
            {
                throw new InvalidOperationException("On a post you can not define a GUID.");
            }

            ValidateHousingAssignment2(housingAssignment);

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the housing assignment
                var housingAssignmentReturn = await _housingAssignmentService.CreateHousingAssignment2Async(housingAssignment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { housingAssignmentReturn.Id }));

                return housingAssignmentReturn;

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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

        private void ValidateHousingAssignment2(Dtos.HousingAssignment2 housingAssignment)
        {
            if (housingAssignment.Person == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person property is a required property.")));
            }

            if (housingAssignment.Person != null && string.IsNullOrEmpty(housingAssignment.Person.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person id property is a required property.")));
            }

            if (housingAssignment.Room == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null room property",
                    IntegrationApiUtility.GetDefaultApiError("The room property is required.")));
            }

            if (housingAssignment.Room != null && string.IsNullOrEmpty(housingAssignment.Room.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null room id property",
                    IntegrationApiUtility.GetDefaultApiError("The room id property is required.")));
            }

            if (!housingAssignment.StartOn.HasValue)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null startOn property",
                        IntegrationApiUtility.GetDefaultApiError("The startOn property is required.")));
            }

            if (!housingAssignment.EndOn.HasValue)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null endOn property",
                        IntegrationApiUtility.GetDefaultApiError("The endOn property is required.")));
            }

            if (housingAssignment.StartOn.HasValue && housingAssignment.EndOn.HasValue && housingAssignment.StartOn.Value > housingAssignment.EndOn.Value)
            {
                throw CreateHttpResponseException(new IntegrationApiException("StartOn property",
                        IntegrationApiUtility.GetDefaultApiError("The end date cannot be earlier start date.")));
            }

            if (housingAssignment.Status == Dtos.EnumProperties.HousingAssignmentsStatus.NotSet)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null status property",
                            IntegrationApiUtility.GetDefaultApiError("The status property is required.")));
            }

            if (!housingAssignment.StatusDate.HasValue)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null statusDate property",
                            IntegrationApiUtility.GetDefaultApiError("The statusDate property is required.")));
            }
        }
        #endregion

        /// <summary>
        /// Return all housingAssignments
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">mealplan  search criteria in JSON format</param>
        /// <returns>List of HousingAssignments <see cref="Dtos.HousingAssignment"/> objects representing matching housingAssignments</returns>
        [HttpGet, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })] 
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.HousingAssignment))]
        [FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetHousingAssignmentsAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            var criteriaFilter = GetFilterObject<Dtos.HousingAssignment>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.HousingAssignment>>(new List<Dtos.HousingAssignment>(), page, 0, this.Request);
            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                if (page == null)
                {
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _housingAssignmentService.GetHousingAssignmentsAsync(page.Offset, page.Limit, criteriaFilter, bypassCache);

                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.HousingAssignment>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a housingAssignment using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignment</param>
        /// <returns>A housingAssignment object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter, PermissionsFilter(new string[] { StudentPermissionCodes.ViewHousingAssignment, StudentPermissionCodes.CreateUpdateHousingAssignment })]
        public async Task<Dtos.HousingAssignment> GetHousingAssignmentByGuidAsync(string guid)
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
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                var housingAssignment = await _housingAssignmentService.GetHousingAssignmentByGuidAsync(guid);

                if (housingAssignment != null)
                {

                    AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { housingAssignment.Id }));
                }


                return housingAssignment;

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
        /// Create (POST) a new housingAssignment
        /// </summary>
        /// <param name="housingAssignment">DTO of the new housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        public async Task<Dtos.HousingAssignment> PostHousingAssignmentAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment housingAssignment)
        {
            if (housingAssignment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the housingRequest object has an Id as it is required
            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }

            if (housingAssignment.Id != Guid.Empty.ToString())
            {
                throw new ArgumentNullException("housingAssignmentsDto", "On a post you can not define a GUID.");
            }

            ValidateHousingAssignment(housingAssignment);

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //call import extend method that needs the extracted extension data and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the housing assignment
                var housingAssignmentReturn = await _housingAssignmentService.CreateHousingAssignmentAsync(housingAssignment);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { housingAssignmentReturn.Id }));

                return housingAssignmentReturn;

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing housingAssignment
        /// </summary>
        /// <param name="guid">GUID of the housingAssignments to update</param>
        /// <param name="housingAssignment">DTO of the updated housingAssignments</param>
        /// <returns>A housingAssignments object <see cref="Dtos.HousingAssignment"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter, PermissionsFilter(StudentPermissionCodes.CreateUpdateHousingAssignment)]
        public async Task<Dtos.HousingAssignment> PutHousingAssignmentAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.HousingAssignment housingAssignment)
        {
            //make sure id was specified on the URL
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request URL.")));
            }

            if (housingAssignment == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null housingRequest argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            //make sure the id on the url is not a nil one
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Invalid id value",
                    IntegrationApiUtility.GetDefaultApiError("Nil GUID cannot be used in PUT operation.")));
            }

            //make sure the id in the body and on the url match
            if (!string.Equals(guid, housingAssignment.Id, StringComparison.InvariantCultureIgnoreCase))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Id not the same as in request body.")));
            }

            if (string.IsNullOrEmpty(housingAssignment.Id))
            {
                housingAssignment.Id = guid.ToLowerInvariant();
            }

            try
            {
                _housingAssignmentService.ValidatePermissions(GetPermissionsMetaData());
                //get Data Privacy List
                var dpList = await _housingAssignmentService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _housingAssignmentService.ImportExtendedEthosData(await ExtractExtendedData(await _housingAssignmentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var housingAssignmentReturn = await _housingAssignmentService.UpdateHousingAssignmentAsync(guid,
                    await PerformPartialPayloadMerge(housingAssignment, async () => await _housingAssignmentService.GetHousingAssignmentByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _housingAssignmentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return housingAssignmentReturn;

            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        private void ValidateHousingAssignment(Dtos.HousingAssignment housingAssignment)
        {
            if (housingAssignment.Person == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null Person property", 
                    IntegrationApiUtility.GetDefaultApiError("The person property is a required property.")));
            }

            if (housingAssignment.Person != null && string.IsNullOrEmpty(housingAssignment.Person.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null Person property",
                    IntegrationApiUtility.GetDefaultApiError("The person id property is a required property.")));
            }

            if (housingAssignment.Room == null) 
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null room property",
                    IntegrationApiUtility.GetDefaultApiError("The room property is required.")));
            }

            if (housingAssignment.Room != null && string.IsNullOrEmpty(housingAssignment.Room.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null room id property",
                    IntegrationApiUtility.GetDefaultApiError("The room id property is required.")));
            }

            if (!housingAssignment.StartOn.HasValue) 
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null startOn property",
                        IntegrationApiUtility.GetDefaultApiError("The startOn property is required.")));
            }

            if (!housingAssignment.EndOn.HasValue)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null endOn property",
                        IntegrationApiUtility.GetDefaultApiError("The endOn property is required.")));
            }

            if (housingAssignment.StartOn.HasValue && housingAssignment.EndOn.HasValue && housingAssignment.StartOn.Value > housingAssignment.EndOn.Value)
            {
                throw CreateHttpResponseException(new IntegrationApiException("StartOn property",
                        IntegrationApiUtility.GetDefaultApiError("The end date cannot be earlier start date.")));
            }

            if (housingAssignment.Status == Dtos.EnumProperties.HousingAssignmentsStatus.NotSet) 
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null status property",
                            IntegrationApiUtility.GetDefaultApiError("The status property is required.")));
            }

            if (!housingAssignment.StatusDate.HasValue )
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null statusDate property",
                            IntegrationApiUtility.GetDefaultApiError("The statusDate property is required.")));
            }
        }

        /// <summary>
        /// Delete (DELETE) a housingAssignment
        /// </summary>
        /// <param name="guid">GUID to desired housingAssignments</param>
        [HttpDelete]
        public async Task DeleteHousingAssignmentAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}