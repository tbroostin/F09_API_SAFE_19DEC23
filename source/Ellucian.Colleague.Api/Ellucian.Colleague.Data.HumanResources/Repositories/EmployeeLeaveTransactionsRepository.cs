/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
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
        RepositoryException exception = null;
        const string AllEmployeeLeaveTransactionsCache = "AllEmployeeLeaveTransactions";
        const int AllEmployeeLeaveTransactionsCacheTimeout = 20; // Clear from cache every 20 minutes

        public EmployeeLeaveTransactionsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get PerleaveDetails objects for all records.
        /// </summary>
        /// <param name="offset">Offset for record index on page reads.</param>
        /// <param name="limit">Take number of records on page reads.</param>
        /// <returns>Tuple of PerleaveDetails Entity objects <see cref="PerleaveDetails"/> and a count for paging.</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails>, int>> GetEmployeeLeaveTransactionsAsync(int offset, int limit, bool bypassCache = false)
        {

            //string criteria = string.Empty;
            int totalCount = 0;

            string[] subList = null;
            string employeeLeaveTransactionsCacheKey = CacheSupport.BuildCacheKey(AllEmployeeLeaveTransactionsCache);

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(

                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                employeeLeaveTransactionsCacheKey,
                "PERLVDTL",
                offset,
                limit,
                AllEmployeeLeaveTransactionsCacheTimeout,
                async () => {
                    return  new CacheSupport.KeyCacheRequirements() { };
                });
            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())

            {
                return new Tuple<IEnumerable<Domain.HumanResources.Entities.PerleaveDetails>, int>(null, 0);
            }

            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

            var employeeLeaveTransactionsRecords = new List<Domain.HumanResources.Entities.PerleaveDetails>();        
            var empLeaveplanRecords = await DataReader.BulkReadRecordAsync<DataContracts.Perlvdtl>("PERLVDTL", subList);
            foreach (var plan in empLeaveplanRecords)
            {
                try
                {
                    employeeLeaveTransactionsRecords.Add(BuildEmployeeLeaveTransactions(plan));
                }

                catch (Exception ex)
                {
                    if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "An unexpected error occurred extracting the Employee-leave-transaction data."));
                }
            }

            if (exception != null && exception.Errors.Any())
                throw exception;

            return new Tuple<IEnumerable<Domain.HumanResources.Entities.PerleaveDetails>, int>(employeeLeaveTransactionsRecords, totalCount);

        }

        /// <summary>
        /// Get PerleaveDetailsentity for a specific id
        /// </summary>   
        /// <param name="id">id of the PerleaveDetails record.</param>
        /// <returns>PerleaveDetails Entity <see cref="Domain.HumanResources.Entities.PerleaveDetails"./></returns>
        public async Task<Ellucian.Colleague.Domain.HumanResources.Entities.PerleaveDetails> GetEmployeeLeaveTransactionsByIdAsync(string id)
        {
            var entity = await this.GetRecordInfoFromGuidAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException(string.Format("EmployeeLeaveTransactions not found for id {0}", id));
            }
            if (entity.Entity != "PERLVDTL")
            {
                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("GUID.Wrong.Type", "GUID '" + id + "' has different entity, '" + entity.Entity + "', than expected, 'PERLVDTL'"));
                throw exception;
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
            var retval = BuildEmployeeLeaveTransactions(empLeavePlanRecord);

            if (exception != null && exception.Errors.Any())
                throw exception;
            return retval;

        }

        /// <summary>
        /// Helper to build PerleaveDetails domain entity
        /// </summary>
        /// <param name="empLeavePlanTranRecord">the Perlvdtl data contract</param>
        /// <returns>PerleaveDetails domain entity</returns>
        private Domain.HumanResources.Entities.PerleaveDetails BuildEmployeeLeaveTransactions(DataContracts.Perlvdtl empLeavePlanTranRecord)
        {

            if (empLeavePlanTranRecord == null)
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "An unexpected error occurred attempting to build the PerleaveDetails domain entity."));
                throw exception;
            }

            if (string.IsNullOrEmpty(empLeavePlanTranRecord.RecordGuid))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeaveTransactions guid can not be null or empty. Entity: 'PERLVDTL'")
                {Id = empLeavePlanTranRecord.RecordGuid, SourceId = empLeavePlanTranRecord.Recordkey });
            }
            if (string.IsNullOrEmpty(empLeavePlanTranRecord.Recordkey))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeaveTransactions id can not be null or empty. Entity: 'PERLVDTL'")
                { Id = empLeavePlanTranRecord.RecordGuid, SourceId = empLeavePlanTranRecord.Recordkey });
            }
            if (string.IsNullOrEmpty(empLeavePlanTranRecord.PldPerleaveId))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeaveTransactions Leave Id can not be null or empty. Entity: 'PERLVDTL'")
                { Id = empLeavePlanTranRecord.RecordGuid, SourceId = empLeavePlanTranRecord.Recordkey });
            }
            if ((empLeavePlanTranRecord.PldDate == null) || (!empLeavePlanTranRecord.PldDate.HasValue))
            {
                if (exception == null)
                    exception = new RepositoryException();
                exception.AddError(new RepositoryError("Bad.Data", "EmployeeLeaveTransactions Transaction Date can not be null or empty. Entity: 'PERLVDTL'")
                { Id = empLeavePlanTranRecord.RecordGuid, SourceId = empLeavePlanTranRecord.Recordkey });
            }

            if (exception != null && exception.Errors.Any())
                throw exception;

            var empLeaveplanEntity = new Domain.HumanResources.Entities.PerleaveDetails(empLeavePlanTranRecord.RecordGuid, 
                empLeavePlanTranRecord.Recordkey,empLeavePlanTranRecord.PldDate,empLeavePlanTranRecord.PldPerleaveId)
            {
                LeaveHours = empLeavePlanTranRecord.PldHours,
                
                AvailableHours = empLeavePlanTranRecord.PldForwardingBalance
            };
            return empLeaveplanEntity;
        }
    }
}
