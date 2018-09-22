using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestCourseLevelRepository
    {
        private string[,] courseLevels = {
                                            //CODE   DESCRIPTION
                                            {"100", "First Year"}, 
                                            {"200", "Second Year"},
                                            {"300", "Third Year"},
                                            {"400", "Fourth Year"}, 
                                            {"500", "Graduate"}
                                      };


        public IEnumerable<CourseLevel> Get()
        {
            var crsLevels = new List<CourseLevel>();

            // There are 2 fields for each courseLevel in the array
            var items = courseLevels.Length / 2;

            for (int x = 0; x < items; x++)
            {
                crsLevels.Add(new CourseLevel(Guid.NewGuid().ToString(), courseLevels[x, 0], courseLevels[x, 1]));
            }
            return crsLevels;
        }
    }
}
