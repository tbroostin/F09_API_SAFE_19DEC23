/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes PersonPosition data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonPositionsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonPositionService personPositionService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personPositionService"></param>
        public PersonPositionsController(ILogger logger, IPersonPositionService personPositionService)
        {
            this.logger = logger;
            this.personPositionService = personPositionService;
        }

        /// <summary>
        /// Get PersonPosition objects. This endpoint returns objects based on the current
        /// user's permissions.
        /// Example: If the current user/user who has proxy is an employee, this endpoint returns that employee's/proxy's PersonPositions
        /// Example: If the current user/user who has proxy is a manager, this endpoint returns all the PersonPositions of the employees reporting to the manager
        /// Example: If the current user is an admin, this endpoint returns the PersonPositions for the effectivePersonId
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns>A list of PersonPosition objects</returns>
        public async Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null, DateTime? lookupStartDate = null)
        {
            try
            {
                return await personPositionService.GetPersonPositionsAsync(effectivePersonId, lookupStartDate);
            }
            catch(Exception e)
            {
                logger.Error(e, "Unknown error getting person positions");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}