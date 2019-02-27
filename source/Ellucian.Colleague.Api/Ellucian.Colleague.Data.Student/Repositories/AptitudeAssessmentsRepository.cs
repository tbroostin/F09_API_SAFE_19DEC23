// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Student.DataContracts;
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
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class AptitudeAssessmentsRepository : BaseColleagueRepository, IAptitudeAssessmentsRepository
    {
        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public AptitudeAssessmentsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        #region GET methods
        /// <summary>
        /// Returns tuple for AptitudeAssessment & total count
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<NonCourse>> GetAptitudeAssessmentsAsync(bool bypassCache)
        {
            string criteria = "WITH NCRS.CATEGORY.INDEX = 'A''P''T'";
            if (bypassCache)
            {
                Collection<NonCourses> nonCourseDataContracts = await DataReader.BulkReadRecordAsync<NonCourses>(criteria);
                var aptitudeAssessmentList = BuildAptitudeAssessments(nonCourseDataContracts);
                return aptitudeAssessmentList != null && aptitudeAssessmentList.Any() ? aptitudeAssessmentList.ToList() : null;
            }
            else
            {
                var aptitudeAssessmentEntities = await GetOrAddToCacheAsync<IEnumerable<NonCourse>>("AllNonCourses",
                      async () =>
                      {
                          Collection<NonCourses> nonCourseDataContracts = await DataReader.BulkReadRecordAsync<NonCourses>(criteria);
                          var aptitudeAssessmentList = BuildAptitudeAssessments(nonCourseDataContracts);
                          return aptitudeAssessmentList != null && aptitudeAssessmentList.Any() ? aptitudeAssessmentList.ToList() : new List<NonCourse>();
                      });
                return aptitudeAssessmentEntities;
            }
                
        }

        /// <summary>
        /// Get guid for AptitudeAssessments code
        /// </summary>
        /// <param name="code">AptitudeAssessments code</param>
        /// <returns>Guid</returns>
        public async Task<string> GetAptitudeAssessmentsGuidAsync(string code)
        {
            //get all the codes from the cache
            string guid = string.Empty;
            if (string.IsNullOrEmpty(code))
                return guid;
            var allCodesCache = await GetAptitudeAssessmentsAsync(false);
            NonCourse codeCache = null;
            if (allCodesCache != null && allCodesCache.Any())
            {
                codeCache = allCodesCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            //if we cannot find that code in the cache, then refresh the cache and try again.
            if (codeCache == null)
            {
                var allCodesNoCache = await GetAptitudeAssessmentsAsync(true);
                if (allCodesCache == null)
                {
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'NON.COURSES', Record ID:'", code, "'"));
                }
                var codeNoCache = allCodesNoCache.FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (codeNoCache != null && !string.IsNullOrEmpty(codeNoCache.Guid))
                    guid = codeNoCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'NON.COURSES', Record ID:'", code, "'"));
            }
            else
            {
                if (!string.IsNullOrEmpty(codeCache.Guid))
                    guid = codeCache.Guid;
                else
                    throw new RepositoryException(string.Concat("No Guid found, Entity:'NON.COURSES', Record ID:'", code, "'"));
            }
            return guid;
        }

        /// <summary>
        /// Get aptitude assessment by Id
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<NonCourse> GetAptitudeAssessmentByIdAsync(string guid)
        {
            var assessmentId = await GetRecordKeyFromGuidAsync(guid);
            if (string.IsNullOrEmpty(assessmentId))
            {
                throw new KeyNotFoundException("No aptitude assessment was found for GUID " + guid);
            }
            
            var aptitudeAssessmentDataContract = await DataReader.ReadRecordAsync<NonCourses>("NON.COURSES", assessmentId);
            if (aptitudeAssessmentDataContract == null)
            {
                throw new KeyNotFoundException("aptitude-assessments data contract not found for Id " + assessmentId);
            }

            var category = aptitudeAssessmentDataContract.NcrsCategoryIdx;
            if (category == "A" || category == "P" || category == "T")
            {

                var aptitudeAssessmentEntity = BuildAptitudeAssessment(aptitudeAssessmentDataContract);
                return aptitudeAssessmentEntity;
            }
            else
            {
                throw new KeyNotFoundException("No aptitude assessment was found for GUID " + guid);
            }
        }

        /// <summary>
        /// Gets all the guids for the aptitude assessment keys
        /// </summary>
        /// <param name="aptitudeAssessmentKeys"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetAptitudeAssessmentGuidsAsync(IEnumerable<string> aptitudeAssessmentKeys)
        {
            if (aptitudeAssessmentKeys != null && !aptitudeAssessmentKeys.Any())
            {
                return null;
            }
            string criteria = "WITH NCRS.CATEGORY.INDEX = 'A''P''T'";

            var aptitudeAssessmenIds = await DataReader.SelectAsync("NON.COURSES", criteria);

            var sublist = aptitudeAssessmentKeys.ToArray().Intersect(aptitudeAssessmenIds);

            var assessmentGuids = new Dictionary<string, string>();

            if (sublist != null && sublist.Any())
            {
                // convert the person keys to person guids
                var personGuidLookup = sublist.ToList().ConvertAll(p => new RecordKeyLookup("NON.COURSES", p, false)).ToArray();
                var recordKeyLookupResults = await DataReader.SelectAsync(personGuidLookup);
                foreach (var recordKeyLookupResult in recordKeyLookupResults)
                {
                    string[] splitKeys = recordKeyLookupResult.Key.Split(new[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!assessmentGuids.ContainsKey(splitKeys[1]))
                    {
                        assessmentGuids.Add(splitKeys[1], recordKeyLookupResult.Value.Guid);
                    }
                }
            }
            return (assessmentGuids != null && assessmentGuids.Any()) ? assessmentGuids : null;
        }
        #endregion

        #region Build methods
        /// <summary>
        /// Build AptitudeAssessment entities
        /// </summary>
        /// <param name="nonCourseDataContracts"></param>
        /// <returns></returns>
        private IEnumerable<NonCourse> BuildAptitudeAssessments(IEnumerable<NonCourses> nonCourseDataContracts)
        {
            List<NonCourse> aptitudeAssessmentList = new List<NonCourse>();

            foreach (var ncDataContract in nonCourseDataContracts)
            {
                NonCourse aptitudeAssessment = BuildAptitudeAssessment(ncDataContract);
                aptitudeAssessmentList.Add(aptitudeAssessment);
            }
            return aptitudeAssessmentList;
            //return (aptitudeAssessmentList != null && aptitudeAssessmentList.Any()) ? aptitudeAssessmentList : null;
        }

        /// <summary>
        /// Builds individual entity
        /// </summary>
        /// <param name="ncDataContract"></param>
        /// <returns></returns>
        private static NonCourse BuildAptitudeAssessment(NonCourses ncDataContract)
        {
            NonCourse aptitudeAssessment = new NonCourse(ncDataContract.RecordGuid, ncDataContract.Recordkey)
            {
                Title = string.IsNullOrEmpty(ncDataContract.NcrsShortTitle) ? ncDataContract.Recordkey : ncDataContract.NcrsShortTitle,
                Description = ncDataContract.NcrsDesc,
                ParentAssessmentId = ncDataContract.NcrsPrimaryNcrsId,
                ScoreMin = ncDataContract.NcrsMinScore,
                ScoreMax = ncDataContract.NcrsMaxScore,
                //No increment is enforced in Colleague, and as a result ScoreIncrement property can be published as 'null' when the validScores object is published for a test score.
                ScoreIncrement = null,
                CalculationMethod = ncDataContract.NcrsGradeUse,
                AssessmentTypeId = ncDataContract.NcrsCategory
            };
            return aptitudeAssessment;
        }
        #endregion
    }
}
