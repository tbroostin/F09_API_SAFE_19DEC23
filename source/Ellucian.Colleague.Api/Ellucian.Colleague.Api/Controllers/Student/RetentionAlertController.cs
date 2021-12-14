// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Net;
using System.Net.Http;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Retention Alert related data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RetentionAlertController : BaseCompressedApiController
    {
        private readonly IRetentionAlertService _retentionAlertService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the RetentionAlertController class.
        /// </summary>
        /// <param name="retentionAlertService">Service of type <see cref="IRetentionAlertService">IRetentionAlertService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RetentionAlertController(IRetentionAlertService retentionAlertService, ILogger logger)
        {
            _retentionAlertService = retentionAlertService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Case Types.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseType">Case Types</see></returns>
        public async Task<IEnumerable<CaseType>> GetCaseTypesAsync()
        {
            try
            {
                return await _retentionAlertService.GetCaseTypesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Case types", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all Case Categories.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseCategory">Case Categories</see></returns>
        public async Task<IEnumerable<CaseCategory>> GetCaseCategoriesAsync()
        {
            try
            {
                return await _retentionAlertService.GetCaseCategoriesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Case categories", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the Org Role settings for Case Categories.
        /// </summary>
        /// <param name="caseCategoryIds">Case Category Ids</param>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <returns>A list of RetentionAlertCaseCategoryOrgRoles</returns>
        [HttpPost]
        public async Task<IEnumerable<RetentionAlertCaseCategoryOrgRoles>> QueryRetentionAlertCaseCategoryOrgRolesAsync([FromBody]List<string> caseCategoryIds)
        {
            try
            {
                return await _retentionAlertService.GetRetentionAlertCaseCategoryOrgRolesAsync(caseCategoryIds);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve Case category org roles");
                throw CreateHttpResponseException("Unable to retrieve Case category org roles", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the retention alert permissions asynchronous.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns><see cref="RetentionAlertPermissions">Retention alert permissions for the current user</see></returns>
        public async Task<RetentionAlertPermissions> GetRetentionAlertPermissionsAsync()
        {
            try
            {
                return await _retentionAlertService.GetRetentionAlertPermissionsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("An error occurred while retrieving retention alert permissions.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all Case Closure reasons.
        /// </summary>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>All <see cref="CaseClosureReason">Case Categories</see></returns>
        public async Task<IEnumerable<CaseClosureReason>> GetCaseClosureReasonsAsync()
        {
            try
            {
                return await _retentionAlertService.GetCaseClosureReasonsAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Case closure Reasons", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Criteria to retrieve retention alert cases</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase">RetentionAlertWorkCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may query retention alert cases if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<RetentionAlertWorkCase>> QueryRetentionAlertWorkCasesByPostAsync([FromBody] RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            if (retentionAlertQueryCriteria == null)
            {
                string errorText = "Must provide the retentionAlertQueryCriteria item to query cases.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.GetRetentionAlertCasesAsync(retentionAlertQueryCriteria);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve Retention Alert work cases");
                throw CreateHttpResponseException("Unable to retrieve Retention Alert work cases", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert work cases 2 Async
        /// </summary>
        /// <param name="retentionAlertQueryCriteria">Criteria to retrieve retention alert cases</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase2">RetentionAlertWorkCase2</see> object</returns>
        /// <accessComments>
        /// An authenticated user may query retention alert cases if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<RetentionAlertWorkCase2>> QueryRetentionAlertWorkCasesByPost2Async([FromBody] RetentionAlertQueryCriteria retentionAlertQueryCriteria)
        {
            if (retentionAlertQueryCriteria == null)
            {
                string errorText = "Must provide the retentionAlertQueryCriteria item to query cases.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.GetRetentionAlertCases2Async(retentionAlertQueryCriteria);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve Retention Alert work cases 2");
                throw CreateHttpResponseException("Unable to retrieve Retention Alert work cases 2", HttpStatusCode.BadRequest);
            }
        }
        /// <summary>
        /// Retrieves retention alert case Detail
        /// </summary>
        /// <param name="id">Retention alert case id to retrieve case detail</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the case id is not provided to get the case detail.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert case detail.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertCaseDetail">RetentionAlertCaseDetail</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert case detail if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        public async Task<RetentionAlertCaseDetail> GetRetentionAlertCaseDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                string errorText = "Case id must be specified to get the case detail";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            try
            {
                return await _retentionAlertService.GetRetentionAlertCaseDetailAsync(id);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                _logger.Error(ex, "User is not authorized to retrieve retention alert case detail");
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve retention alert case detail");
                throw CreateHttpResponseException("Unable to retrieve retention alert case detail", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Query retention alert contributions for the current user
        /// </summary>
        /// <param name="contributionsQueryCriteria">Query criteria</param>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert contributions.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>A list of <see cref="RetentionAlertWorkCase">RetentionAlertWorkCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert contributions if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<RetentionAlertWorkCase>> QueryRetentionAlertContributionsAsync([FromBody] ContributionsQueryCriteria contributionsQueryCriteria)
        {
            if (contributionsQueryCriteria == null)
            {
                string errorText = "Must provide the contributionsQueryCriteria item to query contributions.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.GetRetentionAlertContributionsAsync(contributionsQueryCriteria);
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to get contributions");
                throw CreateHttpResponseException("User is not authorized to get contributions", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to retrieve retention alert contributions");
                throw CreateHttpResponseException("Unable to retrieve retention alert contributions", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a retention alert case for student
        /// </summary>
        /// <param name="retentionAlertCase">Information for adding a case for student</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="RetentionAlertCaseCreateResponse">add retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert contributions if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        public async Task<HttpResponseMessage> PostRetentionAlertCaseAsync([FromBody] RetentionAlertCase retentionAlertCase)
        {
            if (retentionAlertCase == null)
            {
                string errorText = "Must provide the retentionAlertCase item to add a new case for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertCaseCreateResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseAsync(retentionAlertCase);
                var response = Request.CreateResponse<RetentionAlertCaseCreateResponse>(HttpStatusCode.Created, newCaseResponse);
                SetResourceLocationHeader("PostRetentionAlertCase", new { id = newCaseResponse.CaseId });
                return response;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to add retention alert case");
                throw CreateHttpResponseException("Unable to add retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a note to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote">Information for adding a note to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">add a note to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a note to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseNoteAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a note to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseNote == null)
            {
                string errorText = "Must provide the RetentionAlertWorkCaseNote item to add a note to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseNoteAsync(caseId, retentionAlertCaseNote);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to add note to retention alert case");
                throw CreateHttpResponseException("Unable to add note to retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a FollowUp Note to a Retention Alert Case, this will not add the user to the list of Case Owners.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseNote">Information for adding a note to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">add a note to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a note to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// CONTRIBUTE.TO.CASES
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseFollowUpAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseNote retentionAlertCaseNote)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add followup to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseNote == null)
            {
                string errorText = "Must provide the RetentionAlertWorkCaseNote item to add followup to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseFollowUpAsync(caseId, retentionAlertCaseNote);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to add followup to retention alert case");
                throw CreateHttpResponseException("Unable to add followup to retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a communication code to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseCommCode">Information for adding a communication code to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">adding a communication code to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a communication code to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseCommCodeAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseCommCode retentionAlertCaseCommCode)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a communication code to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseCommCode == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to add a communication code to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseCommCodeAsync(caseId, retentionAlertCaseCommCode);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to add communication code to a retention alert case");
                throw CreateHttpResponseException("Unable to add communication code to a retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Add a Case Type to a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseType">Information for adding a case type to a case for student</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">adding a case type to a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may add a case type to a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseTypeAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseType retentionAlertCaseType)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to add a type to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseType == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to add a case type to a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.AddRetentionAlertCaseTypeAsync(caseId, retentionAlertCaseType);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to add a case type to a retention alert case");
                throw CreateHttpResponseException("Unable to add a case type to a retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Change Retention Alert Case Priority
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCasePriority">Information for changing the priority of a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">changing the priority of a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may change the priority of a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> ChangeRetentionAlertCasePriorityAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCasePriority retentionAlertCasePriority)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to change the priority of a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCasePriority == null)
            {
                string errorText = "Must provide the retentionAlertCasePriority item to change the priority of the retention alert case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.ChangeRetentionAlertCasePriorityAsync(caseId, retentionAlertCasePriority);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to change priority of the retention alert case");
                throw CreateHttpResponseException("Unable to change priority of the retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Close a Retention Alert Case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertCaseClose">Information for closing a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">closing a retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may close a retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> CloseRetentionAlertCaseAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseClose retentionAlertCaseClose)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to close a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertCaseClose == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseAction item to close a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.CloseRetentionAlertCaseAsync(caseId, retentionAlertCaseClose);
                return newCaseResponse;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to close a retention alert case");
                throw CreateHttpResponseException("Unable to close a retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates a retention alert case for student
        /// </summary>
        /// <param name="id">Id of the Retention alert case to update</param>
        /// <param name="retentionAlertCase">Information for updating a case</param>
        /// <returns><see cref="RetentionAlertCaseCreateResponse">Reponse of the case update</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [HttpPut]
        public async Task<RetentionAlertCaseCreateResponse> PutRetentionAlertCaseAsync([FromUri] string id, [FromBody] RetentionAlertCase retentionAlertCase)
        {
            if (string.IsNullOrEmpty(id))
            {
                string errorText = "Case id must be specified to update a case for student";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }

            if (retentionAlertCase == null)
            {
                string errorText = "Must provide the retentionAlertCase item to update case for student.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.UpdateRetentionAlertCaseAsync(id, retentionAlertCase);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to update retention alert case");
                throw CreateHttpResponseException("Unable to update retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of cases for each Org Role and Org Entity owning cases for that category
        /// </summary>
        /// <param name="categoryId">Retention Alert Case Category Id</param>
        /// <returns>A list of cases for each Org Role and Org Entity owning cases for that category</returns>        
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may change the priority of a retention alert case if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        public async Task<RetentionAlertGroupOfCasesSummary> GetRetentionAlertCaseOwnerSummaryAsync([FromUri] string categoryId)
        {
            if (string.IsNullOrEmpty(categoryId))
            {
                string errorText = "CategoryId must be specified to get a Category Summary.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _retentionAlertService.GetRetentionAlertCaseOwnerSummaryAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to get Category Summary for retention alert cases.");
                throw CreateHttpResponseException("Unable to get Category Summary for retention alert cases.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the case priorities
        /// </summary>
        /// <returns>All <see cref="CasePriority"/></returns>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<IEnumerable<CasePriority>> GetCasePrioritiesAsync()
        {
            try
            {
                return await _retentionAlertService.GetCasePrioritiesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to retrieve Case priorities");
                throw CreateHttpResponseException("Unable to retrieve Case priorities", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Sends the retention alert mail for the work case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseSendMail">Information for sending the mails for the work case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">send mail for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may send mails for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> SendRetentionAlertWorkCaseMailAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseSendMail retentionAlertWorkCaseSendMail)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertWorkCaseSendMail == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseSendMail item to send a mail.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse newCaseResponse = await _retentionAlertService.SendRetentionAlertWorkCaseMailAsync(caseId, retentionAlertWorkCaseSendMail);
                return newCaseResponse;
            }
            catch (PermissionsException pex)
            {                
                _logger.Error(pex, "User is not able to send email for case.");
                throw CreateHttpResponseException("User is not able to send email for case.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to send mail for retention alert case");
                throw CreateHttpResponseException("Unable to send mail for retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set a reminder for a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="reminder">Information for setting a reminder for a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">setting a reminder for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may set a reminder for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> AddRetentionAlertCaseReminderAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseSetReminder reminder)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (reminder == null)
            {
                string errorText = "Must provide the reminder item to set a reminder.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse response = await _retentionAlertService.AddRetentionAlertCaseReminderAsync(caseId, reminder);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not able to set a reminder for case.");
                throw CreateHttpResponseException("User is not able to set a reminder for case.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to set a reminder for retention alert case");
                throw CreateHttpResponseException("Unable to set a reminder for retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set a reminder for a retention alert case.
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="reminders">Information for setting a reminder for a retention alert case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating of there were any errors <see cref="RetentionAlertWorkCaseActionResponse">setting a reminder for the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>
        /// An authenticated user may set a reminder for retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> ManageRetentionAlertCaseRemindersAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseManageReminders reminders)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to send a mail.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (reminders == null)
            {
                string errorText = "Must provide the reminders to manage retention alert case reminders.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse response = await _retentionAlertService.ManageRetentionAlertCaseRemindersAsync(caseId, reminders);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not able to set a reminder for case.");
                throw CreateHttpResponseException("User is not able to set a reminder for case.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to set a reminder for retention alert case");
                throw CreateHttpResponseException("Unable to set a reminder for retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves retention alert open cases.
        /// </summary>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <returns>An <see cref="RetentionAlertOpenCase">RetentionAlertOpenCase</see> object</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert open cases if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        public async Task<IEnumerable<RetentionAlertOpenCase>> GetRetentionAlertOpenCasesAsync()
        {
            try
            {
                return await _retentionAlertService.GetRetentionAlertOpenCasesAsync();
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to get open cases.");
                throw CreateHttpResponseException("User is not authorized to get open cases.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to retrieve retention alert open cases");
                throw CreateHttpResponseException("Unable to retrieve retention alert open cases", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a list of closed retention alert cases grouped by closure reason.
        /// </summary>
        /// <param name="categoryId">Retention Alert Category Id</param>
        /// <returns>a list of closed retention alert cases grouped by closure reason.</returns>
        /// <accessComments>
        /// An authenticated user may retrieve retention alert open cases if they have one of the following permissions:
        /// WORK.ANY.CASE
        /// </accessComments>
        public async Task<IEnumerable<RetentionAlertClosedCasesByReason>> GetRetentionAlertClosedCasesByReasonAsync([FromUri] string categoryId)
        {
            try
            {
                return await _retentionAlertService.GetRetentionAlertClosedCasesByReasonAsync(categoryId);
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to get cases closed by reason.");
                throw CreateHttpResponseException("User is not authorized to get cases closed by reason.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to retrieve retention alert closed cases by reason");
                throw CreateHttpResponseException("Unable to retrieve retention alert closed cases by reason", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Reassigns the retention alert work case
        /// </summary>
        /// <param name="caseId">Case ID</param>
        /// <param name="retentionAlertWorkCaseReassign">Information for reassigning the work case</param>
        /// <returns>An RetentionAlertWorkCaseActionResponse indicating if there were any errors <see cref="RetentionAlertWorkCaseActionResponse">reassign the retention alert case</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertWorkCaseActionResponse> ReassignRetentionAlertWorkCaseAsync([FromUri] string caseId, [FromBody] RetentionAlertWorkCaseReassign retentionAlertWorkCaseReassign)
        {
            if (string.IsNullOrEmpty(caseId))
            {
                string errorText = "Must provide the caseId item to reassign a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (retentionAlertWorkCaseReassign == null)
            {
                string errorText = "Must provide the retentionAlertWorkCaseReassign item to reassign a case.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertWorkCaseActionResponse caseResponse = await _retentionAlertService.ReassignRetentionAlertWorkCaseAsync(caseId, retentionAlertWorkCaseReassign);
                return caseResponse;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to reassign case.");
                throw CreateHttpResponseException("User is not authorized to reassign case.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to reassign the retention alert case");
                throw CreateHttpResponseException("Unable to reassign the retention alert case", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Set the Send Email Preference 
        /// </summary>
        /// <param name="orgEntityId">Org Entity ID</param>
        /// <param name="sendEmailPreference">Information for setting send email preference</param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpPost]
        public async Task<RetentionAlertSendEmailPreference> SetRetentionAlertEmailPreferenceAsync([FromUri] string orgEntityId, [FromBody] RetentionAlertSendEmailPreference sendEmailPreference)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                string errorText = "Must provide the orgEntityId to set the send email preference.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            if (sendEmailPreference == null)
            {
                string errorText = "Must provide the sendEmailPreference to set the send email preference..";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertSendEmailPreference response = await _retentionAlertService.SetRetentionAlertEmailPreferenceAsync(orgEntityId, sendEmailPreference);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to set the Send Email Preference.");
                throw CreateHttpResponseException("User is not authorized to set the Send Email Preference.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to set the Send Email Preference.");
                throw CreateHttpResponseException("Unable  to set the Send Email Preference.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get the Send Email Preference 
        /// </summary>
        /// <param name="orgEntityId">Org Entity ID</param>
        /// <returns>An RetentionAlertSendEmailPreference <see cref="RetentionAlertSendEmailPreference">the send email preferences.</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. Forbidden returned if the user is not allowed to retrieve retention alert open cases.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. BadRequest returned if the DTO is not present in the request or any unexpected error has occured.</exception>
        /// <accessComments>
        /// An authenticated user can reassign the retention alert case if they have one of the following permissions:
        /// WORK.CASES
        /// WORK.ANY.CASE
        /// </accessComments>
        [HttpGet]
        public async Task<RetentionAlertSendEmailPreference> GetRetentionAlertEmailPreferenceAsync([FromUri] string orgEntityId)
        {
            if (string.IsNullOrEmpty(orgEntityId))
            {
                string errorText = "Must provide the orgEntityId to set the send email preference.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                RetentionAlertSendEmailPreference response = await _retentionAlertService.GetRetentionAlertEmailPreferenceAsync(orgEntityId);
                return response;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex, "User is not authorized to get the Send Email Preference.");
                throw CreateHttpResponseException("User is not authorized to get the Send Email Preference.", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unable to get the Send Email Preference.");
                throw CreateHttpResponseException("Unable  to get the Send Email Preference.", HttpStatusCode.BadRequest);
            }
        }
    }
}
