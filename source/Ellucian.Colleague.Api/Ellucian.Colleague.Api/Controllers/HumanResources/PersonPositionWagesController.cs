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
    /// PersonPositionWages Controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonPositionWagesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonPositionWageService personPositionWageService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personPositionWageService"></param>
        public PersonPositionWagesController(ILogger logger, IPersonPositionWageService personPositionWageService)
        {
            this.logger = logger;
            this.personPositionWageService = personPositionWageService;
        }

        /// <summary>
        /// Gets PersonPositionWage objects based on the authenticated user's permissions. 
        /// </summary> 
        /// <accessComments>
        /// This endpoint returns PersonPositionWage objects based on the current authenticated user's permissions.
        /// Example: If the current user is an admin, this endpoint returns the PersonPositionWages for the effectivePersonId.
        /// Example: If the current user/user who has proxy is an employee, this endpoint returns that employee's/proxy user's PersonPositionWages.
        /// Example: If the current user/user who has proxy is a supervisor with the permission ACCEPT.REJECT.TIME.ENTRY, this endpoint returns the supervisor's PersonPositionWages 
        /// and PersonPositionWages of all the employees reporting to this supervisor.
        /// Example: If the current user is a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, this end point returns the leave approver's PersonPositionWages
        /// and PersonPositionWages of all the employees whose leave requests are handled by this leave approver.
        /// </accessComments>
        /// <param name="effectivePersonId">Optional parameter for effective personId, which should be used when proxying on behalf of another user.</param>
        /// <returns>A collection of PersonPositionWage objects</returns>
        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(string effectivePersonId = null)
        {
            try
            {
                return await personPositionWageService.GetPersonPositionWagesAsync(effectivePersonId);
            }
            catch(Exception e)
            {
                logger.Error(e, "Unknown error getting person position wages");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}