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
        public async Task<SampleDegreePlan> GetAsync(string code)
        {
            return GetAllSampleDegreePlans().Where(c => c.Code == code).FirstOrDefault();
        }

        private string[,] sampleDegreePlans = {
                                            //CODE,   DESCRIPTION,     COURSE BLOCKS
                                            {"TRACK1", "BA MATH 2009", "1,2"}, 
                                            {"TRACK2", "BA MATH",      "3"},
                                            {"TRACK3", "Core",         "1,2,3"}
                                             };

        private string[,] sampleCourseBlocks = {
                                                 // CODE, DESCRIPTION, COURSE IDS
                                                 {"0","block 0","87"},
                                                 {"1","block 1","139,142"},
                                                 {"2","block 2","110,21"},
                                                 {"3","block 3","91,333,64"},
                                                 {"4","block 4","143,86"}
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
                    courseBlocks.Add(new CourseBlocks(sampleCourseBlocks[Int16.Parse(blockId), 1], sampleCourseBlocks[Int16.Parse(blockId), 2].Split(new char[] { ',' })));
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
            var items = sampleCourseBlocks.Length / 3;
            for (int x = 0; x < items; x++)
            {
                var courseIds = sampleCourseBlocks[x, 2].Split(new char[] { ',' });
                courseBlocks.Add(new CourseBlocks(sampleCourseBlocks[x, 1], courseIds));
            }
            return courseBlocks;
        }
    }
}