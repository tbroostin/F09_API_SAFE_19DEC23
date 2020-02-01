// Copyright 2017-2019 Ellucian Company L.P. and its affiliates

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
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Implement the IStudentFinancialAidAwardsRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentFinancialAidAwardRepository : BaseColleagueRepository, IStudentFinancialAidAwardRepository
    {

        const string AllStudentFinancialAidAwardsCache = "StudentFinancialAidAwards";
        const int AllStudentFinancialAidAwardsCacheTimeout = 20; // Clear from cache every 20 minutes


        /// <summary>
        /// Constructor to instantiate a student FinancialAidAwards repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public StudentFinancialAidAwardRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get the StudentFinancialAidAwards requested.
        /// </summary>
        /// <param name="id">StudentFinancialAidAwards GUID</param>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<StudentFinancialAidAward> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Read the TC.ACYR record
            var recordInfo = await GetRecordInfoFromGuidAsync(id);
            if (recordInfo == null || string.IsNullOrEmpty(recordInfo.PrimaryKey) || string.IsNullOrEmpty(recordInfo.Entity) || recordInfo.Entity.Substring(0, 3) != "TC.")
            {
                throw new KeyNotFoundException(string.Format("No student FA award was found for guid {0}'. ", id));
            }
            var year = recordInfo.Entity.Substring(3, 4);
            var tcAcyrFile = string.Concat("TC.", year);
            var tcAcyrDataContract = await DataReader.ReadRecordAsync<TcAcyr>(tcAcyrFile, recordInfo.PrimaryKey);
            {
                if (tcAcyrDataContract == null)
                {
                    throw new KeyNotFoundException(string.Format("No student FA award was found for guid {0}'. ", id));
                }
            }

            var tcAcyrKeys = new List<string>();
            foreach (var tcTerm in tcAcyrDataContract.TcTaTerms)
            {
                tcAcyrKeys.Add(tcTerm);
            }
            var subList = tcAcyrKeys.ToArray();
            var taAcyrFile = string.Concat("TA.", year);
            var taAcyrDataContracts = await DataReader.BulkReadRecordAsync<TaAcyr>(taAcyrFile, subList);

            var recordKey = tcAcyrDataContract.Recordkey;

            var studentId = tcAcyrDataContract.Recordkey.Split('*').ElementAt(0);
            var awardId = tcAcyrDataContract.Recordkey.Split('*').ElementAt(1);
            var studentFinancialAidAward = new StudentFinancialAidAward(tcAcyrDataContract.RecordGuid, studentId, awardId, year);

            studentFinancialAidAward.AwardHistory = await BuildStudentFinancialAidAward(taAcyrDataContracts, year);

            return studentFinancialAidAward;
        }

        /// <summary>
        /// Get student FinancialAidAwards for specific filters.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="restricted">True if you are allowed to see restricted awards</param>
        /// <param name="awardYears">List of award years to include</param>
        /// <param name="criteriaEntity">Filter parameters</param>
        /// <returns>A list of StudentFinancialAidAward domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentFinancialAidAward>, int>> GetAsync(int offset, int limit, bool bypassCache, bool restricted, IEnumerable<string> unrestrictedFunds, 
            IEnumerable<string> awardYears, StudentFinancialAidAward criteriaEntity = null)
        {
            var studentFinancialAidAwardsEntities = new List<StudentFinancialAidAward>();
            string faCriteria = string.Empty;
            string tcCriteria = string.Empty;

            var tcAcyrIds = new List<string>();
            int totalCount = 0;

            /*
                Filter Scenarios:
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}, 
                "aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"}}
                criteria={"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}}
                criteria={"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"},"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
            */
            if (criteriaEntity != null)
            {
                if (!string.IsNullOrEmpty(criteriaEntity.StudentId))
                {
                    tcCriteria = string.Format("WITH TC.STUDENT.ID EQ '{0}'", criteriaEntity.StudentId);

                    var record = await DataReader.ReadRecordAsync<FinAid>("FIN.AID", criteriaEntity.StudentId);                    
                    if (record == null)
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAward>, int>( new List<StudentFinancialAidAward>(), 0);
                    }

                    if (record.FaSaYears == null || !record.FaSaYears.Any())
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(new List<StudentFinancialAidAward>(), 0);
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(criteriaEntity.AidYearId))
                        {
                            if (record.FaSaYears.Contains(criteriaEntity.AidYearId))
                            {
                                awardYears = new List<string>() { criteriaEntity.AidYearId };
                            }
                            else
                            {
                                return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(new List<StudentFinancialAidAward>(), 0);
                            }
                        }
                        else
                        {
                            awardYears = record.FaSaYears;
                        }
                    }
                }
                else if(!string.IsNullOrEmpty(criteriaEntity.AidYearId) && string.IsNullOrEmpty(criteriaEntity.StudentId))
                {
                    faCriteria = string.Format("WITH FA.SA.YEARS EQ '{0}'", criteriaEntity.AidYearId);
                    var records = await DataReader.SelectAsync("FIN.AID", faCriteria);
                    if (records == null || !records.Any())
                    {
                        return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(new List<StudentFinancialAidAward>(), 0);
                    }
                    awardYears = new List<string>() { criteriaEntity.AidYearId };
                }

                //awardFundId
                if (!string.IsNullOrEmpty(criteriaEntity.AwardFundId))
                {
                    if(string.IsNullOrEmpty(tcCriteria))
                    {
                        tcCriteria = string.Format("WITH TC.AWARD.ID EQ '{0}'", criteriaEntity.AwardFundId);
                    }
                    else
                    {
                        tcCriteria = string.Format("{0} AND TC.AWARD.ID EQ '{1}'", tcCriteria, criteriaEntity.AwardFundId);
                    }
                }
            }

            foreach (var awardYear in awardYears)
            {                
                var tcAcyrFile = string.Concat("TC.", awardYear);
                string[] studentFinancialAidAwardIds = await DataReader.SelectAsync(tcAcyrFile, tcCriteria);
                if(studentFinancialAidAwardIds == null || !studentFinancialAidAwardIds.Any())
                {
                    continue;
                }
                if (restricted == true)
                {
                    // Running for restricted only so exclude any student awards from unrestricted funds.
                    if (unrestrictedFunds != null && unrestrictedFunds.Any())
                    {
                        studentFinancialAidAwardIds = studentFinancialAidAwardIds.Where(id => !(unrestrictedFunds.Contains(id.Split('*')[1]))).ToArray();
                    }
                }
                else
                {
                    //  Running for unrestricted only so include only student awards for unrestricted funds.
                    if (unrestrictedFunds != null && unrestrictedFunds.Any())
                    {
                        studentFinancialAidAwardIds = studentFinancialAidAwardIds.Where(id => unrestrictedFunds.Contains(id.Split('*')[1])).ToArray();
                    }
                    else
                    {
                        // Need to return unrestricted, but no unrestricted funds found.  So return no student awards.
                        studentFinancialAidAwardIds = null;
                    }
                }
                totalCount += studentFinancialAidAwardIds == null ? 0 : studentFinancialAidAwardIds.Count();

                if (studentFinancialAidAwardIds != null)
                {
                    Array.Sort(studentFinancialAidAwardIds);
                    foreach (var id in studentFinancialAidAwardIds)
                    {
                        tcAcyrIds.Add(string.Concat(awardYear, '.', id));
                    }
                }
            }

            var subItems = tcAcyrIds.Skip(offset).Take(limit).ToArray();
            List<string> years = subItems.GroupBy(s => s.Split('.')[0])
                           .Select(g => g.First().Split('.')[0]).Distinct()
                           .ToList();

            foreach (var year in years)
            {
                var tcAcyrFile = string.Concat("TC.", year);
                var subList = subItems.Where(s => s.Split('.')[0] == year)
                           .Select(g => g.Split('.')[1]).ToArray();

                var tcAcyrDataContracts = await DataReader.BulkReadRecordAsync<TcAcyr>(tcAcyrFile, subList);

                var taAcyrKeys = tcAcyrDataContracts.SelectMany(tc => tc.TcTaTerms).Distinct().ToArray();
                var taAcyrFile = string.Concat("TA.", year);
                var taAcyrDataContracts = await DataReader.BulkReadRecordAsync<TaAcyr>(taAcyrFile, taAcyrKeys);

                foreach (var dataContract in tcAcyrDataContracts)
                {
                    var studentId = dataContract.Recordkey.Split('*').ElementAt(0);
                    var awardId = dataContract.Recordkey.Split('*').ElementAt(1);
                    var studentFinancialAidAward = new StudentFinancialAidAward(dataContract.RecordGuid, studentId, awardId, year);

                    var recordKey = string.Concat(dataContract.Recordkey.Split('*')[0], '*', dataContract.Recordkey.Split('*')[1]);

                    var taAcyrData = taAcyrDataContracts.Where(ta => dataContract.TcTaTerms.Contains(ta.Recordkey));
                    studentFinancialAidAward.AwardHistory = await BuildStudentFinancialAidAward(taAcyrData, year);

                    studentFinancialAidAwardsEntities.Add(studentFinancialAidAward);
                }
            }

            return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(studentFinancialAidAwardsEntities, totalCount);
        }


        /// <summary>
        /// Get student FinancialAidAwards for specific filters.
        /// </summary>
        /// <param name="offset">Offset for paging results</param>
        /// <param name="limit">Limit for paging results</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="restricted">True if you are allowed to see restricted awards</param>
        /// <param name="awardYears">List of award years to include</param>
        /// <param name="criteriaEntity">Filter parameters</param>
        /// <returns>A list of StudentFinancialAidAward domain entities</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no database records exist for the given id argument</exception>
        public async Task<Tuple<IEnumerable<StudentFinancialAidAward>, int>> Get2Async(int offset, int limit, bool bypassCache, bool restricted, IEnumerable<string> unrestrictedFunds,
            IEnumerable<string> awardYears, StudentFinancialAidAward criteriaEntity = null)
        {
            var studentFinancialAidAwardsEntities = new List<StudentFinancialAidAward>();
            string faCriteria = string.Empty;
            string tcCriteria = string.Empty;

            var tcAcyrIds = new List<string>();

            int totalCount = 0;

            /*
                Filter Scenarios:
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}, 
                "aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"}}
                criteria={"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}}
                criteria={"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"}}
                criteria={"student":{"id":"154749aa-4dff-471b-b7da-64dd7757010e"},"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
                criteria={"awardFund":{"id":"c96507ea-6936-4884-9388-3f49fc0299f7"},"aidYear":{"id":"e784b12c-87b1-4976-8801-c8c875aba9ce"}}
            */
            var criteriaCacheKey = string.Empty;
            if (criteriaEntity != null)
            {
                criteriaCacheKey = string.Concat(criteriaEntity.StudentId, ",", criteriaEntity.AidYearId, "," , criteriaEntity.AwardFundId);
            }
            string studentFinancialAidAwardsCacheKey = CacheSupport.BuildCacheKey(AllStudentFinancialAidAwardsCache, unrestrictedFunds != null ? unrestrictedFunds.ToList() : null,
                awardYears!= null ? awardYears.ToList() : null, criteriaCacheKey);

            try
            {
                var keyCache = await CacheSupport.GetOrAddKeyCacheToCache(
                    this,
                    ContainsKey,
                    GetOrAddToCacheAsync,
                    AddOrUpdateCacheAsync,
                    transactionInvoker,
                    studentFinancialAidAwardsCacheKey,
                    "",
                    offset,
                    limit,
                    AllStudentFinancialAidAwardsCacheTimeout,
                    async () =>
                    {
                        if (criteriaEntity != null)
                        {
                            if (!string.IsNullOrEmpty(criteriaEntity.StudentId))
                            {
                                tcCriteria = string.Format("WITH TC.STUDENT.ID EQ '{0}'", criteriaEntity.StudentId);

                                var record = await DataReader.ReadRecordAsync<FinAid>("FIN.AID", criteriaEntity.StudentId);
                                if (record == null)
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }

                                if (record.FaSaYears == null || !record.FaSaYears.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                else
                                {

                                    if (!string.IsNullOrEmpty(criteriaEntity.AidYearId))
                                    {
                                        if (record.FaSaYears.Contains(criteriaEntity.AidYearId))
                                        {
                                            awardYears = new List<string>() { criteriaEntity.AidYearId };
                                        }
                                        else
                                        {
                                            return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                        }
                                    }
                                    else
                                    {
                                        awardYears = record.FaSaYears;
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(criteriaEntity.AidYearId) && string.IsNullOrEmpty(criteriaEntity.StudentId))
                            {
                                faCriteria = string.Format("WITH FA.SA.YEARS EQ '{0}'", criteriaEntity.AidYearId);
                                var records = await DataReader.SelectAsync("FIN.AID", faCriteria);
                                if (records == null || !records.Any())
                                {
                                    return new CacheSupport.KeyCacheRequirements() { NoQualifyingRecords = true };
                                }
                                awardYears = new List<string>() { criteriaEntity.AidYearId };
                            }

                            //awardFundId
                            if (!string.IsNullOrEmpty(criteriaEntity.AwardFundId))
                            {
                                if (string.IsNullOrEmpty(tcCriteria))
                                {
                                    tcCriteria = string.Format("WITH TC.AWARD.ID EQ '{0}'", criteriaEntity.AwardFundId);
                                }
                                else
                                {
                                    tcCriteria = string.Format("{0} AND TC.AWARD.ID EQ '{1}'", tcCriteria, criteriaEntity.AwardFundId);
                                }
                            }
                        }

                        foreach (var awardYear in awardYears)
                        {
                            var tcAcyrFile = string.Concat("TC.", awardYear);
                            string[] studentFinancialAidAwardIds = await DataReader.SelectAsync(tcAcyrFile, tcCriteria);
                            if (studentFinancialAidAwardIds == null || !studentFinancialAidAwardIds.Any())
                            {
                                continue;
                            }
                            if (restricted == true)
                            {
                                // Running for restricted only so exclude any student awards from unrestricted funds.
                                if (unrestrictedFunds != null && unrestrictedFunds.Any())
                                {
                                    studentFinancialAidAwardIds = studentFinancialAidAwardIds.Where(id => !(unrestrictedFunds.Contains(id.Split('*')[1]))).ToArray();
                                }
                            }
                            else
                            {
                                //  Running for unrestricted only so include only student awards for unrestricted funds.
                                if (unrestrictedFunds != null && unrestrictedFunds.Any())
                                {
                                    studentFinancialAidAwardIds = studentFinancialAidAwardIds.Where(id => unrestrictedFunds.Contains(id.Split('*')[1])).ToArray();
                                }
                                else
                                {
                                    // Need to return unrestricted, but no unrestricted funds found.  So return no student awards.
                                    studentFinancialAidAwardIds = null;
                                }
                            }
                            totalCount += studentFinancialAidAwardIds == null ? 0 : studentFinancialAidAwardIds.Count();

                            if (studentFinancialAidAwardIds != null)
                            {
                                Array.Sort(studentFinancialAidAwardIds);
                                foreach (var id in studentFinancialAidAwardIds)
                                {
                                    tcAcyrIds.Add(string.Concat(awardYear, '.', id));
                                }
                            }
                        }

                        CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                        {
                            limitingKeys = tcAcyrIds.Distinct().ToList(),
                            criteria = ""
                        };

                        return requirements;

                    });

                if (keyCache == null || keyCache.Sublist == null || !keyCache.Sublist.Any())
                {

                    return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(new List<StudentFinancialAidAward>(), 0);

                }

                var subItems = keyCache.Sublist.ToArray();
 
                totalCount = keyCache.TotalCount.Value;

                //var subItems = subList.Skip(offset).Take(limit).ToArray();
                List<string> years = subItems.GroupBy(s => s.Split('.')[0])
                               .Select(g => g.First().Split('.')[0]).Distinct()
                               .ToList();

                foreach (var year in years)
                {
                    var tcAcyrFile = string.Concat("TC.", year);
                    var subListItems = subItems.Where(s => s.Split('.')[0] == year)
                               .Select(g => g.Split('.')[1]).ToArray();

                    var tcAcyrDataContracts = await DataReader.BulkReadRecordAsync<TcAcyr>(tcAcyrFile, subListItems);

                    var taAcyrKeys = tcAcyrDataContracts.SelectMany(tc => tc.TcTaTerms).Distinct().ToArray();
                    var taAcyrFile = string.Concat("TA.", year);
                    var taAcyrDataContracts = await DataReader.BulkReadRecordAsync<TaAcyr>(taAcyrFile, taAcyrKeys);

                    foreach (var dataContract in tcAcyrDataContracts)
                    {
                        var studentId = dataContract.Recordkey.Split('*').ElementAt(0);
                        var awardId = dataContract.Recordkey.Split('*').ElementAt(1);
                        var studentFinancialAidAward = new StudentFinancialAidAward(dataContract.RecordGuid, studentId, awardId, year);

                        var recordKey = string.Concat(dataContract.Recordkey.Split('*')[0], '*', dataContract.Recordkey.Split('*')[1]);

                        var taAcyrData = taAcyrDataContracts.Where(ta => dataContract.TcTaTerms.Contains(ta.Recordkey));
                        studentFinancialAidAward.AwardHistory = await BuildStudentFinancialAidAward(taAcyrData, year);

                        studentFinancialAidAwardsEntities.Add(studentFinancialAidAward);
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = new RepositoryException();
                exception.AddError(new RepositoryError("Global.Internal.Error", ex.Message));
                throw exception;
            }

            return new Tuple<IEnumerable<StudentFinancialAidAward>, int>(studentFinancialAidAwardsEntities, totalCount);
        }

        /// <summary>
        /// Returns the list of award status categories that are excluded from 
        /// being considered as an awarded status.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetNotAwardedCategoriesAsync()
        {
            var sysParms = await GetSystemParametersAsync();
            return sysParms.FspNotAwardedCat;
        }

        private async Task<List<StudentAwardHistoryByPeriod>> BuildStudentFinancialAidAward(IEnumerable<TaAcyr> taAcyrDataContracts, string year)
        {
            var studentAwardHistories = new List<StudentAwardHistoryByPeriod>();

            foreach (var taDataContract in taAcyrDataContracts)
            {
                var studentId = taDataContract.Recordkey.Split('*').ElementAt(0);
                var awardId = taDataContract.Recordkey.Split('*').ElementAt(1);
                var awardPeriod = taDataContract.Recordkey.Split('*').ElementAt(2);

                var studentAwardsHistory = new StudentAwardHistoryByPeriod()
                {
                    AwardPeriod = awardPeriod,
                    Status = taDataContract.TaTermAction,
                    StatusDate = taDataContract.TaTermActionDate,
                    Amount = taDataContract.TaTermAmount,
                    XmitAmount = taDataContract.TaTermXmitAmt
                };

                // Get all award history for this award
                var criteria = new StringBuilder();
                criteria.AppendFormat("WITH FAWH.STUDENT.ID = '{0}'", studentId);
                criteria.AppendFormat(" AND WITH FAWH.FA.YEAR = '{0}'", year);
                criteria.AppendFormat(" AND WITH FAWH.AWARD.PERIOD = '{0}'", awardPeriod);
                criteria.AppendFormat(" AND WITH FAWH.AWARD.CODE = '{0}'", awardId);
                criteria.Append(" BY FA.AWARD.HISTORY.CHGDATE BY FAWH.CHG.TIME ");

                string[] awardHistoryIds = await DataReader.SelectAsync("FA.AWARD.HISTORY", criteria.ToString());
                Collection<FaAwardHistory> faAwardHistoryDataContracts = null;
                if (awardHistoryIds != null && awardHistoryIds.Any())
                {
                    faAwardHistoryDataContracts = await DataReader.BulkReadRecordAsync<FaAwardHistory>(awardHistoryIds);
                }

                if (faAwardHistoryDataContracts != null && faAwardHistoryDataContracts.Any())
                {
                    var studentAwardHistoryStatuses = new List<StudentAwardHistoryStatus>();
                    foreach (var history in faAwardHistoryDataContracts)
                    {
                        var datePart = history.FaAwardHistoryChgdate.Value;
                        var timePart = history.FawhChgTime.Value;
                        var changeDate = new DateTime(datePart.Year, datePart.Month, datePart.Day, timePart.Hour, timePart.Minute, timePart.Second);
                        var studentAwardsHistoryStatus = new StudentAwardHistoryStatus()
                        {
                            Status = history.FawhCrntTermAction,
                            StatusDate = changeDate,
                            Amount = history.FawhCrntTermAmt
                        };
                        studentAwardHistoryStatuses.Add(studentAwardsHistoryStatus);
                    }
                    if (studentAwardHistoryStatuses != null && studentAwardHistoryStatuses.Any())
                    {
                        studentAwardsHistory.StatusChanges = studentAwardHistoryStatuses;
                    }
                }
                studentAwardHistories.Add(studentAwardsHistory);
            }

            return studentAwardHistories;
        }


        private async Task<FaSysParams> GetSystemParametersAsync()
        {
            return await GetOrAddToCacheAsync<FaSysParams>("FinancialAidSystemParameters", 
                async () =>
                {
                    return await DataReader.ReadRecordAsync<FaSysParams>("ST.PARMS", "FA.SYS.PARAMS");

                }, Level1CacheTimeoutValue);
        }
    }
}