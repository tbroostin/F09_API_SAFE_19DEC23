// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Api.Licensing;
using slf4net;

using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Student.Services;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Student Academic term data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentTermsController : BaseCompressedApiController
    {
        private readonly IStudentTermService _studentTermService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentStandingsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentTermService">Repository of type <see cref="IStudentTermService">IStudentTermService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentTermsController(IAdapterRegistry adapterRegistry, IStudentTermService studentTermService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentTermService = studentTermService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of Student Terms from a list of Student keys
        /// </summary>
        /// <param name="criteria">DTO Object containing List of Student Keys, Academic Level and Term.</param>
        /// <returns>List of StudentTerm Objects <see cref="Ellucian.Colleague.Dtos.Student.StudentTerm">StudentTerm</see></returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentTerm>> QueryStudentTermsAsync([FromBody] StudentTermsQueryCriteria criteria)
        {
            if (criteria.StudentIds.Count() <= 0)
            {
                _logger.Error("Invalid studentIds parameter");
                throw CreateHttpResponseException("The studentIds are required.", HttpStatusCode.BadRequest);
            }
            try
            {
                return(await _studentTermService.QueryStudentTermsAsync(criteria));

            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryStudentTerms error");
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Gets Pilot student term GPA based on query criteria of studentIds and term.
        /// </summary>
        /// <param name="criteria">Contains selection criteria:
        /// StudentIds: List of IDs.
        /// Term: Term filter for academic history</param>
        /// <returns>PilotAcademicHistoryLevel DTO Objects, including cumulative term GPA for selected credits</returns>
        /// <accessComments>
        /// Users with VIEW.STUDENT.INFORMATION permission can retrieve Pilot student term GPA.
        /// </accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.PilotStudentTermLevelGpa>> QueryPilotStudentTermsGpaAsync([FromBody] StudentTermsQueryCriteria criteria)
        {
            if (criteria.StudentIds.Count() <= 0)
            {
                _logger.Error("Invalid studentIds parameter");
                throw CreateHttpResponseException("The studentIds are required.", HttpStatusCode.BadRequest);
            }
            if (criteria.Term.Count() <= 0)
            {
                _logger.Error("Invalid term parameter");
                throw CreateHttpResponseException("The term is required.", HttpStatusCode.BadRequest);
            }
            var studentIds = criteria.StudentIds;
            var term = criteria.Term;
            try
            {
                var pilotStudentTermLevelGpaCollection = await _studentTermService.QueryPilotStudentTermsGpaAsync(studentIds, term);
                return pilotStudentTermLevelGpaCollection;
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Query Student Terms  GPA error");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}