// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to Student Affiliation data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentAffiliationsController : BaseCompressedApiController
    {
        private readonly IStudentAffiliationService _studentAffiliationService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the StudentAffiliationsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentAffiliationService">Repository of type <see cref="IStudentAffiliationService">IStudentAffiliationService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentAffiliationsController(IAdapterRegistry adapterRegistry, IStudentAffiliationService studentAffiliationService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentAffiliationService = studentAffiliationService;
            _logger = logger;
        }

        /// <summary>
        /// Get a list of Student Affiliations from a list of Student keys
        /// </summary>
        /// <param name="criteria">DTO Object containing List of Student Keys, Affiliation and Term.</param>
        /// <returns>List of StudentAffiliation Objects <see cref="Ellucian.Colleague.Dtos.Student.StudentTerm">StudentAffiliation</see></returns>
        [HttpPost]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.StudentAffiliation>> QueryStudentAffiliationsAsync([FromBody] StudentAffiliationQueryCriteria criteria)
        {
            if (criteria != null && criteria.StudentIds.Count() <= 0)
            {
                _logger.Error("Invalid studentIds parameter");
                throw CreateHttpResponseException("The studentIds are required.", HttpStatusCode.BadRequest);
            }
            try
            {
                return await _studentAffiliationService.QueryStudentAffiliationsAsync(criteria);

            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryStudentAffiliations error");
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}