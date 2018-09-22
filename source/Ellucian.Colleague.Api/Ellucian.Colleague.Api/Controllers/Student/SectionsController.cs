// Copyright 2014-2018 Ellucian Company L.P. and its affiliates

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json;
using Section = Ellucian.Colleague.Dtos.Student.Section;
using Section2 = Ellucian.Colleague.Dtos.Student.Section2;
using Section3 = Ellucian.Colleague.Dtos.Student.Section3;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.Converters;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionsController : BaseCompressedApiController
    {
        private readonly ISectionCoordinationService _sectionCoordinationService;
        private readonly ISectionRegistrationService _sectionRegistrationService;
        private readonly IRegistrationGroupService _registrationGroupService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="sectionCoordinationService">Service of type <see cref="ISectionCoordinationService">ISectionCoordinationService</see></param>
        /// <param name="sectionRegistrationService">Service of type <see cref="ISectionRegistrationService">ISectionRegistrationService</see></param>
        /// <param name="registrationGroupService">Service of type <see cref="IRegistrationGroupService">IRegistrationGroupService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionsController(ISectionCoordinationService sectionCoordinationService,
            ISectionRegistrationService sectionRegistrationService,
            IRegistrationGroupService registrationGroupService,
            ILogger logger)
        {
            _sectionCoordinationService = sectionCoordinationService;
            _sectionRegistrationService = sectionRegistrationService;
            _registrationGroupService = registrationGroupService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section">Section</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section a list of active students Ids is not retrieved and
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        ///[CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        [Obsolete("Obsolete as of Api version 1.3, use version 2 of this API")]
        [ParameterSubstitutionFilter]
        public async Task<Section> GetSectionAsync(string sectionId)
        {
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSectionAsync(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionDto;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.Error(exception,exception.Message);
                throw CreateNotFoundException("Section",sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
           
        }

        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section2">Section</see></returns>
        ///  <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids  and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        ///[CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        [Obsolete("Obsolete as of Api version 1.5, use version 3 of this API")]
        [ParameterSubstitutionFilter]
        public async Task<Section2> GetSection2Async(string sectionId)
        {
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSection2Async(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section2;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionDto;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves information about a specific course section. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionId">Id of the section desired</param>
        /// <returns>The requested <see cref="Dtos.Student.Section3">Section3</see></returns>
        ///  <accessComments>
        /// Any authenticated user can retrieve course section information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a course section.
        /// For all other users that are not assigned faculty to a course section cannot retrieve list of active students Ids  and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [ParameterSubstitutionFilter]
        public async Task<Section3> GetSection3Async(string sectionId)
        {
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
             try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSection3Async(sectionId, useCache);
                var sectionDto = privacyWrapper.Dto as Ellucian.Colleague.Dtos.Student.Section3;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionDto;
            }
            catch (KeyNotFoundException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateNotFoundException("Section", sectionId);
            }
            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves roster information for a course section.
        /// </summary>
        /// <param name="sectionId">ID of the course section for which roster students will be retrieved</param>
        /// <returns>All <see cref="RosterStudent">students</see> in the course section</returns>
        [ParameterSubstitutionFilter]
        [Obsolete("Obsolete as of Api version 1.19, use version 2 of this API")]
        public async Task<IEnumerable<RosterStudent>> GetSectionRosterAsync(string sectionId)
        {
            try
            {
                return await _sectionCoordinationService.GetSectionRosterAsync(sectionId);
            }
            catch (ArgumentNullException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (ApplicationException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Get a course roster for a given course section ID
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        /// <returns>A course roster</returns>
        [ParameterSubstitutionFilter]
        public async Task<SectionRoster> GetSectionRoster2Async(string sectionId)
        {
            try
            {
                return await _sectionCoordinationService.GetSectionRoster2Async(sectionId);
            }
            catch (ArgumentNullException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Retrieves the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">comma delimited list of section IDs</param>
        /// <returns>The requested <see cref="Section">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.3, use version 2 of this API")]
        public async Task<IEnumerable<Section>> GetSectionsAsync(string sectionIds)
        {
            if (string.IsNullOrEmpty(sectionIds))
            {
                
                    string errorText = "At least one item in list of sectionIds must be provided.";
                    _logger.Error(errorText);
                    throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
                
            }
            var lstOfSectionIds = sectionIds.Trim().Split(',').ToList();

            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSectionsAsync(lstOfSectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Retrieves the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">comma delimited list of section IDs</param>
        /// <returns>The requested <see cref="Section2">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.4, use endpoint POST qapi/sections")]
        public async Task<IEnumerable<Section2>> GetSections2Async(string sectionIds)
        {
            if (string.IsNullOrEmpty(sectionIds))
            {

                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);

            }
            var lstOfSectionIds = sectionIds.Trim().Split(',').ToList();
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections2Async(lstOfSectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">list of section IDs</param>
        /// <returns>The requested <see cref="Section2">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and  
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.5, use version 2 of this API")]
        [HttpPost]
        public async Task<IEnumerable<Section2>> QuerySectionsByPostAsync([FromBody] IEnumerable<string> sectionIds)
        {
            bool useCache = true;
            if (sectionIds == null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections2Async(sectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section2>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }
           
            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="sectionIds">list of section IDs</param>
        /// <returns>The requested <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        /// For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and  
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [Obsolete("Obsolete as of Api version 1.6, use version 3 of this API")]
        [HttpPost]
        public async Task<IEnumerable<Section3>> QuerySectionsByPost2Async([FromBody] IEnumerable<string> sectionIds)
        {
            bool useCache = true;
            if (sectionIds == null )
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }
            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections3Async(sectionIds, useCache);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        #region HeDM Methods

        /// <summary>
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">GUID of the Section</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A registration response object</returns>
        [Obsolete("Obsolete as of HeDM Version 4, use Accept Header Version 4 instead.")]
        [HttpPut]
        public async Task<Dtos.SectionRegistration> PutSectionRegistrationAsync([FromUri] string guid, [FromBody] Dtos.SectionRegistration sectionRegistration)
        {
            if (sectionRegistration == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionRegistration.Guid))
            {
                sectionRegistration.Guid = sectionRegistration.Section.Guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != sectionRegistration.Guid.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                return await _sectionRegistrationService.UpdateRegistrationAsync(sectionRegistration);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
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

        #region EEDM V6 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page">Section page Contains ...page...</param>
        /// <param name="title">Section Title Contains ...title...</param>
        /// <param name="startOn">Section starts on or after this date</param>
        /// <param name="endOn">Section ends on or before this date</param>
        /// <param name="code">Section Name Contains ...code...</param>
        /// <param name="number">Section Number equal to</param>
        /// <param name="instructionalPlatform">Learning Platform equal to (guid)</param>
        /// <param name="academicPeriod">Section Term equal to (guid)</param>
        /// <param name="academicLevels">Section Academic Level equal to (guid)</param>
        /// <param name="course">Section Course equal to (guid)</param>
        /// <param name="site">Section Location equal to (guid)</param>
        /// <param name="status">Section Status matches closed, open, pending, or cancelled</param>
        /// <param name="owningInstitutionUnits">Section Department equal to (guid)</param>
        /// <returns>List of Section2 <see cref="Dtos.Section3"/> objects representing matching sections</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "title", "startOn", "endOn", "code", "number", "instructionalPlatform", "academicPeriod", "academicLevels", "course", "site", "status", "owningInstitutionUnits" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetHedmSections2Async(Paging page, [FromUri] string title = "", [FromUri] string startOn = "", [FromUri] string endOn = "",
            [FromUri] string code = "", [FromUri] string number = "", [FromUri] string instructionalPlatform = "", [FromUri] string academicPeriod = "",
            [FromUri] string academicLevels = "", [FromUri] string course = "", [FromUri] string site = "", [FromUri] string status = "", [FromUri] string owningInstitutionUnits = "")
        {
            if (page == null)
            {
                page = new Paging(100, 0);
            }
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (title == null || startOn == null || endOn == null || code == null || number == null || instructionalPlatform == null || academicPeriod == null || academicLevels == null || course == null || site == null || status == null || owningInstitutionUnits == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Section3>>(new List<Dtos.Section3>(), page, 0, this.Request);
            }

            try
            {
                if ((!string.IsNullOrEmpty(status)) && (!ValidEnumerationValue(typeof(SectionStatus2), status)))
                {
                    throw new Exception(string.Concat("'", status, "' is an invalid enumeration value. "));
                }

                var pageOfItems = await _sectionCoordinationService.GetSections3Async(page.Offset, page.Limit, title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels, course, site, status, owningInstitutionUnits);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Section3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Section3> GetHedmSectionByGuid2Async(string id)
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
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                 return await _sectionCoordinationService.GetSection3ByGuidAsync(id);
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
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Section3> PostHedmSection2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section3 section)
        {
            if (section == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection3Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section3"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Section3> PutHedmSection2Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section3 section)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection3Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection3ByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>Nothing</returns>
        [HttpDelete]
        public async Task DeleteHedmSectionByGuid2Async(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region EEDM V8 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <param name="searchable"></param>
        /// <param name="keywordSearch"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section4 <see cref="Dtos.Section4"/> objects representing matching sections</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.SectionFilter))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetHedmSections4Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter  keywordSearch , QueryStringFilter subject, 
           QueryStringFilter instructor)           
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {              
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
                            
                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Filters.SectionFilter>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id)) 
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null  && !(string.IsNullOrEmpty(criteriaObj.Course.Id )) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                    // Subject needs to be supported in criteria object
                    if (string.IsNullOrEmpty(subjectName))
                    {
                        subjectName = criteriaObj.Subject != null && !string.IsNullOrEmpty(criteriaObj.Subject.Id) ? criteriaObj.Subject.Id : string.Empty;
                    }
                    if (string.IsNullOrEmpty(instructorId))
                    {
                        instructorId = criteriaObj.Instructor != null && !string.IsNullOrEmpty(criteriaObj.Instructor.Id) ? criteriaObj.Instructor.Id : string.Empty;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Section4>>(new List<Dtos.Section4>(), page, 0, this.Request);

                var pageOfItems = await _sectionCoordinationService.GetSections4Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Section4>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Section4> GetHedmSectionByGuid3Async(string id)
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
                AddEthosContextProperties(
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { id }));
                return await _sectionCoordinationService.GetSection4ByGuidAsync(id);
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
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Section4> PostHedmSection4Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section4 section)
        {
            if (section == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection4Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="id">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Section4> PutHedmSection4Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section4 section)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = id.ToLowerInvariant();
            }
            else if (id.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection4Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection4ByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a section
        /// </summary>
        /// <param name="id">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section4"/> in HeDM format</returns>
        [HttpDelete]
        public async Task DeleteHedmSectionByGuid4Async(string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion

        #region EEDM V11 Methods

        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria"> - JSON formatted selection criteria.  Can contain:</param>
        /// <param name="searchable"></param>
        /// <param name="keywordSearch"></param>
        /// <param name="subject"></param>
        /// <param name="instructor"></param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section5 <see cref="Dtos.Section5"/> objects representing matching sections</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.SectionFilter2))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetHedmSections5Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter keywordSearch, QueryStringFilter subject,
           QueryStringFilter instructor)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {
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

                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Filters.SectionFilter2>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Title != null ? criteriaObj.Title : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id))
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null && !(string.IsNullOrEmpty(criteriaObj.Course.Id)) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                    if (string.IsNullOrEmpty(subjectName))
                    {
                        subjectName = criteriaObj.SubjectName != null && !string.IsNullOrEmpty(criteriaObj.SubjectName.Id) ? criteriaObj.SubjectName.Id : string.Empty;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Section5>>(new List<Dtos.Section5>(), page, 0, this.Request);

                var pageOfItems = await _sectionCoordinationService.GetSections5Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Section5>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Section5> GetHedmSectionByGuid5Async(string guid)
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
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionCoordinationService.GetSection5ByGuidAsync(guid);
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
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Section5> PostHedmSection5Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section5 section)
        {
            if (section == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection5Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="guid">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section5"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Section5> PutHedmSection5Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section5 section)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection5Async(
                    await PerformPartialPayloadMerge(section, async () => await _sectionCoordinationService.GetSection5ByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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


        #endregion EEDM V11 Methods

        #region EEDM V16 Methods
        /// <summary>
        /// Return a list of Sections objects based on selection criteria.
        /// </summary>
        /// <param name="page"> - Section page Contains ...page...</param>
        /// <param name="criteria">filter criteria</param>
        /// <param name="searchable">named query</param>
        /// <param name="keywordSearch">named query</param>
        /// <param name="subject">named query</param>
        /// <param name="instructor">named query</param>
        /// "title" - Section Title Contains ...title...
        /// "startOn" - Section starts on or after this date
        /// "endOn" - Section ends on or before this date
        /// "code" - Section Name Contains ...code...
        /// "number" - Section Number equal to
        /// "instructionalPlatform" - Learning Platform equal to (guid)
        /// "academicPeriod" - Section Term equal to (guid)
        /// "academicLevels" - Section Academic Level equal to (guid)
        /// "course" - Section Course equal to (guid)
        /// "site" - Section Location equal to (guid)
        /// "status" - Section Status matches closed, open, pending, or cancelled
        /// "owningInstitutionUnits" - Section Department equal to (guid) [renamed from owningOrganizations in v8]
        /// <returns>List of Section6 <see cref="Dtos.Section6"/> objects representing matching sections</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Section6))]
        [QueryStringFilterFilter("searchable", typeof(Dtos.Filters.SearchableFilter))]
        [QueryStringFilterFilter("keywordSearch", typeof(Dtos.Filters.KeywordSearchFilter))]
        [QueryStringFilterFilter("subject", typeof(Dtos.Filters.SubjectFilter))]
        [QueryStringFilterFilter("instructor", typeof(Dtos.Filters.InstructorFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetHedmSections6Async(Paging page, QueryStringFilter criteria,
           QueryStringFilter searchable, QueryStringFilter keywordSearch, QueryStringFilter subject,
           QueryStringFilter instructor)
        {
            string title = string.Empty, startOn = string.Empty, endOn = string.Empty, code = string.Empty,
                   number = string.Empty, instructionalPlatform = string.Empty, academicPeriod = string.Empty,
                   reportingAcademicPeriod = string.Empty, course = string.Empty, site = string.Empty, status = string.Empty,
                   instructorId = string.Empty, subjectName = string.Empty, keyword = string.Empty;

            var bypassCache = false;

            List<string> academicLevels = new List<string>(), owningOrganizations = new List<string>();
            SectionsSearchable search = SectionsSearchable.NotSet;

            try
            {
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

                var keywordSearchObj = GetFilterObject<Dtos.Filters.KeywordSearchFilter>(_logger, "keywordSearch");
                if (keywordSearchObj != null)
                {
                    keyword = keywordSearchObj.Search;
                }
                var searchableObj = GetFilterObject<Dtos.Filters.SearchableFilter>(_logger, "searchable");
                if (searchableObj != null)
                {
                    search = searchableObj.Search;
                }
                var subjectObj = GetFilterObject<Dtos.Filters.SubjectFilter>(_logger, "subject");
                if (subjectObj != null)
                {
                    subjectName = subjectObj.SubjectName != null && !string.IsNullOrEmpty(subjectObj.SubjectName.Id) ? subjectObj.SubjectName.Id : string.Empty;
                }
                var instructorObj = GetFilterObject<Dtos.Filters.InstructorFilter>(_logger, "instructor");
                if (instructorObj != null)
                {
                    instructorId = instructorObj.InstructorId != null && !string.IsNullOrEmpty(instructorObj.InstructorId.Id) ? instructorObj.InstructorId.Id : string.Empty;
                }
                var criteriaObj = GetFilterObject<Dtos.Section6>(_logger, "criteria");
                if (criteriaObj != null)
                {
                    title = criteriaObj.Titles != null && !string.IsNullOrEmpty(criteriaObj.Titles[0].Value) ? criteriaObj.Titles[0].Value : string.Empty;
                    startOn = criteriaObj.StartOn != null ? criteriaObj.StartOn.ToString() : string.Empty;
                    endOn = criteriaObj.EndOn != null ? criteriaObj.EndOn.ToString() : string.Empty;
                    code = criteriaObj.Code != null ? criteriaObj.Code : string.Empty;
                    number = criteriaObj.Number != null ? criteriaObj.Number : string.Empty;
                    instructionalPlatform = criteriaObj.InstructionalPlatform != null && !(string.IsNullOrEmpty(criteriaObj.InstructionalPlatform.Id))
                        ? criteriaObj.InstructionalPlatform.Id : string.Empty;
                    academicPeriod = criteriaObj.AcademicPeriod != null ? criteriaObj.AcademicPeriod.Id : string.Empty;
                    reportingAcademicPeriod = criteriaObj.ReportingAcademicPeriod != null ? criteriaObj.ReportingAcademicPeriod.Id : string.Empty;
                    academicLevels = criteriaObj.AcademicLevels != null ? ConvertGuidObject2ListToStringList(criteriaObj.AcademicLevels) : new List<string>();
                    course = criteriaObj.Course != null && !(string.IsNullOrEmpty(criteriaObj.Course.Id)) ? criteriaObj.Course.Id : string.Empty;
                    site = criteriaObj.Site != null && !(string.IsNullOrEmpty(criteriaObj.Site.Id)) ? criteriaObj.Site.Id : string.Empty;
                    status = ((criteriaObj.Status != null) && (criteriaObj.Status.Category != SectionStatus2.NotSet))
                        ? criteriaObj.Status.Category.ToString() : string.Empty;

                    if ((criteriaObj.OwningInstitutionUnits != null) && (criteriaObj.OwningInstitutionUnits.Any()))
                    {
                        var organizations = new List<string>();
                        foreach (var owningInstitutionUnit in criteriaObj.OwningInstitutionUnits)
                        {
                            if ((owningInstitutionUnit != null) && (owningInstitutionUnit.InstitutionUnit != null))
                            {
                                organizations.Add(owningInstitutionUnit.InstitutionUnit.Id);
                            }
                        }
                        owningOrganizations = organizations;
                    }
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Section5>>(new List<Dtos.Section5>(), page, 0, this.Request);

                var pageOfItems = await _sectionCoordinationService.GetSections6Async(page.Offset, page.Limit,
                    title, startOn, endOn, code, number, instructionalPlatform, academicPeriod, reportingAcademicPeriod, academicLevels,
                    course, site, status, owningOrganizations, subjectName, instructorId, search, keyword, bypassCache);

                AddEthosContextProperties(
                  await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                  await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                      pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Section6>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a section using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Section6> GetHedmSectionByGuid6Async(string guid)
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
                   await _sectionCoordinationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                       new List<string>() { guid }));
                return await _sectionCoordinationService.GetSection6ByGuidAsync(guid);
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
        /// Create (POST) a new section
        /// </summary>
        /// <param name="section">DTO of the new section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Section6> PostHedmSection6Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Section6 section)
        {
            if (section == null)
            {
                throw CreateHttpResponseException("Request body must contain a valid Section.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }
            if (section.Id != Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }

            try
            {
                // Don't allow update to alternate ID field
                if (section.AlternateIds != null && section.AlternateIds.Any())
                {
                    foreach (var altIds in section.AlternateIds)
                    {
                        if (!string.IsNullOrEmpty(altIds.Value))
                        {
                            throw new ArgumentException("alternateIds cannot be assigned in a POST request. ", "alternateIds");
                        }
                    }
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section
                var sectionReturn = await _sectionCoordinationService.PostSection6Async(section);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionReturn.Id }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing section
        /// </summary>
        /// <param name="guid">GUID of the section to update</param>
        /// <param name="section">DTO of the updated section</param>
        /// <returns>A section object <see cref="Dtos.Section6"/> in HeDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Section6> PutHedmSection6Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.Section6 section)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (section == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null section argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(section.Id))
            {
                section.Id = guid.ToLowerInvariant();
            }
            else if (guid.ToLowerInvariant() != section.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            Guid guidOutput;
            if (!Guid.TryParse(guid, out guidOutput) || guid == Guid.Empty.ToString())
            {
                throw CreateHttpResponseException("Input ID is not a valid GUID.", HttpStatusCode.BadRequest);
            }

            try
            {
                //get Data Privacy List
                var dpList = await _sectionCoordinationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCoordinationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCoordinationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var origSectionData = new Dtos.Section6();
                try
                {
                    origSectionData = await _sectionCoordinationService.GetSection6ByGuidAsync(guid);
                }
                catch (KeyNotFoundException)
                {
                    origSectionData = null;  
                }

                if (origSectionData != null)
                {
                    if (section.Waitlist != null && section.Waitlist.Eligible == SectionWaitlistEligible.NotEligible)
                    {
                        // Replace waitlist object with the incoming object and ignore for partial merge.
                        origSectionData.Waitlist = section.Waitlist;
                    }
                    // Don't allow a change to the charge assessment method
                    if (origSectionData.ChargeAssessmentMethod != null && !string.IsNullOrEmpty(origSectionData.ChargeAssessmentMethod.Id))
                    {
                        if (section.ChargeAssessmentMethod != null && !string.IsNullOrEmpty(section.ChargeAssessmentMethod.Id))
                        {
                            if (origSectionData.ChargeAssessmentMethod.Id != section.ChargeAssessmentMethod.Id)
                            {
                                throw new ArgumentException("The charge assessment method cannot be changed on a PUT request.", "chargeAssessmentMethod.id");
                            }
                        }
                    }
                    // Don't allow update/change to alternate ID field
                    if (section.AlternateIds != null && section.AlternateIds.Any())
                    {
                        foreach (var altIds in section.AlternateIds)
                        {
                            if (altIds != null)
                            {
                                if (altIds.Title != "Source Key")
                                {
                                    throw new ArgumentException("The only alternateIds.title supported is 'Source Key'. ", "alternateIds.title");
                                }
                                var origAltIdsObject = origSectionData.AlternateIds.Where(sk => sk.Title == "Source Key").FirstOrDefault();
                                if (origAltIdsObject == null || altIds.Value != origAltIdsObject.Value)
                                {
                                    throw new ArgumentException("The 'Source Key' cannot be changed in a PUT request. ", "alternateIds.value");
                                }
                            }
                        }
                    }
                }
                //do update with partial logic
                var sectionReturn = await _sectionCoordinationService.PutSection6Async(
                    await PerformPartialPayloadMerge(section, origSectionData,
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCoordinationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionReturn;
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
            catch (ConfigurationException e)
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

        #endregion EEDM V13 Methods

        /// <summary>
        /// Query by post method used to get the sections for the given section Ids. 
        /// If the request header "Cache-Control" attribute is set to "no-cache" the data returned will be pulled fresh from the database, otherwise cached data is returned.
        /// </summary>
        /// <param name="criteria">DTO Object with a list of Section keys</param>
        /// <returns>The requested <see cref="Section3">Sections</see></returns>
        /// <accessComments>
        /// Any authenticated user can retrieve course sections information; however,
        /// only an assigned faculty user may retrieve list of active students Ids in a given course section.
        ///For all other users that are not assigned faculty to a given course section a list of active students Ids is not retrieved and 
        /// response object is returned with a X-Content-Restricted header with a value of "partial".
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Section3>> QuerySectionsByPost3Async([FromBody] SectionsQueryCriteria criteria)
        {
            if (criteria == null  || criteria.SectionIds==null)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            bool bestFit = criteria.BestFit;
            bool useCache = true;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    useCache = false;
                }
            }

            try
            {
                var privacyWrapper = await _sectionCoordinationService.GetSections3Async(criteria.SectionIds, useCache, bestFit);
                var sectionsDto = privacyWrapper.Dto as List<Ellucian.Colleague.Dtos.Student.Section3>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    System.Web.HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                return sectionsDto;
            }

            catch (ArgumentNullException exception)
            {
                _logger.Error(exception, exception.Message);
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

       

        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        [Obsolete("Obsolete , use version 2 of this API")]
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGradesAsync([FromUri] string sectionId, [FromBody] SectionGrades sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGradesAsync(sectionGrades);
                return returnDto;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        [Obsolete("Obsolete as of Api version 1.12, use version 3 of this API")]
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGrades2Async([FromUri] string sectionId, [FromBody] SectionGrades2 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades2Async(sectionGrades);
                return returnDto;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        [Obsolete("Obsolete as of Api version 1.13, use version 4 for non-ILP callers, or version 1 of the json ILP header for ILP callers")]
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGrades3Async([FromUri] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades3Async(sectionGrades);
                return returnDto;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades from a standard non-ILP caller.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<SectionGradeResponse>> PutCollectionOfStudentGrades4Async([FromUri] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportGrades4Async(sectionGrades);
                return returnDto;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Puts a collection of student section grades from an ILP caller.
        /// </summary>
        /// <param name="sectionId">Section ID</param>
        /// <param name="sectionGrades">DTO of section grade information</param>
        /// <returns><see cref="Dtos.Student.Grade">StudentSectionGradeResponse</see></returns>
        public async Task<IEnumerable<SectionGradeResponse>> PutIlpCollectionOfStudentGrades1Async([FromUri] string sectionId, [FromBody] SectionGrades3 sectionGrades)
        {
            try
            {
                if (ModelState != null && !ModelState.IsValid)
                {
                    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).ToList();
                    if (modelErrors != null && modelErrors.Count() > 0)
                    {
                        var formatExceptions = modelErrors.Where(x => x.Exception is System.FormatException).Select(x => x.Exception as System.FormatException).ToList();

                        if (formatExceptions != null && formatExceptions.Count() > 0)
                        {
                            throw formatExceptions.First();
                        }
                    }
                }

                if (string.IsNullOrEmpty(sectionGrades.SectionId))
                {
                    throw new ArgumentException("SectionId", "Section Id must be provided.");
                }

                // Compare uri value to body value for section Id
                if (!sectionId.Equals(sectionGrades.SectionId))
                {
                    throw new ArgumentException("sectionId", "Section Ids do not match in the request.");
                }

                if (sectionGrades.StudentGrades == null || sectionGrades.StudentGrades.Count() == 0)
                {
                    throw new ArgumentException("StudentGrades", "At least one student grade must be provided.");
                }

                var returnDto = await _sectionCoordinationService.ImportIlpGrades1Async(sectionGrades);
                return returnDto;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query by post method used to get the section registration date overrides for any of the specified section Ids based on the registration group of the person making the request. 
        /// </summary>
        /// <param name="criteria">DTO Object that contains the list of Section ids for which registration dates are requested.</param>
        /// <returns><see cref="SectionRegistrationDate">SectionRegistrationDate</see> DTOs.</returns>
        [HttpPost]
        public async Task<IEnumerable<SectionRegistrationDate>> QuerySectionRegistrationDatesAsync([FromBody] SectionDateQueryCriteria criteria)
        {
            IEnumerable<string> sectionIds = criteria.SectionIds;

            if (sectionIds == null || sectionIds.Count() == 0)
            {
                string errorText = "At least one item in list of sectionIds must be provided.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _registrationGroupService.GetSectionRegistrationDatesAsync(sectionIds);

            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Get all section meeting instances for a specific section id
        /// </summary>
        /// <param name="sectionId">Id of Section. (Required)</param>
        /// <returns>The requested section <see cref="SectionMeetingInstance">meeting instances</see></returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>NotFound.</exception>
        [ParameterSubstitutionFilter]
        public async Task<IEnumerable<SectionMeetingInstance>> GetSectionMeetingInstancesAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                string errText = "Section ID must be provided to get section meeting instances.";
                _logger.Error(errText);
                throw CreateHttpResponseException(errText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _sectionCoordinationService.GetSectionMeetingInstancesAsync(sectionId);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("section events", sectionId);
            }
        }      
    }
}
