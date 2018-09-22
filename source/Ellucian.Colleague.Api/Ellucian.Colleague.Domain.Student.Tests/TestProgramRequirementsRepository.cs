// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestProgramRequirementsRepository : IProgramRequirementsRepository
    {
        private TestCourseRepository courserepo;
        private TestGradeRepository graderepo;
        private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

        public async Task<Requirement> GetRequirementAsync(string id)
        {
            // For now
            return (await GetAsync(id, "2012")).Requirements.First();
        }

        /// <summary>
        /// This method is used by various unit test classes to build program requirements scenarios for the automated tests. The input to this method is the ID of a program that
        /// must exist somewhere in the case statement and any catalog ID. Each case constructs the basic program requirements needed for the specific test scenario,
        /// along with the subrequirements and groups needed. The subrequirements and groups can be set up manually in the case or can be set up for automatic generation
        /// by providing requirement, subrequirement and group names to the method BuildProgramAndRequirements (see that method for details)
        /// </summary>
        /// <param name="id">Id of the program (required and must be in case statement)</param>
        /// <param name="cat">Id of the catalog</param>
        /// <returns>Program Requirements as specified</returns>
        public async Task<ProgramRequirements> GetAsync(string id, string cat)
        {
            // This method will give you a fully-baked PR object with whatever I feel like putting in it.
            graderepo = new TestGradeRepository();
            IEnumerable<Grade> grades = await graderepo.GetAsync();

            switch (id)
            {
                case "":
                    {
                        throw new NotSupportedException("Calling Testprogramrequirmentsrepository with empty string id");
                    }
                case "EmptyRequirements":
                    {
                        ProgramRequirements pr = new ProgramRequirements(id, cat);
                        pr.Requirements = new List<Requirement>();
                        pr.ActivityEligibilityRules = new List<RequirementRule>();
                        pr.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("STCUSERX")));
                        pr.MaximumCredits = null;
                        pr.MinGrade = grades.Where(g => g.Id == "D").First();
                        pr.MinimumInstitutionalCredits = 40m;
                        pr.MinInstGpa = 2m;
                        pr.MinimumCredits = 120;
                        pr.MinOverallGpa = 2.1m;
                        pr.AllowedGrades = grades.Where(g => g.Id == "P" || g.Id == "AU").ToList();
                        pr.CurriculumTrackCode = "TRACK1";
                        return pr;
                    }
                case "MATH.BS.BLANKTAKEONE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHGROUPRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.AcademicCreditRules.Add(new RequirementRule(new Rule<Course>("MATH100")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHSUBRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        // Group just says "take 1 course" so we can see what it gets. Test56 a misnomer in this case
                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        pr.Requirements.First().SubRequirements.First().AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("MATH100")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHREQRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        // Group just says "take 1 course" so we can see what it gets. Test56 a misnomer in this case
                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        pr.Requirements.First().AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("MATH100")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHPROGRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        // Group just says "take 1 course" so we can see what it gets. Test56 a misnomer in this case
                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        pr.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("MATH100")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHGROUPCREDRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("BIOL");
                        group1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("NEWSTAT")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHSUBCREDRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("BIOL");
                        pr.Requirements.First().SubRequirements.First().AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("NEWSTAT")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHREQCREDRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("BIOL");
                        pr.Requirements.First().AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("NEWSTAT")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "MATH.BS.WITHPROGCREDRULE":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        Group group1 = new Group("Test56", "Test56", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("BIOL");
                        pr.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("NEWSTAT")));
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }

                case "MATH.BS.TOBEOPTIMIZED":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        var removeme = pr.Requirements.First().SubRequirements.Last();
                        pr.Requirements.First().SubRequirements.Remove(removeme);


                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        // TAKE ENGL*101, ENGL*102 
                        Group group1 = new Group("Test3", "Test3", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10003";
                        group1.Courses.Add("130");
                        group1.Courses.Add("187");
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        // TAKE 1 COURSE; MAXIMUM 3 CREDITS
                        Group group2 = new Group("Test32", "Test32", pr.Requirements.First().SubRequirements.First());
                        group2.Id = "10032";
                        group2.MinCourses = 1;
                        //group2.MaxCredits = 3;
                        group2.InternalType = "33";
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group2);

                        pr.Requirements.First().SubRequirements.First().MinGroups = 1;

                        return pr;
                    }
                case "MATH.BS.TOBEOPTIMIZED2":
                    {

                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();

                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.Last().Groups.Clear();


                        // TAKE ENGL*101, ENGL*102 
                        Group group1 = new Group("Test3", "Test3", pr.Requirements.First().SubRequirements.First());
                        group1.Id = "10003";
                        group1.Courses.Add("130");
                        group1.Courses.Add("187");
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        // TAKE 1 COURSE; MAXIMUM 3 CREDITS
                        Group group2 = new Group("Test32", "Test32", pr.Requirements.First().SubRequirements.Last());
                        group2.Id = "10032";
                        group2.MinCourses = 1;
                        //group2.MaxCredits = 3;
                        group2.InternalType = "33";
                        pr.Requirements.First().SubRequirements.Last().Groups.Add(group2);

                        pr.Requirements.First().MinSubRequirements = 1;



                        return pr;
                    }

                case "STSS.MATH.BS*2010":
                    {
                        string reqname = id;
                        List<string> requirementNames = new List<string>() { "STSS.CORE.REQ", "STSS.MATH.MAJ", "STSS.CORE.HUM" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();

                        List<string> Subreqs = new List<string>() { "STSS.CORE.SUB.ENG", "STSS.CORE.SUB.MATH" };
                        SubrequirementNames.Add("STSS.CORE.REQ", Subreqs);

                        List<string> Subreq2 = new List<string>() { "STSS.MATH.PRAC.SUB", "STSS.MATH.APPL.SUB", "STSS.MATH.THRY.SUB" };
                        SubrequirementNames.Add("STSS.MATH.MAJ", Subreq2);

                        List<string> Subreq3 = new List<string>() { "STSS.CORE.HUM.HU", "STSS.CORE.HUM.COMM" };
                        SubrequirementNames.Add("STSS.CORE.HUM", Subreq3);

                        // for now groups are 1:1 with Subrequirements so this will work
                        groupNames.Add("STSS.CORE.SUB.ENG", new List<string>() { "20886" });
                        groupNames.Add("STSS.CORE.SUB.MATH", new List<string>() { "20888" });
                        groupNames.Add("STSS.MATH.PRAC.SUB", new List<string>() { "20892" });
                        groupNames.Add("STSS.MATH.APPL.SUB", new List<string>() { "20895" });
                        groupNames.Add("STSS.MATH.THRY.SUB", new List<string>() { "20897" });
                        groupNames.Add("STSS.CORE.HUM.HU", new List<string>() { "20900" });
                        groupNames.Add("STSS.CORE.HUM.COMM", new List<string>() { "20902" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);

                        // add a few other elements

                        pr.MinimumCredits = 120;
                        pr.MinimumInstitutionalCredits = 90;
                        pr.MaximumCredits = 150;
                        pr.MinOverallGpa = 2.0m;
                        pr.MinInstGpa = 2.2m;

                        pr.Requirements.First(rq => rq.Code == "STSS.CORE.REQ").RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                        pr.Requirements.First(rq => rq.Code == "STSS.CORE.REQ")
                          .SubRequirements.First(sr => sr.Code == "STSS.CORE.SUB.MATH").MinGrade = grades.First(grd => grd.LetterGrade == "C");
                        pr.Requirements.First(rq => rq.Code == "STSS.CORE.REQ")
                          .SubRequirements.First(sr => sr.Code == "STSS.CORE.SUB.MATH").AllowedGrades.Add(grades.First(grd => grd.LetterGrade == "P"));

                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").RequirementType = requirementTypes.First(rt => rt.Code == "MAJ");
                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").Exclusions.Add("MAJ");
                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").Exclusions.Add("MIN");
                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").Exclusions.Add("ELE");
                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").Exclusions.Add("GEN");
                        pr.Requirements.First(rq => rq.Code == "STSS.MATH.MAJ").MinGpa = 3.0m;


                        pr.Requirements.First(rq => rq.Code == "STSS.CORE.HUM").RequirementType = requirementTypes.First(rt => rt.Code == "ELE");
                        pr.Requirements.First(rq => rq.Code == "STSS.CORE.HUM").MinSubRequirements = 1;


                        return pr;
                    }

                case "MATH.BS.GROUP43":
                    {


                        // Get a basic requirements tree
                        ProgramRequirements pr = await GetAsync("BS.MATH", "2010");


                        //var removeme = pr.Requirements.Last();
                        //pr.Requirements.Remove(removeme);

                        var removemesub = pr.Requirements.First().SubRequirements.Last();
                        pr.Requirements.First().SubRequirements.Remove(removemesub);


                        // Cut out the groups and put in a particular one
                        pr.Requirements.First().SubRequirements.First().Groups.Clear();


                        Group group1 = await BuildGroupAsync("Group43", "Group43", pr.Requirements.First().SubRequirements.First());
                        pr.Requirements.First().SubRequirements.First().Groups.Add(group1);

                        return pr;
                    }
                case "SIMPLE":
                    {
                        // Simple program, group requires at least 10 courses from any course level
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }
                case "SIMPLE1":
                    {
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "500ONLY" };
                        groupNames.Add(Subreqs[0], groups1);
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }

                case "EXCLUDED_REQ":
                    {
                        // Simple program with two requirements, reqs do not share with other reqs of the excluded type
                        string progname = id + "*" + cat;
                        string reqname1 = progname + "1";
                        string reqname2 = progname + "2";
                        List<string> requirementNames = new List<string>() { reqname1, reqname2 };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname1, Subreqs);
                        SubrequirementNames.Add(reqname2, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY", "500ONLY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(progname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                            req.Exclusions.Add("GEN");
                        }
                        return programRequirements;
                    }

                case "EXCLUDED_REQ2":
                    {
                        // Simple program with two requirements, reqs do not share with other reqs of the excluded type - different groups to test overrides.
                        string progname = id + "*" + cat;
                        string reqname1 = progname + "1";
                        string reqname2 = progname + "2";
                        List<string> requirementNames = new List<string>() { reqname1, reqname2 };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs1 = new List<string>() { "Subreq1" };
                        List<string> Subreqs2 = new List<string>() { "Subreq2" };
                        SubrequirementNames.Add(reqname1, Subreqs1);
                        SubrequirementNames.Add(reqname2, Subreqs2);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY", "500ONLY" };
                        List<string> groups2 = new List<string>() { "100-200ONLY", "500ONLY" };
                        groupNames.Add(Subreqs1[0], groups1);
                        groupNames.Add(Subreqs2[0], groups2);
                        var programRequirements = await BuildTestProgramRequirementsAsync(progname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                            req.Exclusions.Add("GEN");
                        }
                        return programRequirements;
                    }

                case "EXCLUDED_REQ3":
                    {
                        // Simple program with two requirements, reqs do not share with other reqs of the excluded type
                        string progname = id + "*" + cat;
                        string reqname1 = progname + "1";
                        string reqname2 = progname + "2";
                        List<string> requirementNames = new List<string>() { reqname1, reqname2 };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname1, Subreqs);
                        SubrequirementNames.Add(reqname2, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY"};
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(progname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                            req.Exclusions.Add("GEN");
                        }
                        return programRequirements;
                    }
                case "EXCLUDED_REQ_WITH_TAKEALL":
                    {
                        // Simple program with two requirements, reqs DO share with other reqs of excluded type
                        string progname = id + "*" + cat;
                        string reqname1 = progname + "1";
                        string reqname2 = progname + "2";
                        List<string> requirementNames = new List<string>() { reqname1, reqname2 };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname1, Subreqs);
                        SubrequirementNames.Add(reqname2, Subreqs);
                        List<string> groups1 = new List<string>() { "MUSC-209" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(progname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                            req.Exclusions.Add("GEN");
                        }
                        return programRequirements;
                    }

                case "EXCLUDED_REQ_WITH_TAKEALL_AND_TAKECREDITS":
                    {
                        // Simple program with two requirements, reqs DO share with other reqs of excluded type when TAKEALL, DONT share with NON-TAKEALL
                        string progname = id + "*" + cat;
                        string reqname1 = progname + "1";
                        string reqname2 = progname + "2";
                        List<string> requirementNames = new List<string>() { reqname1, reqname2 };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname1, Subreqs);
                        SubrequirementNames.Add(reqname2, Subreqs);
                        List<string> groups1 = new List<string>() { "MUSC-209", "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(progname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                            req.Exclusions.Add("GEN");
                        }
                        return programRequirements;
                    }

                case "MINGRADE_AND_ALLOWEDGRADES":
                    {
                        // Program with a different min grade and allowed grades and req, subreq and group levels
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "D").First();
                            req.AllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "AU").First());
                            req.AllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "P").First());
                            foreach (var subReq in req.SubRequirements)
                            {
                                subReq.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "C").First();
                                var subreqAllowedGrades = new List<Grade>();
                                subreqAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "W").First());
                                subreqAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "F").First());
                                subreqAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "1").First());
                                subReq.AllowedGrades = subreqAllowedGrades;
                                foreach (var grp in subReq.Groups)
                                {
                                    grp.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "B").First();
                                    var groupAllowedGrades = new List<Grade>();
                                    groupAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "S").First());
                                    groupAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "WF").First());
                                    grp.AllowedGrades = groupAllowedGrades;
                                }
                            }
                        }
                        return programRequirements;
                    }

                case "ALLOWED_GRADES":
                    {
                        // Program with a different min grade and allowed grades and req, subreq and group levels
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "C").First();
                            req.AllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "P").First());
                            foreach (var subReq in req.SubRequirements)
                            {
                                subReq.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "C").First();
                                var subreqAllowedGrades = new List<Grade>();
                                subreqAllowedGrades.Add((await graderepo.GetAsync()).Where(g => g.Id == "AU").First());
                                subReq.AllowedGrades = subreqAllowedGrades;
                            }
                        }
                        return programRequirements;
                    }

                case "MIN_GRADE_B":
                    {
                        // Program includes low grades in GPA calculation by default even though min grade set to B
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var programRequirements = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                        programRequirements.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "B").First();
                        foreach (var req in programRequirements.Requirements)
                        {
                            req.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "B").First();
                            foreach (var subReq in req.SubRequirements)
                            {
                                subReq.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "B").First();
                                foreach (var grp in subReq.Groups)
                                {
                                    grp.MinGrade = (await graderepo.GetAsync()).Where(g => g.Id == "B").First();
                                }
                            }
                        }
                        return programRequirements;
                    }

                case "MINGRADE_ALLOWEDGRADE_TOPLEVEL":
                    {
                        // Builds program requirements with min grade and allowed grades at only the program requirement
                        // level. Used in a test to prove that min/allowed grades are cascaded down to every level in
                        // the repository
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> groups1 = new List<string>() { "SIMPLE.ANY" };
                        groupNames.Add(Subreqs[0], groups1);
                        var pr = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                        pr.MinGrade = grades.Where(g => g.Id == "D").First();
                        pr.AllowedGrades = grades.Where(g => g.Id == "P" || g.Id == "AU").ToList();
                        return pr;
                    }

                case "MULTIPLE_REQTYPE":
                    {
                        // By default, these requirements to not allow reuse of credits
                        string reqname = id;
                        List<string> requirementNames = new List<string>() { "REQU1", "REQU2" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();

                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add("REQU1", Subreqs);

                        List<string> Subreq2 = new List<string>() { "Subreq2" };
                        SubrequirementNames.Add("REQU2", Subreq2);

                        // The group will simply take any credit--assign this to both subreqs
                        groupNames.Add("Subreq1", new List<string>() { "SIMPLE.ANY" });
                        groupNames.Add("Subreq2", new List<string>() { "SIMPLE.ANY" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);

                        pr.Requirements.First(rq => rq.Code == "REQU1").RequirementType = requirementTypes.First(rt => rt.Code == "GEN"); // priority 6
                        // Credits will be applied to this requirement's group and will not be allowed to be applied to the other.
                        pr.Requirements.First(rq => rq.Code == "REQU2").RequirementType = requirementTypes.First(rt => rt.Code == "MIN"); // priority 2

                        // Neither allows sharing with each other
                        pr.Requirements.First(rq => rq.Code == "REQU1").Exclusions.Add("MIN");
                        pr.Requirements.First(rq => rq.Code == "REQU2").Exclusions.Add("GEN");

                        return pr;
                    }

                case "SAME_REQTYPE_PRIORITY":
                    {
                        // By default, these requirements to not allow reuse of credits
                        string reqname = id;
                        List<string> requirementNames = new List<string>() { "REQU1", "REQU2" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();

                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add("REQU1", Subreqs);

                        List<string> Subreq2 = new List<string>() { "Subreq2" };
                        SubrequirementNames.Add("REQU2", Subreq2);

                        // The group will simply take any credit--assign this to both subreqs
                        groupNames.Add("Subreq1", new List<string>() { "SIMPLE.ANY" });
                        groupNames.Add("Subreq2", new List<string>() { "SIMPLE.ANY" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);

                        // Credits will be applied to this requirement's group and will not be allowed to be applied to the other--ONLY BECAUSE IT APPEARS
                        // FIRST IN THE PROGRAM REQUIREMENTS LIST.
                        pr.Requirements.First(rq => rq.Code == "REQU1").RequirementType = requirementTypes.First(rt => rt.Code == "CCD"); // priority 2
                        pr.Requirements.First(rq => rq.Code == "REQU2").RequirementType = requirementTypes.First(rt => rt.Code == "MIN"); // priority 2

                        // Neither allows sharing with each other
                        pr.Requirements.First(rq => rq.Code == "REQU1").Exclusions.Add("MIN");
                        pr.Requirements.First(rq => rq.Code == "REQU2").Exclusions.Add("CCD");

                        return pr;
                    }

                case "TWO_SUBREQS_NOSHARE":
                    {
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { "REQU1:N" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1", "Subreq2" };
                        SubrequirementNames.Add("REQU1", Subreqs);
                        // The first group requires 3 credits from subject MUSC. Second group requires 4 credits from subject MUSC. Add one to each subreq
                        groupNames.Add("Subreq1", new List<string>() { "SUBJ_MUSC_3CREDITS" });
                        groupNames.Add("Subreq2", new List<string>() { "SUBJ_MUSC_4CREDITS" });
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }

                case "TWO_GROUPS_NOSHARE":
                    {
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { "REQU1" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1:N" };
                        SubrequirementNames.Add("REQU1", Subreqs);
                        // The first group requires 3 credits from subject MUSC. Second group requires 4 credits from subject MUSC. Add both to the one subrequirement
                        groupNames.Add("Subreq1", new List<string>() { "SUBJ_MUSC_3CREDITS", "SUBJ_MUSC_4CREDITS" });
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }
                case "GROUP_EXCLUDED_REQ":
                    {
                        string reqname = id;
                        List<string> requirementNames = new List<string>() { "REQU1", "REQU2" };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();

                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add("REQU1", Subreqs);

                        List<string> Subreq2 = new List<string>() { "Subreq2" };
                        SubrequirementNames.Add("REQU2", Subreq2);

                        groupNames.Add("Subreq1", new List<string>() { "SUBJ_MUSC_3CREDITS" });
                        groupNames.Add("Subreq2", new List<string>() { "SUBJ_MUSC_3CREDITS_E" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);

                        pr.Requirements.First(rq => rq.Code == "REQU1").RequirementType = requirementTypes.First(rt => rt.Code == "MAJ");
                        pr.Requirements.First(rq => rq.Code == "REQU2").RequirementType = requirementTypes.First(rt => rt.Code == "GEN");


                        return pr;
                    }
                case "PROG.COURSE.REUSE":
                    {
                        //PROG.COURSE.REUSE
                        //Scenario 1: Req1->SubReq1 - type MAJ - excludes GEN - priority 1
                        //          Take 1 group;
                        //  Take ENGL-200 ENGL-300 COMM-100; (type 30)
                        //  Take 2 courses;
                        //  From Department COMM;  (type 32)
                        //Course Reuse = "Y" at subrequirement and group level (REQU, AEVP)

                        //Req 2-> Sub Req1 - type GEN - excludes MAJ  - priority 6
                        //Take 2 courses (type 32)

                        //Req1 excludes Req2 and vice-versa
                        string reqname = id;

                        List<string> reqnames = new List<string>();
                        Dictionary<string, List<string>> Subreq = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

                        reqnames.Add("REQ-1");
                        reqnames.Add("REQ-2");
                        //one sub-requirment for requirment 1 that have 2 groups
                        Subreq.Add("REQ-1", new List<string>() { "REQ-1-SUBREQ-1"});
                        groups.Add(Subreq["REQ-1"][0], new List<string>() { "GROUP-1-COURSE-REUSE", "GROUP-2-COURSE-REUSE" });

                        //one sub-requirment for requirement 2 that have 1 group
                        Subreq.Add("REQ-2", new List<string>() { "REQ-2-SUBREQ-1" });
                        groups.Add(Subreq["REQ-2"][0], new List<string>() { "GROUP-3-COURSE-REUSE" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname + "*" + cat, reqnames, Subreq, groups);
                        //modify settings of requirments here
                        //Take 1 group
                        pr.Requirements[0].SubRequirements[0].MinGroups = 1;
                        pr.Requirements[0].RequirementType = requirementTypes.First(rt => rt.Code == "MAJ");
                        pr.Requirements[1].RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                        pr.Requirements[0].Exclusions = new List<string>() { "GEN" };
                        pr.Requirements[1].Exclusions = new List<string>() { "MAJ" };
                        return pr;
                    }

                case "PROG.COURSE.REUSE.2":
                    {
                        //REq-1 (MAJ)  priority 1 with course reuse=N
                        //excludes ELE, GEN, MIN, MAJ
                        //has 2 subrequirments with course reuse = N
                        //Subrequirment 1
                        //Take 1 group;
                        //  Take 1 course from ENGL-200 ENGL-300;  (block type 31)
                        //  TAKE 3 COURSES FROM COMM;  (33)
                         //Subrequirment 2
                        //Take 1 group;
                        //#take 1 course from COMM-1315 COMM-100 COMM-1321;  (31)
                        //#TAKE 1 COURSE FROM department ART;  (33)


                        //Requirment 2 type GEN priority 6 course reuse N
                        //excludes MAJ, MIN, GEN, ELE
                        //subrequirment -1 
                        //Take 3 courses
                        string reqname = id;

                        List<string> reqnames = new List<string>();
                        Dictionary<string, List<string>> Subreq = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

                        reqnames.Add("REQ-1");
                        reqnames.Add("REQ-2");
                        //one sub-requirment for requirment 1 that have 2 groups
                        Subreq.Add("REQ-1", new List<string>() { "REQ-1-SUBREQ-1","REQ-1-SUBREQ-2" });
                        groups.Add(Subreq["REQ-1"][0], new List<string>() { "GROUP-1-COURSE-REUSE.2", "GROUP-2-COURSE-REUSE.2" });
                        groups.Add(Subreq["REQ-1"][1], new List<string>() { "GROUP-3-COURSE-REUSE.2", "GROUP-4-COURSE-REUSE.2" });

                        //one sub-requirment for requirement 2 that have 1 group
                        Subreq.Add("REQ-2", new List<string>() { "REQ-2-SUBREQ-1" });
                        groups.Add(Subreq["REQ-2"][0], new List<string>() { "GROUP-5-COURSE-REUSE.2" });

                        ProgramRequirements pr = await BuildTestProgramRequirementsAsync(reqname + "*" + cat, reqnames, Subreq, groups);
                        //modify settings of requirments here
                        //Take 1 group
                        pr.Requirements[0].SubRequirements[0].MinGroups = 1;
                        pr.Requirements[0].SubRequirements[1].MinGroups = 1;
                        pr.Requirements[0].AllowsCourseReuse = false;
                        pr.Requirements[0].SubRequirements[0].AllowsCourseReuse = false;
                        pr.Requirements[0].SubRequirements[1].AllowsCourseReuse = false;
                        pr.Requirements[0].RequirementType = requirementTypes.First(rt => rt.Code == "MAJ");
                        pr.Requirements[0].Exclusions = new List<string>() { "GEN", "MAJ" };

                        pr.Requirements[1].AllowsCourseReuse = false;
                        pr.Requirements[1].SubRequirements[0].AllowsCourseReuse = false;
                        pr.Requirements[1].RequirementType = requirementTypes.First(rt => rt.Code == "GEN");
                        pr.Requirements[1].Exclusions = new List<string>() { "MAJ","GEN" };
                        return pr;
                    }

                case "OVERRIDE.WITH.TRANSCRIPT.GROUPING":
                    {
                        // This is take 2 courses from level 100 , 200
                        string reqname = id + "*" + cat;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();
                        List<string> Subreqs = new List<string>() { "Subreq1" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "Test13" };
                        groupNames.Add(Subreqs[0], groups1);
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }

                default:
                    {
                        string reqname = id;
                        List<string> requirementNames = new List<string>() { reqname };
                        Dictionary<string, List<string>> SubrequirementNames = new Dictionary<string, List<string>>();
                        Dictionary<string, List<string>> groupNames = new Dictionary<string, List<string>>();

                        List<string> Subreqs = new List<string>() { "Math Core", "Engl Core" };
                        SubrequirementNames.Add(reqname, Subreqs);
                        List<string> groups1 = new List<string>() { "Math Theory", "Math Practical" };
                        List<string> groups2 = new List<string>() { "Engl Theory", "Engl Practical" };
                        groupNames.Add(Subreqs[0], groups1);
                        groupNames.Add(Subreqs[1], groups2);
                        return await BuildTestProgramRequirementsAsync(reqname, requirementNames, SubrequirementNames, groupNames);
                    }

            }

        }

        /// <summary>
        /// This method is used to generate program requirements with the given requirements, subrequirements and groups. 
        /// The calling process passes in a list of:
        ///   * Requirement names: List of Requirement IDs. optionally, can contain an asterisk to denote program and catalog. If no asterisk with specific program and catalog
        ///     is specified, MATH.BS*2011 is used as default. Each requirement ID can be followed by :N if the requirement does not allow sharing between its subrequirements.
        ///   * Subrequirement names: a dictionary keyed by requirement names with values containing the IDs of the subrequirements within each requirement. Each subrequirement ID
        ///     can be followed by :N if the subrequirement does not allow sharing between its groups. 
        ///   * Group Names: a dictionary keyed by subrequirement names with values containing the IDs of the groups for each subrequirement. A group name must reflect a value 
        ///     in the case statement within the BuildGroups method below, which sets up the group specification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requirementNames">List of requirement IDs (followed by :N to indicate if subrequirement sharing not allowed)</param>
        /// <param name="SubrequirementNames">Dictionary of subrequirements keyed by requirement ID (followed by :N to indicate if group sharing not allowed</param>
        /// <param name="groupNames">Dictionary of groups keyed by subrequirement ID (group names found in BuildGroup method case statement)</param>
        /// <returns></returns>
        public async Task<ProgramRequirements> BuildTestProgramRequirementsAsync(string id, List<string> requirementNames, Dictionary<string, List<string>> SubrequirementNames, Dictionary<string, List<string>> groupNames)
        {

            // Using new test repo
            courserepo = new TestCourseRepository();
            graderepo = new TestGradeRepository();
            ProgramRequirements pr;
            string prog;
            string cat;

            if (id.Contains('*'))
            {
                prog = id.Split('*')[0];
                cat = id.Split('*')[1];
            }
            else
            {
                prog = "MATH.BS";
                cat = "2011";
            }

            pr = new ProgramRequirements(prog, cat);

            // With any sort of luck, what has been passed in is 
            // 1 a list of requiement names, and for each of those 
            // 2 a dictionary of requirement name=>Subrequirement list and
            // 3 a dictionary of Subrequirement name => group names

            // The group names will be "Group3" or "Group8" or such, and will be matched up
            // to the groups from the group tests, so "Group8" will include the group data
            // from group test "Test8" in the BuildGroup() method.

            // Requirement/Subrequirement/Group "Ids" are now numeric, like the block IDs 
            // they represent

            // 19xxx  Requirement
            // 18xxx  Subrequirement
            // 10xxx  Group

            string requirementId = "19000";
            string SubrequirementId = "18000";
            int reqCount = 0;
            int subCount = 0;

            foreach (string reqSpec in requirementNames)
            {
                // parse the requirement name and the allows course reuse flag and min institutional credits value from the passed reqname string
                var reqValues = reqSpec.Split(':');
                // requirement name
                string reqname = reqValues[0];
                // allows course reuse
                bool reqAllowsCourseReuse = false;
                if (reqValues.Count() > 1)
                {
                    reqAllowsCourseReuse = reqValues[1] == "Y";
                }
                else
                {
                    reqAllowsCourseReuse = true;
                }

                // minimum institutional credits
                decimal? reqMinInstitutionalCredits = null;
                if (reqValues.Count() == 3)
                {
                    reqMinInstitutionalCredits = decimal.Parse(reqValues[2]);
                }

                // create requirement object
                var reqType = requirementTypes.First(rt => rt.Code == "MAJ");
                string gradeschemecode = "UG";
                string description = "Description of requirement " + reqname;

                requirementId = ((Int32.Parse(requirementId)) + reqCount).ToString();
                Requirement Requirement = new Requirement(requirementId, reqname, description, gradeschemecode, reqType);
                Requirement.AllowsCourseReuse = reqAllowsCourseReuse;
                Requirement.MinInstitutionalCredits = reqMinInstitutionalCredits;
                Requirement.ExtraCourseDirective = ExtraCourses.Apply;

                foreach (string subreqSpec in SubrequirementNames[reqname])
                {
                    // parse the sub-requirement name and the allows course reuse flag and min institutional credits value from the passed subreq string
                    var subreqValues = subreqSpec.Split(':');
                    // subreq name
                    string Subreqname = subreqValues[0];
                    // allows course reuse
                    bool subreqAllowsCourseReuse = false;
                    if (subreqValues.Count() > 1)
                    {
                        subreqAllowsCourseReuse = subreqValues[1] == "Y";
                    }
                    else
                    {
                        subreqAllowsCourseReuse = true;
                    }
                    // minimum institutional credits
                    decimal? subreqMinInstitutionalCredits = null;
                    if (subreqValues.Count() == 3)
                    {
                        subreqMinInstitutionalCredits = decimal.Parse(subreqValues[2]);
                    }
                    // create subrequirement object
                    SubrequirementId = ((Int32.Parse(SubrequirementId)) + subCount).ToString();
                    Subrequirement Subrequirement = new Subrequirement(SubrequirementId, Subreqname);
                    Subrequirement.Requirement = Requirement;   // backpointer
                    Subrequirement.AllowsCourseReuse = subreqAllowsCourseReuse;
                    Subrequirement.MinInstitutionalCredits = subreqMinInstitutionalCredits;
                    // build group objects
                    foreach (string groupname in groupNames[Subreqname])
                    {
                        Group Group = await BuildGroupAsync("temp ID", groupname, Subrequirement);
                        Subrequirement.Groups.Add(Group);
                    }
                    Requirement.SubRequirements.Add(Subrequirement);
                    subCount++;
                }
                Requirement.ProgramRequirements = pr; // backpointer
                pr.Requirements.Add(Requirement);
                reqCount++;
            }
            // Program level default settings
            pr.ActivityEligibilityRules = new List<RequirementRule>();
            pr.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("STCUSERX")));
            pr.MaximumCredits = null;
            pr.MinGrade = null;
            pr.MinimumInstitutionalCredits = 40m;
            pr.MinInstGpa = 2m;
            pr.MinimumCredits = 120;
            pr.MinOverallGpa = 2.1m;
            pr.AllowedGrades = null;



            return pr;
        }


        public async Task<Group> BuildGroupAsync(string id, string code, Subrequirement s)
        {

            // For convenience, dictionaries of courses, departments, etc.
            // I should probably rewrite these to just use the get methods, but I find this looks cleaner.
            // I think we can spare the k of memory.

            Dictionary<string, Course> courses = new Dictionary<string, Course>();
            Dictionary<string, Grade> grades = new Dictionary<string, Grade>();

            foreach (Course c in (await courserepo.GetAsync())) { courses.Add(c.SubjectCode + "-" + c.Number, c); }
            foreach (Grade g in (await graderepo.GetAsync())) { grades.Add(g.Id, g); }


            Group group1 = new Group("100", code, s);

            //group1.Id = "100";  //default


            //group1.MaxCreditsRule = new Rule();
            group1.AllowedGrades = new List<Grade>();
            group1.ButNotCourses = new List<string>();
            group1.ButNotCourseLevels = new List<string>();
            group1.ButNotDepartments = new List<string>();
            group1.ButNotSubjects = new List<string>();
            group1.FromCourses = new List<string>();
            group1.FromLevels = new List<string>();
            group1.FromDepartments = new List<string>();
            group1.FromSubjects = new List<string>();
            group1.Courses = new List<string>();
            group1.FromCoursesException = new List<string>();

            #region Specific Tests Group Data


            // Add logic to enable re-use of group number in group test. 
            // If the code has the character X, assume the part before the X is the code
            // that will be found in the switch, and the part after the X makes it unique 
            // for the test verification.
            code = code.Split('X').ToList().ElementAt(0);

            code = code.Replace("Group", "Test");
            switch (code)
            {
                case "Test1":
                    {
                        // TAKE ENGL*101
                        group1.Id = "10001";
                        group1.Courses.Add(courses["ENGL-101"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                case "Test2":
                    {
                        // TAKE 1 COURSE; FROM ENGL*101, ENGL*102
                        group1.Id = "10002";
                        group1.MinCourses = 1;
                        group1.FromCourses.Add(courses["ENGL-101"].Id);
                        group1.FromCourses.Add(courses["ENGL-102"].Id);
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }

                case "Test3":
                    {
                        // TAKE ENGL*101, ENGL*102 
                        group1.Id = "10003";
                        group1.Courses.Add(courses["ENGL-101"].Id);
                        group1.Courses.Add(courses["ENGL-102"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }


                case "Test4":
                    {
                        // TAKE 2 COURSES; FROM ENGL*101, ENGL*102 
                        group1.Id = "10004";
                        group1.MinCourses = 2;
                        group1.FromCourses.Add(courses["ENGL-101"].Id);
                        group1.FromCourses.Add(courses["ENGL-102"].Id);
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "Test5":
                    {
                        // TAKE 2 COURSES
                        group1.Id = "10005";
                        group1.MinCourses = 2;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test6":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENT MATH
                        group1.Id = "10006";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("MATH");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test7":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENT MATH; MINIMUM 1 DEPARTMENT
                        group1.Id = "10007";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("MATH");
                        group1.MinDepartments = 1;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test8":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENTS MATH, ENGL
                        group1.Id = "10008";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("MATH");
                        group1.FromDepartments.Add("ENGL");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test9":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENTS MATH, ENGL; MINIMUM 1 DEPARTMENT
                        group1.Id = "10009";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("MATH");
                        group1.FromDepartments.Add("ENGL");
                        group1.MinDepartments = 1;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test10":
                    {
                        // TAKE 2 COURSES; MINIMUM 2 DEPARTMENTS
                        group1.Id = "10010";
                        group1.MinCourses = 2;
                        group1.MinDepartments = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test11":
                    {
                        // TAKE 1 COURSE; FROM LEVEL 200 
                        group1.Id = "10011";
                        group1.MinCourses = 1;
                        group1.FromLevels.Add("200");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test12":
                    {
                        // TAKE 2 COURSES; FROM LEVELS 100, 200 
                        group1.Id = "10012";
                        group1.MinCourses = 2;
                        group1.FromLevels.Add("100");
                        group1.FromLevels.Add("200");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test13":
                    {
                        // TAKE 1 COURSE; FROM SUBJECT MATH
                        group1.Id = "10013";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("MATH");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test14":
                    {
                        // TAKE 1 COURSE; FROM SUBJECT MATH, ENGL 
                        group1.Id = "10014";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("ENGL");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test15":
                    {
                        // TAKE 2 COURSES; MINIMUM 2 SUBJECTS
                        group1.Id = "10015";
                        group1.MinCourses = 2;
                        group1.MinSubjects = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test16":
                    {
                        // TAKE 2 COURSES; EXCEPT MATH*100
                        group1.Id = "10016";
                        group1.MinCourses = 2;
                        group1.ButNotCourses.Add(courses["MATH-100"].Id);
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test17":
                    {
                        // TAKE 2 COURSES; EXCEPT MATH*100, DANC*200
                        group1.Id = "10017";
                        group1.MinCourses = 2;
                        group1.ButNotCourses.Add(courses["MATH-100"].Id);
                        group1.ButNotCourses.Add(courses["DANC-200"].Id);
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test18":
                    {
                        // TAKE 1 COURSE; FROM SUBJECT MATH; EXCEPT MATH*100
                        group1.Id = "10018";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("MATH");
                        group1.ButNotCourses.Add(courses["MATH-100"].Id);
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test19":
                    {
                        // TAKE 1 COURSE; EXCEPT DEPARTMENTS MATH
                        group1.Id = "10019";
                        group1.MinCourses = 1;
                        group1.ButNotDepartments.Add("MATH");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test20":
                    {
                        // TAKE 2 COURSES; EXCEPT DEPARTMENTS MATH, PERF 
                        group1.Id = "10020";
                        group1.MinCourses = 2;
                        group1.ButNotDepartments.Add("MATH");
                        group1.ButNotDepartments.Add("PERF");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test21":
                    {
                        // TAKE 1 COURSE; EXCEPT SUBJECT MATH
                        group1.Id = "10021";
                        group1.MinCourses = 1;
                        group1.ButNotSubjects.Add("MATH");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test22":
                    {
                        // TAKE 1 COURSE; EXCEPT SUBJECTS ENGL, DANC
                        group1.Id = "10022";
                        group1.MinCourses = 1;
                        group1.ButNotSubjects.Add("ENGL");
                        group1.ButNotSubjects.Add("DANC");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test23":
                    {
                        // TAKE 1 COURSE; EXCEPT LEVEL 100
                        group1.Id = "10023";
                        group1.MinCourses = 1;
                        group1.ButNotCourseLevels.Add("100");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test24":
                    {
                        // TAKE 1 COURSE; EXCEPT LEVELS 100,200
                        group1.Id = "10024";
                        group1.MinCourses = 1;
                        group1.ButNotCourseLevels.Add("100");
                        group1.ButNotCourseLevels.Add("200");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test25":
                    {
                        // TAKE 4 CREDITS
                        group1.Id = "10025";
                        group1.MinCredits = 4;
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test26":
                    {
                        // TAKE 3 CREDITS
                        group1.Id = "10026";
                        group1.MinCredits = 3;
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test27":
                    {
                        // TAKE 1 COURSE; MINIMUM 4 CREDITS
                        group1.Id = "10027";
                        group1.MinCourses = 1;
                        group1.MinCredits = 4;
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test28":
                    {
                        // TAKE 8 CREDITS; MAXIMUM 2 COURSES
                        group1.Id = "10028";
                        group1.MinCredits = 8;
                        group1.MaxCourses = 2;
                        group1.InternalType = "32";
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test29":
                    {
                        // TAKE 4 CREDITS; MAXIMUM 1 COURSE 
                        group1.Id = "10029";
                        group1.MinCredits = 4;
                        group1.MaxCourses = 1;
                        group1.InternalType = "32";
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test30":
                    {
                        // TAKE 2 COURSES; MAXIMUM 1 DEPARTMENT
                        group1.Id = "10030";
                        group1.MinCourses = 2;
                        group1.MaxDepartments = 1;
                        group1.InternalType = "32";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test31":
                    {
                        // TAKE 2 COURSES; MAXIMUM 1 SUBJECT
                        group1.Id = "10031";
                        group1.MinCourses = 2;
                        group1.MaxSubjects = 1;
                        group1.InternalType = "32";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test32":
                    {
                        // TAKE 1 COURSE; MAXIMUM 3 CREDITS
                        group1.Id = "10032";
                        group1.MinCourses = 1;
                        group1.MaxCredits = 3;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test33":
                    {
                        // TAKE 2 COURSES; MAXIMUM 6 CREDITS
                        group1.Id = "10033";
                        group1.MinCourses = 2;
                        group1.MaxCredits = 6;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test34":
                    {
                        // TAKE 2 COURSES; MAXIMUM 1 COURSE PER SUBJECT 
                        group1.Id = "10034";
                        group1.MinCourses = 2;
                        group1.MaxCoursesPerSubject = 1;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test35":
                    {
                        // TAKE 4 COURSES; MAXIMUM 2 COURSES PER SUBJECT
                        group1.Id = "10035";
                        group1.MinCourses = 4;
                        group1.MaxCoursesPerSubject = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test36":
                    {
                        // TAKE 4 COURSES; MAXIMUM 1 COURSE PER SUBJECT
                        group1.Id = "10036";
                        group1.MinCourses = 4;
                        group1.MaxCoursesPerSubject = 1;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test37":
                    {
                        // TAKE 1 COURSE; MAXIMUM 1 COURSE PER DEPARTMENT
                        group1.Id = "10037";
                        group1.MinCourses = 1;
                        group1.MaxCoursesPerDepartment = 1;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test38":
                    {
                        // TAKE 3 COURSES; MAXIMUM 2 COURSES PER DEPARTMENT 
                        group1.Id = "10038";
                        group1.MinCourses = 3;
                        group1.MaxCoursesPerDepartment = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test39":
                    {
                        // TAKE 2 COURSES; MAXIMUM 1 100,200 LEVEL COURSES
                        group1.Id = "10039";
                        group1.MinCourses = 2;
                        List<string> mxlevlist = new List<string>();
                        mxlevlist.Add("100");
                        mxlevlist.Add("200");
                        group1.MaxCoursesAtLevels = new MaxCoursesAtLevels(1, mxlevlist);
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test40":
                    {
                        // TAKE 2 COURSES; MAXIMUM 3 CREDITS PER COURSE
                        group1.Id = "10040";
                        group1.MinCourses = 2;
                        group1.MaxCreditsPerCourse = 3;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test41":
                    {
                        // TAKE 2 COURSES; MAXIMUM 3 CREDITS PER SUBJECT
                        group1.Id = "10041";
                        group1.MinCourses = 2;
                        group1.MaxCreditsPerSubject = 3;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test42":
                    {
                        // TAKE 3 COURSES; MAXIMUM 6 CREDITS PER DEPARTMENT
                        group1.Id = "10042";
                        group1.MinCourses = 3;
                        group1.MaxCreditsPerDepartment = 6;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test43":
                    {
                        // TAKE 2 COURSES; MAXIMUM 3 100,200 LEVEL CREDITS
                        group1.Id = "10043";
                        group1.MinCourses = 2;
                        List<string> mxlevlist = new List<string>();
                        mxlevlist.Add("100");
                        mxlevlist.Add("200");
                        group1.MaxCreditsAtLevels = new MaxCreditAtLevels(3, mxlevlist);
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test44":
                    {
                        // TAKE 6 CREDITS; MINIMUM 3 INST.HOURS
                        group1.Id = "10044";
                        group1.MinCredits = 6;
                        group1.MinInstitutionalCredits = 3;
                        group1.InternalType = "32";
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "Test45":
                    {
                        // TAKE 2 COURSES; MINIMUM 2 INST.HOURS
                        group1.Id = "10045";
                        group1.MinCourses = 2;
                        group1.MinInstitutionalCredits = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test46":
                    {
                        // TAKE 2 COURSES; MINIMUM 4 CREDITS PER COURSE
                        group1.Id = "10046";
                        group1.MinCourses = 2;
                        group1.MinCreditsPerCourse = 4;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test47":
                    {
                        // TAKE 2 COURSES FROM SUBJECTS MUSC; MINIMUM 4 CREDITS PER SUBJECT
                        group1.Id = "10047";
                        group1.MinCourses = 2;
                        group1.FromSubjects.Add("MUSC");
                        group1.MinCreditsPerSubject = 4;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test48":
                    {
                        // TAKE 2 COURSES FROM SUBJECTS MATH,PHYS;MINIMUM 4 CREDITS PER SUBJECT
                        group1.Id = "10048";
                        group1.MinCourses = 2;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("PHYS");
                        group1.MinCreditsPerSubject = 4;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test49":
                    {
                        // TAKE 2 COURSES FROM SUBJECTS MATH,PHYS;MINIMUM 4 CREDITS PER SUBJECT;MINIMUM 2 SUBJECTS 
                        group1.Id = "10049";
                        group1.MinCourses = 2;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("PHYS");
                        group1.MinCreditsPerSubject = 4;
                        group1.MinSubjects = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test50":
                    {
                        // TAKE 3 COURSES FROM DEPARTMENTS HIST,MATH; MINIMUM 4 CREDITS PER DEPARTMENT
                        group1.Id = "10050";
                        group1.MinCourses = 3;
                        group1.FromDepartments.Add("HIST");
                        group1.FromDepartments.Add("MATH");
                        group1.MinCreditsPerDepartment = 4;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test51":
                    {
                        // TAKE 4 COURSES FROM DEPARTMENTS HIST,MATH; MINIMUM 2 COURSES PER DEPARTMENT
                        group1.Id = "10051";
                        group1.MinCourses = 4;
                        group1.FromDepartments.Add("HIST");
                        group1.FromDepartments.Add("MATH");
                        group1.MinCoursesPerDepartment = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test52":
                    {
                        // TAKE 4 COURSES FROM SUBJECTS HIST,MATH; MINIMUM 2 COURSES PER SUBJECT
                        group1.Id = "10052";
                        group1.MinCourses = 4;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("HIST");
                        group1.MinCoursesPerSubject = 2;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test53":
                    {
                        // TAKE 2 COURSES; MIN Gpa 2.5 
                        group1.Id = "10053";
                        group1.MinCourses = 2;
                        group1.MinGpa = decimal.Parse("2.5");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test54":
                    {
                        // TAKE 2 COURSES; MIN GRADE A
                        group1.Id = "10054";
                        group1.MinCourses = 2;
                        group1.MinGrade = grades["A"];
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "Test55":
                    {
                        // TAKE 2 COURSES; MIN GRADE A,AU,P 
                        group1.Id = "10055";
                        group1.MinCourses = 2;
                        group1.MinGrade = grades["A"];
                        group1.AllowedGrades.Add(grades["AU"]);
                        group1.AllowedGrades.Add(grades["P"]);
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test56":
                    {   // TAKE 1 COURSE FROM RULE MATH100
                        group1.Id = "10056";
                        group1.MinCourses = 1;
                        group1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("MATH100")));
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test57":
                    {
                        // Take 1 course from dept "PERF"
                        group1.Id = "10057";
                        group1.MaxCourses = 1;
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("PERF");
                        group1.GroupType = GroupType.TakeCourses;
                        break;

                    }

                case "Test58":
                    {
                        // TAKE ENGL*101, ENGL*102 
                        group1.Id = "10058";
                        group1.Courses.Add(courses["ENGL-101"].Id);
                        group1.Courses.Add(courses["HIST-200"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                case "Test59":
                    {
                        group1.Id = "10059";
                        group1.Courses.Add(courses["MATH-201"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                case "Test60":
                    {
                        // TAKE 2 COURSES
                        group1.Id = "10060";
                        group1.MinCourses = 2;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("DANC");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test61":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENT HIST -- TESTING EQUATED DEPARTMENT
                        group1.Id = "10061";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("HIST");
                        group1.InternalType = "33";
                        break;
                    }

                case "Test62":
                    {
                        // TAKE 1 COURSE; FROM SUBJECT POLI -- TESTING EQUATED SUBJECT
                        group1.Id = "10062";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("POLI");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test63":
                    {
                        // TAKE 1 COURSE; FROM LEVEL 200  -- TESTING EQUATED LEVEL
                        group1.Id = "10063";
                        group1.MinCourses = 1;
                        group1.FromLevels.Add("200");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test64":
                    {
                        // TAKE 1 COURSE; FROM DEPARTMENT HIST, BUT NOT POLI --EQUATED COURSE FROM EXCEPT DEPT NOT APPLIED
                        group1.Id = "10061";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("HIST");
                        group1.ButNotDepartments.Add("POLI");
                        group1.InternalType = "33";
                        break;
                    }

                case "Test65":
                    {
                        // TAKE 1 COURSE; FROM SUBJECT POLI, BUT NOT HIST -- EQUATED COURSE FROM EXCEPT SUBJECT NOT APPLIED
                        group1.Id = "10062";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("POLI");
                        group1.ButNotSubjects.Add("HIST");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test66":
                    {
                        // TAKE 1 COURSE; FROM LEVEL 200, BUT NOT LEVEL 100 -- EQUATED COURSE FROM EXCEPT LEVEL NOT APPLIED
                        group1.Id = "10063";
                        group1.MinCourses = 1;
                        group1.FromLevels.Add("200");
                        group1.ButNotCourseLevels.Add("100");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test67":
                    {
                        // Take 1 course; from subject HIST with Additional Courses Exception allowing MATH-100 (id 143)
                        group1.Id = "10067";
                        group1.MinCourses = 2;
                        group1.FromSubjects = new List<string>() { "HIST" };
                        group1.GroupType = GroupType.TakeCourses;
                        group1.IsModified = true;
                        group1.FromCoursesException.Add(courses["MATH-100"].Id);
                        group1.IsWaived = false;
                        break;
                    }

                case "Test68":
                    {
                        // Take 1 course with rule, Exception allowing MATH-100 (id 143)
                        group1.Id = "10068";
                        group1.MinCourses = 2;
                        group1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("SUBJENGL")));
                        group1.GroupType = GroupType.TakeCourses;
                        group1.IsModified = true;
                        group1.FromCoursesException.Add(courses["MATH-100"].Id);
                        group1.IsWaived = false;
                        break;
                    }

                case "Test69":
                    {
                        // Take 4 credits; from ENGL*101 ENGL*102
                        group1.Id = "10069";
                        group1.MinCredits = 4;
                        group1.FromCourses.Add(courses["ENGL-101"].Id);
                        group1.FromCourses.Add(courses["ENGL-102"].Id);
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test70":
                    {
                        // Take 4 Credits; from DEPT HIST; Exception allowing ENGL*101 ENGL*102 
                        group1.Id = "10070";
                        group1.MinCredits = 4;
                        group1.FromDepartments.Add("HIST");
                        group1.FromCoursesException.Add(courses["ENGL-101"].Id);
                        group1.FromCoursesException.Add(courses["ENGL-102"].Id);
                        group1.IsModified = true;
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test71":
                    {
                        // Take 4 Credits; from ENGL*101 ENGL*102. Exception allowing MATH*201
                        group1.Id = "10071";
                        group1.MinCredits = 4;
                        group1.FromCourses.Add(courses["ENGL-101"].Id);
                        group1.FromCourses.Add(courses["ENGL-102"].Id);
                        group1.FromCoursesException.Add(courses["MATH-201"].Id);
                        group1.IsModified = true;
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test72":
                    {
                        // TAKE 1 COURSES; MIN Gpa 2.5 
                        group1.Id = "10072";
                        group1.MinCourses = 1;
                        group1.MinGpa = decimal.Parse("2.5");
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test73":
                    {
                        // Take 9 credits; From Subjects ART HIST MUSC DANC ENGL HUMT RELG CERA; Minimum 2 subjects;
                        group1.Id = "10073";
                        group1.MinCredits = 9;
                        group1.MinSubjects = 2;
                        group1.FromSubjects = new List<string>() { "ART", "HIST", "MUSC", "DANC", "ENGL", "HUMT", "RELG", "CERA" };
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test74":
                    {
                        // Take 9 credits; From Subjects ART HIST MUSC DANC ENGL HUMT RELG CERA; Minimum 2 departments;
                        group1.Id = "10074";
                        group1.MinCredits = 9;
                        group1.MinDepartments = 2;
                        group1.FromSubjects = new List<string>() { "ART", "HIST", "MUSC", "DANC", "ENGL", "HUMT", "RELG", "CERA" };
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }                    

                case "Test75":
                    {
                        // Take 9 credits; From Subjects ART HIST MUSC DANC ENGL HUMT RELG CERA; Minimum 5 courses;
                        group1.Id = "10075";
                        group1.MinCredits = 9;
                        group1.MinCourses = 5;
                        group1.FromSubjects = new List<string>() { "ART", "HIST", "MUSC", "DANC", "ENGL", "HUMT", "RELG", "CERA" };
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test76":
                    {
                        // Take 6 Credits; From Subjects ART MUSC RELG;
                        group1.Id = "10076";
                        group1.MinCredits = 6;
                        group1.FromSubjects = new List<string>() { "ART", "MUSC", "RELG", "HU" };
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCredits;
                        group1.IsWaived = false;
                        break;
                    }

                case "Test77":
                    {
                        // TAKE 3 COURSES; MAX 2 FROM LEVELS 100, 200 
                        group1.Id = "10077";
                        group1.MinCourses = 3;
                        group1.MaxCoursesAtLevels = new MaxCoursesAtLevels(2, new List<string>() { "100", "200" });
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test78":
                    {
                        // TAKE 16 CREDITS; MAX 8 CREDITS FROM LEVELS 100, 200 
                        group1.Id = "10078";
                        group1.MinCredits = 16;
                        group1.MaxCreditsAtLevels = new MaxCreditAtLevels(9, new List<string>() { "100", "200" });
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "Test79":
                    {
                        // TAKE 3 CREDITS 
                        group1.Id = "10079";
                        group1.MinCredits = 3;
                        group1.InternalType = "33";
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                case "Math Theory":
                    {
                        group1.Id = "10056";
                        group1.Courses.Add(courses["MATH-100"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "Math Practical":
                    {
                        group1.Id = "10057";
                        group1.Courses.Add(courses["MATH-200"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "Engl Theory":
                    {
                        // TAKE ENGL*101
                        group1.Id = "10058";
                        group1.Courses.Add(courses["ENGL-101"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "Engl Practical":
                    {
                        group1.Id = "10059";
                        group1.Courses.Add(courses["ENGL-102"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                // For RequirementsRepositoryTests 

                //Requirement:          Math Core
                //Subrequirements:      Math Basic(grp 1)      Math Practical (grps 2&3)(take both)
                //Groups:     (1) TAKE MATH-100   (2) TAKE 4 CREDITS FROM DEPT MATH  (3)  TAKE 1 COURSE FROM DEPT MATH LEVEL 300,400

                //Requirement:          Engl Core
                //Subrequirements:      Engl Basic(grp 4)      Humanities (grps 5&6)(take 1)
                //Groups:     (4) TAKE ENGL-101 and COMP-100   (5) TAKE 1 COURSE FROM (ART-200,DANC-100)   (6)  TAKE 1 COURSE FROM SUBJECT SOCI


                case "ReqTestGrp1":
                    {
                        group1.Id = "10061";
                        group1.Courses.Add(courses["MATH-100"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "ReqTestGrp2":
                    {
                        group1.Id = "10062";
                        group1.MinCredits = 4;
                        group1.FromDepartments.Add("MATH");
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "ReqTestGrp3":
                    {
                        group1.Id = "10063";
                        group1.MinCourses = 1;
                        group1.FromDepartments.Add("MATH");
                        group1.FromLevels.Add("300");
                        group1.FromLevels.Add("400");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "ReqTestGrp4":
                    {
                        group1.Id = "10064";
                        group1.Courses.Add(courses["ENGL-101"].Id);
                        group1.Courses.Add(courses["COMP-100"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "ReqTestGrp5":
                    {
                        group1.Id = "10065";
                        group1.MinCourses = 1;
                        group1.FromCourses.Add(courses["ART-200"].Id);
                        group1.FromCourses.Add(courses["DANC-100"].Id);
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "ReqTestGrp6":
                    {
                        group1.Id = "10066";
                        group1.MinCourses = 1;
                        group1.FromSubjects.Add("SOCI");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                // Cases for groups in coll18dev
                case "20886":
                    {
                        group1.Id = "20886";
                        group1.Courses.Add(courses["ENGL-1000"].Id);
                        group1.Courses.Add(courses["ENGL-1001"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "20888":
                    {
                        group1.Id = "20888";
                        group1.MinCourses = 1;
                        group1.FromCourses.Add(courses["MATH-1000"].Id);
                        group1.FromCourses.Add(courses["MATH-1001"].Id);
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "20892":
                    {
                        group1.Id = "20892";
                        group1.MinCourses = 4;
                        group1.FromDepartments.Add("MATH");
                        group1.FromLevels.Add("100");
                        group1.FromLevels.Add("200");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "20895":
                    {
                        group1.Id = "20895";
                        group1.MinCredits = 12;
                        group1.FromSubjects.Add("MATH");
                        group1.FromSubjects.Add("COMP");
                        group1.ButNotCourseLevels.Add("100");
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "20897":
                    {
                        group1.Id = "20897";
                        group1.MinCourses = 2;
                        group1.FromCourses.Add(courses["MATH-4000"].Id);
                        group1.FromCourses.Add(courses["MATH-4001"].Id);
                        group1.FromCourses.Add(courses["MATH-4002"].Id);
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "20900":
                    {
                        group1.Id = "20900";
                        group1.Courses.Add(courses["HU-1000"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "20902":
                    {
                        group1.Id = "20902";
                        group1.Courses.Add(courses["COMM-2000"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }


                case "SIMPLE.ANY":
                    {
                        group1.Id = "SIMPLE.ANY";
                        group1.MinCourses = 10;
                        // accept courses from any level
                        group1.FromLevels.Add("100");
                        group1.FromLevels.Add("200");
                        group1.FromLevels.Add("300");
                        group1.FromLevels.Add("400");
                        group1.FromLevels.Add("500");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "500ONLY":
                    {
                        group1.Id = "500ONLY";
                        group1.MinCourses = 10;
                        // accept courses from a level that will not work
                        group1.FromLevels.Add("500");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "100-200ONLY":
                    {
                        group1.Id = "100-200ONLY";
                        group1.MinCourses = 10;
                        // accept courses from a level that will not work
                        group1.FromLevels.Add("100");
                        group1.FromLevels.Add("200");
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }

                case "MUSC-209":
                    {
                        group1.Id = "MUSC-209";
                        group1.Courses.Add(courses["MUSC-209"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }

                case "SUBJ_MUSC_3CREDITS":
                    {
                        group1.Id = "MUSC-209";
                        group1.MinCredits = 3;
                        group1.FromSubjects.Add("MUSC");
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }

                case "SUBJ_MUSC_3CREDITS_E":
                    {
                        group1.Id = "MUSC-209";
                        group1.MinCredits = 3;
                        group1.FromSubjects.Add("MUSC");
                        group1.GroupType = GroupType.TakeCredits;
                        group1.Exclusions = new List<string>() {"MAJ", "MIN"};
                        break;
                    }
                case "SUBJ_MUSC_4CREDITS":
                    {
                        group1.Id = "MUSC-209-TWO";
                        group1.MinCredits = 4;
                        group1.FromSubjects.Add("MUSC");
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                case "ZERO_CREDITS_COUNTED":
                    {
                        // TAKE 2 COURSES; FROM ENGL*101, ACCT*101 (a course with 0 credits) 
                        group1.Id = "10000-BB";
                        group1.MinCourses = 2;
                        group1.FromCourses.Add(courses["ENGL-101"].Id);
                        group1.FromCourses.Add(courses["ACCT-101"].Id);
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "GROUP-1-COURSE-REUSE":
                    {
                        //  Take ENGL-200 ENGL-300 COMM-100; (type 30)
                        group1.Id = "GROUP-1-COURSE-REUSE-BB";
                        group1.Courses.Add(courses["ENGL-200"].Id);
                        group1.Courses.Add(courses["ENGL-300"].Id);
                        group1.Courses.Add(courses["COMM-100"].Id);
                        group1.GroupType = GroupType.TakeAll;
                        break;
                    }
                case "GROUP-2-COURSE-REUSE":
                    {
                        //  Take 2 courses From Department COMM;  (type 32)
                        group1.Id = "GROUP-2-COURSE-REUSE-BB";
                        group1.FromDepartments.Add("COMM");
                        group1.MinCourses = 2;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "GROUP-3-COURSE-REUSE":
                    {
                        //Take 2 courses (type 32)
                        group1.Id = "GROUP-3-COURSE-REUSE";
                        group1.MinCourses = 2;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "GROUP-1-COURSE-REUSE.2":
                    {
                        //  Take 1 course from ENGL-200 ENGL-300 (31)
                        group1.Id = "GROUP-1-COURSE-REUSE-BB.2";
                        group1.Courses.Add(courses["ENGL-200"].Id);
                        group1.Courses.Add(courses["ENGL-300"].Id);
                        group1.MinCourses = 1;
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "GROUP-2-COURSE-REUSE.2":
                    {
                        //  TAKE 3 COURSES FROM COMM;  (33)
                        group1.Id = "GROUP-2-COURSE-REUSE-BB.2";
                        group1.FromDepartments.Add("COMM");
                        group1.MinCourses = 3;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "GROUP-3-COURSE-REUSE.2":
                    {
                        //#take 1 course from COMM-1315 COMM-100 COMM-1321;  (31)
                        group1.Id = "GROUP-3-COURSE-REUSE-BB.2";
                        group1.Courses.Add(courses["COMM-1315"].Id);
                        group1.Courses.Add(courses["COMM-100"].Id);
                        group1.Courses.Add(courses["COMM-1321"].Id);

                        group1.MinCourses = 1;
                        group1.GroupType = GroupType.TakeSelected;
                        break;
                    }
                case "GROUP-4-COURSE-REUSE.2":
                    {
                        //#TAKE 1 COURSE FROM department ART;  (33)

                        group1.Id = "GROUP-4-COURSE-REUSE-BB.2";
                        group1.FromDepartments.Add("ART");
                        group1.MinCourses = 1;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "GROUP-5-COURSE-REUSE.2":
                    {
                        //Take 3 courses
                        group1.Id = "GROUP-5-COURSE-REUSE-BB.2";
                        group1.MinCourses = 3;
                        group1.GroupType = GroupType.TakeCourses;
                        break;
                    }
                case "TAKE_0_CREDITS":
                    {
                        // TAKE 0 CREDITS
                        group1.Id = "TAKE0CREDITS";
                        group1.MinCredits = 0;
                        group1.GroupType = GroupType.TakeCredits;
                        break;
                    }
                default:
                    {
                        // oh come now
                        throw new NotImplementedException("Specified group id " + code + " not found in Build Group switch statement in TestProgramRequirementsRepository");
                    }

            }

            #endregion
            return group1;
        }

        public ProgramRequirements Get(string program, string catalog)
        {
            try
            {
                var programReqmtsTask = GetAsync(program, catalog);
                var programRequirementsData = programReqmtsTask.Result;
                return programRequirementsData;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    var ae = ex as AggregateException;
                    throw ex.InnerException;
                }
                throw;
            }
        }
    }
}