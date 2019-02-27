// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// This is the controller for GL cost centers.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class CostCentersController : BaseCompressedApiController
    {
        private readonly ICostCenterService costCenterService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GL cost center object.
        /// </summary>
        /// <param name="costCenterService">GL cost center service object</param>
        /// <param name="logger">Logger object</param>
        public CostCentersController(ICostCenterService costCenterService, ILogger logger)
        {
            this.costCenterService = costCenterService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all the GL cost centers assigned to the user for the specific fiscal year.
        /// If no fiscal year is passed in as an argument, it obtains data 
        /// for the fiscal year for today's date.
        /// Only expense GL accounts are considered.
        /// </summary>
        /// <param name="fiscalYear">General Ledger fiscal year. Optional</param>
        /// <returns>List of GL cost center DTOs for the specified fiscal year.</returns>
        /// <accessComments>
        /// The user can only access those cost centers for which they have
        /// GL account security access granted.
        /// </accessComments>
        public async Task<IEnumerable<CostCenter>> GetAsync(string fiscalYear)
        {
            try
            {
                return await costCenterService.GetAsync(fiscalYear);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get cost centers.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the GL cost center the user selected.
        /// If no fiscal year is passed in as an argument, it obtains data 
        /// for the fiscal year for today's date.         
        /// </summary>
        /// <param name="costCenterId">Selected cost center ID.</param>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <returns>Cost Center DTO.</returns>
        /// <accessComments>
        /// The user can only access those cost centers for which they have
        /// GL account security access granted.
        /// </accessComments>
        public async Task<CostCenter> GetCostCenterAsync(string costCenterId, string fiscalYear)
        {
            try
            {
                return await costCenterService.GetCostCenterAsync(costCenterId, fiscalYear);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to get the cost center.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the filtered cost centers
        /// </summary>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <returns>Cost centers that match the filter criteria.</returns>
        /// <accessComments>
        /// The user can only access those cost centers for which they have
        /// GL account security access granted.
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<CostCenter>> QueryCostCentersByPostAsync([FromBody]CostCenterQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "The query criteria must be specified.");
                }

                if (criteria.Ids != null && criteria.Ids.Count > 1)
                {
                    throw new ArgumentException("Only 0 or 1 cost center IDs may be specified.");
                }

                return await costCenterService.QueryCostCentersAsync(criteria);
            }
            catch (ArgumentNullException anex)
            {
                logger.Error(anex, anex.Message);
                throw CreateHttpResponseException("Invalid argument.", HttpStatusCode.BadRequest);
            }
            // Application exceptions will be caught below.
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unable to get cost centers", HttpStatusCode.BadRequest);
            }
        }
    }
}
