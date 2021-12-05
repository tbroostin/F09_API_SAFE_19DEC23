// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType]
    public class StudentAcademicCredentialsRepository : BaseColleagueRepository, IStudentAcademicCredentialsRepository
    {
        RepositoryException exception = new RepositoryException();
        const string AllStudentAcademicCredentialsCache = "AllStudentAcademicCredentials:";

        const int AllStudentAcademicCredentialsCacheTimeout = 20; // Clear from cache every 20 minutes
        public StudentAcademicCredentialsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET, GET By ID

        /// <summary>
        /// Gets student academic credentials.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="criteriaEntity"></param>
        /// <param name="filterPersonIds"></param>
        /// <param name="acadProgramFilter"></param>
        /// <param name="filterQualifiers"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<StudentAcademicCredential>, int>> GetStudentAcademicCredentialsAsync(int offset, int limit, StudentAcademicCredential criteriaEntity, string[] filterPersonIds, 
            string acadProgramFilter, Dictionary<string, string> filterQualifiers)
        {
            List<StudentAcademicCredential> entities = new List<StudentAcademicCredential>();
            int totalCount= 0;
            List<string> limitingKeys = new List<string> { };
            List<string> acadCredIds = new List<string>();

            //default host corp id.
            var defaults = await GetDefaultsAsync();
            string defaultHostCorpId = defaults.DefaultHostCorpId;

            //main criteria.
            string criteria = string.Empty;
            //common criteria with default host corp id.
            string commonCriteria = string.Format("WITH ACAD.INSTITUTIONS.ID EQ '{0}'", defaultHostCorpId);

            string allStudentAcademicCredentialsKey = CacheSupport.BuildCacheKey(AllStudentAcademicCredentialsCache, criteriaEntity.StudentId, criteriaEntity.Degrees, criteriaEntity.Ccds,
                criteriaEntity.GraduatedOn, criteriaEntity.AcademicLevel, criteriaEntity.AcadAcadProgramId, criteriaEntity.AcademicPeriod, acadProgramFilter, filterPersonIds, filterQualifiers
                );

            string[] subList = null;

            var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(

                        this,
                        ContainsKey,
                        GetOrAddToCacheAsync,
                        AddOrUpdateCacheAsync,
                        transactionInvoker,
                        allStudentAcademicCredentialsKey,
                        "ACAD.CREDENTIALS",
                        offset,
                        limit,
                        AllStudentAcademicCredentialsCacheTimeout,

                async () => {
                    //var acadCredentialsIds = new List<string> { };
                    //Student filter, ACAD.PERSON.ID indexed on UTBI for ACAD.CREDENTIALS
                    if (criteriaEntity != null && !string.IsNullOrEmpty(criteriaEntity.StudentId))
                    {
                        criteria = string.Format("ACAD.PERSON.ID EQ '{0}'", criteriaEntity.StudentId);
                    }

                    //credentials Degrees
                    if(criteriaEntity != null && criteriaEntity.Degrees != null && criteriaEntity.Degrees.Any())
                    {
                        var degreeIds = string.Join(" ", criteriaEntity.Degrees.Select(a => string.Format("'{0}'", a.Item1)));
                        criteria = string.IsNullOrEmpty(criteria)? string.Format("ACAD.DEGREE EQ {0}", degreeIds) :
                                                                   string.Format("{0} AND ACAD.DEGREE EQ {1}", criteria, degreeIds);
                    }
                    //Certificates
                    if (criteriaEntity != null && criteriaEntity.Ccds != null && criteriaEntity.Ccds.Any())
                    {
                        var certIds = string.Join(" ", criteriaEntity.Ccds.Select(a => string.Format("'{0}'", a.Item1)));
                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.CCD EQ {0}", certIds) :
                                                                    string.Format("{0} AND ACAD.CCD EQ {1}", criteria, certIds);
                    }

                    //graduatedOn
                    if(criteriaEntity.GraduatedOn.HasValue)
                    {
                        string convertedGadDate = await GetUnidataFormatDateAsync(criteriaEntity.GraduatedOn.Value);
                        var dateFilterOperation = string.Empty;
                        dateFilterOperation = filterQualifiers != null && filterQualifiers.ContainsKey("GraduatedOn") ? filterQualifiers["GraduatedOn"] : "EQ";
                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.END.DATE {0} '{1}'", dateFilterOperation, convertedGadDate) :
                                                                    string.Format("{0} AND ACAD.END.DATE {1} '{2}'", criteria, dateFilterOperation, convertedGadDate);
                    }

                    //academicLevel
                    if(!string.IsNullOrEmpty(criteriaEntity.AcademicLevel))
                    {
                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.ACAD.LEVEL EQ '{0}'", criteriaEntity.AcademicLevel) :
                                                                    string.Format("{0} AND ACAD.ACAD.LEVEL EQ '{1}'", criteria, criteriaEntity.AcademicLevel);
                    }

                    //studentProgram
                    if (!string.IsNullOrEmpty(criteriaEntity.AcadAcadProgramId))
                    {
                        string tempProgCriteria = string.Empty;
                        if (string.IsNullOrEmpty(criteriaEntity.StudentId))
                        {
                            tempProgCriteria = string.Format("AND ACAD.PERSON.ID EQ '{0}'", criteriaEntity.StudentId);
                            limitingKeys.Add(criteriaEntity.StudentId);
                        }
                        else
                        {
                            tempProgCriteria = string.Empty;
                        }

                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.ACAD.PROGRAM EQ '{0}' {1}", criteriaEntity.AcadAcadProgramId, tempProgCriteria) :
                                                                    string.Format("{0} AND ACAD.ACAD.PROGRAM EQ '{1}' {2}", criteria, criteriaEntity.AcadAcadProgramId, tempProgCriteria);
                    }
                    //graduationAcademicPeriod
                    if(!string.IsNullOrEmpty(criteriaEntity.AcademicPeriod))
                    {
                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.TERM EQ '{0}'", criteriaEntity.AcademicPeriod) :
                                                                    string.Format("{0} AND ACAD.TERM EQ '{1}'", criteria, criteriaEntity.AcademicPeriod);
                    }

                    //academicPrograms
                    if (!string.IsNullOrEmpty(acadProgramFilter))
                    {
                        criteria = string.IsNullOrEmpty(criteria) ? string.Format("ACAD.ACAD.PROGRAM EQ '{0}'", acadProgramFilter) :
                                                                    string.Format("{0} AND ACAD.ACAD.PROGRAM EQ '{1}'", criteria, acadProgramFilter);
                    }
                    
                    //Named query person filter
                    if (filterPersonIds != null && filterPersonIds.Any())
                    {
                        // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                        var keys = filterPersonIds.Select(a => string.Format("{0}*{1}", a, defaultHostCorpId)).Distinct().ToArray();
                        criteria = "WITH INSTA.ACAD.CREDENTIALS BY.EXP INSTA.ACAD.CREDENTIALS SAVING INSTA.ACAD.CREDENTIALS";
                        var credIds = await DataReader.SelectAsync("INSTITUTIONS.ATTEND", keys, criteria);
                        if (credIds == null || !credIds.Any())
                        {
                            return new CacheSupport.KeyCacheRequirements()
                            {
                                NoQualifyingRecords = true
                            };
                        }
                        limitingKeys.AddRange(credIds);
                        criteria = string.Empty;
                    }

                    criteria = string.IsNullOrEmpty(criteria) ? commonCriteria : string.Format("{0} AND {1}", commonCriteria, criteria);
                    
                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = limitingKeys.Distinct().ToList(),
                        criteria = criteria.ToString(),
                    };

                    return requirements;
                });

            if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
            {
                return new Tuple<IEnumerable<StudentAcademicCredential>, int>(new List<StudentAcademicCredential>(), 0);
            }

            subList = keyCache.Sublist.ToArray();
            totalCount = keyCache.TotalCount.Value;

            //Get guids dictionary with secondarykey & index
            var acadCredDC = await DataReader.BulkReadRecordAsync<AcadCredentials>(subList);
            //var stAcadCredIds = acadCredDC.Select(ac => ac.Recordkey);

            var dictAcadCreds = new Dictionary<string, string>();
            foreach (var source in acadCredDC)
            {
                if (!dictAcadCreds.ContainsKey(source.Recordkey))
                {
                    dictAcadCreds.Add(source.Recordkey, source.AcadPersonId);
                }
            }

            Dictionary<string, string> dict = null;
            try
            {
                dict = await GetStAcadCredDictionaryAsync(dictAcadCreds);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("data.access", ex.Message));
                exception.AddError(new RepositoryError("data.access", "Guids not found for ACAD.CREDENTIALS with ACAD.PERSON.ID."));
                throw exception;
            }

            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("data.access", "Guids not found for ACAD.CREDENTIALS with ACAD.PERSON.ID."));
                throw exception;
            }
            
            //Get student program guids
            var stProgIds = acadCredDC.Where(rec => !string.IsNullOrWhiteSpace(rec.AcadAcadProgram))
                                      .Select(i => string.Join("*", i.AcadPersonId, i.AcadAcadProgram)).ToList();
            var stProgDCs = await DataReader.BulkReadRecordAsync<StudentPrograms>(stProgIds.Distinct().ToArray());

            foreach (var dc in acadCredDC)
            {
                try
                {
                    StudentAcademicCredential entity = BuildStudentAcadCredentials(dc, dict, stProgDCs != null && stProgDCs.Any() ? stProgDCs : null);
                    entities.Add(entity);
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("data.access", string.Format("ACAD.CREDENTIALS error occurred found for key '{0}'. {1}", dc.Recordkey, ex.Message))
                    { SourceId = dc.Recordkey });
                }
            }

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }

            return entities != null ? new Tuple<IEnumerable<StudentAcademicCredential>, int>(entities, totalCount) :
                new Tuple<IEnumerable<StudentAcademicCredential>, int>(new List<StudentAcademicCredential>(), 0);
        }


        /// <summary>
        /// Gets student academic credentials by guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<StudentAcademicCredential> GetStudentAcademicCredentialByGuidAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key is required.");
            }

            StudentAcademicCredential entity = new StudentAcademicCredential();

            AcadCredentials dc = null;
            try
            {
                dc = await DataReader.ReadRecordAsync<AcadCredentials>(key);
                if (dc == null)
                {
                    throw new KeyNotFoundException(string.Format("Student academic credential not found for '{0}'", key));
                }
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("Student academic credential not found for '{0}'", key));
            }

            var defaults = await GetDefaultsAsync();
            string defaultHostCortId = defaults.DefaultHostCorpId;

            if(!dc.AcadInstitutionsId.Equals(defaultHostCortId, StringComparison.OrdinalIgnoreCase))
            {
                throw new KeyNotFoundException(string.Format("Student academic credential not found for '{0}'", key));
            }

            Dictionary<string, string> dict = null;
            try
            {
                var acadCredDict = new Dictionary<string, string>();
                acadCredDict.Add(dc.Recordkey, dc.AcadPersonId);
                dict = await GetStAcadCredDictionaryAsync(acadCredDict);
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("data.access", ex.Message));
                exception.AddError(new RepositoryError("data.access", "Guids not found for ACAD.CREDENTIALS with ACAD.PERSON.ID."));
                throw exception;
            }

            if (dict == null || !dict.Any())
            {
                exception.AddError(new RepositoryError("data.access", "Guids not found for ACAD.CREDENTIALS with ACAD.PERSON.ID."));
                throw exception;
            }

            //Get student program guids
            var stProgId = string.Empty;
            StudentPrograms stProgDC = null;
            if (!string.IsNullOrWhiteSpace(dc.AcadAcadProgram))
            {
                stProgId = string.Join("*", dc.AcadPersonId, dc.AcadAcadProgram);
                stProgDC = await DataReader.ReadRecordAsync<StudentPrograms>(stProgId);
            }

            entity = BuildStudentAcadCredentials(dc, dict, stProgDC != null ? new List<StudentPrograms>() { stProgDC } : null);

            if (exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return entity;
        }

        #endregion

        #region Helper Methods

        private StudentAcademicCredential BuildStudentAcadCredentials(AcadCredentials source, Dictionary<string, string> dict, IEnumerable<StudentPrograms> stProgDC)
        {
            string guid = string.Empty;
            if(!dict.TryGetValue(source.Recordkey, out guid))
            {
                exception.AddError(new RepositoryError("data.access", string.Format("Guids not found for ACAD.CREDENTIALS for key '{0}'.", source.Recordkey)));
            }

            StudentAcademicCredential entity = new StudentAcademicCredential(guid, source.Recordkey);

            //student.id
            if(string.IsNullOrEmpty(source.AcadPersonId))
            {
                exception.AddError(new RepositoryError("data.access", string.Format("Student not found for key '{0}'.", source.Recordkey)));
            }
            entity.AcadPersonId = source.AcadPersonId;

            //studentProgram.id
            var stProgId = string.Join("*", source.AcadPersonId, source.AcadAcadProgram);
            if (stProgDC != null && stProgDC.Any())
            {
                var stPro = stProgDC.FirstOrDefault(i => i.Recordkey.Equals(stProgId, StringComparison.OrdinalIgnoreCase));
                if (stPro != null)
                {
                    entity.StudentProgramGuid = stPro.RecordGuid;
                }
            }

            //academicLevel.id
            entity.AcademicLevel = string.IsNullOrEmpty(source.AcadAcadLevel) ? string.Empty : source.AcadAcadLevel;

            //credentials.credential.id degrees
            List<Tuple<string, DateTime?>> degreeTuples = new List<Tuple<string, DateTime?>>();
            if(!string.IsNullOrEmpty(source.AcadDegree))
            {
                var degreeDate = source.AcadDegreeDate.HasValue ? source.AcadDegreeDate.Value : default(DateTime?);
                Tuple<string, DateTime?> degreeTuple = new Tuple<string, DateTime?>(source.AcadDegree, degreeDate);
                degreeTuples.Add(degreeTuple);
            }
            entity.Degrees = degreeTuples.Any() ? degreeTuples : null;

            //credentials.credential.id ccds
            List<Tuple<string, DateTime?>> ccdsTuples = new List<Tuple<string, DateTime?>>();
            if (source.AcadCcd != null && source.AcadCcd.Any())
            {
                int total = source.AcadCcd.Count();
                for (int i = 0; i < total; i++)
                {
                    DateTime? ccdDate = default(DateTime?);
                    if(source.AcadCcdDate != null && source.AcadCcdDate.Any())
                    {
                        //Since there is no association returned you have to base it on order #
                        try
                        {
                            ccdDate = source.AcadCcdDate[i];
                        }
                        catch
                        {
                            //Since there is no association returned ignore error
                        }
                    }
                    Tuple<string, DateTime?> ccdTuple = new Tuple<string, DateTime?>(source.AcadCcd[i], ccdDate);
                    ccdsTuples.Add(ccdTuple);
                }
                entity.Ccds = ccdsTuples.Any() ? ccdsTuples : null;
            }

            //disciplines.id
            List<string> disciplines = new List<string>();
            if(source.AcadMajors != null && source.AcadMajors.Any())
            {
                disciplines.AddRange(source.AcadMajors);
                entity.AcadMajors = source.AcadMajors;
            }

            if (source.AcadMinors != null && source.AcadMinors.Any())
            {
                disciplines.AddRange(source.AcadMinors);
                entity.AcadMinors = source.AcadMinors;
            }

            if (source.AcadSpecialization != null && source.AcadSpecialization.Any())
            {
                disciplines.AddRange(source.AcadSpecialization);
                entity.AcadSpecializations = source.AcadSpecialization;
            }

            if(disciplines.Any())
            {
                entity.AcadDisciplines = disciplines;
            }

            //recognitions.id
            if(source.AcadHonors != null && source.AcadHonors.Any())
            {
                entity.AcadHonors = source.AcadHonors;
            }

            //graduatedOn
            entity.GraduatedOn = source.AcadEndDate.HasValue ? source.AcadEndDate.Value : default(DateTime?);

            //thesisTitle
            entity.AcadThesis = string.IsNullOrEmpty(source.AcadThesis)? string.Empty : source.AcadThesis;

            //graduationAcademicPeriod.id
            entity.AcadTerm = string.IsNullOrEmpty(source.AcadTerm) ? string.Empty : source.AcadTerm;

            entity.AcadAcadProgramId = source.AcadAcadProgram;            

            return entity;
        }

        /// <summary>
        /// Gets tuple with entity, primary key & secondary key.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Tuple<string, string, string>> GetAcadCredentialKeyAsync(string guid)
        {
            var result = await GetRecordInfoFromGuidAsync(guid);
            return result == null? null : new Tuple<string, string, string>(result.Entity, result.PrimaryKey, result.SecondaryKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private async Task<Dictionary<string, string>> GetStAcadCredDictionaryAsync(Dictionary<string, string> ids)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
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
                throw new Exception(string.Format("Error occured while getting guids for '{0}'.", "ACAD.CREDENTIALS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        /// Gets the cache key.
        /// </summary>
        /// <param name="criteriaEntity"></param>
        /// <param name="filterPersonIds"></param>
        /// <param name="acadProgramFilter"></param>
        /// <returns></returns>
        private string GetCacheKey(StudentAcademicCredential criteriaEntity, string[] filterPersonIds, string acadProgramFilter)
        {
            string cacheKey = "AllStudentAcademicCredentialsKey";
            string personFiltIds = string.Empty;
            string creds = string.Empty;

            //credentials Degrees
            if (criteriaEntity != null && criteriaEntity.Degrees != null && criteriaEntity.Degrees.Any())
            {
                creds = string.Join("", criteriaEntity.Degrees.Select(a => string.Format("{0}", a)));
                cacheKey = string.Format("{0}{1}", cacheKey, creds);
            }

            //credentials Certs
            if (criteriaEntity != null && criteriaEntity.Ccds != null && criteriaEntity.Ccds.Any())
            {
                creds = string.Join("", criteriaEntity.Ccds.Select(a => string.Format("{0}", a)));
                cacheKey = string.Format("{0}{1}", cacheKey, creds);
            }

            //graduatedOn
            if (criteriaEntity != null && criteriaEntity.GraduatedOn.HasValue)
            {
                cacheKey = string.Format("{0}{1}", cacheKey, criteriaEntity.GraduatedOn.Value.ToString());
            }

            //student
            if(criteriaEntity != null && !string.IsNullOrEmpty(criteriaEntity.StudentId))
            {
                cacheKey = string.Format("{0}{1}", cacheKey, criteriaEntity.StudentId);
            }

            //academicLevel
            if(criteriaEntity != null && !string.IsNullOrEmpty(criteriaEntity.AcademicLevel))
            {
                cacheKey = string.Format("{0}{1}", cacheKey, criteriaEntity.AcademicLevel);
            }

            //studentProgram
            if (criteriaEntity != null && !string.IsNullOrEmpty(criteriaEntity.AcadAcadProgramId))
            {
                cacheKey = string.Format("{0}{1}", cacheKey, criteriaEntity.AcadAcadProgramId);
            }

            //graduationAcademicPeriod
            if (criteriaEntity != null && !string.IsNullOrEmpty(criteriaEntity.AcademicPeriod))
            {
                cacheKey = string.Format("{0}{1}", cacheKey, criteriaEntity.AcademicPeriod);
            }

            //personFilter
            if (filterPersonIds != null && filterPersonIds.Any())
            {
                personFiltIds = string.Join("", filterPersonIds.Select(a => string.Format("{0}", a)));
                cacheKey = string.Format("{0}{1}", cacheKey, personFiltIds);
            }

            cacheKey = string.Format("{0}{1}", cacheKey, acadProgramFilter);
            return cacheKey;
        }

        /// <summary>
        /// Get the Defaults from CORE to compare default institution Id
        /// </summary>
        /// <returns>Core Defaults</returns>
        private async Task<Defaults> GetDefaultsAsync()
        {
            return await GetOrAddToCacheAsync<Defaults>("CoreDefaults",
                async () =>
                {
                    Defaults coreDefaults = await DataReader.ReadRecordAsync<Defaults>("CORE.PARMS", "DEFAULTS");
                    if (coreDefaults == null)
                    {
                        logger.Info("Unable to access DEFAULTS from CORE.PARMS table.");
                        coreDefaults = new Defaults();
                    }
                    return coreDefaults;
                }, Level1CacheTimeoutValue);
        }

        #endregion Helper Methods
    }
}