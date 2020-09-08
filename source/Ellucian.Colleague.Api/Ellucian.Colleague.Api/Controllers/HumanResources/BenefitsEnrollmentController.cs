﻿/*Copyright 2019-2020 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Security;
using System.Net;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Api.Utility;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    ///  BenefitsEnrollment controller
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class BenefitsEnrollmentController : BaseCompressedApiController
    {
        private readonly IBenefitsEnrollmentService benefitsEnrollmentService;
        private readonly ILogger logger;
        private const string noEmployeeIDErrorMessage = "EmployeeId is required.";
        private const string noBenefitEnrollmentPoolDTOErrorMessage = "Benefit Enrollment Pool DTO is required in body of request";
        private const string organizationOrLastNameRequiredMessage = "OrganizationName or LastName is required.";
        private const string benefitElectionsActionFailureMessage = "Unable to submit/reopen the benefit elections.";
        private const string noBenefitEnrollmentCompletionCriteriaErrorMessage = "BenefitEnrollmentCompletionCriteria DTO is required in the body of the request";
        private const string invalidBenefitEnrollmentCompletionCriteriaErrorMessage = "Required parameters of BenefitEnrollmentCompletionCriteria DTO contain invalid values.";
        private const string beneficiaryCategoryFailureMessage = "Unable to get beneficiary categories";
        private const string forbiddenSubmitOrReopenErrorMessage = "User does not have the permission to submit/re-open the elected benefits of others";
        /// <summary>
        /// Initializes a new instance of the BenefitsEnrollmentController class
        /// </summary>
        /// <param name="benefitsEnrollmentService">Service of type <see cref="IBenefitsEnrollmentService">IBenefitsEnrollmentService</see></param>
        /// <param name="logger">Interface to logger</param>
        public BenefitsEnrollmentController(IBenefitsEnrollmentService benefitsEnrollmentService, ILogger logger)
        {
            this.benefitsEnrollmentService = benefitsEnrollmentService;
            this.logger = logger;
        }

        /// <summary>
        /// Returns benefits enrollment eligibility for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment eligibility</param>
        /// <returns>EmployeeBenefitsEnrollmentEligibility dto containing the enrollment period if eligible or a reson for ineligibility.<see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentEligibility"></see> </returns>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment eligibility.
        /// </accessComments>
        [HttpGet]
        public async Task<EmployeeBenefitsEnrollmentEligibility> GetEmployeeBenefitsEnrollmentEligibilityAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                return await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentEligibilityAsync(employeeId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, "User does not have permission to access EmployeeBenefitsEnrollmentEligibility for " + employeeId);
                throw CreateHttpResponseException("You are not authorized to retrieve EmployeeBenefitsEnrollmentEligibility", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to get EmployeeBenefitsEnrollmentEligibility details";
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Returns benefits enrollment pool items (dependent and beneficiary information) for an employee
        /// </summary>
        /// <param name="employeeId">Id of employee to request benefits enrollment pool items</param>
        /// <returns>List of EmployeeBenefitsEnrollmentPoolItem dtos containing the dependent and beneficiary information for employee <see cref="Dtos.HumanResources.EmployeeBenefitsEnrollmentPoolItem"></see></returns>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment pool items
        /// </accessComments>
        [HttpGet]
        public async Task<IEnumerable<EmployeeBenefitsEnrollmentPoolItem>> GetEmployeeBenefitsEnrollmentPoolAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new ArgumentNullException("employeeId");
            }
            try
            {
                return await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentPoolAsync(employeeId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, "User does not have permission to access EmployeeBenefitsEnrollmentPool for " + employeeId);
                throw CreateHttpResponseException("You are not authorized to retrieve EmployeeBenefitsEnrollmentPool information", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to get EmployeeBenefitsEnrollmentPool details";
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// Gets EmployeeBenefitsEnrollmentPackage object for the specified employee id
        /// </summary>
        /// <param name="employeeId">employee id for whom to get benefits enrollment package</param>
        /// <param name="enrollmentPeriodId">(optional) enrollment perod id</param>
        /// <accessComments>
        /// An authenticated user can view their own benefits enrollment package
        /// </accessComments>
        /// <returns>EmployeeBenefitsEnrollmentPackage DTO</returns>
        [HttpGet]
        public async Task<EmployeeBenefitsEnrollmentPackage> GetEmployeeBenefitsEnrollmentPackageAsync(string employeeId, string enrollmentPeriodId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(employeeId))
                {
                    throw new ArgumentNullException("employeeId");
                }
                return await benefitsEnrollmentService.GetEmployeeBenefitsEnrollmentPackageAsync(employeeId);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, "User does not have permission to access EmployeeBenefitsEnrollmentPackage for " + employeeId);
                throw CreateHttpResponseException("You are not authorized to retrieve EmployeeBenefitsEnrollmentPackage information", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                var message = "Unable to get EmployeeBenefitsEnrollmentPackage details";
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(message);
            }
        }

        /// <summary>
        /// This endpoint will adds new benefits enrollment pool information to an employee
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can add benefits enrollment pool information.     
        /// The endpoint will reject the add of benefits enrollment pool information if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to add benefits enrollment pool information to an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Newly added EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId or employeeBenefitsEnrollmentPoolItem are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeBenefitsEnrollmentPoolItem is not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
        [HttpPost]
        public async Task<EmployeeBenefitsEnrollmentPoolItem> AddEmployeeBenefitsEnrollmentPoolAsync([FromUri]string employeeId, [FromBody]EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {
                throw CreateHttpResponseException(noBenefitEnrollmentPoolDTOErrorMessage);
            }

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                throw CreateHttpResponseException(organizationOrLastNameRequiredMessage);
            }

            try
            {
                return await benefitsEnrollmentService.AddEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
            }
            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment pool information of an employee
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own benefits enrollment pool information .     
        /// The endpoint will reject the updated benefits enrollment pool information if the employee does not have a valid permission.
        /// </accessComments>       
        /// <param name="employeeId">Required parameter to update benefits enrollment pool information of an employee</param>
        /// <param name="employeeBenefitsEnrollmentPoolItem"><see cref="EmployeeBenefitsEnrollmentPoolItem">EmployeeBenefitsEnrollmentPoolItem DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentPoolItem">Updated EmployeeBenefitsEnrollmentPoolItem DTO object</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the employeeId, employeeBenefitsEnrollmentPoolItem or employeeBenefitsEnrollmentPoolItem.Id are not present in the request or any unexpected error has occured.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if the user is not allowed to add benefits enrollment pool information.</exception>
        [HttpPut]
        public async Task<EmployeeBenefitsEnrollmentPoolItem> UpdateEmployeeBenefitsEnrollmentPoolAsync([FromUri]string employeeId, [FromBody]EmployeeBenefitsEnrollmentPoolItem employeeBenefitsEnrollmentPoolItem)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentPoolItem == null)
            {

                throw CreateHttpResponseException(noBenefitEnrollmentPoolDTOErrorMessage);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentPoolItem.Id))
            {
                string message = "Benefit Enrollment Pool Id is required in body of request";
                throw CreateHttpResponseException(message);
            }

            if (string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.OrganizationName) && string.IsNullOrWhiteSpace(employeeBenefitsEnrollmentPoolItem.LastName))
            {
                throw CreateHttpResponseException(organizationOrLastNameRequiredMessage);
            }

            try
            {
                return await benefitsEnrollmentService.UpdateEmployeeBenefitsEnrollmentPoolAsync(employeeId, employeeBenefitsEnrollmentPoolItem);
            }

            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }

            catch (KeyNotFoundException knfe)
            {
                logger.Error(knfe, knfe.Message);
                throw CreateNotFoundException("BenefitsEnrollmentPool", employeeBenefitsEnrollmentPoolItem.Id);
            }

            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This endpoint will update benefits enrollment information of an employee for the given benefit types specified
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can update their own benefits enrollment information
        /// The endpoint will reject the updated benefits enrollment information if the employee does not have a valid permission
        /// </accessComments>    
        /// <param name="employeeId">Required parameter to update benefits enrollment information</param>
        /// <param name="employeeBenefitsEnrollmentInfo"><see cref="EmployeeBenefitsEnrollmentInfo">EmployeeBenefitsEnrollmentInfo DTO</see></param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentInfo">Updated EmployeeBenefitsEnrollmentInfo DTO object</see></returns>
        [HttpPut]
        public async Task<EmployeeBenefitsEnrollmentInfo> UpdateEmployeeBenefitsEnrollmentInfoAsync([FromUri]string employeeId, [FromBody]EmployeeBenefitsEnrollmentInfo employeeBenefitsEnrollmentInfo)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw CreateHttpResponseException(noEmployeeIDErrorMessage);
            }

            if (employeeBenefitsEnrollmentInfo == null)
            {
                string message = "EmployeeBenefitsEnrollmentInfo DTO is required in body of request";
                throw CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.EmployeeId))
            {
                string message = "EmployeeId is required in body of request";
                throw CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.EnrollmentPeriodId))
            {
                string message = "EnrollmentPeriodId is required in body of request";
                throw CreateHttpResponseException(message);
            }

            if (string.IsNullOrEmpty(employeeBenefitsEnrollmentInfo.BenefitPackageId))
            {
                string message = "BenefitPackageId is required in body of request";
                throw CreateHttpResponseException(message);
            }

            try
            {
                return await benefitsEnrollmentService.UpdateEmployeeBenefitsEnrollmentInfoAsync(employeeBenefitsEnrollmentInfo);
            }

            catch (PermissionsException ex)
            {
                var message = string.Format(ex.Message);
                logger.Error(ex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Forbidden);
            }

            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries enrollment period benefits based on specified criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <accessComments>Any authenticated user can get enrollment period benefits information
        /// </accessComments>
        /// <returns>Set of enrollment period benefits</returns>
        [HttpPost]
        public async Task<IEnumerable<EnrollmentPeriodBenefit>> QueryEnrollmentPeriodBenefitsAsync(BenefitEnrollmentBenefitsQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria");
                }
                var benefits = await benefitsEnrollmentService.QueryEnrollmentPeriodBenefitsAsync(criteria);
                return benefits;
            }
            catch (RepositoryException re)
            {
                logger.Error(re, re.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(re));
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Unable to get enrollment period benefits", HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to get enrollment period benefits", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Queries benefits enrollment information based on specified criteria; if no benefit type is provided, all of the employee's elected benefit information is returned
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can view their own benefits enrollment information
        /// </accessComments>    
        /// <param name="criteria">Required parameter used to query employee benefits enrollment information</param>
        /// <returns><see cref="EmployeeBenefitsEnrollmentInfo">EmployeeBenefitsEnrollmentInfo DTO object</see></returns>
        [HttpPost]
        public async Task<EmployeeBenefitsEnrollmentInfo> QueryEmployeeBenefitsEnrollmentInfoAsync(EmployeeBenefitsEnrollmentInfoQueryCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria");
                }
                var benefits = await benefitsEnrollmentService.QueryEmployeeBenefitsEnrollmentInfoAsync(criteria);
                return benefits;
            }
            catch (RepositoryException re)
            {
                logger.Error(re, re.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(re));
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, ane.Message);
                throw CreateHttpResponseException("Unable to get benefits enrollment information", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException pe)
            {
                logger.Error(pe, pe.Message);
                throw CreateHttpResponseException("You are not authorized to retrieve Employee Benefits Enrollment Info", HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to get benefits enrollment information", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// This end-point submits/re-opens the benefits elected by an employee.
        /// A boolean flag present in the input criteria object indicates whether to submit or re-open the benefit elections.
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can submit/re-open their own elected benefits.      
        /// </accessComments>   
        /// <param name="criteria">BenefitEnrollmentCompletionCriteria object</param>
        /// <returns><see cref="BenefitEnrollmentCompletionInfo">BenefitEnrollmentCompletionInfo DTO</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the required parameters in the input object has no value (or) in case of any unexpected error while processing the request.</exception>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.Forbidden returned if a user tries to submit/re-open elected benefits of others.</exception>
        [HttpPost]
        public async Task<BenefitEnrollmentCompletionInfo> SubmitOrReOpenBenefitElectionsAsync([FromBody]BenefitEnrollmentCompletionCriteria criteria)
        {
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "BenefitEnrollmentCompletionCriteria is required");
                }

                if (string.IsNullOrWhiteSpace(criteria.EmployeeId))
                {
                    throw new ArgumentException("Employee Id is required");
                }
                if (string.IsNullOrWhiteSpace(criteria.EnrollmentPeriodId))
                {
                    throw new ArgumentException("Enrollment Period Id is required");
                }
                if (criteria.SubmitBenefitElections && string.IsNullOrWhiteSpace(criteria.BenefitsPackageId))
                {
                    throw new ArgumentException("BenefitsPackageId is required");
                }

                var benefitsEnrollmentCompletionInfo = await benefitsEnrollmentService.SubmitOrReOpenBenefitElectionsAsync(criteria);
                return benefitsEnrollmentCompletionInfo;
            }
            catch (ArgumentNullException ane)
            {
                logger.Error(ane, noBenefitEnrollmentCompletionCriteriaErrorMessage);
                throw CreateHttpResponseException(noBenefitEnrollmentCompletionCriteriaErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (ArgumentException ae)
            {
                logger.Error(ae, invalidBenefitEnrollmentCompletionCriteriaErrorMessage);
                throw CreateHttpResponseException(invalidBenefitEnrollmentCompletionCriteriaErrorMessage, HttpStatusCode.BadRequest);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex, forbiddenSubmitOrReopenErrorMessage);
                throw CreateHttpResponseException(forbiddenSubmitOrReopenErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(benefitElectionsActionFailureMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Gets the beneficiary categories/types
        /// </summary>
        ///  <accessComments>
        /// Any authenticated user can retrieve beneficiary category information.
        /// </accessComments> 
        /// <returns>Returns a list of Beneficiary Category DTO objects</returns>

        [HttpGet]
        public async Task<IEnumerable<BeneficiaryCategory>> GetBeneficiaryCategoriesAsync()
        {
            try
            {
                return await benefitsEnrollmentService.GetBeneficiaryCategoriesAsync();
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException(beneficiaryCategoryFailureMessage, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get benefits enrollment acknowledgement report
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can download benefits acknowledgement report.      
        /// </accessComments>  
        /// <param name="employeeId"></param>
        /// <returns>The pdf report of enrolled benefits information</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetBenefitsEnrollmentAcknowledgementReportAsync(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw CreateHttpResponseException("Employee id must be specified.");
            }

            try
            {
                var path = HttpContext.Current.Server.MapPath("~/Reports/HumanResources/BenefitsEnrollmentAcknowledgement.rdlc");

                var resourceFilePath = HttpContext.Current.Server.MapPath("~/App_GlobalResources/HumanResources/BenefitsEnrollment.resx");

                var renderedBytes = await benefitsEnrollmentService.GetBenefitsInformationForAcknowledgementReport(employeeId, path, resourceFilePath);

                var fileNameString = string.Format("Open Enrollment Benefits {0}", DateTime.Now.ToString("MMddyyyy HH:mm:ss"));

                var response = new HttpResponseMessage();

                response.Content = new ByteArrayContent(renderedBytes);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Regex.Replace(fileNameString, "[^a-zA-Z0-9_]", "_") + ".pdf"
                };
                response.Content.Headers.ContentLength = renderedBytes.Length;

                return response;
            }
            catch (ArgumentException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Parameters are not valid. See log for details.", HttpStatusCode.BadRequest);
            }
            catch (PermissionsException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Access to Benefits Enrollment is forbidden. See log for details.", HttpStatusCode.Forbidden);
            }
            catch (ApplicationException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Benefits Enrollment Acknowledgement could not be generated. See log for details.");
            }
            catch (InvalidOperationException ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Invalid operation based on state of Benefits Enrollment resource. See log for details.", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw CreateHttpResponseException("Unknown error occurred while getting Benefits Enrollment Acknowledgement resource. See log for details.");
            }
        }
    }
}