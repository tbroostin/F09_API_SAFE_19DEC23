// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentAcademicProgramRepository : IStudentAcademicProgramRepository
    {

        private string[,] StudentAcademicPrograms = {
                                            //person id","program","guid","catalog","person id
                                                {"12345678","BA-MATH","bfde7c40-f27b-4747-bbd1-aab4b3b77bb9","2012","A","MC", "MATH", "HIST", "CERT", "BA", "ELE", "2000RSU", "01/01/2001","ENG","UG","3.4","CL","05/15/2014","06/15/2014","THESIS","100", "12/31/2001"},
                                                {"12345678","AA-NURS","45d8557f-56a9-4abc-8308-ee026983080c","2013", "P", "MC", "ENGL", "HIST", "CERT", "BA", "ELE", "2000/S1", "01/01/2001", "MATH", "GR","2.0", "FE","12/31/2015","06/15/2015","THESIS1","200", ""},
                                                {"12345678","MA-LAW","688583fc-6499-4a05-90b0-685745d6b465","2014", "G", "SBCD", "MATH", "HIST", "CERT",  "BA", "ELE", "2000CS1", "01/01/2001", "ART", "GR","","","","","","", ""},
                                                {"12345678","MS-SCI","6ceb37da-b617-4b4c-8737-a9cec24a548f","2012" , "P", "SBCD", "", "", "" , "", "", "2000CS1", "01/01/2001", "ART", "UG","","","","","","", ""}
                                                
                                                };
        private string defaultInstitution = "0000043";

        private List<StudentAcademicProgram> studentProgEntities = new List<StudentAcademicProgram>();

        public Dictionary<string, string> EthosExtendedDataDictionary
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        // First student's program is MATH.BS, Second student's program returned as ECON.BA
        public async Task<StudentAcademicProgram> GetStudentAcademicProgramByGuidAsync(string id, string defaultInstitutionId)
        {
            var stuProgs = await GetStudentAcademicProgramsAsync(false);
            var stuProg = studentProgEntities.FirstOrDefault(g => g.Guid == id);
            return await Task.FromResult<StudentAcademicProgram>(stuProg);
        }

        // First student's program is MATH.BS, Second student's program returned as ECON.BA
        public async Task<StudentAcademicProgram> GetStudentAcademicProgramByGuid2Async(string id, string defaultInstitutionId)
        {
            var stuProgs = await GetStudentAcademicProgramsAsync(false);
            var stuProg = studentProgEntities.FirstOrDefault(g => g.Guid == id);
            return await Task.FromResult<StudentAcademicProgram>(stuProg);
        }

        public async Task<StudentAcademicProgram> CreateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId)
        {
            StudentAcademicProgram sp1 = new StudentAcademicProgram("0001731", "MATH.BA", "2012", "6ceb37da-b617-4b4c-8737-a9cec24a548f", DateTime.Parse("01/06/2016"),"active");
            return sp1;

        }

        public async Task<StudentAcademicProgram> UpdateStudentAcademicProgramAsync(StudentAcademicProgram acadProgEnroll, string defaultInstitutionId)
        {
            StudentAcademicProgram sp1 = new StudentAcademicProgram("0001731", "MATH.BA", "2012", "6ceb37da-b617-4b4c-8737-a9cec24a548f", DateTime.Parse("01/06/2016"), "active");
            return sp1;
        }

        public Task<string> GetUnidataFormattedDate(string date)
        {
            throw new NotImplementedException();
        }
        public async Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicProgramsAsync(string defaultInstitutionId, int offset, int limit, bool bypassCache = false, 
            string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "",
            string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", string ccdCredential = "", string degreeCredential = "", string graduatedAcademicPeriod = "", string completeStatus = "")
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentAcademicProgram>> GetStudentAcademicProgramsAsync(bool bypassCache = false)
        {
            var items = StudentAcademicPrograms.Length / 21;

            for (int x = 0; x < items; x++)
            {

                var stuProgs = new StudentAcademicProgram(StudentAcademicPrograms[x, 0], StudentAcademicPrograms[x, 1], StudentAcademicPrograms[x, 3], StudentAcademicPrograms[x, 2], DateTime.Parse(StudentAcademicPrograms[x, 12]), StudentAcademicPrograms[x, 4]);
                stuProgs.Location = StudentAcademicPrograms[x, 5];
                stuProgs.StartTerm = StudentAcademicPrograms[x, 11];
                stuProgs.DepartmentCode = StudentAcademicPrograms[x, 13];
                stuProgs.AcademicLevelCode = StudentAcademicPrograms[x, 14];
                if (!string.IsNullOrEmpty(StudentAcademicPrograms[x, 15]))
                stuProgs.GradGPA = decimal.Parse(StudentAcademicPrograms[x, 15]);
                if (!string.IsNullOrEmpty(StudentAcademicPrograms[x, 17]))
                stuProgs.GraduationDate = DateTime.Parse(StudentAcademicPrograms[x, 17]);
                if (!string.IsNullOrEmpty(StudentAcademicPrograms[x, 18]))
                stuProgs.CredentialsDate = DateTime.Parse(StudentAcademicPrograms[x, 18]);
                stuProgs.ThesisTitle = StudentAcademicPrograms[x, 19];
                if (!string.IsNullOrEmpty(StudentAcademicPrograms[x, 20]))
                stuProgs.CreditsEarned = decimal.Parse(StudentAcademicPrograms[x, 20]);
                stuProgs.DegreeCode = StudentAcademicPrograms[x, 9];
                stuProgs.AddMajors(StudentAcademicPrograms[x, 6]);
                stuProgs.AddMinors(StudentAcademicPrograms[x, 7]);
                stuProgs.AddSpecializations(StudentAcademicPrograms[x, 8]);
                stuProgs.AddCcds(StudentAcademicPrograms[x, 10]);
                stuProgs.AddHonors(StudentAcademicPrograms[x, 16]);
                stuProgs.EndDate = string.IsNullOrEmpty((StudentAcademicPrograms[x, 21]))? default(DateTime?) : DateTime.Parse(StudentAcademicPrograms[x, 21]);
                studentProgEntities.Add(stuProgs);

            }
            return Task.FromResult<IEnumerable<Student.Entities.StudentAcademicProgram>>(studentProgEntities);

        }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicProgramsAsync(string defaultInstitutionId, int offset, int limit, bool bypassCache = false, string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "", string completeStatus = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms2Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false, string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "", string completeStatus = "")
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms2Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false, string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "", string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<IEnumerable<StudentAcademicProgram>, int>> GetStudentAcademicPrograms3Async(string defaultInstitutionId, int offset, int limit, bool bypassCache = false, string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "", string programOwner = "", string site = "", string academicLevel = "", string graduatedOn = "", List<string> ccdCredential = null, List<string> degreeCredential = null, string graduatedAcademicPeriod = "", string completeStatus = "", CurriculumObjectiveCategory curriculumObjective = CurriculumObjectiveCategory.NotSet)
        {
            throw new NotImplementedException();
        }
    }
}