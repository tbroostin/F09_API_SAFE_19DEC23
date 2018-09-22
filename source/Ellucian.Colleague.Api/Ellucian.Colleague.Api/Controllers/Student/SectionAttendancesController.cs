// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Information about student attendances data for particular sections.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionAttendancesController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IStudentAttendanceService _studentAttendanceService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="studentAttendanceService"></param>
        /// <param name="logger"></param>
        public SectionAttendancesController(IStudentAttendanceService studentAttendanceService, ILogger logger)
        {
            _studentAttendanceService = studentAttendanceService;
            _logger = logger;
        }

        /// <summary>
        /// Update attendance information for a particular section and meeting instance.
        /// </summary>
        /// <param name="sectionAttendance"><see cref="SectionAttendance">Section Attendance</see> DTO that contains the section and the attendance information to be updated.</param>
        /// <returns><see cref="SectionAttendanceResponse">SectionAttendanceResponse</see> DTO.</returns>
        /// <accessComments>Only a faculty user who is assigned to the associated course section can update student attendance data for that course section.</accessComments>
        [HttpPut]
        public async Task<SectionAttendanceResponse> PutSectionAttendancesAsync([FromBody] SectionAttendance sectionAttendance)
        {
            if (sectionAttendance == null)
            {
                string errorText = "Must provide the section attendance to update.";
                _logger.Error(errorText);
                throw CreateHttpResponseException(errorText, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentAttendanceService.UpdateSectionAttendanceAsync(sectionAttendance);
            }
            catch (PermissionsException pe)
            {
                _logger.Info(pe.ToString());
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }

}
