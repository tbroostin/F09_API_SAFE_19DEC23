// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using slf4net;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to get and update Degree Plans.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentDegreePlansController : BaseCompressedApiController
    {

        private readonly IStudentDegreePlanService _studentDegreePlanService;
        private readonly ILogger _logger;

        /// <summary>
        /// StudentDegreePlansController class constructor
        /// </summary>
        /// <param name="studentDegreePlanService">Service of type <see cref="IStudentDegreePlanService">IStudentDegreePlanService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentDegreePlansController(IStudentDegreePlanService studentDegreePlanService, ILogger logger)
        {
            _studentDegreePlanService = studentDegreePlanService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId.
        /// </summary>
        /// <param name="id">id of the degree plan</param>
        /// <returns>The student's <see cref="DegreePlan">Degree Plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task<DegreePlan> GetAsync(int id)
        {
            try
            {
                return await _studentDegreePlanService.GetDegreePlanAsync(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("Degree Plan", id.ToString());
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId.
        /// </summary>
        /// <remarks>This is the current version.</remarks>
        /// <param name="id">id of the degree plan</param>
        /// <returns>The student's <see cref="DegreePlan2">Degree Plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 3 of this API")]
        public async Task<DegreePlan2> Get2Async(int id)
        {
            try
            {
                return await _studentDegreePlanService.GetDegreePlan2Async(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("Degree Plan", id.ToString());
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId.
        /// </summary>
        /// <remarks>This is the current version.</remarks>
        /// <param name="id">id of the degree plan</param>
        /// <returns>The student's <see cref="DegreePlan3">Degree Plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.6, use version 4 of this API")]
        public async Task<DegreePlan3> Get3Async(int id)
        {
            try
            {
                return await _studentDegreePlanService.GetDegreePlan3Async(id);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("Degree Plan", id.ToString());
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId, which will be validated by default. A validated degree plan checks the student's academic history and planned coursework as follows:
        /// - Planned courses are evaluated against course-based rules from any requisites on the student's planned courses and sections
        /// - Academic credits are evaluated against credit-based rules from any requisites on the student's planned courses and sections
        /// - Planned courses and academic credits are evaluated for any session and yearly cycle restrictions by location
        /// - All student coursework is evaluated for unsatisfied prerequisites
        /// - All student coursework is evaluated for missing corequisites
        /// - All student planned courses are evaluated to determine if they are planned in an academic term where they are not offered
        /// - All student coursework is evaluated for scheduling conflicts
        /// - All student planned courses are evaluated for invalid credits based on course and/or section minimum/maximum/increment credit values
        /// When retrieving a degree plan, you can choose to skip the aforementioned degree plan validation using the 'validate' parameter
        /// </summary>
        /// <remarks>This is not the current version.</remarks>
        /// <param name="id">id of the degree plan</param>
        /// <param name="validate">Defaults to true. If false, returns a non-validated degree plan (use when planned course warnings are not needed to improve performance)</param>
        /// <returns>A combined dto <see cref="DegreePlanAcademicHistory">DegreePlanAcademicHistory</see> which includes the student's <see cref="DegreePlan3">Degree Plan</see> and <see cref="AcademicHistory2">AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.11, use Get5Async going forward")]
        public async Task<DegreePlanAcademicHistory> Get4Async(int id, bool validate = true)
        {
            try
            {
                return await _studentDegreePlanService.GetDegreePlan4Async(id, validate);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception)
            {
                throw CreateNotFoundException("Degree Plan", id.ToString());
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId, which will be validated by default. A validated degree plan checks the student's academic history and planned coursework as follows:
        /// - Planned courses are evaluated against course-based rules from any requisites on the student's planned courses and sections
        /// - Academic credits are evaluated against credit-based rules from any requisites on the student's planned courses and sections
        /// - Planned courses and academic credits are evaluated for any session and yearly cycle restrictions by location
        /// - All student coursework is evaluated for unsatisfied prerequisites
        /// - All student coursework is evaluated for missing corequisites
        /// - All student planned courses are evaluated to determine if they are planned in an academic term where they are not offered
        /// - All student coursework is evaluated for scheduling conflicts
        /// - All student planned courses are evaluated for invalid credits based on course and/or section minimum/maximum/increment credit values
        /// When retrieving a degree plan, you can choose to skip the aforementioned degree plan validation using the 'validate' parameter 
        /// </summary>
        /// <remarks>This is the current version.</remarks>
        /// <param name="id">id of the degree plan</param>
        /// <param name="validate">Defaults to true. If false, returns a non-validated degree plan (use when planned course warnings are not needed to improve performance)</param>
        /// <returns>A combined dto <see cref="DegreePlanAcademicHistory2">DegreePlanAcademicHistory2</see> which includes the student's <see cref="DegreePlan4">Degree Plan</see> and <see cref="AcademicHistory3">AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.18, use Get6Async going forward")]
        public async Task<DegreePlanAcademicHistory2> Get5Async(int id, bool validate = true)
        {
            try
            {
                return await _studentDegreePlanService.GetDegreePlan5Async(id, validate);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString(), "Unable to get DegreePlanAcademicHistory for plan " + id);
                throw CreateNotFoundException("DegreePlanAcademicHistory", id.ToString());
            }
        }

        /// <summary>
        /// Retrieves a student's degree plan using the unique DegreePlanId, which will be validated by default. A validated degree plan checks the student's academic history and planned coursework as follows:
        /// a. Planned courses are evaluated against course-based rules from any requisites on the student's planned courses and sections
        /// b. Academic credits are evaluated against credit-based rules from any requisites on the student's planned courses and sections
        /// c. Planned courses and academic credits are evaluated for any session and yearly cycle restrictions by location
        /// d. All student coursework is evaluated for unsatisfied prerequisites
        /// e. All student coursework is evaluated for missing corequisites
        /// f. All student planned courses are evaluated to determine if they are planned in an academic term where they are not offered
        /// g. All student coursework is evaluated for scheduling conflicts
        /// h. All student planned courses are evaluated for invalid credits based on course and/or section minimum/maximum/increment credit values
        /// When retrieving a degree plan, you can choose to skip the aforementioned degree plan validation using the 'validate' parameter
        /// </summary>
        /// <remarks>This is the current version.</remarks>
        /// <param name="id">id of the degree plan</param>
        /// <param name="validate">Defaults to true. If false, returns a non-validated degree plan (use when planned course warnings are not needed to improve performance)</param>
        /// <param name="includeDrops">Defaults to false, If true, includes dropped academic credits in the degree plan</param>
        /// <returns>A combined dto <see cref="DegreePlanAcademicHistory3">DegreePlanAcademicHistory3</see> which includes the student's <see cref="DegreePlan4">Degree Plan</see> and <see cref="AcademicHistory3">AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if user does not have the required role and permissions to access this degree plan</exception>
        /// <accessComments>        /// A person may retrieve their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may retrieve the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// </accessComments>
        public async Task<DegreePlanAcademicHistory3> Get6Async(int id, bool validate = true, bool includeDrops = false)
        {
            try
            {
                var degreePlanAcadHistory = await _studentDegreePlanService.GetDegreePlan6Async(id, validate, includeDrops);
                return degreePlanAcadHistory;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout have occurred while retrieval of degree plan for the student {0}", id);
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException  ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString(), "Unable to get DegreePlanAcademicHistory for plan " + id);
                throw CreateNotFoundException("DegreePlanAcademicHistory", id.ToString());
            }
           
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety</param>
        /// <returns>The updated <see cref="DegreePlan">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may make any updates to their own degree plan.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task<DegreePlan> PutAsync(DegreePlan degreePlan)
        {
            DegreePlan returnDegreePlanDto = null;

            try
            {
                returnDegreePlanDto = await _studentDegreePlanService.UpdateDegreePlanAsync(degreePlan);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDegreePlanDto;
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety (DegreePlan2)</param>
        /// <returns>The updated <see cref="DegreePlan2">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may update their own degree plan but are not allowed to move or remove a protected planned course item.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 3 of this API")]
        public async Task<DegreePlan2> Put2Async(DegreePlan2 degreePlan)
        {
            DegreePlan2 returnDegreePlanDto = null;

            try
            {
                returnDegreePlanDto = await _studentDegreePlanService.UpdateDegreePlan2Async(degreePlan);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDegreePlanDto;
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety (DegreePlan3)</param>
        /// <returns>The updated <see cref="DegreePlan3">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may update their own degree plan but are not allowed to move or remove a protected planned course item.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        [Obsolete("Obsolete as of API version 1.6, use version 4 of this API")]
        public async Task<DegreePlan3> Put3Async(DegreePlan3 degreePlan)
        {
            DegreePlan3 returnDegreePlanDto = null;

            try
            {
                returnDegreePlanDto = await _studentDegreePlanService.UpdateDegreePlan3Async(degreePlan);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {

                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDegreePlanDto;
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety (DegreePlan3)</param>
        /// <returns>The updated <see cref="DegreePlan3">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may update their own degree plan but are not allowed to move or remove a protected planned course item.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        [Obsolete("Obsolete as of API version 1.11, use Put5Async instead")]
        public async Task<DegreePlanAcademicHistory> Put4Async(DegreePlan4 degreePlan)
        {
            DegreePlanAcademicHistory returnDto = null;

            try
            {
                returnDto = await _studentDegreePlanService.UpdateDegreePlan4Async(degreePlan);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDto;
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety (DegreePlan4)</param>
        /// <returns>The updated <see cref="DegreePlanAcademicHistory2">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may update their own degree plan but are not allowed to move or remove a protected planned course item.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        [Obsolete("Obsolete as of API version 1.18, use Put6Async instead")]
        public async Task<DegreePlanAcademicHistory2> Put5Async(DegreePlan4 degreePlan)
        {
            DegreePlanAcademicHistory2 returnDto = null;

            try
            {
                returnDto = await _studentDegreePlanService.UpdateDegreePlan5Async(degreePlan);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDto;
        }

        /// <summary>
        /// Updates an existing degree plan.
        /// </summary>
        /// <param name="degreePlan">The degree plan in its entirety (DegreePlan4)</param>
        /// <returns>The updated <see cref="DegreePlanAcademicHistory3">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if version number of passed degree plan object does not match the version in the database, indicating that an update has occurred on the degree plan by another user and this action has not been saved.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may update their own degree plan but are not allowed to move or remove a protected planned course item.        ///         /// An authenticated user (advisor) with REVIEW.ASSIGNED.ADVISEES permission may perform review type of updates on a degree plan for one of their assigned advisees.        /// An authenticated user (advisor) with REVIEW.ANY.ADVISEE permission may perform review type of updates on a degree plan for any advisee.        /// Review types of updates include approving or denying planned courses, setting protection level for planned courses, adding advising notes, marking a plan as review complete or advisment complete.        ///         /// In addition to the above review actions, an advisor with any of the following permission codes may make any change to the plan for either their assigned advisees or any        /// advisee based on the specific permission code:        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        /// Additional update actions include adding, moving or removing planned courses and adding or removing terms from the plan.         /// </accessComments>
        public async Task<DegreePlanAcademicHistory3> Put6Async(DegreePlan4 degreePlan)
        {
            DegreePlanAcademicHistory3 returnDto = null;
            if (degreePlan == null)
            {
                throw new ArgumentNullException("degreePlan", "degreePlan is null while updating degree plan");
            }
            try
            {
                returnDto = await _studentDegreePlanService.UpdateDegreePlan6Async(degreePlan);
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = string.Format("Timeout have occurred while updating degree plan with Id {0}", degreePlan.Id);
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (InvalidOperationException ioex)
            {
                // Version number mismatch
                _logger.Error(ioex.ToString());
                throw CreateHttpResponseException(ioex.Message, HttpStatusCode.Conflict);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return returnDto;
        }

        /// <summary>
        /// Creates a new degree plan for the specified student. The plan will be created with any applicable terms based on 
        /// the Default on Plan attribute of the terms and the student's anticipated completion date.
        /// </summary>
        /// <remarks>In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 4 of this API")]
        public async Task<HttpResponseMessage> PostAsync([FromBody]string studentId)
        {
            try
            {
                DegreePlan newPlanDto = await _studentDegreePlanService.CreateDegreePlanAsync(studentId);
                var response = Request.CreateResponse<DegreePlan>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist, student is locked, etc.
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new degree plan for the specified student.
        /// </summary>
        /// <remarks>This is the current version. In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use version 4 of this API")]
        public async Task<HttpResponseMessage> Post2Async([FromBody]string studentId)
        {
            try
            {
                DegreePlan2 newPlanDto = await _studentDegreePlanService.CreateDegreePlan2Async(studentId);
                var response = Request.CreateResponse<DegreePlan2>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist, student is locked, etc.
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new degree plan for the specified student.
        /// </summary>
        /// <remarks>This is the current version. In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan">degree plan</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.6, use version 4 of this API")]
        public async Task<HttpResponseMessage> Post3Async([FromBody]string studentId)
        {
            try
            {
                DegreePlan3 newPlanDto = await _studentDegreePlanService.CreateDegreePlan3Async(studentId);
                var response = Request.CreateResponse<DegreePlan3>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist, student is locked, etc.
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new degree plan for the specified student.
        /// </summary>
        /// <remarks>This is the current version. In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan3">degree plan</see> and the <see cref="AcademicHistory2">student's AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.11, use Post5Async instead")]
        public async Task<HttpResponseMessage> Post4Async([FromBody]string studentId)
        {
            try
            {
                DegreePlanAcademicHistory newPlanDto = await _studentDegreePlanService.CreateDegreePlan4Async(studentId);
                var response = Request.CreateResponse<DegreePlanAcademicHistory>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.DegreePlan.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist, student is locked, etc.
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new degree plan for the specified student.
        /// </summary>
        /// <remarks>This is the current version. In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan4">degree plan</see> and the <see cref="AcademicHistory3">student's AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.18, use Post6Async instead")]
        public async Task<HttpResponseMessage> Post5Async([FromBody]string studentId)
        {
            try
            {
                DegreePlanAcademicHistory2 newPlanDto = await _studentDegreePlanService.CreateDegreePlan5Async(studentId);
                var response = Request.CreateResponse<DegreePlanAcademicHistory2>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.DegreePlan.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist, student is locked, etc.
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Creates a new degree plan for the specified student. Depending on your institution's Self-Service registration parameters, the plan may be created with any applicable terms based on 
        /// the Default on Plan attribute of the terms and the student's anticipated completion date.
        /// </summary>
        /// <remarks>This is the current version. In MVC4 RC the [FromBody] attribute is required on the studentId parameter. Without this
        /// the parameter is always mapped as null and the call will fail since by default, simple types line
        /// strings are pulled from the Uri and not the body.</remarks>
        /// <param name="studentId">The ID of the student for whom to create a new degree plan</param>
        /// <returns>An HttpResponseMessage which includes the newly created <see cref="DegreePlan4">degree plan</see> and the <see cref="AcademicHistory3">student's AcademicHistory</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to create a degree plan for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Conflict returned if a degree plan already exists for this student</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if invalid student id or student locked or any other creation problem.</exception>
        /// <accessComments>        /// A person may create their own degree plan.        ///         /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for one of their assigned advisees        /// VIEW.ASSIGNED.ADVISEES        /// REVIEW.ASSIGNED.ADVISEES        /// UPDATE.ASSIGNED.ADVISEES        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) with any of the following permission codes may create the degree plan for any advisee
        /// VIEW.ANY.ADVISEE        /// REVIEW.ANY.ADVISEE        /// UPDATE.ANY.ADVISEE        /// ALL.ACCESS.ANY.ADVISEE        ///         /// Note: Advisor view only permissions are included because when accessing an advisee for the first time, even to view it, it must exist.        /// </accessComments>
        public async Task<HttpResponseMessage> Post6Async([FromBody]string studentId)
        {
            try
            {
                DegreePlanAcademicHistory3 newPlanDto = await _studentDegreePlanService.CreateDegreePlan6Async(studentId);
                var response = Request.CreateResponse<DegreePlanAcademicHistory3>(HttpStatusCode.Created, newPlanDto);
                SetResourceLocationHeader("GetDegreePlan", new { id = newPlanDto.DegreePlan.Id });
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (ExistingDegreePlanException eex)
            {
                _logger.Info(eex.ToString());
                SetResourceLocationHeader("GetDegreePlan", new { id = eex.ExistingPlanId });
                throw CreateHttpResponseException(eex.Message, HttpStatusCode.Conflict);
            }
            catch (RecordLockException rex)
            {
                _logger.Info(rex.ToString());
                throw CreateHttpResponseException(rex.Message, HttpStatusCode.Conflict);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                // Student Id not provided, student Id does not exist
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Given a degree plan and a term, submit a registration request.
        /// Any sections selected on the plan should be submitted to registration
        /// </summary>
        /// <param name="degreePlanId">The degree plan, which contains the sections the student has planned and now wishes to register for </param>
        /// <param name="termId">The term for which the student is registering</param>
        /// <returns>A list of <see cref="RegistrationMessage">registration messages</see> returned from the registration system.
        /// </returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if either the degreePlanId or termId argument is not specified</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the student's degree plan</exception>
        /// <accessComments>        /// A person may perform registration actions (register, drop, waitlist, etc) for themselves.          /// An advisor with ALL.ACCESS.ANY.ADVISEE may perform registration actions for any student.        /// An advisor with ALL.ACCESS.ASSIGNED.ADVISEES may perform registration actions for one of their assigned advisees.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 2 of this API")]
        public async Task<IEnumerable<RegistrationMessage>> PutRegistrationAsync(int degreePlanId, string termId)
        {
            if (degreePlanId == 0)
            {
                _logger.Error("Invalid degreePlanId");
                throw CreateHttpResponseException("Invalid degreePlanId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(termId))
            {
                _logger.Error("Invalid termId");
                throw CreateHttpResponseException("Invalid termId", HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<RegistrationMessage> messages = (await _studentDegreePlanService.RegisterAsync(degreePlanId, termId)).Messages;
                return messages;
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Given a degree plan and a term, submit a registration request.
        /// Any sections selected on the plan should be submitted to registration
        /// </summary>
        /// <param name="degreePlanId">The degree plan, which contains the sections the student has planned and now wishes to register for </param>
        /// <param name="termId">The term for which the student is registering</param>
        /// <returns>A <see cref="RegistrationResponse">response</see> returned from the registration system.
        /// </returns>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if either the degreePlanId or termId argument is not specified</exception>
        /// <exception> <see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the student's degree plan</exception>
        /// <accessComments>        /// A person may perform registration actions (register, drop, waitlist, etc) for themselves.          /// An advisor with ALL.ACCESS.ANY.ADVISEE may perform registration actions for any student.        /// An advisor with ALL.ACCESS.ASSIGNED.ADVISEES may perform registration actions for one of their assigned advisees.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use students/{studentId}/register moving forward.")]
        public async Task<RegistrationResponse> PutRegistration2Async(int degreePlanId, string termId)
        {
            if (degreePlanId == 0)
            {
                _logger.Error("Invalid degreePlanId");
                throw CreateHttpResponseException("Invalid degreePlanId", HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(termId))
            {
                _logger.Error("Invalid termId");
                throw CreateHttpResponseException("Invalid termId", HttpStatusCode.BadRequest);
            }
            try
            {
                RegistrationResponse response = await _studentDegreePlanService.RegisterAsync(degreePlanId, termId);
                return response;
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Takes registration action (add drop waitlist) on specified course sections.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan for which section registration actions are being taken</param>
        /// <param name="sectionRegistrations">List of <see cref="SectionRegistration">section registration actions</see> to take</param>
        /// <returns>A list of <see cref="RegistrationMessage">registration messages</see> from the registration process indicating success or failure</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if either the degreePlanId argument or the sectionRegistration information is not specified</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may perform registration actions (register, drop, waitlist, etc) for themselves.          /// An advisor with ALL.ACCESS.ANY.ADVISEE may perform registration actions for any student.        /// An advisor with ALL.ACCESS.ASSIGNED.ADVISEES may perform registration actions for one of their assigned advisees.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.3, use version 2 of this API")]
        public async Task<IEnumerable<RegistrationMessage>> PutSectionRegistrationAsync(int degreePlanId, IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (degreePlanId == 0)
            {
                _logger.Error("Invalid degreePlanId");
                throw CreateHttpResponseException("Invalid degreePlanId", HttpStatusCode.BadRequest);
            }
            if (sectionRegistrations == null)
            {
                _logger.Error("Invalid sectionRegistration");
                throw CreateHttpResponseException("Invalid sectionRegistration", HttpStatusCode.BadRequest);
            }
            try
            {
                IEnumerable<RegistrationMessage> messages = (await _studentDegreePlanService.RegisterSectionsAsync(degreePlanId, sectionRegistrations)).Messages;
                return messages;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Takes registration action (add drop waitlist) on specified course sections.
        /// </summary>
        /// <param name="degreePlanId">Id of the degree plan for which section registration actions are being taken</param>
        /// <param name="sectionRegistrations">List of <see cref="SectionRegistration">section registration actions</see> to take</param>
        /// <returns>A <see cref="RegistrationResponse">response</see> returned from the registration system.</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if either the degreePlanId argument or the sectionRegistration information is not specified</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user does not have the role or permissions required to update the degree plan</exception>
        /// <accessComments>        /// A person may perform registration actions (register, drop, waitlist, etc) for themselves.          /// An advisor with ALL.ACCESS.ANY.ADVISEE may perform registration actions for any student.        /// An advisor with ALL.ACCESS.ASSIGNED.ADVISEES may perform registration actions for one of their assigned advisees.        /// </accessComments>
        [Obsolete("Obsolete as of API version 1.5, use students/{studentId}/register moving forward.")]
        public async Task<RegistrationResponse> PutSectionRegistration2Async(int degreePlanId, IEnumerable<SectionRegistration> sectionRegistrations)
        {
            if (degreePlanId == 0)
            {
                _logger.Error("Invalid degreePlanId");
                throw CreateHttpResponseException("Invalid degreePlanId", HttpStatusCode.BadRequest);
            }
            if (sectionRegistrations == null)
            {
                _logger.Error("Invalid sectionRegistration");
                throw CreateHttpResponseException("Invalid sectionRegistration", HttpStatusCode.BadRequest);
            }
            try
            {
                RegistrationResponse response = await _studentDegreePlanService.RegisterSectionsAsync(degreePlanId, sectionRegistrations);
                return response;
            }
            catch (PermissionsException peex)
            {
                _logger.Info(peex.ToString());
                throw CreateHttpResponseException(peex.Message, HttpStatusCode.Forbidden);
            }
            catch (EllucianLicenseException ex)
            {
                _logger.Info(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
        }
    }
}
