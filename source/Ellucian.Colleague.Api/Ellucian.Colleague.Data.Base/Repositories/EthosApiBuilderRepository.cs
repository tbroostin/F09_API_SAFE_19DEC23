/// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class EthosApiBuilderRepository : BaseColleagueRepository, IEthosApiBuilderRepository
    {
        private RepositoryException exception = new RepositoryException();
        const string AllSelectedRecordsCache = "AllSelectedRecordKeys";
        const int AllSelectedRecordsCacheTimeout = 20;
        char _VM = Convert.ToChar(DynamicArray.VM);
        char _SM = Convert.ToChar(DynamicArray.SM);

        public EthosApiBuilderRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using Level 1 Cache Timeout Value for data that changes rarely.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region Get

        /// <summary>
        /// Gets all API Builder entities
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<EthosApiBuilder>, int>> GetEthosApiBuilderAsync(int offset, int limit, EthosApiConfiguration configuration, Dictionary<string, EthosExtensibleDataFilter> filterDictionary, bool bypassCache)
        {
            string instance = string.Empty;
            IEnumerable<string> fileSuiteYears = new List<string>();
            var fileName = !string.IsNullOrEmpty(configuration.SelectFileName) ? configuration.SelectFileName : configuration.PrimaryEntity;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = configuration.PrimaryGuidFileName;
            }

            Dictionary<string, string> dictionaryItems = new Dictionary<string, string>();
            foreach (var dictItem in filterDictionary)
            {
                if (!string.IsNullOrEmpty(dictItem.Key))
                {
                    dictionaryItems.Add(dictItem.Key, dictItem.Value != null && dictItem.Value.FilterValue != null ? dictItem.Value.FilterValue.ToString() : null);
                }
            }
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllSelectedRecordsCache,fileName, dictionaryItems);

            int totalCount = 0;

            string[] limitingKeys = null;
            var criteria = new StringBuilder();
            var selectParagraph = new List<string>();
            selectParagraph.AddRange(configuration.SelectParagraph);
            if (configuration.SelectSubroutineName != null && !string.IsNullOrEmpty(configuration.SelectSubroutineName))
            {
                if (selectParagraph != null && selectParagraph.Any())
                {
                    if (selectParagraph.Last() != "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA")
                    {
                        selectParagraph.Add("IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA");
                    }
                }
                selectParagraph.Add(string.Concat("APISEL ", configuration.SelectSubroutineName));
            }

            // Main Selection Criteria
            criteria = BuildSelectCriteria(configuration.SelectColumnName, "", configuration.SavingField,
                configuration.SavingOption, configuration.SelectionCriteria, configuration.SortColumns);
            if (selectParagraph != null && selectParagraph.Any())
            {
                // If we don't have any criteria but we still have a paragraph to execute
                // then we don't want to select the entire file so we will null out the file name
                // so that the CTX will only execute the paragraph.
                if (criteria.Length == 0)
                {
                    fileName = string.Empty;
                }
                else
                {
                    var paragraph = new List<string>() { "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA" };
                    paragraph.AddRange(selectParagraph);
                    selectParagraph = paragraph;
                }
            }

            // If we have filters but the selection criteria uses SAVING field then
            // We can't execute the main select and must rely on the filter to get
            // limitingKeys for the resulting data.
            if (!string.IsNullOrEmpty(configuration.SavingField) && filterDictionary != null && filterDictionary.Any())
            {
                fileName = string.Empty;
                criteria.Clear();
            }

            // Rules
            if (configuration.SelectRules != null && configuration.SelectRules.Any())
            {
                foreach (var rule in configuration.SelectRules)
                {
                    if (!string.IsNullOrEmpty(rule))
                    {
                        if (selectParagraph != null && selectParagraph.Any())
                        {
                            if (selectParagraph.Last() != "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA")
                            {
                                selectParagraph.Add("IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA");
                            }
                        }
                        selectParagraph.Add(string.Format("EVALUATE.RULES {0}", rule));
                    }
                }
            }

            // File Suites
            if (await IsEthosFileSuiteTemplateFile(fileName, bypassCache))
            {
                List<string> limitKeys = new List<string>();
                var prefix = fileName.Split('.')[0].ToString();
                fileSuiteYears = await GetFileSuiteYearsAsync(prefix, bypassCache);
                foreach (var year in fileSuiteYears)
                {
                    var physicalFileName = await GetEthosFileSuiteFileNameAsync(fileName, year, false);
                    var fileSuiteKeys = await DataReader.SelectAsync(physicalFileName, criteria.ToString());
                    foreach (var key in fileSuiteKeys)
                    {
                        limitKeys.Add(string.Concat(physicalFileName, "+", key));
                    }
                }
                if (limitKeys != null && limitKeys.Any())
                {
                    limitingKeys = limitKeys.ToArray();
                }
                fileName = string.Empty;
                criteria.Clear();
            }

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                fileName,
                offset,
                limit,
                AllSelectedRecordsCacheTimeout,
                async () =>
                {
                    // Filters and named queries
                    if (filterDictionary != null)
                    {
                        foreach (var dictItem in filterDictionary)
                        {
                            var columnName = dictItem.Key;
                            var columnData = dictItem.Value;
                            var filterValues = columnData.FilterValue;
                            if (filterValues != null && filterValues.Any())
                            {
                                var selectFileName = columnData.SelectFileName;
                                foreach (var filterValue in filterValues)
                                {
                                    var filterCriteria = BuildSelectCriteria(columnData.SelectColumnName, filterValue, columnData.SavingField,
                                        columnData.SavingOption, columnData.SelectionCriteria, columnData.SortColumns);

                                    var newLimitingKeys = await DataReader.SelectAsync(selectFileName, limitingKeys, filterCriteria.ToString());
                                    if (newLimitingKeys == null || !newLimitingKeys.Any())
                                    {
                                        return new CacheSupport.KeyCacheRequirements()
                                        {
                                            NoQualifyingRecords = true
                                        };
                                    }
                                    limitingKeys = newLimitingKeys;
                                    var paragraph = BuildParagraphForFilter(filterValue, columnData);
                                    if (paragraph != null && paragraph.Any())
                                    {
                                        if (selectParagraph != null && selectParagraph.Any())
                                        {
                                            if (selectParagraph.Last() != "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA")
                                            {
                                                selectParagraph.Add("IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA");
                                            }
                                        }
                                        selectParagraph.AddRange(paragraph);
                                    }
                                }
                            }
                            else
                            {
                                return new CacheSupport.KeyCacheRequirements()
                                {
                                    NoQualifyingRecords = true
                                };
                            }
                        }
                    }

                    if (selectParagraph != null && selectParagraph.Any())
                    {
                        if (selectParagraph.Last() != "ENDPA: DISPLAY Paragraph Complete.")
                        {
                            selectParagraph.Add("ENDPA: DISPLAY Paragraph Complete.");
                        }
                    }

                    limitingKeys = await GetRecordKeysHookAsync(configuration.PrimaryEntity, limitingKeys, configuration.ResourceName, "", true, bypassCache);

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                        criteria = criteria.ToString(),
                        paragraph = selectParagraph != null && selectParagraph.Any() ? selectParagraph : null
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<EthosApiBuilder>, int>(new List<EthosApiBuilder>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            //Array.Sort(limitingKeys);
            var subList = keyCacheObject.Sublist.ToArray();          

            List<EthosApiBuilder> extendedDatas = new List<EthosApiBuilder>();
            
            var guidCollectionAndKeys = await GetGuidCollectionAsync(subList, configuration, limitingKeys);
            var recordKeys = guidCollectionAndKeys.Item1;
            var guidCollection = guidCollectionAndKeys.Item2;
            foreach (var id in recordKeys)
            {
                string guid = string.Empty;
                if (guidCollection.TryGetValue(id.Contains('+') ? id.Split('+')[1] : id, out guid))
                {
                    EthosApiBuilder extendedData = await BuildEthosApiBuilder(guid, id.Contains('+') ? id.Split('+')[1] : id, configuration.PrimaryEntity);
                    extendedDatas.Add(extendedData);
                }
                else
                {
                    if (guidCollection.TryGetValue(id, out guid))
                    {
                        EthosApiBuilder extendedData = await BuildEthosApiBuilder(guid, id.Contains('+') ? id.Split('+')[1] : id, configuration.PrimaryEntity);
                        extendedDatas.Add(extendedData);
                    }
                    else
                    {
                        exception.AddError(new RepositoryError("GUID.Not.Found", string.Format("Cannot find a guid for {0}.", configuration.PrimaryGuidFileName))
                        {
                            SourceId = id
                        });
                    }
                }
            }

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return extendedDatas.Any() ? new Tuple<IEnumerable<EthosApiBuilder>, int>(extendedDatas, totalCount) : null;
        }

        /// <summary>
        /// Gets API builder record by Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>EthosApiBuilder</returns>
        public async Task<EthosApiBuilder> GetEthosApiBuilderByIdAsync(string id, EthosApiConfiguration configuration)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "Must provide an id to get the person visa.");
            }
            var fileName = !string.IsNullOrEmpty(configuration.PrimaryGuidFileName) ? configuration.PrimaryGuidFileName : configuration.PrimaryEntity;
                    
            var guidEntity = await this.GetRecordInfoFromGuidAsync(id);
            if (guidEntity != null && guidEntity.Entity != fileName)
            {
                // File Suites
                if (await IsEthosFileSuiteTemplateFile(fileName))
                {
                    var prefix = fileName.Split('.')[0].ToString();
                    if (prefix == guidEntity.Entity.Split('.')[0].ToString())
                    {
                        var year = guidEntity.Entity.Split('.')[1];
                        fileName = await GetEthosFileSuiteFileNameAsync(fileName, year, false);
                    }
                }
            }
            if (guidEntity == null || guidEntity.Entity != fileName)           
            {
                throw new KeyNotFoundException("No resource information for id " + id + ".  Key not found.");
            }

            var selectFileName = !string.IsNullOrEmpty(configuration.SelectFileName) ? configuration.SelectFileName : configuration.PrimaryEntity;
            if (string.IsNullOrEmpty(fileName))
            {
                selectFileName = configuration.PrimaryGuidFileName;
            }
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllSelectedRecordsCache, fileName, id);

            string[] limitingKeys = new string[] { guidEntity.PrimaryKey };
            var criteria = new StringBuilder();
            var selectParagraph = new List<string>();
            selectParagraph.AddRange(configuration.SelectParagraph);
            if (configuration.SelectSubroutineName != null && !string.IsNullOrEmpty(configuration.SelectSubroutineName))
            {
                if (selectParagraph != null && selectParagraph.Any())
                {
                    if (selectParagraph.Last() != "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA")
                    {
                        selectParagraph.Add("IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA");
                    }
                }
                selectParagraph.Add(string.Concat("APISEL ", configuration.SelectSubroutineName));
            }

            // Main Selection Criteria
            if (configuration.SelectionCriteria != null && configuration.SelectionCriteria.Any() && string.IsNullOrEmpty(configuration.SavingField))
            {
                criteria = BuildSelectCriteria(configuration.SelectColumnName, "", configuration.SavingField,
                    configuration.SavingOption, configuration.SelectionCriteria, configuration.SortColumns);
                if (selectParagraph != null && selectParagraph.Any())
                {
                    // If we don't have any criteria but we still have a paragraph to execute
                    // then we don't want to select the entire file so we will null out the file name
                    // so that the CTX will only execute the paragraph.
                    if (criteria.Length == 0)
                    {
                        fileName = string.Empty;
                    }
                    else
                    {
                        var paragraph = new List<string>() { "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA" };
                        paragraph.AddRange(selectParagraph);
                        selectParagraph = paragraph;
                    }
                }
            }
            else
            {
                selectFileName = fileName;
            }

            // Rules
            if (configuration.SelectRules != null && configuration.SelectRules.Any())
            {
                foreach (var rule in configuration.SelectRules)
                {
                    if (!string.IsNullOrEmpty(rule))
                    {
                        if (selectParagraph != null && selectParagraph.Any())
                        {
                            if (selectParagraph.Last() != "IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA")
                            {
                                selectParagraph.Add("IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA");
                            }
                        }
                        selectParagraph.Add(string.Format("EVALUATE.RULES {0}", rule));
                    }
                }
            }

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                selectFileName,  0, 1,
                AllSelectedRecordsCacheTimeout,
                async () =>
                {
                    if (selectParagraph != null && selectParagraph.Any())
                    {
                        selectParagraph.Add("ENDPA: DISPLAY Paragraph Complete.");
                    }

                    limitingKeys = await GetRecordKeysHookAsync(configuration.PrimaryEntity, limitingKeys, configuration.ResourceName, "", true, true);

                    return new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                        criteria = criteria.ToString().TrimStart(' ').TrimEnd(' '),
                        paragraph = selectParagraph != null && selectParagraph.Any() ? selectParagraph : null
                    };
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                exception.AddError(new RepositoryError("Validation.Exception", string.Format("The record represented by GUID '{0}' doesn't meet primary selection criteria.", id))
                {
                    SourceId = guidEntity.PrimaryKey,
                    Id = id
                });
                throw exception;
            }

            EthosApiBuilder extendedData = await BuildEthosApiBuilder(id, "", configuration.PrimaryEntity);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return extendedData;
        }
        #endregion

        #region PUT
        /// <summary>
        /// Updates API information
        /// </summary>
        /// <param name="extendedDataRequest">Domain.Base.Entities.EthosApiBuilderRequest</param>
        /// <returns>Domain.Base.Entities.EthosApiBuilderResponse</returns>
        public async Task<Domain.Base.Entities.EthosApiBuilder> UpdateEthosApiBuilderAsync(EthosApiBuilder updateRequest, EthosApiConfiguration configuration)
        {
            //UpdateEthosApiBuilderRequest updateRequest = new UpdateEthosApiBuilderRequest();

            //var extendedDataTuple = GetEthosEthosApiBuilderLists();

            //if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            //{
            //    updateRequest.ExtendedNames = extendedDataTuple.Item1;
            //    updateRequest.ExtendedValues = extendedDataTuple.Item2;
            //}

            //UpdateEthosApiBuilderResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateEthosApiBuilderRequest, UpdateEthosApiBuilderResponse>(updateRequest);

            //if (updateResponse.VisaErrorMessages.Any())
            //{
            //    var errorMessage = string.Empty;
            //    foreach (var message in updateResponse.VisaErrorMessages)
            //    {
            //        errorMessage = string.Format("Error occurred updating person visa '{0} {1}'", extendedDataRequest.Guid, extendedDataRequest.PersonId);
            //        errorMessage += string.Join(Environment.NewLine, message.ErrorMsg);
            //        logger.Error(errorMessage.ToString());
            //    }
            //    throw new InvalidOperationException(errorMessage);
            //}

            EthosApiBuilder updateResponse = new EthosApiBuilder(updateRequest.Guid, updateRequest.Code, updateRequest.Description);

            return updateResponse;

        }
        #endregion

        #region Other methods
        /// <summary>
        /// Builds EthosApiBuilder entity
        /// </summary>
        /// <param name="foreignPersonContract">ForeignPerson</param>
        /// <param name="personContract">Ellucian.Colleague.Data.Base.DataContracts.Person</param>
        /// <returns>EthosApiBuilder</returns>
        private async Task<EthosApiBuilder> BuildEthosApiBuilder(string guid, string recordKey, string entity)
        {
            EthosApiBuilder extendedData = null;
            try
            {
                if (string.IsNullOrEmpty(recordKey) && !string.IsNullOrEmpty(guid))
                {
                    var recordInfo = await GetRecordInfoFromGuidAsync(guid);
                    recordKey = recordInfo.PrimaryKey;
                }
                extendedData = new EthosApiBuilder(guid, recordKey, entity);
            }
            catch
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", string.Format("No guid found for resource entity '{0}' and id '{1}'.", entity, recordKey))
                {
                    SourceId = recordKey,
                    Id = guid
                });
            }
            return extendedData;
        }

        /// <summary>
        /// Get a guid collection from a list of keys
        /// </summary>
        /// <param name="ids">List of keys to get a collection for.</param>
        /// <param name="configuration">Configuration parameters for API</param>
        /// <returns>Dictionary of keys and guids</returns>
        private async Task<Tuple<List<string>, Dictionary<string, string>>> GetGuidCollectionAsync(IEnumerable<string> ids, EthosApiConfiguration configuration, string[] limitingKeys)
        {

            List<string> recordKeys = ids.ToList();
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Tuple<List<string>, Dictionary<string, string>>(recordKeys, new Dictionary<string, string>());
            }
            var guidCollection = new Dictionary<string, string>();
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
            string primaryFileName = !string.IsNullOrEmpty(configuration.PrimaryGuidFileName) ? configuration.PrimaryGuidFileName : configuration.PrimaryEntity;
            
            if (configuration.PrimaryGuidDbType == "K" || string.IsNullOrEmpty(configuration.PrimaryGuidDbType))
            {
                if (await IsEthosFileSuiteTemplateFile(primaryFileName, false))
                {
                    var guidLookup = ids
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct().ToList()
                        .ConvertAll(p => new RecordKeyLookup(p.Split('+')[0].ToString(), p.Split('+')[1].ToString(), false)).ToArray();
                    recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
                }
                else
                {
                    var guidLookup = ids
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct().ToList()
                        .ConvertAll(p => new RecordKeyLookup(primaryFileName, p, false)).ToArray();
                    recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
                }
            }
            else
            {
                if (await IsEthosFileSuiteTemplateFile(primaryFileName, false))
                {
                    var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(p.Split('+')[0].ToString(), p.Split('+')[1].ToString(), configuration.PrimaryGuidSource, p.Split('+')[1].ToString(), false)).ToArray();
                    recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
                }
                else
                {
                    if (configuration.PrimaryEntity.EndsWith("VALCODES"))
                    {
                        var guidLookup = ids
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct().ToList()
                        .ConvertAll(p => new RecordKeyLookup(primaryFileName, !string.IsNullOrWhiteSpace(configuration.PrimaryTableName) ? configuration.PrimaryTableName : configuration.SelectionCriteria.FirstOrDefault().SelectValue.Trim('"'), configuration.PrimaryGuidSource, p, false)).ToArray();
                        recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
                    }
                    else
                    {
                        var guidLookup = ids
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct().ToList()
                            .ConvertAll(p => new RecordKeyLookup(primaryFileName, p, configuration.PrimaryGuidSource, p, false)).ToArray();
                        recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                        // If we don't find any GUIDs with the same primary key as the secondary key, then
                        // try selecting from limiting keys.
                        var selectedGuids = recordKeyLookupResults.Where(s => s.Value != null && !string.IsNullOrEmpty(s.Value.Guid));
                        if (selectedGuids == null || !selectedGuids.Any())
                        {
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                var criteria = BuildSelectCriteria(configuration.SelectColumnName, "", "", "",
                                    configuration.SelectionCriteria, configuration.SortColumns);

                                limitingKeys = await DataReader.SelectAsync(primaryFileName, criteria.ToString());
                            }

                            recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                            foreach (var primaryKey in limitingKeys)
                            {
                                guidLookup = ids
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Distinct().ToList()
                                   .ConvertAll(p => new RecordKeyLookup(primaryFileName, primaryKey, configuration.PrimaryGuidSource, p, false)).ToArray();
                                var recordKeyLookup = await DataReader.SelectAsync(guidLookup);
                                foreach (var recordLookup in recordKeyLookup)
                                {
                                    if (!recordKeyLookupResults.ContainsKey(recordLookup.Key) && recordLookup.Value != null && !string.IsNullOrEmpty(recordLookup.Value.Guid))
                                    {
                                        recordKeyLookupResults.Add(recordLookup.Key, recordLookup.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    int index = splitKeys.Count() - 1;
                    if (!guidCollection.ContainsKey(splitKeys[index]))
                    {
                        string guid = splitKeys[1];
                        if (recordKeyLookupResult.Value != null && !string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid))
                        {
                            guid = recordKeyLookupResult.Value.Guid;
                            // If we ever want to support an API without GUIDs
                            // Move this to outside of this if statement.
                            guidCollection.Add(splitKeys[index], guid);
                        }
                        // guidCollection.Add(splitKeys[index], guid);
                    }
                }
                catch (Exception) // Do not throw error.
                {
                }
            }

            return new Tuple<List<string>, Dictionary<string, string>>(recordKeys, guidCollection);

        }

        /// <summary>
        /// Validate the record and secondary key exist before including the GUID in the output.
        /// </summary>
        /// <param name="fileName">File Name where GUID is attached</param>
        /// <param name="fieldName">Secondary Field attached to the GUID</param>
        /// <param name="primaryKey">The Primary Key from the GUID entity</param>
        /// <param name="secondaryKey">The Secondary Key from the GUID entity</param>
        /// <returns>Boolean to say if the entry is valid or not.</returns>
        private async Task<bool> ValidateGuidReference(string fileName, string fieldName, string primaryKey, string secondaryKey)
        {
            bool validEntityData = true;
            var entityData = await DataReader.ReadRecordColumnsAsync(fileName, primaryKey, new string[] { fieldName });
            if (entityData == null || !entityData.ContainsKey(fieldName))
            {
                return false;
            }
            var secondaryData = entityData[fieldName].Split(_VM);
            if (!secondaryData.Contains(secondaryKey))
            {
                return false;
            }

            return validEntityData;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="entityName"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public async Task<string> GetRecordIdFromTranslationAsync(string sourceData, string entityName, string sourceColumn = "", string tableName = "")
        {
            string recordKey = sourceData;
            if (!string.IsNullOrEmpty(tableName))
            {
                var valcodeTable = await DataReader.ReadRecordAsync<ApplValcodes>(entityName, tableName);
                switch (sourceColumn)
                {
                    case ("VAL.INTERNAL.CODE"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(es => es.ValInternalCodeAssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.EXTERNAL.REPRESENTATION"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValExternalRepresentationAssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.1"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode1AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.2"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode2AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.3"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode3AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                    case ("VAL.ACTION.CODE.4"):
                        {
                            var assoc = valcodeTable.ValsEntityAssociation.FirstOrDefault(ex => ex.ValActionCode4AssocMember == sourceData);
                            if (assoc != null)
                                recordKey = assoc.ValInternalCodeAssocMember;
                            break;
                        }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(entityName))
                {
                    var recordKeys = await DataReader.SelectAsync(entityName, string.Format("WITH {0} EQ '{1}'", sourceColumn, sourceData));
                    if (recordKeys == null || !recordKeys.Any())
                    {
                        // Try translation on the record key.
                        recordKeys = await DataReader.SelectAsync(entityName, string.Format("WITH @ID EQ '{0}' SAVING {1}", sourceData, sourceColumn));
                    }
                    if (recordKeys != null && recordKeys.Any())
                    {
                        recordKey = recordKeys[0];
                    }
                }
            }
            return recordKey;
        }

        /// <summary>
        /// Checks to see if a file name is a template file for file suites.
        /// </summary>
        /// <param name="fileName">Name of the file to check to see if it's a file suite template.</param>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>True if File Suite Template file</returns>
        private async Task<bool> IsEthosFileSuiteTemplateFile(string fileName, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    if (templateFile.FstTemplateAssocMember == fileName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get the file name for a template file and specific year/instance.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="instance"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Physical file name for a template file and instance.</returns>
        private async Task<string> GetEthosFileSuiteFileNameAsync(string fileName, string instance, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    if (templateFile.FstTemplateAssocMember == fileName)
                    {
                        var prefix = templateFile.FstFilePrefixAssocMember;
                        var suffix = templateFile.FstFileSuffixAssocMember;
                        var physicalFileName = string.Concat(prefix, ".", instance);
                        if (!string.IsNullOrEmpty(suffix))
                        {
                            physicalFileName = string.Concat(physicalFileName, ".", suffix);
                        }
                        return physicalFileName;
                    }
                }
            }
            return string.Empty;
        }

        private IEnumerable<FileSuiteTemplates> _fileSuiteTemplates = null;
        /// <summary>
        /// Returns a list of all File Suite Templates
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of data contracts for Templates</returns>
        private async Task<IEnumerable<FileSuiteTemplates>> GetEthosFileSuiteTemplatesAsync(bool bypassCache = false)
        {
            if (_fileSuiteTemplates == null)
            {
                const string ethosFileSuiteCacheKey = "AllEthosApiBuilderFileSuites";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosFileSuiteCacheKey)))
                {
                    ClearCache(new List<string> { ethosFileSuiteCacheKey });
                }
                var fileSuiteTemplates = await GetOrAddToCacheAsync<List<FileSuiteTemplates>>(ethosFileSuiteCacheKey,
                    async () =>
                    {
                        var ethosFaFileSuitesList =
                            await DataReader.ReadRecordAsync<FileSuiteTemplates>("ST.PARMS", "FST_FA.FILE.SUITES");
                    // Hack in the FA.YEARLY.USER.ACYR and FA.TERM.USER.ACYR file suites
                    if (ethosFaFileSuitesList.FstTemplate.Any() && ethosFaFileSuitesList.FstFilePrefix.Any())
                        {
                            ethosFaFileSuitesList.FstTemplate.Add("FA.YEARLY.USER.ACYR");
                            ethosFaFileSuitesList.FstTemplate.Add("FA.TERM.USER.ACYR");
                            ethosFaFileSuitesList.FstTemplate.Add("FA.YEARLY.USER");
                            ethosFaFileSuitesList.FstTemplate.Add("FA.TERM.USER");
                            ethosFaFileSuitesList.FstTemplate.Add("");
                            ethosFaFileSuitesList.FstTemplate.Add("");
                        }

                        var fsKeys = new List<string>()
                        {
                        "FST_BCT",
                        "FST_BJT",
                        "FST_BOC",
                        "FST_BWK",
                        "FST_BPJ",
                        "FST_GL.SUITES.WITH.GLU"
                        };
                        var ethosCfFileSuitesList =
                            await DataReader.BulkReadRecordAsync<FileSuiteTemplates>("CF.PARMS", fsKeys.ToArray());

                        var fileSuitesList = new List<FileSuiteTemplates>();
                        fileSuitesList.Add(ethosFaFileSuitesList);
                        fileSuitesList.AddRange(ethosCfFileSuitesList);
                        return fileSuitesList;
                    }, CacheTimeout);

                _fileSuiteTemplates = fileSuiteTemplates;
            }
            return _fileSuiteTemplates;
        }

        /// <summary>
        /// Return a valid list of File Suite Years based on Instance
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<IEnumerable<string>> GetFileSuiteYearsAsync(string instance, bool bypassCache = false)
        {
            switch (instance)
            {
                case "GLA":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                case "GLS":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                case "GAA":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                case "GLU":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                case "ENC":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                case "GLP":
                    return await GetGeneralLedgerYearsAsync(bypassCache);
                default:
                    return await GetFinancialAidYearsAsync(bypassCache);
            }
        }

        private IEnumerable<string> _financialAidYears = null;
        /// <summary>
        /// Get a collection of financial aid years.
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        private async Task<IEnumerable<string>> GetFinancialAidYearsAsync(bool bypassCache = false)
        {
            if (_financialAidYears == null)
            {
                const string ethosFinancialAidYearsCacheKey = "AllEthosApiBuilderFinancialAidYears";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosFinancialAidYearsCacheKey)))
                {
                    ClearCache(new List<string> { ethosFinancialAidYearsCacheKey });
                }
                var financialAidYears = await GetOrAddToCacheAsync<List<string>>(ethosFinancialAidYearsCacheKey,
                    async () =>
                    {
                        var faSuiteYears = new List<string>();
                        var suiteYears = await DataReader.BulkReadRecordAsync<FaSuitesBase>("FA.SUITES", "");
                        foreach (var year in suiteYears)
                        {
                            if (year.FaSuitesStatus.Equals("C", StringComparison.OrdinalIgnoreCase))
                            {
                                faSuiteYears.Add(year.Recordkey);
                            }
                        }
                        return faSuiteYears;
                    }, CacheTimeout);

                _financialAidYears = financialAidYears;
            }
            return _financialAidYears;
        }

        private IEnumerable<string> _genLedgerYears = null;
        /// <summary>
        /// Get a collection of financial aid years.
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of financial aid years</returns>
        private async Task<IEnumerable<string>> GetGeneralLedgerYearsAsync(bool bypassCache = false)
        {
            if (_genLedgerYears == null)
            {
                const string ethosFinancialAidYearsCacheKey = "AllEthosApiBuilderGeneralLedgerYears";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosFinancialAidYearsCacheKey)))
                {
                    ClearCache(new List<string> { ethosFinancialAidYearsCacheKey });
                }
                var genLedgerYears = await GetOrAddToCacheAsync<List<string>>(ethosFinancialAidYearsCacheKey,
                    async () =>
                    {
                        var suiteYears = await DataReader.SelectAsync("GEN.LDGR", "WITH GEN.LDGR.TYPE EQ 'LEDGER' AND WITH GEN.LDGR.STATUS NE 'X'");
                        return suiteYears.ToList();
                    }, CacheTimeout);

                _genLedgerYears = genLedgerYears;
            }
            return _genLedgerYears;
        }

        private IEnumerable<EdmCodeHooks> _ethosExtensibleCodeHooks = null;
        /// <summary>
        /// Gets all of the Ethos Extensiblity code hook settings stored on EDM.CODE.HOOKS
        /// </summary>
        /// <param name="bypassCache">bool to determine if cache should be bypassed</param>
        /// <returns>List of DataContracts.EdmCodeHooks</returns>
        public async Task<IEnumerable<EdmCodeHooks>> GetEthosExtensibilityCodeHooks(bool bypassCache = false)
        {
            if (_ethosExtensibleCodeHooks == null)
            {
                const string ethosExtensiblityCacheKey = "AllEthosExtensibiltyCodeHooks";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosExtensiblityCacheKey)))
                {
                    ClearCache(new List<string> { ethosExtensiblityCacheKey });
                }

                var ethosExtensiblityCodeHooks = new List<EdmCodeHooks>();
                try
                {
                    ethosExtensiblityCodeHooks =
                        await GetOrAddToCacheAsync<List<EdmCodeHooks>>(ethosExtensiblityCacheKey,
                            async () => (await DataReader.BulkReadRecordAsync<EdmCodeHooks>("EDM.CODE.HOOKS", "")).ToList(), CacheTimeout);
                }
                catch (Exception)
                {
                    // if the EDM.CODE.HOOKS file is missing, do not through an exception.
                }

                _ethosExtensibleCodeHooks = ethosExtensiblityCodeHooks;
            }
            return _ethosExtensibleCodeHooks;
        }

        private List<string> BuildParagraphForFilter(string value, EthosExtensibleDataFilter configuration)
        {
            var returnParagraph = new List<string>();
            
            if (configuration.SelectParagraph != null && configuration.SelectParagraph.Any())
            {
                var paragraph = new List<string>();
                foreach (var para in configuration.SelectParagraph)
                {
                    var newParaLine = para.Replace(configuration.SelectColumnName, value);
                    paragraph.Add(newParaLine);
                }
                returnParagraph = paragraph;
            }
            if (!string.IsNullOrEmpty(configuration.SelectSubroutineName))
            {
                if (returnParagraph != null && returnParagraph.Any())
                {
                    if (returnParagraph.Last() != "IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA")
                    {
                        returnParagraph.Add("IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA");
                    }
                }
                returnParagraph.Add(string.Concat("APISEL ", configuration.SelectSubroutineName, " ", value));
            }
            if (configuration.SelectRules != null)
            {
                foreach (var rule in configuration.SelectRules)
                {
                    if (!string.IsNullOrEmpty(rule))
                    {
                        if (returnParagraph != null && returnParagraph.Any())
                        {
                            if (returnParagraph.Last() != "IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA")
                            {
                                returnParagraph.Add("IF @SYSTEM.RETURN.CODE <= 0 THEN GO ENDPA");
                            }
                        }
                        returnParagraph.Add(string.Format("EVALUATE.RULES {0}", rule));
                    }
                }
            }

            return returnParagraph;
        }

        private StringBuilder BuildSelectCriteria(string columnName, string value, string savingColumn, string savingOption, List<EthosApiSelectCriteria> selectionCriteria, List<EthosApiSortCriteria> sortCriteria)
        {
            var criteria = new StringBuilder();
            if (selectionCriteria != null && selectionCriteria.Any())
            {
                foreach (var select in selectionCriteria)
                {
                    switch (select.SelectConnector.ToUpperInvariant())
                    {
                        case ("EVERY"):
                            criteria.Append("AND EVERY");
                            break;
                        case ("OREVERY"):
                            criteria.Append("OR EVERY");
                            break;
                        case ("ORWITH"):
                            criteria.Append("OR WITH");
                            break;
                        case ("OWE"):
                            criteria.Append("OR WITH EVERY");
                            break;
                        case ("WE"):
                            criteria.Append("WITH EVERY");
                            break;
                        default:
                            criteria.Append(select.SelectConnector);
                            break;
                    }
                    criteria.Append(" ");
                    criteria.Append(select.SelectColumn);
                    criteria.Append(" ");
                    criteria.Append(select.SelectOper);
                    criteria.Append(" ");
                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(value))
                    {
                        criteria.Append(select.SelectValue.Replace(columnName, value));
                    }
                    else
                    {
                        criteria.Append(select.SelectValue);
                    }
                    criteria.Append(" ");
                }
            }

            // Saving Options
            if (!string.IsNullOrEmpty(savingColumn))
            {
                criteria.Append("SAVING ");
                // Can't have UNIQUE keyword in a select if we have Sort Criteria
                if (!string.IsNullOrEmpty(savingOption) && (sortCriteria == null || !sortCriteria.Any()))
                {
                    criteria.Append("UNIQUE ");
                }
                criteria.Append(savingColumn);
                criteria.Append(" ");
            }

            // Sorting Options
            if (sortCriteria != null && sortCriteria.Any())
            {
                foreach (var sortData in sortCriteria)
                {
                    if (!string.IsNullOrEmpty(sortData.SortSequence) && !string.IsNullOrEmpty(sortData.SortColumn))
                    {
                        criteria.Append(string.Format("{0} {1} ", sortData.SortSequence, sortData.SortColumn));
                    }
                }
            }

            return criteria;
        }

        /// <summary>
        /// Returns Output Data from Code Hooks Execution.
        /// </summary>
        /// <param name="selectFileName"></param>
        /// <param name="inputData"></param>
        /// <param name="resourceName"></param>
        /// <param name="resourceVersionNumber"></param>
        /// <param name="reportEthosApiErrors"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<string[]> GetRecordKeysHookAsync(string selectFileName, string[] limitingKeys, string resourceName, string resourceVersionNumber, bool reportEthosApiErrors = false, bool bypassCache = false)
        {
            var exception = new RepositoryException("Extensibility configuration errors.");

            var allCodeHooks = await GetEthosExtensibilityCodeHooks(bypassCache);
            if (allCodeHooks == null || !allCodeHooks.Any())
            {
                return limitingKeys;
            }
            var selectCodeHooks = allCodeHooks.Where(ch => (ch.EdmcResourceName.Contains(resourceName.ToUpper())
                || (ch.EdmcResourceName == null || !ch.EdmcResourceName.Any()))
                && ch.EdmcType.Equals("select", StringComparison.OrdinalIgnoreCase)
                && ch.EdmcFileName.Contains(selectFileName.ToUpper())
                && (ch.EdmcFieldName == null || !ch.EdmcFieldName.Any()));

            foreach (var selectHook in selectCodeHooks)
            {
                bool matchingResourceVersion = false;
                foreach (var resource in selectHook.EdmcResourceEntityAssociation)
                {
                    if (!string.IsNullOrEmpty(resource.EdmcResourceVersionAssocMember))
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase) && resource.EdmcResourceVersionAssocMember.Equals(resourceVersionNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                    else
                    {
                        if (resource.EdmcResourceNameAssocMember.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingResourceVersion = true;
                        }
                    }
                }
                if (matchingResourceVersion)
                {
                    CodeBuilderObject inputObject = new CodeBuilderObject()
                    {
                        LimitingKeys = limitingKeys,
                        SourceCode = selectHook.EdmcHookCode
                    };

                    var cacheName = CodeBuilderSupport.BuildCacheKey("SelectionHooks", selectHook.Recordkey);
                    var result = await CodeBuilderSupport.CodeBuilderAsync(this, ContainsKey, GetOrAddToCache,
                        transactionInvoker, DataReader, cacheName, bypassCache, CacheTimeout, inputObject);

                    if (result != null && result.ErrorFlag)
                    {
                        var message = string.Format("Errors executing Select Hooks on '{2}' for resource '{0}' version number '{1}'", resourceName, resourceVersionNumber, selectFileName);
                        logger.Error(message);
                        exception.AddError(new RepositoryError("Data.Access", message));

                        foreach (var error in result.ErrorMessages)
                        {
                            message = error;
                            logger.Error(message);
                            if (reportEthosApiErrors)
                            {
                                exception.AddError(new RepositoryError("Data.Access", message));
                            }
                        }
                        if (reportEthosApiErrors && exception != null && exception.Errors != null && exception.Errors.Any())
                        {
                            throw exception;
                        }
                    }

                    return result.LimitingKeys;
                }
            }

            return limitingKeys;
        }
    }
}