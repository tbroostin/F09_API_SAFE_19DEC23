// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to approver objects.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class ApproversController : BaseCompressedApiController
    {
        private readonly IApproverService approverService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the approver service controller.
        /// </summary>
        /// <param name="approverService">Approves service object.</param>
        /// <param name="logger">Logger object</param>
        public ApproversController(IApproverService approverService, ILogger logger)
        {
            this.approverService = approverService;
            this.logger = logger;
        }

        /// <summary>
        /// Validate an approver ID. 
        /// A next approver ID and an approver ID are the same. They are just
        /// populated under different circumstances.
        /// </summary>
        /// <param name="nextApproverId">Next approver ID.</param>
        /// <returns>A <see cref="NextApproverValidationResponse">DTO.</see>/></returns>
        /// <accessComments>
        /// Requires permission CREATE.UPDATE.BUDGET.ADJUSTMENT
        /// </accessComments>
        [HttpGet]
        public async Task<NextApproverValidationResponse> GetNextApproverValidationAsync(string nextApproverId)
        {
            try
            {
                return await approverService.ValidateApproverAsync(nextApproverId);
            }
            catch (PermissionsException peex)
            {
                logger.Error(peex.Message);
                throw CreateHttpResponseException("Insufficient permissions to validate the next approver.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw CreateHttpResponseException("Unable to validate a next approver.", HttpStatusCode.BadRequest);
            }
        }
    }
}