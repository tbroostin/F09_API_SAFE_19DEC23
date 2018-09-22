using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestEducationHistoryRepository
    {
        private string[,] otherDegrees = {
                                            //CODE   DESCRIPTION
                                            {"BA", "Bachelor of Arts"}, 
                                            {"MA", "Master of Arts"},
                                            {"BS", "Bachelor of Science"}
                                      };

        public IEnumerable<OtherDegree> GetOtherDegrees()
        {
            var otherDegeesTable = new List<OtherDegree>();
            // There are 2 fields for each courseLevel in the array
            var items = otherMinor.Length / 2;
            for (int x = 0; x < items; x++)
            {
                otherDegeesTable.Add(new OtherDegree(otherMinor[x, 0], otherMinor[x, 1]));
            }
            return otherDegeesTable;
        }

        private string[,] otherMajors = {
                                            //CODE   DESCRIPTION
                                            {"MATH", "Math"}, 
                                            {"ENGL", "English"},
                                            {"HIST", "History"}
                                      };

        public IEnumerable<OtherMajor> GetOtherMajors()
        {
            var otherMajorTable = new List<OtherMajor>();
            // There are 2 fields for each courseLevel in the array
            var items = otherMinor.Length / 2;
            for (int x = 0; x < items; x++)
            {
                otherMajorTable.Add(new OtherMajor(otherMinor[x, 0], otherMinor[x, 1]));
            }
            return otherMajorTable;
        }

        private string[,] otherMinor = {
                                            //CODE   DESCRIPTION
                                            {"MATH", "Math"}, 
                                            {"ENGL", "English"},
                                            {"HIST", "History"}
                                      };

        public IEnumerable<OtherMinor> GetOtherMinors()
        {
            var otherMinorTable = new List<OtherMinor>();
            // There are 2 fields for each courseLevel in the array
            var items = otherMinor.Length / 2;
            for (int x = 0; x < items; x++)
            {
                otherMinorTable.Add(new OtherMinor(otherMinor[x, 0], otherMinor[x, 1]));
            }
            return otherMinorTable;
        }
    }
}
