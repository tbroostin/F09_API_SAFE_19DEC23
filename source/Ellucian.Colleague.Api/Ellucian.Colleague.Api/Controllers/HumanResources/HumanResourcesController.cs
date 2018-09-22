/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{

    /// <summary>
    /// Expose Human Resources Employment Positions data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class HumanResourcesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IHumanResourceDemographicsService humanResourceDemographicsService;

        /// <summary>
        /// HumanResourcesController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="humanResourceDemographicsService"></param>
        public HumanResourcesController(ILogger logger, IAdapterRegistry adapterRegistry, IHumanResourceDemographicsService humanResourceDemographicsService)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.humanResourceDemographicsService = humanResourceDemographicsService;
        }

        /// <summary>
        /// Gets a list filled with all of the HumanResourceDemographics the current user/user with proxy is able to access
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective person Id</param>
        /// <returns></returns>
        ///<accessComments>
        /// 1. Any user can get a human-resources resource that represents their self.
        /// 2. Users who have the permission - ACCEPT.REJECT.TIME.ENTRY - are considered supervisors and can get resources owned by their
        /// supervisees. If a supervisor user attempts to get a resource owned by a non-supervisee, this endpoint will throw a 403.
        /// 3. Users who are proxying for a supervisor (a user with the aforementioned permission) have the same authorization as the
        /// supervisor. When this is the case, set the effectivePersonId argument in the route url to the id of the supervisor.
        /// </accessComments>         
        [HttpGet]
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographicsAsync(string effectivePersonId = null)
        {
            try
            {
                return await humanResourceDemographicsService.GetHumanResourceDemographicsAsync(effectivePersonId);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetHumanResourceDemographicsAsync";
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
        /// Gets a list filled with all of the HumanResourceDemographics the current user/user with proxy is able to access
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective person Id</param>
        /// <returns></returns>
        ///<accessComments>
        /// 1. Any user can get a human-resources resource that represents their self.
        /// 2. Users who have the permission - ACCEPT.REJECT.TIME.ENTRY - are considered supervisors and can get resources owned by their
        /// supervisees. If a supervisor user attempts to get a resource owned by a non-supervisee, this endpoint will throw a 403.
        /// 3. Users who are proxying for a supervisor (a user with the aforementioned permission) have the same authorization as the
        /// supervisor. When this is the case, set the effectivePersonId argument in the route url to the id of the supervisor.
        /// </accessComments>         
        [HttpGet]
        public async Task<IEnumerable<HumanResourceDemographics>> GetHumanResourceDemographics2Async(string effectivePersonId = null)
        {
            try
            {
                return await humanResourceDemographicsService.GetHumanResourceDemographics2Async(effectivePersonId);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to use GetHumanResourceDemographics2Async";
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
        /// Gets a HumanResourceDemographics object for the person id in the route.
        /// </summary>
        /// <returns></returns>
        ///  <accessComments>
        /// 1. Any user can get a human-resources resource that represents their self.
        /// 2. Users who have the permission - ACCEPT.REJECT.TIME.ENTRY - are considered supervisors and can get resources owned by their
        /// supervisees. If a supervisor user attempts to get a resource owned by a non-supervisee, this endpoint will throw a 403.
        /// 3. This endpoint does not support proxy access. Use <code>GET /human-resources</code> instead.
        /// </accessComments> 
        [HttpGet]
        public async Task<HumanResourceDemographics> GetSpecificHumanResourceDemographicsAsync(string id)
        {
            try
            {
                return await humanResourceDemographicsService.GetSpecificHumanResourceDemographicsAsync(id);
            }
            catch (PermissionsException pe)
            {
                var message = "You do not have permission to GetHumanResourceDemographicsAsync";
                logger.Error(pe, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (ArgumentNullException ane)
            {
                
                logger.Error(ane, "Some argument is null and is expected not to be");
                throw CreateHttpResponseException(ane.Message, HttpStatusCode.BadRequest);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, string.Format("id {0} cannot be found in HumanResource demographics", id));
                throw CreateNotFoundException("HumanResourceDemographics", id);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }


        /// <summary>
        /// Gets a collection of HumanResourceDemographics using a collectiong of Ids
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria</param>
        /// <param name="effectivePersonId">(Optional) ID of a grantor if the current user is acting as their proxy - must be their proxy</param>
        /// <returns>The response value is a list of Person DTOs for the matching set of employees.</returns>
        /// <accessComments>
        /// Users may only access
        /// 1. Their own information
        /// 2. Their supervisees information if they have the APPROVE.REJECT.TIME.ENTRY permission
        /// 3. Their grantor's supervisees information when acting as a proxy for the grantor
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<HumanResourceDemographics>> QueryHumanResourceDemographicsAsync([FromBody] Dtos.Base.HumanResourceDemographicsQueryCriteria criteria, string effectivePersonId = null)
        {
            try
            {
                return await humanResourceDemographicsService.QueryHumanResourceDemographicsAsync(criteria, effectivePersonId);
            }
            catch (PermissionsException pe)
            {
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}