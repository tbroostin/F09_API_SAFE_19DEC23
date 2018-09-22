// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to EducationHistory data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class EducationHistoryController : BaseCompressedApiController
    {
        private readonly IEducationHistoryService _educationHistoryService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the EducationHistoryController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="educationHistoryService">Service of type <see cref="IEducationHistoryService">IEducationHistoryService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public EducationHistoryController(IAdapterRegistry adapterRegistry, IEducationHistoryService educationHistoryService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _educationHistoryService = educationHistoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get Education History from a list of Student keys
        /// </summary>
        /// <param name="criteria">DTO Object containing a list of Student Keys for selection</param>
        /// <returns>List of EducationHistory Objects <see cref="Ellucian.Colleague.Dtos.Student.EducationHistory">EducationHistory</see></returns>
        /// <accessComments>
        /// Users with VIEW.STUDENT.INFORMATION permission can access education history for given student Ids.
        /// </accessComments>
        [HttpPost]
        public  async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.EducationHistory>> QueryEducationHistoryAsync([FromBody] EducationHistoryQueryCriteria criteria)
        {
            try
            {
                return await _educationHistoryService.QueryEducationHistoryByIdsAsync(criteria.StudentIds);
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
