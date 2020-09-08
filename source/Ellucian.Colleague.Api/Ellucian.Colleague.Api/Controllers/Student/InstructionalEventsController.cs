﻿// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class InstructionalEventsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly ISectionCoordinationService _sectionCoordinationService;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="sectionCoordinationService">Coordination service interface for sections</param>
        /// <param name="logger"> <see cref="ILogger">ILogger</see></param>
        public InstructionalEventsController(IAdapterRegistry adapterRegistry, ISectionCoordinationService sectionCoordinationService, ILogger logger)
        {
            _sectionCoordinationService = sectionCoordinationService;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        #region HeDM version 6 methods
        /// <summary>
        /// Return a list of InstructionalEvents objects based on selection criteria.
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="section">Section Id</param>
        /// <param name="startOn">Start Date and Time</param>
        /// <param name="endOn">End Date and Time</param>
        /// <param name="room">Room where class is being held</param>
        /// <param name="instructor">Instructor ID</param>
        /// <returns>List of InstructionalEvent2 <see cref="Dtos.InstructionalEvent2"/> objects representing matching InstructionalEvents</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "section", "startOn", "endOn", "room", "instructor" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetHedmInstructionalEventsAsync(Paging page, [FromUri] string section = "",
            [FromUri] string startOn = "", [FromUri] string endOn = "",
            [FromUri] string room = "", [FromUri] string instructor = "")
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (page == null)
            {
                page = new Paging(100, 0);
            }

            if (section == null || startOn == null || endOn == null || room == null || instructor == null)
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent2>>(new List<Dtos.InstructionalEvent2>(), page, 0, this.Request);

            try
            {
                var pageOfItems = await _sectionCoordinationService.GetInstructionalEvent2Async(page.Offset, page.Limit, section, startOn, endOn, room, instructor);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A DTO in the format of the sections LDM</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent2> GetHedmAsync(string id)
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
                 await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { id }));
                return await _sectionCoordinationService.GetInstructionalEvent2Async(id);
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Create (POST) a new section
        /// </summary>
        /// <param name="meeting">DTO of the new section</param>
        /// <returns>A DTO in the format of the updated section's LDM</returns>
        [HttpPost,EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent2> PostHedmAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent2 meeting)
        {
            if (meeting == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null instructionalEvent argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            if (string.IsNullOrEmpty(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null instructionalEvent id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {
                var routeInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(routeInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(extendedData);
                
                //create the course
                var eventReturn = await _sectionCoordinationService.CreateInstructionalEvent2Async(meeting);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { eventReturn.Id }));

                return eventReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, "Argument exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, "Integration API exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occurred");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the instructional event</param>
        /// <param name="meeting">DTO of the updated instructional event</param>
        /// <returns>A DTO in the format of the CDM sections</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent2> PutHedmAsync([FromUri]string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent2 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("Null instructionalEvent argument", "The request body is required.");
            }
            if (!string.IsNullOrEmpty(id))
            {
                if (string.IsNullOrEmpty(meeting.Id))
                {
                    meeting.Id = id;
                }
                else if (id != meeting.Id)
                {
                    throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                        IntegrationApiUtility.GetDefaultApiError("ID not the same as in request body.")));
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }

            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
            }

            if (string.IsNullOrEmpty(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Meeting Id must be provided.")));
            }

            // Compare uri value to body value for section Id
            if (!id.Equals(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("ID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(meeting, async () => await _sectionCoordinationService.GetInstructionalEvent2Async(id), dpList, _logger);
                var eventReturn = await _sectionCoordinationService.UpdateInstructionalEvent2Async(partialmerged);
                
                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _sectionCoordinationService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return eventReturn;

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
        #endregion

        #region HeDM version 8 methods
        /// <summary>
        /// Return a list of InstructionalEvents objects based on selection criteria.
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="criteria">Filter criteria</param>
        /// <param name="academicPeriod">Filter academicPeriod Named Query</param>
        /// <returns>List of InstructionalEvent3 <see cref="Dtos.InstructionalEvent3"/> objects representing matching InstructionalEvents</returns>
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructionalEventFilter2))]
        [QueryStringFilterFilter("academicPeriod", typeof(Dtos.Filters.AcademicPeriodNamedQueryFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetInstructionalEvents3Async(Paging page, QueryStringFilter criteria, QueryStringFilter academicPeriod)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            string section = string.Empty, startOn = string.Empty, endOn = string.Empty, academicPeriodId = string.Empty;
            var roomsList = new List<string>();
            var instructorList = new List<string>();

            var criteriaValues = GetFilterObject<Dtos.Filters.InstructionalEventFilter2>(_logger, "criteria");
            var academicPeriodFilter = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>(_logger, "academicPeriod");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent3>>(new List<Dtos.InstructionalEvent3>(), page, this.Request);

            if (academicPeriodFilter != null)
            {
                if (academicPeriodFilter.AcademicPeriod != null && !string.IsNullOrEmpty(academicPeriodFilter.AcademicPeriod.Id))
                    academicPeriodId = academicPeriodFilter.AcademicPeriod.Id;
            }

            if (criteriaValues != null)
            {
                if (criteriaValues.Section != null && !string.IsNullOrEmpty(criteriaValues.Section.Id))
                    section = criteriaValues.Section.Id;
                if (criteriaValues.StartOn != null && criteriaValues.StartOn != null)
                    startOn = criteriaValues.StartOn.ToString();
                if (criteriaValues.EndOn != null && criteriaValues.EndOn != null)
                    endOn = criteriaValues.EndOn.ToString();
                if (criteriaValues.Recurrence != null && criteriaValues.Recurrence.TimePeriod != null && criteriaValues.Recurrence.TimePeriod.StartOn != null)
                    startOn = criteriaValues.Recurrence.TimePeriod.StartOn.ToString();
                if (criteriaValues.Recurrence != null && criteriaValues.Recurrence.TimePeriod != null && criteriaValues.Recurrence.TimePeriod.EndOn != null)
                    endOn = criteriaValues.Recurrence.TimePeriod.EndOn.ToString();
                if (criteriaValues.AcademicPeriod != null && !string.IsNullOrEmpty(criteriaValues.AcademicPeriod.Id))
                    academicPeriodId = criteriaValues.AcademicPeriod.Id;
                if (criteriaValues.Locations != null && criteriaValues.Locations.Any())
                {
                    foreach (var instructionalLocation in criteriaValues.Locations)
                    {
                        var instructionalRoom = instructionalLocation.Location;
                        if (instructionalRoom.Room != null && !string.IsNullOrEmpty(instructionalRoom.Room.Id))
                        {
                            roomsList.Add(instructionalRoom.Room.Id);
                        }
                    }
                }
                else
                {
                    if (criteriaValues.Room != null && !string.IsNullOrEmpty(criteriaValues.Room.Id))
                        roomsList.Add(criteriaValues.Room.Id);
                }
                if (criteriaValues.Instructors != null && criteriaValues.Instructors.Any())
                {
                    foreach (var instructors in criteriaValues.Instructors)
                    {
                        if (instructors.Instructor != null && !string.IsNullOrEmpty(instructors.Instructor.Id))
                        {
                            instructorList.Add(instructors.Instructor.Id);
                        }
                    }
                }
                else
                {
                    if (criteriaValues.InstructorId != null && !string.IsNullOrEmpty(criteriaValues.InstructorId.Id))
                        instructorList.Add(criteriaValues.InstructorId.Id);
                }
            }
            try
            {                
                var pageOfItems = await _sectionCoordinationService.GetInstructionalEvent3Async(page.Offset, page.Limit, section, startOn, endOn, roomsList, instructorList, academicPeriodId);
                AddEthosContextProperties(
                await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));
                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A DTO in the format of the sections LDM</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent3> GetInstructionalEvent3Async(string id)
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
                    await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        new List<string>() { id }));
                
                return await _sectionCoordinationService.GetInstructionalEvent3Async(id);
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Create (POST) a new section
        /// </summary>
        /// <param name="meeting">DTO of the new section</param>
        /// <returns>A DTO in the format of the updated section's LDM</returns>
        [HttpPost,EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent3> PostInstructionalEvent3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent3 meeting)
        {
            if (meeting == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null instructionalEvent argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }

            if (string.IsNullOrEmpty(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null instructionalEvent id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            try
            {

                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));


                //create the course
                var eventReturn = await _sectionCoordinationService.CreateInstructionalEvent3Async(meeting);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { eventReturn.Id }));

                return eventReturn;
                
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, "Argument exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, "Integration API exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occurred");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the instructional event</param>
        /// <param name="meeting">DTO of the updated instructional event</param>
        /// <returns>A DTO in the format of the CDM sections</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent3> PutInstructionalEvent3Async([FromUri]string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent3 meeting)
        {
            if (meeting == null)
            {
                throw new ArgumentNullException("Null instructionalEvent argument", "The request body is required.");
            }
            if (!string.IsNullOrEmpty(id))
            {
                if (string.IsNullOrEmpty(meeting.Id))
                {
                    meeting.Id = id;
                }
                else if (id != meeting.Id)
                {
                    throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                        IntegrationApiUtility.GetDefaultApiError("ID not the same as in request body.")));
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The ID must be specified in the request URL.")));
            }

            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
            }

            if (string.IsNullOrEmpty(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("Meeting Id must be provided.")));
            }

            // Compare uri value to body value for section Id
            if (!id.Equals(meeting.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("ID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("ID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(meeting, async () => await _sectionCoordinationService.GetInstructionalEvent3Async(id), dpList, _logger);
                var eventReturn = await _sectionCoordinationService.UpdateInstructionalEvent3Async(partialmerged);

                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _sectionCoordinationService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return eventReturn;
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
        #endregion

        #region EeDM version 11 methods

        /// <summary>
        /// Return a list of InstructionalEvents objects based on selection criteria.
        /// </summary>
        /// <param name="page">page</param>
        /// <param name="criteria">Filter criteria</param>
        /// <param name="academicPeriod">Named Query for academicPeriod</param>
        /// <param name="instructionalEventInstances">Named Query for instructionalEventInstances</param>
        /// <returns>List of InstructionalEvent4 <see cref="Dtos.InstructionalEvent4"/> objects representing matching InstructionalEvents</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.InstructionalEventFilter3))]
        [QueryStringFilterFilter("instructionalEventInstances", typeof(Dtos.Filters.InstructionalEventInstancesFilter))]
        [QueryStringFilterFilter("academicPeriod", typeof(Dtos.Filters.AcademicPeriodNamedQueryFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetInstructionalEvents4Async(Paging page, QueryStringFilter criteria, QueryStringFilter academicPeriod, QueryStringFilter instructionalEventInstances)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(100, 0);
            }

            string section = string.Empty, startOn = string.Empty, endOn = string.Empty, academicPeriodId = string.Empty;

            var criteriaValues = GetFilterObject<Dtos.Filters.InstructionalEventFilter3>(_logger, "criteria");
            var academicPeriodFilter = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>(_logger, "academicPeriod");
            var instructionalEventInstancesFilter = GetFilterObject<Dtos.Filters.InstructionalEventInstancesFilter>(_logger, "instructionalEventInstances");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent4>>(new List<Dtos.InstructionalEvent4>(), page, 0, this.Request);

            if (academicPeriodFilter != null)
            {
                if (academicPeriodFilter.AcademicPeriod != null && !string.IsNullOrEmpty(academicPeriodFilter.AcademicPeriod.Id))
                    academicPeriodId = academicPeriodFilter.AcademicPeriod.Id;
            }

            if (instructionalEventInstancesFilter != null)
            {
                if (instructionalEventInstancesFilter.InstructionalEventInstances != null && !string.IsNullOrEmpty(instructionalEventInstancesFilter.InstructionalEventInstances.Id))
                    return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent4>>(new List<Dtos.InstructionalEvent4>(), page, 0, this.Request);
            }

            if (criteriaValues != null)
            {
                if (criteriaValues.Section != null && !string.IsNullOrEmpty(criteriaValues.Section.Id))
                    section = criteriaValues.Section.Id;
                if (criteriaValues.Recurrence != null && criteriaValues.Recurrence.TimePeriod != null && criteriaValues.Recurrence.TimePeriod.StartOn != null)
                    startOn = criteriaValues.Recurrence.TimePeriod.StartOn.ToString();
                if (criteriaValues.Recurrence != null && criteriaValues.Recurrence.TimePeriod != null && criteriaValues.Recurrence.TimePeriod.EndOn != null)
                    endOn = criteriaValues.Recurrence.TimePeriod.EndOn.ToString();
                if (criteriaValues.AcademicPeriod != null && !string.IsNullOrEmpty(criteriaValues.AcademicPeriod.Id))
                    academicPeriodId = criteriaValues.AcademicPeriod.Id;
            }

            try
            {                               
                var pageOfItems = await _sectionCoordinationService.GetInstructionalEvent4Async(page.Offset, page.Limit, section, startOn, endOn, academicPeriodId);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));
                
                return new PagedHttpActionResult<IEnumerable<Dtos.InstructionalEvent4>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) an instructional-event
        /// </summary>
        /// <param name="id">GUID to desired instructional-event</param>
        /// <returns>A DTO in the format of the instructional-event</returns>
        [HttpGet, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent4> GetInstructionalEvent4Async(string id)
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
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _sectionCoordinationService.GetInstructionalEvent4Async(id);
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
        /// Create (POST) a new instructional-event
        /// </summary>
        /// <param name="meeting">DTO of the new instructional-event</param>
        /// <returns>A DTO in the format of the updated instructional-event</returns>
        [HttpPost, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent4> PostInstructionalEvent4Async([ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent4 meeting)
        {          
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the course
                var eventReturn = await _sectionCoordinationService.CreateInstructionalEvent4Async(meeting);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { eventReturn.Id }));

                return eventReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e, "Argument exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e, "Repository exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e, "Integration API exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception occurred");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update (PUT) an existing instructional-event V11
        /// </summary>
        /// <param name="id">GUID of the instructional event</param>
        /// <param name="meeting">DTO of the updated instructional event</param>
        /// <returns>A DTO in the format of InstructionalEvent4</returns>
        [HttpPut, CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2), EedmResponseFilter]
        public async Task<Dtos.InstructionalEvent4> PutInstructionalEvent4Async([FromUri]string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.InstructionalEvent4 meeting)
        {
            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                // Get extended data  
                var resourceInfo = GetEthosResourceRouteInfo();
                var extendedConfig = await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(resourceInfo);
                var extendedData = await ExtractExtendedData(extendedConfig, _logger);

                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(extendedData);

                //do update with partial logic
                var partialmerged = await PerformPartialPayloadMerge(meeting, async () => await _sectionCoordinationService.GetInstructionalEvent4Async(id), dpList, _logger);
                var eventReturn = await _sectionCoordinationService.UpdateInstructionalEvent4Async(partialmerged);

                //store dataprivacy list and extended data
                AddEthosContextProperties(dpList, await _sectionCoordinationService.GetExtendedEthosDataByResource(resourceInfo, new List<string>() { id }));

                return eventReturn;

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
        #endregion

        /// <summary>
        /// Delete (DELETE) an existing section meeting
        /// </summary>
        /// <param name="id">Unique ID of the Instructional Event to delete</param>
        [HttpDelete]
        public async Task DeleteHedmAsync(string id)
        {
            try
            {
                await _sectionCoordinationService.DeleteInstructionalEventAsync(id);
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
    }
}
