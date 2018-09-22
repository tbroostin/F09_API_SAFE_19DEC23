/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
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
    /// Exposes Financial Aid Budget Components assigned to students
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidBudgetComponentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _AdapterRegistry;
        private readonly IStudentBudgetComponentService _StudentBudgetComponentService;
        private readonly ILogger _Logger;

        /// <summary>
        /// StudentBudgetComponentsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentBudgetComponentService">StudentBudgetComponentService</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentFinancialAidBudgetComponentsController(IAdapterRegistry adapterRegistry, IStudentBudgetComponentService studentBudgetComponentService, ILogger logger)
        {
            _AdapterRegistry = adapterRegistry;
            _StudentBudgetComponentService = studentBudgetComponentService;
            _Logger = logger;
        }

        /// <summary>
        /// Get a student's Financial Aid Budget Components for all award years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get budget components</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentBudgetComponent DTOs</returns>
        [HttpGet]
        public async Task<IEnumerable<StudentBudgetComponent>> GetStudentFinancialAidBudgetComponentsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId argument is required");
            }

            try
            {
                return await _StudentBudgetComponentService.GetStudentBudgetComponentsAsync(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get budget component resources for student {0}", studentId);
                _Logger.Error(pex, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = "Unknown error occurred getting student budget components";
                _Logger.Error(ex, message);
                throw CreateHttpResponseException(message);
            }
        }


    }
}