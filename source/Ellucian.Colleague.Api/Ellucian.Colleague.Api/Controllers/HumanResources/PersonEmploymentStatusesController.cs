/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    /// Exposes personemploymentstatus data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PersonEmploymentStatusesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IPersonEmploymentStatusService personEmploymentStatusService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="personEmploymentStatusService"></param>
        public PersonEmploymentStatusesController(ILogger logger, IPersonEmploymentStatusService personEmploymentStatusService)
        {
            this.logger = logger;
            this.personEmploymentStatusService = personEmploymentStatusService;
        }

        /// <summary>
        /// Get personEmploymentStatus objects. This endpoint returns objects based on the current
        /// user's/user with proxy's permissions.
        /// </summary>
        /// <accessComments>
        /// Example: If the current user is an admin, this endpoint returns the personEmploymentStatuses for the effectivePersonId
        /// Example: If the current user/user with proxy is an employee, this endpoint returns that employee's/proxied employee's personEmploymentStatuses
        /// Example: If the current user/user with proxy is a manager, this endpoint returns all the personEmploymentStatuses of the employees reporting to the manager
        /// Example: If the current user is a leave approver with the APPROVE.REJECT.LEAVE.REQUEST permission, this end point returns the leave approver's PersonEmploymentStatus
        /// and personEmploymentStatuses of all the employees whose leave requests are handled by this leave approver.
        ///</accessComments>
        /// <param name="effectivePersonId">Optional parameter for effective person Id</param>
        /// <returns>A list of PersonEmploymentStatus objects</returns>
        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(string effectivePersonId = null)
        {
            try
            {
                return await personEmploymentStatusService.GetPersonEmploymentStatusesAsync(effectivePersonId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting person employment statuses");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}