/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    /// <summary>
    /// Repository for PersonPositionWage endpoints
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonPositionWageRepository : BaseColleagueRepository, IPersonPositionWageRepository
    {
        private readonly int bulkReadSize;

        public PersonPositionWageRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get PersonPositionWages for the given personIds
        /// </summary>
        /// <param name="personIds">a list of personids for whom to get PersonPositionWages</param>
        /// <param name="lookupStartDate"> optional look up start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonPositionWage>> GetPersonPositionWagesAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null, 
            IEnumerable<string> payCycleIds = null)
        {
            if (personIds == null)
            {
                throw new ArgumentNullException("personIds");
            }
            if (!personIds.Any())
            {
                throw new ArgumentException("personIds is required to get PersonPositionWages");
            }

            //select all the PERPOSWG ids with the HRP.ID equal to the input personids
            var criteria = "WITH PPWG.HRP.ID EQ ?";
            if (lookupStartDate.HasValue)
            {
                criteria += " AND (PPWG.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter) 
                    + "' OR PPWG.END.DATE EQ '')";
            }            
            var perposwgKeys = await DataReader.SelectAsync("PERPOSWG", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray());
            if (perposwgKeys == null)
            {
                var message = "Unexpected null returned from PERPOSWG SelectAsync";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!perposwgKeys.Any())
            {
                logger.Error("No PERPOSWG keys exist for the given person Ids: " + string.Join(",", personIds));
            }

            //bulkread the records in chunks for all the keys
            var perposwgRecords = new List<Perposwg>();
            for (int i = 0; i < perposwgKeys.Count(); i += bulkReadSize)
            {
                var subList = perposwgKeys.Skip(i).Take(bulkReadSize);
                var selectedRecords = await DataReader.BulkReadRecordAsync<Perposwg>(subList.ToArray());
                if (selectedRecords == null)
                {
                    logger.Error("Unexpected null from bulk read of Perposwg records");
                }
                else
                {
                    perposwgRecords.AddRange(selectedRecords);
                }
            }

            //filter records by paycycle ids if any were provided
            List<Perposwg> wageRecords = null;
            if (payCycleIds != null && payCycleIds.Any())
            {
                wageRecords = perposwgRecords.Where(r => payCycleIds.Contains(r.PpwgPaycycleId)).ToList();
            }
            else
            {
                wageRecords = perposwgRecords;
            }

            //find all the POSPAY ids in the perposwgRecords PPWG_POSPAY_ID
            var posPayRecords = new Dictionary<string, Pospay>();

            var posPayIds = wageRecords
                .Where(rec => rec != null && !string.IsNullOrEmpty(rec.PpwgPospayId))
                .Select(i => i.PpwgPospayId)
                .Distinct()
                .ToArray();
            if (!posPayIds.Any())
            {
                logger.Info("No POSPAY keys exist for the given perposKeys: " + string.Join(",", perposwgKeys.ToString()));
            }
            else
            {

                var records = await DataReader.BulkReadRecordAsync<Pospay>(posPayIds);
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Perposwg records");
                }
                else
                {
                    posPayRecords = records.ToDictionary(r => r.Recordkey);
                }
            }

            var personPositionWageEntities = new List<PersonPositionWage>();
            foreach (var perposwgRecord in wageRecords)
            {
                if (perposwgRecord != null)
                {
                    try
                    {
                        var posPay = posPayRecords.ContainsKey(perposwgRecord.PpwgPospayId) ? posPayRecords[perposwgRecord.PpwgPospayId] : null;
                        var personPositionWage = BuildPersonPositionWage(perposwgRecord, posPay);
                        if (personPositionWage != null)
                        {
                            personPositionWageEntities.Add(personPositionWage);
                        }
                    }
                    catch (Exception e)
                    {
                        LogDataError("Perposwg", perposwgRecord.Recordkey, perposwgRecord, e, e.Message);
                    }
                }
            }

            return personPositionWageEntities;
        }
        
        /// <summary>
        /// Helper to build a PersonPositionWage object based on a PERPOSWG record
        /// </summary>
        /// <param name="perposwgRecord"></param>
        /// <returns></returns>
        private PersonPositionWage BuildPersonPositionWage(Perposwg perposwgRecord, Pospay posPayRecord)
        {
            if (perposwgRecord == null)
            {
                throw new ArgumentNullException("perposwgRecord");
            }

            if (!perposwgRecord.PpwgStartDate.HasValue)
            {
                throw new ArgumentException("Perposwg Start Date must have value", "perposwgRecord.PpwgStartDate");
            }

            var earningsTypeGroupId = posPayRecord != null ? posPayRecord.PospayEarntypeGrouping : null;
                
                
            var fundingSources = new List<PositionFundingSource>();
            foreach (var payItemRecord in perposwgRecord.PpwitemsEntityAssociation)
            {
                var fundingSource = new PositionFundingSource(
                    payItemRecord.PpwgPiFndsrcIdAssocMember,
                    perposwgRecord.PpwitemsEntityAssociation.IndexOf(payItemRecord))
                {
                    ProjectId = payItemRecord.PpwgProjectsIdsAssocMember
                };

                fundingSources.Add(fundingSource);
            }

            PersonPositionWage personPositionWage = null;
            //Determine whether this is a regular wage - if not do not create a PersonPositionWage object
            bool isRegularWage = !string.IsNullOrEmpty(perposwgRecord.PpwgType) && perposwgRecord.PpwgType.Equals("W", StringComparison.InvariantCultureIgnoreCase);
            if (isRegularWage)
            {
                personPositionWage = new PersonPositionWage(perposwgRecord.Recordkey,
                    perposwgRecord.PpwgHrpId,
                    perposwgRecord.PpwgPositionId,
                    perposwgRecord.PpwgPerposId,
                    perposwgRecord.PpwgPospayId,
                    perposwgRecord.PpwgPayclassId,
                    perposwgRecord.PpwgPaycycleId,
                    perposwgRecord.PpwgBaseEt,
                    perposwgRecord.PpwgStartDate.Value,
                    earningsTypeGroupId)
                {
                    EndDate = perposwgRecord.PpwgEndDate,
                    IsPaySuspended = !string.IsNullOrEmpty(perposwgRecord.PpwgSuspendPayFlag) && perposwgRecord.PpwgSuspendPayFlag.Equals("Y", StringComparison.InvariantCultureIgnoreCase),
                    FundingSources = fundingSources
                };
            }

            return personPositionWage;
        }
    }
}
