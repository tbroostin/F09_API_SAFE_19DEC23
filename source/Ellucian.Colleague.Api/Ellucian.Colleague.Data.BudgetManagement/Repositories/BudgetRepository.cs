// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Data.Colleague.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.BudgetManagement.Repositories
{
    /// <summary>
    ///  This class implements the IBudgetRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BudgetRepository : BaseColleagueRepository, IBudgetRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public BudgetRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        { }

        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        private string _hostCountry;
        RepositoryException exception = null;

        public async Task<Tuple<IEnumerable<BudgetWork>, int>> GetBudgetPhaseLineItemsAsync(int offset,
            int limit, string budgetPhaseId, string accountingStringComponentValue, bool bypassCache = false)
        {
            var budgetWorkCollection = new List<BudgetWork>();
            int totalRecords = 0;
            var criteria = "WITH BU.LOCATION EQ 'O'";
            var accountingStringCriteria = string.Empty;
            var budgetWorkRecordKeyValue = new Dictionary<string, Collection<DataContracts.BudWork>>();

            if (!string.IsNullOrEmpty(budgetPhaseId))
            {
                criteria = string.Concat(criteria, " AND WITH BUDGET.ID EQ '", budgetPhaseId, "'");
            }
            if (!string.IsNullOrEmpty(accountingStringComponentValue))
            {
                accountingStringCriteria = string.Format("WITH BW.EXPENSE.ACCT EQ '{0}'", accountingStringComponentValue);
            }

            try
            {
                var budgetIds = await DataReader.SelectAsync("BUDGET", criteria);
                if ((budgetIds == null || !budgetIds.Any()))
                {
                    return new Tuple<IEnumerable<BudgetWork>, int>(null, 0);
                }

                var budgetWorkIds = new List<string>();
                foreach (var budgetId in budgetIds)
                {
                    var bwkFileName = string.Format("BWK.{0}", budgetId);
                    var bwkFileIds = await DataReader.SelectAsync(bwkFileName, accountingStringCriteria);
                    budgetWorkIds.AddRange(bwkFileIds.Select(ids => string.Concat(bwkFileName, "*", ids)));
                }

                totalRecords = budgetWorkIds.Count();
                budgetWorkIds.Sort();
                var subListIds = budgetWorkIds.Skip(offset).Take(limit);

                //we can have an list of Ids that looks like:   
                //                   file1*record1
                //                   file1*record2
                //                   file2*record1
                //                   file3*record1
                // so we need a new collection that groups the file name, and contains a sublist
                // or records associated with that filename

                var groups = subListIds.Select(x => x.Split('*'))
                   .GroupBy(x => x[0])
                   .ToDictionary(x => x.Key, x => x.Select(g => g[1]).Distinct().ToList());

         
                foreach (var group in groups)
                {
                    budgetWorkRecordKeyValue.Add(group.Key,
                        await DataReader.BulkReadRecordAsync<DataContracts.BudWork>(group.Key,
                        group.Value.ToArray()));
                }
            }
            catch (Exception ex)
            {

                if (exception == null)
                    exception = new RepositoryException();

                exception.AddError(new RepositoryError("Bad.Data", "Error occurred while getting budget work records: " + ex.Message));
                throw exception;
            }

            foreach (var budgetWorkRecords in budgetWorkRecordKeyValue)
            {
                try
                {
                    if (!string.IsNullOrEmpty(budgetWorkRecords.Key) && (budgetWorkRecords.Value != null))
                    {
                        var budgetPhase = string.Empty;
                        var budgetWorkRecord = budgetWorkRecords.Key.Split(new char[] { '.' }, 2);
                        if (budgetWorkRecord != null && budgetWorkRecord.Any() && budgetWorkRecord.Count() > 1)
                        {
                            budgetPhase = budgetWorkRecord[1];
                        }
                        foreach (var record in budgetWorkRecords.Value)
                        {

                            if (string.IsNullOrEmpty(record.RecordGuid))
                            {
                                if (exception == null)
                                    exception = new RepositoryException();

                                exception.AddError(new RepositoryError("GUID.Not.Found", string.Concat("Unable to find the GUID for budget-phase-line-items: fileName: "
                                    , budgetWorkRecord[0], ".instance: ", budgetWorkRecord[0], ".", budgetWorkRecord[1]))
                                    { SourceId = record.Recordkey } );
                 
                            }
                            else
                            {
                                var budgetWork = new BudgetWork(record.RecordGuid, record.Recordkey);
                                budgetWork.BudgetPhase = budgetPhase;
                                budgetWork.AccountingStringComponentValue = record.BwExpenseAcct;
                                budgetWork.LineAmount = record.BwLineAmt;
                                budgetWork.Comments = record.BwNotes;
                                budgetWork.HostCountry = await BuildHostCountry();
                                budgetWorkCollection.Add(budgetWork);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    if (exception == null)
                        exception = new RepositoryException();

                    exception.AddError(new RepositoryError("Bad.Data", "Error occurred while getting budget work records: " + ex.Message));
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<BudgetWork>, int>(budgetWorkCollection, totalRecords);
        }


        public async Task<Tuple<IEnumerable<Budget>, int>> GetBudgetPhasesAsync(string budgetCode, bool bypassCache = false)
        {
            var budgets = new List<Budget>();

            var criteria = "WITH BU.LOCATION EQ 'O'";
            if (!string.IsNullOrEmpty(budgetCode))
            {
                criteria = string.Concat(criteria, " AND WITH BUDGET.ID LIKE \"'", budgetCode, "_'...\" OR BUDGET.ID EQ \"", budgetCode, "\"");
            }
            var budgetIds = await DataReader.SelectAsync("BUDGET", criteria);
            if ((budgetIds == null || !budgetIds.Any()))
            {
                return new Tuple<IEnumerable<Budget>, int>(null, 0);
            }

            int totalRecords = budgetIds.Count();
            try
            {
                var budgetRecords = await DataReader.BulkReadRecordAsync<DataContracts.Budget>(budgetIds, bypassCache);
                foreach (var budgetRecord in budgetRecords)
                {
                    budgets.Add(await GetBudgetEntity(budgetRecord));

                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException("Error occurred while getting budget records: " + ex.Message, ex);
            }

            return new Tuple<IEnumerable<Budget>, int>(budgets, totalRecords);
        }

        public async Task<Tuple<IEnumerable<Budget>, int>> GetBudgetCodesAsync(bool bypassCache = false)
        {
            List<Budget> budgets = new List<Budget>();

            var criteria = "WITH BU.LOCATION EQ 'O' AND WITH @ID UNLIKE '..._...'";

            var budgetIds = await DataReader.SelectAsync("BUDGET", criteria);
            if ((budgetIds == null || !budgetIds.Any()))
            {
                return new Tuple<IEnumerable<Budget>, int>(null, 0);
            }

            int totalRecords = budgetIds.Count();
            try
            {
                var budgetRecords = await DataReader.BulkReadRecordAsync<DataContracts.Budget>(budgetIds, bypassCache);
                if (budgetRecords == null || !budgetRecords.Any())
                {
                    return new Tuple<IEnumerable<Budget>, int>(null, 0);
                }
                var budgetRecordIds = budgetRecords.Where(br => (!string.IsNullOrWhiteSpace(br.Recordkey))).Select(br => br.Recordkey).Distinct().ToArray();
                foreach (var budgetRecord in budgetRecords)
                {
                    if (budgetRecord.Recordkey != null)
                    {
                        if (budgetIds.Contains(budgetRecord.Recordkey))
                        {
                            Budget budgetEntity = await GetBudgetEntity(budgetRecord);
                            budgets.Add(budgetEntity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(string.Concat("Error occurred while getting budget records.  ",ex.Message), ex);
            }


            return new Tuple<IEnumerable<Budget>, int>(budgets, totalRecords);
        }

        public async Task<Budget> GetBudgetPhasesAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }
            var budgetId = await GetBudgetPhasesIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(budgetId))
            {
                throw new KeyNotFoundException(string.Format("No budget-phases was found for GUID {0}.", guid));
            }
            var budgetRecord = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", budgetId);
            if (budgetRecord == null)
            {
                throw new KeyNotFoundException(string.Format("No budget-phases was found for GUID {0}.", guid));
            }
            if (budgetRecord.BuLocation != "O")
            {
                throw new ArgumentException("This budget is not available as it is not marked as being online.");
            }

            return await GetBudgetEntity(budgetRecord);
        }

        public async Task<Budget> GetBudgetCodesAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }
            var budgetId = await GetBudgetCodesIdFromGuidAsync(guid);
            if (string.IsNullOrWhiteSpace(budgetId))
            {
                throw new KeyNotFoundException(string.Format("No budget code was found for guid {0}.", guid));
            }
            var budgetRecord = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", budgetId);
            if (budgetRecord == null || string.IsNullOrEmpty(budgetRecord.BuBudgetCodesIntgIdx))
            {
                throw new KeyNotFoundException(string.Format("No budget code was found for guid {0}.", guid));
            }
            if (budgetRecord.BuLocation != "O")
            {
                throw new ArgumentException("This budget is not available as it is not marked as being online.");
            }

            return await GetBudgetEntity(budgetRecord);
        }

        /// <summary>
        /// Returns a single BudgetWork domain entity record.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<BudgetWork> GetBudgetPhaseLineItemsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("BudgetPhaseLineItem not found for GUID " + guid);
            }

            var foundEntry = idDict.FirstOrDefault();

           
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("BudgetPhaseLineItem not found for GUID " + guid);
            }

            var recordInfo = foundEntry.Value;

            if (!recordInfo.Entity.StartsWith("BWK", StringComparison.OrdinalIgnoreCase))
            {
                throw new RepositoryException(string.Format("GUID '{0}' has different entity, {1}, than expected.", guid, recordInfo.Entity));
            }


            var budgetWorkRecord = await DataReader.ReadRecordAsync<DataContracts.BudWork>(recordInfo.Entity, recordInfo.PrimaryKey, true);
            if (budgetWorkRecord == null)
            {
                throw new KeyNotFoundException("BudgetPhaseLineItem not found for GUID " + guid);
            }


            var budgetPhaseId = recordInfo.Entity.Split(new char[] { '.' }, 2)[1];

            var budgetWork = new BudgetWork(budgetWorkRecord.RecordGuid, budgetWorkRecord.Recordkey);
            budgetWork.BudgetPhase = budgetPhaseId;
            budgetWork.AccountingStringComponentValue = budgetWorkRecord.BwExpenseAcct;
            budgetWork.LineAmount = budgetWorkRecord.BwLineAmt;
            budgetWork.Comments = budgetWorkRecord.BwNotes;
            budgetWork.HostCountry = await BuildHostCountry();

            return budgetWork;
        }

        public async Task<string> GetBudgetPhasesGuidFromIdAsync(string id)
        {
            try
            {
                var budgetPhasesGuid = await GetGuidFromRecordInfoAsync("BUDGET", id);
                if (string.IsNullOrEmpty(budgetPhasesGuid))
                {
                    throw new KeyNotFoundException(string.Format("No budget phase was found for id {0}.", id));
                }
                return budgetPhasesGuid;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }


        /// <summary>
        /// Gets a dictionary of guids for Budget records.
        /// </summary>
        /// <param name="budgetIds"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, string>> GetBudgetGuidCollectionAsync(IEnumerable<string> budgetIds)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (budgetIds != null && budgetIds.Any())
            {
                var budgetGuidLookup = budgetIds
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .Distinct().ToList()
                   .ConvertAll(gl => new RecordKeyLookup("BUDGET", gl, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(budgetGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!dict.ContainsKey(splitKeys[1]))
                    {
                        dict.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
            }

            return dict;
        }

        public async Task<string> GetBudgetCodesGuidFromIdAsync(string id)
        {
            try
            {
                var budgetCodesGuid = await GetGuidFromRecordInfoAsync("BUDGET", id, "BU.BUDGET.CODES.INTG.IDX", id);
                if (string.IsNullOrEmpty(budgetCodesGuid))
                {
                    throw new KeyNotFoundException(string.Format("No budget code was found for id {0}.", id));
                }
                return budgetCodesGuid;
            }
            catch (RepositoryException e)
            {
                throw new ArgumentException(e.Message);
            }
        }
        
        public async Task<string> GetBudgetPhasesIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException(string.Format("No budget-phases was found for GUID {0}.", guid));
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException(string.Format("No budget-phases was found for GUID {0}.", guid));
            }

            if (foundEntry.Value.Entity != "BUDGET")
            {
                throw new ArgumentException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, BUDGET");
            }

            return foundEntry.Value.PrimaryKey;
        }

        public async Task<string> GetBudgetCodesIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("Budget codes GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("Budget codes GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "BUDGET")
            {
                throw new ArgumentException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, BUDGET");
            }

            return foundEntry.Value.PrimaryKey;
        }

        private async Task<Budget> GetBudgetEntity(DataContracts.Budget budget)
        {
            var budgetEntity = new Budget()
            {
                RecordKey = budget.Recordkey,
            };

            budgetEntity.BudgetPhaseGuid = budget.RecordGuid;
            var budgetCode = budget.Recordkey.Split('_')[0];
            try
            {
                budgetEntity.BudgetCodeGuid = await GetGuidFromRecordInfoAsync("BUDGET", budgetCode, "BU.BUDGET.CODES.INTG.IDX", budgetCode);
            }
            catch (RepositoryException e)
            {
                throw new ArgumentException(e.Message);
            }

            budgetEntity.Title = budget.BuTitle;
            budgetEntity.CurrentVersionDesc = budget.BuCurrentVersionDesc;
            budgetEntity.CurrentVersionName = budget.BuCurrentVersionName;
            budgetEntity.Status = budget.BuStatus;
            budgetEntity.Version = budget.BuVersion;
            budgetEntity.BudgetCodesIntgIdx = budget.BuBudgetCodesIntgIdx;
            budgetEntity.FiscalYear = budget.BuFiscalYear;

            return budgetEntity;
        }

        private string GetGuid(string key, IDictionary<string, string> dict)
        {
            string guid = null;
            if (!string.IsNullOrEmpty(key))
            {
                if (dict.TryGetValue(key, out guid))
                {
                    return guid;
                }
            }
            return guid;
        }

        
        /// <summary>
        /// Builds host country.
        /// </summary>
        /// <returns></returns>
        private async Task<string> BuildHostCountry()
        {
            if (string.IsNullOrEmpty(_hostCountry))
            {
                var internationalParameters = await InternationalParametersAsync();
                _hostCountry = internationalParameters.HostCountry;
            }
            return _hostCountry;
        }
       

        /// <summary>
        /// Gets international parameters.
        /// </summary>
        /// <returns></returns>
        private async Task<Ellucian.Data.Colleague.DataContracts.IntlParams> InternationalParametersAsync()
        {

            if (_internationalParameters == null)
            {
                _internationalParameters = await GetInternationalParametersAsync();
            }
            return _internationalParameters;
        }
    }
}