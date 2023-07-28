//Copyright 2014-2023 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// The StudentAwardYearsController exposes a student's financial aid years
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAwardYearsController : BaseCompressedApiController
    {
        private readonly IStudentAwardYearService StudentAwardYearService;
        private readonly IAdapterRegistry AdapterRegistry;
        private readonly ILogger Logger;

        /// <summary>
        /// Dependency Injection constructor for StudentAwardYearsController
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="studentAwardYearService"></param>
        /// <param name="logger"></param>
        public StudentAwardYearsController(IAdapterRegistry adapterRegistry, IStudentAwardYearService studentAwardYearService, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            StudentAwardYearService = studentAwardYearService;
            Logger = logger;
        }

        /// <summary>
        /// Get all of a student's financial aid years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Id of the student for whom to get award years</param>
        /// <returns>A list of StudentAwardYear objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        public IEnumerable<StudentAwardYear> GetStudentAwardYears(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return StudentAwardYearService.GetStudentAwardYears(studentId);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all of a student's financial aid years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Id of the student for whom to get award years</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentAwardYear2 objects</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId argument is null or empty</exception>
        public async Task<IEnumerable<StudentAwardYear2>> GetStudentAwardYears2Async(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await StudentAwardYearService.GetStudentAwardYears2Async(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the specified financial aid award year for the student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to get award year data</param>
        /// <param name="awardYear">award year code for which to retrieve award year data</param>
        /// <returns>StudentAwardYear object</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId or awardYearCode argument is null or empty</exception>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpGet]
        public StudentAwardYear GetStudentAwardYear(string studentId, string awardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYearCode cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return StudentAwardYearService.GetStudentAwardYear(studentId, awardYear);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("StudentAwardYears", studentId);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the specified financial aid award year for the student
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have 
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions
        /// can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id for whom to get award year data</param>
        /// <param name="awardYear">award year code for which to retrieve award year data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>StudentAwardYear2 object</returns>
        /// <exception cref="HttpResponseException">Thrown if the studentId or awardYearCode argument is null or empty</exception>
        [HttpGet]
        [EthosEnabledFilter(typeof(IEthosApiBuilderService))]
        public async Task<StudentAwardYear2> GetStudentAwardYear2Async(string studentId, string awardYear, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYearCode cannot be null or empty", System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return await StudentAwardYearService.GetStudentAwardYear2Async(studentId, awardYear, getActiveYearsOnly);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to StudentAwardYears resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException knfe)
            {
                Logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("StudentAwardYears", awardYear);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYears resource. See log for details.", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates the student award year. Currently only the IsPaperCopyOptionSelected property is updated
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>student award year</returns>
        [Obsolete("Obsolete as of Api version 1.8, use version 2 of this API")]
        [HttpPut]
        public StudentAwardYear UpdateStudentAwardYear([FromUri] string studentId, [FromBody] StudentAwardYear studentAwardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (studentAwardYear == null)
            {
                throw CreateHttpResponseException("studentAwardYear cannot be null");
            }

            try
            {
                return StudentAwardYearService.UpdateStudentAwardYear(studentAwardYear);
            }
            catch (ArgumentNullException ne)
            {
                Logger.Error(ne, ne.Message);
                throw CreateHttpResponseException("studentAwardYear in request body is null. See log for details.");
            }
            catch (ArgumentException ae)
            {
                Logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("studentAwardYear in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student award letter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (OperationCanceledException oce)
            {
                Logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("PaperCopyOptionFlag Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYear resource. See log for details.");
            }
        }

        /// <summary>
        /// Updates the student award year. Currently only the IsPaperCopyOptionSelected property is updated
        /// </summary>
        /// <accessComments>
        /// Users may make changes to their own data only
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="studentAwardYear">student award year carrying the info</param>
        /// <returns>StudentAwardYear2 object</returns>
        [HttpPut]
        public async Task<StudentAwardYear2> UpdateStudentAwardYear2Async([FromUri] string studentId, [FromBody] StudentAwardYear2 studentAwardYear)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId cannot be null or empty");
            }
            if (studentAwardYear == null)
            {
                throw CreateHttpResponseException("studentAwardYear cannot be null");
            }

            try
            {
                return await StudentAwardYearService.UpdateStudentAwardYear2Async(studentAwardYear);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw CreateHttpResponseException(csee.Message, HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException ne)
            {
                Logger.Error(ne, ne.Message);
                throw CreateHttpResponseException("studentAwardYear in request body is null. See log for details.");
            }
            catch (ArgumentException ae)
            {
                Logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("studentAwardYear in request body contains invalid attribute values. See log for details.");
            }
            catch (PermissionsException pe)
            {
                Logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Access to student award letter resource is forbidden. See log for details.", System.Net.HttpStatusCode.Forbidden);
            }
            catch (OperationCanceledException oce)
            {
                Logger.Error(oce, oce.Message);
                throw CreateHttpResponseException("PaperCopyOptionFlag Update request was canceled because of a conflict on the server. See log for details.", System.Net.HttpStatusCode.Conflict);
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unknown error occurred getting StudentAwardYear resource. See log for details.");
            }
        }

    }
}
