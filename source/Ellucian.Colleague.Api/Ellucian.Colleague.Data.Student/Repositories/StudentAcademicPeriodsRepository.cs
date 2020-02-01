// Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentAcademicPeriodsRepository : BaseColleagueRepository, IStudentAcademicPeriodRepository
    {
        private RepositoryException exception;
        private readonly int bulkReadSize;

        // setting for AllSections Cache
        const string AllStudentAcademicPeriodsCache = "AllStudentAcademicPeriods";
        const int AllStudentAcademicPeriodsCacheTimeout = 20; // Clear from cache every 20 minutes

        #region constructor
        public StudentAcademicPeriodsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
            exception = new RepositoryException();
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }
        #endregion

        #region public methods

        /// <summary>
        /// Returns StudentAcademicPeriod Entity as per the select criteria
        /// </summary>
        /// <returns>List of StudentAcademicPeriod Entities</returns>
        public async Task<Tuple<IEnumerable<StudentAcademicPeriod>, int>> GetStudentAcademicPeriodsAsync(int offset, int limit, bool bypassCache = false,
            string person = "", string academicPeriod = "", string[] filterPersonIds = null, List<string> newStudentStatuses = null)
        {
            var dict = new Dictionary<string, string>();
            var studentAcademicPeriodEntities = new List<StudentAcademicPeriod>();
            int totalCount = 0;
            string[] subList = null;

            try
            {
                string studentAcademicPeriodsCacheKey = CacheSupport.BuildCacheKey(AllStudentAcademicPeriodsCache, person, academicPeriod, filterPersonIds, newStudentStatuses);
                var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    studentAcademicPeriodsCacheKey,
                    "",
                    offset,
                    limit,
                    AllStudentAcademicPeriodsCacheTimeout,
                    async () => {
                        #region person filter
                        var keys = new List<string>();

                        if (filterPersonIds != null && filterPersonIds.ToList().Any())
                        {
                            // Set limiting keys to previously retrieved personIds from SAVE.LIST.PARMS
                            string[] limitingKeys = filterPersonIds;
                            for (int i = 0; i < limitingKeys.Count(); i += bulkReadSize)
                            {
                                var personSubList = limitingKeys.Skip(i).Take(bulkReadSize);
                                var records = await DataReader.BulkReadRecordAsync<Students>(personSubList.ToArray());

                                if (records != null && records.Any())
                                {
                                    keys.AddRange(ExtractStudentTermKeys(records));
                                }
                            }
                        }
                        #endregion

                        #region additional filters
                        var criteria = new StringBuilder();
                        if (!string.IsNullOrEmpty(person))
                        {
                            criteria.Append("WITH STTR.STUDENT EQ '" + person + "'");
                        }

                        if (!string.IsNullOrEmpty(academicPeriod))
                        {
                            if (criteria.Length > 0)
                            {
                                criteria.Append(" AND ");
                            }

                            criteria.Append("WITH STTR.TERM EQ '" + academicPeriod + "'");
                        }

                        if (newStudentStatuses != null && newStudentStatuses.Any())
                        {
                            if (criteria.Length > 0)
                            {
                                criteria.Append(" AND ");
                            }

                            criteria.Append("WITH STTR.CURRENT.STATUS EQ '" + (string.Join(" ", newStudentStatuses.Distinct())).Replace(" ", "' '") + "'");
                        }

                        var studentAcademicPeriodIds = await DataReader.SelectAsync("STUDENT.TERMS", keys.Distinct().ToArray(), criteria.ToString());

                        // Group by combinations of students and terms
                        var groupedResults = studentAcademicPeriodIds
                            .Select(s => new
                            {
                                person = s.Split(new[] { '*' })[0],
                                term = s.Split(new[] { '*' })[1],
                                recordKey = s
                            })
                            .GroupBy(x => new { x.person, x.term }, (key, group) => new
                            {
                                 Person = key.person,
                                 Term = key.term,
                                 Result = group.ToList()
                             }
                         ).ToList();

                        //Each record in the groupedResults consists of a: 
                        // 1) unique person/student id
                        // 2) unique term id
                        // 3) group of STUDENT.TERM ids that start with personId*termId. 
                        var limitingKeysList = new List<string>();
                        foreach (var sub in groupedResults)
                        {
                            if (sub.Result != null && sub.Result.Any())
                            {
                                var studentTermKeys = sub.Result.Select(s => s.recordKey).ToList();
                                //limitingKeysList.Add(string.Concat(sub.Person, "|", sub.Term, "|", string.Join("|", studentTermKey)));
                                limitingKeysList.Add(string.Join("|", studentTermKeys));
                            }
                        }

                        #endregion
                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = limitingKeysList,
                            criteria = criteria.ToString(),
                        };
                        return requirements;
                    });

                if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
                {
                    return new Tuple<IEnumerable<StudentAcademicPeriod>, int>(studentAcademicPeriodEntities, 0);
                }
                subList = keyCacheObject.Sublist.ToArray();
                totalCount = keyCacheObject.TotalCount.Value;

                var keysSubList = new List<string>();
                var idKeys = new List<string>();
               
                foreach (var sub in subList)
                {
                    var studentTerms = sub.Split('|');
                    var splitKey = studentTerms[0].Split('*');
                    idKeys.Add(splitKey[0] + "|" + splitKey[1]);  
                    keysSubList.AddRange(studentTerms);
                    
                }
                var studentAcademicPeriodRecords = await DataReader.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", keysSubList.ToArray());
                
                try
                {
                    dict = await GetGuidsCollectionAsync(idKeys);
                }
                catch (Exception ex)
                {
                    exception.AddError(new RepositoryError("Bad.Data", "An error occurred extracting guids. " + ex.Message));
                    throw exception;
                }

                foreach (var result in idKeys)
                {
                    var resultKey = result.Split('|');

                    string applGuidInfo = string.Empty;
                    dict.TryGetValue(result, out applGuidInfo);

                    if (string.IsNullOrEmpty(applGuidInfo))
                    {
                        exception.AddError(new RepositoryError("Bad.Data",
                           "Unable to locate guid for Entity STUDENTS, Secondary field STU.TERMS.")
                        { SourceId = result });
                    }
                    else
                    {
                        var studentAcademicPeriods = studentAcademicPeriodRecords
                           .Where(a => a.Recordkey.Split('*')[0] == resultKey[0]
                              && a.Recordkey.Split('*')[1] == resultKey[1]);

                        if (studentAcademicPeriods == null || !studentAcademicPeriods.Any())
                        {
                            exception.AddError(new RepositoryError("Bad.Data",
                                "Unable to build student academic periods. StudentAcademicPeriods is required.")
                            { SourceId = result });
                        }
                        else
                        {
                            var studentAcadPeriod = BuildStudentAcademicPeriod(applGuidInfo, studentAcademicPeriods.ToList());

                            if (studentAcadPeriod != null)
                            {
                                studentAcademicPeriodEntities.Add(studentAcadPeriod);
                            }
                        }
                    }
                }

                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }
                return new Tuple<IEnumerable<StudentAcademicPeriod>, int>(studentAcademicPeriodEntities.AsEnumerable(), totalCount);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                exception.AddError(new RepositoryError("Bad.Data", e.Message));
                throw exception;
            }
        }

        /// <summary>
        /// Using a collection of student data contracts, build a collection of related student.term keys
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        private List<string> ExtractStudentTermKeys(System.Collections.ObjectModel.Collection<Students> records)
        {
            var keys = new List<string>();

            if (records == null || !records.Any())
            {
                return keys;
            }

            foreach (var record in records)
            {
                if (record.StuTerms != null && record.StuTerms.Any())
                {
                    foreach (var term in record.StuTerms)
                    {
                        if (record.StuAcadLevels != null && record.StuAcadLevels.Any())
                        {
                            foreach (var acdLevel in record.StuAcadLevels)
                            {
                                keys.Add(string.Concat(record.Recordkey, '*', term, '*', acdLevel));
                            }
                        }
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// Using a collection of ids in the format STUDENTS|STU.TERMS
        ///  get a dictionary collection of associated guids
        /// </summary>
        /// <param name="ids">collection of ids</param>
        /// <returns>Dictionary consisting of a STUDENTS|STU.TERMS (key) and guids (value)</returns>
        public async Task<Dictionary<string, string>> GetGuidsCollectionAsync(IEnumerable<string> ids)
        {
            if ((ids == null) || (ids != null && !ids.Any()))
            {
                return new Dictionary<string, string>();
            }
            var guidCollection = new Dictionary<string, string>();
            try
            {

                var guidLookup = ids.Select(s => new
                {
                    recordKey = s.Split(new[] { '|' })[0],
                    secondardaryKey = s.Split(new[] { '|' })[1],
                })
                    //.Where(s => !string.IsNullOrWhiteSpace(s.recordKey))
                    .ToList()
                    .ConvertAll(applicationKey => new RecordKeyLookup("STUDENTS", applicationKey.recordKey,
                    "STU.TERMS", applicationKey.secondardaryKey, false))
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
                                guidCollection.Add(string.Concat(splitKeys[1], "|", splitKeys[2]), recordKeyLookupResult.Value.Guid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while getting guids for {0}.", "STUDENTS"), ex);
            }

            return guidCollection;
        }

        /// <summary>
        /// Get Student Academic Period for a guid
        /// </summary>
        /// <param name="guid">IDs</param>
        /// <returns>StudentAcademicPeriods entity objects</returns>
        public async Task<StudentAcademicPeriod> GetStudentAcademicPeriodByGuidAsync(string guid)
        {
            StudentAcademicPeriod studentAcademicPeriod = null;
            if (string.IsNullOrEmpty(guid))
            {
                return studentAcademicPeriod;
            }
            var key = await GetStudentAcademicPeriodKeyFromGuidAsync(guid);

            if (string.IsNullOrEmpty(key))
            {
                exception.AddError(new RepositoryError("Bad.Data",
                            "Unable to build student academic periods. Guid not found.")
                { Id = guid });
                throw exception;
            }
            try
            {
                var splitKey = key.Split('|');
                var student = await DataReader.ReadRecordAsync<Students>("STUDENTS", splitKey[0]);
                if (student == null)
                {
                    exception.AddError(new RepositoryError("Bad.Data",
                        "Unable to build student academic periods. Students record not found.")
                    { Id = guid });
                    throw exception;
                }
                var studentAcademicPeriodIds = new List<string>();
                foreach (var acadLevel in student.StuAcadLevels)
                {
                    studentAcademicPeriodIds.Add(string.Concat(splitKey[0], '*', splitKey[1], '*', acadLevel));
                }

                var studentAcademicPeriodRecords = await DataReader.BulkReadRecordAsync<StudentTerms>("STUDENT.TERMS", studentAcademicPeriodIds.ToArray());
                if (studentAcademicPeriodRecords == null)
                {
                    exception.AddError(new RepositoryError("Bad.Data",
                        "Unable to build student academic periods. Students.Terms record not found for: "
                        + string.Join(", ", studentAcademicPeriodIds))
                    { Id = guid });
                    throw exception;
                }
                var returnAcademicPeriod = BuildStudentAcademicPeriod(guid, studentAcademicPeriodRecords.ToList());
                if (exception != null && exception.Errors != null && exception.Errors.Any())
                {
                    throw exception;
                }

                return returnAcademicPeriod;
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data",
                    "Unable to build student academic periods. " + ex.Message)
                { Id = guid });
                throw exception;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Build Student Academic Period domain entity
        /// </summary>
        /// <param name="applGuidInfo">guid</param>
        /// <param name="studentTerms">Collection of StudentTerms</param>
        /// <returns>StudentAcademicPeriod domain entity</returns>
        private StudentAcademicPeriod BuildStudentAcademicPeriod(string applGuidInfo, List<StudentTerms> studentTerms)
        {
            StudentAcademicPeriod studentAcademicPeriod = null;
            if (string.IsNullOrEmpty(applGuidInfo))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build student academic periods. StudentAcademicPeriods guid is required."));
                return null;
            }

            if (studentTerms == null || !studentTerms.Any())
            {
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build student academic periods. StudentAcademicPeriods is required."));
                return null;
            }

            var studentTerm = studentTerms.FirstOrDefault();

            if (string.IsNullOrEmpty(studentTerm.Recordkey))
            {
                exception.AddError(new RepositoryError("Bad.Data", "Unable to build student academic periods. StudentAcademicPeriods recordKey is required."));
                return null;
            }

            var studentAcademicPeriodsDelimited = studentTerm.Recordkey.Split(new[] { '*' });
            if (studentAcademicPeriodsDelimited == null || !studentAcademicPeriodsDelimited.Any() || studentAcademicPeriodsDelimited.Count() < 3)
            {
                exception.AddError(new RepositoryError("Bad.Data",
                    string.Concat("Invalid record key, Entity:'STUDENT.TERMS', Record ID:'", studentTerm.Recordkey, "'"))
                { SourceId = studentTerm.Recordkey, Id = applGuidInfo });
                return null;
            }
            var studentId = studentAcademicPeriodsDelimited[0];
            var termKey = studentAcademicPeriodsDelimited[1];
            if (string.IsNullOrEmpty(studentId) && string.IsNullOrEmpty(termKey))
            {
                exception.AddError(new RepositoryError("Bad.Data",
                    "Unable to build student academic periods. StudentAcademicPeriods studentId, term, and level are required.")
                { SourceId = studentTerm.Recordkey, Id = applGuidInfo });
                return null;
            }
            try
            {
                var studentAcademicPeriodTerms = new List<StudentTerm>();
                foreach (var term in studentTerms)
                {
                    var termDelimited = term.Recordkey.Split(new[] { '*' });
                    var studentAcademicPeriodTerm = new StudentTerm(termDelimited[0], termDelimited[1], termDelimited[2])
                    {
                        StudentLoad = term.SttrStudentLoad
                    };

                    if (term.SttrStatus != null && term.SttrStatus.Any())
                    {
                        studentAcademicPeriodTerm.StudentTermStatuses = new List<StudentTermStatus>();
                        try
                        {
                            var stTermStatus = new StudentTermStatus(term.SttrStatus[0], term.SttrStatusDate[0]);
                            studentAcademicPeriodTerm.StudentTermStatuses.Add(stTermStatus);
                        }
                        catch (ArgumentNullException e)
                        {
                            exception.AddError(new RepositoryError("Missing.Required.Property", string.Format("{0} is a required field.", e.ParamName)) 
                            { SourceId = studentTerm.Recordkey, Id = applGuidInfo });
                            return null;
                        }
                    }

                    studentAcademicPeriodTerms.Add(studentAcademicPeriodTerm);
                }

                try
                {
                    studentAcademicPeriod = new StudentAcademicPeriod(applGuidInfo, studentId, termKey)
                    {
                        StudentTerms = studentAcademicPeriodTerms
                    };
                }
                catch (ArgumentNullException e)
                {
                    exception.AddError(new RepositoryError("Missing.Required.Property", string.Format("{0} is a required field.", e.ParamName)) 
                    { SourceId = studentTerm.Recordkey, Id = applGuidInfo });
                    return null;
                }
            }
            catch (Exception ex)
            {
                exception.AddError(new RepositoryError("Bad.Data",
                   ex.Message)
                { SourceId = studentTerm.Recordkey, Id = applGuidInfo });
                return null;
            }

            return studentAcademicPeriod;
        }

        /// <summary>
        /// Get the record key from a GUID
        /// </summary>
        /// <param name="guid">The GUID</param>
        /// <returns>Primary key</returns>
        private async Task<string> GetStudentAcademicPeriodKeyFromGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid");
            }

            var idDict = await DataReader.SelectAsync(new GuidLookup[] { new GuidLookup(guid) });
            if (idDict == null || idDict.Count == 0)
            {
                throw new KeyNotFoundException("StudentAcademicPeriod GUID " + guid + " not found.");
            }

            var foundEntry = idDict.FirstOrDefault();
            if (foundEntry.Value == null)
            {
                throw new KeyNotFoundException("StudentAcademicPeriod GUID " + guid + " lookup failed.");
            }


            if (foundEntry.Value.Entity != "STUDENTS" && string.IsNullOrEmpty(foundEntry.Value.SecondaryKey))
            {
                var errorMessage = string.Format("GUID {0} has different entity, {1}, than expected, STUDENTS", guid, foundEntry.Value.Entity);
                logger.Error(errorMessage);
                var exception = new RepositoryException(errorMessage);
                exception.AddError(new RepositoryError("GUID.Wrong.Type", errorMessage));
                throw exception;
            }

            return string.Concat(foundEntry.Value.PrimaryKey, "|", foundEntry.Value.SecondaryKey);
        }
        #endregion
    }
}