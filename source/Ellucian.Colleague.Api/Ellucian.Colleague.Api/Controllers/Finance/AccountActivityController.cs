// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Coordination.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Finance;

namespace Ellucian.Colleague.Api.Controllers.Finance
{
    /// <summary>
    /// Provides access to get student financial account activity.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Finance)]
    public class AccountActivityController : BaseCompressedApiController
    {
        private readonly IAccountActivityService _service;
        private readonly ILogger _logger;

        /// <summary>
        /// AccountActivityController class constructor
        /// </summary>
        /// <param name="service">Service of type <see cref="IAccountActivityService">IAccountActivityService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AccountActivityController(IAccountActivityService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the account period data for a student.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">Student ID</param>
        /// <returns>The student's <see cref="AccountActivityPeriods">account activity period</see> data</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public AccountActivityPeriods GetAccountActivityPeriodsForStudent(string studentId)
        {
            try
            {
                return _service.GetAccountActivityPeriodsForStudent(studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> data for the specified student and term.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.8, use GetAccountActivityByTermForStudent2 instead")]      
        public DetailedAccountPeriod GetAccountActivityByTermForStudent(string termId, string studentId)
        {
            try
            {
                return _service.GetAccountActivityByTermForStudent(termId, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified term.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="termId">Term ID</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> for the specified student and term.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public DetailedAccountPeriod GetAccountActivityByTermForStudent2(string termId, string studentId)
        {
            try
            {
                return _service.GetAccountActivityByTermForStudent2(termId, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified period.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="arguments">The <see cref="AccountActivityPeriodArguments">AccountActivityPeriodArguments</see> for the desired period</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">detailed account period</see> data for the specified student and period.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        [Obsolete("Obsolete as of API version 1.8, use PostAccountActivityByPeriodForStudent2 instead")]      
        public DetailedAccountPeriod PostAccountActivityByPeriodForStudent(AccountActivityPeriodArguments arguments, [FromUri]string studentId)
        {
            try
            {
                return _service.PostAccountActivityByPeriodForStudent(arguments.AssociatedPeriods, arguments.StartDate, arguments.EndDate, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Retrieves the account activity data for a student for a specified period.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="arguments">The <see cref="AccountActivityPeriodArguments">AccountActivityPeriodArguments</see> for the desired period</param>
        /// <param name="studentId">Student ID</param>
        /// <returns>The <see cref="DetailedAccountPeriod">Detailed Account Period</see> for the specified student and period.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this information</exception>
        public DetailedAccountPeriod PostAccountActivityByPeriodForStudent2(AccountActivityPeriodArguments arguments, [FromUri]string studentId)
        {
            try
            {
                return _service.PostAccountActivityByPeriodForStudent2(arguments.AssociatedPeriods, arguments.StartDate, arguments.EndDate, studentId);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Gets student award disbursement information for the specified award for the specified year
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">student id</param>
        /// <param name="awardYear">award year code</param>
        /// <param name="awardId">award id</param>
        /// <returns>StudentAwardDisbursementInfo DTO</returns>
        public async Task<StudentAwardDisbursementInfo> GetStudentAwardDisbursementInfoAsync([FromUri]string studentId, [FromUri]string awardYear, [FromUri]string awardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId is required");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw CreateHttpResponseException("awardYearCode is required");
            }
            if (string.IsNullOrEmpty(awardId))
            {
                throw CreateHttpResponseException("awardId is required");
            }
            try
            {
                return await _service.GetStudentAwardDisbursementInfoAsync(studentId, awardYear, awardId);
            }
            catch(ArgumentNullException ane)
            {
                _logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("One of the provided arguments is invalid. See log for details");
            }
            catch(PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to retrieve disbursement data. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch(ApplicationException ae)
            {
                _logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while retrieving disbursement info. See log for details");
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.Error(knfe, knfe.Message);
                throw CreateHttpResponseException("Could not locate requested disbursement data. See log for details", System.Net.HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unknown error occurred while retrieving disbursement info. See log for more details.");
            }
        }

        /// <summary>
        /// Returns information about potentially untransmitted D7 financial aid, based on
        /// current charges, credits, and awarded aid.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.STUDENT.ACCOUNT.ACTIVITY
        /// permission or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="criteria">The <see cref="PotentialD7FinancialAidCriteria"/> criteria of
        /// potential financial aid for which to search.</param>
        /// <returns>Enumeration of <see cref="Dtos.Finance.AccountActivity.PotentialD7FinancialAid"/> 
        /// awards and potential award amounts.</returns>
        /// 
        [HttpPost]
        public async Task<IEnumerable<PotentialD7FinancialAid>> QueryStudentPotentialD7FinancialAidAsync([FromBody]PotentialD7FinancialAidCriteria criteria)
        {
            if (criteria == null)
            {
                throw CreateHttpResponseException("criteria cannot be null");
            }

            try
            {
                return await _service.GetPotentialD7FinancialAidAsync(criteria);
            }
            catch (ArgumentNullException ane)
            {
                _logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("One of the provided arguments is invalid. See log for details");
            }
            catch (PermissionsException pe)
            {
                _logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("Permission denied to retrieve finacial aid data. See log for details", System.Net.HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ae)
            {
                _logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while retrieving financial aid info. See log for details");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unknown error occurred while retrieving financial info. See log for more details.");
            }
        }
    }

}