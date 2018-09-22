//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller class for FinancialAidCounselors
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FinancialAidCounselorsController : BaseCompressedApiController
    {
        private readonly IFinancialAidCounselorService financialAidCounselorService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for FinancialAidCounselorsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="financialAidCounselorService">FinancialAidCounselorService</param>
        /// <param name="logger">Logger</param>
        public FinancialAidCounselorsController(IAdapterRegistry adapterRegistry, IFinancialAidCounselorService financialAidCounselorService, ILogger logger)
        {
            this.financialAidCounselorService = financialAidCounselorService;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Get a FinancialAidCounselor object for the given counselorId
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="counselorId">Colleague PERSON id of the counselor to get</param>
        /// <returns>FinancialAidCounselor object</returns>
        /// <exception cref="HttpResponseException">400, Thrown if the counselor id is null or empty, or if some unknown error occurs</exception>
        /// <exception cref="HttpResponseException">403, Thrown if the access to the counselor resource is forbidden</exception>
        /// <exception cref="HttpResponseException">404, Thrown if the counselor with the given id cannot be found or is not an active staff member</exception>
        public FinancialAidCounselor GetCounselor(string counselorId)
        {
            if (string.IsNullOrEmpty(counselorId))
            {
                throw CreateHttpResponseException("counselorId cannot be null");
            }
            try
            {
                return financialAidCounselorService.GetCounselor(counselorId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to FinancialAidCounselor forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("FinancialAidCounselor", counselorId);
            }
            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateNotFoundException("FinancialAidCounselor", counselorId);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting FinancialAidCounselor resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get FinancialAidCounselor DTOs list for given counselor ids.
        /// If a specified record is not found to be a valid staff type, that does not cause an exception, instead,
        /// item is not returned in a list
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <param name="criteria">Query criteria</param>
        /// <returns>List of FinancialAidCounselor DTOs</returns>
        [HttpPost]
        public async Task<IEnumerable<FinancialAidCounselor>> QueryFinancialAidCounselorsAsync([FromBody]FinancialAidCounselorQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }
            try
            {
                return await financialAidCounselorService.GetCounselorsByIdAsync(criteria.FinancialAidCounselorIds);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex, pex.Message);
                throw CreateHttpResponseException(pex.Message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message);
            }
            
        }
    }
}