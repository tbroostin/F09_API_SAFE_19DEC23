//Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
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
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The StudentLoanLimitationsController exposes a student's loan limits, which describe the parameters within which a student can request changes to their loans.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentLoanLimitationsController : BaseCompressedApiController
    {
        private readonly IStudentLoanLimitationService StudentLoanLimitationService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Dependency Injection Constructor for StudentLoanLimitationsController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="studentLoanLimitationService">StudentLoanLimitationService object</param>
        /// <param name="logger">Logger object</param>
        public StudentLoanLimitationsController(IAdapterRegistry adapterRegistry, IStudentLoanLimitationService studentLoanLimitationService, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            StudentLoanLimitationService = studentLoanLimitationService;
            Logger = logger;
        }

        /// <summary>
        /// Returns a list of StudentLoanLimitation objects for all the years a student has award data.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">Student id for whom to retrieve the loan limitations</param>
        /// <returns>A list of StudentLoanLimitation objects for all the years a student has award data.</returns>       
        public async Task<IEnumerable<StudentLoanLimitation>> GetStudentLoanLimitationsAsync(string studentId)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var loanLimits = await StudentLoanLimitationService.GetStudentLoanLimitationsAsync(studentId);
                stopWatch.Stop();
                Logger.Info(string.Format("Time elapsed to GetStudentLoanLimitations(controller): {0}", stopWatch.ElapsedMilliseconds));
                return loanLimits;
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to loan limitations resource forbidden. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find loan limitations resource. See log for details", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting loan limitations resource. See log for details", System.Net.HttpStatusCode.BadRequest);
            }            
        }
    }
}