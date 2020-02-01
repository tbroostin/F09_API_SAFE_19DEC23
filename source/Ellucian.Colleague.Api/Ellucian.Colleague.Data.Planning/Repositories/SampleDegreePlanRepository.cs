// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using slf4net;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Utility;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SampleDegreePlanRepository : BaseColleagueRepository, ISampleDegreePlanRepository
    {
        public SampleDegreePlanRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        public async Task<SampleDegreePlan> GetAsync(string curriculumTrackCode)
        {
            var sampleDegreePlan = await GetOrAddToCacheAsync<SampleDegreePlan>("SampleDegreePlan*" + curriculumTrackCode,
            async () =>
            {
                SampleDegreePlan samplePlan = await BuildSampleDegreePlanAsync(curriculumTrackCode);
                return samplePlan;
            }
            );
            return sampleDegreePlan;
        }

        private async Task<SampleDegreePlan> BuildSampleDegreePlanAsync(string code)
        {
            SampleDegreePlan sampleDegreePlan = null;

            var currTrackRepoData = await DataReader.ReadRecordAsync<CurriculumTracks>("CURRICULUM.TRACKS", code);
            if (currTrackRepoData == null || string.IsNullOrEmpty(currTrackRepoData.Recordkey))
            {
                throw new ArgumentOutOfRangeException("No Curriculum Track record found for Id " + code);
            }

            var courseBlockIds = new List<string>();
            // Make sure this curriculum track has a current effective date
            if (currTrackRepoData.CtkStartDate <= DateTime.Today && (currTrackRepoData.CtkEndDate == null || currTrackRepoData.CtkEndDate == DateTime.MaxValue || currTrackRepoData.CtkEndDate >= DateTime.Today))
            {
                // Keep a map of curriculum tracks and blocks
                courseBlockIds.AddRange(currTrackRepoData.CtkCourseBlocks);
            }
            // Get course blocks pointed to by each curriculum track
            var courseBlockRepoData = new Collection<Student.DataContracts.CourseBlocks>();
            if (courseBlockIds != null && courseBlockIds.Count() > 0)
            {
                courseBlockRepoData = await DataReader.BulkReadRecordAsync<Student.DataContracts.CourseBlocks>("COURSE.BLOCKS", courseBlockIds.ToArray());
            }

            // Build course blocks that appear in this track
            var courseBlocks = new List<Ellucian.Colleague.Domain.Planning.Entities.CourseBlocks>();
            if (courseBlockRepoData != null)
            {
                foreach (var courseBlockId in currTrackRepoData.CtkCourseBlocks)
                {
                    var courseBlockData = courseBlockRepoData.Where(c => c.Recordkey == courseBlockId).FirstOrDefault();
                    if (courseBlockData != null)
                    {
                        // Make sure block has current dates
                        if (courseBlockData.CblStartDate <= DateTime.Today && (courseBlockData.CblEndDate == null || courseBlockData.CblEndDate == DateTime.MaxValue || courseBlockData.CblEndDate >= DateTime.Today))
                        {
                            try
                            {
                                var currBlock = new Ellucian.Colleague.Domain.Planning.Entities.CourseBlocks(courseBlockData.CblDesc, courseBlockData.CblCourses.Where(c => (!string.IsNullOrEmpty(c))));
                                courseBlocks.Add(currBlock);
                            }
                            catch (Exception ex)
                            {
                                var error = "Unable to build Course Block record";
                                LogDataError("Course Block", courseBlockData.Recordkey, courseBlockData, ex, error);
                            }
                        }
                    }
                }
            }
            try
            {
                // Course blocks are assembled; build the sample degree plan item
                sampleDegreePlan = new SampleDegreePlan(currTrackRepoData.Recordkey, currTrackRepoData.CtkDesc, courseBlocks);
            }
            catch (Exception ex)
            {
                var error = "Unable to build Sample Degree Plan from curriculum track data";
                LogDataError("Sample Degree Plan", currTrackRepoData.Recordkey, currTrackRepoData, ex, error);
                throw new Exception("Error constructing SampleDegreePlan");
            }
            return sampleDegreePlan;
        }
    }
}
