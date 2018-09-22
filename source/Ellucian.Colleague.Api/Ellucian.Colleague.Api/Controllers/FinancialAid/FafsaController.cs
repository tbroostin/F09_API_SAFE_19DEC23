/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes FAFSA data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FafsaController : BaseCompressedApiController
    {
        private readonly IFafsaService fafsaService;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Constructor for the FafsaController
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="fafsaService">fafsaService</param>
        /// <param name="logger">Logger</param>
        public FafsaController(IAdapterRegistry adapterRegistry, IFafsaService fafsaService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.fafsaService = fafsaService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves many FAFSA objects at once using a FafsaQueryCriteria object. This endpoint gets the federally flagged FAFSA 
        /// object for each student/awardYear.
        /// </summary>
        /// <param name="criteria">criteria object including a comma delimited list of IDs from request body</param>
        /// <returns>List of <see cref="Fafsa">Objects</see></returns>
        [HttpPost]
        public async Task<IEnumerable<Fafsa>> QueryFafsaByPostAsync([FromBody] FafsaQueryCriteria criteria)
        {
            try
            {
                return await fafsaService.QueryFafsaAsync(criteria);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a list of all FAFSAs for the given student id
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have VIEW.FINANCIAL.AID.INFORMATION permission 
        /// or proxy permissions can request other users' data
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id for whom to retrieve FAFSAs</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of FAFSA objects assigned to the student</returns>
        [HttpGet]
        public async Task<IEnumerable<Fafsa>> GetStudentFafsasAsync([FromUri]string studentId, [FromUri]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("StudentId is required in request");
            }
            try
            {
                return await fafsaService.GetStudentFafsasAsync(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex, "Permisions exception getting data for student {0}", studentId);
                throw CreateHttpResponseException("You do not have permission to get FAFSAs for this student", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting FAFSAs for student {0}", studentId);
                throw CreateHttpResponseException("Unknown error occurred. See log for details.");
            }
        }
    }
}