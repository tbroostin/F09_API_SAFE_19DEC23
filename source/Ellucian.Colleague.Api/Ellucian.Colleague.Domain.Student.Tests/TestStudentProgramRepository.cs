// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities.Requirements.Modifications;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestStudentProgramRepository : IStudentProgramRepository
    {

        private string[,] studentPrograms = {
                                            //person id","program","guid","catalog","person id
                                                {"12345678","BA-MATH","bfde7c40-f27b-4747-bbd1-aab4b3b77bb9","2012","A","MC", "MATH", "HIST", "CERT",  "BA", "ELE", "2000RSU"},
                                                {"12345679","AA-NURS","45d8557f-56a9-4abc-8308-ee026983080c","2013", "P", "MC", "ENGL,MATH", "HIST,ACCT", "CERT,SCIE", "BA", "ELE,DI", "2000/S1" },
                                                {"12345680","MA-LAW","688583fc-6499-4a05-90b0-685745d6b465","2014", "G", "SBCD", "MATH", "HIST", "CERT",  "BA", "ELE", "2000CS1"},
                                                {"12345681","MS-SCI","6ceb37da-b617-4b4c-8737-a9cec24a548f","" , "", "SBCD", "", "", "" , "", "", ""}

                                                };

        private List<StudentProgram> studentProgEntities = new List<StudentProgram>();
        // First student's program is MATH.BS, Second student's program returned as ECON.BA
        public async Task<IEnumerable<StudentProgram>> GetStudentProgramsByIdsAsync(IEnumerable<string> studentIds, bool includeInactivePrograms = false, Term term = null, bool includeHistory = false)
        {
            string catcode = "2013";
            string progcode1 = "MATH.BS";
            string progcode2 = "ECON.BA";

            var studentId1 = studentIds.ElementAt(0);
            string studentId2 = (studentIds.Count() == 2) ? studentIds.ElementAt(1) : studentIds.ElementAt(0);
            StudentProgram sp1 = new StudentProgram(studentId1, progcode1, catcode);
            StudentProgram sp2 = new StudentProgram(studentId2, progcode2, catcode);

            IEnumerable<StudentProgram> studentPrograms = new List<StudentProgram>() { sp1, sp2 };
            return await Task.FromResult(studentPrograms);
        }

        // Two programs returned for this single student: MATH.BS and ECON.BA
        public async Task<IEnumerable<StudentProgram>> GetAsync(string studentid)
        {
            string catcode = "2013";
            string progcode1 = "MATH.BS";
            string progcode2 = "ECON.BA";

            StudentProgram sp1 = new StudentProgram(studentid, progcode1, catcode);
            StudentProgram sp2 = new StudentProgram(studentid, progcode2, catcode);

            Override o1 = new Override("100", new List<string>() { "1", "2" }, new List<string>() { "3", "4" });
            Override o2 = new Override("200", new List<string>() { "5" }, null);

            sp2.AddOverride(o1);
            sp2.AddOverride(o2);

            IEnumerable<StudentProgram> programs = new List<StudentProgram>() { sp1, sp2 };
            return await Task.FromResult(programs);

        }

        public IEnumerable<StudentProgram> Get(string studentid)
        {
            var task = this.GetAsync(studentid);
            return task.Result;
        }

        public async Task<StudentProgram> GetAcademicProgramEnrollmentByGuidAsync(string id)
        {
            var stuProgs = await GetAcademicProgramEnrollmentsAsync(false);
            var stuProg = studentProgEntities.FirstOrDefault(g => g.Guid == id);
            return await Task.FromResult<StudentProgram>(stuProg);
        }

        public IEnumerable<StudentProgram> GetStudentProgramsNoCache(string studentid)
        {
            string catcode = "2013";
            string progcode1 = "MATH.BS";
            string progcode2 = "ECON.BA";

            StudentProgram sp1 = new StudentProgram(studentid, progcode1, catcode);
            StudentProgram sp2 = new StudentProgram(studentid, progcode2, catcode);

            Override o1 = new Override("100", new List<string>() { "1", "2" }, new List<string>() { "3", "4" });
            Override o2 = new Override("200", new List<string>() { "5" }, null);

            sp2.AddOverride(o1);
            sp2.AddOverride(o2);

            IEnumerable<StudentProgram> programs = new List<StudentProgram>() { sp1, sp2 };
            return programs;

        }

        public async Task<StudentProgram> GetAsync(string studentid, string programid)
        {
            string catcode = "2013";
            StudentProgram sp = new StudentProgram(studentid, programid, catcode);
            return await Task.FromResult(sp);
        }

        public async Task<StudentProgram> GetAsync(string studentid, string programid, List<Override> overrides)
        {
            StudentProgram sp = await GetAsync(studentid, programid);
            foreach (var over in overrides)
            {
                sp.AddOverride(over);
            }
            return sp;
        }

        public async Task<StudentProgram> GetAsync(string studentid, string programid, List<Override> overrides, List<RequirementModification> modifications)
        {
            StudentProgram sp = await GetAsync(studentid, programid, overrides);
            foreach (var mod in modifications)
            {
                sp.AddRequirementModification(mod);
            }
            return sp;
        }


        public Task<IEnumerable<EvaluationNotice>> GetStudentProgramEvaluationNoticesAsync(string studentId, string programId)
        {
            throw new NotImplementedException();
        }

        public async Task<StudentProgram> CreateAcademicProgramEnrollmentAsync(StudentProgram acadProgEnroll)
        {
            StudentProgram sp1 = new StudentProgram("0001731", "MATH.BA", "2012");
            return sp1;

        }

        public async Task<StudentProgram> UpdateAcademicProgramEnrollmentAsync(StudentProgram acadProgEnroll)
        {
            StudentProgram sp1 = new StudentProgram("0001731", "MATH.BA", "2012");
            return sp1;
        }

        public Task<string> GetUnidataFormattedDate(string date)
        {
            throw new NotImplementedException();
        }
        public async Task<Tuple<IEnumerable<StudentProgram>, int>> GetAcademicProgramEnrollmentsAsync(int offset, int limit, bool bypassCache = false, string Program = "", string StartOn = "", string EndOn = "", string Student = "", string Catalog = "", string Status = "")
        {
            throw new NotImplementedException();
        }

        public async Task<List<StudentProgram>> GetStudentAcademicPeriodProfileStudentProgramInfoAsync(List<string> stuProgIds)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StudentProgram>> GetAcademicProgramEnrollmentsAsync(bool bypassCache = false)
        {
            var items = studentPrograms.Length / 13;

            for (int x = 0; x < items; x++)
            {

                var stuProgs = new StudentProgram(studentPrograms[x, 0], studentPrograms[x, 1], studentPrograms[x, 3], studentPrograms[x, 2]);
                stuProgs.Status = studentPrograms[x, 4];
                stuProgs.Location = studentPrograms[x, 5];
                stuProgs.StartTerm = studentPrograms[x, 11];
                stuProgs.StartDate = DateTime.Parse("01/06/2016");
                var major = new StudentMajors(studentPrograms[x, 6], "MAJOR1", DateTime.Parse("01/06/2016"), null);
                stuProgs.AddMajors(major);
                var minor = new StudentMinors(studentPrograms[x, 7], "MINOR1", DateTime.Parse("01/06/2016"), null);
                stuProgs.AddMinors(minor);
                var sp = new AdditionalRequirement(studentPrograms[x, 8], null, Domain.Student.Entities.Requirements.AwardType.Specialization, "sp1");
                stuProgs.AddAddlRequirement(sp);
                stuProgs.DegreeCode = studentPrograms[x, 9];
                var ccd = new AdditionalRequirement(studentPrograms[x, 10], null, Domain.Student.Entities.Requirements.AwardType.Ccd, "CCD1");
                stuProgs.AddAddlRequirement(ccd);
                studentProgEntities.Add(stuProgs);

            }
            return Task.FromResult<IEnumerable<Student.Entities.StudentProgram>>(studentProgEntities);
        }




    }
}


