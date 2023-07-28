/* Copyright 2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes Organizational Chart data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class OrganizationalChartController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IOrganizationalChartService organizationalChartService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="organizationalChartService"></param>
        public OrganizationalChartController(ILogger logger, IOrganizationalChartService organizationalChartService)
        {
            this.logger = logger;
            this.organizationalChartService = organizationalChartService;
        }

        /// <summary>
        /// Gets a list of employees for the organizational chart.
        /// </summary>
        /// <param name="rootEmployeeId">The employee id of the root employee to build the org chat off of</param>
        /// <returns>A list of <see cref="OrgChartEmployee"> objects.</see></returns>
        /// <accessComments>
        /// </accessComments>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Builds an Organizational Chart",
            HttpMethodDescription = "Gets employee information to be used in building the organizational chart.")]
        public async Task<IEnumerable<OrgChartEmployee>> GetOrganizationalChartAsync(string rootEmployeeId)
        {
            try
            {
                if (String.IsNullOrEmpty(rootEmployeeId)) throw new Exception("Please provide the Employee Id for the org-chart.");
                return await organizationalChartService.GetOrganizationalChartAsync(rootEmployeeId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetOrganizationalChartAsync - {0}", pe.Message);
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a single employee for the org chart.
        /// </summary>
        /// <param name="rootEmployeeId">The employee id of the root employee to build the org chat off of</param>
        /// <returns>A single <see cref="OrgChartEmployee"> object.</see></returns>
        /// <accessComments>
        /// </accessComments>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Builds a single employee in a Organizational Chart",
            HttpMethodDescription = "Gets employee information to be used in building the organizational chart.")]
        public async Task<OrgChartEmployee> GetOrganizationalChartEmployeeAsync(string rootEmployeeId)
        {
            try
            {
                if (String.IsNullOrEmpty(rootEmployeeId)) throw new Exception("Please provide the Employee Id for the org-chart.");
                return await organizationalChartService.GetOrganizationalChartEmployeeAsync(rootEmployeeId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = string.Format("You do not have permission to GetOrganizationalChartAsync - {0}", pe.Message);
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets a list of employees matching the given search criteria.
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria.</param>
        /// <returns>A list of <see cref="EmployeeSearchResult"> objects.</see></returns>
        /// <accessComments>
        /// Only the users with VIEW.ORG.CHART permission can query employee names.
        /// </accessComments>
        [HttpPost, PermissionsFilter(HumanResourcesPermissionCodes.ViewOrgChart)]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets a list of employees matching the given search criteria.",
            HttpMethodDescription = "Gets a list of employees matching the given search criteria.", HttpMethodPermission = "VIEW.ORG.CHART")]
        public async Task<IEnumerable<EmployeeSearchResult>> QueryEmployeesByPostAsync([ModelBinder(typeof(EedmModelBinder))] EmployeeNameQueryCriteria criteria)
        {
            try
            {
                if (criteria == null) throw new ArgumentNullException("criteria", "Search criteria cannot be null.");
                organizationalChartService.ValidatePermissions(GetPermissionsMetaData());
                return await organizationalChartService.QueryEmployeesByPostAsync(criteria);
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException(ane.Message, HttpStatusCode.BadRequest);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to access QueryEmployeesByPostAsync";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}