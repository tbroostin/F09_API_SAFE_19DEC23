// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Data.Colleague.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
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

        private readonly string budgetPhasesCriteria = "WITH LDM.GUID.SECONDARY.FLD EQ '' AND LDM.GUID.SECONDARY.KEY EQ '' AND LDM.GUID.ENTITY EQ 'BUDGET' AND LDM.GUID.PRIMARY.KEY EQ '{0}'";
        private readonly string budgetCodesCriteria = "WITH LDM.GUID.SECONDARY.FLD EQ 'BU.BUDGET.CODES.INTG.IDX' AND LDM.GUID.ENTITY EQ 'BUDGET' AND LDM.GUID.SECONDARY.KEY EQ '{0}'";
        private Ellucian.Data.Colleague.DataContracts.IntlParams _internationalParameters;
        private string _hostCountry;

        public async Task<Tuple<IEnumerable<BudgetWork>, int>> GetBudgetPhaseLineItemsAsync(int offset,
            int limit, string budgetPhaseId, string accountingStringComponentValue,  bool bypassCache = false)
        {
            var budgetWorkCollection = new List<BudgetWork>();
            int totalRecords = 0;
            var criteria = "WITH BU.LOCATION EQ 'O'";
            var accountingStringCriteria = string.Empty;

            if (!string.IsNullOrEmpty(budgetPhaseId))
            {
                criteria = string.Concat(criteria, " AND WITH BUDGET.ID EQ '" , budgetPhaseId, "'");
            }
            if (!string.IsNullOrEmpty(accountingStringComponentValue))
            {
                accountingStringCriteria = string.Format("WITH BW.EXPENSE.ACCT EQ '{0}'", accountingStringComponentValue);
            }

            var budgetIds = await DataReader.SelectAsync("BUDGET", criteria);
            if ((budgetIds == null || !budgetIds.Any()))
            {
                return new Tuple<IEnumerable<BudgetWork>, int>(null, 0);
            }
            try
            {
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
                // so we need a new collection that grpoups the file name, and contains a sublist
                // or records associated with that filename
                      
                var groups = subListIds.Select(x => x.Split('*'))
                   .GroupBy(x => x[0])
                   .ToDictionary(x => x.Key, x => x.Select(g => g[1]).Distinct().ToList());

                var budgetWorkRecordKeyValue = new Dictionary<string, Collection<DataContracts.BudWork>>();

                foreach (var group in groups)
                {
                    budgetWorkRecordKeyValue.Add(group.Key,
                        await DataReader.BulkReadRecordAsync<DataContracts.BudWork>(group.Key,
                        group.Value.ToArray()));
                }
                foreach (var budgetWorkRecords in budgetWorkRecordKeyValue)
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
                throw new Exception("Error occurred while getting budget work records.", ex);
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
                var budgetPhasesDict = await GetBudgetGuidDictionary(budgetIds.ToArray(), budgetPhasesCriteria);

                var budgetCodesDict = await GetBudgetGuidDictionary(budgetIds.ToArray(), budgetCodesCriteria);

                var budgetRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Budget>(budgetIds, bypassCache);
                foreach (var budgetRecord in budgetRecords)
                {
                    budgets.Add(GetBudgetEntity(budgetRecord, budgetPhasesDict, budgetCodesDict));

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while getting budget records.", ex);
            }

            return new Tuple<IEnumerable<Budget>, int>(budgets, totalRecords);
        }

        public async Task<Tuple<IEnumerable<Budget>, int>> GetBudgetCodesAsync(bool bypassCache = false)
        {
            List<Budget> budgets = new List<Budget>();

            var criteria = "WITH BU.LOCATION EQ 'O' AND WITH BU.BUDGET.CODES.INTG.IDX NE ''";

            var budgetIds = await DataReader.SelectAsync("BUDGET", criteria);
            if ((budgetIds == null || !budgetIds.Any()))
            {
                return new Tuple<IEnumerable<Budget>, int>(null, 0);
            }

            int totalRecords = budgetIds.Count();
            try
            {

                var budgetCodesDict = await GetBudgetGuidDictionary(budgetIds.ToArray(), budgetCodesCriteria);

                var budgetRecords = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Budget>(budgetIds, bypassCache);
                foreach (var budgetRecord in budgetRecords)
                {
                    Budget budgetEntity = GetBudgetEntity(budgetRecord, null, budgetCodesDict);
                    budgets.Add(budgetEntity);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while getting budget records.", ex);
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
                throw new KeyNotFoundException(string.Format("No budget phase was found for guid {0}.", guid));
            }
            var budgetRecord = await DataReader.ReadRecordAsync<DataContracts.Budget>("BUDGET", budgetId);
            if (budgetRecord == null)
            {
                throw new KeyNotFoundException(string.Format("No budget phase was found for guid {0}.", guid));
            }
            if (budgetRecord.BuLocation != "O")
            {
                throw new ArgumentException("This budget is not available as it is not marked as being online.");
            }
            var budgetPhasesDict = await GetBudgetGuidDictionary(new List<string>() { budgetId }, budgetPhasesCriteria);

            var budgetCodesDict = await GetBudgetGuidDictionary(new List<string>() { budgetId }, budgetCodesCriteria);

            return GetBudgetEntity(budgetRecord, budgetPhasesDict, budgetCodesDict);
        }

        public async Task<Budget> GetBudgetCodesAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("Guid is required.");
            }
            var budgetId = await GetBudgetCodesIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(budgetId))
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
            var budgetCodesDict = await GetBudgetGuidDictionary(new List<string>() { budgetId }, budgetCodesCriteria);

            return GetBudgetEntity(budgetRecord, null, budgetCodesDict);
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
            var recordInfo = await GetRecordInfoFromGuidAsync(guid);
            if (recordInfo == null)
            {
                throw new KeyNotFoundException("BudgetPhaseLineItem not found for GUID " + guid);
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
                var guidRecord = await DataReader.SelectAsync("LDM.GUID", string.Format(budgetPhasesCriteria, id));
                if (guidRecord == null || guidRecord.Count() > 1)
                {
                    throw new KeyNotFoundException(string.Format("No budget phase was found for id {0}.", id));
                }
                return guidRecord.FirstOrDefault();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Guid NotFound", "GUID not found for budget phase " + id));
                throw ex;
            }
        }
        
        public async Task<string> GetBudgetCodesGuidFromIdAsync(string id)
        {
            try
            {
                var guidRecord = await DataReader.SelectAsync("LDM.GUID", string.Format(budgetCodesCriteria, id));
                if (guidRecord == null || guidRecord.Count() > 1)
                {
                    throw new KeyNotFoundException(string.Format("No budget code was found for id {0}.", id));
                }
                return guidRecord.FirstOrDefault();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("Guid NotFound", "GUID not found for budget code " + id));
                throw ex;
            }
        }
        
        public async Task<string> GetBudgetPhasesIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("Budgets GUID " + guid + " not found.");
            }
            if ((guidRecord.LdmGuidEntity != "BUDGET") || (!string.IsNullOrEmpty(guidRecord.LdmGuidSecondaryFld)))
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, BUDGET");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        public async Task<string> GetBudgetCodesIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var guidRecord = await DataReader.ReadRecordAsync<LdmGuid>("LDM.GUID", guid);
            if (guidRecord == null)
            {
                throw new KeyNotFoundException("Budgets GUID " + guid + " not found.");
            }
            if ((guidRecord.LdmGuidEntity != "BUDGET") || (guidRecord.LdmGuidSecondaryFld != "BU.BUDGET.CODES.INTG.IDX"))
            {
                throw new KeyNotFoundException("GUID " + guid + " has different entity, than expected, BUDGET");
            }
            return guidRecord.LdmGuidPrimaryKey;
        }

        private Budget GetBudgetEntity(Ellucian.Colleague.Data.ColleagueFinance.DataContracts.Budget budget,
          Dictionary<string, string> budgetPhasesDict, Dictionary<string, string> budgetCodesDict)
        {
            var budgetEntity = new Budget()
            {
                RecordKey = budget.Recordkey,
            };

            if (budgetPhasesDict != null && budgetPhasesDict.Any())
            {
                budgetEntity.BudgetPhaseGuid = GetGuid(budget.Recordkey, budgetPhasesDict);
            }
            if (budgetCodesDict != null && budgetCodesDict.Any())
            {
                budgetEntity.BudgetCodeGuid = GetGuid(budget.Recordkey, budgetCodesDict);
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

        /// <summary>
        /// Gets dictionary with colleague id and guid key pair for BUDGET.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetBudgetGuidDictionary(IEnumerable<string> ids, string criteria)
        {
            if (ids == null || !Enumerable.Any<string>(ids))
            {
                throw new ArgumentNullException("Budget id's are required.");
            }

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var id in ids)
            {
                var guidRecords = await DataReader.SelectAsync("LDM.GUID", string.Format(criteria, id));
                if (!dict.ContainsKey(id))
                {
                    if (guidRecords != null && guidRecords.Any())
                    {
                        dict.Add(id, guidRecords[0]);
                    }
                }
            }
            return dict;
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