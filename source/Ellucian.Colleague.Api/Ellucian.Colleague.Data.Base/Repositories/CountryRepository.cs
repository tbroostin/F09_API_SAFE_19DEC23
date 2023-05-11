// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;
using Ellucian.Web.Dependency;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class CountryRepository : BaseColleagueRepository, ICountryRepository
    {
        private RepositoryException exception;

        public CountryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
        }

        /// <summary>
        /// Countries
        /// </summary>
        public async Task<IEnumerable<Country>> GetCountryCodesAsync(bool ignoreCache = false)
        {
            if (ignoreCache)
            {
                var countries = await BuildAllCountriesAsync();
                return await AddOrUpdateCacheAsync<IEnumerable<Country>>("AllCountries2", countries);

            }
            else
            {
                return await GetOrAddToCacheAsync<IEnumerable<Country>>("AllCountries2", async () => await this.BuildAllCountriesAsync(), Level1CacheTimeoutValue);
            }
        }

        /// <summary>
        /// UpdateCountryAsync
        /// </summary>
        /// <param name="country">country domain entity to be updated</param>
        /// <returns>updated country domain entity</returns>
        public async Task<Country> UpdateCountryAsync(Country country)
        {
            if (country == null)
            {
                throw new RepositoryException("'country' is required to perform update operation.");
            }

            var extendedDataTuple = GetEthosExtendedDataLists();
            var request = new UpdateCountryRequest()
            {
                IsoCode = country.IsoAlpha3Code,
                Country = country.Code
            };

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }
            var response = await transactionInvoker.ExecuteAsync<UpdateCountryRequest, UpdateCountryResponse>(request);

            if (response.CountryErrors != null && response.CountryErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating country '{0}':", country.Guid);
                var exception = new RepositoryException();// errorMessage);

                response.CountryErrors.ForEach(e => exception.AddError(new RepositoryError(string.IsNullOrEmpty(e.ErrorCode)? "" : e.ErrorCode, e.ErrorMsg)
                { Id = country.Guid, SourceId = country.Code }));

                logger.Error(errorMessage);
                throw exception;
            }

            Country updatedCountry = null;
            try
            {
                //After the update, repopulate the cache
                updatedCountry = (await GetCountryCodesAsync(true)).FirstOrDefault(x => x.Guid.Equals(country.Guid));
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Country has been updated, however, an error occurred retrieving data: " + ex.Message)
                { Id = country.Guid, SourceId = country.Code });

                throw exception;
            }
            return updatedCountry;
        }

        /// <summary>
        /// Get an Country entity from guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Country entity</returns>
        public async Task<Country> GetCountryByGuidAsync(string guid)
        {
            var countryId = await GetCountryIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(countryId))
            {
                var errorMessage = "Unable to locate country from guid.";
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }
            return await GetCountryByIDAsync(countryId);
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetCountryIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Country GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Country GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "COUNTRIES")
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, COUNTRIES", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("invalid.guid", errorMessage));
                throw exception;
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single country using an ID
        /// </summary>
        /// <param name="id">The country ID</param>
        /// <returns>The country</returns>
        private async Task<Country> GetCountryByIDAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get an country.");
            }

            // Build the country data
            return await BuildCountryAsync(id);
        }

        /// <summary>
        ///  Build Country domain entity collection that also includes the guid. 
        /// </summary>
        /// <returns>Collection of Country domain entities</returns>
        private async Task<IEnumerable<Country>> BuildAllCountriesAsync()
        {
            var countryEntities = new List<Country>();
            var countryRecords = await DataReader.BulkReadRecordAsync<Countries>("");
            var repositoryException = new RepositoryException();
            foreach (var c in countryRecords)
            {
                try
                {
                    var desc = string.IsNullOrEmpty(c.CtryDesc) ? c.Recordkey : c.CtryDesc;

                    countryEntities.Add(new Country(c.RecordGuid, c.Recordkey, desc, c.CtryIsoCode,
                        c.CtryIsoAlpha3Code, c.CtryNotInUseFlag.ToUpper() == "Y"));
                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Error(csee, "Colleague session expired while extracting country");
                    throw;
                }
                catch (Exception ex)
                {
                    repositoryException.AddError(new RepositoryError("data.exception", string.Format("An error occurred extracting country. {0}",
                        ex.Message)));
                }
            }
            if (repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return countryEntities;
        }

        /// <summary>
        ///  Build Country domain entity that also includes the guid. 
        /// </summary>
        /// <returns>Collection of Country domain entities</returns>
        private async Task<Country> BuildCountryAsync(string id)
        {
            Country countryEntity = null;
            var record = await DataReader.ReadRecordAsync<DataContracts.Countries>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or country with ID ", id, " invalid."));
            }

            var repositoryException = new RepositoryException();

            try
            {
                var desc = string.IsNullOrEmpty(record.CtryDesc) ? record.Recordkey : record.CtryDesc;

                        countryEntity = new Country(record.RecordGuid, record.Recordkey, desc, record.CtryIsoCode,
                    record.CtryIsoAlpha3Code, record.CtryNotInUseFlag.ToUpper() == "Y");
            }
            catch (Exception ex)
            {
                repositoryException.AddError(new RepositoryError("data.exception", string.Format("An error occurred extracting country. {0}",
                    ex.Message)));
            }

            if (repositoryException.Errors != null && repositoryException.Errors.Any())
            {
                throw repositoryException;
            }
            return countryEntity;
        }
    }
}