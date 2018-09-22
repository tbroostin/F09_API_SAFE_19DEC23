/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers.FinancialAid
{
    /// <summary>
    /// Exposes Student FA Checklist Items
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentFinancialAidChecklistsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IStudentChecklistService studentChecklistService;
        private readonly ILogger logger;

        /// <summary>
        /// StudentChecklistItemController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentChecklistService">studentChecklistService</param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentFinancialAidChecklistsController(IAdapterRegistry adapterRegistry, IStudentChecklistService studentChecklistService, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.studentChecklistService = studentChecklistService;
            this.logger = logger;
        }

        /// <summary>
        /// Create a student's Financial Aid Checklist for the specified award year. 
        /// </summary>
        /// <accessComments>
        /// Users may make changes to create their own data.
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to create a checklist</param>
        /// <param name="year">The year for which to create a checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>An HttpResponseMessage with the Location header that specifies URL of the created resource, and the Content property of the object set to the created checklist</returns>
        /// <exception cref="HttpResponseException">400: Thrown if either of the arguments are null or empty, or some other error occurred while creating the checklist</exception>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to create a checklist for the specified student</exception>
        /// <exception cref="HttpResponseException">409: Thrown if a Checklist already exists for this student and year. The Location header specifies the URL to get the existing resource.</exception>"
        [HttpPost]
        public async Task<HttpResponseMessage> CreateStudentFinancialAidChecklistAsync([FromUri]string studentId, [FromUri]string year, [FromUri]bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId argument is required");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw CreateHttpResponseException("year argument is required");
            }

            try
            {
                var newStudentChecklist = await studentChecklistService.CreateStudentChecklistAsync(studentId, year, getActiveYearsOnly);
                var response = Request.CreateResponse<StudentFinancialAidChecklist>(System.Net.HttpStatusCode.Created, newStudentChecklist);
                SetResourceLocationHeader("GetStudentFinancialAidChecklistAsync", new { studentId = studentId, year = year });
                return response;
            }

            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to create a checklist resource for student {0}", studentId);
                logger.Error(pex, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }

            catch (ExistingResourceException erex)
            {
                logger.Error(erex, erex.Message);
                SetResourceLocationHeader("GetStudentFinancialAidChecklistAsync", new { studentId = studentId, id = erex.ExistingResourceId });
                throw CreateHttpResponseException("Cannot create resource that already exists. See log for details.", System.Net.HttpStatusCode.Conflict);
            }

            catch (ApplicationException ae)
            {
                logger.Error(ae, ae.Message);
                throw CreateHttpResponseException("Exception encountered while creating the FinancialAidChecklist resource: " + Environment.NewLine + ae.Message);
            }

            catch (Exception ex)
            {
                var message = "Unknown error occurred creating student checklist items";
                logger.Error(ex, message);
                throw CreateHttpResponseException(message + Environment.NewLine + ex.Message );
            }
        }

        /// <summary>
        /// Get a student's Financial Aid Checklist for a given year.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get the checklist</param>
        /// <param name="year">The year for which to get the checklist</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A StudentFinancialAidChecklist DTO</returns>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to get a checklist for the specified student</exception>
        [HttpGet]
        public async Task<StudentFinancialAidChecklist> GetStudentFinancialAidChecklistAsync(string studentId, string year, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId argument is required");
            }

            if (string.IsNullOrEmpty(year))
            {
                throw CreateHttpResponseException("year argument is required");
            }

            try
            {
                return await studentChecklistService.GetStudentChecklistAsync(studentId, year, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get checklist item resources for student {0}", studentId);
                logger.Error(pex, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = string.Format("Unknown error occurred getting StudentFinancialAidChecklist resources for id {0} awardYear", studentId, year);
                logger.Error(ex, message);
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Get a student's Financial Aid Checklists for all award years.
        /// </summary>
        /// <accessComments>
        /// Users may request their own data. Additionally, users who have
        /// VIEW.FINANCIAL.AID.INFORMATION permission or proxy permissions can request
        /// other users' data"
        /// </accessComments>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklist items</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to get active award years data only</param>
        /// <returns>A list of StudentFinancialAidChecklist DTOs</returns>
        /// <exception cref="HttpResponseException">403: Thrown if you don't have permission to get checklists for the specified student</exception>
        [HttpGet]
        public async Task<IEnumerable<StudentFinancialAidChecklist>> GetAllStudentFinancialAidChecklistsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw CreateHttpResponseException("studentId argument is required");
            }

            try
            {
                return await studentChecklistService.GetAllStudentChecklistsAsync(studentId, getActiveYearsOnly);
            }
            catch (PermissionsException pex)
            {
                var message = string.Format("You do not have permission to get StudentFinancialAidChecklist resources for student {0}", studentId);
                logger.Error(pex, message);
                throw CreateHttpResponseException(message, System.Net.HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                var message = string.Format("Unknown error occurred getting StudentFinancialAidChecklist resources for id {0}", studentId);
                logger.Error(ex, message);
                throw CreateHttpResponseException(message + Environment.NewLine + ex.Message);
            }
        }
    }
}