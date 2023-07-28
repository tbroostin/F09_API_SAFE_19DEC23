/* Copyright 2017-2023 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Pay Statement Repository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PayStatementRepository : BaseColleagueRepository, IPayStatementRepository
    {
        private readonly int bulkReadSize;
        private readonly string PayStatementDataCacheKeySuffix;

        /// <summary>
        /// Repository Constructor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="settings"></param>
        public PayStatementRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings) : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
            PayStatementDataCacheKeySuffix = "PayStatementSummaryData";
        }

        /// <summary>
        /// Get a list of PayStatementSourceData objects for the given record ids
        /// </summary>
        /// <param name="ids">Required: a list of record ids of Pay Statement objects. must have at least one item</param>
        /// <returns></returns>
        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataAsync(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                throw new ArgumentNullException("ids");
            }

            var payStatementEntities = new List<PayStatementSourceData>();
            var records = await DataReader.BulkReadRecordAsync<WebPayAdvices>(ids.Distinct().ToArray());
            if (records == null || !records.Any())
            {
                logger.Debug("************Pay Statement source (WebPayAdvice) data for the requested ID(s), not found in the database************");
                return payStatementEntities;
            }

            foreach (var record in records)
            {
                try
                {
                    var entity = BuildPayStatementSourceData(record);
                    payStatementEntities.Add(entity);
                }
                catch (Exception e)
                {
                    LogDataError("WEB.PAY.ADVICE", record.Recordkey, new object(), e, "Error building PayStatementSource");
                }
            }
            logger.Debug("************Pay Statement source data(WebPayAdvice) entities created successfully************");
            return payStatementEntities;
        }

        /// <summary>
        /// Get PayStatementSourceData object for a single WEB.PAY.ADVICES ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PayStatementSourceData> GetPayStatementSourceDataAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var record = await DataReader.ReadRecordAsync<WebPayAdvices>(id);
            if (record == null)
            {
                logger.Debug("************Pay Statement data for the requested ID, not found in the database************");
                throw new KeyNotFoundException(string.Format("WebPayAdvice with id {0} not found in database", id));
            }

            try
            {
                return BuildPayStatementSourceData(record);
            }
            catch (Exception e)
            {
                LogDataError("WEB.PAY.ADVICE", record.Recordkey, "Error building single PayStatementSource");
                throw new ApplicationException(string.Format("Error building single PayStatementSource {0}", record.Recordkey), e);
            }
        }

        /// <summary>
        /// Get PayStatementSourceData objects for the given personId
        /// 
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(string personId, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId");
            }

            var cacheKey = string.Format("{0}-{1}", personId, PayStatementDataCacheKeySuffix);

            var payStatementSourceData = await GetOrAddToCacheAsync<IEnumerable<PayStatementSourceData>>(cacheKey,
                async () =>
                {
                    var wpaCriteria = string.Format("WITH WPA.INDEX.EMP.ID EQ '{0}'", personId);
                    logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - emp ID - data reader - query string: '{0}'.************", wpaCriteria));

                    if (startDate.HasValue)
                    {
                        wpaCriteria = string.Concat(wpaCriteria, string.Format(" AND WITH WPA.CHECK.DATE GE '{0}'", startDate));
                        logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - startdate - data reader - query string: '{0}'.************", wpaCriteria));

                    }

                    if (endDate.HasValue)
                    {
                        wpaCriteria = string.Concat(wpaCriteria, string.Format(" AND WITH WPA.CHECK.DATE LE '{0}'", endDate));
                        logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - endDate - data reader - query string: '{0}'************.", wpaCriteria));
                    }

                    var records = await BulkReadWebPayAdvicesAsync(string.Format(wpaCriteria));

                    var sourceDataObjects = new List<PayStatementSourceData>();
                    if (!records.Any())
                    {
                        logger.Debug("************Web Pay Advice records not found. No data retrived************");
                        return sourceDataObjects;
                    }

                    foreach (var record in records)
                    {
                        try
                        {
                            var sourceData = BuildPayStatementSourceData(record);
                            sourceDataObjects.Add(sourceData);
                        }
                        catch (Exception e)
                        {
                            LogDataError("WEB.PAY.ADVICE", record.Recordkey, new object(), e, "Error building PayStatement for employee " + personId);
                        }
                    }
                    return sourceDataObjects;


                }, Level1CacheTimeoutValue);


            return payStatementSourceData;
        }

        public async Task<IEnumerable<PayStatementSourceData>> GetPayStatementSourceDataByPersonIdAsync(IEnumerable<string> personIds, DateTime? startDate, DateTime? endDate)
        {
            if (personIds == null || !personIds.Any())
            {
                logger.Debug("************PersonIds are null or empty. Unable to retrieve PayStatement Summary.************");
                throw new ArgumentNullException("personIds");
            }

            personIds = personIds.Distinct();

            //this is the return list
            var payStatementSourceDataEntities = new List<PayStatementSourceData>();

            //build a set of tuples with 
            //item1: personId
            //item2: full cache key
            var personIdCacheKeys = personIds
                .Select(id => new Tuple<string, string>(id, BuildFullCacheKey(BuildPersonPayStatementSourceCacheKey(id))))
                .ToList();

            //find the tuples where the full cache key is already contained in the cache provider
            var keysInCache = personIdCacheKeys.Where(tuple => _cacheProvider.Contains(tuple.Item2));
            foreach (var entry in keysInCache)
            {
                try
                {
                    //for each of the tuples, get the object from the cache and cast it as an IEnumerable<PersonBenefitDeduction>
                    var cachedEntities = (IEnumerable<PayStatementSourceData>)_cacheProvider.Get(entry.Item2);
                    if (cachedEntities != null)
                    {
                        payStatementSourceDataEntities.AddRange(cachedEntities);
                    }
                }
                catch (Exception e)
                {
                    logger.Debug(e.Message, string.Format("************Unable to get cached PayStatementSourceData from cacheKey {0}************", entry.Item2));
                }
            }

            //find the tuples where the full cache key is not contained in the cache provider
            //and get the personids from those tuples.
            var uncachedPersonIds = personIdCacheKeys
                .Where(tuple => !_cacheProvider.Contains(tuple.Item2))
                .Select(tuple => tuple.Item1)
                .ToList();

            if (!uncachedPersonIds.Any())
            {
                return payStatementSourceDataEntities;
            }

            //bulk read webPayADvice records for the employeeids that don't have a cache entry.
            var criteria = "WITH WPA.INDEX.EMP.ID EQ ?";
            if (startDate.HasValue)
            {
                var uniDataStartDate = await GetUnidataFormatDateAsync(startDate.Value);
                criteria = string.Concat(criteria, string.Format(" AND WITH WPA.CHECK.DATE GE '{0}'", uniDataStartDate));
                logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - startDate - data reader - query string: '{0}'************.", criteria));

            }

            if (endDate.HasValue)
            {
                var uniDataEndDate = await GetUnidataFormatDateAsync(endDate.Value);
                criteria = string.Concat(criteria, string.Format(" AND WITH WPA.CHECK.DATE LE '{0}'", uniDataEndDate));
                logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - endDate - data reader - query string: '{0}'.************", criteria));
            }

            var substitutionList = uncachedPersonIds.Select(id => string.Format("\"{0}\"", id));
            logger.Debug(string.Format("************PayStatementSourceData(Web Pay Advice) - Final query criteria - data reader -'{0}'************.", criteria));

            var webPayAdviceKeys = await DataReader.SelectAsync("WEB.PAY.ADVICES", criteria, substitutionList.ToArray());
            if (webPayAdviceKeys == null || !webPayAdviceKeys.Any())
            {
                logger.Debug("************Web Pay Advice Keys not found. No data retrived************");
                return payStatementSourceDataEntities;
            }
            var webPayAdviceRecords = new List<WebPayAdvices>();
            for (int i = 0; i < webPayAdviceKeys.Count(); i += bulkReadSize)
            {
                var records = await DataReader.BulkReadRecordAsync<WebPayAdvices>(webPayAdviceKeys.Skip(i).Take(bulkReadSize).ToArray());
                if (records != null)
                {
                    webPayAdviceRecords.AddRange(records);
                }
            }

            if (!webPayAdviceRecords.Any())
            {
                logger.Debug("************Web Pay Advice records not found. No data retrived************");
                return payStatementSourceDataEntities;
            }

            //group the records by employee id in order to cache objects for each employee.
            var recordsByEmployeeId = webPayAdviceRecords.GroupBy(r => r.WpaEmployeId);
            foreach (var recordGroup in recordsByEmployeeId)
            {
                var entities = GetOrAddToCache(BuildPersonPayStatementSourceCacheKey(recordGroup.Key),
                    () =>
                    {
                        var sourceDataEntities = new List<PayStatementSourceData>();
                        foreach (var webPayAdviceRecord in recordGroup)
                        {
                            try
                            {
                                var entity = BuildPayStatementSourceData(webPayAdviceRecord);
                                sourceDataEntities.Add(entity);

                            }
                            catch (Exception e)
                            {
                                //revisit this
                                LogDataError("WEB.PAY.ADVICE", webPayAdviceRecord.Recordkey, new object(), e, "Error building PayStatement for employee " + recordGroup.Key);
                            }
                        }
                        logger.Debug(string.Format("************PayStatement Entities Count := {0} ************", sourceDataEntities.Count()));

                        return sourceDataEntities;
                    }, Level1CacheTimeoutValue);

                payStatementSourceDataEntities.AddRange(entities);

            }

            logger.Debug(string.Format("--> Total PayStatement Entities := {0} --<", payStatementSourceDataEntities.Count()));
            return payStatementSourceDataEntities;
        }

        /// <summary>
        /// Helper to Build an employee specific cache key
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        private string BuildPersonPayStatementSourceCacheKey(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                logger.Debug("************PersonId is null or empty. Failed to build employee specific cache key.  Unable to retrieve PayStatement Summary.************");
                throw new ArgumentNullException("personId");
            }
            return string.Format("{0}-{1}", personId, PayStatementDataCacheKeySuffix);
        }

        /// <summary>
        /// Get PayStatementSourceData objects for the given criteria. Criteria are applied using the AND operator.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private async Task<IEnumerable<WebPayAdvices>> BulkReadWebPayAdvicesAsync(string criteria = "")
        {
            var webPayAdvices = new List<WebPayAdvices>();
            var webPayAdviceKeys = await DataReader.SelectAsync("WEB.PAY.ADVICES", criteria);
            if (webPayAdviceKeys == null || !webPayAdviceKeys.Any())
            {
                return webPayAdvices;
            }
            for (var i = 0; i < webPayAdviceKeys.Count(); i += bulkReadSize)
            {
                var records = await DataReader.BulkReadRecordAsync<WebPayAdvices>("WEB.PAY.ADVICES", webPayAdviceKeys.Skip(i).Take(bulkReadSize).ToArray());
                if (records != null)
                {
                    webPayAdvices.AddRange(records);
                }
            }
            return webPayAdvices;
        }

        private PayStatementSourceData BuildPayStatementSourceData(WebPayAdvices webPayAdviceRecord)
        {
            if (webPayAdviceRecord == null)
            {
                logger.Debug("************WebPayAdviceRecord is null. Unable to build PayStatement entity************");
                throw new ArgumentNullException("webPayAdviceRecord");
            }

            var employeeName = webPayAdviceRecord.WpaMailLabel.FirstOrDefault();
            var employeeMailingLabel = webPayAdviceRecord.WpaMailLabel.Skip(1)
                .Select(s => new PayStatementAddress(s));


            var payStatement = new PayStatementSourceData(
                        webPayAdviceRecord.Recordkey,
                        webPayAdviceRecord.WpaEmployeId,
                        employeeName,
                        webPayAdviceRecord.WpaSsn,
                        employeeMailingLabel,
                        webPayAdviceRecord.WpaCheckNumber,
                        webPayAdviceRecord.WpaAdviceNumber,
                        webPayAdviceRecord.WpaCheckDate.Value,
                        webPayAdviceRecord.WpaPeriodDate.Value,
                        webPayAdviceRecord.WpaGrossPay.Value,
                        webPayAdviceRecord.WpaNetPay.Value,
                        webPayAdviceRecord.WpaTotalYtdGrossPay.Value,
                        webPayAdviceRecord.WpaTotalYtdNetPay.Value,
                        webPayAdviceRecord.WpaComments,
                        webPayAdviceRecord.WpaTotalTaxes.Value,
                        webPayAdviceRecord.WpaTotalBendeds.Value);

            logger.Debug(string.Format("************ PayStatement Entity = {0},{1}************",
                        webPayAdviceRecord.WpaEmployeId, webPayAdviceRecord.WpaPeriodDate.Value));

            if (webPayAdviceRecord.DirDepInfoEntityAssociation.Any())
            {
                foreach (var deposit in webPayAdviceRecord.DirDepInfoEntityAssociation)
                {
                    try
                    {
                        payStatement.SourceBankDeposits.Add(buildPayStatementSourceBankDeposits(deposit));
                    }
                    catch (Exception e)
                    {
                        logger.Error(string.Format("Error adding SourceBankDeposit (bankcode: {0}) in WebPayAdvice record {1}", deposit.WpaDirDepositCodesAssocMember, webPayAdviceRecord.Recordkey));
                        throw;
                    }
                }
            }

            return payStatement;
        }

        private PayStatementSourceBankDeposit buildPayStatementSourceBankDeposits(WebPayAdvicesDirDepInfo depositInfoRecord)
        {
            var depositToAdd = new PayStatementSourceBankDeposit(
                depositInfoRecord.WpaDirDepositDescsAssocMember,
                ConvertInternalCode(depositInfoRecord.WpaDirDepositTypesAssocMember),
                depositInfoRecord.WpaDirDepAcctsLast4AssocMember,
                depositInfoRecord.WpaDirDepositAmtsAssocMember ?? 0
                );

            return depositToAdd;
        }

        /// <summary>
        /// Converts a string value to the internal bank account type
        /// </summary>
        /// <param name="internalCode"></param>
        /// <returns></returns>
        private BankAccountType ConvertInternalCode(string internalCode)
        {
            if (string.IsNullOrEmpty(internalCode))
            {
                return BankAccountType.Checking;
            }

            switch (internalCode.ToUpperInvariant())
            {
                case "D":
                    return BankAccountType.Checking;
                case "S":
                    return BankAccountType.Savings;
                default:
                    return BankAccountType.Checking;
            }
        }
    }
}
