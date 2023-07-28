﻿/*Copyright 2018-2023 Ellucian Company L.P. and its affiliates.*/

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http;
using System.Linq;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using System.Collections;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Attributes;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Provides access to Leave balance configuration items
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class LeaveBalanceConfigurationController: BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly ILeaveBalanceConfigurationService leaveBalanceConfigurationService;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string unexpectedGenericErrorMessage = "Unexpected error occurred while processing the request.";

        /// <summary>
        /// Constructor
        /// </summary>
        public LeaveBalanceConfigurationController(ILeaveBalanceConfigurationService leaveBalanceConfigurationService, ILogger logger)
        {
            this.leaveBalanceConfigurationService = leaveBalanceConfigurationService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets the configurations for leave balance
        /// </summary>
        /// <returns>LeaveBalanceConfiguration</returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        [Metadata(ApiVersionStatus = "R", HttpMethodSummary = "Gets access to Leave Balance Configuration object for the currently authenticated API user (as an employee.leave approver,supervisor).",
          HttpMethodDescription = "Gets access to Leave Balance Configuration object for the currently authenticated API user (as an employee.leave approver,supervisor).")]
        public async Task<LeaveBalanceConfiguration> GetLeaveBalanceConfigurationAsync()
        {
            try
            {
                return await leaveBalanceConfigurationService.GetLeaveBalanceConfigurationAsync();
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }

            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                throw CreateHttpResponseException(unexpectedGenericErrorMessage, HttpStatusCode.InternalServerError);
            }            
        }
    }
}