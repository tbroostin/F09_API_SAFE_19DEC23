﻿/// Copyright 2020 Ellucian Company L.P. and its affiliates.
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
            string secondaryKeyName = !string.IsNullOrEmpty(configuration.SecondaryKeyName) ? configuration.SecondaryKeyName : string.Empty;
            if (!string.IsNullOrEmpty(configuration.PrimaryGuidSource) && !configuration.PrimaryGuidDbType.Equals("K", StringComparison.OrdinalIgnoreCase))
            {
                secondaryKeyName = configuration.PrimaryGuidSource;
            }

            IEnumerable<string> fileSuiteYears = new List<string>();
            var fileName = !string.IsNullOrEmpty(configuration.SelectFileName) ? configuration.SelectFileName : configuration.PrimaryEntity;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = configuration.PrimaryGuidFileName;
            }
            if (fileName.EndsWith("VALCODES"))
            {
                fileName = configuration.PrimaryEntity;
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
            criteria = BuildSelectCriteria(configuration.SelectColumnName, "", "", configuration.SavingField,
                configuration.SavingOption, configuration.SelectionCriteria, configuration.SortColumns);
            if (selectParagraph != null && selectParagraph.Any())
            {
                // If we don't have any criteria but we still have a paragraph to execute
                // then we don't want to select the entire file so we will null out the file name
                // so that the CTX will only execute the paragraph.  Once the paragraph is
                // executed, then make sure that the keys exist on the primary selection table.
                if (criteria.Length == 0)
                {
                    selectParagraph.Add(string.Format("MIOSEL {0} REQUIRE.SELECT", fileName));
                    fileName = string.Empty;
                }
                else
                {
                    var paragraph = new List<string>() { "IF @SYSTEM.RETURN.CODE LE 0 THEN GO ENDPA" };
                    paragraph.AddRange(selectParagraph);
                    selectParagraph = paragraph;
                }
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
                var prefix = await GetFileSuitePrefix(fileName, bypassCache);
                fileSuiteYears = await GetFileSuiteYearsAsync(prefix, bypassCache);
                foreach (var year in fileSuiteYears)
                {
                    try
                    {
                        var physicalFileName = await GetEthosFileSuiteFileNameAsync(fileName, year, false);
                        // Primary and Secondary keys
                        if (!string.IsNullOrEmpty(secondaryKeyName))
                        {
                            string primarySelect = string.Empty;
                            string[] primaryKeys = null;
                            primarySelect = string.Concat("WITH ", secondaryKeyName, " NE '' BY.EXP ", secondaryKeyName);
                            var primaryKeyList = await DataReader.SelectAsync(physicalFileName, primaryKeys, primarySelect);
                            primarySelect = string.Concat(primarySelect, " SAVING " + secondaryKeyName);
                            var secondaryKeyList = await DataReader.SelectAsync(physicalFileName, primaryKeys, primarySelect);
                            for (int i = 0; i < primaryKeyList.Count(); i++)
                            {
                                if (!string.IsNullOrEmpty(primaryKeyList[i]) && !string.IsNullOrEmpty(secondaryKeyList[i]))
                                {
                                    // Unidata will return a list of keys, Value Mark, and Position whereby SQL only returns the keys
                                    limitKeys.Add(physicalFileName + "+" + primaryKeyList[i].Split(_VM)[0] + "+" + secondaryKeyList[i]);
                                }
                            }
                        }
                        else
                        {
                            var fileSuiteKeys = await DataReader.SelectAsync(physicalFileName, criteria.ToString());
                            foreach (var key in fileSuiteKeys)
                            {
                                limitKeys.Add(string.Concat(physicalFileName, "+", key));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Just skip this file selection if it fails.
                    }
                }
                if (limitKeys != null && limitKeys.Any())
                {
                    limitingKeys = limitKeys.ToArray();
                }
                fileName = string.Empty;
                criteria.Clear();
            }
            else
            {
                // Primary and Secondary keys or VALCODES tables
                if (!string.IsNullOrEmpty(secondaryKeyName) || (!string.IsNullOrEmpty(configuration.PrimaryTableName) && !string.IsNullOrEmpty(configuration.PrimaryApplication)))
                {
                    List<string> limitKeys = new List<string>();
                    string primarySelect = string.Empty;
                    if (!string.IsNullOrEmpty(configuration.PrimaryTableName) && !string.IsNullOrEmpty(configuration.PrimaryApplication))
                    {
                        primarySelect = string.Format("WITH VALCODE.ID EQ '{0}' ", configuration.PrimaryTableName);
                        fileName = string.Concat(configuration.PrimaryApplication, ".VALCODES");
                        secondaryKeyName = "VAL.INTERNAL.CODE";
                    }
                    string[] primaryKeys = null;
                    primarySelect = string.Concat(primarySelect, "WITH ", secondaryKeyName, " NE '' BY.EXP ", secondaryKeyName);
                    var primaryKeyList = await DataReader.SelectAsync(fileName, primaryKeys, primarySelect);
                    primarySelect = string.Concat(primarySelect, " SAVING " + secondaryKeyName);
                    var secondaryKeyList = await DataReader.SelectAsync(fileName, primaryKeys, primarySelect);
                    for (int i = 0; i < primaryKeyList.Count(); i++)
                    {
                        if (!string.IsNullOrEmpty(primaryKeyList[i]) && !string.IsNullOrEmpty(secondaryKeyList[i]))
                        {
                            // Unidata will return a list of keys, Value Mark, and Position whereby SQL only returns the keys
                            limitKeys.Add(fileName + "+" + primaryKeyList[i].Split(_VM)[0] + "+" + secondaryKeyList[i]);
                        }
                    }
                    if (limitKeys != null && limitKeys.Any())
                    {
                        limitingKeys = limitKeys.ToArray();
                    }
                    fileName = string.Empty;
                    criteria.Clear();
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
                            var filterOper = columnData.FilterOper;

                            //attempt to populate a default selection criteria
                            if (columnData.SelectionCriteria == null || !columnData.SelectionCriteria.Any())
                            {
                                var listEthosApiSelectCriteria = new List<EthosApiSelectCriteria>();
                                listEthosApiSelectCriteria.Add(
                                    new EthosApiSelectCriteria("WITH", columnData.ColleagueColumnName, "EQ", columnData.JsonTitle)
                                    );
                                columnData.SelectionCriteria = listEthosApiSelectCriteria;
                            }

                            if (filterValues != null && filterValues.Any())
                            {
                                var selectFileName = columnData.SelectFileName;

                                foreach (var filterValue in filterValues)
                                {
                                    var filterCriteria = BuildSelectCriteria(columnData.SelectColumnName, filterValue, filterOper, columnData.SavingField,
                                        columnData.SavingOption, columnData.SelectionCriteria, columnData.SortColumns);

                                    // File Suites Processing
                                    if (await IsEthosFileSuiteTemplateFile(selectFileName, bypassCache))
                                    {
                                        var limitKeys = new List<string>();
                                        var newKeys = limitingKeys.GroupBy(lk => lk.Split('+')[0]).ToDictionary(lk => lk.Key);
                                        foreach (var group in newKeys)
                                        {
                                            var physicalFileName = group.Key;
                                            var newLimitingKeys = group.Value.Select(gv => gv.Split('+')[1]).ToArray();
                                            if (!string.IsNullOrEmpty(secondaryKeyName))
                                            {
                                                var tempLimitKeys = await FilterSecondaryKeys(physicalFileName, secondaryKeyName, filterValue, newLimitingKeys, columnData, filterCriteria);
                                                if (tempLimitKeys != null && tempLimitKeys.Any())
                                                {
                                                    limitKeys.AddRange(tempLimitKeys);
                                                }
                                            }
                                            else
                                            {
                                                var fileSuiteKeys = await DataReader.SelectAsync(physicalFileName, newLimitingKeys, filterCriteria.ToString());
                                                if (fileSuiteKeys != null && fileSuiteKeys.Any())
                                                {
                                                    foreach (var key in fileSuiteKeys)
                                                    {
                                                        limitKeys.Add(string.Concat(physicalFileName, "+", key));
                                                    }
                                                }
                                            }
                                        }
                                        if (limitKeys == null || !limitKeys.Any())
                                        {
                                            return new CacheSupport.KeyCacheRequirements()
                                            {
                                                NoQualifyingRecords = true
                                            };
                                        }
                                        else
                                        {
                                            limitingKeys = limitingKeys.Intersect(limitKeys.ToArray()).ToArray();
                                        }
                                    }
                                    else
                                    {
                                        // Primary and Secondary Key processing
                                        if (!string.IsNullOrEmpty(secondaryKeyName))
                                        {
                                            string primaryEntity = configuration.PrimaryEntity;
                                            if (!string.IsNullOrEmpty(configuration.PrimaryApplication) && !string.IsNullOrEmpty(configuration.PrimaryTableName))
                                            {
                                                primaryEntity = string.Concat(configuration.PrimaryApplication, ".VALCODES");
                                            }
                                            var limitKeys = await FilterSecondaryKeys(primaryEntity, secondaryKeyName, filterValue, limitingKeys, columnData, filterCriteria);

                                            if (limitKeys == null || !limitKeys.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements()
                                                {
                                                    NoQualifyingRecords = true
                                                };
                                            }
                                            else
                                            {
                                                limitingKeys = limitingKeys.Intersect(limitKeys.ToArray()).ToArray();
                                            }
                                        }
                                        else
                                        {
                                            // Filter processing on primary key or GUID
                                            var newLimitingKeys = await DataReader.SelectAsync(selectFileName, limitingKeys, filterCriteria.ToString());

                                            if (newLimitingKeys == null || !newLimitingKeys.Any())
                                            {
                                                return new CacheSupport.KeyCacheRequirements()
                                                {
                                                    NoQualifyingRecords = true
                                                };
                                            }
                                            if (limitingKeys != null && limitingKeys.Any())
                                            {
                                                limitingKeys = limitingKeys.Intersect(newLimitingKeys.ToArray()).ToArray();
                                            }
                                            else
                                            {
                                                limitingKeys = newLimitingKeys.ToArray();
                                            }
                                        }
                                    }
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

            if (string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                // We are using GUIDs to identify the record key
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
            }
            else
            {
                foreach (var id in subList)
                {
                    string recordKey = id.Contains('+') ? id.Split('+')[1] : id;
                    string guid = EncodePrimaryKey(id);
                    if (!string.IsNullOrEmpty(configuration.PrimaryTableName) && !id.Contains('+'))
                    {
                        guid = EncodePrimaryKey(string.Concat(configuration.PrimaryEntity, "+", configuration.PrimaryTableName, "+", recordKey));
                    }
                    else
                    {
                        if (!id.Contains('+'))
                        {
                            guid = EncodePrimaryKey(string.Concat(configuration.PrimaryEntity, "+", id));
                        }
                    }
                    EthosApiBuilder extendedData = await BuildEthosApiBuilder(guid, recordKey, configuration.PrimaryEntity);
                    extendedDatas.Add(extendedData);
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
                throw new ArgumentNullException("id", string.Format("Must provide an id to get the {0}.", configuration.ResourceName));
            }
            var fileName = !string.IsNullOrEmpty(configuration.PrimaryGuidFileName) ? configuration.PrimaryGuidFileName : configuration.PrimaryEntity;
            string primaryKey = string.Empty;
            string secondaryKey = string.Empty;
            if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                var idSplit = UnEncodePrimaryKey(id).Split('+');
                if (idSplit.Count() > 1)
                {
                    fileName = idSplit[0];
                    primaryKey = idSplit[1];
                    if (idSplit.Count() > 2)
                    {
                        secondaryKey = idSplit[2];
                    }
                }
                else
                {
                    primaryKey = idSplit[0];
                }
                // Attempt to select the primary record pointed to from the entity.  If we can't select the record, it may have been deleted.
                var recordKey = await DataReader.SelectAsync(fileName, new string[] { primaryKey }, "");
                if (recordKey == null || !recordKey.Any())
                {
                    throw new KeyNotFoundException(string.Format("Invalid Id for {0}: '{1}'", configuration.ResourceName, id));
                }
                if (!string.IsNullOrEmpty(configuration.SecondaryKeyName))
                {
                    if (string.IsNullOrEmpty(secondaryKey))
                    {
                        throw new KeyNotFoundException(string.Format("Invalid Id for {0}: '{1}'", configuration.ResourceName, id));
                    }
                    var columns = new string[] { configuration.SecondaryKeyName };
                    var readResult = await DataReader.ReadRecordColumnsAsync(fileName, primaryKey, columns);
                    string columnValue = string.Empty;
                    if (readResult != null && readResult.TryGetValue(configuration.SecondaryKeyName, out columnValue))
                    {
                        var columnSplit = columnValue.Replace(_SM, _VM).Split(_VM);
                        if (!columnSplit.Contains(secondaryKey))
                        {
                            throw new KeyNotFoundException(string.Format("Invalid Id for {0}: '{1}'", configuration.ResourceName, id));
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException(string.Format("Invalid Id for {0}: '{1}'", configuration.ResourceName, id));
                    }
                }
            }
            else
            {
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
                        else
                        {
                            fileName = guidEntity.Entity;
                        }
                    }
                }
                if (guidEntity == null || (guidEntity.Entity != fileName && fileName != "VALCODES"))
                {
                    if (guidEntity != null && !string.IsNullOrEmpty(guidEntity.Entity))
                    {
                        var errorMessage = string.Format("GUID '{0}' has different entity, '{1}', than expected, {2}.", id, guidEntity.Entity, fileName);
                        exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage));
                        throw exception;
                    }
                    else
                    {
                        throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", configuration.ResourceName, id));
                        //var errorMessage = string.Format("Invalid GUID for {0}: '{1}'", configuration.ResourceName, id);
                        //exception.AddError(new RepositoryError("GUID.Not.Found", errorMessage));
                        //throw exception;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(configuration.PrimaryTableName) && guidEntity.PrimaryKey != configuration.PrimaryTableName)
                    {
                        var errorMessage = string.Format("GUID '{0}' has different entity, '{1}' or primary key, '{2}', than expected.", id, guidEntity.Entity, guidEntity.PrimaryKey);
                        exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage));
                        throw exception;
                    }

                    // Attempt to select the primary record pointed to from the entity.  If we can't select the record, it may have been deleted.
                    var recordKey = await DataReader.SelectAsync(guidEntity.Entity, new string[] { guidEntity.PrimaryKey }, "");
                    if (recordKey == null || !recordKey.Any())
                    {
                        throw new KeyNotFoundException(string.Format("Invalid GUID for {0}: '{1}'", configuration.ResourceName, id));
                        //var errorMessage = string.Format("Invalid GUID for {0}: '{1}'", configuration.ResourceName, id);
                        //exception.AddError(new RepositoryError("GUID.Not.Found", errorMessage));
                        //throw exception;
                    }
                }
                primaryKey = guidEntity.PrimaryKey;
                secondaryKey = guidEntity.SecondaryKey;
            }

            // Insure that the selected ID will pass all selection criteria defined on the API.
            var selectFileName = !string.IsNullOrEmpty(configuration.SelectFileName) ? configuration.SelectFileName : configuration.PrimaryEntity;
            if (string.IsNullOrEmpty(selectFileName))
            {
                selectFileName = configuration.PrimaryGuidFileName;
            }
            if (await IsEthosFileSuiteTemplateFile(selectFileName))
            {
                selectFileName = fileName;
            }
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllSelectedRecordsCache, selectFileName, id);

            string[] limitingKeys = new string[] { primaryKey };
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
            if (string.IsNullOrEmpty(configuration.SavingField) && string.IsNullOrEmpty(configuration.PrimaryTableName))
            {
                criteria = BuildSelectCriteria(configuration.SelectColumnName, "", "", configuration.SavingField,
                    configuration.SavingOption, configuration.SelectionCriteria, configuration.SortColumns);
                if (selectParagraph != null && selectParagraph.Any())
                {
                    // If we don't have any criteria but we still have a paragraph to execute
                    // then we don't want to select the entire file so we will null out the file name
                    // so that the CTX will only execute the paragraph.  Once the paragraph is
                    // executed, then make sure that the keys exist on the primary selection table.
                    if (criteria.Length == 0)
                    {
                        selectParagraph.Add(string.Format("MIOSEL {0} REQUIRE.SELECT", fileName));
                        selectFileName = string.Empty;
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
                criteria.Clear();
                selectFileName = string.Empty;
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
                var text = string.IsNullOrEmpty(configuration.PrimaryKeyName) ? "GUID" : "KEY";
                exception.AddError(new RepositoryError("Validation.Exception", string.Format("The record represented by " + text + " '{0}' doesn't meet primary selection criteria.", id))
                {
                    SourceId = primaryKey,
                    Id = id
                });
                throw exception;
            }

            EthosApiBuilder extendedData = null;
            if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                extendedData = await BuildEthosApiBuilder(id, primaryKey, configuration.PrimaryEntity);
            }
            else
            {
                extendedData = await BuildEthosApiBuilder(id, "", configuration.PrimaryEntity);
            }

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
        /// <param name="configuration">Configuration from EthosApiBuilder</param>
        /// <returns>Domain.Base.Entities.EthosApiBuilderResponse</returns>
        public async Task<Domain.Base.Entities.EthosApiBuilder> UpdateEthosApiBuilderAsync(EthosApiBuilder extendedDataRequest, EthosApiConfiguration configuration)
        {
            EthosApiBuilder extendedDataResponse = null;
            if (configuration.ApiType == "T")
            {
                ProcessScreenApiRequest updateRequest = new ProcessScreenApiRequest();
                updateRequest.ColumnData = new List<ColumnData>();
                updateRequest.KeyData = new List<KeyData>();
                updateRequest.ProcessId = configuration.ProcessId;
                updateRequest.ProcessMode = (string.IsNullOrEmpty(extendedDataRequest.Code) || extendedDataRequest.Code.StartsWith("$NEW")) ? "POST" : "PUT";

                var extendedDataTuple = GetEthosExtendedDataLists();

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    foreach (var extendedData in extendedDataTuple.Item1)
                    {
                        var extendedValue = extendedDataTuple.Item2.ElementAt(extendedDataTuple.Item1.IndexOf(extendedData));
                        if (!string.IsNullOrEmpty(extendedData) && !extendedData.Equals("EDME.VERSION.NUMBER", StringComparison.OrdinalIgnoreCase))
                        {
                            updateRequest.ColumnData.Add(new ColumnData()
                            {
                                ColumnNames = extendedData,
                                ColumnValues = !string.IsNullOrEmpty(extendedValue) ? extendedValue : String.Empty
                            });
                        }
                    }
                }

                updateRequest.KeyData.Add(new KeyData()
                {
                    PrimaryKeyNames = extendedDataRequest.Description,
                    PrimaryKeyValues = extendedDataRequest.Code
                });

                ProcessScreenApiResponse updateResponse = await transactionInvoker.ExecuteAsync<ProcessScreenApiRequest, ProcessScreenApiResponse>(updateRequest);

                if (updateResponse.ProcessScreenApiErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating {0}.", configuration.ResourceName);
                    var exception = new RepositoryException();
                    updateResponse.ProcessScreenApiErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)
                    {
                        SourceId = extendedDataRequest.Code == "$NEW" ? string.Empty : extendedDataRequest.Code
                    })
                    );

                    logger.Error(errorMessage);
                    throw exception;
                }
                var recordKeyData = updateResponse.KeyData.FirstOrDefault(kd => kd.PrimaryKeyNames == extendedDataRequest.Description);
                if (recordKeyData != null && !string.IsNullOrEmpty(recordKeyData.PrimaryKeyValues))
                {
                    var recordKey = recordKeyData.PrimaryKeyValues;
                    var recordGuid = EncodePrimaryKey(string.Concat(extendedDataRequest.Description, "+", recordKey));
                    extendedDataResponse = new EthosApiBuilder(recordGuid, recordKey, extendedDataRequest.Description);
                }
            }
            else
            {
                UpdateEthosApiBuilderRequest updateRequest = new UpdateEthosApiBuilderRequest();

                var extendedDataTuple = GetEthosExtendedDataLists();

                if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
                {
                    updateRequest.ExtendedNames = extendedDataTuple.Item1;
                    updateRequest.ExtendedValues = extendedDataTuple.Item2;
                }
                string secondaryKey = string.Empty;
                if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
                {
                    updateRequest.RecordGuid = string.Empty;
                    updateRequest.RecordKey = extendedDataRequest.Code;
                    updateRequest.Entity = extendedDataRequest.Description;
                    updateRequest.ResourceName = configuration.ResourceName;
                    if (!string.IsNullOrEmpty(configuration.SecondaryKeyName))
                    {
                        if (!updateRequest.ExtendedNames.Contains(configuration.SecondaryKeyName))
                        {
                            exception.AddError(new RepositoryError("Create.Update.Exception", string.Format("The POST request is missing the secondary key field '{0}'. Create not allowed.", configuration.SecondaryKeyName)));
                            throw exception;
                        }
                        var secondaryKeyIndex = updateRequest.ExtendedNames.IndexOf(configuration.SecondaryKeyName);
                        if (secondaryKeyIndex >= 0) secondaryKey = updateRequest.ExtendedValues.ElementAt(secondaryKeyIndex);
                    }
                }
                else
                {
                    updateRequest.RecordGuid = extendedDataRequest.Guid;
                    updateRequest.RecordKey = extendedDataRequest.Code != "$NEW" ? extendedDataRequest.Code : string.Empty;
                    updateRequest.Entity = extendedDataRequest.Description;
                    updateRequest.ResourceName = configuration.ResourceName;
                }

                UpdateEthosApiBuilderResponse updateResponse = await transactionInvoker.ExecuteAsync<UpdateEthosApiBuilderRequest, UpdateEthosApiBuilderResponse>(updateRequest);

                if (updateResponse.UpdateEthosApiBuilderErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating {0}.", configuration.ResourceName);
                    var exception = new RepositoryException();
                    updateResponse.UpdateEthosApiBuilderErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)
                    {
                        SourceId = updateRequest.RecordKey == "$NEW" ? string.Empty : updateRequest.RecordKey,
                        Id = updateRequest.RecordGuid
                    })
                    );

                    logger.Error(errorMessage);
                    throw exception;
                }
                extendedDataResponse = new EthosApiBuilder(updateResponse.RecordGuid, updateResponse.RecordKey, updateResponse.Entity);
            }
            
            return extendedDataResponse;

        }
        #endregion

        #region DELETE
        /// <summary>
        /// Updates API information
        /// </summary>
        /// <param name="extendedDataRequest">Domain.Base.Entities.EthosApiBuilder</param>
        /// <param name="configuration">Configuration from EthosApiBuilder</param>
        /// <returns>Domain.Base.Entities.EthosApiBuilderResponse</returns>
        public async Task DeleteEthosApiBuilderAsync(string guid, EthosApiConfiguration configuration)
        {
            DeleteEthosApiBuilderRequest deleteRequest = new DeleteEthosApiBuilderRequest();

            if (!string.IsNullOrEmpty(configuration.PrimaryKeyName))
            {
                var entity = configuration.PrimaryEntity;
                var primaryKey = string.Empty;
                var secondaryKey = string.Empty;
                var recordKey = UnEncodePrimaryKey(guid);
                if (recordKey.Contains('+'))
                {
                    var idSplit = recordKey.Split('+');
                    if (idSplit.Count() > 1)
                    {
                        entity = idSplit[0];
                        primaryKey = idSplit[1];
                        if (idSplit.Count() > 2)
                        {
                            secondaryKey = idSplit[2];
                        }
                    }
                    else
                    {
                        primaryKey = idSplit[0];
                    }

                }
                else
                {
                    primaryKey = recordKey;
                }
                if (!string.IsNullOrEmpty(configuration.PrimaryTableName))
                {
                    primaryKey = string.Concat(configuration.PrimaryTableName, "|", primaryKey);
                }
                if (!string.IsNullOrEmpty(configuration.SecondaryKeyName))
                {
                    entity = string.Concat(entity, "|", configuration.SecondaryKeyName);
                    primaryKey = string.Concat(primaryKey, "|", secondaryKey);
                }
                deleteRequest.RecordKey = primaryKey;
                deleteRequest.Entity = entity;
            }
            else
            {
                deleteRequest.RecordGuid = guid;
                deleteRequest.RecordKey = configuration.PrimaryTableName;
                deleteRequest.Entity = !string.IsNullOrEmpty(configuration.PrimaryGuidFileName) ? configuration.PrimaryGuidFileName : configuration.PrimaryEntity;
            }
            deleteRequest.ResourceName = configuration.ResourceName;

            DeleteEthosApiBuilderResponse deleteResponse = await transactionInvoker.ExecuteAsync<DeleteEthosApiBuilderRequest, DeleteEthosApiBuilderResponse>(deleteRequest);

            if (deleteResponse.DeleteEthosApiBuilderErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating {0}.", configuration.ResourceName);
                var exception = new RepositoryException();
                deleteResponse.DeleteEthosApiBuilderErrors.ForEach(e => exception.AddError(new RepositoryError(e.ErrorCodes, e.ErrorMessages)
                    {
                        Id = deleteRequest.RecordGuid
                    })
                );

                logger.Error(errorMessage);
                if (deleteResponse.DeleteEthosApiBuilderErrors.FirstOrDefault().ErrorCodes.Equals("GUID.Not.Found", StringComparison.OrdinalIgnoreCase))
                {
                    throw new KeyNotFoundException(deleteResponse.DeleteEthosApiBuilderErrors.FirstOrDefault().ErrorMessages);
                }
                throw exception;
            }
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
                if (await IsEthosFileSuiteTemplateFile(primaryFileName, false) || ids.FirstOrDefault().Contains("+"))
                {
                    var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct().ToList()
                    .ConvertAll(p => new RecordKeyLookup(p.Split('+')[0].ToString(), p.Split('+')[1].ToString(), configuration.PrimaryGuidSource, p.Split('+')[2].ToString(), false)).ToArray();
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
                                limitingKeys = ids.ToArray();
                            }
                            var limitingKeyValues = limitingKeys.ToList().Distinct();
                            var sortCriteria = new List<EthosApiSortCriteria>()
                            {
                                new EthosApiSortCriteria(configuration.PrimaryGuidSource, "BY")
                            };

                            List<string> limitingPrimaryKeys = new List<string>();
                            foreach (var limitingKey in limitingKeyValues)
                            {
                                if (!string.IsNullOrEmpty(limitingKey))
                                {
                                    var selectCriteria = new List<EthosApiSelectCriteria>();
                                    selectCriteria.AddRange(configuration.SelectionCriteria);
                                    selectCriteria.Add(
                                        new EthosApiSelectCriteria("WITH", configuration.PrimaryGuidSource, "EQ", "'" + limitingKey + "'")
                                    );

                                    var criteria = BuildSelectCriteria("", "", "", "", "",
                                        selectCriteria, sortCriteria);

                                    var selectedPrimaryKeys = await DataReader.SelectAsync(primaryFileName, criteria.ToString());
                                    limitingPrimaryKeys.AddRange(selectedPrimaryKeys.ToList());
                                }
                            }

                            List<RecordKeyLookup> guidLookupList = new List<RecordKeyLookup>();
                            foreach (var primaryKey in limitingPrimaryKeys)
                            {
                                var guidLookupArray = ids
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Distinct().ToList()
                                   .ConvertAll(p => new RecordKeyLookup(primaryFileName, primaryKey, configuration.PrimaryGuidSource, p, false));
                                guidLookupList.AddRange(guidLookupArray);
                            }
                            recordKeyLookupResults = await DataReader.SelectAsync(guidLookupList.ToArray());
                        }
                    }
                }
            }
            foreach (var recordKeyLookupResult in recordKeyLookupResults)
            {
                try
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!guidCollection.ContainsKey(recordKeyLookupResult.Key))
                    {
                        if (recordKeyLookupResult.Value != null && !string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid))
                        {
                            guidCollection.Add(recordKeyLookupResult.Key, recordKeyLookupResult.Value.Guid);
                        }
                    }

                    int index = splitKeys.Count() - 1;
                    if (!guidCollection.ContainsKey(splitKeys[index]))
                    {
                        if (recordKeyLookupResult.Value != null && !string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid))
                        {
                            guidCollection.Add(splitKeys[index], recordKeyLookupResult.Value.Guid);
                        }
                    }
                }
                catch (Exception) // Do not throw error.
                {
                }
            }

            return new Tuple<List<string>, Dictionary<string, string>>(recordKeys, guidCollection);

        }

        /// <summary>
        /// Get a guid collection from a list of keys
        /// </summary>
        /// <param name="ids">List of keys to get a collection for.</param>
        /// <param name="configuration">Configuration parameters for API</param>
        /// <returns>Dictionary of keys and guids</returns>
        private async Task<Tuple<List<string>, Dictionary<string, string>>> GetKeyCollectionAsync(IEnumerable<string> ids, EthosApiConfiguration configuration, string[] limitingKeys)
        {

            List<string> recordKeys = ids.ToList();
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Tuple<List<string>, Dictionary<string, string>>(recordKeys, new Dictionary<string, string>());
            }
            var guidCollection = new Dictionary<string, string>();
            Dictionary<string, RecordKeyLookupResult> recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
            string primaryFileName = configuration.PrimaryEntity;

            if (!string.IsNullOrEmpty(configuration.SecondaryKeyName))
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
                        .ConvertAll(p => new RecordKeyLookup(primaryFileName, configuration.PrimaryTableName, configuration.SecondaryKeyName, p, false)).ToArray();
                        recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);
                    }
                    else
                    {
                        var guidLookup = ids
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Distinct().ToList()
                            .ConvertAll(p => new RecordKeyLookup(primaryFileName, p, configuration.PrimaryKeyName, p, false)).ToArray();
                        recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                        // If we don't find any GUIDs with the same primary key as the secondary key, then
                        // try selecting from limiting keys.
                        var selectedGuids = recordKeyLookupResults.Where(s => s.Value != null && !string.IsNullOrEmpty(s.Value.Guid));
                        if (selectedGuids == null || !selectedGuids.Any())
                        {
                            if (limitingKeys == null || !limitingKeys.Any())
                            {
                                limitingKeys = ids.ToArray();
                            }
                            var limitingKeyValues = limitingKeys.ToList().Distinct();
                            var sortCriteria = new List<EthosApiSortCriteria>()
                            {
                                new EthosApiSortCriteria(configuration.PrimaryGuidSource, "BY")
                            };

                            List<string> limitingPrimaryKeys = new List<string>();
                            foreach (var limitingKey in limitingKeyValues)
                            {
                                if (!string.IsNullOrEmpty(limitingKey))
                                {
                                    var selectCriteria = new List<EthosApiSelectCriteria>();
                                    selectCriteria.AddRange(configuration.SelectionCriteria);
                                    selectCriteria.Add(
                                        new EthosApiSelectCriteria("WITH", configuration.PrimaryGuidSource, "EQ", "'" + limitingKey + "'")
                                    );

                                    var criteria = BuildSelectCriteria("", "", "", "", "",
                                        selectCriteria, sortCriteria);

                                    var selectedPrimaryKeys = await DataReader.SelectAsync(primaryFileName, criteria.ToString());
                                    limitingPrimaryKeys.AddRange(selectedPrimaryKeys.ToList());
                                }
                            }

                            recordKeyLookupResults = new Dictionary<string, RecordKeyLookupResult>();
                            foreach (var primaryKey in limitingPrimaryKeys)
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
                        string guid = splitKeys[index];
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

        private async Task<string> GetFileSuitePrefix(string fileName, bool bypassCache = false)
        {
            var ethosFileSuitesList = await GetEthosFileSuiteTemplatesAsync(bypassCache);

            foreach (var template in ethosFileSuitesList)
            {
                foreach (var templateFile in template.FstmpltEntityAssociation)
                {
                    if (templateFile.FstTemplateAssocMember == fileName)
                    {
                        return templateFile.FstFilePrefixAssocMember;
                    }
                }
            }
            return string.Empty;
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
                case "BCT":
                    return await GetBudgetYearsAsync(bypassCache);
                case "BJT":
                    return await GetBudgetYearsAsync(bypassCache);
                case "BOC":
                    return await GetBudgetYearsAsync(bypassCache);
                case "BWK":
                    return await GetBudgetYearsAsync(bypassCache);
                case "BPJ":
                    return await GetBudgetYearsAsync(bypassCache);
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
        /// Get a collection of General Ledger years.
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of General Ledger years</returns>
        private async Task<IEnumerable<string>> GetGeneralLedgerYearsAsync(bool bypassCache = false)
        {
            if (_genLedgerYears == null)
            {
                const string ethosGeneralLedgerYearsCacheKey = "AllEthosApiBuilderGeneralLedgerYears";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosGeneralLedgerYearsCacheKey)))
                {
                    ClearCache(new List<string> { ethosGeneralLedgerYearsCacheKey });
                }
                var genLedgerYears = await GetOrAddToCacheAsync<List<string>>(ethosGeneralLedgerYearsCacheKey,
                    async () =>
                    {
                        var suiteYears = await DataReader.SelectAsync("GEN.LDGR", "WITH GEN.LDGR.TYPE EQ 'LEDGER' AND WITH GEN.LDGR.STATUS NE 'X'");
                        return suiteYears.ToList();
                    }, CacheTimeout);

                _genLedgerYears = genLedgerYears;
            }
            return _genLedgerYears;
        }

        private IEnumerable<string> _budgetYears = null;
        /// <summary>
        /// Get a collection of budget/years.
        /// </summary>
        /// <param name="bypassCache">Bypass cache flag</param>
        /// <returns>Collection of budgets/years</returns>
        private async Task<IEnumerable<string>> GetBudgetYearsAsync(bool bypassCache = false)
        {
            if (_budgetYears == null)
            {
                const string ethosBudgetYearsCacheKey = "AllEthosApiBuilderBudgetYears";

                if (bypassCache && ContainsKey(BuildFullCacheKey(ethosBudgetYearsCacheKey)))
                {
                    ClearCache(new List<string> { ethosBudgetYearsCacheKey });
                }
                var budgetYears = await GetOrAddToCacheAsync<List<string>>(ethosBudgetYearsCacheKey,
                    async () =>
                    {
                        var suiteYears = await DataReader.SelectAsync("BUDGET", "WITH BU.LOCATION EQ 'O'");
                        return suiteYears.ToList();
                    }, CacheTimeout);

                _budgetYears = budgetYears;
            }
            return _budgetYears;
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

        private StringBuilder BuildSelectCriteria(string columnName, string value, string oper, string savingColumn, string savingOption, List<EthosApiSelectCriteria> selectionCriteria, List<EthosApiSortCriteria> sortCriteria)
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
                    if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(value))
                    {
                        criteria.Append(" ");
                        if (!string.IsNullOrEmpty(oper))
                        {
                            criteria.Append(oper);
                        }
                        else
                        {
                            criteria.Append(select.SelectOper);
                        }
                        criteria.Append(" ");
                        criteria.Append(select.SelectValue.Replace(columnName, value));
                    }
                    else
                    {
                        criteria.Append(select.SelectOper);
                        criteria.Append(" ");
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

        /// <summary>
        /// Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Encoded string to use as guid on a non-guid based API.</returns>
        private string EncodePrimaryKey(string id)
        {
            // Preserve all lower case and dashes in original key by manually escaping those characters.
            var returnData = id.Replace("-", "-2D").Replace("a", "-61").
                Replace("b", "-62").Replace("c", "-63").Replace("d", "-64").
                Replace("e", "-65").Replace("f", "-66").Replace("g", "-67").
                Replace("h", "-68").Replace("i", "-69").Replace("j", "-6A").
                Replace("k", "-6B").Replace("l", "-6C").Replace("m", "-6D").
                Replace("n", "-6E").Replace("o", "-6F").Replace("p", "-70").
                Replace("q", "-71").Replace("r", "-72").Replace("s", "-73").
                Replace("t", "-74").Replace("u", "-75").Replace("v", "-76").
                Replace("w", "-77").Replace("x", "-78").Replace("y", "-79").
                Replace("z", "-7A");
            return Uri.EscapeDataString(returnData).Replace("%", "-").ToLower();
        }

        /// <summary>
        /// Un-Encode a primary key for use in Extensibility
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Un-Encoded string taken from a non-guid based API guid.</returns>
        private string UnEncodePrimaryKey(string id)
        {
            var primaryKey = id.Replace("-", "%").ToUpper();
            return Uri.UnescapeDataString(primaryKey);
        }

        /// <summary>
        /// Filter on all primary/secondary key values and rebuild the limiting list.
        /// </summary>
        /// <param name="primaryEntity"></param>
        /// <param name="secondaryKeyName"></param>
        /// <param name="filterValue"></param>
        /// <param name="limitingKeys"></param>
        /// <param name="columnData"></param>
        /// <param name="filterCriteria"></param>
        /// <returns></returns>
        private async Task<List<string>> FilterSecondaryKeys(string primaryEntity, string secondaryKeyName, string filterValue, string[] limitingKeys, EthosExtensibleDataFilter columnData, StringBuilder filterCriteria)
        {
            var filterOper = columnData.FilterOper;

            List<string> limitKeys = new List<string>();
            if (limitingKeys == null || !limitingKeys.Any())
            {
                return limitKeys;
            }

            if (string.IsNullOrEmpty(secondaryKeyName))
            {
                return limitingKeys.ToList();
            }

            var sortColumns = new List<EthosApiSortCriteria>()
            {
                new EthosApiSortCriteria(secondaryKeyName, "BY.EXP")
            };

            var newLimitingKeys = limitingKeys.Select(gv => (gv.Contains('+') ? gv.Split('+')[1] : gv)).Distinct().ToArray();
            if (columnData.SelectFileName != primaryEntity && !string.IsNullOrEmpty(columnData.SelectFileName) && columnData.SelectFileName != "VALCODES")
            {
                var tempLimitKeys = await DataReader.SelectAsync(columnData.SelectFileName, newLimitingKeys, filterCriteria.ToString());
                newLimitingKeys = tempLimitKeys;
            }
            var primarySelect = BuildSelectCriteria(columnData.SelectColumnName, filterValue, filterOper, string.Empty,
                 string.Empty, columnData.SelectionCriteria, sortColumns).ToString();
            var primaryKeyList = await DataReader.SelectAsync(primaryEntity, newLimitingKeys, primarySelect);

            var secondarySelect = BuildSelectCriteria(columnData.SelectColumnName, filterValue, filterOper, secondaryKeyName,
                string.Empty, columnData.SelectionCriteria, sortColumns).ToString();
            var secondaryKeyList = await DataReader.SelectAsync(primaryEntity, newLimitingKeys, secondarySelect);
            var convertedValue = ConvertFilterValue(filterValue, columnData.JsonPropertyType.ToLower());

            for (int i = 0; i < primaryKeyList.Count(); i++)
            {
                if (!string.IsNullOrEmpty(primaryKeyList[i]) && secondaryKeyList.Count() > i && !string.IsNullOrEmpty(secondaryKeyList[i]))
                {
                    bool includeKey = false;
                    if (columnData.DatabaseUsageType != null && (columnData.DatabaseUsageType.Equals("K", StringComparison.OrdinalIgnoreCase)
                        || (columnData.DatabaseUsageType.Equals("D", StringComparison.OrdinalIgnoreCase) 
                        && columnData.ColleagueFileName.Equals(primaryEntity, StringComparison.OrdinalIgnoreCase))))
                    {
                        includeKey = true;
                    }
                    else
                    {
                        if (columnData.ColleagueColumnName != secondaryKeyName)
                        {
                            var columnNames = new List<string>() { secondaryKeyName };
                            if (columnData.SelectFileName.Equals(primaryEntity, StringComparison.OrdinalIgnoreCase) || (primaryEntity.EndsWith("VALCODES") && columnData.SelectFileName.EndsWith("VALCODES")))
                            {
                                columnNames.Add(columnData.ColleagueColumnName);
                            }
                            Dictionary<string, string> dataRecordColumns = new Dictionary<string, string>();
                            bool tryAgain = false;
                            try
                            {
                                dataRecordColumns = await DataReader.ReadRecordColumnsAsync(primaryEntity, primaryKeyList[i], columnNames.ToArray());
                            }
                            catch
                            {
                                tryAgain = true;
                            }
                            if (primaryEntity.EndsWith("VALCODES") && tryAgain)
                            {
                                try
                                {
                                    dataRecordColumns = await DataReader.ReadRecordColumnsAsync("VALCODES", primaryKeyList[i], columnNames.ToArray());
                                }
                                catch
                                {
                                    return new List<string>();
                                }
                            }
                            else
                            {
                                if (tryAgain)
                                {
                                    return new List<string>();
                                }
                            }
                            string filterData = string.Empty;
                            string secondaryKey = string.Empty;
                            if (dataRecordColumns != null && dataRecordColumns.TryGetValue(secondaryKeyName, out secondaryKey) && dataRecordColumns.TryGetValue(columnData.ColleagueColumnName, out filterData))
                            {
                                var splitSecondary = secondaryKey.Split(_VM);
                                var splitFilterData = filterData.Split(_VM);
                                int idx = 0;
                                foreach (var secondary in splitSecondary)
                                {
                                    if (secondary.Equals(secondaryKeyList[i], StringComparison.OrdinalIgnoreCase) && splitFilterData != null && splitFilterData.Count() > idx)
                                    {
                                        includeKey = CompareFilterValue(splitFilterData[idx], convertedValue, filterOper);
                                    }
                                    else
                                    {
                                        if (splitFilterData.Count() != splitSecondary.Count())
                                        {
                                            includeKey = CompareFilterValue(splitFilterData[0], convertedValue, filterOper);
                                        }
                                    }
                                    idx++;
                                }
                            }
                        }
                        else
                        {
                            includeKey = CompareFilterValue(secondaryKeyList[i], convertedValue, filterOper);
                        }
                    }
                    if (includeKey)
                    {
                        // Unidata will return a list of keys, Value Mark, and Position whereby SQL only returns the keys
                        limitKeys.Add(primaryEntity + "+" + primaryKeyList[i].Split(_VM)[0] + "+" + secondaryKeyList[i]);
                    }
                }
            }
            return limitKeys.Distinct().ToList();
        }

        /// <summary>
        /// Convert a filter value into it's Unidata Value
        /// </summary>
        /// <param name="filterValue"></param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        private string ConvertFilterValue(string filterValue, string propertyType)
        {
            string outputData = filterValue;
            switch (propertyType.ToLower())
            {
                case ("date"):
                    {
                        try
                        {
                            var dateValues = filterValue.Split('/');
                            if (dateValues.Count() >= 3)
                            {
                                var year = Convert.ToInt32(dateValues[2]);
                                var month = Convert.ToInt32(dateValues[0]);
                                var day = Convert.ToInt32(dateValues[1]);
                                outputData = DmiString.DateTimeToPickDate(new DateTime(year, month, day)).ToString();
                            }
                        }
                        catch (FormatException ex)
                        {

                        }
                    }
                    break;
                case ("datetime"):
                    {
                        try
                        {
                            //DateTime temp = Convert.ToDateTime(filterData);
                            //filterData = string.Concat(temp.Month.ToString(), "/", temp.Day.ToString(), "/", temp.Year.ToString());
                        }
                        catch (FormatException ex)
                        {

                        }
                    }
                    break;
                case ("time"):
                    {
                        try
                        {
                            //filterData = DateTime.Today.ToString() + "T" + filterData;
                            //DateTime temp = Convert.ToDateTime(filterData);
                            //filterData = string.Concat(temp.Hour.ToString(), ":", temp.Minute.ToString(), ":", temp.Second.ToString());
                        }
                        catch (FormatException ex)
                        {

                        }
                    }
                    break;
                case ("number"):
                    {
                        try
                        {
                            decimal temp = Convert.ToDecimal(filterValue);
                            outputData = temp.ToString();
                        }
                        catch (FormatException ex)
                        {

                        }
                    }
                    break;
                default:
                    {
                        outputData = filterValue;
                        break;
                    }
            }

            return outputData;
        }

        private bool CompareFilterValue(string filterData, string filterValue, string filterOper)
        {
            if (string.IsNullOrEmpty(filterOper)) filterOper = "EQ";
            if (string.IsNullOrEmpty(filterData)) return false;
            if (string.IsNullOrEmpty(filterValue)) return false;

            try
            {
                bool includeKey;
                switch (filterOper.ToUpper())
                {
                    case "LE":
                        includeKey = (Convert.ToDecimal(filterData) <= Convert.ToDecimal(filterValue));
                        break;
                    case "GE":
                        includeKey = (Convert.ToDecimal(filterData) >= Convert.ToDecimal(filterValue));
                        break;
                    case "LT":
                        includeKey = (Convert.ToDecimal(filterData) < Convert.ToDecimal(filterValue));
                        break;
                    case "GT":
                        includeKey = (Convert.ToDecimal(filterData) > Convert.ToDecimal(filterValue));
                        break;
                    case "NE":
                        includeKey = (filterData != filterValue);
                        break;
                    case "EQ":
                        includeKey = (filterData == filterValue);
                        break;
                    default:
                        includeKey = (filterData == filterValue);
                        break;
                }

                return includeKey;
            }
            catch
            {
                var comparison = string.Compare(filterData, filterValue, StringComparison.OrdinalIgnoreCase);
                if (filterOper.Equals("LE", StringComparison.OrdinalIgnoreCase))
                {
                    if (comparison <= 0) return true;
                }
                if (filterOper.Equals("GE", StringComparison.OrdinalIgnoreCase))
                {
                    if (comparison >= 0) return true;
                }
                if (filterOper.Equals("LT", StringComparison.OrdinalIgnoreCase))
                {
                    if (comparison < 0) return true;
                }
                if (filterOper.Equals("GT", StringComparison.OrdinalIgnoreCase))
                {
                    if (comparison > 0) return true;
                }
                return false;
            }
        }
    }
}