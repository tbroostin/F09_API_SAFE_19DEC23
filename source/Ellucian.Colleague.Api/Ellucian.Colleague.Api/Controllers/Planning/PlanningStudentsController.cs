// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Planning
{
    /// <summary>
    /// Supplements the Students controller in the student module namespace
    /// with functionality that is only available from within the planning module namespace.
    /// </summary>
    [System.Web.Http.Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Planning)]
    public class PlanningStudentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IProgramEvaluationService _programEvaluationService;
        private readonly IPlanningStudentService _planningStudentService;
        private readonly ILogger _logger;

        /// <summary>
        /// PlanningStudentsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="programEvaluationService">Program Evaluation Service of type <see cref="IProgramEvaluationService">IProgramEvaluationService</see></param>
        /// <param name="planningStudentService">Planning Student Service</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PlanningStudentsController(IAdapterRegistry adapterRegistry, IProgramEvaluationService programEvaluationService, 
            IPlanningStudentService planningStudentService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _programEvaluationService = programEvaluationService;
            _planningStudentService = planningStudentService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <returns>The <see cref="ProgramEvaluation">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.11, use GetEvaluation2Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        public async Task<Ellucian.Colleague.Dtos.Planning.ProgramEvaluation> GetEvaluationAsync(string id, string program)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, null)).First();

                // Get the adapter
                var programEvaluationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>();

                // use adapter to map data to DTO
                var evaluation = programEvaluationDtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a program evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 
        /// An authenticated user (advisor) may retrieve any proram evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>> QueryEvaluationsAsync(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Ellucian.Colleague.Dtos.Planning.ProgramEvaluation>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluationDtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the notice for the specified student and program. Student does not have to be currently enrolled in the program.
        /// </summary>
        /// <param name="studentId">The student's ID</param>
        /// <param name="programCode">The program code</param>
        /// <returns>List of <see cref="EvaluationNotice">Evaluation Notices</see></returns>
        /// <accessComments>
        /// Evaluation Notices can only be retrieved if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve any Evaluation Notices from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Evaluation Notices if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 3. User with VIEW.STUDENT.INFORMATION permission
        /// 4. User is a proxy user
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "programCode" })]
        public async Task<IEnumerable<Dtos.Student.EvaluationNotice>> GetEvaluationNoticesAsync(string studentId, string programCode)
        {
            try
            {
                var notices = await _programEvaluationService.GetEvaluationNoticesAsync(studentId, programCode);
                return notices;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves a planning student Dtos containing fewer properties than student Dtos for the specified list of student ids
        /// </summary>
        /// <param name="criteria">Planning Student Criteria</param>
        /// <returns>List of <see cref="PlanningStudent">Planning Students</see></returns>
        /// <accessComments>
        /// Student information can be retrieved only if:
        /// 1. An Advisor with any of the following codes is accessing the student's data if the student is not assigned advisee.
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// 2. An Advisor with any of the following codes is accessing the student's data if the student is assigned advisee.
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// 3. A user with permission of VIEW.PERSON.INFORMATION is accessing the student's data.
        /// 
        /// Student privacy is enforced by this response. If any student has an assigned privacy code that the requestor is not authorized to access, 
        /// the response object is returned with an X-Content-Restricted header with a value of "partial" to indicate only partial information is returned for some subset of students. 
        /// In this situation, all details except the student name are cleared from the specific student object.        
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<PlanningStudent>> QueryPlanningStudentsAsync([FromBody]PlanningStudentCriteria criteria)
        {
            _logger.Info("Entering QueryPlanningStudentsAsync");
            var watch = new Stopwatch();
            watch.Start();
            try
            {
                //this will call planning student service with ienumerable of student ids
                var privacyWrapper = await _planningStudentService.QueryPlanningStudentsAsync(criteria.StudentIds);
                var planningStudents = privacyWrapper.Dto as List<PlanningStudent>;
                if (privacyWrapper.HasPrivacyRestrictions)
                {
                    HttpContext.Current.Response.AppendHeader("X-Content-Restricted", "partial");
                }
                watch.Stop();
                _logger.Info("QueryPlanningStudentsAsync... completed in " + watch.ElapsedMilliseconds.ToString());

                return (IEnumerable<PlanningStudent>)planningStudents;
            }
            catch (Exception exception)
            {
                _logger.Error(exception.ToString());
                throw CreateHttpResponseException(exception.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <returns>The <see cref="ProgramEvaluation2">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.13, use GetEvaluation3Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        public async Task<Dtos.Planning.ProgramEvaluation2> GetEvaluation2Async(string id, string program)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, null)).First();

                // Get the adapter
                var programEvaluation2DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation2DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation2">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.13, use QueryEvaluations3Async instead.")]
        public async Task<IEnumerable<Dtos.Planning.ProgramEvaluation2>> QueryEvaluations2Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation2DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation2>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation2DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }


        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <param name="catalogYear">The catalogYear code for the program</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.33, use QueryEvaluations4Async instead.")]
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        public async Task<Dtos.Planning.ProgramEvaluation3> GetEvaluation3Async(string id, string program, string catalogYear = null)
        {
            try
            {
                Domain.Student.Entities.ProgramEvaluation programEvaluationEntity;
                // Call the coordination-layer evaluation service
                if (string.IsNullOrEmpty(catalogYear)) { catalogYear = null; }

                programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, catalogYear)).First();         

                // Get the adapter
                var programEvaluation3DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation3DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results for the student's specified program.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="program">The student's program code</param>
        /// <param name="catalogYear">The catalogYear code for the program</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "program" })]
        public async Task<Dtos.Planning.ProgramEvaluation4> GetEvaluation4Async(string id, string program, string catalogYear = null)
        {
            try
            {
                Domain.Student.Entities.ProgramEvaluation programEvaluationEntity;
                // Call the coordination-layer evaluation service
                if (string.IsNullOrEmpty(catalogYear)) { catalogYear = null; }

                programEvaluationEntity = (await _programEvaluationService.EvaluateAsync(id, new List<string>() { program }, catalogYear)).First();

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>();

                // use adapter to map data to DTO
                var evaluation = programEvaluation4DtoAdapter.MapToType(programEvaluationEntity);
                return evaluation;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation3">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        [Obsolete("Obsolete as of Colleague API 1.33, use QueryEvaluations4Async instead.")]
        public async Task<IEnumerable<Dtos.Planning.ProgramEvaluation3>> QueryEvaluations3Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation3DtoAdapter = _adapterRegistry.GetAdapter< Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation3>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation3DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the program evaluation results of the given student against a list of programs.
        /// </summary>
        /// <remarks>
        /// Routing is used to expose this action under the /Students path.
        /// </remarks>
        /// <param name="id">The student's ID</param>
        /// <param name="programCodes">The list of programs to evaluate</param>
        /// <returns>The <see cref="ProgramEvaluation4">Program Evaluation</see> result</returns>
        /// <accessComments>
        /// Program Evaluation can be retrieved only if:
        /// 1. A student is accessing their own data
        /// 2. An authenticated user (advisor) may retrieve a Program Evaluation from their own list of assigned advisees if they have one of the following 4 permissions:
        /// VIEW.ASSIGNED.ADVISEES
        /// REVIEW.ASSIGNED.ADVISEES
        /// UPDATE.ASSIGNED.ADVISEES
        /// ALL.ACCESS.ASSIGNED.ADVISEES
        /// An authenticated user (advisor) may retrieve any Program Evaluation if they have one of the following 4 permissions:
        /// VIEW.ANY.ADVISEE
        /// REVIEW.ANY.ADVISEE
        /// UPDATE.ANY.ADVISEE
        /// ALL.ACCESS.ANY.ADVISEE
        /// </accessComments>
        public async Task<IEnumerable<Dtos.Planning.ProgramEvaluation4>> QueryEvaluations4Async(string id, [FromBody] List<string> programCodes)
        {
            try
            {
                // Call the coordination-layer evaluation service
                var programEvaluationEntities = await _programEvaluationService.EvaluateAsync(id, programCodes, null);

                // Get the adapter
                var programEvaluation4DtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>();

                // use adapter to map data to DTO
                var evaluations = new List<ProgramEvaluation4>();
                foreach (var evaluation in programEvaluationEntities)
                {
                    evaluations.Add(programEvaluation4DtoAdapter.MapToType(evaluation));
                }
                return evaluations;
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.Message);
                throw CreateHttpResponseException("User does not have permissions to access this student.", HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,ex.Message);
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

    }
}

