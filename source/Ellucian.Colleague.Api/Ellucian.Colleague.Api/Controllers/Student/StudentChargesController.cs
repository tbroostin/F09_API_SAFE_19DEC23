// Copyright 2016-18 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Http.Models;
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
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// The controller for student charges for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    [Authorize]
    public class StudentChargesController : BaseCompressedApiController
    {
        private readonly IStudentChargeService studentChargeService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the StudentChargeController object
        /// </summary>
        /// <param name="studentChargeService">student charges service object</param>
        /// <param name="logger">Logger object</param>
        public StudentChargesController(IStudentChargeService studentChargeService, ILogger logger)
        {
            this.studentChargeService = studentChargeService;
            this.logger = logger;
        }

        #region EEDM Student Charges V6

        /// <summary>
        /// Retrieves a specified student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <returns>A StudentCharge DTO</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentCharge> GetByIdAsync([FromUri] string id)
        {
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

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }
                var studentCharge = await studentChargeService.GetByIdAsync(id);

                if (studentCharge != null)
                {

                    AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentCharge.Id }));
                }


                return studentCharge;
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student charges for the data model version 6
        /// </summary>
        /// <returns>A Collection of StudentCharges</returns>
        [HttpGet, FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(new string[] { "student", "academicPeriod", "accountingCode", "chargeType" }, false, true)]
        public async Task<IHttpActionResult> GetAsync(Paging page, [FromUri] string student = "", string academicPeriod = "", string accountingCode = "", string chargeType = "")
        {
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

                if (student == null || academicPeriod == null || accountingCode == null || chargeType == null)
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge>>(new List<Dtos.StudentCharge>(), page, 0, this.Request);
                }

                if ((!string.IsNullOrEmpty(chargeType)) && (!ValidEnumerationValue(typeof(Dtos.EnumProperties.StudentChargeTypes), chargeType)))
                {
                    throw new Exception(string.Concat("'", chargeType, "' is an invalid enumeration value. "));
                }

                var pageOfItems = await studentChargeService.GetAsync(page.Offset, page.Limit, bypassCache, student, academicPeriod, accountingCode, chargeType);

                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update a single student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [HttpPut]
        public async Task<Dtos.StudentCharge> UpdateAsync([FromUri] string id, [FromBody] Dtos.StudentCharge studentChargeDto)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            //try
            //{
            //    if (string.IsNullOrEmpty(id))
            //    {
            //        throw new ArgumentNullException("id", "id is a required for update");
            //    }
            //    if (studentChargeDto == null)
            //    {
            //        throw new ArgumentNullException("studentChargeDto", "The request body is required.");
            //    }
            //    if (string.IsNullOrEmpty(studentChargeDto.Id))
            //    {
            //        studentChargeDto.Id = id.ToUpperInvariant();
            //    }
            //    var studentChargeTransaction = await studentChargeService.UpdateAsync(id, studentChargeDto);
            //    return studentChargeTransaction;
            //}
            //catch (PermissionsException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            //}
            //catch (ArgumentException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            //}
            //catch (RepositoryException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            //}
            //catch (Exception e)
            //{
            //    logger.Error(e, "Unknown error getting student charge");
            //    throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            //}
        }

        /// <summary>
        /// Create a single student charge for the data model version 6
        /// </summary>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentCharge> CreateAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentCharge studentChargeDto)
        {
            try
            {
                if (studentChargeDto == null)
                {
                    throw new ArgumentNullException("studentChargeDto", "The request body is required.");
                }
                if (studentChargeDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }

                //call import extend method that needs the extracted extension data and the config
                await studentChargeService.ImportExtendedEthosData(await ExtractExtendedData(await studentChargeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student charge
                var studentChargeTransaction = await studentChargeService.CreateAsync(studentChargeDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentChargeTransaction.Id }));

                return studentChargeTransaction;
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region EEDM Student Charges V11

        /// <summary>
        /// Retrieves a specified student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <returns>A StudentCharge DTO</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentCharge1> GetByIdAsync1([FromUri] string id)
        {
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

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var studentCharge = await studentChargeService.GetByIdAsync1(id);

                if (studentCharge != null)
                {

                    AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentCharge.Id }));
                }


                return studentCharge;

            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student charges for the data model version 11
        /// </summary>
        /// <returns>A Collection of StudentCharges</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Filters.StudentCharges1Filter)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter]
        public async Task<IHttpActionResult> GetAsync1(Paging page, QueryStringFilter criteria = null)
        {
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

                string student = "", academicPeriod = "", fundSource = "", fundDestination = "", chargeType = "";

                var criteriaObj = GetFilterObject<Dtos.Filters.StudentCharges1Filter>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge1>>(new List<Dtos.StudentCharge1>(), page, 0, this.Request);
                }

                if (criteriaObj.Person != null)
                {
                    student = !string.IsNullOrEmpty(criteriaObj.Person.Id) ? criteriaObj.Person.Id : "";
                }

                if (criteriaObj.AcademicPeriod != null)
                {
                    academicPeriod = !string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Id) ? criteriaObj.AcademicPeriod.Id : "";
                }

                if (criteriaObj.FundingSource != null)
                {
                    fundSource = !string.IsNullOrEmpty(criteriaObj.FundingSource.Id) ? criteriaObj.FundingSource.Id : "";
                }

                if (criteriaObj.FundingDestination != null)
                {
                    fundDestination = !string.IsNullOrEmpty(criteriaObj.FundingDestination.Id) ? criteriaObj.FundingDestination.Id : "";
                }
                if (criteriaObj.ChargeType != Dtos.EnumProperties.StudentChargeTypes.notset)
                {
                    chargeType = criteriaObj.ChargeType.ToString();
                }

                var pageOfItems = await studentChargeService.GetAsync1(page.Offset, page.Limit, bypassCache, student, academicPeriod, fundDestination, fundSource, chargeType);

                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge1>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update a single student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [HttpPut]
        public async Task<Dtos.StudentCharge1> UpdateAsync1([FromUri] string id, [FromBody] Dtos.StudentCharge1 studentChargeDto)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create a single student charge for the data model version 6
        /// </summary>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentCharge1> CreateAsync1([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentCharge1 studentChargeDto)
        {
            try
            {
                if (studentChargeDto == null)
                {
                    throw new ArgumentNullException("studentChargeDto", "The request body is required.");
                }
                if (studentChargeDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }

                //call import extend method that needs the extracted extension data and the config
                await studentChargeService.ImportExtendedEthosData(await ExtractExtendedData(await studentChargeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student charge
                var studentChargeTransaction = await studentChargeService.CreateAsync1(studentChargeDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentChargeTransaction.Id }));

                return studentChargeTransaction;
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
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region EEDM Student Charges V16.0.0

        /// <summary>
        /// Retrieves a specified student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <returns>A StudentCharge DTO</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.StudentCharge2> GetStudentChargesByIdAsync([FromUri] string id)
        {
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

                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var studentCharge = await studentChargeService.GetStudentChargesByIdAsync(id);

                if (studentCharge != null)
                {

                    AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { studentCharge.Id }));
                }


                return studentCharge;

            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all student charges for the data model version 11
        /// </summary>
        /// <returns>A Collection of StudentCharges</returns>
        [HttpGet]
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.StudentCharge2)), FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter]
        public async Task<IHttpActionResult> GetStudentChargesAsync(Paging page, QueryStringFilter criteria = null)
        {
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

                string student = "", academicPeriod = "", fundSource = "", fundDestination = "", usage = "";

                var criteriaObj = GetFilterObject<Dtos.StudentCharge2>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                {
                    return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge2>>(new List<Dtos.StudentCharge2>(), page, 0, this.Request);
                }

                if (criteriaObj.Person != null)
                {
                    student = !string.IsNullOrEmpty(criteriaObj.Person.Id) ? criteriaObj.Person.Id : "";
                }

                if (criteriaObj.AcademicPeriod != null)
                {
                    academicPeriod = !string.IsNullOrEmpty(criteriaObj.AcademicPeriod.Id) ? criteriaObj.AcademicPeriod.Id : "";
                }

                if (criteriaObj.FundingSource != null)
                {
                    fundSource = !string.IsNullOrEmpty(criteriaObj.FundingSource.Id) ? criteriaObj.FundingSource.Id : "";
                }

                if (criteriaObj.FundingDestination != null)
                {
                    fundDestination = !string.IsNullOrEmpty(criteriaObj.FundingDestination.Id) ? criteriaObj.FundingDestination.Id : "";
                }
                if (criteriaObj.ReportingDetail != null && criteriaObj.ReportingDetail.Usage != Dtos.EnumProperties.StudentChargeUsageTypes.notset)
                {
                    usage = criteriaObj.ReportingDetail.Usage.ToString();
                }

                var pageOfItems = await studentChargeService.GetStudentChargesAsync(page.Offset, page.Limit, bypassCache, student, academicPeriod, fundDestination, fundSource, usage);

                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                                  await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                                  pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.StudentCharge2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }

        }

        /// <summary>
        /// Update a single student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [HttpPut]
        public async Task<Dtos.StudentCharge2> UpdateStudentChargesAsync([FromUri] string id, [FromBody] Dtos.StudentCharge2 studentChargeDto)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Create a single student charge for the data model version 6
        /// </summary>
        /// <param name="studentChargeDto">General Ledger DTO from Body of request</param>
        /// <returns>A single StudentCharge</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.StudentCharge2> CreateStudentChargesAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.StudentCharge2 studentChargeDto)
        {
            try
            {
                if (studentChargeDto == null)
                {
                    throw new ArgumentNullException("studentChargeDto", "The request body is required.");
                }
                if (studentChargeDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("studentChargeDto", "On a post you can not define a GUID");
                }

                //call import extend method that needs the extracted extension data and the config
                await studentChargeService.ImportExtendedEthosData(await ExtractExtendedData(await studentChargeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //create the student charge
                var studentChargeTransaction = await studentChargeService.CreateStudentChargesAsync(studentChargeDto);

                //store dataprivacy list and get the extended data to store 
                AddEthosContextProperties(await studentChargeService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await studentChargeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { studentChargeTransaction.Id }));

                return studentChargeTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Forbidden);
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
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error getting student charge");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        /// <summary>
        /// Delete a single student charge for the data model version 6
        /// </summary>
        /// <param name="id">The requested student charge GUID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAsync([FromUri] string id)
        {
            // The code is in the service and repository to perform this function but at this time, we
            // are not allowing an update or a delete.  Just throw unsupported error instead.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
            //try
            //{
            //    if (string.IsNullOrEmpty(id))
            //    {
            //        throw new ArgumentNullException("id", "guid is a required for delete");
            //    }
            //    await studentChargeService.DeleteAsync(id);
            //}
            //catch (PermissionsException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            //}
            //catch (ArgumentException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            //}
            //catch (RepositoryException e)
            //{
            //    logger.Error(e.ToString());
            //    throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            //}
            //catch (Exception e)
            //{
            //    logger.Error(e, "Unknown error getting student charge");
            //    throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            //}
            //return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}