//Copyright 2017-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using System.Linq;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Domain.Base.Services;
using System.Text;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class JobApplicationsRepository : BaseColleagueRepository, IJobApplicationsRepository
    {
        private static char _VM = Convert.ToChar(DynamicArray.VM);
        private readonly int _readSize;
        const string AllJobApplicationsRecordsCache = "AllJobApplicationsRecordKeys";
        const int AllJobApplicationsRecordsCacheTimeout = 20;
        private RepositoryException exception = new RepositoryException();

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public JobApplicationsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET Method
        /// <summary>
        ///  Get a collection of JobApplication domain entity objects
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns>collection of JobApplication domain entity objects</returns>
        public async Task<Tuple<IEnumerable<JobApplication>, int>> GetJobApplicationsAsync(int offset, int limit, bool bypassCache = false)
        {
            string selectedRecordCacheKey = CacheSupport.BuildCacheKey(AllJobApplicationsRecordsCache);
            List<JobApplication> jobApplications = new List<JobApplication>();

            if (limit == 0) limit = _readSize;
            int totalCount = 0;
            var selectionCriteria = new StringBuilder();

            var keyCacheObject = await CacheSupport.GetOrAddKeyCacheToCache(
                this,
                ContainsKey,
                GetOrAddToCacheAsync,
                AddOrUpdateCacheAsync,
                transactionInvoker,
                selectedRecordCacheKey,
                "",
                offset,
                limit,
                AllJobApplicationsRecordsCacheTimeout,
                async () =>
                {
                    var criteria = "WITH JBAP.INTG.POS.IDX NE '' BY.EXP JBAP.INTG.POS.IDX";

                    var jobappsIds = await DataReader.SelectAsync("JOBAPPS", criteria);

                    criteria = string.Concat(criteria, " SAVING JBAP.INTG.POS.IDX");
                    var jobappsIndexIds = await DataReader.SelectAsync("JOBAPPS", criteria);

                    var jobappsIds2 = new List<string>();
                    int index = 0;
                    foreach (var jobappId in jobappsIds)
                    {
                        var jobappKey = string.Concat(jobappId.Split(_VM)[0], "|", jobappsIndexIds[index]);
                        jobappsIds2.Add(jobappKey);
                        index++;
                    }

                    CacheSupport.KeyCacheRequirements requirements = new CacheSupport.KeyCacheRequirements()
                    {
                        limitingKeys = jobappsIds2,
                        criteria = ""
                    };
                    return requirements;
                });

            if (keyCacheObject == null || keyCacheObject.Sublist == null || !keyCacheObject.Sublist.Any())
            {
                return new Tuple<IEnumerable<JobApplication>, int>(new List<JobApplication>(), 0);
            }

            totalCount = keyCacheObject.TotalCount.Value;

            var keysSubList = keyCacheObject.Sublist.ToArray();

            if (keysSubList == null || !keysSubList.Any())
            {
                return new Tuple<IEnumerable<JobApplication>, int>(new List<JobApplication>(), 0);
            }

            var jobApplicationEntities = new List<JobApplication>();
                       
            if (keysSubList.Any())
            {
                var keysJobappsSubList = keysSubList.Select(k => k.Split('|')[0]).Distinct();
                
                var jobApplicationsContracts = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DataContracts.Jobapps>("JOBAPPS", keysJobappsSubList.ToArray());
                if ((jobApplicationsContracts.InvalidKeys != null && jobApplicationsContracts.InvalidKeys.Any()) ||
                   jobApplicationsContracts.InvalidRecords != null && jobApplicationsContracts.InvalidRecords.Any())
                {
                    if (jobApplicationsContracts.InvalidKeys.Any())
                    {
                        exception.AddErrors(jobApplicationsContracts.InvalidKeys
                            .Select(key => new RepositoryError("Bad.Data",
                            string.Format("Unable to locate the following JOBAPPS key '{0}'.", key.ToString()))));
                    }
                    if (jobApplicationsContracts.InvalidRecords.Any())
                    {
                        exception.AddErrors(jobApplicationsContracts.InvalidRecords
                           .Select(r => new RepositoryError("Bad.Data",
                           string.Format("Error: '{0}' ", r.Value))
                           { SourceId = r.Key }));
                    }
                }

                foreach (var key in keysSubList)
                {
                    var jobappsKey = key.Split('|')[0];
                    var indexKey = key.Split('|')[1];
                    var jobapps = jobApplicationsContracts.BulkRecordsRead.FirstOrDefault(x => x.Recordkey == jobappsKey);

                    try
                    {
                        var index = 0;
                        foreach (var jobappsIndex in jobapps.JbapIntgPosIdx)
                        {
                            if (jobappsIndex == indexKey)
                            {
                                var jobApplicationGuidInfo = await GetGuidFromRecordInfoAsync("JOBAPPS", jobapps.Recordkey, "JBAP.INTG.POS.IDX", jobappsIndex);
                                jobApplicationEntities.Add(new JobApplication(jobApplicationGuidInfo, jobapps.Recordkey)
                                {
                                    PositionId = (jobapps.JbapPosId.Count >= index + 1) ? jobapps.JbapPosId[index] : null,
                                    AppliedOn = (jobapps.JbapApplicationDate.Count >= index + 1) ? jobapps.JbapApplicationDate[index] : null,
                                    DesiredCompensationRateValue = (jobapps.JbapMinSalary.Count >= index + 1) ? jobapps.JbapMinSalary[index] : null,
                                });
                            }
                            
                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        exception.AddError(new RepositoryError("GUID.Not.Found", e.Message)
                        {
                            Id = jobapps != null && jobapps.Recordkey != null ? jobapps.Recordkey : ""
                        });
                    }
                }
            }
            if (exception != null && exception.Errors != null && exception.Errors.Any())
            {
                throw exception;
            }
            return new Tuple<IEnumerable<JobApplication>, int>(jobApplicationEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Job Application key.
        /// </summary>
        /// <param name="guid">Key to Job Application to be returned</param>
        /// <returns>JobApplication Objects</returns>
        public async Task<JobApplication> GetJobApplicationByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                var errorMessage = "ID is required to get job-applications.";
                throw new ArgumentException(errorMessage);
            }

            var jobApplication = await GetRecordInfoFromGuidAsync(id);

            if (jobApplication == null)
                throw new KeyNotFoundException();

            if (jobApplication.Entity != "JOBAPPS")
            {
                throw new RepositoryException("GUID " + id + " has different entity, " + jobApplication.Entity + ", than expected, JOBAPPS");
            }

            var jobapps = await DataReader.ReadRecordAsync<Jobapps>("JOBAPPS", jobApplication.PrimaryKey);
            if (jobapps == null && !string.IsNullOrEmpty(jobApplication.PrimaryKey))
            {
                throw new KeyNotFoundException("Invalid JOBAPPS ID: " + jobApplication.PrimaryKey);
            }
            int index = 0;
            foreach (var jobappsRecord in jobapps.JbapIntgPosIdx)
            {
                ////convert a datetime to a unidata internal value 
                //var offsetDate = DmiString.DateTimeToPickDate(effectiveDate);

                //if (offsetDate.ToString().Equals(perposKey[1]))
                //{
                var jobApplicationGuidInfo = await GetGuidFromRecordInfoAsync("JOBAPPS", jobapps.Recordkey, "JBAP.INTG.POS.IDX", jobappsRecord);

                if (jobApplicationGuidInfo.Equals(id))
                {
                    return new JobApplication(id, jobapps.Recordkey)
                    {
                        PositionId = (jobapps.JbapPosId.Count >= index + 1) ? jobapps.JbapPosId[index] : null,
                        AppliedOn = (jobapps.JbapApplicationDate.Count >= index + 1) ? jobapps.JbapApplicationDate[index] : null,
                        DesiredCompensationRateValue = (jobapps.JbapMinSalary.Count >= index + 1) ? jobapps.JbapMinSalary[index] : null,           
                    };
                }
                index++;
                //}
            }

            throw new KeyNotFoundException(String.Format("No job-applications found for GUID {0}.", id));
        }

        #endregion


    }

}
