/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
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
    /// Repository for StipendHistory endpoint
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonStipendRepository : BaseColleagueRepository, IPersonStipendRepository
    {
        private readonly int bulkReadSize;

        public PersonStipendRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get PersonStipends for the given personIds
        /// </summary>
        /// <param name="personIds">a list of personids</param>
        /// <returns></returns>
        public async Task<IEnumerable<PersonStipend>> GetPersonStipendAsync(IEnumerable<string> personIds)
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
                throw new KeyNotFoundException("No perposwg keys found for the given person ids");
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

            //Filter out all non-stipend PERPOSWG records - A stipend record will not have any value for the field PpwgPospayId
            perposwgRecords = perposwgRecords.Where(p => p.PpwgPospayId == null || p.PpwgPospayId == string.Empty).ToList();

            if (!perposwgRecords.Any())
            {
                var message = "No PersonStipend records available for the given personIds";
                logger.Error(message);
            }

            var personStipendEntities = new List<PersonStipend>();

            foreach (var perposwgRecord in perposwgRecords)
            {
                if (perposwgRecord != null)
                {
                    try
                    {
                        // Build the PersonStipend object
                        personStipendEntities.Add(BuildPersonStipendItem(perposwgRecord));
                    }
                    catch (Exception e)
                    {
                        LogDataError("Perposwg", perposwgRecord.Recordkey, perposwgRecord, e, e.Message);
                    }
                }
            }
    
            return personStipendEntities;
        }

        /// <summary>
        /// Helper to build a PersonStipend object based on a PERPOSWG record
        /// </summary>
        /// <param name="ppwgRecord"></param>
        private PersonStipend BuildPersonStipendItem(Perposwg ppwgRecord)
        {
            if (ppwgRecord == null)
            {
                throw new ArgumentNullException("ppwgRecord");
            }
            if (!ppwgRecord.PpwgStartDate.HasValue)
            {
                throw new ArgumentException("PERPOSWG Start Date must have a value", "ppwgRecord.PpwgStartDate");
            }

            var personStipend = new PersonStipend(
            ppwgRecord.Recordkey,
            ppwgRecord.PpwgHrpId,
            ppwgRecord.PpwgPositionId,
            ppwgRecord.PpwgStartDate.Value,
            ppwgRecord.PpwgEndDate,
            ppwgRecord.PpwgDesc,
            ppwgRecord.PpwgBaseAmt,
            ppwgRecord.PpwgPayrollDesignation,
            ppwgRecord.PpwgNoPayments,
            ppwgRecord.PpwgNoPaymentsTaken,
            ppwgRecord.PpwgCourseSecAsgmt,
            ppwgRecord.PpwgAdvisorAsgmt,
            ppwgRecord.PpwgMembershipAsgmt);

            return personStipend;
        }
    }
}
