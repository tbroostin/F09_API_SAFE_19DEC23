/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    ///  Provides access to Employee Compensation API(s)
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeeCompensationController : BaseCompressedApiController
    {

        private readonly IEmployeeCompensationService employeeCompensationService;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the EmployeeCompensationController class.
        /// </summary>
        /// <param name="employeeCompensationService">Service of type <see cref="IEmployeeCompensationService">IEmployeeCompensationService</see></param>
        /// <param name="logger">IEmployeeCompensationService</param>
        public EmployeeCompensationController(IEmployeeCompensationService employeeCompensationService, ILogger logger)
        {
            this.employeeCompensationService = employeeCompensationService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns Employee Compensation Details 
        /// </summary>
        /// <param name="effectivePersonId">EmployeeId of a user used for retrieving compensation details </param>
        /// <param name="salaryAmount">Estimated Annual Salary amount
        /// If this value is provided,it will be used in computing compensation details in Total Compensation Colleague Transaction.
        /// When not provided, the salary amount will be computed in Total Compensation Colleague Transaction
        /// </param>
        /// <returns>Employee Compensation DTO containing Compensation Details(Benefit-Deductions,Taxes and Stipends).<see cref="Dtos.HumanResources.EmployeeCompensation"></see> </returns>
        /// <accessComments>
        /// Any authenticated user can
        /// 1) view their own compensation information; 
        /// 2) view other employee's compensation information upon having admin access (i.e. VIEW.ALL.TOTAL.COMPENSATION permission)
        /// </accessComments>
        [HttpGet]
        public async Task<EmployeeCompensation> GetEmployeeCompensationAsync(string effectivePersonId = null, decimal? salaryAmount = null)
        {
            
            try
            {
                return await employeeCompensationService.GetEmployeeCompensationAsync(effectivePersonId, salaryAmount);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (RepositoryException re)
            {
                var message = re.Message;
                logger.Error(re, message);
                throw CreateHttpResponseException(message);
            }
            catch (Exception e)
            {
                var message = "Something unexpected occured.Unable to fetch Employee Compensation Information";
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(message);
            }
        }
    }
}