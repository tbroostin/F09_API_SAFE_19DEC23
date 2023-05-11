/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using slf4net;
using Ellucian.Web.Http.Configuration;
using Ellucian.Colleague.Data.HumanResources.DataContracts;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository to interact with a PersonBenefitDeduction data in Colleague
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonBenefitDeductionRepository : BaseColleagueRepository, IPersonBenefitDeductionRepository
    {
        private readonly int bulkReadSize;
        private const string PersonBenefitDataCacheKeySuffix = "PersonBenefitDeduction";

        /// <summary>
        /// Repository Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PersonBenefitDeductionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Helper to build an employee specific cache key
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        private string BuildPersonBenefitDeductionCacheKey(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            return string.Format(string.Format("{0}-{1}", personId, PersonBenefitDataCacheKeySuffix));
        }

        /// <summary>
        /// Get PersonBenefitDeductions for the list of personids
        /// </summary>
        /// <param name="personIds"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(IEnumerable<string> personIds)
        {
            if (personIds == null || !personIds.Any())
            {
                throw new ArgumentNullException("personIds");
            }

            personIds = personIds.Distinct();

            //this is the return list
            var personBenefitDeductions = new List<PersonBenefitDeduction>();

            //build a set of tuples with 
            //item1: personId
            //item2: full cache key
            var personIdCacheKeys = personIds
                .Select(id => new Tuple<string, string>(id, BuildFullCacheKey(BuildPersonBenefitDeductionCacheKey(id))));

            //find the tuples where the full cache key is already contained in the cache provider
            var keysInCache = personIdCacheKeys.Where(tuple => _cacheProvider.Contains(tuple.Item2));
            foreach (var entry in keysInCache)
            {
                try
                {
                    //for each of the tuples, get the object from the cache and cast it as an IEnumerable<PersonBenefitDeduction>
                    var cachedEntities = (IEnumerable<PersonBenefitDeduction>)_cacheProvider.Get(entry.Item2);
                    if (cachedEntities != null)
                    {
                        personBenefitDeductions.AddRange(cachedEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Info(e, string.Format("Unable to get cached PersonBenefitDedutions from cacheKey {0}", entry.Item2));
                }
            }

            //find the tuples where the full cache key is not contained in the cache provider
            //and get the personids from those tuples.
            var uncachedPersonIds = personIdCacheKeys
                .Where(tuple => !_cacheProvider.Contains(tuple.Item2))
                .Select(tuple => tuple.Item1);

            if (!uncachedPersonIds.Any())
            {
                return personBenefitDeductions;
            }

            //bulk read the hrper records from the list of uncached personIds
            var hrperRecords = new List<Hrper>(uncachedPersonIds.Count());
            for (int i = 0; i < uncachedPersonIds.Count(); i += bulkReadSize)
            {
                var subList = uncachedPersonIds.Skip(i).Take(bulkReadSize);
                var subRecords = await DataReader.BulkReadRecordAsync<Hrper>(subList.ToArray());
                if (subRecords != null)
                {
                    hrperRecords.AddRange(subRecords);
                }
            }

            if (!hrperRecords.Any())
            {
                return personBenefitDeductions;
            }

           
            foreach (var record in hrperRecords)
            {
                try
                {
                    //build person benefit deductions for each hrper record and cache it for next time using the employee specific caching scheme
                    var cachedPersonBenefitDeductions = GetOrAddToCache(BuildPersonBenefitDeductionCacheKey(record.Recordkey),
                        () => BuildPersonBenefitDeductions(record),
                        Level1CacheTimeoutValue);

                    personBenefitDeductions.AddRange(BuildPersonBenefitDeductions(record));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to add person bended.");
                }
            }

            return personBenefitDeductions;


        }

        /// <summary>
        /// Get the PersonBenefitDeduction objects for the given person Id.
        /// PersonBenefitDeductions are cached by person.
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonBenefitDeduction>> GetPersonBenefitDeductionsAsync(string personId)
        {
            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new ArgumentNullException("personId");
            }
            var cacheKey = string.Format("{0}-{1}", personId, PersonBenefitDataCacheKeySuffix);
            var cachedPersonBenefitDeductions = await GetOrAddToCacheAsync(cacheKey,
                async () =>
                {
                    var hrPerRecord = await DataReader.ReadRecordAsync<Hrper>(personId);
                    if (hrPerRecord == null)
                    {
                        throw new KeyNotFoundException(string.Format("Hrper record does not exist for {0} ", personId));
                    }

                    var personBenefitDeductions = BuildPersonBenefitDeductions(hrPerRecord);
                    return personBenefitDeductions;
                });
            return cachedPersonBenefitDeductions;
        }

        /// <summary>
        /// Helper to build a list of PersonBenefitDeductions for a given hrper record
        /// </summary>
        /// <param name="hrperRecord"></param>
        /// <returns></returns>
        private IEnumerable<PersonBenefitDeduction> BuildPersonBenefitDeductions(Hrper hrperRecord)
        {
            if (hrperRecord == null)
            {
                throw new ArgumentNullException("hrperRecord");
            }

            if (hrperRecord.PerbenInfoEntityAssociation == null || !hrperRecord.PerbenInfoEntityAssociation.Any())
            {
                return new List<PersonBenefitDeduction>();
            }
            var personBenefitDeductions = new List<PersonBenefitDeduction>(hrperRecord.PerbenInfoEntityAssociation.Count);

            foreach (var hrperBen in hrperRecord.PerbenInfoEntityAssociation)
            {
                try
                {
                    var perben = new PersonBenefitDeduction(hrperRecord.Recordkey,
                        hrperBen.HrpPerbenBdIdAssocMember,
                        hrperBen.HrpPerbenEnrollDateAssocMember.Value,
                        hrperBen.HrpPerbenCancelDateAssocMember,
                        hrperBen.HrpPerbenLastPayDateAssocMember);
                    personBenefitDeductions.Add(perben);
                }
                catch (Exception e)
                {
                    LogDataError("HRPER", hrperRecord.Recordkey, hrperBen, e, string.Format("unable to add HR PERBEN for code {0}", hrperBen.HrpPerbenBdIdAssocMember));
                }
            }
            return personBenefitDeductions;
        }
    }
}
