using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Exceptions;
//Copyright 2017 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Data.HumanResources.Transactions;

namespace Ellucian.Colleague.Data.HumanResources.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class JobApplicationsRepository : BaseColleagueRepository, IJobApplicationsRepository
    {
        public static char _VM = Convert.ToChar(DynamicArray.VM);

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
            var totalCount = 0;
            var jobApplicationEntities = new List<JobApplication>();
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

            totalCount = jobappsIds2.Count();
            jobappsIds2.Sort();

            var keysSubList = jobappsIds2.Skip(offset).Take(limit).ToArray().Distinct();
            
            if (keysSubList.Any())
            {
                var keysJobappsSubList = keysSubList.Select(k => k.Split('|')[0]).Distinct();
                var jobappsCollection = await DataReader.BulkReadRecordAsync<Jobapps>("JOBAPPS", keysJobappsSubList.ToArray());

                foreach (var key in keysSubList)
                {
                    var jobappsKey = key.Split('|')[0];
                    var indexKey = key.Split('|')[1];
                    var jobapps = jobappsCollection.FirstOrDefault(x => x.Recordkey == jobappsKey);

                    try
                    {
                        index = 0;
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
                        throw new Exception(e.Message);
                    }
                }
            }
            return new Tuple<IEnumerable<JobApplication>, int>(jobApplicationEntities, totalCount);

        }

        /// <summary>
        /// Returns a review for a specified Job Application key.
        /// </summary>
        /// <param name="ids">Key to Job Application to be returned</param>
        /// <returns>JobApplication Objects</returns>
        public async Task<JobApplication> GetJobApplicationByIdAsync(string id)
        {
            var jobApplicationId = await GetRecordInfoFromGuidAsync(id);

            if (jobApplicationId == null)
                throw new KeyNotFoundException();

            var jobapps = await DataReader.ReadRecordAsync<Jobapps>("JOBAPPS", jobApplicationId.PrimaryKey);

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

            throw new KeyNotFoundException(String.Format("No job application was found for guid '{0}'.", id));
        }

        #endregion

        /// <summary>
        /// Get the GUID for a entity using its ID
        /// </summary>
        /// <param name="id">entity ID</param>
        /// <param name="entity">entity</param>
        /// <returns>entity GUID</returns>
        public async Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            try
            {
                return await GetGuidFromRecordInfoAsync(entity, id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("perpos.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> GetIdFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordKeyFromGuidAsync(id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("review.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

        /// <summary>
        /// Gets id from guid input
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<GuidLookupResult> GetInfoFromGuidAsync(string id)
        {
            try
            {
                return await GetRecordInfoFromGuidAsync(id);
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (RepositoryException ex)
            {
                ex.AddError(new RepositoryError("review.guid.NotFound", "GUID not found for employment performance review " + id));
                throw ex;
            }
        }

    }

}
