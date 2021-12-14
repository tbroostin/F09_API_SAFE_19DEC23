// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for course placeholder data
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class CoursePlaceholderRepository : BaseApiRepository, ICoursePlaceholderRepository
    {
        // Sets the maximum number of records to bulk read at one time
        private readonly int readSize;

        // Specify the Colleague entity name to use throughout this repository
        private const string colleagueEntityName = "COURSE.PLACEHOLDERS";

        /// <summary>
        /// Creates a new instance of the <see cref="CoursePlaceholderRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">Interface to cache provider</param>
        /// <param name="transactionFactory">Interface to Colleague Transaction Factory</param>
        /// <param name="logger">Interface to logger</param>
        /// <param name="apiSettings">Colleague Web API settings"/></param>
        public CoursePlaceholderRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Retrieve a collection of course placeholders by ID
        /// </summary>
        /// <param name="coursePlaceholderIds">Unique identifiers for course placeholders to retrieve</param>
        /// <param name="bypassCache">Flag indicating whether or not to bypass the API's cached course placeholder data and retrieve the data directly from Colleague; defaults to false</param>
        /// <returns>Collection of <see cref="CoursePlaceholder"/></returns>
        public async Task<IEnumerable<CoursePlaceholder>> GetCoursePlaceholdersByIdsAsync(IEnumerable<string> coursePlaceholderIds, bool bypassCache = false)
        {
            var coursePlaceHolders = new List<CoursePlaceholder>();
            if (bypassCache)
            {
                coursePlaceHolders = (await GetNonCachedCoursePlaceholdersAsync(coursePlaceholderIds)).ToList();
            }
            else
            {
                var coursePlaceholderDictionary = await GetCachedCoursePlaceholdersAsync();
                var coursePlaceHoldersInDictionary = coursePlaceholderIds.Where(k => coursePlaceholderDictionary.ContainsKey(k)).Select(k => coursePlaceholderDictionary[k]).ToList();
                var coursePlaceholderIdsMissingFromDictionary = coursePlaceholderIds.Where(k => !coursePlaceholderDictionary.ContainsKey(k)).ToList();
                var coursePlaceholdersMissingFromDictionary = new List<CoursePlaceholder>();
                if (coursePlaceholderIdsMissingFromDictionary.Any())
                {
                    coursePlaceholdersMissingFromDictionary = (await GetNonCachedCoursePlaceholdersAsync(coursePlaceholderIdsMissingFromDictionary)).ToList();
                }
                coursePlaceHolders = coursePlaceHoldersInDictionary.Union(coursePlaceholdersMissingFromDictionary).ToList();
            }
            var missingKeys = coursePlaceholderIds.Except(coursePlaceHolders.Select(cph => cph.Id));
            if (missingKeys.Any())
            {
                throw new KeyNotFoundException(string.Format("Data for the following course placeholders could not be retrieved: {0}", String.Join(", ", missingKeys)));
            }

            return coursePlaceHolders;
        }

        /// <summary>
        /// Retrieves course placeholder data directly from Colleague
        /// </summary>
        /// <param name="coursePlaceholderKeys">Course placeholder record keys</param>
        /// <returns>Course placeholders</returns>
        private async Task<IEnumerable<CoursePlaceholder>> GetNonCachedCoursePlaceholdersAsync(IEnumerable<string> coursePlaceholderKeys)
        {
            var placeholders = (await BuildCoursePlaceholders(coursePlaceholderKeys)).ToList();
            var missingKeys = coursePlaceholderKeys.Except(placeholders.Select(cph => cph.Id));
            if (missingKeys.Any())
            {
                throw new KeyNotFoundException(string.Format("Data for the following course placeholders could not be retrieved: {0}", String.Join(", ", missingKeys)));
            }
            return placeholders;
        }

        /// <summary>
        /// Retrieves course placeholder data from the API cache; if cached course placeholder data is not available then it is built and added to the cache
        /// </summary>
        /// <returns>Cached ourse placeholder dictionary keyed by ID</returns>
        private async Task<Dictionary<string, CoursePlaceholder>> GetCachedCoursePlaceholdersAsync()
        {
            var cachedCoursePlaceholders = await GetOrAddToCacheAsync<Dictionary<string, CoursePlaceholder>>("AllCoursePlaceholders",
                async () =>
                {
                    var allCoursePlaceholderKeys = await DataReader.SelectAsync(colleagueEntityName, string.Empty);
                    var coursePlaceholders = await BuildCoursePlaceholders(allCoursePlaceholderKeys);
                    var coursePlaceholderDictionary = coursePlaceholders.ToDictionary(cph => cph.Id);
                    return coursePlaceholderDictionary;
                }
            );
            return cachedCoursePlaceholders;
        }

        /// <summary>
        /// Builds <see cref="CoursePlaceholder"/> entities for course placeholders with the specified record keys
        /// </summary>
        /// <param name="coursePlaceholderIds">Course placeholder record keys</param>
        /// <returns><see cref="CoursePlaceholder"/> entities for course placeholders with the specified record keys</returns>
        private async Task<IEnumerable<CoursePlaceholder>> BuildCoursePlaceholders(IEnumerable<string> coursePlaceholderKeys)
        {
            if (coursePlaceholderKeys == null || !coursePlaceholderKeys.Any())
            {
                throw new ArgumentNullException("coursePlaceholderIds", "At least one course placeholder ID is required when retrieving course placeholders by ID.");
            }
            else
            { 
                var coursePlaceholderRecords = await BulkReadRecordWithLoggingAsync<CoursePlaceholders>(colleagueEntityName, coursePlaceholderKeys.ToArray(), readSize, true, true);
                var coursePlaceholders = new List<CoursePlaceholder>();
                if (coursePlaceholderRecords != null)
                {
                    var sanitizedCoursePlaceholderRecords = coursePlaceholderRecords.Where(cpr => cpr != null && !string.IsNullOrEmpty(cpr.Recordkey)).Distinct().ToList();
                    foreach (var cphr in sanitizedCoursePlaceholderRecords)
                    {
                        try
                        {
                            CoursePlaceholder cph = new CoursePlaceholder(cphr.Recordkey, cphr.CphTitle, cphr.CphDescription, cphr.CphStartDate, cphr.CphEndDate, cphr.CphCredits, 
                                new Domain.Student.Entities.Requirements.AcademicRequirementGroup(cphr.CphAcadReqmt, cphr.CphSreqAcadReqmtBlock, cphr.CphGroupAcadReqmtBlock));
                            coursePlaceholders.Add(cph);
                        }
                        catch (Exception ex)
                        {
                            LogDataError(colleagueEntityName, cphr.Recordkey, cphr, ex);
                        }
                    }
                }
                return coursePlaceholders;
            }
        }
    }
}
