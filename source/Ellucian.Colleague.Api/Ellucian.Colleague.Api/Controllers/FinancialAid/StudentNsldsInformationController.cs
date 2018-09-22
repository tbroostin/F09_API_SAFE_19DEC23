/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.FinancialAid;
using System.Net;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Web.Security;


namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// StudentNsldsInformationController class
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentNsldsInformationController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IStudentNsldsInformationService studentNsldsInformationService;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">adapter registry</param>
        /// <param name="studentNsldsInformationService">student nslds information service</param>
        /// <param name="logger">logger</param>
        public StudentNsldsInformationController(IAdapterRegistry adapterRegistry, IStudentNsldsInformationService studentNsldsInformationService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentNsldsInformationService = studentNsldsInformationService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets student NSLDS related information
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve nslds information</param>
        /// <returns>StudentNsldsInformation DTO</returns>
        [HttpGet]
        public async Task<StudentNsldsInformation> GetStudentNsldsInformationAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }            
            try
            {
                return await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student NSLDS information forbidden. See log for details.", HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException(string.Format("No StudentNsldsInformation was found for student {0}", studentId), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException(string.Format("Unknown error occured while trying to retrieve StudentNsldsInformation for student {0}. See log for details", studentId));
            }
        }
    }
}