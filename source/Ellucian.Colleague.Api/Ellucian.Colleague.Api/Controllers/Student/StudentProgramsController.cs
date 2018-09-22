// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Filters;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to AcademicHistory data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentProgramsController : BaseCompressedApiController
    {
        private readonly IStudentProgramService _studentProgramService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the CoursesController class.
        /// </summary>
        /// <param name="service">Service of type <see cref="IStudentProgramService">IStudentProgramService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentProgramsController(IStudentProgramService service, ILogger logger)
        {
            _studentProgramService = service;
            _logger = logger;
        }

        /// <summary>
        /// get student programs from a list of Ids
        /// </summary>
        /// <param name="criteria">Criteria Object containing Student IDs and term</param>
        /// <returns>Student Programs DTO Objects</returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentProgram2>> QueryStudentProgramsAsync([FromBody] StudentProgramsQueryCriteria criteria)
        {
            if (criteria.StudentIds == null)
            {
                _logger.Error("Invalid studentIds");
                throw CreateHttpResponseException("At least one Student Id is required.", HttpStatusCode.BadRequest);
            }
            if (!criteria.IncludeInactivePrograms && criteria.IncludeHistory)
            {
                _logger.Error("Conflict between IncludeInactivePrograms and IncludeHistory parameters.");
                throw CreateHttpResponseException("Cannot exclude inactive programs when requesting historical data.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentProgramService.GetStudentProgramsByIdsAsync(criteria.StudentIds, criteria.IncludeInactivePrograms, criteria.Term, criteria.IncludeHistory);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}
