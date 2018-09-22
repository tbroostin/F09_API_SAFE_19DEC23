/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class EmployeeLeaveTransactionsRepository : BaseColleagueRepository, IEmployeeLeaveTransactionsRepository
    {
        public EmployeeLeaveTransactionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get EmployeeLeaveTransactions objects for all EmployeeLeaveTransactions bypassing cache and reading directly from the database.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <returns>Tuple of PerleaveDetails Entity objects <see cref="PerleaveDetails"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails>, int>> GetEmployeeLeaveTransactionsAsync(int offset, int limit, bool bypassCache = false)
        {
            try
            {
                string criteria = string.Empty;
                var EmployeeLeaveTransactionsKeys = await DataReader.SelectAsync("PERLVDTL", criteria);
                var EmployeeLeaveTransactionsRecords = new List<Domain.HumanResources.Entities.PerleaveDetails>();
                var totalCount = 0;
                if (EmployeeLeaveTransactionsKeys == null || !EmployeeLeaveTransactionsKeys.Any())
                {
                    return new Tuple<IEnumerable<Domain.HumanResources.Entities.PerleaveDetails>, int>(null, 0);
                }

                totalCount = EmployeeLeaveTransactionsKeys.Count();
                Array.Sort(EmployeeLeaveTransactionsKeys);
                var EmployeeLeaveTransactionsubList = EmployeeLeaveTransactionsKeys.Skip(offset).Take(limit);
                if (EmployeeLeaveTransactionsubList != null && EmployeeLeaveTransactionsubList.Any())
                {
                    try
                    {
                        var empLeaveplanRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perlvdtl>(EmployeeLeaveTransactionsubList.ToArray(), bypassCache);
                        foreach (var plan in empLeaveplanRecords)
                        {

                            EmployeeLeaveTransactionsRecords.Add(await BuildEmployeeLeaveTransactions(plan));
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                return new Tuple<IEnumerable<Domain.HumanResources.Entities.PerleaveDetails>, int>(EmployeeLeaveTransactionsRecords, totalCount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get EmployeeLeaveTransactions entity for a specific id
        /// </summary>   
        /// <param name="id">id of the EmployeeLeaveTransactions record.</param>
        /// <returns>EmployeeLeaveTransactions Entity <see cref="Domain.HumanResources.Entities.PerleaveDetails"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails> GetEmployeeLeaveTransactionsByIdAsync(string id)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null || entity.Entity != "PERLVDTL")
            {
                throw new KeyNotFoundException(string.Format("EmployeeLeaveTransactions not found for id {0}", id));
            }
            var empLeaveplanId = entity.PrimaryKey;
            if (string.IsNullOrWhiteSpace(empLeaveplanId))
            {
                throw new KeyNotFoundException("EmployeeLeaveTransactions id " + id + "does not exist");
            }
            var empLeavePlanRecord = await DataReader.ReadRecordAsync<DataContracts.Perlvdtl>("PERLVDTL", empLeaveplanId);
            if (empLeavePlanRecord == null)
            {
                throw new KeyNotFoundException("EmployeeLeaveTransactions not found with id " + id);
            }
            return await BuildEmployeeLeaveTransactions(empLeavePlanRecord);
        }

        /// <summary>
        /// Helper to build employee Leave Transactions Entity
        /// </summary>
        /// <param name="empLeavePlanTranRecord">the Perlvdtl db record</param>
        /// <returns></returns>
        private async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails> BuildEmployeeLeaveTransactions(DataContracts.Perlvdtl empLeavePlanTranRecord)
        {
            Domain.HumanResources.Entities.PerleaveDetails empLeaveplanEntity = new Domain.HumanResources.Entities.PerleaveDetails(empLeavePlanTranRecord.RecordGuid, empLeavePlanTranRecord.Recordkey,empLeavePlanTranRecord.PldDate,empLeavePlanTranRecord.PldPerleaveId)
            {
                LeaveHours = empLeavePlanTranRecord.PldHours,
                //AvailableHours = empLeavePlanTranRecord.PldCurrentBalance
                AvailableHours = empLeavePlanTranRecord.PldForwardingBalance
            };
            return empLeaveplanEntity;
        }
    }
}
