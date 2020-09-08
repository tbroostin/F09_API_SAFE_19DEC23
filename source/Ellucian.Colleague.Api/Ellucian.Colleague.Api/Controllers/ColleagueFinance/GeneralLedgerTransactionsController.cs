// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Api.Utility;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
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

namespace Ellucian.Colleague.Api.Controllers.ColleagueFinance
{
    /// <summary>
    /// The controller for general ledger transactions for the Ellucian Data Model.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.ColleagueFinance)]
    [Authorize]
    public class GeneralLedgerTransactionsController : BaseCompressedApiController
    {
        private readonly IGeneralLedgerTransactionService generalLedgerTransactionService;
        private readonly ILogger logger;

        /// <summary>
        /// This constructor initializes the GeneralLedgerTransactionController object
        /// </summary>
        /// <param name="generalLedgerTransactionService">General Ledger Transaction service object</param>
        /// <param name="logger">Logger object</param>
        public GeneralLedgerTransactionsController(IGeneralLedgerTransactionService generalLedgerTransactionService, ILogger logger)
        {
            this.generalLedgerTransactionService = generalLedgerTransactionService;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction> GetByIdAsync([FromUri] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetByIdAsync(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction2> GetById2Async([FromUri] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetById2Async(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves a specified general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns>A GeneralLedgerTransaction DTO</returns>
        [HttpGet]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction3> GetById3Async([FromUri] string id)
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
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is required.");
                }

                var generalLedgerTransaction = await generalLedgerTransactionService.GetById3Async(id);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                    new List<string>() { id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e), HttpStatusCode.NotFound);
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 6
        /// </summary>
        /// <returns>A Collection of GeneralLedgerTransactions</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction>> GetAsync()
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
                var generalLedgerTransactions = await generalLedgerTransactionService.GetAsync();

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        generalLedgerTransactions.Select(i => i.Id).ToList()));
              
                return generalLedgerTransactions;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 8
        /// </summary>
        /// <returns>A Collection of GeneralLedgerTransactions</returns>
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction2>> Get2Async()
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
                var generalLedgerTransactions = await generalLedgerTransactionService.Get2Async();

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        generalLedgerTransactions.Select(i => i.Id).ToList()));

                return generalLedgerTransactions;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Retrieves all general ledger transactions for the data model version 12
        /// </summary>
        /// <returns>A Collection of GeneralLedgerTransactions</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpGet]
        [EedmResponseFilter]
        [ValidateQueryStringFilter(), FilteringFilter(IgnoreFiltering = true)]
        public async Task<IEnumerable<Dtos.GeneralLedgerTransaction3>> Get3Async()
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
                var generalLedgerTransactions = await generalLedgerTransactionService.Get3Async(bypassCache);

                AddEthosContextProperties(
                    await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), bypassCache),
                    await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(),
                        generalLedgerTransactions.Select(i => i.Id).ToList()));

                return generalLedgerTransactions;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPut]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction> UpdateAsync([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.UpdateAsync(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPut]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction2> Update2Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Update2Async(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Update a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPut]
        [EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction3> Update3Async([FromUri] string id, [ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentNullException("id", "id is a required for update");
                }
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (string.IsNullOrEmpty(generalLedgerDto.Id))
                {
                    generalLedgerDto.Id = id.ToUpperInvariant();
                }
                if (id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("id", "Invalid id value. Nil GUID cannot be used in PUT operation.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Update3Async(id, generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction> CreateAsync([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction generalLedgerDto)
        {
            try
            {
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.CreateAsync(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 8
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction2> Create2Async([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction2 generalLedgerDto)
        {
            try
            {
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Create2Async(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Create a single general ledger transaction for the data model version 12
        /// </summary>
        /// <param name="generalLedgerDto">General Ledger DTO from Body of request</param>
        /// <returns>A single GeneralLedgerTransaction</returns>
        [CustomMediaTypeAttributeFilter(ErrorContentType = IntegrationErrors2)]
        [HttpPost, EedmResponseFilter]
        public async Task<Dtos.GeneralLedgerTransaction3> Create3Async([ModelBinder(typeof(EedmModelBinder))] Dtos.GeneralLedgerTransaction3 generalLedgerDto)
        {
            try
            {
                if (generalLedgerDto == null)
                {
                    throw new ArgumentNullException("generalLedgerDto", "The request body is required.");
                }
                if (generalLedgerDto.Id != Guid.Empty.ToString())
                {
                    throw new ArgumentException("Non-empty general ledger transaction id not allowed in POST operation. You cannot update an existing general ledger transaction via POST.");
                }

                await generalLedgerTransactionService.ImportExtendedEthosData(await ExtractExtendedData(await generalLedgerTransactionService.GetExtendedEthosConfigurationByResource(GetEthosResourceRouteInfo()), logger));

                var generalLedgerTransaction = await generalLedgerTransactionService.Create3Async(generalLedgerDto);

                AddEthosContextProperties(
                           await generalLedgerTransactionService.GetDataPrivacyListByApi(GetEthosResourceRouteInfo(), false),
                           await generalLedgerTransactionService.GetExtendedEthosDataByResource(GetEthosResourceRouteInfo(), new List<string>() { generalLedgerTransaction.Id }));

                return generalLedgerTransaction;
            }
            catch (PermissionsException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (KeyNotFoundException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentNullException e)
            {
                logger.Error(e.ToString());
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
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (ArgumentException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (InvalidOperationException e)
            {
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
            catch (FormatException e)
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
                logger.Error(e.ToString());
                throw CreateHttpResponseException(IntegrationApiUtility.ConvertToIntegrationApiException(e));
            }
        }

        /// <summary>
        /// Delete a single general ledger transaction for the data model version 6
        /// </summary>
        /// <param name="id">The requested general ledger transaction GUID</param>
        /// <returns></returns>
        [HttpDelete]
        public async Task DeleteAsync([FromUri] string id)
        {
            //Update is not supported for Colleague but HeDM requires full crud support.
            throw CreateHttpResponseException(new IntegrationApiException(IntegrationApiUtility.DefaultNotSupportedApiErrorMessage, IntegrationApiUtility.DefaultNotSupportedApiError));            
        }
    }
}