//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
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
    /// Provides access to StudentTagAssignments
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof (EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTagAssignmentsController : BaseCompressedApiController
    {
        
        private readonly ILogger _logger;
        /// <summary>
        /// Initializes a new instance of the StudentTagAssignmentsController class.
        /// </summary>
        /// <param name="logger">Interface to logger</param>
        public StudentTagAssignmentsController(ILogger logger)
        {
            _logger = logger;
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Retrieves all student-tag-assignments
        /// </summary>
        /// <returns>All <see cref="Dtos.StudentTagAssignments">StudentTagAssignments</see></returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentTagAssignments))]
        public async Task<IEnumerable<StudentTagAssignments>> GetStudentTagAssignmentsAsync()
        {
            return new List<StudentTagAssignments>();
        }
        /// <summary>
        /// Retrieve (GET) an existing student-tag-assignments
        /// </summary>
        /// <param name="guid">GUID of the student-tag-assignments to get</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        public async Task<Dtos.StudentTagAssignments> GetStudentTagAssignmentsByGuidAsync([FromUri] string guid)
        {
            try
            {
                throw new ColleagueWebApiException(string.Format("No student-tag-assignments was found for guid {0}.", guid));
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
        }
        /// <summary>
        /// Create (POST) a new studentTagAssignments
        /// </summary>
        /// <param name="studentTagAssignments">DTO of the new studentTagAssignments</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost]
        public async Task<Dtos.StudentTagAssignments> PostStudentTagAssignmentsAsync([FromBody] Dtos.StudentTagAssignments studentTagAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        /// <summary>
        /// Update (PUT) an existing studentTagAssignments
        /// </summary>
        /// <param name="guid">GUID of the studentTagAssignments to update</param>
        /// <param name="studentTagAssignments">DTO of the updated studentTagAssignments</param>
        /// <returns>A studentTagAssignments object <see cref="Dtos.StudentTagAssignments"/> in EEDM format</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        public async Task<Dtos.StudentTagAssignments> PutStudentTagAssignmentsAsync([FromUri] string guid, [FromBody] Dtos.StudentTagAssignments studentTagAssignments)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
        /// <summary>
        /// Delete (DELETE) a studentTagAssignments
        /// </summary>
        /// <param name="guid">GUID to desired studentTagAssignments</param>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpDelete]
        public async Task DeleteStudentTagAssignmentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }
    }
}