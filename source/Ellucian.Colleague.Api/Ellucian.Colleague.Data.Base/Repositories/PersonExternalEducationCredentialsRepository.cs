/*Copyright 2019-2023 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class PersonExternalEducationCredentialsRepository : BaseColleagueRepository, IPersonExternalEducationCredentialsRepository
    {
        private RepositoryException exception = new RepositoryException();
        private readonly int _readSize;

        public PersonExternalEducationCredentialsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
            this._readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        ///  Get all external education data consisting of academic credential data, excluding home insitution,
        /// along with some data from institutions attended
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="filterPersonIds"></param>
        /// <param name="personFilter"></param>
        /// <param name="personId"></param>
        /// <param name="externalEducationID"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Collection of ExternalEducation domain entities</returns>
        public async Task<Tuple<IEnumerable<ExternalEducation>, int>> GetExternalEducationCredentialsAsync(int offset, int limit,
            string[] filterPersonIds = null, string personFilter = "", string personId = "", string externalEducationID = "", bool bypassCache = false)
        {
            var criteria = string.Empty;
            var coreDefaultData = await GetDefaults();
            List<string> acadCredentialsLimitingKeys = null;
            string[] limitingKeys = new string[] { };
            int totalCount = 0;

            var hostInstitutionId = coreDefaultData.DefaultHostCorpId;

            if (!(string.IsNullOrEmpty(hostInstitutionId)))
            {
                criteria += "WITH ACAD.DEGREE OR WITH ACAD.CCD AND WITH ACAD.INSTITUTIONS.ID NE '" + hostInstitutionId + "'";
            }

            #region  Named query person filter
            if (filterPersonIds != null && filterPersonIds.Any())
            {
                try
                {
                    string institutionAttendPersonFilterKey = CacheSupport.BuildCacheKey("ExternalEducationPersonFilterKeys", personFilter,
                       filterPersonIds.Count().ToString());
                    if (offset == 0 && ContainsKey(BuildFullCacheKey(institutionAttendPersonFilterKey)))
                    {
                        ClearCache(new List<string> { institutionAttendPersonFilterKey });
                    }
                    acadCredentialsLimitingKeys = new List<string>();
                    acadCredentialsLimitingKeys = await GetOrAddToCacheAsync<List<string>>(institutionAttendPersonFilterKey,
                       async () =>
                       {
                           List<string> IdsFromStuFil = new List<string>();

                           var personIds = await DataReader.SelectAsync("PERSON", filterPersonIds.Distinct().ToArray(), "WITH PERSON.INSTITUTIONS.ATTEND NE ''");
                           if (personIds == null || !personIds.Any())
                           {
                               return new List<string>();
                           }
                           var columns = await DataReader.BatchReadRecordColumnsAsync("PERSON", personIds, new string[] { "PERSON.INSTITUTIONS.ATTEND" });

                           foreach (KeyValuePair<string, Dictionary<string, string>> entry in columns)
                           {
                               var studentId = entry.Key;
                               foreach (KeyValuePair<string, string> institutitionsAttended in entry.Value)
                               {
                                   var institutionsAttends = institutitionsAttended.Value.Split(DmiString._VM);
                                   foreach (var institutionsAttend in institutionsAttends)
                                   {
                                       if (!string.IsNullOrEmpty(institutionsAttend) && (hostInstitutionId != institutionsAttend))
                                       {
                                           IdsFromStuFil.Add(string.Concat(studentId, "*", institutionsAttend));
                                       }
                                   }
                               }
                           }

                           if (IdsFromStuFil != null && IdsFromStuFil.Any())
                           {
                               List<string> acadCredIds = new List<string>();

                               var instAttendIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", IdsFromStuFil.Distinct().ToArray(), "WITH INSTA.ACAD.CREDENTIALS NE ''");

                               if (instAttendIds != null && instAttendIds.Any())
                               {
                                   columns = await DataReader.BatchReadRecordColumnsAsync("INSTITUTIONS.ATTEND", instAttendIds, new string[] { "INSTA.ACAD.CREDENTIALS" });

                                   foreach (KeyValuePair<string, Dictionary<string, string>> entry in columns)
                                   {
                                       var instAttendId = entry.Key;
                                       foreach (KeyValuePair<string, string> institutitionsAttended in entry.Value)
                                       {
                                           var acadCredentials = institutitionsAttended.Value.Split(DmiString._VM);
                                           foreach (var acadCredKey in acadCredentials)
                                           {
                                               if (!string.IsNullOrEmpty(acadCredKey))
                                               {
                                                   acadCredIds.Add(acadCredKey);
                                               }
                                           }
                                       }
                                   }

                                   acadCredentialsLimitingKeys.AddRange(acadCredIds);
                               }
                           }
                           if (acadCredentialsLimitingKeys.Any())
                           {
                               acadCredentialsLimitingKeys.Sort();
                           }
                           return acadCredentialsLimitingKeys;
                       }, 20);

                    if (acadCredentialsLimitingKeys == null || !acadCredentialsLimitingKeys.Any())
                    {
                        return new Tuple<IEnumerable<ExternalEducation>, int>(new List<ExternalEducation>(), 0);
                    }
                    limitingKeys = acadCredentialsLimitingKeys.Distinct().ToArray();
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Validation.Exception", ex.Message));
                    throw exception;
                }

            }

            #endregion

            #region externalEducation filter
            //WITH ACAD.PERSON.ID EQ 'the first part of the key' AND ACAD.INSTITUTION.ID EQ 'the second part of the key'
            if (!string.IsNullOrEmpty(externalEducationID))
            {
                var externalEducation = externalEducationID.Split('*');
                if (externalEducation != null && externalEducation.Count() > 1)
                {
                    var externalEducationPersonId = externalEducation[0];
                    var externalEducationInstitutionId = externalEducation[1];

                    if (!string.IsNullOrEmpty(criteria))
                    {
                        criteria += " AND ";
                    }
                    criteria += string.Format("WITH ACAD.PERSON.ID EQ '{0}' AND ACAD.INSTITUTIONS.ID EQ '{1}'", externalEducationPersonId, externalEducationInstitutionId);
                }
            }
            #endregion

            #region personId filter

            if (!string.IsNullOrEmpty(personId))
            {

                if (!string.IsNullOrEmpty(criteria))
                {
                    criteria += " AND ";
                }
                criteria += string.Format("WITH ACAD.PERSON.ID EQ '{0}'", personId);
            }

            #endregion

            string personExternalEducationCredentialsKey = CacheSupport.BuildCacheKey("PersonExternalEducationCredentialsKeys", personFilter,
               filterPersonIds != null && filterPersonIds.Any() ? filterPersonIds.Count().ToString() : "", personId, externalEducationID);

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                personExternalEducationCredentialsKey,
                "ACAD.CREDENTIALS",
                offset,
                limit,
                20,
                async () =>
                {
                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys != null && limitingKeys.Any() ? limitingKeys.Distinct().ToList() : null,
                        criteria = criteria
                    };
                    return requirements;
                }
            );

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<ExternalEducation>, int>(new List<ExternalEducation>(), 0);
            }
            var subList = keyCacheObject.Sublist.ToArray();
            totalCount = keyCacheObject.TotalCount.Value;

            //var externalEducationData = await DataReader.BulkReadRecordAsync<AcadCredentials>("ACAD.CREDENTIALS", subList);
            var acadCredentialData = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.AcadCredentials>("ACAD.CREDENTIALS", subList);

            if (acadCredentialData.Equals(default(BulkReadOutput<DataContracts.AcadCredentials>)))
            {
                return new Tuple<IEnumerable<ExternalEducation>, int>(new List<ExternalEducation>(), 0);
            }
            if ((acadCredentialData.InvalidKeys != null && acadCredentialData.InvalidKeys.Any())
                    || (acadCredentialData.InvalidRecords != null && acadCredentialData.InvalidRecords.Any()))
            {
                var repositoryException = new RepositoryException();

                if (acadCredentialData.InvalidKeys.Any())
                {
                    repositoryException.AddErrors(acadCredentialData.InvalidKeys
                        .Select(key => new RepositoryError("invalid.key",
                        string.Format("Unable to locate the following key '{0}'.", key.ToString()))));
                }
                if (acadCredentialData.InvalidRecords.Any())
                {
                    repositoryException.AddErrors(acadCredentialData.InvalidRecords
                       .Select(r => new RepositoryError("invalid.record",
                       string.Format("Error: '{0}' ", r.Value))
                       { SourceId = r.Key }));
                }
                throw repositoryException;
            }
            var externalEducationData = acadCredentialData.BulkRecordsRead;

            if (externalEducationData == null)
            {
                return new Tuple<IEnumerable<ExternalEducation>, int>(new List<ExternalEducation>(), 0);
            }
            var institutionsAttendKeys = externalEducationData.Select(ee => string.Concat(ee.AcadPersonId, "*", ee.AcadInstitutionsId));
            if (institutionsAttendKeys == null || !institutionsAttendKeys.Any())
            {
                return new Tuple<IEnumerable<ExternalEducation>, int>(new List<ExternalEducation>(), 0);
            }
            var institutionsAttendData = await DataReader.BulkReadRecordAsync<Data.Base.DataContracts.InstitutionsAttend>("INSTITUTIONS.ATTEND", institutionsAttendKeys.ToArray());

            var externalEducationEntities = await BuildExternalEducationsCredentialsAsync(subList, externalEducationData, institutionsAttendData);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return new Tuple<IEnumerable<ExternalEducation>, int>(externalEducationEntities, totalCount);
        }

        /// <summary>
        /// Get a single External Education domain entity from an academic credential guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<ExternalEducation> GetExternalEducationCredentialsByGuidAsync(string guid)
        {
            return await GetExternalEducationCredentialsByIdAsync(await GetExternalEducationCredentialsIdFromGuidAsync(guid));
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetExternalEducationCredentialsIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("ACAD.CREDENTIALS GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value.Entity != "ACAD.CREDENTIALS" || string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", "The GUID specified: " + guid + " is used by a different entity/secondary key: " + foundEntry.Value.Entity)
                {
                    Id = guid
                });
                throw exception;
            }
            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        public async Task<string> GetExternalEducationIdFromGuidAsync(string guid)
        {

            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }
            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("INSTITUTIONS.ATTEND GUID " + guid + " not found.");
            }
            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value.Entity != "INSTITUTIONS.ATTEND" || !string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                exception.AddError(new RepositoryError("GUID.Wrong.Type", "The GUID specified: " + guid + " is used by a different entity/secondary key: " + foundEntry.Value.Entity)
                {
                    Id = guid
                });
                throw exception;
            }
            return foundEntry.Value.PrimaryKey;
        }

        /// <summary>
        /// Get a single External Education domain entity from an academic credential id.
        /// </summary>
        /// <param name="id">The academic credential id</param>
        /// <returns>External Education domain entity object</returns>
        public async Task<ExternalEducation> GetExternalEducationCredentialsByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is required to get a AcademicCredentials.");
            }

            var acadCredential = await DataReader.ReadRecordAsync<AcadCredentials>(id);
            if (acadCredential == null)
            {
                throw new KeyNotFoundException(string.Concat("Record not found, or AcademicCredentials with ID ", id, "invalid."));
            }

            var dict = new Dictionary<string, string>
            {
                { id, acadCredential.AcadPersonId }
            };
            Dictionary<string, string> academicCredentialsDict = await GetGuidsCollectionAsync(dict);

            if (acadCredential != null && !(string.IsNullOrEmpty(acadCredential.AcadInstitutionsId)))
            {
                var coreDefaultData = await GetDefaults();
                if (coreDefaultData != null)
                {
                    var hostInstitutionId = coreDefaultData.DefaultHostCorpId;
                    if ((!string.IsNullOrEmpty(hostInstitutionId)) && (hostInstitutionId == acadCredential.AcadInstitutionsId))
                    {
                        throw new ArgumentException("id", "Record associated with home institution can not be returned.");

                    }
                }
            }

            var institutionsAttendKey = string.Concat(acadCredential.AcadPersonId, "*", acadCredential.AcadInstitutionsId);
            var instAttendData = await DataReader.ReadRecordAsync<DataContracts.InstitutionsAttend>(institutionsAttendKey);

            var academicCredentialsGuid = GetGuid(id, academicCredentialsDict);
            var externalEducation = BuildExternalEducationCredentialsAsync(acadCredential, instAttendData, academicCredentialsGuid);

            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return externalEducation;
        }

        #region Update/Create ExternalEducation

        public async Task<ExternalEducation> UpdateExternalEducationCredentialsAsync(ExternalEducation externalEducation)
        {
            if (externalEducation == null)
            {
                throw new ArgumentNullException("externalEducationEntity", "Must provide a externalEducation Entity to update.");
            }

            if (string.IsNullOrEmpty(externalEducation.Guid))
            {
                throw new ArgumentNullException("externalEducationEntity", "Must provide the guid of the externalEducation Entity to update.");
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            var externalEducationId = await GetExternalEducationCredentialsIdFromGuidAsync(externalEducation.Guid);

            if (externalEducationId != null && !string.IsNullOrEmpty(externalEducationId))
            {
                var updateRequest = BuildUpdateRequest(externalEducation);

                // write the  data
                var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(updateRequest);

                if (updateResponse.UpdateAcadCredentialsErrors.Any())
                {
                    var errorMessage = string.Format("Error(s) occurred updating person-external-education-credentials '{0}':", externalEducation.Guid);
                    var exception = new RepositoryException(errorMessage);
                    updateResponse.UpdateAcadCredentialsErrors.ForEach(e => exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                    {
                        Id = externalEducation.Guid,
                        SourceId = externalEducationId
                    }
                    ));
                    logger.Error(errorMessage);
                    throw exception;
                }
                // get the updated entity from the database
                return await GetExternalEducationCredentialsByGuidAsync(updateResponse.Guid);
            }

            // Perform a create instead
            return await CreateExternalEducationCredentialsAsync(externalEducation);
        }

        public async Task<ExternalEducation> CreateExternalEducationCredentialsAsync(ExternalEducation externalEducation)
        {
            if (externalEducation == null)
            {
                throw new ArgumentNullException("externalEducationEntity", "Must provide a externalEducation Entity to update.");
            }

            var updateRequest = BuildUpdateRequest(externalEducation);

            // write the  data
            var updateResponse = await transactionInvoker.ExecuteAsync<UpdateAcadCredentialsRequest, UpdateAcadCredentialsResponse>(updateRequest);

            if (updateResponse.UpdateAcadCredentialsErrors.Any())
            {
                var errorMessage = string.Format("Error(s) occurred updating person-external-education-credentials '{0}':", externalEducation.Guid);
                var exception = new RepositoryException(errorMessage);
                updateResponse.UpdateAcadCredentialsErrors.ForEach(e => exception.AddError(new RepositoryError("Create.Update.Exception", string.Concat(e.ErrorCodes, ": ", e.ErrorMessages))
                {
                    Id = externalEducation.Guid
                }
                ));
                logger.Error(errorMessage);
                throw exception;
            }
            // get the updated entity from the database
            return await GetExternalEducationCredentialsByGuidAsync(updateResponse.Guid);
        }

        /// <summary>
        /// Create an ApplicationSupportingItemRequest from a domain entity
        /// </summary>
        /// <param name="externalEducationEntity">supporting item domain entity</param>
        /// <returns>UpdateApplicationSupportingItemRequest transaction object</returns>
        private UpdateAcadCredentialsRequest BuildUpdateRequest(ExternalEducation externalEducationEntity)
        {
            var request = new UpdateAcadCredentialsRequest()
            {
                Guid = externalEducationEntity.Guid,
                CredentialsId = externalEducationEntity.Id,
                PersonId = externalEducationEntity.AcadPersonId,
                InstitutionsId = externalEducationEntity.AcadInstitutionsId,
                Degree = externalEducationEntity.AcadDegree,
                DegreeDate = externalEducationEntity.AcadDegreeDate,
                Ccd = externalEducationEntity.AcadCcd,
                CcdDate = externalEducationEntity.AcadCcdDate,
                Majors = externalEducationEntity.AcadMajors,
                Minors = externalEducationEntity.AcadMinors,
                Specialization = externalEducationEntity.AcadSpecialization,
                StartDate = externalEducationEntity.AcadStartDate,
                EndDate = externalEducationEntity.AcadEndDate,
                Gpa = externalEducationEntity.AcadGpa,
                Honors = externalEducationEntity.AcadHonors,
                RankDenominator = externalEducationEntity.AcadRankDenominator,
                RankNumerator = externalEducationEntity.AcadRankNumerator,
                RankPercent = externalEducationEntity.AcadRankPercent,
                Thesis = externalEducationEntity.AcadThesis
            };

            var extendedDataTuple = GetEthosExtendedDataLists();
            if (extendedDataTuple != null && extendedDataTuple.Item1 != null && extendedDataTuple.Item2 != null)
            {
                request.ExtendedNames = extendedDataTuple.Item1;
                request.ExtendedValues = extendedDataTuple.Item2;
            }

            return request;
        }

        #endregion

        private async Task<IEnumerable<ExternalEducation>> BuildExternalEducationsCredentialsAsync(string[] academicCredentialsIds, Collection<AcadCredentials> sources, Collection<Data.Base.DataContracts.InstitutionsAttend> institutionsAttendData)
        {
            var acadCredentialCollection = new List<ExternalEducation>();
            if (academicCredentialsIds != null && academicCredentialsIds.Any())
            {
                var dict = new Dictionary<string, string>();
                foreach (var source in sources)
                {
                    if (!dict.ContainsKey(source.Recordkey))
                    {
                        dict.Add(source.Recordkey, source.AcadPersonId);
                    }
                }
                var academicCredentialsDict = await GetGuidsCollectionAsync(dict);

                foreach (var source in sources)
                {
                    var academicCredentialsGuid = GetGuid(source.Recordkey, academicCredentialsDict);
                    var instAttendData = institutionsAttendData.Where(ia => ia.Recordkey == string.Concat(source.AcadPersonId, "*", source.AcadInstitutionsId)).FirstOrDefault();
                    acadCredentialCollection.Add(BuildExternalEducationCredentialsAsync(source, instAttendData, academicCredentialsGuid));
                }
            }
            return acadCredentialCollection.AsEnumerable();
        }

        private ExternalEducation BuildExternalEducationCredentialsAsync(AcadCredentials source, DataContracts.InstitutionsAttend instAttendData, string guid)
        {
            if (source == null)
            {
                exception.AddError(new RepositoryError("Bad.Data", "Selected, but can not read ACAD.CREDENTIALS record."));
                return null;
            }

            ExternalEducation externalEducation = null;

            try
            {
                externalEducation = new ExternalEducation(source.Recordkey, guid)
                {
                    AcadPersonId = source.AcadPersonId,
                    AcadInstitutionsId = source.AcadInstitutionsId,
                    AcadAcadProgram = source.AcadAcadProgram,
                    AcadDegree = source.AcadDegree,
                    AcadMajors = source.AcadMajors,
                    AcadMinors = source.AcadMinors,
                    AcadSpecialization = source.AcadSpecialization,
                    AcadStartDate = source.AcadStartDate,
                    AcadEndDate = source.AcadEndDate,
                    AcadGpa = source.AcadGpa,
                    AcadHonors = source.AcadHonors,
                    AcadAwards = source.AcadAwards,
                    AcadCommencementDate = source.AcadCommencementDate,
                    AcadComments = source.AcadComments,
                    AcadNoYears = source.AcadNoYears,
                    AcadDegreeDate = source.AcadDegreeDate,
                    AcadThesis = source.AcadThesis,
                    AcadRankDenominator = source.AcadRankDenominator,
                    AcadRankPercent = source.AcadRankPercent,
                    AcadRankNumerator = source.AcadRankNumerator,
                    AcadCcd = source.AcadCcd,
                    AcadCcdDate = source.AcadCcdDate
                };

                if (instAttendData != null && !string.IsNullOrEmpty(source.AcadPersonId) && !string.IsNullOrEmpty(source.AcadInstitutionsId))
                {
                    externalEducation.InstExtCredits = instAttendData.InstaExtCredits;
                    externalEducation.InstTransciptDate = instAttendData.InstaTranscriptDate;
                    externalEducation.InstAttendGuid = instAttendData.RecordGuid;
                }
                else
                {
                    exception.AddError(new RepositoryError("Missing.Required.Property", "Invalid ACAD.CREDENTIALS record.  Missing either person or institution reference.")
                    {
                        Id = guid,
                        SourceId = source.Recordkey
                    });
                }
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Validation.Exception", ex.Message));
            }

            return externalEducation;
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private async Task<Base.DataContracts.Defaults> GetDefaults()
        {
            return await GetOrAddToCacheAsync<Data.Base.DataContracts.Defaults>("CoreDefaultsPersonExternalEducationCredentials",
                async () =>
                {
                    var coreDefaults = await DataReader.ReadRecordAsync<Data.Base.DataContracts.Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        /// <summary>
        /// Using a collection of ids with guids
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a application.id with guids</returns>
        private async Task<Dictionary<string, string>> GetGuidsCollectionAsync(Dictionary<string, string> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {
                var guidLookup = ids
                    .Where(s => !string.IsNullOrWhiteSpace(s.Key) && !string.IsNullOrWhiteSpace(s.Value))
                    .Distinct().ToList()
                    .ConvertAll(key => new RecordKeyLookup("ACAD.CREDENTIALS", key.Key,
                    "ACAD.PERSON.ID", key.Value, false))
                    .ToArray();

                var recordKeyLookupResults = await DataReader.SelectAsync(guidLookup);

                if ((recordKeyLookupResults != null) && (recordKeyLookupResults.Any()))
                {
                    foreach (var recordKeyLookupResult in recordKeyLookupResults)
                    {
                        if (recordKeyLookupResult.Value != null)
                        {
                            var splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                            if (!guidCollection.ContainsKey(splitKeys[1]))
                            {
                                guidCollection.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("GUID.Not.Found", string.Format("Error occured while getting guids for {0}.  {1}", "ACAD.CREDENTIALS", ex.Message)));
            }

            return guidCollection;
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
    }
}