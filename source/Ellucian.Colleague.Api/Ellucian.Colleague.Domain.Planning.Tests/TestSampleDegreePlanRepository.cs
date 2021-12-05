// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using System.Threading.Tasks;



namespace Ellucian.Colleague.Domain.Planning.Tests
{
    public class TestSampleDegreePlanRepository : ISampleDegreePlanRepository
    {

        public async Task<IEnumerable<CurriculumTrack>> GetProgramCatalogCurriculumTracksAsync(string programCode, string catalogCode)
        {
            throw new NotImplementedException();
        }


        public async Task<SampleDegreePlan> GetAsync(string code)
        {
            return GetAllSampleDegreePlans().Where(c => c.Code == code).FirstOrDefault();
        }

        private string[,] sampleDegreePlans = {
            // CODE,   DESCRIPTION,     COURSE BLOCKS
            {"TRACK1", "BA MATH 2009", "1,2"},
            {"TRACK2", "BA MATH",      "3"},
            {"TRACK3", "Core",         "1,2,3"},
            {"TRACK4", "BA MUSC",      "0,5,6"},
            {"TRACK5", "MA MUSC",      "6,7"}
                };

        private string[,] sampleCourseBlocks = {
            // CODE, DESCRIPTION, COURSE IDS, PLACEHOLDER IDS
            {"0","block 0","87", ""},
            {"1","block 1","139,142", ""},
            {"2","block 2","110,21", ""},
            {"3","block 3","91,333,64", ""},
            {"4","block 4","143,86", ""},
            {"5","block 5","139,142", "MUSC-100,MUSC-101"},
            {"6","block 6","", "MUSC-200,MUSC-201,MUSC-202"},
            {"7","block 7","", "MUSC-500,MUSC-501"},
        };

        public IEnumerable<SampleDegreePlan> GetAllSampleDegreePlans()
        {
            var samplePlans = new List<SampleDegreePlan>();
            var items = sampleDegreePlans.Length / 3;
            for (int x = 0; x < items; x++)
            {
                // Add each block from the course blocks "repository"
                var courseBlockIds = sampleDegreePlans[x, 2].Split(new char[] { ',' });
                var courseBlocks = new List<CourseBlocks>();
                foreach (var blockId in courseBlockIds)
                {
                    var id = Int16.Parse(blockId);
                    var courseIds = sampleCourseBlocks[id, 2].Split(new char[] { ',' }).Where(c => (!string.IsNullOrWhiteSpace(c))).ToArray();
                    var coursePlaceholderIds = sampleCourseBlocks[id, 3].Split(new char[] { ',' }).Where(cp => (!string.IsNullOrWhiteSpace(cp))).ToArray();
                    if (courseIds.Length > 0 || coursePlaceholderIds.Length > 0)
                        courseBlocks.Add(new CourseBlocks(sampleCourseBlocks[id, 1], courseIds, coursePlaceholderIds));
                }

                // Create sample degree plan
                var samplePlan = new SampleDegreePlan(sampleDegreePlans[x, 0], sampleDegreePlans[x, 1], courseBlocks);

                samplePlans.Add(samplePlan);
            }
            return samplePlans;
        }

        public IEnumerable<CourseBlocks> GetCourseBlocks()
        {
            var courseBlocks = new List<CourseBlocks>();
            var items = sampleCourseBlocks.Length / 4;
            for (int x = 0; x < items; x++)
            {
                var courseIds = sampleCourseBlocks[x, 2].Split(new char[] { ',' }).Where(c => (!string.IsNullOrWhiteSpace(c))).ToArray();
                var coursePlaceholderIds = sampleCourseBlocks[x, 3].Split(new char[] { ',' }).Where(cp => (!string.IsNullOrWhiteSpace(cp))).ToArray();
                if (courseIds.Length > 0 || coursePlaceholderIds.Length > 0)
                    courseBlocks.Add(new CourseBlocks(sampleCourseBlocks[x, 1], courseIds, coursePlaceholderIds: coursePlaceholderIds));
            }
            return courseBlocks;
        }
    }
}