// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Dependency;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class CurrencyRepository : BaseColleagueRepository, ICurrencyRepository
    {
        private RepositoryException exception;

        public CurrencyRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }

        /// <summary>
        /// Get a collection of CurrencyConv domain entities
        /// </summary>
        public async Task<IEnumerable<Domain.Base.Entities.CurrencyConv>> GetCurrencyConversionAsync(bool ignoreCache = false)
        {
            return await GetGuidCodeItemAsync<DataContracts.CurrencyConv, Domain.Base.Entities.CurrencyConv>("AllCurrencyCodes2", "CURRENCY.CONV",
                 (m, g) => new Domain.Base.Entities.CurrencyConv(g, m.Recordkey, m.CurrencyConvDesc, m.CurrencyConvIsoCode) { }, bypassCache: ignoreCache);
        }

        /// <summary>
        /// UpdateCurrencyAsync
        /// </summary>
        /// <param name="currency">currency domain entity to be updated</param>
        /// <returns>updated currency domain entity</returns>
        public async Task<Domain.Base.Entities.CurrencyConv> UpdateCurrencyConversionAsync(Domain.Base.Entities.CurrencyConv currency)
        {
            if (currency == null)
            {
                throw new RepositoryException("'currency' is required to perform update operation.");
            }
            Domain.Base.Entities.CurrencyConv updatedCurrency = null;
            var extendedDataTuple = GetEthosExtendedDataLists();
            var request = new UpdateCurrencyRequest()
            {
                IsoCode = currency.IsoCode,
                Currencies = currency.Code
            };

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            var response = await transactionInvoker.ExecuteAsync<UpdateCurrencyRequest, UpdateCurrencyResponse>(request);

            if (response.CurrencyConvErrors != null && response.CurrencyConvErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating currency '{0}':", currency.Guid);
                var exception = new RepositoryException(errorMessage);

                response.CurrencyConvErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode) ? "" : e.ErrorCode, e.ErrorMsg)
                { Id = currency.Guid, SourceId = currency.Code }));

                logger.Error(errorMessage);
                throw exception;
            }


            try
            {
                //After the update, repopulate the cache
                updatedCurrency = (await GetCurrencyConversionAsync(true)).FirstOrDefault(x => x.Guid.Equals(currency.Guid));
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Currency has been updated, however, an error occurred retrieving data: " + ex.Message)
                { Id = currency.Guid, SourceId = currency.Code });

                throw exception;
            }
            return updatedCurrency;
        }

        /// <summary>
        /// Get an Currency entity from guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Currency entity</returns>
        public async Task<Domain.Base.Entities.CurrencyConv> GetCurrencyConversionByGuidAsync(string guid)
        {
            var currencyId = await GetCurrencyIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(currencyId))
            {
                var errorMessage = "Unable to locate currency from guid.";
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }
            return await GetCurrencyByIDAsync(currencyId);
        }


        /// <summary>
        /// Get a collection of IntgIsoCurrencyCodes
        /// </summary>
        /// <param name="ignoreCache">Bypass cache flag</param>
        /// <returns>Collection of IntgIsoCurrencyCodes</returns>
        public async Task<IEnumerable<IntgIsoCurrencyCodes>> GetIntgIsoCurrencyCodesAsync(bool ignoreCache)
        {
            return await GetGuidValcodeAsync<IntgIsoCurrencyCodes>("CF", "INTG.ISO.CURRENCY.CODES",
                (cl, g) => new IntgIsoCurrencyCodes(g, cl.ValInternalCodeAssocMember, (string.IsNullOrEmpty(cl.ValExternalRepresentationAssocMember)
                    ? cl.ValInternalCodeAssocMember : cl.ValExternalRepresentationAssocMember), cl.ValActionCode1AssocMember), bypassCache: ignoreCache);
        }

    /// <summary>
    /// Get the record key from a GUID
    /// </summary>
    /// <param name="guid">The GUID</param>
    /// <returns>Primary key</returns>
    private async Task<string> GetCurrencyIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Currency GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Currency GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "CURRENCY.CONV")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, CURRENCY.CONV", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single currency using an ID
        /// </summary>
        /// <param name="id">The currency ID</param>
        /// <returns>The currency</returns>
        private async Task<Domain.Base.Entities.CurrencyConv> GetCurrencyByIDAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get an currency.");
            }

            // Build the currency data
            return await BuildCurrencyAsync(id);
        }

        ///// <summary>
        /////  Build Currency domain entity collection that also includes the guid. 
        ///// </summary>
        ///// <returns>Collection of Currency domain entities</returns>
        //private async Task<IEnumerable<Domain.Base.Entities.CurrencyConv>> BuildAllCurrenciesAsync()
        //{
        //    var currencyEntities = new List<Domain.Base.Entities.CurrencyConv>();
        //    var currencyRecords = await DataReader.BulkReadRecordAsync<DataContracts.CurrencyConv>("");
        //    var repositoryException = new RepositoryException();
        //    foreach (var record in currencyRecords)
        //    {
        //        try
        //        {
        //            currencyEntities.Add(new Domain.Base.Entities.CurrencyConv(record.RecordGuid, record.Recordkey, record.CurrencyConvDesc, record.CurrencyConvIsoCode));
        //        }
        //        catch (Exception ex)
        //        {
        //            repositoryException.AddError(new RepositoryError("data.exception", string.Format("An error occurred extracting currency. {0}",
        //                ex.Message)));
        //        }
        //    }
        //    if (repositoryException.Errors != null && repositoryException.Errors.Any())
        //    {
        //        throw repositoryException;
        //    }
        //    return currencyEntities;
        //}

        ///// <summary>
        ///  Build Currency domain entity that also includes the guid. 
        /// </summary>
        /// <returns>Collection of Currency domain entities</returns>
        private async Task<Domain.Base.Entities.CurrencyConv> BuildCurrencyAsync(string id)
        {
            Domain.Base.Entities.CurrencyConv currencyEntity = null;
            var record = await DataReader.ReadRecordAsync<DataContracts.CurrencyConv>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or currency with ID ", id, " invalid."));
            }

            var repositoryException = new RepositoryException();

            try
            {
                currencyEntity = new Domain.Base.Entities.CurrencyConv(record.RecordGuid, record.Recordkey, record.CurrencyConvDesc, record.CurrencyConvIsoCode);
            }
            catch (Exception ex)
            {
                repositoryException.AddError(new RepositoryError("data.exception", string.Format("An error occurred extracting currency. {0}",
                    ex.Message)));
            }

            if (repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return currencyEntity;
        }
    }
}