// Copyright 2019-2023 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType]
    public class InstitutionsAttendRepository : BaseColleagueRepository, IInstitutionsAttendRepository
    {
        private RepositoryException exception;
        readonly int readSize;
        private ApplValcodes _institutionTypes;

        public InstitutionsAttendRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }


        /// <summary>
        /// Using a collection of  ids, get a dictionary collection of associated guids
        /// </summary>
        /// <param name="insAttendIds">collection of ids</param>
        /// <returns>Dictionary consisting of an id (key) and guid (value)</returns>
        public async Task<Dictionary<string, string>> GetInsAttendGuidsCollectionAsync(IEnumerable<string> insAttendIds)
        {
            if ((insAttendIds == null) || (insAttendIds != null && !insAttendIds.Any()))
            {
                return new Dictionary<string, string>();
            }

            var guidCollection = new Dictionary<string, string>();
            try
            {
                var instAttendGuidLookup = insAttendIds.Distinct().ToList().ConvertAll(p => new RecordKeyLookup("INSTITUTIONS.ATTEND", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(instAttendGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!guidCollection.ContainsKey(splitKeys[1]))
                    {
                        if ((recordKeyLookupResult.Value != null) && (!string.IsNullOrEmpty(recordKeyLookupResult.Value.Guid)))
                            guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                        else
                            throw new KeyNotFoundException(string.Concat("LDM GUID not found, Entity: ‘INSTITUTIONS.ATTEND’, Record ID: '", splitKeys[1], "'"));
                    }
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex); ;
            }
            catch (Exception ex)
            {
                throw new RepositoryException("Error occurred while getting INSTITUTIONS.ATTEND guids.", ex); ;
            }

            return guidCollection;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetInstitutionsAttendIdFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new RepositoryException("InstitutionsAttend GUID is a required field.");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new RepositoryException("InstitutionsAttend GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new RepositoryException("InstitutionsAttend GUID " + guid + " lookup failed.");
            }

            if (foundEntry.Value.Entity != "INSTITUTIONS.ATTEND")
            {
                throw new RepositoryException("GUID " + guid + " has different entity, " + foundEntry.Value.Entity + ", than expected, INSTITUTIONS.ATTEND");
            }

            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single InstitutionsAttend domain entity from an id.
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>InstitutionsAttend domain entity object</returns>
        public async Task<Domain.Base.Entities.InstitutionsAttend> GetInstitutionAttendByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new RepositoryException("ID is required to get a InstitutionsAttend.");
            }

            // Now we have an ID, so we can read the record
            var record = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.InstitutionsAttend>(id);
            if (record == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or InstitutionsAttend with ID ", id, "invalid."));
            }

            var defaults = GetDefaults();
            if ((defaults != null) && (!string.IsNullOrEmpty(record.Recordkey)))
            {
                var splitKey = id.Split('*');
                if (splitKey.Count() != 2)
                {
                    throw new KeyNotFoundException(string.Concat("InstitutionAttend Record Key is not valid: ", id));
                }

                if (splitKey[1] == defaults.DefaultHostCorpId)
                {
                    throw new KeyNotFoundException("Record associated with home institution can not be returned.");
                }
            }

            Domain.Base.Entities.InstitutionsAttend institutionAttend = BuildInstitutionsAttend(record);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
                throw exception;

            return institutionAttend;
        }

        /// <summary>
        /// Get all InstitutionsAttend domain entity data 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="personId"></param>
        /// <param name="filterPersonIds"></param>
        /// <param name="personFilter"></param>
        /// <param name="personByInstitutionTypePersonId"></param>
        /// <param name="typeFilter"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Collection of InstitutionsAttend domain entities</returns>
        public async Task<Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>> GetInstitutionsAttendAsync(int offset, int limit, string personId = "",
            string[] filterPersonIds = null, string personFilter = "", string personByInstitutionTypePersonId = "",
            InstType? typeFilter = null, bool bypassCache = false)
        {
            var criteria = string.Empty;
            var hostInstitutionId = string.Empty;
            List<string> institutionAttendLimitingKeys = null;
            var coreDefaultData = GetDefaults();
            if (coreDefaultData != null)
            {
                hostInstitutionId = coreDefaultData.DefaultHostCorpId;
            }

            #region personFilter named query
            if (filterPersonIds != null && filterPersonIds.Any())
            {
                try
                {
                    string[] limitingKeys = new string[] { };

                    string institutionAttendPersonFilterKey = CacheSupport.BuildCacheKey("InstitutionsAttendKeys", personFilter, filterPersonIds.Count());
                    if (offset == 0 && ContainsKey(BuildFullCacheKey(institutionAttendPersonFilterKey)))
                    {
                        ClearCache(new List<string> { institutionAttendPersonFilterKey });
                    }
                    institutionAttendLimitingKeys = new List<string>();
                    institutionAttendLimitingKeys = await GetOrAddToCacheAsync<List<string>>(institutionAttendPersonFilterKey,
                       async () =>
                       {
                           List<string> IdsFromStuFil = new List<string>();

                           for (var i = 0; i < filterPersonIds.Count(); i += readSize)
                           {
                               var personSubList = filterPersonIds.Skip(i).Take(readSize);
                               BatchColumnarReadOutput columnsOutput = await DataReader.BatchReadRecordColumnsWithInvalidKeysAsync("PERSON", personSubList.ToArray(), new string[] { "PERSON.INSTITUTIONS.ATTEND" });
                               if (columnsOutput.Equals(default(BatchColumnarReadOutput)))
                               {
                                   return institutionAttendLimitingKeys;
                               }
                               if ((columnsOutput.InvalidKeys != null) && (columnsOutput.InvalidKeys.Any()))
                               {
                                   exception.AddErrors(columnsOutput.InvalidKeys
                                       .Select(key => new RepositoryError("invalid.key",
                                       string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                                   return institutionAttendLimitingKeys;
                               }
                               var columns = columnsOutput.BatchColumnarRecordsRead;
                               foreach (KeyValuePair<string, Dictionary<string, string>> entry in columns)
                               {
                                   //The key will contain the personID
                                   //The Value will contain a keyValuePair consisting of the fieldname and value
                                   //  since the only field we are retrieving is the PERSON.INSTITUTIONS.ATTEND, we dont need to exclude any results from this collection,
                                   //  but since we also want to get rid of any empty records, we can remove those first.
                                   var validRecords = entry.Value.Where(v => v.Key.Equals("PERSON.INSTITUTIONS.ATTEND") && !string.IsNullOrEmpty(v.Value));

                                   if (validRecords != null && validRecords.Any())
                                   {
                                       foreach (KeyValuePair<string, string> institutitionsAttended in validRecords)
                                       {
                                           var studentId = entry.Key;
                                           var institutionsAttends = institutitionsAttended.Value.Split(DmiString._VM);
                                           foreach (var institutionsAttend in institutionsAttends)
                                           {
                                               if (!string.IsNullOrEmpty(institutionsAttend)) //&& (hostInstitutionId != institutionsAttend))
                                               {
                                                   IdsFromStuFil.Add(string.Concat(studentId, "*", institutionsAttend));
                                               }
                                           }
                                       }
                                   }
                               }
                           }

                           if (IdsFromStuFil != null && IdsFromStuFil.Any())
                               institutionAttendLimitingKeys.AddRange(IdsFromStuFil);

                           institutionAttendLimitingKeys.Sort();
                           return institutionAttendLimitingKeys;
                       }, 20);
                    if (institutionAttendLimitingKeys == null || !institutionAttendLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(new List<Domain.Base.Entities.InstitutionsAttend>(), 0);
                    }
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Bad.Data", ex.Message));
                    throw exception;
                }

                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
            }
            #endregion

            #region personByInstitutionType named query

            if ((!string.IsNullOrEmpty(personByInstitutionTypePersonId)) && (typeFilter != null))
            {
                // Select records in INSTITUTIONS.ATTEND where INSTA.PERSON.ID is equal to the person specified, 
                // INSTA.INSTITUTIONS.ID is not equal to DFLTS.HOST.CORP.ID (which is part of the GET ALL) 
                // and INSTA.INST.TYPE.ACTION1 is equal to the code that corresponds to the type enumeration specified.

                string instaInstTypeAction = string.Empty;
                if (typeFilter != null)
                {
                    switch (typeFilter)
                    {
                        case InstType.College:
                            instaInstTypeAction = "C";
                            break;
                        case InstType.HighSchool:
                            instaInstTypeAction = "H";
                            break;
                    }
                }
                if (!string.IsNullOrEmpty(hostInstitutionId))
                {
                    criteria += "WITH INSTA.INSTITUTIONS.ID NE '" + hostInstitutionId + "'";
                }

                if (!string.IsNullOrEmpty(personByInstitutionTypePersonId))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria += " AND ";
                    }

                    criteria += "WITH INSTA.PERSON.ID EQ '" + personByInstitutionTypePersonId + "'";
                }

                if (!string.IsNullOrEmpty(instaInstTypeAction))
                {
                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria += " AND ";
                    }

                    criteria += "WITH INSTA.INST.TYPE.ACTION1 EQ '" + instaInstTypeAction + "'";
                }

                institutionAttendLimitingKeys = (await DataReader.SelectAsync("INSTITUTIONS.ATTEND",
                        institutionAttendLimitingKeys != null && institutionAttendLimitingKeys.Any() ? institutionAttendLimitingKeys.ToArray() : null, criteria))
                        .ToList();

                if (institutionAttendLimitingKeys == null || !institutionAttendLimitingKeys.Any())
                {
                    return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(new List<Domain.Base.Entities.InstitutionsAttend>(), 0);
                }
            }
            #endregion

            #region criteria filter

            //Note: this should be called independently of the named queries
            criteria = string.Empty;

            if (!string.IsNullOrEmpty(hostInstitutionId))
            {
                criteria += "WITH INSTA.INSTITUTIONS.ID NE '" + hostInstitutionId + "'";
            }

            if (!string.IsNullOrEmpty(personId))
            {
                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }

                criteria += "WITH INSTA.PERSON.ID EQ '" + personId + "'";
            }
            #endregion

            var institutionAttends = await DataReader.SelectAsync("INSTITUTIONS.ATTEND",
                institutionAttendLimitingKeys != null && institutionAttendLimitingKeys.Any() ? institutionAttendLimitingKeys.ToArray() : null, criteria);

            //after applying initial criteria, need to get a collection of institution records that 
            //are  associated with an INSTITUTIONS record with special processing codes
            var types = (await this.GetInstitutionTypesAsync()).ValsEntityAssociation.Where(x => !string.IsNullOrEmpty(x.ValActionCode1AssocMember))
            .Select(z => z.ValInternalCodeAssocMember).ToArray();

            var institutionAttendCacheKey = CacheSupport.BuildCacheKey("InstitutionsAttendedKeys", personId, personFilter, personByInstitutionTypePersonId, typeFilter.ToString());
            if (offset == 0 && ContainsKey(BuildFullCacheKey(institutionAttendCacheKey)))
            {
                ClearCache(new List<string> { institutionAttendCacheKey });
            }
            string[] institutionAttendIds = new string[] { };

            institutionAttendIds = await GetOrAddToCacheAsync<string[]>(institutionAttendCacheKey,
            async () =>
            {
                var institutionIds = await DataReader.SelectAsync("INSTITUTIONS", "WITH INST.TYPE = '?'", types);
                institutionAttendIds =
                    institutionAttends.Join(
                        institutionIds,
                        l1 => l1.Split('*')[1], //Selector for items from the inner list splits on '*'
                        l2 => l2,               //Select the current item
                        (l1, l2) => l1).ToArray();
                Array.Sort(institutionAttendIds);
                return institutionAttendIds;
            }, 20);

            if ((institutionAttendIds == null || !institutionAttendIds.Any()))
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(new List<Domain.Base.Entities.InstitutionsAttend>(), 0);
            }

            var totalCount = institutionAttendIds.Count();
            var subList = institutionAttendIds.Skip(offset).Take(limit).ToArray();

            var institutionsAttendData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<Ellucian.Colleague.Data.Base.DataContracts.InstitutionsAttend>("INSTITUTIONS.ATTEND", subList);

            if (institutionsAttendData.Equals(default(BulkReadOutput<Ellucian.Colleague.Data.Base.DataContracts.InstitutionsAttend>)))
            {
                return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(new List<Domain.Base.Entities.InstitutionsAttend>(), 0);
            }
            if ((institutionsAttendData.InvalidKeys != null && institutionsAttendData.InvalidKeys.Any())
                || (institutionsAttendData.InvalidRecords != null && institutionsAttendData.InvalidRecords.Any()))
            {

                if (institutionsAttendData.InvalidKeys.Any())
                {
                    exception.AddErrors(institutionsAttendData.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (institutionsAttendData.InvalidRecords.Any())
                {
                    exception.AddErrors(institutionsAttendData.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
                throw exception;
            }

            var institutionAttendsEntities = BuildInstitutionsAttendedCollection(institutionsAttendData.BulkRecordsRead);
            return new Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>(institutionAttendsEntities, totalCount);
        }

        /// <summary>
        /// UpdateExternalEducationAsync
        /// </summary>
        /// <param name="personExternalEducationEntity"></param>
        /// <returns></returns>
        public async Task<Domain.Base.Entities.InstitutionsAttend> UpdateExternalEducationAsync(Domain.Base.Entities.InstitutionsAttend personExternalEducationEntity)
        {

            if (personExternalEducationEntity == null)
                return null;

            var extendedDataTuple = GetEthosExtendedDataLists();
            UpdateInstitutionsAttendRequest updateRequest = null;
            try
            {
                updateRequest = ConvertInstitutionsAttendEntityToUpdateInstitutionsAttendRequest(personExternalEducationEntity);
                if (updateRequest == null)
                {
                    var exception = new RepositoryException();
                    exception.AddError(new RepositoryError("Validation.Exception", "Unable to build transaction object"));
                    throw exception;
                }
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Validation.Exception", "Unable to build transaction object. " + ex.Message));
                throw exception;
            }

            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateInstitutionsAttendRequest, UpdateInstitutionsAttendResponse>(updateRequest);

            if (updateResponse.UpdateInstitutionsAttendErrors != null && updateResponse.UpdateInstitutionsAttendErrors.Any())
            {
                var exception = new RepositoryException();
                updateResponse.UpdateInstitutionsAttendErrors
                   .ForEach(e => exception.AddError(new RepositoryError("Validation.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))));
                throw exception;
            }

            var id = await GetInstitutionsAttendIdFromGuidAsync(updateResponse.Guid);
            return await GetInstitutionAttendByIdAsync(id);
        }

        /// <summary>
        /// Convert InstitutionsAttend domain entity to an UpdateInstitutionsAttendRequest
        /// </summary>
        /// <param name="personExternalEducationEntity"></param>
        /// <returns></returns>
        private UpdateInstitutionsAttendRequest ConvertInstitutionsAttendEntityToUpdateInstitutionsAttendRequest(Domain.Base.Entities.InstitutionsAttend personExternalEducationEntity)
        {

            if (personExternalEducationEntity == null)
                return null;

            var extendedDataTuple = GetEthosExtendedDataLists();
            var updateRequest = new UpdateInstitutionsAttendRequest();
            updateRequest.Guid = personExternalEducationEntity.Guid;
            updateRequest.InstitutionsId = personExternalEducationEntity.InstitutionId;
            updateRequest.PersonId = personExternalEducationEntity.PersonId;

            if (personExternalEducationEntity.RankPercent != null)
            {
                updateRequest.ClassPercentile = personExternalEducationEntity.RankPercent;
            }
            if (personExternalEducationEntity.RankNumerator != null)
            {
                updateRequest.ClassRank = personExternalEducationEntity.RankNumerator;
            }
            if (personExternalEducationEntity.RankDenominator != null)
            {
                updateRequest.ClassSize = personExternalEducationEntity.RankDenominator;
            }
            if (personExternalEducationEntity.InstaIntgHsGradDate != null && personExternalEducationEntity.InstaIntgHsGradDate != default(DateTime))
            {
                updateRequest.HsGradDate = personExternalEducationEntity.InstaIntgHsGradDate;
            }
            if (personExternalEducationEntity.ExtGpa != null)
            {
                updateRequest.PerformanceMeasure = personExternalEducationEntity.ExtGpa;
            }
            if (personExternalEducationEntity.ExtCredits != null)
            {
                updateRequest.TotalCredits = personExternalEducationEntity.ExtCredits;
            }

            if (personExternalEducationEntity.DatesAttended != null && personExternalEducationEntity.DatesAttended.Any())
            {
                var datesAttended = new List<DatesAttended>();
                foreach (var dates in personExternalEducationEntity.DatesAttended)
                {
                    DatesAttended datesAttend = new DatesAttended();
                    if (dates.Item1 != null && dates.Item1 != default(DateTime))
                        datesAttend.StartDates = dates.Item1;
                    if (dates.Item2 != null && dates.Item2 != default(DateTime))
                        datesAttend.EndDates = dates.Item2;

                    datesAttended.Add(datesAttend);
                }
                updateRequest.DatesAttended = datesAttended;
            }

            if (personExternalEducationEntity.YearsAttended != null && personExternalEducationEntity.YearsAttended.Any())
            {
                var yearsAttended = new List<YearsAttended>();

                foreach (var years in personExternalEducationEntity.YearsAttended)
                {
                    var yearsAttend = new YearsAttended();
                    if (years.Item1 != null && years.Item1 != 0)
                        yearsAttend.YearsAttendedStart = years.Item1;
                    if (years.Item2 != null && !string.IsNullOrEmpty(years.Item2))
                    {
                        int endDate = 0;
                        var parsed = Int32.TryParse(years.Item2, out endDate);
                        if (parsed && endDate != 0)
                        {
                            yearsAttend.YearsAttendedEnd = endDate;
                        }
                    }
                    yearsAttended.Add(yearsAttend);
                }

                updateRequest.YearsAttended = yearsAttended;
            }

            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                updateRequest.ExtendedNames = extendedDataTuple.Item1;
                updateRequest.ExtendedValues = extendedDataTuple.Item2;
            }

            return updateRequest;
        }

        /// <summary>
        /// Build InstitutionsAttended domain entity collection.
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private IEnumerable<Domain.Base.Entities.InstitutionsAttend> BuildInstitutionsAttendedCollection(Collection<Ellucian.Colleague.Data.Base.DataContracts.InstitutionsAttend> sources)
        {
            var institutionsAttendCollection = new List<Domain.Base.Entities.InstitutionsAttend>();
            foreach (var source in sources)
            {
                institutionsAttendCollection.Add(BuildInstitutionsAttend(source));
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
                throw exception;

            return institutionsAttendCollection.AsEnumerable();
        }

        /// <summary>
        /// Build individual InstitutionsAttend domain entity
        /// </summary>
        /// <param name="institutionsAttendData"></param>
        /// <returns></returns>
        private Domain.Base.Entities.InstitutionsAttend BuildInstitutionsAttend(Ellucian.Colleague.Data.Base.DataContracts.InstitutionsAttend institutionsAttendData)
        {
            if (institutionsAttendData == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "InstitutionsAttend is a required property."));
                return null;
            }
            Domain.Base.Entities.InstitutionsAttend institutionsAttend = null;

            try
            {
                institutionsAttend = new Domain.Base.Entities.InstitutionsAttend(institutionsAttendData.RecordGuid, institutionsAttendData.Recordkey)
                {
                    AcadCredentials = institutionsAttendData.InstaAcadCredentials,
                    Comments = institutionsAttendData.InstaComments,
                    ExtCredits = institutionsAttendData.InstaExtCredits,
                    ExtGpa = institutionsAttendData.InstaExtGpa,
                    GradType = institutionsAttendData.InstaGradType,
                    RankDenominator = institutionsAttendData.InstaRankDenominator,
                    RankNumerator = institutionsAttendData.InstaRankNumerator,
                    RankPercent = institutionsAttendData.InstaRankPercent,
                    TranscriptDate = institutionsAttendData.InstaTranscriptDate,
                    TranscriptStatus = institutionsAttendData.InstaTranscriptStatus,
                    TranscriptType = institutionsAttendData.InstaTranscriptType,
                    InstaIntgHsGradDate = institutionsAttendData.InstaIntgHsGradDate,
                };

                var datesAttended = new List<Tuple<DateTime?, DateTime?>>();

                if (institutionsAttendData.DatesAttendedEntityAssociation != null &&
                    institutionsAttendData.DatesAttendedEntityAssociation.Any())
                {
                    foreach (var dateAttended in institutionsAttendData.DatesAttendedEntityAssociation)
                    {
                        datesAttended.Add(new Tuple<DateTime?, DateTime?>(dateAttended.InstaStartDatesAssocMember, dateAttended.InstaEndDatesAssocMember));
                    }

                    if (datesAttended.Any())
                    {
                        institutionsAttend.DatesAttended = datesAttended;
                    }
                }

                var yearsAttended = new List<Tuple<int?, string>>();

                if (institutionsAttendData.YearsAttendedEntityAssociation != null &&
                    institutionsAttendData.YearsAttendedEntityAssociation.Any())
                {
                    foreach (var yearAttended in institutionsAttendData.YearsAttendedEntityAssociation)
                    {
                        yearsAttended.Add(new Tuple<int?, string>(yearAttended.InstaYearAttendStartAssocMember, yearAttended.InstaYearAttendEndAssocMember));
                    }

                    if (yearsAttended.Any())
                    {
                        institutionsAttend.YearsAttended = yearsAttended;
                    }
                }
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data", ex.Message)
                {
                    Id = string.IsNullOrEmpty(institutionsAttendData.RecordGuid) ? "" : institutionsAttendData.RecordGuid,
                    SourceId = string.IsNullOrEmpty(institutionsAttendData.Recordkey) ? "" : institutionsAttendData.Recordkey
                });
            }

            return institutionsAttend;
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private Base.DataContracts.Defaults GetDefaults()
        {
            return GetOrAddToCache<Data.Base.DataContracts.Defaults>("CoreDefaults",
            () =>
            {
                Data.Base.DataContracts.Defaults coreDefaults = DataReader.ReadRecord<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                if (coreDefaults == null)
                {
                    logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                    coreDefaults = new Defaults();
                }
                return coreDefaults;
            }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Return the Validation Table InstTypes for determination of High School or College
        /// within the Institutions Attended data.
        /// </summary>
        /// <returns>Validation Table Object for Institution Types</returns>
        private async Task<ApplValcodes> GetInstitutionTypesAsync()
        {
            if (_institutionTypes != null)
            {
                return _institutionTypes;
            }

            _institutionTypes = await GetOrAddToCacheAsync<ApplValcodes>("InstitutionTypes",
            async () =>
            {
                ApplValcodes typesTable = await DataReader.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INST.TYPES");
                if (typesTable == null)
                {
                    var errorMessage = "Unable to access INST.TYPES valcode table.";
                    logger.Info(errorMessage);
                    throw new ColleagueWebApiException(errorMessage);
                }
                return typesTable;
            }, Level1CacheTimeoutValue);
            return _institutionTypes;
        }
    }
}
