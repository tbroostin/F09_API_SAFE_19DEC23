// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
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
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;


namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to course Section data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionCrosslistsController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly ISectionCrosslistService _sectionCrosslistService;

        /// <summary>
        /// Initializes a new instance of the SectionsController class.
        /// </summary>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="sectionCrosslist">SectionCrosslist Service <see cref="ISectionCrosslistService">ISectionCrosslistService</see></param>
        public SectionCrosslistsController(ILogger logger, ISectionCrosslistService sectionCrosslist)
        {
            _logger = logger;
            _sectionCrosslistService = sectionCrosslist;
        }


        /// <summary>
        /// Read (GET) all SectionCrosslists or all SectionCrosslists with section selected in filter
        /// </summary>
        /// <param name="section">GUID to desired Section to filter SectionCrosslists by</param>
        /// <param name="page">paging data from the url</param>
        /// <returns>A List SectionCrosslist object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "section" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 500), EedmResponseFilter]
        public async Task<IHttpActionResult> GetDataModelSectionCrosslistsAsync(Paging page, [FromUri] string section = "")
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (section == null || section == "null")
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.SectionCrosslist>>(new List<Dtos.SectionCrosslist>(), page, 0, this.Request);
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(500, 0);
                }

                var pageOfItems = await _sectionCrosslistService.GetDataModelSectionCrosslistsPageAsync(page.Offset, page.Limit, section);

                AddEthosContextProperties(await _sectionCrosslistService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.SectionCrosslist>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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

        /// <summary>
        /// Read (GET) a SectionCrosslist using a GUID
        /// </summary>
        /// <param name="id">GUID to desired SectionCrosslist</param>
        /// <returns>A SectionCrosslist object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.SectionCrosslist> GetDataModelSectionCrosslistsByGuidAsync(string id)
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
                var crossslist = await _sectionCrosslistService.GetDataModelSectionCrosslistsByGuidAsync(id);

                if (crossslist != null)
                {

                    AddEthosContextProperties((await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList(),
                              await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { crossslist.Id }));
                }


                return crossslist;

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
            catch (KeyNotFoundException	e)
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

        /// <summary>
        /// Create (POST) a new SectionCrosslist
        /// </summary>
        /// <param name="sectionCrosslist">DTO of the new SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.SectionCrosslist> PostDataModelSectionCrosslistsAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.SectionCrosslist sectionCrosslist)
        {
            if (sectionCrosslist == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionCrosslist.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist id",
                    IntegrationApiUtility.GetDefaultApiError("Id is a required property.")));
            }

            ValidateSectionCrosslist(sectionCrosslist);

            try
            {
                //call import extend method that needs the extracted extension data and the config
                await _sectionCrosslistService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCrosslistService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                //create the section crosslist
                var sectionCrosslistCreate = await _sectionCrosslistService.CreateDataModelSectionCrosslistsAsync(sectionCrosslist);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { sectionCrosslistCreate.Id }));

                return sectionCrosslistCreate;
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
        /// Update (PUT) an existing SectionCrosslist
        /// </summary>
        /// <param name="id">GUID of the SectionCrosslist to update</param>
        /// <param name="sectionCrosslist">DTO of the updated SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.SectionCrosslist> PutDataModelSectionCrosslistsAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.SectionCrosslist sectionCrosslist)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (sectionCrosslist == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null sectioncrosslist argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(sectionCrosslist.Id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                   IntegrationApiUtility.GetDefaultApiError("The id must be specified in the request body.")));
            }
            else if (id.ToLowerInvariant() != sectionCrosslist.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //call import extend method that needs the extracted extension dataa and the config
                await _sectionCrosslistService.ImportExtendedEthosData(await ExtractExtendedData(await _sectionCrosslistService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));
                
                //get Data Privacy List
                var dpList = await _sectionCrosslistService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //do update with partial logic
                var sectionCrosslistReturn = await _sectionCrosslistService.UpdateDataModelSectionCrosslistsAsync(
                    await PerformPartialPayloadMerge(sectionCrosslist, async () => await _sectionCrosslistService.GetDataModelSectionCrosslistsByGuidAsync(id),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(dpList,
                    await _sectionCrosslistService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { id }));

                return sectionCrosslistReturn; 
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
        /// Delete (DELETE) a SectionCrosslist
        /// </summary>
        /// <param name="id">GUID to desired SectionCrosslist</param>
        /// <returns>A section object <see cref="Dtos.SectionCrosslist"/> in DataModel format</returns>
        [HttpDelete]
        public async Task DeleteDataModelSectionCrosslistsByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                await _sectionCrosslistService.DeleteDataModelSectionCrosslistsByGuidAsync(id);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Validates the data in the SectionCrosslist object
        /// </summary>
        /// <param name="sectionCrosslist">SectoinCrosslist from the request</param>
        private void ValidateSectionCrosslist(SectionCrosslist sectionCrosslist)
        {
            if (sectionCrosslist.Sections == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null sections argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list is required.")));
            }

            if (sectionCrosslist.Sections.Count <= 1)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Missing sections argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list must have at least two sections.")));
            }

            if (sectionCrosslist.Sections.Count > sectionCrosslist.Sections.Select(s => s.Section.Id).ToList().Distinct().ToList().Count)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Repeating section ids",
                   IntegrationApiUtility.GetDefaultApiError("The sections list must contain unique sections, section ids cannot repeat.")));
            }

            if (!sectionCrosslist.Sections.Any(s => s.Type == SectionTypeForCrosslist.Primary))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Missing primary section argument",
                    IntegrationApiUtility.GetDefaultApiError("The sections list must have at least one section marked as primary.")));
            }

            if (sectionCrosslist.Sections.Where(s => s.Type == SectionTypeForCrosslist.Primary).ToList().Count > 1)
            {
                throw CreateHttpResponseException(new IntegrationApiException("To many primary sections",
                    IntegrationApiUtility.GetDefaultApiError("The sections list may only have one section marked as primary.")));
            }
        }
    }
}
