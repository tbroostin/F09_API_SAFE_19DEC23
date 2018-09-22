// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to Accounting Strings
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountingStringsController : BaseCompressedApiController
    {
        private readonly IAccountingStringService _accountingStringService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountingStringsController class.
        /// </summary>
        /// <param name="accountingStringService">Service of type <see cref="IAccountingStringService">IAccountingStringService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AccountingStringsController(IAccountingStringService accountingStringService, ILogger logger)
        {
            _accountingStringService = accountingStringService;
            _logger = logger;
        }

        #region Accounting String
        /// <summary>
        /// Read (GET) an AccountingString using an accounting string as a filter, return confirms it exists and is valid
        /// </summary>
        /// <param name="accountingString">Accounting String to search for, may contain project number</param>
        /// <param name="validOn">date to check for to see if accounting string is valid on the date</param>
        /// <returns>An AccountingString object <see cref="Dtos.AccountingString"/> in DataModel format</returns>
        [HttpGet, EedmResponseFilter, FilteringFilter(IgnoreFiltering = true)]
        [ValidateQueryStringFilter(new string[] { "accountingString", "validOn" }, false, true)]
        public async Task<Dtos.AccountingString> GetAccountingStringByFilterAsync([FromUri] string accountingString = "", [FromUri] string validOn = "")
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }

            if (string.IsNullOrEmpty(accountingString))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null accountingString argument",
                    IntegrationApiUtility.GetDefaultApiError("The accountingString must be specified in the request URL.")));
            }

            try
            {
                DateTime? validOnDate = null;

                if (!string.IsNullOrEmpty(validOn))
                {
                    DateTime outDate;
                    if (DateTime.TryParse(validOn, out outDate))
                    {
                        validOnDate = outDate;
                    }
                    else
                    {
                        throw new ArgumentException(
                            "The value provided for validOn filter could not be converted into a date");
                    }
                }

                AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                
                return await _accountingStringService.GetAccoutingStringByFilterCriteriaAsync(accountingString, validOnDate);
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                var exception = new Exception(e.Message);
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(exception));
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// AccountingString Get all
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpGet]
        public async Task<IEnumerable<Dtos.AccountingString>> GetAccountingStringsAsync()
        {
            //Get all is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException("Null accountingString argument",
                    IntegrationApiUtility.GetDefaultApiError("The accountingString must be specified in the request URL when a GET operation is requested.")));
            //throw new ArgumentNullException("Null accountingString argument", "The accountingString must be specified in the request URL when a GET operation is requested.");
        }

        /// <summary>
        /// AccountingString Post
        /// </summary>
        /// <returns>MethodNotAllowed error</returns>
        [HttpPost]
        public async Task<Dtos.AccountingString> PostAccountingStringsAsync([FromBody] Dtos.AccountingString accountingString)
        {
            //Post is not supported for Colleague but Data Model requires full crud support.
            throw CreateHttpResponseException(
                new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage,
                    IntegrationApiUtility.DefaultNotSupportedApiError), HttpStatusCode.MethodNotAllowed);
        }

        #endregion

        #region Accounting String Components

        /// <summary>
        /// Return all accountingStringComponents
        /// </summary>
        /// <returns>List of AccountingStringComponents <see cref="Dtos.AccountingStringComponent"/> objects representing matching accountingStringComponents</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponent>> GetAccountingStringComponentsAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _accountingStringService.GetAccountingStringComponentsAsync(bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponents using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponents</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AccountingStringComponent> GetAccountingStringComponentsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _accountingStringService.GetAccountingStringComponentsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new accountingStringComponents
        /// </summary>
        /// <param name="accountingStringComponents">DTO of the new accountingStringComponents</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringComponent> PostAccountingStringComponentsAsync([FromBody] Dtos.AccountingStringComponent accountingStringComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponents
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponents to update</param>
        /// <param name="accountingStringComponents">DTO of the updated accountingStringComponent</param>
        /// <returns>A accountingStringComponents object <see cref="Dtos.AccountingStringComponent"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringComponent> PutAccountingStringComponentsAsync([FromUri] string guid, [FromBody] Dtos.AccountingStringComponent accountingStringComponents)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringComponents
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponents</param>
        [HttpDelete]
        public async Task DeleteAccountingStringComponentsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion

        #region Accounting String Formats


        /// <summary>
        /// Return all accountingStringFormats
        /// </summary>
        /// <returns>List of AccountingStringFormats <see cref="Dtos.AccountingStringFormats"/> objects representing matching accountingStringFormats</returns>
        [HttpGet]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringFormats>> GetAccountingStringFormatsAsync()
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                return await _accountingStringService.GetAccountingStringFormatsAsync(bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringFormats using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpGet]
        public async Task<Dtos.AccountingStringFormats> GetAccountingStringFormatsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            try
            {
                return await _accountingStringService.GetAccountingStringFormatsByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new accountingStringFormats
        /// </summary>
        /// <param name="accountingStringFormats">DTO of the new accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringFormats> PostAccountingStringFormatsAsync([FromBody] Dtos.AccountingStringFormats accountingStringFormats)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringFormats
        /// </summary>
        /// <param name="guid">GUID of the accountingStringFormats to update</param>
        /// <param name="accountingStringFormats">DTO of the updated accountingStringFormats</param>
        /// <returns>A accountingStringFormats object <see cref="Dtos.AccountingStringFormats"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringFormats> PutAccountingStringFormatsAsync([FromUri] string guid, [FromBody] Dtos.AccountingStringFormats accountingStringFormats)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringFormats
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringFormats</param>
        [HttpDelete]
        public async Task DeleteAccountingStringFormatsAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        #endregion

        #region Accounting String Components Values

        /// <summary>
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValuesFilter)), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAccountingStringComponentValuesAsync(Paging page, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string component = string.Empty, transactionStatus = string.Empty, typeAccount = string.Empty, typeFund = string.Empty;
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValuesFilter>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues>>(new List<Dtos.AccountingStringComponentValues>(), page, 0, this.Request);

                if (criteriaValues != null)
                {
                    component = criteriaValues.Component != null && !string.IsNullOrEmpty(criteriaValues.Component.Id)
                        ? criteriaValues.Component.Id : string.Empty;
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    typeAccount = criteriaValues.Type != null && criteriaValues.Type.Account != null ?
                       criteriaValues.Type.Account.ToString() : string.Empty;
                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                    if (typeAccount == string.Empty && criteriaValues.TypeAccount != null)
                        typeAccount = criteriaValues.TypeAccount.ToString();
                    if (typeFund == string.Empty && !string.IsNullOrEmpty(criteriaValues.TypeFund))
                        typeFund = criteriaValues.TypeFund;
                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValuesAsync(page.Offset, page.Limit, component, transactionStatus, typeAccount, typeFund, bypassCache);

                AddEthosContextProperties(
                await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountingStringComponentValues> GetAccountingStringComponentValuesByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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

                //AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValuesByGuidAsync(guid);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create (POST) a new accountingStringComponentValues
        /// </summary>
        /// <param name="accountingStringComponentValues">DTO of the new accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringComponentValues> PostAccountingStringComponentValuesAsync([FromBody] Dtos.AccountingStringComponentValues accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponentValues to update</param>
        /// <param name="accountingStringComponentValues">DTO of the updated accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringComponentValues> PutAccountingStringComponentValuesAsync([FromUri] string guid, [FromBody] Dtos.AccountingStringComponentValues accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Delete (DELETE) a accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        [HttpDelete]
        public async Task DeleteAccountingStringComponentValuesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValues2)), FilteringFilter(IgnoreFiltering = true)]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAccountingStringComponentValues2Async(Paging page, QueryStringFilter criteria)
        {
           var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string component = string.Empty, transactionStatus = string.Empty, typeAccount = string.Empty, typeFund = string.Empty;
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValues2>(_logger, "criteria");

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues2>>(new List<Dtos.AccountingStringComponentValues2>(), page, 0, this.Request);

                if (criteriaValues != null)
                {
                    component = criteriaValues.Component != null && !string.IsNullOrEmpty(criteriaValues.Component.Id)
                        ? criteriaValues.Component.Id : string.Empty;
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    typeAccount = criteriaValues.Type != null && criteriaValues.Type.Account != null ?
                       criteriaValues.Type.Account.ToString() : string.Empty;
                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValues2Async(page.Offset, page.Limit, component, transactionStatus, typeAccount, typeFund, bypassCache);
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Return all accountingStringComponentValues
        /// </summary>
        /// <returns>List of AccountingStringComponentValues <see cref="Dtos.AccountingStringComponentValues"/> objects representing matching accountingStringComponentValues</returns>
        [HttpGet, ValidateQueryStringFilter()]
        [QueryStringFilterFilter("criteria", typeof(Dtos.AccountingStringComponentValues3)), FilteringFilter(IgnoreFiltering = true)]
        [QueryStringFilterFilter("effectiveOn", typeof(Dtos.Filters.AccountingStringsFilter))]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 200), EedmResponseFilter]
        public async Task<IHttpActionResult> GetAccountingStringComponentValues3Async(Paging page, QueryStringFilter effectiveOn, QueryStringFilter criteria)
        {
            var bypassCache = false;
            if (Request.Headers.CacheControl != null)
            {
                if (Request.Headers.CacheControl.NoCache)
                {
                    bypassCache = true;
                }
            }
            try
            {
                if (page == null)
                {
                    page = new Paging(200, 0);
                }
                string transactionStatus = string.Empty, typeFund = string.Empty;                
                var criteriaValues = GetFilterObject<Dtos.AccountingStringComponentValues3>(_logger, "criteria");

                DateTime? effectiveOnValue = null;
                var effectiveOnFilterObj = GetFilterObject<Dtos.Filters.AccountingStringsFilter>(_logger, "effectiveOn");
                if(effectiveOnFilterObj != null && effectiveOnFilterObj.EffectiveOn.HasValue)
                {
                    effectiveOnValue = effectiveOnFilterObj.EffectiveOn.Value;
                }

                if (CheckForEmptyFilterParameters())
                    return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, this.Request);

                if (criteriaValues != null)
                {
                    transactionStatus = criteriaValues.TransactionStatus != null ?
                        criteriaValues.TransactionStatus.ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(transactionStatus) && !transactionStatus.ToLowerInvariant().Equals("available", StringComparison.OrdinalIgnoreCase))
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, this.Request);
                    }

                    typeFund = criteriaValues.Type != null && criteriaValues.Type.Fund != null ?
                       criteriaValues.Type.Fund.ToString() : string.Empty;

                    if(!string.IsNullOrEmpty(typeFund))
                    {
                        return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(new List<Dtos.AccountingStringComponentValues3>(), page, 0, this.Request);
                    }

                }
                var pageOfItems = await _accountingStringService.GetAccountingStringComponentValues3Async(page.Offset, page.Limit, criteriaValues, effectiveOnValue, bypassCache);

                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     pageOfItems.Item1.Select(i => i.Id).ToList()));

                return new PagedHttpActionResult<IEnumerable<Dtos.AccountingStringComponentValues3>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountingStringComponentValues3> GetAccountingStringComponentValues3ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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

                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync(guid, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Read (GET) a accountingStringComponentValues using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountingStringComponentValues2> GetAccountingStringComponentValues2ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
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

                //AddDataPrivacyContextProperty((await _accountingStringService.GetDataPrivacyListByApi(GetRouteResourceName(), bypassCache)).ToList());
                AddEthosContextProperties(
                 await _accountingStringService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                 await _accountingStringService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                     new List<string>() { guid }));

                return await _accountingStringService.GetAccountingStringComponentValues2ByGuidAsync(guid, bypassCache);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.BadRequest);
            }
        }        

        /// <summary>
        /// Create (POST) a new accountingStringComponentValues
        /// </summary>
        /// <param name="accountingStringComponentValues">DTO of the new accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPost]
        public async Task<Dtos.AccountingStringComponentValues> PostAccountingStringComponentValues2Async([FromBody] Dtos.AccountingStringComponentValues2 accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        /// <summary>
        /// Update (PUT) an existing accountingStringComponentValues
        /// </summary>
        /// <param name="guid">GUID of the accountingStringComponentValues to update</param>
        /// <param name="accountingStringComponentValues">DTO of the updated accountingStringComponentValues</param>
        /// <returns>A accountingStringComponentValues object <see cref="Dtos.AccountingStringComponentValues"/> in EEDM format</returns>
        [HttpPut]
        public async Task<Dtos.AccountingStringComponentValues> PutAccountingStringComponentValues2Async([FromUri] string guid, [FromBody] Dtos.AccountingStringComponentValues2 accountingStringComponentValues)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }

        #endregion Accounting String Components Values
    }
}
