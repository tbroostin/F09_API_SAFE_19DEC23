// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to get and update Degree Plans.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class DegreePlansController : BaseCompressedApiController
    {
        private readonly IDegreePlanService _degreePlanService;
        private readonly ILogger _logger;
        private readonly ApiSettings apiSettings;

        /// <summary>
        /// DegreePlansController class constructor
        /// </summary>
        /// <param name="degreePlanService">Service of type <see cref="IDegreePlanService">IDegreePlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings"><see cref="ApiSettings"/>instance</param>
        public DegreePlansController(IDegreePlanService degreePlanService, ILogger logger, ApiSettings apiSettings)
        {
            _degreePlanService = degreePlanService;
            _logger = logger;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Retrieve all curriculum track codes available for a student for a given academic program code.
        /// </summary>
        /// <param name="criteria">A <see cref="CurriculumTrackQueryCriteria">curriculum tracks</see> used to retrieve Curriculum Tracks.</param>
        /// <returns>A collection of <see cref="CurriculumTrack">curriculum tracks</see> for student and a given program code.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a curriculum track is not found, either for the specified student and program</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the student id or program code is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view available curriculum tracks for their catalog.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view program curriculum tracks for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view program curriculum tracks for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        //[ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        //public async Task<IEnumerable<CurriculumTrack>> GetCurriculumTracksForStudentByProgramAsync(string studentId, string programCode)
        [HttpPost]
        public async Task<IEnumerable<CurriculumTrack>> QueryCurriculumTracksForStudentByProgramAsync([FromBody] CurriculumTrackQueryCriteria criteria)
        {
            try
            {
                return await _degreePlanService.QueryCurriculumTracksForStudentByProgramAsync(criteria);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                var exceptionMsg = "No curriculum tracks found for this program.";
                _logger.Error(aoure, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                var exceptionMsg = "No curriculum tracks found for this program.";
                _logger.Error(knfex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // student id or program code not provided.
                var exceptionMsg = "Either a student id or a program code was not provided.";
                _logger.Error(anex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                string exceptionMsg = "User is not permitted to program curriculum tracks for the student.";
                _logger.Error(peex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Could not retrieve curriculum tracks for this student and program code.", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Applies a sample degree plan to the degree plan and updates it in the database.
        /// </summary>
        /// <param name="request">The <see cref="LoadDegreePlanRequest">Load DegreePlan request</see> specifying which sample plan to load into the degree plan</param>
        /// <returns>The updated <see cref="DegreePlan">degree plan</see></returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A user may apply a sample degree plan to their own plan.
        /// An authenticated user (advisor) with either UPDATE.ASSIGNED.ADVISEES or ALL.ACCESS.ASSIGNED.ADVISEES permission may apply a sample plan to any of their assigned advisees.
        /// An authenticated user (advisor) with either UPDATE.ANY.ADVISEE or ALL.ACCESS.ANY.ADVISEE permission may apply a sample plan to any of their assigned advisees.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.2, use GET degree-plans/{degreePlanId}/preview-sample and PUT degree-plans to apply a sample plan")]
        public async Task<DegreePlan> PutApplySampleAsync([FromBody] LoadDegreePlanRequest request)
        {
            DegreePlan returnDegreePlanDto;
            try
            {
                returnDegreePlanDto = await _degreePlanService.ApplySampleDegreePlanAsync(request.StudentId, request.ProgramCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch only
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return returnDegreePlanDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task<DegreePlanPreview> GetSamplePlanPreviewAsync(int degreePlanId, string programCode)
        {
            DegreePlanPreview degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlanAsync(degreePlanId, programCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 3 of this API")]
        public async Task<DegreePlanPreview2> GetSamplePlanPreview2Async(int degreePlanId, string programCode, string firstTermCode)
        {
            DegreePlanPreview2 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan2Async(degreePlanId, programCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete on API version 1.6, use version 4 of this API")]
        public async Task<DegreePlanPreview3> GetSamplePlanPreview3Async(int degreePlanId, string programCode, string firstTermCode)
        {
            DegreePlanPreview3 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan3Async(degreePlanId, programCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete on API version 1.11, use version 5 of this API")]
        public async Task<DegreePlanPreview4> GetSamplePlanPreview4Async(int degreePlanId, string programCode, string firstTermCode)
        {
            DegreePlanPreview4 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan4Async(degreePlanId, programCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan4">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.18, use version 6 of this API")]
        public async Task<DegreePlanPreview5> GetSamplePlanPreview5Async(int degreePlanId, string programCode, string firstTermCode)
        {
            DegreePlanPreview5 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan5Async(degreePlanId, programCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Returns a student degree plan preview given a sample degree plan for the supplied program. The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="programCode">The program from which the sample plan should be derived.</param>
        /// <param name="firstTermCode">Code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan4">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API 1.32, use version 7 of this API")]
        public async Task<DegreePlanPreview6> GetSamplePlanPreview6Async(int degreePlanId, string programCode, string firstTermCode)
        {
            DegreePlanPreview6 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan6Async(degreePlanId, programCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample available for this program
                _logger.Error(aoure.ToString());
                throw CreateHttpResponseException(aoure.Message, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample available for this program
                _logger.Error(knfex.ToString());
                throw CreateHttpResponseException(knfex.Message, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or program code not provided.
                _logger.Error(anex.ToString());
                throw CreateHttpResponseException(anex.Message, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Retrieve a sample degree plan (curriculum track) for the given curriculum track code, using the provided term code as the first term.
        /// The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="curriculumTrackCode">The code of the curriculum Track from which the sample plan will be derived.</param>
        /// <param name="firstTermCode">The code for the term at which to start the sample plan</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan4">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete on version 1.33 of the Api. Use GetSamplePlanPreview8Async going forward.")]
        public async Task<DegreePlanPreview6> GetSamplePlanPreview7Async(int degreePlanId, string curriculumTrackCode, string firstTermCode)
        {
            DegreePlanPreview6 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan7Async(degreePlanId, curriculumTrackCode, firstTermCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample plan available for this curriculum track code
                var exceptionMsg = "No sample course plan found for this curriculum track code.";
                _logger.Error(aoure, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample plan available for this curriculum track code
                var exceptionMsg = "No sample course plan found for this curriculum track code.";
                _logger.Error(knfex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or curriculum track code not provided.
                var exceptionMsg = "Either a degree pland id or a curriculum track code was not provided.";
                _logger.Error(anex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                string exceptionMsg = "User is not permitted to preview the sample degree plan.";
                _logger.Error(peex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Could not retrieve sample degree plan.", HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }

        /// <summary>
        /// Retrieve a sample degree plan (curriculum track) for the given curriculum track code, using the provided term code as the first term.
        /// The student's plan is unchanged in the database.
        /// </summary>
        /// <param name="degreePlanId">The degree plan id of the plan to use as the basis for the plan preview.</param>
        /// <param name="curriculumTrackCode">The code of the curriculum Track from which the sample plan will be derived.</param>
        /// <param name="firstTermCode">The code for the term at which to start the sample plan</param>
        /// <param name="programCode">Academic program to evaluate when considering the student's academic history</param>
        /// <returns>A degree plan preview which contains both a limited preview of courses suggested along with a version of the student's <see cref="DegreePlan4">DegreePlan</see> now including the overlaid sample degree plan.</returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.NotFound returned if a sample degree plan is not found, either for the specified program or a default sample</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the degree plan id or programCode is not provided.</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to view the degree plan</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned for other errors that may occur</exception>
        /// <accessComments>
        /// A student may view their own sample plan preview.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may view a sample plan preview for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        public async Task<DegreePlanPreview7> GetSamplePlanPreview8Async(int degreePlanId, string curriculumTrackCode, string firstTermCode, string programCode)
        {
            DegreePlanPreview7 degreePlanPreviewDto;
            try
            {
                degreePlanPreviewDto = await _degreePlanService.PreviewSampleDegreePlan8Async(degreePlanId, curriculumTrackCode, firstTermCode, programCode);
            }
            catch (ArgumentOutOfRangeException aoure)
            {
                // no sample plan available for this curriculum track code
                var exceptionMsg = "No sample course plan found for this curriculum track code.";
                _logger.Error(aoure, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (KeyNotFoundException knfex)
            {
                // no sample plan available for this curriculum track code
                var exceptionMsg = "No sample course plan found for this curriculum track code.";
                _logger.Error(knfex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException anex)
            {
                // degree plan id or curriculum track code not provided.
                var exceptionMsg = "Either a degree pland id or a curriculum track code was not provided.";
                _logger.Error(anex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException peex)
            {
                string exceptionMsg = "User is not permitted to preview the sample degree plan.";
                _logger.Error(peex, exceptionMsg);
                throw CreateHttpResponseException(exceptionMsg, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Any other error, described in returned message
                _logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Could not retrieve sample degree plan.", HttpStatusCode.BadRequest);
            }

            return degreePlanPreviewDto;
        }


        /// <summary>
        /// Create an archive (snap-shot) of the degree plan
        /// </summary>
        /// <param name="degreePlan">The degree plan to be archived</param>
        /// <returns>The updated degree plan</returns>
        /// <accessComments>
        /// A person may archive their own degree plan.
        /// 
        /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES, UPDATE.ASSIGNED.ADVISEES or ALL.ACCESS.ASSIGNED.ADVISEES permission may archive 
        /// the degree plan for one of their assigned advisees.
        /// 
        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE, UPDATE.ANY.ADVISEE, or ALL.ACCESS.ANY.ADVISEE permission may may archive 
        /// the degree plan forany advisee.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 2 of this API")]
        public async Task<DegreePlanArchive> PostArchiveAsync(DegreePlan2 degreePlan)
        {
            try
            {
                if (degreePlan == null)
                {
                    throw new ArgumentNullException("degreePlan", "You must specify a degree plan id to retrieve archives.");
                }
                var degreePlanArchive = await _degreePlanService.ArchiveDegreePlanAsync(degreePlan);
                return degreePlanArchive;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create an archive (snap-shot) of the degree plan
        /// </summary>
        /// <param name="degreePlan">The degree plan to be archived</param>
        /// <returns>The <see cref="DegreePlanArchive2">degree plan archive</see></returns>
        /// <accessComments>
        /// A person may archive their own degree plan.
        /// 
        /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES, UPDATE.ASSIGNED.ADVISEES or ALL.ACCESS.ASSIGNED.ADVISEES permission may archive 
        /// the degree plan for one of their assigned advisees.
        /// 
        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE, UPDATE.ANY.ADVISEE, or ALL.ACCESS.ANY.ADVISEE permission may may archive 
        /// the degree plan forany advisee.
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.7, use version 3 of this API")]
        public async Task<DegreePlanArchive2> PostArchive2Async(DegreePlan3 degreePlan)
        {
            try
            {
                if (degreePlan == null)
                {
                    throw new ArgumentNullException("degreePlan", "You must specify a degree plan id to retrieve archives.");
                }
                var degreePlanArchive = await _degreePlanService.ArchiveDegreePlan2Async(degreePlan);
                return degreePlanArchive;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create an archive (snap-shot) of the degree plan
        /// </summary>
        /// <param name="degreePlan">The degree plan to be archived</param>
        /// <returns>The <see cref="DegreePlanArchive2">degree plan archive</see></returns>
        /// <accessComments>
        /// A person may archive their own degree plan.
        /// 
        /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES, UPDATE.ASSIGNED.ADVISEES or ALL.ACCESS.ASSIGNED.ADVISEES permission may archive 
        /// the degree plan for one of their assigned advisees.
        /// 
        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE, UPDATE.ANY.ADVISEE, or ALL.ACCESS.ANY.ADVISEE permission may may archive 
        /// the degree plan forany advisee.
        /// </accessComments>
        public async Task<DegreePlanArchive2> PostArchive3Async(DegreePlan4 degreePlan)
        {
            try
            {
                if (degreePlan == null)
                {
                    throw new ArgumentNullException("degreePlan", "You must specify a degree plan id to retrieve archives.");
                }
                var degreePlanArchive = await _degreePlanService.ArchiveDegreePlan3Async(degreePlan);
                return degreePlanArchive;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a list of all archives for a particular degree plan.
        /// </summary>
        /// <param name="degreePlanId">The degree plan for which the archives are requested.</param>
        /// <returns>All degree plan archives that have been created for the specified degree plan. The list may not contain any items.</returns>
        /// <accessComments>
        /// A person may retrieve their own degree plan archives.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archives for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archives for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 2 of this API")]
        public async Task<IEnumerable<DegreePlanArchive>> GetDegreePlanArchivesAsync(int degreePlanId)
        {
            try
            {
                if (degreePlanId == 0)
                {
                    throw new ArgumentNullException("degreePlanId");
                }
                var degreePlanArchive = await _degreePlanService.GetDegreePlanArchivesAsync(degreePlanId);

                return degreePlanArchive;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a list of all archives for a particular degree plan.
        /// </summary>
        /// <param name="degreePlanId">The degree plan for which the archives are requested.</param>
        /// <returns>All <see cref="DegreePlanArchive2">degree plan archives</see> that have been created for the specified degree plan. An empty list may be returned.</returns>
        /// <accessComments>
        /// A person may retrieve their own degree plan archives.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archives for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archives for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        public async Task<IEnumerable<DegreePlanArchive2>> GetDegreePlanArchives2Async(int degreePlanId)
        {
            try
            {
                if (degreePlanId == 0)
                {
                    throw new ArgumentNullException("degreePlanId");
                }
                var degreePlanArchive = await _degreePlanService.GetDegreePlanArchives2Async(degreePlanId);

                return degreePlanArchive;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Given a plan archive ID, return the pdf report for that archive.
        /// </summary>
        /// <param name="id">The system id for the plan archive object being requested</param>
        /// <returns>The pdf report based on the requested archive.</returns>
        /// <accessComments>
        /// A person may retrieve their own degree plan archive.
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archive for one of their assigned advisees
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan archive for any advisee
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        public async Task<HttpResponseMessage> GetPlanArchiveAsync(int id)
        {
            try
            {
                if (Request.Headers.Accept.Where(rqa => rqa.MediaType == "application/pdf").Count() > 0)
                {
                    var path = HttpContext.Current.Server.MapPath("~/Reports/Planning/DegreePlanArchive.rdlc");
                    var reportLogoPath = !string.IsNullOrEmpty(apiSettings.ReportLogoPath) ? apiSettings.ReportLogoPath : "";
                    if (!reportLogoPath.StartsWith("~"))
                    {
                        reportLogoPath = "~" + reportLogoPath;
                    }
                    reportLogoPath = HttpContext.Current.Server.MapPath(reportLogoPath);
                    // Get the archive report object (this contains the archived plan data, which gets fed into the report object)
                    var archiveReport = await _degreePlanService.GetDegreePlanArchiveReportAsync(id);
                    var renderedBytes = _degreePlanService.GenerateDegreePlanArchiveReport(archiveReport, path, reportLogoPath);

                    // Create the http response object, use the byte array for the response content, and set header content type to the mime type of the report
                    var response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(renderedBytes);

                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = Regex.Replace(
                            (archiveReport.StudentLastName +
                            " " + archiveReport.StudentFirstName +
                            " " + archiveReport.StudentId +
                            " " + archiveReport.ArchivedOn.DateTime.ToShortDateString() +
                            " " + archiveReport.ArchivedOn.DateTime.ToShortTimeString()),
                            "[^a-zA-Z0-9_]", "_")
                            + ".pdf"
                    };
                    response.Content.Headers.ContentLength = renderedBytes.Length;
                    return response;
                }
                // If the request didn't specify pdf, it's an unsupported request
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (NotSupportedException)
            {
                throw CreateHttpResponseException("Only application/pdf is served from this endpoint", HttpStatusCode.NotAcceptable);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (KeyNotFoundException)
            {
                throw CreateNotFoundException("Degree Plan Archive", id.ToString());
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
