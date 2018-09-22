using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
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
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]

    public class FacultyContractsController : BaseCompressedApiController
    {
        private readonly IFacultyContractService _facultyContractService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the FacultyContractcontroller class.
        /// </summary>
        /// <param name="facultyContractService">Service of type<see cref="IFacultyContractService">IFacultyContractService</see></param>
        /// <param name="logger">Interface to logger</param>
        public FacultyContractsController(IFacultyContractService facultyContractService, ILogger logger)
        {
            _facultyContractService = facultyContractService;
            _logger = logger;
        }

        /// <summary>
        /// Query Faculty Contracts
        /// </summary>
        /// <param name="facultyId">Id of the faculty member</param>
        /// <returns></returns>
        /// <accessComments>
        /// Only the current user can get their own faculty contracts. 
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<FacultyContract>> GetFacultyContractsAsync(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw CreateHttpResponseException("Faculty id required to retrieve faculty contracts");
            }
            
            try
            {
                var facultyContracts = await _facultyContractService.GetFacultyContractsByFacultyIdAsync(facultyId);
                return facultyContracts;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e, "User does not have permission to retrieve faculty ID " + facultyId);
                throw CreateHttpResponseException("You are not authorized to retrieve this contract", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to retrieve faculty contracts");
                throw CreateHttpResponseException("Failed to retrieve faculty contracts");
            }

            
        }
    }
}
