//Copyright 2015-2018 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Controller for StudentDefaultAwardPeriod
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentDefaultAwardPeriodController : BaseCompressedApiController
    {
        private readonly IStudentDefaultAwardPeriodService StudentDefaultAwardPeriodService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Dependency Injection constructor for StudentDefaultAwardPeriodController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentDefaultAwardPeriodService"></param>
        /// <param name="logger"></param>
        public StudentDefaultAwardPeriodController(IAdapterRegistry adapterRegistry, IStudentDefaultAwardPeriodService studentDefaultAwardPeriodService, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            StudentDefaultAwardPeriodService = studentDefaultAwardPeriodService;
            Logger = logger;
        }
        /// <summary>
        /// Call service to get the default award periods
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">student id for whom to retrieve default award periods</param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentDefaultAwardPeriod>> GetStudentDefaultAwardPeriodsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await StudentDefaultAwardPeriodService.GetStudentDefaultAwardPeriodsAsync(studentId);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to StudentDefaultAwardPeriod resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("StudentDefaultAwardPeriods", studentId);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentDefaultAwardPeriod resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}