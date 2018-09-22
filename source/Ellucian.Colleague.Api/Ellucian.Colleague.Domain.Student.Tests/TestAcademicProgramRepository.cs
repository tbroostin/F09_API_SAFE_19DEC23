using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAcademicProgramRepository
    {

        private string[,] academicPrograms = {
                                        //CODE   DESCRIPTION  ACADEMIC LEVEL  DEGREE CODE  MAJORS  MINORS  SPECIALIZATIONS  CCDS
                                        {"BA-MATH", "Bachelor Math/Science", "UG", "BA", "MATH", "HIST", "CERT", "ELE","1c5bbbbc-80e3-4042-8151-db9893ac337a"},
                                        {"AA-NURS", "Associate Nursing", "UG", "AA", "HEAL,MEDI", "PHYS", "MEDI,HEAL", "HSCI", "17a21cdc-7912-459e-a065-03895471a644"},
                                        {"MA-LAW", "Master of Law", "GR", "MA", "LAW", "", "", "", "fbdfafd6-69a1-4362-88a0-62eac70da5c9"},
                                        {"MS-SCI", "Master of Science", "GR", "SCI","PHYS", "TECH", "", "" , "d328fd10-9c90-4a2f-b1a3-543bc099be37"}
                                      };


        public Task<IEnumerable<AcademicProgram>> GetAsync()
        {
            var acadPrograms = new List<AcademicProgram>();

            // There are 2 fields for each academicLevel in the array
            var items = academicPrograms.Length / 9;

            for (int x = 0; x < items; x++)
            {
                // Majors, may be multiple values separated by comma
                var cMajors = new List<string>();
                List<string> majors = academicPrograms[x, 4].Split(',').ToList();
                foreach (var d in majors)
                {
                    cMajors.Add(d);
                }

                // Minors, may be multiple values separated by comma
                var cMinors = new List<string>();
                List<string> minors = academicPrograms[x, 5].Split(',').ToList();
                foreach (var d in minors)
                {
                    cMinors.Add(d);
                }

                // Specializations, may be multiple values separated by comma
                var cSpecials = new List<string>();
                List<string> specials = academicPrograms[x, 6].Split(',').ToList();
                foreach (var d in specials)
                {
                    cSpecials.Add(d);
                }

                // CCDs, may be multiple values separated by comma
                var cCerts = new List<string>();
                List<string> certs = academicPrograms[x, 7].Split(',').ToList();
                foreach (var d in certs)
                {
                    cCerts.Add(d);
                }

                var acadProgram = new AcademicProgram(academicPrograms[x, 8], academicPrograms[x, 0], academicPrograms[x, 1])
                {
                    AcadLevelCode = academicPrograms[x, 2],
                    DegreeCode = academicPrograms[x, 3],
                    MajorCodes = cMajors,
                    MinorCodes = cMinors,
                    SpecializationCodes = cSpecials,
                    CertificateCodes = cCerts
                };

                acadPrograms.Add(acadProgram);
            }

            return Task.FromResult<IEnumerable<Student.Entities.AcademicProgram>>(acadPrograms);
        }
    }
}
