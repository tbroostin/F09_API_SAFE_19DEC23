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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Provide access to the StudentLoanSummary information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentLoanSummaryController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;
        private readonly IStudentLoanSummaryService studentLoanSummaryService;

        /// <summary>
        /// Constructor for StudentLoanSummary
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentLoanSummaryService">StudentLoanSummaryRepository</param>
        /// <param name="logger">Logger</param>
        public StudentLoanSummaryController(IAdapterRegistry adapterRegistry, IStudentLoanSummaryService studentLoanSummaryService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
            this.studentLoanSummaryService = studentLoanSummaryService;
        }

        /// <summary>
        /// Get a student's loan summary data
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">Student's Colleague PERSON id</param>
        /// <returns>A StudentLoanSummary object</returns>       
        public async Task<StudentLoanSummary> GetStudentLoanSummaryAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await studentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to loan summary resource forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Unable to find loan summary resource. See log for details.", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting loan summary resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}