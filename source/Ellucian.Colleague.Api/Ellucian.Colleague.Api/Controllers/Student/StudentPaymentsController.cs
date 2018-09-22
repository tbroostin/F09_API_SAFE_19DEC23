// Copyright 2017-18 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Security;
using System.Net.Http;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http;
using Newtonsoft.Json.Linq;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// The controller for student payments for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Authorize]
    public class StudentPaymentsController : BaseCompressedApiController
    {
        private readonly IStudentPaymentService studentPaymentService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the StudentPaymentController object
        /// </summary>
        /// <param name="studentPaymentService">student payments service object</param>
        /// <param name="logger">Logger object</param>
        public StudentPaymentsController(IStudentPaymentService studentPaymentService, ILogger logger)
        {
            this.studentPaymentService = studentPaymentService;
            this.logger = logger;
        }

        #region Student payments V6

        /// <summary>
        /// Retrieves a specified student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns>A StudentPayment DTO</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentPayment> GetByIdAsync([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var studentPayment = await studentPaymentService.GetByIdAsync(id);

                if (studentPayment != null)
                {

                    AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentPayment.Id }));
                }

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student payments for the data model version 6
        /// </summary>
        /// <returns>A Collection of StudentPayments</returns>
        [HttpGet]
        [ValidateQueryStringFilter(new string[] { "student", "academicPeriod", "accountingCode", "paymentType" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAsync(Paging page, [FromUri] string student = "", string academicPeriod = "", string accountingCode = "", string paymentType = "")
        {

            string criteria = string.Concat(student, academicPeriod, accountingCode, paymentType);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentPayment>>(new List<Dtos.StudentPayment>(), page, 0, this.Request);
            }

            if (student == null || academicPeriod == null || accountingCode == null || paymentType == null)
                // null vs. empty string means they entered a filter with no criteria and we should return an empty set.
                return new PagedHttpActionResult<IEnumerable<Dtos.StudentPayment>>(new List<Dtos.StudentPayment>(), page, 0, this.Request);

            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            if (page == null)
            {
                page = new Paging(200, 0);
            }
            try
            {

                var pageOfItems = await studentPaymentService.GetAsync(page.Offset, page.Limit, bypassCache, student, academicPeriod, accountingCode, paymentType);

                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentPayment>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <param name="studentPaymentDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [HttpPut]
        public async Task<Dtos.StudentPayment> UpdateAsync([FromUri] string id, [FromBody] Dtos.StudentPayment studentPaymentDto)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create a single student payment for the data model version 6
        /// </summary>
        /// <param name="studentPaymentDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentPayment> CreateAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentPayment studentPaymentDto)
        {
            try
            {
                if (studentPaymentDto == null)
                {
                    throw new ArgumentNullException("studentPaymentDto", "The request body is required.");
                }
                if (studentPaymentDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }
                ValidateStudentPayments(studentPaymentDto);

                //call import extend method that needs the extracted extension data and the config
                await studentPaymentService.ImportExtendedEthosData(await ExtractExtendedData(await studentPaymentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student charge
                var studentPayment = await studentPaymentService.CreateAsync(studentPaymentDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentPayment.Id }));

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Helper method to validate Student Payments.
        /// </summary>
        /// <param name="studentPayment">student payment DTO object of type <see cref="Dtos.StudentPayment"/></param>
        private void ValidateStudentPayments(Dtos.StudentPayment studentPayment)
        {
            if (studentPayment.AcademicPeriod == null)
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period is required when submitting a student payment. ");
            }
            if (studentPayment.AcademicPeriod != null && string.IsNullOrEmpty(studentPayment.AcademicPeriod.Id))
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period id is required when submitting a student payment. ");
            }
            if (studentPayment.Amount == null)
            {
                throw new ArgumentNullException("studentPayments.paymentAmount", "The payment amount cannot be null when submitting a student payment. ");
            }
            if (studentPayment.Amount != null && (studentPayment.Amount.Value == 0 || studentPayment.Amount.Value == null))
            {
                throw new ArgumentNullException("studentPayments.paymentAmount.value", "A student-payments in the amount of zero dollars is not permitted. ");
            }
            if (studentPayment.Amount != null && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentPayments.amount.currency");
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                throw new ArgumentException("The paymentType is either invalid or empty and is required when submitting a student payment. ", "studentPayments.paymentType");
            }
            if (studentPayment.Person == null || string.IsNullOrEmpty(studentPayment.Person.Id))
            {
                throw new ArgumentNullException("studentPayments.student.id", "The student id is required when submitting a student payment. ");
            }
            if (studentPayment.AccountingCode != null && string.IsNullOrEmpty(studentPayment.AccountingCode.Id))
            {
                throw new ArgumentException("The accountingCode requires an id when submitting student payments. ", "studentPayments.accountingCode.id");
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.sponsor && studentPayment.AccountingCode == null)
            {
                throw new ArgumentNullException("studentPayments.accountingCode", "The accountingCode is required when submitting sponsor payments. ");
            }
        }

        #endregion

        #region Student payments V11

        /// <summary>
        /// Retrieves a specified student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns>A StudentPayment DTO</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentPayment2> GetByIdAsync2([FromUri] string id)
        {
            bool bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                var studentPayment = await studentPaymentService.GetByIdAsync2(id);

                if (studentPayment != null)
                {

                    AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentPayment.Id }));
                }

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student payments for the data model version 11
        /// </summary>
        /// <returns>A Collection of StudentPayments</returns>
        [HttpGet]
        [ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentPayment2)), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAsync2(Paging page, QueryStringFilter criteria)
        {
            string student = "", academicPeriod = "", fundSource = "", fundDestination = "", paymentType = "";

            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }
                if (page == null)
                {
                    page = new Paging(200, 0);
                }

                var rawFilterData = GetFilterObject<Dtos.StudentPayment2>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentPayment2>>(new List<Dtos.StudentPayment2>(), page, 0, this.Request);

                if (rawFilterData != null)
                {
                    student = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    academicPeriod = rawFilterData.AcademicPeriod != null ? rawFilterData.AcademicPeriod.Id : null;
                    fundSource = rawFilterData.FundingSource != null ? rawFilterData.FundingSource.Id : null;
                    fundDestination = rawFilterData.FundingDestination != null ? rawFilterData.FundingDestination.Id : null;
                    paymentType = rawFilterData.PaymentType.ToString();
                    if (paymentType == Dtos.EnumProperties.StudentPaymentTypes.notset.ToString())
                    {
                        paymentType = string.Empty;
                    }
                }

                var pageOfItems = await studentPaymentService.GetAsync2(page.Offset, page.Limit, bypassCache, student, academicPeriod, fundSource, paymentType, fundDestination);

                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                           await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                           pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentPayment2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Create a single student payment for the data model version 6
        /// </summary>
        /// <param name="studentPaymentDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentPayment</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentPayment2> CreateAsync2([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentPayment2 studentPaymentDto)
        {

            try
            {
                if (studentPaymentDto == null)
                {
                    throw new ArgumentNullException("studentPaymentDto", "The request body is required.");
                }
                if (studentPaymentDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }
                ValidateStudentPayments2(studentPaymentDto);

                //call import extend method that needs the extracted extension data and the config
                await studentPaymentService.ImportExtendedEthosData(await ExtractExtendedData(await studentPaymentService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student payment
                var studentPayment = await studentPaymentService.CreateAsync2(studentPaymentDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentPaymentService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentPaymentService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentPayment.Id }));

                return studentPayment;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student payment");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Helper method to validate Student Payments.
        /// </summary>
        /// <param name="studentPayment">student payment DTO object of type <see cref="Dtos.StudentPayment2"/></param>
        private void ValidateStudentPayments2(Dtos.StudentPayment2 studentPayment)
        {
            if (studentPayment.AcademicPeriod == null)
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period is required when submitting a student payment. ");
            }
            if (studentPayment.AcademicPeriod != null && string.IsNullOrEmpty(studentPayment.AcademicPeriod.Id))
            {
                throw new ArgumentNullException("studentPayments.academicPeriod", "The academic period id is required when submitting a student payment. ");
            }
            if (studentPayment.Amount == null)
            {
                throw new ArgumentNullException("studentPayments.paymentAmount", "The payment amount cannot be null when submitting a student payment. ");
            }
            if (studentPayment.Amount != null && (studentPayment.Amount.Value == 0 || studentPayment.Amount.Value == null))
            {
                throw new ArgumentNullException("studentPayments.paymentAmount.value", "A student-payments in the amount of zero dollars is not permitted. ");
            }
            if (studentPayment.Amount != null && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.USD && studentPayment.Amount.Currency != Dtos.EnumProperties.CurrencyCodes.CAD)
            {
                throw new ArgumentException("The currency code must be set to either 'USD' or 'CAD'. ", "studentPayments.amount.currency");
            }
            if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.notset)
            {
                throw new ArgumentException("The paymentType is either invalid or empty and is required when submitting a student payment. ", "studentPayments.paymentType");
            }
            if (studentPayment.Person == null || string.IsNullOrEmpty(studentPayment.Person.Id))
            {
                throw new ArgumentNullException("studentPayments.student.id", "The student id is required when submitting a student payment. ");
            }
            if (studentPayment.FundingSource != null && string.IsNullOrEmpty(studentPayment.FundingSource.Id))
            {
                throw new ArgumentException("The fundingSource requires an id when submitting student payments. ", "studentPayments.fundingSource.id");
            }
            //if (studentPayment.PaymentType == Dtos.EnumProperties.StudentPaymentTypes.sponsor && studentPayment.FundingDestination == null)
            //{
            //    throw new ArgumentNullException("studentPayments.fundingDestination", "The fundingDestination is required when submitting sponsor payments. ");
            //}
            //if (studentPayment.GlPosting == Dtos.EnumProperties.GlPosting.NotSet)
            //{
            //    throw new ArgumentNullException("studentPayments.generalLedgerPosting", "The generalLedgerPosting is required when submitting a sponsor payment.");
            //}
        }

        #endregion

        /// <summary>
        /// Delete a single student payment for the data model version 6
        /// </summary>
        /// <param name="id">The requested student payment GUID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAsync([FromUri] string id)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
    }
}