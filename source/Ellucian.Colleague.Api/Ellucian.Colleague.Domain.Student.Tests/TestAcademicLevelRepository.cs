using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAcademicLevelRepository
    {

        private string[,] academicLevels = {
                                        //CODE   DESCRIPTION
                                        {"01de0349-1a28-49da-b985-f0cd161499b5", "CE", "Continuing Education"},
                                        {"7509fa18-3fea-45f6-90e8-aee03f61728c", "GR", "Graduate"},
                                        {"1bcdc5ae-944e-462d-a0fc-d83e7b0dce8c", "JD", "Law"},
                                        {"558ca14c-718a-4b6e-8d92-77f498034f9f", "UG", "Undergraduate"}
                                      };


        public Task<IEnumerable<AcademicLevel>> GetAsync()
        {
            var acadLevels = new List<AcademicLevel>();

            // There are 3 fields for each academicLevel in the array
            var items = academicLevels.Length / 3;

            for (int x = 0; x < items; x++)
            {
                acadLevels.Add(new AcademicLevel(academicLevels[x, 0], academicLevels[x, 1], academicLevels[x, 2]) { GradeScheme = academicLevels[x, 1] });
            }

            return Task.FromResult<IEnumerable<Student.Entities.AcademicLevel>>(acadLevels);

        }
       
    }
}
