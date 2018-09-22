// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to nonacademic attendance data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NonAcademicAttendancesController : BaseCompressedApiController
    {
        private readonly INonAcademicAttendanceService _nonAcademicAttendanceService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NonAcademicAttendancesController class.
        /// </summary>
        /// <param name="nonAcademicAttendanceService">Service of type <see cref="INonAcademicAttendanceService">INonAcademicAttendanceService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public NonAcademicAttendancesController(INonAcademicAttendanceService nonAcademicAttendanceService, ILogger logger)
        {
            _nonAcademicAttendanceService = nonAcademicAttendanceService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person
        /// </summary>
        /// <param name="studentId">Unique identifier for the person whose nonacademic attendances are being retrieved</param>
        /// <returns>All <see cref="NonAcademicAttendance">nonacademic events attended</see> for a person</returns>
        public async Task<IEnumerable<NonAcademicAttendance>> GetNonAcademicAttendancesAsync(string studentId)
        {
            try
            {
                return await _nonAcademicAttendanceService.GetNonAcademicAttendancesAsync(studentId);
            }
            catch (PermissionsException pex)
            {
                _logger.Error(pex.ToString());
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (ColleagueDataReaderException cdre)
            {
                string message = "An error occurred while trying to read nonacademic attendance data from the database.";
                _logger.Error(message, cdre.ToString());
                throw CreateHttpResponseException(message);
            }
            catch (Exception ex)
            {
                string message = "An error occurred while trying to retrieve nonacademic attendance information.";
                _logger.Error(message, ex.ToString());
                throw CreateHttpResponseException(message);
            }
        }
    }
}