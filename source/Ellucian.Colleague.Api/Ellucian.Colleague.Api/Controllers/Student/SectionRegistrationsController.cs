// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to SectionRegistration
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionRegistrationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ISectionRegistrationService _sectionRegistrationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// SectionRegistrationStatusesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="sectionRegistrationService">Service of type <see cref="ICurriculumService">ISectionRegistrationService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public SectionRegistrationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ISectionRegistrationService sectionRegistrationService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _sectionRegistrationService = sectionRegistrationService;
            _logger = logger;
        }

        #region Get Methods

        #region section-registrations V16.0.0

        /// <summary>
        /// Get section registration get by guid.
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration4> GetSectionRegistrationByGuid3Async([FromUri] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistrationByGuid3Async(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }

                return sectionRegistration;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Gets section registrations with filter V16.0.0.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="criteria"></param>
        /// <param name="academicPeriod"></param>
        /// <param name="sectionInstructor"></param>
        /// <param name="registrationStatusesByAcademicPeriod"></param>
        /// <returns></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("criteria", typeof(Dtos.SectionRegistration4))]
        [QueryStringFilterFilter("academicPeriod", typeof(Dtos.Filters.AcademicPeriodNamedQueryFilter))]
        [QueryStringFilterFilter("sectionInstructor", typeof(Dtos.Filters.SectionInstructorQueryFilter))]
        [QueryStringFilterFilter("registrationStatusesByAcademicPeriod", typeof(Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100)]
        public async Task<IHttpActionResult> GetSectionRegistrations3Async(Paging page, QueryStringFilter criteria, QueryStringFilter academicPeriod, 
            QueryStringFilter sectionInstructor, QueryStringFilter registrationStatusesByAcademicPeriod)
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

                //Criteria
                var criteriaObj = GetFilterObject<Dtos.SectionRegistration4>(_logger, "criteria");

                //academicPeriod
                string academicPeriodFilterValue = string.Empty;
                var academicPeriodFilterObj = GetFilterObject<Dtos.Filters.AcademicPeriodNamedQueryFilter>(_logger, "academicPeriod");
                if (academicPeriodFilterObj != null && academicPeriodFilterObj.AcademicPeriod != null && !string.IsNullOrEmpty(academicPeriodFilterObj.AcademicPeriod.Id))
                {
                    academicPeriodFilterValue = academicPeriodFilterObj.AcademicPeriod.Id != null ? academicPeriodFilterObj.AcademicPeriod.Id : null;
                }

                //sectionInstructor
                string sectionInstructorFilterValue = string.Empty;
                var sectionInstructorFilterObj = GetFilterObject<Dtos.Filters.SectionInstructorQueryFilter>(_logger, "sectionInstructor");
                if (sectionInstructorFilterObj != null && sectionInstructorFilterObj.SectionInstructorId != null && !string.IsNullOrEmpty(sectionInstructorFilterObj.SectionInstructorId.Id))
                {
                    sectionInstructorFilterValue = sectionInstructorFilterObj.SectionInstructorId.Id != null ? sectionInstructorFilterObj.SectionInstructorId.Id : null;
                }

                var registrationStatusesByAcademicPeriodObj = GetFilterObject<Dtos.Filters.RegistrationStatusesByAcademicPeriodFilter>(_logger, "registrationStatusesByAcademicPeriod");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.SectionRegistration4>>(new List<Dtos.SectionRegistration4>(), page, 0, this.Request);

                var pageOfItems = await _sectionRegistrationService.GetSectionRegistrations3Async(page.Offset, page.Limit, criteriaObj, academicPeriodFilterValue, 
                                        sectionInstructorFilterValue, registrationStatusesByAcademicPeriodObj, bypassCache);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.SectionRegistration4>>(pageOfItems.Item1, page, pageOfItems.Item2, Request);
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

        #endregion section-registrations V16.0.0

        /// <summary>
        /// Get section registration
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration2> GetSectionRegistrationAsync([FromUri] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistrationAsync(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }


                return sectionRegistration;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Get section registration V7
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration3> GetSectionRegistration2Async([FromUri] string guid)
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
                var sectionRegistration = await _sectionRegistrationService.GetSectionRegistration2Async(guid);

                if (sectionRegistration != null)
                {

                    AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { sectionRegistration.Id }));
                }


                return sectionRegistration;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Gets section registrations with filter
        /// </summary>
        /// <param name="page"></param>
        /// <param name="section"></param>
        /// <param name="registrant"></param>
        /// <returns></returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "section", "registrant"}, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetSectionRegistrationsAsync(Paging page, [FromUri] string section = "",
            [FromUri] string registrant = "")
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
                var pageOfItems = await
                        _sectionRegistrationService.GetSectionRegistrationsAsync(page.Offset, page.Limit, section, registrant);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.SectionRegistration2>>(pageOfItems.Item1, page,  pageOfItems.Item2, Request);
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
        /// Gets section registrations with filter V7
        /// </summary>
        /// <param name="page"></param>
        /// <param name="section"></param>
        /// <param name="registrant"></param>
        /// <returns></returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "section", "registrant" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetSectionRegistrations2Async(Paging page, [FromUri] string section = "",
            [FromUri] string registrant = "")
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

               var pageOfItems = await _sectionRegistrationService.GetSectionRegistrations2Async(page.Offset, page.Limit,
                    section, registrant);

                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.SectionRegistration3>>(pageOfItems.Item1, page, pageOfItems.Item2, Request);
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

        #region Put Methods

        /// <summary>
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration2> PutSectionRegistrationAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration2 sectionRegistration)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }

                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistrationAsync(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistrationAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn;  

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration3> PutSectionRegistration2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration3 sectionRegistration)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }
                if (!guid.Equals(sectionRegistration.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("GUID not the same as in request body.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistration2Async(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistration2Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn; 

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Update (PUT) section registrations
        /// </summary>
        /// <param name="guid">Id of the SectionRegistration</param>
        /// <param name="sectionRegistration">DTO of the SectionRegistration</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration4> PutSectionRegistrations3Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration4 sectionRegistration)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    throw new ArgumentNullException("Null sectionRegistration guid", "guid is a required property.");
                }
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (guid.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Nil GUID cannot be used in PUT operation.");
                }
                if (!guid.Equals(sectionRegistration.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("GUID not the same as in request body.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    sectionRegistration.Id = guid.ToUpperInvariant();
                }

                //get Data Privacy List
                var dpList = await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension dataa and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //do update with partial logic
                var sectionRegistrationReturn = await _sectionRegistrationService.UpdateSectionRegistration3Async(guid,
                    await PerformPartialPayloadMerge(sectionRegistration, async () => await _sectionRegistrationService.GetSectionRegistrationByGuid3Async(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return sectionRegistrationReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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

        #region Post Methods

        /// <summary>
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration2"/> object</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration2> PostSectionRegistrationAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration2 sectionRegistration)
        {
            try
            {
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistrationAsync(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;

            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration3"/> object</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration3> PostSectionRegistration2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration3 sectionRegistration)
        {
            try
            {
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistration2Async(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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
        /// Create (POST) section registrations
        /// </summary>
        /// <param name="sectionRegistration">A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</param>
        /// <returns>A SectionRegistration <see cref="Dtos.SectionRegistration4"/> object</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.SectionRegistration4> PostSectionRegistrations3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionRegistration4 sectionRegistration)
        {
            try
            {
                if (sectionRegistration == null)
                {
                    throw new ArgumentNullException("Null sectionRegistration argument", "The request body is required.");
                }
                if (string.IsNullOrEmpty(sectionRegistration.Id))
                {
                    throw new ArgumentNullException("Null sectionRegistration id", "Id is a required property.");
                }
                //call import extend method that needs the extracted extension data and the config
                await _sectionRegistrationService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionRegistrationService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section registration
                var sectionRegistrationReturn = await _sectionRegistrationService.CreateSectionRegistration3Async(sectionRegistration);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionRegistrationService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionRegistrationService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionRegistrationReturn.Id }));

                return sectionRegistrationReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (ArgumentOutOfRangeException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
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

        #region Delete Methods

        /// <summary>
        /// Delete (DELETE) an existing section-registrations
        /// </summary>
        /// <param name="guid">Id of the section-registration to delete</param>
        [HttpDelete]
        public async Task DeleteSectionRegistrationAsync([FromUri] string guid)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        #endregion
    }
}
