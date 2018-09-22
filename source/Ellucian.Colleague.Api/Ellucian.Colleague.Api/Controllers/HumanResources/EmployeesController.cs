/* Copyright 2016-2018 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
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
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Exposes Employee data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class EmployeesController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IEmployeeService employeeService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="employeeService"></param>
        public EmployeesController(ILogger logger, IEmployeeService employeeService)
        {
            this.logger = logger;
            this.employeeService = employeeService;
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee"/></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Employee> GetEmployeeByIdAsync([FromUri] string id)
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
                var employee = await employeeService.GetEmployeeByGuidAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee2"/></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Employee2> GetEmployee2ByIdAsync([FromUri] string id)
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
                var employee = await employeeService.GetEmployee2ByIdAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get a single employee using a guid.
        /// </summary>
        /// <param name="id">Guid of the employee to retrieve</param>
        /// <returns>Returns a single Employee object. <see cref="Dtos.Employee2"/></returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.Employee2> GetEmployee3ByIdAsync([FromUri] string id)
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
                var employee = await employeeService.GetEmployee3ByIdAsync(id);

                if (employee != null)
                {

                    AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              new List<string>() { employee.Id }));
                }


                return employee;

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="person">Person id filter.</param>
        /// <param name="campus">Primary campus or location filter.</param>
        /// <param name="status">Status ("active", "terminated", or "leave") filter.</param>
        /// <param name="startOn">Start on a specific date filter.</param>
        /// <param name="endOn">End on a specific date filter.</param>
        /// <param name="rehireableStatusEligibility">Rehireable status ("eligible" or "ineligible") filter.</param>
        /// <param name="rehireableStatusType">Rehireable code filter.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee"/></returns>
        [HttpGet]
        [ValidateQueryStringFilter(new string[] { "person", "campus", "status", "startOn", "endOn", "rehireableStatusEligibility", "rehireableStatusType" }, false, true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetEmployeesAsync(Paging page,
            [FromUri] string person = "", [FromUri] string campus = "", [FromUri] string status = "",
            [FromUri] string startOn = "", [FromUri] string endOn = "", [FromUri] string rehireableStatusEligibility = "", [FromUri] string rehireableStatusType = "")
        {
            string criteria = string.Concat(person, campus, status, startOn, endOn, rehireableStatusEligibility,rehireableStatusType);

            //valid query parameter but empty argument
            if ((!string.IsNullOrEmpty(criteria)) && (string.IsNullOrEmpty(criteria.Replace("\"", ""))))
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, this.Request);
            }
            if (person == null || campus == null || status == null || startOn == null || endOn == null
                || rehireableStatusEligibility == null || rehireableStatusType == null)
            {
                return new PagedHttpActionResult<IEnumerable<Dtos.Employee>>(new List<Dtos.Employee>(), page, 0, this.Request);
            }

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
                page = new Paging(100, 0);
            }
            try
            {
                var pageOfItems = await employeeService.GetEmployeesAsync(page.Offset, page.Limit, bypassCache, person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Employee>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees for V11 using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="criteria">Filter Criteria, includes person, campus, status, startOn, endOn, rehireableStatus.eligibility, and rehireableStatus.type.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee2"/></returns>
        [HttpGet, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Employee2)), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        public async Task<IHttpActionResult> GetEmployees2Async(Paging page, QueryStringFilter criteria)
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
                    page = new Paging(100, 0);
                }

                string person = string.Empty, campus = string.Empty, status = string.Empty, startOn = string.Empty,
                    endOn = string.Empty, rehireableStatusEligibility = string.Empty, rehireableStatusType = string.Empty, contractType = string.Empty, contractDetail = string.Empty;
                var rawFilterData = GetFilterObject<Dtos.Employee2>(logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Employee2>>(new List<Dtos.Employee2>(), page, 0, this.Request);

                if (rawFilterData != null)
                {
                    person = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    campus = rawFilterData.Campus != null ? rawFilterData.Campus.Id : null;
                    status = rawFilterData.Status.ToString();
                    startOn = rawFilterData.StartOn.ToString();
                    endOn = rawFilterData.EndOn.ToString();
                    if (rawFilterData.RehireableStatus != null)
                    {
                        rehireableStatusEligibility = rawFilterData.RehireableStatus.Eligibility.ToString();
                        if (rawFilterData.RehireableStatus.Type != null)
                            rehireableStatusType = rawFilterData.RehireableStatus.Type.Id;
                    }
                    if (rawFilterData.Contract != null)
                    {
                        if (rawFilterData.Contract.Type != null)
                        {
                            contractType = rawFilterData.Contract.Type.ToString();
                        }
                        if (rawFilterData.Contract.Detail != null)
                        {
                            if (rawFilterData.Contract.Detail.Id != null)
                                contractDetail = rawFilterData.Contract.Detail.Id.ToString();
                        }
                    }
                }
                var pageOfItems = await employeeService.GetEmployees2Async(page.Offset, page.Limit, bypassCache, person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType
                    , contractType, contractDetail);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Employee2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Get all employees for V12 using paging and including filters if necessary.
        /// </summary>
        /// <param name="page">Paging offset and limit.</param>
        /// <param name="criteria">Filter Criteria, includes person, campus, status, startOn, endOn, rehireableStatus.eligibility, and rehireableStatus.type.</param>
        /// <returns>Returns a list of Employee objects using paging.  <see cref="Dtos.Employee2"/></returns>
        [HttpGet, ValidateQueryStringFilter()]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [QueryStringFilterFilter("criteria", typeof(Dtos.Employee2)), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetEmployees3Async(Paging page,
            QueryStringFilter criteria)
        {
            string person = string.Empty, campus = string.Empty, status = string.Empty, startOn = string.Empty,
                    endOn = string.Empty, rehireableStatusEligibility = string.Empty, rehireableStatusType = string.Empty, contractType = string.Empty, contractDetail = string.Empty; 

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
                    page = new Paging(100, 0);
                }
              
                var rawFilterData = GetFilterObject<Dtos.Employee2>(logger, "criteria");
                 
                 if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.Employee2>>(new List<Dtos.Employee2>(), page, 0, this.Request);

                if (rawFilterData != null)
                {
                    person = rawFilterData.Person != null ? rawFilterData.Person.Id : null;
                    campus = rawFilterData.Campus != null ? rawFilterData.Campus.Id : null;
                    status = rawFilterData.Status.ToString();
                    startOn = rawFilterData.StartOn.ToString();
                    endOn = rawFilterData.EndOn.ToString();
                    if (rawFilterData.RehireableStatus != null)
                    {
                        rehireableStatusEligibility = rawFilterData.RehireableStatus.Eligibility.ToString();
                        if (rawFilterData.RehireableStatus.Type != null)
                            rehireableStatusType = rawFilterData.RehireableStatus.Type.Id;
                    }
                    if (rawFilterData.Contract != null)
                    {
                        if (rawFilterData.Contract.Type != null)
                        {
                            contractType = rawFilterData.Contract.Type.ToString();
                        }
                        if (rawFilterData.Contract.Detail != null)
                        {
                            if (rawFilterData.Contract.Detail.Id != null)
                            contractDetail = rawFilterData.Contract.Detail.Id.ToString();
                        }
                    }
                }
                var pageOfItems = await employeeService.GetEmployees3Async(page.Offset, page.Limit, bypassCache,
                    person, campus, status, startOn, endOn, rehireableStatusEligibility, rehireableStatusType, contractType, contractDetail);

                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                              pageOfItems.Item1.Select(a => a.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.Employee2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);

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
                logger.Error(e, "Unknown error getting employee");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Update an existing employee
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPut]
        public async Task<Dtos.Employee> PutEmployeeAsync([FromUri] string id, [FromBody] Dtos.Employee employeeDto)
        {
            //Put is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a new employee record
        /// </summary>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPost]
        public async Task<Dtos.Employee> PostEmployeeAsync([FromBody] Dtos.Employee employeeDto)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Create a new employee record v12
        /// </summary>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.Employee2> PostEmployee3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.Employee2 employeeDto)
        {
            if (employeeDto == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null employeeDto argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (!employeeDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID must be used in POST operation.", HttpStatusCode.BadRequest);
            }
            try
            {
                await employeeService.ImportExtendedEthosData(await ExtractExtendedData(await employeeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));
                var employee = await employeeService.PostEmployee2Async(employeeDto);
                AddEthosContextProperties(await employeeService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { employee.Id }));
                return employee;
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
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update an existing employee v12
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <param name="employeeDto">Employee DTO request for update</param>
        /// <returns>A employeeDto object <see cref="Dtos.Employee2"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.Employee2> PutEmployee3Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.Employee2 employeeDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (employeeDto == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null employeeDto argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(employeeDto.Id))
            {
                employeeDto.Id = id.ToLowerInvariant();
            }
            if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw CreateHttpResponseException("Nil GUID cannot be used in PUT operation.", HttpStatusCode.BadRequest);
            }
            else if ((string.Equals(id, Guid.Empty.ToString())) || (string.Equals(employeeDto.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (id.ToLowerInvariant() != employeeDto.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }
            try
            {
                if (employeeDto.HomeOrganization != null && !string.IsNullOrEmpty(employeeDto.HomeOrganization.Id))
                {
                    throw new ArgumentNullException("The Home Organization Id is not allowed for a PUT or POST request. ", "employee.homeOrganization.id");
                }
                //get Data Privacy List

                var dpList = await employeeService.GetDataPrivacyListByApi(GetRouteResourceName(), true);
                
                await employeeService.ImportExtendedEthosData(await ExtractExtendedData(await employeeService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                //this is a section to check all those attributes that cannot be updated in a PUT request. 
                var origDto = new Dtos.Employee2();
                try
                {
                    origDto = await employeeService.GetEmployee3ByIdAsync(id);
                }
                catch (KeyNotFoundException)
                {
                    origDto = null;
                }

            var employee =  await employeeService.PutEmployee2Async(id,
                            await PerformPartialPayloadMerge(employeeDto,
                                    origDto,
                                    dpList,
                                    logger), origDto);
                AddEthosContextProperties(dpList,
              await employeeService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { employee.Id }));
                return employee;
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
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete an existing employee
        /// </summary>
        /// <param name="id">Employee GUID for update.</param>
        /// <returns>Currently not implemented.  Returns default not supported API error message.</returns>
        [HttpDelete]
        public async Task DeleteEmployeeAsync([FromUri] string id)
        {
            //Delete is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));
        }

        /// <summary>
        /// Gets a list of employees matching the given criteria
        /// </summary>
        /// <param name="criteria">An object that specifies search criteria</param>
        /// <returns>The response value is a list of Person DTOs for the matching set of employees.</returns>
        /// <exception cref="HttpResponseException">Http Response Exception</exception>
        /// <accessComments>
        /// Users with the following permission codes can query employee names:
        /// 
        /// ViewAllEarningsStatements
        /// VIEW.EMPLOYEE.DATA
        /// APPROVE.REJECT.TIME.ENTRY
        /// VIEW.EMPLOYEE.W2
        /// VIEW.EMPLOYEE.1095C
        /// </accessComments>
        [HttpPost]
        public async Task<IEnumerable<Dtos.Base.Person>> QueryEmployeeNamesByPostAsync([FromBody] Dtos.Base.EmployeeNameQueryCriteria criteria)
        {
            try
            {
                return await employeeService.QueryEmployeeNamesByPostAsync(criteria);
            }
            catch (PermissionsException pe)
            {
                throw CreateHttpResponseException(pe.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}