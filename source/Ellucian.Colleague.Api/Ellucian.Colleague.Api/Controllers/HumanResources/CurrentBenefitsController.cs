/*Copyright 2019-2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
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
    ///  Provides access to Employee Compensation API(s)
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class CurrentBenefitsController: BaseCompressedApiController
    {
        private readonly ICurrentBenefitsService currentBenefitsService;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the CurrentBenefitsController class.
        /// </summary>
        /// <param name="currentBenefitsService">Service of type <see cref="ICurrentBenefitsService">ICurrentBenefitsService</see></param>
        /// <param name="logger">ILogger</param>
        public CurrentBenefitsController(ICurrentBenefitsService currentBenefitsService, ILogger logger)
        {
            this.currentBenefitsService = currentBenefitsService;
            this.logger = logger;
        }

        /// <summary>
        /// This end point returns Employee's Current Benefits.
        /// </summary>
        /// <param name="effectivePersonId">Optional parameter for effective personId.</param>
        /// <returns>Employee Benefits DTO containing Employee's current benefits details (benefits, coverage, cost, dependants, healthproviders etc).<see cref="Dtos.HumanResources.EmployeeBenefits"></see> </returns>
        /// <accessComments>
        /// Any authenticated user can view their own current benefits details.
        /// </accessComments>
        [HttpGet]
        public async Task<EmployeeBenefits> GetEmployeeCurrentBenefitsAsync(string effectivePersonId = null)
        {
            try
            {
                return await currentBenefitsService.GetEmployeesCurrentBenefitsAsync(effectivePersonId);
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("User does not have permission to view the requested current benefits information", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Something unexpected occured. Unable to fetch Employee current benefits details";
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
        }
    }
}