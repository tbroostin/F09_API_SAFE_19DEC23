﻿//Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding;

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// Provides access to AccountsPayableInvoices
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    public class AccountsPayableInvoicesController : BaseCompressedApiController
    {
        private readonly IAccountsPayableInvoicesService _accountsPayableInvoicesService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AccountsPayableInvoicesController class.
        /// </summary>
        /// <param name="accountsPayableInvoicesService">Service of type <see cref="IAccountsPayableInvoicesService">IAccountsPayableInvoicesService</see></param>
        /// <param name="logger">Interface to logger</param>
        public AccountsPayableInvoicesController(IAccountsPayableInvoicesService accountsPayableInvoicesService, ILogger logger)
        {
            _accountsPayableInvoicesService = accountsPayableInvoicesService;
            _logger = logger;
        }

        /// <summary>
        /// Return all accountsPayableInvoices version 11
        /// </summary>
        /// <returns>List of AccountsPayableInvoices <see cref="Dtos.AccountsPayableInvoices2"/> objects representing matching accountsPayableInvoices</returns>
        [HttpGet]
        [PagingFilter(IgnorePaging = true, DefaultLimit = 100), EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IHttpActionResult> GetAccountsPayableInvoices2Async(Paging page)
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
                    page = new Paging(100, 0);
                }

                var pageOfItems = await _accountsPayableInvoicesService.GetAccountsPayableInvoices2Async(page.Offset, page.Limit, bypassCache);

                AddEthosContextProperties(
                await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    pageOfItems.Item1.Select(i => i.Id).ToList()));
                                
                return new PagedHttpActionResult<IEnumerable<Dtos.AccountsPayableInvoices2>>(pageOfItems.Item1, page, pageOfItems.Item2, this.Request);
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
        /// Read (GET) a accountsPayableInvoices version 11 using a GUID
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [HttpGet, EedmResponseFilter]
        public async Task<Dtos.AccountsPayableInvoices2> GetAccountsPayableInvoices2ByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }

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
                AddEthosContextProperties(
                    await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { guid }));

                return await _accountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid);
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
        /// Create (POST) a new accountsPayableInvoices
        /// </summary>
        /// <param name="accountsPayableInvoices">DTO of the new accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.AccountsPayableInvoices2> PostAccountsPayableInvoices2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.AccountsPayableInvoices2 accountsPayableInvoices)
        {
            if (accountsPayableInvoices == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null accountsPayableInvoices argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            try
            {
                if (accountsPayableInvoices.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentNullException("accountsPayableInvoicesDto", "Nil GUID must be used in POST operation.");
                }
                if ((accountsPayableInvoices.TransactionDate == DateTime.MinValue))
                {
                    throw new ArgumentNullException("accountsPayableInvoices.TransactionDate", "The transactionDate is a required field");
                }

                if (accountsPayableInvoices.VoidDate != null)
                {
                    throw new ArgumentNullException("accountsPayableInvoices.VoidDate", "Cannot have a void date on a POST");
                }

                //call import extend method that needs the extracted extension dataa and the config
                await _accountsPayableInvoicesService.ImportExtendedEthosData(await ExtractExtendedData(await _accountsPayableInvoicesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var accountsPayableInvoiceReturn = await _accountsPayableInvoicesService.PostAccountsPayableInvoices2Async(accountsPayableInvoices);
                
                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetRouteResourceName(), true),
                   await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { accountsPayableInvoiceReturn.Id }));

                return accountsPayableInvoiceReturn;
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
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
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
        /// Update (PUT) an existing accountsPayableInvoices
        /// </summary>
        /// <param name="guid">GUID of the accountsPayableInvoices to update</param>
        /// <param name="accountsPayableInvoices">DTO of the updated accountsPayableInvoices</param>
        /// <returns>A accountsPayableInvoices object <see cref="Dtos.AccountsPayableInvoices2"/> in EEDM format</returns>
        [HttpPut, EedmResponseFilter]
        public async Task<Dtos.AccountsPayableInvoices2> PutAccountsPayableInvoices2Async([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] Dtos.AccountsPayableInvoices2 accountsPayableInvoices)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null id argument",
                    IntegrationApiUtility.GetDefaultApiError("The GUID must be specified in the request URL.")));
            }
            if (accountsPayableInvoices == null)
            {
                throw CreateHttpResponseException(new IntegrationApiException("Null accountsPayableInvoices argument",
                    IntegrationApiUtility.GetDefaultApiError("The request body is required.")));
            }
            if (string.IsNullOrEmpty(accountsPayableInvoices.Id))
            {
                accountsPayableInvoices.Id = guid.ToLowerInvariant();
            }
            else if ((string.Equals(guid, Guid.Empty.ToString())) || (string.Equals(accountsPayableInvoices.Id, Guid.Empty.ToString())))
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID empty",
                    IntegrationApiUtility.GetDefaultApiError("GUID must be specified.")));
            }
            else if (guid.ToLowerInvariant() != accountsPayableInvoices.Id.ToLowerInvariant())
            {
                throw CreateHttpResponseException(new IntegrationApiException("GUID mismatch",
                    IntegrationApiUtility.GetDefaultApiError("GUID not the same as in request body.")));
            }

            try
            {
                //get Data Privacy List
                var dpList = await _accountsPayableInvoicesService.GetDataPrivacyListByApi(GetRouteResourceName(), true);

                //call import extend method that needs the extracted extension data and the config
                await _accountsPayableInvoicesService.ImportExtendedEthosData(await ExtractExtendedData(await _accountsPayableInvoicesService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), _logger));

                var accountsPayableInvoiceReturn = await _accountsPayableInvoicesService.PutAccountsPayableInvoices2Async(guid,
                    await PerformPartialPayloadMerge(accountsPayableInvoices,
                        async () => await _accountsPayableInvoicesService.GetAccountsPayableInvoices2ByGuidAsync(guid),
                        dpList, _logger));

                //store dataprivacy list and get the extended data to store
                AddEthosContextProperties(dpList,
                    await _accountsPayableInvoicesService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { guid }));

                return accountsPayableInvoiceReturn;
            }
            catch (PermissionsException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }          
            catch (RepositoryException e)
            {
                _logger.Error(e.ToString());
                if (e.Errors == null || e.Errors.Count() <= 0)
                {
                    throw CreateHttpResponseException(e.Message);
                }
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (IntegrationApiException e)
            {
                _logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ConfigurationException e)
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
        /// Delete (DELETE) a accountsPayableInvoices
        /// </summary>
        /// <param name="guid">GUID to desired accountsPayableInvoices</param>
        [HttpDelete]
        public async Task DeleteAccountsPayableInvoicesAsync(string guid)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));

        }
        
    }
}