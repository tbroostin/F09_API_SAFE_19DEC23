//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using Newtonsoft.Json;
using System.Net.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionInstructors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionInstructorsController : BaseCompressedApiController
    {
        private readonly ISectionInstructorsService _sectionInstructorsService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the SectionInstructorsController class.
        /// </summary>
        /// <param name="sectionInstructorsService">Service of type <see cref="ISectionInstructorsService">ISectionInstructorsService</see></param>
        /// <param name="logger">Interface to logger</param>
        public SectionInstructorsController(ISectionInstructorsService sectionInstructorsService, ILogger logger)
        {
            _sectionInstructorsService = sectionInstructorsService;
            _logger = logger;
        }

        /// <summary>
        /// Return all sectionInstructors
        /// </summary>
        /// <param name="page">API paging info for used to Offset and limit the amount of data being returned.</param>
        /// <param name="criteria">Filter Criteria including section, instructor, and instructionalEvent.</param>
        /// <returns>List of SectionInstructors <see cref="Dtos.SectionInstructors"/> objects representing matching sectionInstructors</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionInstructors))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetSectionInstructorsAsync(Paging page, QueryStringFilter criteria)
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
            string section = string.Empty, instructor = string.Empty;
            List<string> instructionalEvents = new List<string>();

            var criteriaValues = GetFilterObject<Dtos.SectionInstructors>(_logger, "criteria");

            if (CheckForEmptyFilterParameters())
                return new PagedHttpActionResult<IEnumerable<Dtos.SectionInstructors>>(new List<Dtos.SectionInstructors>(), page, 0, this.Request);

            if (criteriaValues != null)
            {
                if (criteriaValues.Section != null && !string.IsNullOrEmpty(criteriaValues.Section.Id))
                    section = criteriaValues.Section.Id;
                if (criteriaValues.Instructor != null && !string.IsNullOrEmpty(criteriaValues.Instructor.Id))
                    instructor = criteriaValues.Instructor.Id;
                if (criteriaValues.InstructionalEvents != null && criteriaValues.InstructionalEvents.Any())
                    instructionalEvents = ConvertGuidObject2ListToStringList(criteriaValues.InstructionalEvents);
            }
            try
            {
                var pageOfItems = await _sectionInstructorsService.GetSectionInstructorsAsync(page.Offset, page.Limit, section, instructor, instructionalEvents, bypassCache);

                AddEthosContextProperties(await _sectionInstructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionInstructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.SectionInstructors>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
        /// Read (GET) a sectionInstructors using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired sectionInstructors</param>
        /// <returns>A sectionInstructors object <see cref="Dtos.SectionInstructors"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionInstructors> GetSectionInstructorsByGuidAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Id is a required property.");
                }

                var bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }


                var sectionInstructor = await _sectionInstructorsService.GetSectionInstructorsByGuidAsync(guid);

                if (sectionInstructor != null)
                {

                    AddEthosContextProperties(await _sectionInstructorsService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionInstructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionInstructor.Id }));
                }

                return sectionInstructor;
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
        /// Create (POST) a new sectionInstructors
        /// </summary>
        /// <param name="sectionInstructors">DTO of the new sectionInstructors</param>
        /// <returns>A sectionInstructors object <see cref="Dtos.SectionInstructors"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.SectionInstructors> PostSectionInstructorsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionInstructors sectionInstructors)
        {
            try
            {
                if (sectionInstructors == null)
                {
                    throw new ArgumentNullException("The request body is required.");
                }

                if (string.IsNullOrEmpty(sectionInstructors.Id))
                {
                    throw new ArgumentNullException("Id is a required property.");
                }
                if (sectionInstructors.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("sectionInstructorsDto", "On a post you can not define a GUID");
                }

                //call import extend method that needs the extracted extension data and the config
                await _sectionInstructorsService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionInstructorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section instructor
                var sectionInstructor = await _sectionInstructorsService.CreateSectionInstructorsAsync(sectionInstructors);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionInstructorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionInstructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionInstructor.Id }));

                return sectionInstructor;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "Permissions exception");
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e, "Argument exception");
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
        /// Update (PUT) an existing sectionInstructors
        /// </summary>
        /// <param name="guid">GUID of the sectionInstructors to update</param>
        /// <param name="sectionInstructors">DTO of the updated sectionInstructors</param>
        /// <returns>A sectionInstructors object <see cref="Dtos.SectionInstructors"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.SectionInstructors> PutSectionInstructorsAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionInstructors sectionInstructors)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("The GUID must be specified in the request URL.");
                }

                if (sectionInstructors == null)
                {
                    throw new ArgumentNullException("The request body is required.");
                }

                if (string.IsNullOrEmpty(sectionInstructors.Id))
                {
                    sectionInstructors.Id = guid;
                }
                else if (guid != sectionInstructors.Id)
                {
                    throw new ArgumentNullException("GUID not the same as in request body.");
                }

                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Nil GUID cannot be used in PUT operation.");
                }
                
                //get Data Privacy List
                var dpList = await _sectionInstructorsService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionInstructorsService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionInstructorsService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionInstructorReturn = await _sectionInstructorsService.UpdateSectionInstructorsAsync(guid,
                    await PerformPartialPayloadMerge(sectionInstructors, async () => await _sectionInstructorsService.GetSectionInstructorsByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionInstructorsService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionInstructorReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
        /// Delete (DELETE) a sectionInstructors
        /// </summary>
        /// <param name="guid">GUID to desired sectionInstructors</param>
        [HttpDelete]
        public async Task DeleteSectionInstructorsAsync(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("section-instructor guid cannot be null or empty");
                }
                await _sectionInstructorsService.DeleteSectionInstructorsAsync(guid);
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
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }
    }
}