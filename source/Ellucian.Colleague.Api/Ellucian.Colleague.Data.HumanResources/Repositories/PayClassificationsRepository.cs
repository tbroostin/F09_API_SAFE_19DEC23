/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayClassificationsRepository : BaseColleagueRepository, IPayClassificationsRepository
    {
        public PayClassificationsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get PayClassifications objects for all PayClassifications bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="bypassCache">Bypass cache if set to true.</param>
        /// <returns>Tuple of PayClassifications Entity objects <see cref="PayClassifications"/> and a count for paging.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayClassification>> GetPayClassificationsAsync(bool bypassCache = false)
        {
            if (bypassCache)
            {
                return await GetPayClassificationNoCacheAsync();
            }

            return await GetOrAddToCacheAsync<IEnumerable<PayClassification>>("AllPayClassifications",
               async () =>
               {
                   return await GetPayClassificationNoCacheAsync();
               });
        }

        private async Task<IEnumerable<PayClassification>> GetPayClassificationNoCacheAsync()
        {
            {
                try
                {
                    string criteria = string.Empty;
                    var payClassificationsKeys = await DataReader.SelectAsync("SWTABLES", criteria);
                    var payClassificationsRecords = new List<PayClassification>();

                    if (payClassificationsKeys != null && payClassificationsKeys.Any())
                    {
                        Array.Sort(payClassificationsKeys);
                        if (payClassificationsKeys != null && payClassificationsKeys.Any())
                        {
                            try
                            {
                                var payClassificationRecords = await DataReader.BulkReadRecordAsync<DataContracts.Swtables>(payClassificationsKeys);
                                foreach (var payTable in payClassificationRecords)
                                {
                                    payClassificationsRecords.Add(BuildPayClassifications(payTable));
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }
                    return payClassificationsRecords;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Get PayClassifications entity for a specific id
        /// </summary>   
        /// <param name="id">id of the PayClassifications record.</param>
        /// <returns>PayClassifications Entity <see cref="PayClassifications"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PayClassification> GetPayClassificationsByIdAsync(string id)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "SWTABLES")
            {
                throw new KeyNotFoundException(string.Format("PayClassifications not found for id {0}", id));
            }
            var payClassificationId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(payClassificationId))
            {
                throw new KeyNotFoundException("PayClassification id " + id + "does not exist");
            }
            var payClassificationRecord = await DataReader.ReadRecordAsync<DataContracts.Swtables>("SWTABLES", payClassificationId);
            if (payClassificationRecord == null)
            {
                throw new KeyNotFoundException("PayClassification not found with id " + id);
            }
            return BuildPayClassifications(payClassificationRecord);
        }
        
        /// <summary>
        /// Helper to build PersonPosition objects
        /// </summary>
        /// <param name="employRecord">the Perpos db record</param>
        /// <returns></returns>
        private Ellucian.Colleague.Domain.HumanResources.Entities.PayClassification BuildPayClassifications(Swtables payClassificationRecord)
        {
            PayClassification payClassificationEntity = new PayClassification(payClassificationRecord.RecordGuid, payClassificationRecord.Recordkey, !string.IsNullOrEmpty(payClassificationRecord.SwtDesc) ? payClassificationRecord.SwtDesc : payClassificationRecord.Recordkey);

            payClassificationEntity.CompensationType = payClassificationRecord.SwtHrlyOrSlry;
            payClassificationEntity.ClassificationType = payClassificationRecord.SwtType;
            payClassificationEntity.Status = payClassificationRecord.SwtStatus;

            return payClassificationEntity;
        }
    }
}
