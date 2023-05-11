/* Copyright 2016-2022 Ellucian Company L.P. and its affiliates. */
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
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonEmploymentStatusRepository : BaseColleagueRepository, IPersonEmploymentStatusRepository
    {
        private readonly int bulkReadSize;
        public PersonEmploymentStatusRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Retrieves person employment statuses for specified person ids
        /// </summary>
        /// <param name="personIds">person ids to retrieve wages for</param>
        /// <param name="lookupStartDate">optional parameter for look up start date filtering,
        /// all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonEmploymentStatus>> GetPersonEmploymentStatusesAsync(IEnumerable<string> personIds, DateTime? lookupStartDate = null)
        {
            if (personIds == null) { throw new ArgumentNullException("PersonIds"); }
            if (!personIds.Any()) { throw new ArgumentException("Cannot pass in an empty list of personIds to get PersonEmploymentStatuses"); }

            var criteria = "WITH PERSTAT.HRP.ID EQ ?";
            if (lookupStartDate.HasValue)
            {
                criteria += " AND (PERSTAT.END.DATE GE '" + UniDataFormatter.UnidataFormatDate(lookupStartDate.Value, InternationalParameters.HostShortDateFormat, InternationalParameters.HostDateDelimiter)
                    + "' OR PERSTAT.END.DATE EQ '')";
            }
            var perstatKeys = await DataReader.SelectAsync("PERSTAT", criteria, personIds.Select(id => string.Format("\"{0}\"", id)).ToArray()); // wrap each Id in quotes for successful read

            if (perstatKeys == null)
            {
                var message = "Unexpected null returned from PERSTAT SelectAsyc";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!perstatKeys.Any())
            {
                logger.Error("No PERSTAT keys exist for the given person Ids: " + string.Join(",", personIds));
            }

            var perstatRecords = new List<Perstat>();
            for (int i = 0; i < perstatKeys.Count(); i += bulkReadSize)
            {
                var subList = perstatKeys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<Perstat>(subList.ToArray());
                if (records == null)
                {
                    logger.Error("Unexpected null from bulk read of Perstat records");
                }
                else
                {
                    perstatRecords.AddRange(records);
                }
            }

            var domainPersonEmploymentStatuses = new List<PersonEmploymentStatus>();
            foreach (var record in perstatRecords)
            {
                PersonEmploymentStatus entityToAdd = null;
                try
                {
                    if (!string.IsNullOrEmpty(record.PerstatPrimaryPosId) && !string.IsNullOrEmpty(record.PerstatPrimaryPerposId))
                    {
                        entityToAdd = new PersonEmploymentStatus(
                        record.Recordkey,
                        record.PerstatHrpId,
                        record.PerstatPrimaryPosId,
                        record.PerstatPrimaryPerposId,
                        record.PerstatStartDate,
                        record.PerstatEndDate
                    );
                    }

                }
                catch (Exception e)
                {
                    // we don't want to use this record if there is an error creating it
                    LogDataError("Perstat", record.Recordkey, record, e, e.Message);
                    entityToAdd = null;
                }
                if (entityToAdd != null)
                    domainPersonEmploymentStatuses.Add(entityToAdd);
            }
            return domainPersonEmploymentStatuses;
        }
    }
}
