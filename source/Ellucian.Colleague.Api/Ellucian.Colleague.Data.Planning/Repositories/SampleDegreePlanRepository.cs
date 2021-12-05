// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
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

        /// <summary>
        /// Retrieve all curriculum track codes available for a student for a given academic program code.
        /// </summary>
        /// <param name="programCode">The code for the program for which the curriculum tracks should be returned.</param>
        /// <param name="catalogCode">The code for the catalog for which the curriculum tracks should be returned.</param>
        /// <returns>A collection of <see cref="CurriculumTrack">curriculum tracks</see> for a given program code and catalog code.</returns>
        public async Task<IEnumerable<CurriculumTrack>> GetProgramCatalogCurriculumTracksAsync(string programCode, string catalogCode)
        {
            var curriculumTracks = new List<CurriculumTrack>();

            //return curriculum track ids for the given program code and catalog code that are in the academic requirements list
            string fileKey = programCode + "*" + catalogCode;
            var acadProgramReqmt = await DataReader.ReadRecordAsync<AcadProgramReqmts>("ACAD.PROGRAM.REQMTS", fileKey);

            //No academic program was found for the given program code and catalog code (year)
            if (acadProgramReqmt == null)
            {
                logger.Info("Program '" + programCode + "' for catalog '" + catalogCode + "'" + " is missing an ACAD.PROGRAM.REQMTS record.");
                return curriculumTracks;
            }

            //An academic program was found for the given program code and catalog code (year), but no curriculum tracks have been assigned
            if (acadProgramReqmt.AcprCurriculumTrackIds == null || acadProgramReqmt.AcprCurriculumTrackIds.Count == 0)
            {
                logger.Info("No curriculum tracks have been assigned to the academic program '" + programCode + "' for the catalog '" + catalogCode + "'.");
                return curriculumTracks;
            }

            //Retrieve the curriculum track descriptions for the program/catalog curriculum track ids
            var bulkCurriculumTracksReadOutput = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<CurriculumTracks>("CURRICULUM.TRACKS", acadProgramReqmt.AcprCurriculumTrackIds.ToArray());

            //Log any invalid curriculum track pointers
            if (bulkCurriculumTracksReadOutput.InvalidKeys != null && bulkCurriculumTracksReadOutput.InvalidKeys.Length > 0)
            {
                logger.Info("Curriculum track records are missing for the following curriculum track ids: " +
                    string.Join(", ", bulkCurriculumTracksReadOutput.InvalidKeys));
            }

            var curriculumTracksData = bulkCurriculumTracksReadOutput.BulkRecordsRead;
            if (curriculumTracksData == null || !curriculumTracksData.Any())
            {
                logger.Info("No curriculum track records where found for the following curriculum track ids: " + 
                    string.Join(", ", acadProgramReqmt.AcprCurriculumTrackIds));
                return curriculumTracks;
            }

            foreach (var track in curriculumTracksData)
            {
                //filter out curriculum tracks that are not active (only include active tracks where start date is null or in the past and end date is null or in the future)
                if ((!track.CtkStartDate.HasValue || track.CtkStartDate.Value <= DateTime.Today) &&
                    (!track.CtkEndDate.HasValue || track.CtkEndDate.Value >= DateTime.Today))
                {
                    curriculumTracks.Add(new CurriculumTrack(track.Recordkey, track.CtkDesc));
                }
                else
                {
                    logger.Info(string.Format("Curriculum track with Id: {0} Start Date: {1} and End Date: {2} is not active and will not be returned.",
                        track.Recordkey,
                        track.CtkStartDate.HasValue ? track.CtkStartDate.Value.ToShortDateString() : "",
                        track.CtkEndDate.HasValue ? track.CtkEndDate.Value.ToShortDateString() : ""));
                }
            }
            return curriculumTracks;
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
                                var currBlock = new Ellucian.Colleague.Domain.Planning.Entities.CourseBlocks(
                                    description: courseBlockData.CblDesc,
                                    courseIds: courseBlockData.CblCourses.Where(c => (!string.IsNullOrEmpty(c))),
                                    coursePlaceholderIds: courseBlockData.CblCoursePlaceholders.Where(cp => (!string.IsNullOrEmpty(cp)))
                                    );
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
