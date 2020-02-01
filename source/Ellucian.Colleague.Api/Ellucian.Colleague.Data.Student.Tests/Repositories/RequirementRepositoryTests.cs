// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Data.Colleague.DataContracts;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class RequirementRepositoryTests
    {
        private const string _IgnoreCode = "I";
        private const string _DisplayCode = "D";
        private const string _ApplyCode = "S";

        const string _CacheName = "AllGrades";
        ObjectCache localCache = new MemoryCache(_CacheName);


        private List<string> reqs;
        private List<string> sub1;
        private List<string> sub2;
        private List<string> grp1;
        private List<string> grp23;
        private List<string> grp4;
        private List<string> grp56;

        private IDictionary<string, string> courses;
        private TestProgramRequirementsRepository testProgramRequirementsRepo;

        private RequirementRepository requirementRepo;

        private Mock<IColleagueDataReader> dataAccessorMock;


        [TestInitialize]
        public async void Initialize()
        {
            // Taking advantage of the requirements-building business in the TestProgramRequirementsRepository.  Feed it
            // some prefab names and it will build requirements trees.  Then we will pick them apart and form them
            // back into transaction responses.
            testProgramRequirementsRepo = new TestProgramRequirementsRepository();

            //public ProgramRequirements Get(string id, List<string> requirementNames, Dictionary<string, List<string>> SubrequirementNames, Dictionary<string, List<string>> groupNames)
            //Requirement:          Math Core
            //Subrequirements:      Math Basic(grp 1)      Math Practical (grps 2&3)(take both)
            //Groups:     (1) TAKE MATH-100   (2) TAKE 4 CREDITS FROM DEPT MATH  (3)  TAKE 1 COURSE FROM DEPT MATH LEVEL 300,400

            //Requirement:          Engl Core
            //Subrequirements:      Engl Basic(grp 4)      Humanities (grps 5&6)(take 1)
            //Groups:     (4) TAKE ENGL-101 and COMP-100   (5) TAKE 1 COURSE FROM (ART-200,DANC-100)   (6)  TAKE 1 COURSE FROM SUBJECT SOCI

            string id = "TestProgramRequirementsId";
            reqs = new List<string>() { "Math Core", "Engl Core" };
            sub1 = new List<string>() { "Math Basic", "Math Practical" };
            sub2 = new List<string>() { "Engl Basic", "Humanities" };
            grp1 = new List<string>() { "ReqTestGrp1" };
            grp23 = new List<string>() { "ReqTestGrp2", "ReqTestGrp3" };
            grp4 = new List<string>() { "ReqTestGrp4" };
            grp56 = new List<string>() { "ReqTestGrp5", "ReqTestGrp6" };

            Dictionary<string, List<string>> Subrequirementnames = new Dictionary<string, List<string>>();
            Subrequirementnames.Add(reqs[0], sub1);
            Subrequirementnames.Add(reqs[1], sub2);
            Dictionary<string, List<string>> groupnames = new Dictionary<string, List<string>>();
            groupnames.Add(sub1[0], grp1);
            groupnames.Add(sub1[1], grp23);
            groupnames.Add(sub2[0], grp4);
            groupnames.Add(sub2[1], grp56);

            ProgramRequirements pr = await testProgramRequirementsRepo.BuildTestProgramRequirementsAsync(id, reqs, Subrequirementnames, groupnames);
            dataAccessorMock = new Mock<IColleagueDataReader>();
            requirementRepo = await BuildValidRequirementRepository(pr);

            // Getting test course repo and making a dictionary of course names to Ids so I don't have to look up course IDs
            TestCourseRepository courserepo = new TestCourseRepository();
            IEnumerable<Course> allCourses = await courserepo.GetAsync();
            courses = new Dictionary<string, string>();
            foreach (Course c in allCourses)
            {
                courses.Add(c.SubjectCode + "-" + c.Number, c.Id);
            }


        }

        [TestMethod]
        public async Task Get_ReturnListedRequirements()
        {
            IEnumerable<String> codes = new List<String>() { "Math Core", "Engl Core" };
            var requirements = await requirementRepo.GetAsync(codes);
            Assert.AreEqual(2, requirements.Count());
            foreach (var code in codes)
            {
                Assert.IsNotNull(requirements.Select(rr => rr.Id == code));
            }
        }

        [TestMethod]
        public async Task Get_NoRequirementCodesYieldsEmptyRequirementsList()
        {

            var requirements =await requirementRepo.GetAsync(new List<string>());
            Assert.AreEqual(0, requirements.Count());
        }

        [TestMethod]
        public async Task Get_NullandEmptyCodesRemovedFromList()
        {
            IEnumerable<String> codes = new List<String>() { "Math Core", null, "", "Engl Core" };
            var requirements = await requirementRepo.GetAsync(codes);
            Assert.AreEqual(2, requirements.Count());
            foreach (var code in codes)
            {
                Assert.IsNotNull(requirements.Select(rr => rr.Id == code));
            }
        }

        [TestMethod]
        public async Task Get_ReturnedRequirementsHaveRequirementType()
        {
            // This program has two requirements, one with req type=MIN, other with req type = GEN
            // GEN item will get valid sequence (2)
            // MIN item will get max sequence because it is not found in the valcode table
            string programCode = "MULTIPLE_REQTYPE";
            var programRequirements = await testProgramRequirementsRepo.GetAsync(programCode, "2013");
            requirementRepo =await BuildValidRequirementRepository(programRequirements);
            var requirements = await requirementRepo.GetAsync(programRequirements.Requirements.Select(r => r.Code));
            foreach (var req in requirements)
            {
                Assert.IsNotNull(req.RequirementType);
                if (req.RequirementType.Code == "GEN")
                {
                    Assert.AreEqual(2, req.RequirementType.Priority);
                }
                if (req.RequirementType.Code == "MIN")
                {
                    Assert.AreEqual(int.MaxValue, req.RequirementType.Priority);
                }
            }
        }

        [TestMethod]
        public async Task Get_ReturnedRequisiteRequirementsHaveRequirementTypeOfNONE()
        {
            string programCode = "MULTIPLE_REQTYPE";
            var programRequirements = await testProgramRequirementsRepo.GetAsync(programCode, "2013");
            foreach (var req in programRequirements.Requirements)
            {
                req.RequirementType = null;
            }
            requirementRepo = await BuildValidRequirementRepository(programRequirements);
            // By clearing out the RequirementType below, setting the requirement repository response up 
            // that will mimic the data retrieved as if they are requisite requirements not attached to a program.
            // These items will not have a requirement type in Colleague and will be given a NONE type so that
            // a RequisiteType is not null--needed for sorting in program evaluation.
            var requirements = await requirementRepo.GetAsync(programRequirements.Requirements.Select(r => r.Code));
            foreach (var req in requirements)
            {
                Assert.AreEqual("NONE", req.RequirementType.Code);
                Assert.AreEqual(int.MaxValue, req.RequirementType.Priority);
            }
        }
        [TestMethod]
        public async Task Get_ReturnEmptyListIfRequirementsNotFound()
        {
            IEnumerable<String> codes = new List<String>() { "xxx", "yyy" };
            var requirements = await requirementRepo.GetAsync(codes);
            Assert.AreEqual(0, requirements.Count());
        }

        [TestMethod]
        public async Task Get_ReturnsSingleRequirement()
        {
            var requirement = await requirementRepo.GetAsync("Math Core");
            Assert.AreEqual("Math Core", requirement.Code);

        }

        [TestMethod]
        public async Task Get_ReturnsRequestedSubrequirements()
        {
            var requirement = await requirementRepo.GetAsync("Math Core");

            IEnumerable<string> subnames = new List<string>();
            subnames = requirement.SubRequirements.Select(ss => ss.Code);
            Assert.IsTrue(subnames.Contains("Math Basic"));
            Assert.IsTrue(subnames.Contains("Math Practical"));

        }

        [TestMethod]
        public async Task Get_ReturnsRequestedGroups()
        {
            var requirement = await requirementRepo.GetAsync("Engl Core");

            foreach (var sub in requirement.SubRequirements)
            {

                if (sub.Id == "Engl Basic")
                {
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp1"));
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp2"));
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp3"));
                }
                if (sub.Id == "Humanities")
                {
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp4"));
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp5"));
                    Assert.IsNotNull(sub.Groups.First(g => g.Code == "ReqTestGrp6"));
                }

            }
        }

        [TestMethod]
        public async Task Get_ReturnedGroupsContainCourseIds()
        {
            var requirement = await requirementRepo.GetAsync("Engl Core");
            bool grpReallyTested = false;

            foreach (var sub in requirement.SubRequirements)
            {

                if (sub.Code == "Engl Basic")
                {
                    foreach (var grp in sub.Groups)
                    {
                        if (grp.Code == "ReqTestGrp4")
                        {
                            Assert.IsTrue(grp.Courses.Contains(courses["ENGL-101"]));
                            Assert.IsTrue(grp.Courses.Contains(courses["COMP-100"]));
                            Assert.IsFalse(grp.Courses.Contains(courses["MATH-100"]));
                            grpReallyTested = true;
                        }

                    }
                }
            }
            Assert.IsTrue(grpReallyTested);
        }

        [TestMethod]
        public async Task Get_ReturnedGroupsContainFromCourseIds()
        {
            bool grpReallyTested = false;
            var requirement = await requirementRepo.GetAsync("Engl Core");

            foreach (var sub in requirement.SubRequirements)
            {

                if (sub.Code == "Humanities")
                {
                    foreach (var grp in sub.Groups)
                    {
                        if (grp.Code == "ReqTestGrp5")
                        {
                            Assert.IsTrue(grp.FromCourses.Contains(courses["DANC-100"]));
                            Assert.IsFalse(grp.Courses.Contains(courses["MATH-100"]));
                            grpReallyTested = true;
                        }

                    }
                }
            }
            Assert.IsTrue(grpReallyTested);
        }

        [TestMethod]
        public async Task GetManyWithEmptyListReturnsEmptyList()
        {
            var requirements = await requirementRepo.GetAsync(new List<string>());
            Assert.AreEqual(0, requirements.Count());
        }

        [TestMethod]
        public async Task CascadesMinGradeAndAllowedGradesDownFromRequirement()
        {
            string id = "TestReq";
            reqs = new List<string>() { "REQ2" };
            sub1 = new List<string>() { "SUBREQ1" };
            grp1 = new List<string>() { "SIMPLE.ANY" };

            Dictionary<string, List<string>> Subrequirementnames = new Dictionary<string, List<string>>();
            Subrequirementnames.Add(reqs[0], sub1);
            Dictionary<string, List<string>> groupnames = new Dictionary<string, List<string>>();
            groupnames.Add(sub1[0], grp1);

            ProgramRequirements pr = await testProgramRequirementsRepo.BuildTestProgramRequirementsAsync(id, reqs, Subrequirementnames, groupnames);
            pr.Requirements.ElementAt(0).MinGrade = (await new TestGradeRepository().GetAsync()).Where(g => g.Id == "D").First();
            pr.Requirements.ElementAt(0).AllowedGrades =(await new TestGradeRepository().GetAsync()).Where(g => g.Id == "P" || g.Id == "AU").ToList();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            var newRequirementRepo = await BuildValidRequirementRepository(pr);

            var requirement = await newRequirementRepo.GetAsync("REQ2");
            // Make sure requirement exists and has min grade and allowed grades
            Assert.IsNotNull(requirement);
            Assert.IsNotNull(requirement.MinGrade);
            Assert.IsTrue(requirement.AllowedGrades.Count() > 0);

            // Verify requirement min/allowed grades have been cascaded into subreqs and groups
            foreach (var subreq in requirement.SubRequirements)
            {
                Assert.AreEqual(requirement.MinGrade, subreq.MinGrade);
                foreach (var grade in requirement.AllowedGrades)
                {
                    Assert.IsTrue(subreq.AllowedGrades.Contains(grade));
                }

                foreach (var group in subreq.Groups)
                {
                    Assert.AreEqual(requirement.MinGrade, group.MinGrade);
                    foreach (var grade in requirement.AllowedGrades)
                    {
                        Assert.IsTrue(group.AllowedGrades.Contains(grade));
                    }
                }
            }
        }

        [TestMethod]
        public async Task CascadesIncludeLowGradesInGPA_FromDefaults_ThroughAllRequirementLevels()
        {
            string id = "TestReq";
            reqs = new List<string>() { "REQ2" };
            sub1 = new List<string>() { "SUBREQ1" };
            grp1 = new List<string>() { "SIMPLE.ANY" };

            Dictionary<string, List<string>> Subrequirementnames = new Dictionary<string, List<string>>();
            Subrequirementnames.Add(reqs[0], sub1);
            Dictionary<string, List<string>> groupnames = new Dictionary<string, List<string>>();
            groupnames.Add(sub1[0], grp1);

            ProgramRequirements pr = await testProgramRequirementsRepo.BuildTestProgramRequirementsAsync(id, reqs, Subrequirementnames, groupnames);
            pr.Requirements.ElementAt(0).MinGrade =(await  new TestGradeRepository().GetAsync()).Where(g => g.Id == "D").First();
            pr.Requirements.ElementAt(0).AllowedGrades =(await  new TestGradeRepository().GetAsync()).Where(g => g.Id == "P" || g.Id == "AU").ToList();
            dataAccessorMock = new Mock<IColleagueDataReader>();
            var newRequirementRepo = await BuildValidRequirementRepository(pr);
            // Mock DADefaults response with Yes for Use Low Grades (IncludeFailures in Collague)
            DaDefaults daDefaults = new DaDefaults();
            dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(new DaDefaults() { DaIncludeFailures = "Y" }));

            var requirement = await newRequirementRepo.GetAsync("REQ2");
            // Make sure requirement exists and has min grade and allowed grades
            Assert.IsNotNull(requirement);
            Assert.IsNotNull(requirement.MinGrade);
            Assert.IsTrue(requirement.IncludeLowGradesInGpa);

            // Verify requirement min/allowed grades have been cascaded into subreqs and groups
            foreach (var subreq in requirement.SubRequirements)
            {
                Assert.AreEqual(requirement.MinGrade, subreq.MinGrade);
                foreach (var grade in requirement.AllowedGrades)
                {
                    Assert.IsTrue(subreq.IncludeLowGradesInGpa);
                }

                foreach (var group in subreq.Groups)
                {
                    Assert.AreEqual(requirement.MinGrade, group.MinGrade);
                    foreach (var grade in requirement.AllowedGrades)
                    {
                        Assert.IsTrue(group.IncludeLowGradesInGpa);
                    }
                }
            }
        }

        [TestMethod]
        public async Task RepoBuildsMaxCreditsAtLevels()
        {


            var programToBuild = await testProgramRequirementsRepo.GetAsync("MATH.BS.GROUP43", "2011");
            var newRequirementRepo = await BuildValidRequirementRepository(programToBuild);
            var requirements = await newRequirementRepo.GetAsync("BS.MATH");

            Assert.AreEqual(3, requirements.SubRequirements.First().Groups.First().MaxCreditsAtLevels.MaxCredit);

        }

        [TestMethod]
        public async Task LoadsMinGradeAndAllowedGradesForRequirement()
        {
            // This is the program requirements/subrequirements/groups to build
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            // Mock the responses
            var newRequirementRepo = await BuildValidRequirementRepository(programToBuild);
            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");
                
            // Only one requirement is built
            // Verify that there is a min grade and it is equal to the one in the test repo requirement
            Assert.IsNotNull(requirement.MinGrade);
            Assert.AreEqual(programToBuild.Requirements.ElementAt(0).MinGrade.Id, requirement.MinGrade.Id);
            // Verify that there are allowed grades and they are equal to the ones in the original requirement
            Assert.IsTrue(requirement.AllowedGrades.Count() > 0);
            foreach (var allowedGrade in programToBuild.Requirements.ElementAt(0).AllowedGrades)
            {
                Assert.IsNotNull(requirement.AllowedGrades.Where(a => a.Id == allowedGrade.Id));
            }
        }

        [TestMethod]
        public async Task LoadsMinGradeAndAllowedGradesForSubrequirement()
        {
            // This is the program requirements/subrequirements/groups to build
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            // Mock the responses
            var newRequirementRepo = await BuildValidRequirementRepository(programToBuild);
            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            // Verify that there is a min grade in the subrequirement and it is equal to the one in the test repo subrequirement
            Assert.IsNotNull(requirement.SubRequirements.ElementAt(0).MinGrade);
            Assert.AreEqual(programToBuild.Requirements.ElementAt(0).SubRequirements.ElementAt(0).MinGrade.Id, requirement.SubRequirements.ElementAt(0).MinGrade.Id);
            // Verify that there are allowed grades and they are equal to the ones in the original requirement
            var repoSubreqAllowedGrades = requirement.SubRequirements.ElementAt(0).AllowedGrades;
            Assert.IsTrue(repoSubreqAllowedGrades.Count() > 0);
            var testSubreq = programToBuild.Requirements.ElementAt(0).SubRequirements.ElementAt(0);
            foreach (var allowedGrade in testSubreq.AllowedGrades)
            {
                Assert.IsNotNull(repoSubreqAllowedGrades.Where(a => a.Id == allowedGrade.Id));
            }
        }

        [TestMethod]
        public async Task LoadsMinGradeAndAllowedGradesForGroup()
        {
            // This is the program requirements/subrequirements/groups to build
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            // Mock the responses
            var newRequirementRepo = await BuildValidRequirementRepository(programToBuild);
            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            // Verify that there is a min grade in the group and it is equal to the one in the test repo group
            Assert.IsNotNull(requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).MinGrade);
            Assert.AreEqual(programToBuild.Requirements.ElementAt(0).SubRequirements.ElementAt(0).Groups.ElementAt(0).MinGrade.Id, requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).MinGrade.Id);
            // Verify that there are allowed grades and they are equal to the ones in the original requirement
            var repoGroupAllowedGrades = requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).AllowedGrades;
            Assert.IsTrue(repoGroupAllowedGrades.Count() > 0);
            var testGroup = programToBuild.Requirements.ElementAt(0).SubRequirements.ElementAt(0).Groups.ElementAt(0);
            foreach (var allowedGrade in testGroup.AllowedGrades)
            {
                Assert.IsNotNull(repoGroupAllowedGrades.Where(a => a.Id == allowedGrade.Id));
            }            
        }

        [TestMethod]
        public async Task Requirement_UsesDefaultUseLowGradesOfNo()
        {
            // Requirement response returns empty value for the UseLowGrades field. Mocked da.defaults response is N,
            // so verify that the default ends up as false when the default is No
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            var newRequirementRepo =await BuildValidRequirementRepository(programToBuild);
            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            Assert.IsFalse(requirement.IncludeLowGradesInGpa);
            Assert.IsFalse(requirement.SubRequirements.ElementAt(0).IncludeLowGradesInGpa);
            Assert.IsFalse(requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).IncludeLowGradesInGpa);
        }
        
        [TestMethod]
        public async Task Requirement_UsesDefaultUseLowGradesOfYes()
        {
            // Requirement response returns empty value for the UseLowGrades field
            // so verify that the default ends up as True when default is Yes
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            var newRequirementRepo = await BuildValidRequirementRepository(programToBuild);
 
            // Override Mock DADefaults response with Yes
            DaDefaults daDefaults = new DaDefaults();
            dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(new DaDefaults() { DaIncludeFailures = "Y" }));

            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            Assert.IsTrue(requirement.IncludeLowGradesInGpa);
            Assert.IsTrue(requirement.SubRequirements.ElementAt(0).IncludeLowGradesInGpa);
            Assert.IsTrue(requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).IncludeLowGradesInGpa);
        }

        [TestMethod]
        public async Task Requirement_UsesDefaultUseLowGradesOfEmpty()
        {
            // Requirement response returns empty value for the UseLowGrades field
            // so verify that the default ends up as true when default is Blank
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            var newRequirementRepo =await BuildValidRequirementRepository(programToBuild);

            // Override Mock DADefaults response with Yes
            DaDefaults daDefaults = new DaDefaults();
            dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(new DaDefaults() { DaIncludeFailures = string.Empty }));

            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            Assert.IsTrue(requirement.IncludeLowGradesInGpa);
            Assert.IsTrue(requirement.SubRequirements.ElementAt(0).IncludeLowGradesInGpa);
            Assert.IsTrue(requirement.SubRequirements.ElementAt(0).Groups.ElementAt(0).IncludeLowGradesInGpa);
        }

        /// <summary>
        /// Verifies that the min grade and the allowed grades defined on the program requirement top level
        /// are cascaded down to the requirements, subrequirements and groups.
        /// </summary>
        [TestMethod]
        public async Task MinGradeAndAllowedGradesCascadeDownFromProgramRequirements()
        {
            var pr = await new TestProgramRequirementsRepository().GetAsync("MINGRADE_ALLOWEDGRADE_TOPLEVEL", "3000");
            var newRequirementRepo =await BuildValidRequirementRepository(pr);
            // Invoke repository to build requirements for the program
            var reqIds = pr.Requirements.Select(r => r.Id).ToList();
            var reqs = await newRequirementRepo.GetAsync(reqIds, pr);
            // Verify that the requirement, subrequirement and group inherited the min grade and allowed grades from the 
            // program requirements
            // First verify that a min grade and allowed grades have been set up in the program requirements.
            Assert.IsTrue(pr.MinGrade != null);
            Assert.AreEqual(2, pr.AllowedGrades.Count());
            foreach (var req in reqs)
            {
                // Verify program requirements updated into each requirement
                Assert.AreEqual(pr, req.ProgramRequirements);
                // Verify min grade/allowed grades in each requirement
                Assert.AreEqual(pr.MinGrade, req.MinGrade);
                foreach (var grade in pr.AllowedGrades)
                {
                    Assert.IsTrue(req.AllowedGrades.Contains(grade));
                }
                foreach (var subreq in req.SubRequirements)
                {
                    // verify min grade/allowed grades in each subrequirement
                    Assert.AreEqual(pr.MinGrade, subreq.MinGrade);
                    foreach (var grade in pr.AllowedGrades)
                    {
                        Assert.IsTrue(subreq.AllowedGrades.Contains(grade));
                    }
                    foreach (var group in subreq.Groups)
                    {
                        // verify min grade/allowed grades in each group
                        Assert.AreEqual(pr.MinGrade, group.MinGrade);
                        foreach (var grade in pr.AllowedGrades)
                        {
                            Assert.IsTrue(group.AllowedGrades.Contains(grade));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task AllowedGradesIncludesComparisonGrades()
        {
            // The requirement of this program allows "P". Since "P" has a comparison grade of "2" (see TestGradesRepository), "2" should
            // be added to the list of allowed grades.
            var programToBuild = await testProgramRequirementsRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES", "2011");
            var newRequirementRepo =await BuildValidRequirementRepository(programToBuild);

            // Override Mock DADefaults response with Yes
            DaDefaults daDefaults = new DaDefaults();
            dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(new DaDefaults() { DaIncludeFailures = string.Empty }));

            var requirement = await newRequirementRepo.GetAsync("MINGRADE_AND_ALLOWEDGRADES*2011");

            Assert.IsTrue(requirement.AllowedGrades.Select(g=>g.ComparisonGrade).Where(cg => cg != null).Select(cg => cg.ComparisonGradeId).Contains("2"));
            // since the subrequirement allows for the transfer grade of "1" (see TestGradesRepository) which 
            // has a comparison grade of "A", the "A" grade should be added to the allowed grades list
            Assert.IsTrue(requirement.SubRequirements.ElementAt(0).AllowedGrades.Select(g => g.ComparisonGrade).Where(g => g != null).Select(g => g.ComparisonGradeId).Contains("A"));
        }
        

        /// <summary>
        /// test for extracoursedirective is inherited properly
        /// </summary>
         [TestMethod]
        public async Task ValidateExtraCourseDirectiveSettings()
        {
            IEnumerable<String> codes = new List<String>() { "Math Core", "Engl Core" };
            var requirements = await requirementRepo.GetAsync(codes);
            Assert.AreEqual(2, requirements.Count());
             Assert.AreEqual(requirements.ToList()[0].ExtraCourseDirective,ExtraCourses.SemiApply);
             Assert.AreEqual(requirements.ToList()[0].SubRequirements[0].ExtraCourseDirective, ExtraCourses.Ignore);
        }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_DaDefaultsNull()
         {
             // Override Mock DADefaults response to return null
             DaDefaults daDefaults = null;
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();

             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Apply, daParams.ExtraCourseHandling);
            Assert.IsFalse(daParams.ShowRelatedCourses);
         }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_DaDefaultNullProperties()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults();
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Apply, daParams.ExtraCourseHandling);
            Assert.IsFalse(daParams.ShowRelatedCourses);
         }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_DaDefaultEmptyProperties()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaExtraCode = string.Empty, DaIncludeFailures = string.Empty, DaDefaultSortOverride = string.Empty };
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Apply, daParams.ExtraCourseHandling);
            Assert.IsFalse(daParams.ShowRelatedCourses);
         } 

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_LowerCaseTrues()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaIncludeFailures = "y", DaDefaultSortOverride = "y", DaExtraCode = "s", DaRelatedCoursesFlag="y" };
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsTrue(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.SemiApply, daParams.ExtraCourseHandling);
            Assert.IsTrue(daParams.ShowRelatedCourses);
        }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_LowerCaseFalses()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaIncludeFailures = "n", DaDefaultSortOverride = "n", DaExtraCode = "d" , DaRelatedCoursesFlag="n"};
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsFalse(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Display, daParams.ExtraCourseHandling);
            Assert.IsFalse(daParams.ShowRelatedCourses);
         }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_UpperCaseTrues()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaIncludeFailures = "Y", DaDefaultSortOverride = "Y", DaExtraCode = "I" , DaRelatedCoursesFlag="Y"};
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsTrue(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Ignore, daParams.ExtraCourseHandling);
            Assert.IsTrue(daParams.ShowRelatedCourses);
         }
         [TestMethod]
         public async Task ValidateDegreeAuditParameters_UpperCaseFalses()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaIncludeFailures = "N", DaDefaultSortOverride = "N", DaExtraCode = "A", DaRelatedCoursesFlag="N" };
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsFalse(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Apply, daParams.ExtraCourseHandling);
            Assert.IsFalse(daParams.ShowRelatedCourses);
         }

         [TestMethod]
         public async Task ValidateDegreeAuditParameters_BadValuesInProperties()
         {
             // Override Mock DADefaults response to return object with all null properties
             DaDefaults daDefaults = new DaDefaults() { DaIncludeFailures = "x", DaDefaultSortOverride = "x", DaExtraCode = "x", DaRelatedCoursesFlag="x" };
             dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(daDefaults));

             var daParams = await requirementRepo.GetDegreeAuditParametersAsync();
             // Expect all defaults
             Assert.IsTrue(daParams.UseLowGrade);
             Assert.IsFalse(daParams.ModifiedDefaultSort);
             Assert.AreEqual(ExtraCourses.Apply, daParams.ExtraCourseHandling);
             Assert.IsFalse(daParams.ShowRelatedCourses);
         }

        private async Task<RequirementRepository> BuildValidRequirementRepository(ProgramRequirements pr)
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();
            var loggerMock = new Mock<ILogger>();

            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

            // Cache mocking
            var cacheProviderMock = new Mock<ICacheProvider>();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(
             null,
            new SemaphoreSlim(1, 1)
            ));

            //cacheProviderMock.Setup(provider => provider.GetCache(_CacheName)).Returns(localCache);
            //cacheProviderMock.Setup(provider => provider.GetCache("Ellucian.Web.Student.Data.Colleague.Repository.RequirementRepository")).Returns(localCache);
            //cacheProviderMock.Setup(provider => provider.GetCache("Ellucian.Web.Student.Data.Colleague.Repository.GradeRepository")).Returns(localCache);

            // Mock up response for grade repository
            Collection<Grades> graderesp = await BuildGradeResp();
            dataAccessorMock.Setup<Task<Collection<Grades>>>(grds => grds.BulkReadRecordAsync<Grades>("GRADES", "", true)).Returns(Task.FromResult(graderesp));

            // Set up Requirement Responses, three possibilities
            // A: First requirement
            // B: Second requirement
            // C: Both requirements

            // The keys in arrays for the mock objects

            // "Name" holds the requirement codes for selecting against 
            //  ACAD.REQMTS
            string[] reqNameArrayA = { pr.Requirements.First().Code };
            string[] reqNameArrayB = { pr.Requirements.Last().Code };
            string[] reqNameArrayC = reqNameArrayA.Concat(reqNameArrayB).ToArray();

            // Everything else is in ACAD.REQMT.BLOCKS by ID
            string[] reqArrayA = { pr.Requirements.First().Id };
            string[] reqArrayB = { pr.Requirements.Last().Id };
            string[] reqArrayC = reqArrayA.Concat(reqArrayB).ToArray();
            string[] reqArrayD = new List<String>() { "xxx", "yyy" }.ToArray() ;

            IEnumerable<Subrequirement> subsA = pr.Requirements.First().SubRequirements;
            IEnumerable<Subrequirement> subsB = pr.Requirements.Last().SubRequirements;

            string[] subArrayA = subsA.Select(sa => sa.Id).ToArray();
            string[] subArrayB = subsB.Select(sb => sb.Id).ToArray();
            string[] subArrayC = subArrayA.Concat(subArrayB).ToArray();

            IEnumerable<Group> grpsA = subsA.SelectMany(sa => sa.Groups);
            IEnumerable<Group> grpsB = subsB.SelectMany(sb => sb.Groups);

            string[] grpArrayA = grpsA.Select(ga => ga.Id).ToArray();
            string[] grpArrayB = grpsB.Select(gb => gb.Id).ToArray();
            string[] grpArrayC = grpArrayA.Concat(grpArrayB).ToArray();
            

            // ACAD.REQMT.BLOCKS responses
            var reqResponseA = BuildBlockResponse("R", pr.Requirements.First());
            var reqResponseB = BuildBlockResponse("R", pr.Requirements.Last());
            var reqResponseC = BuildBlockResponse("R", pr.Requirements);

            var subResponseA = BuildBlockResponse("S", pr.Requirements.First());
            var subResponseB = BuildBlockResponse("S", pr.Requirements.Last());
            var subResponseC = BuildBlockResponse("S", pr.Requirements);

            var grpResponseA = BuildBlockResponse("G", pr.Requirements.First());
            var grpResponseB = BuildBlockResponse("G", pr.Requirements.Last());
            var grpResponseC = BuildBlockResponse("G", pr.Requirements);

            // ACAD.REQMTS responses
            var areqResponseA = BuildAcadReqmtsResponse(pr.Requirements.First());
            var areqResponseB = BuildAcadReqmtsResponse(pr.Requirements.Last());
            var areqResponseC = BuildAcadReqmtsResponse(pr.Requirements);


            // Set up response for a single student request for student programs
            //example
            //dataAccessorMock.Setup<Collection<StudentPrograms>>(acc => acc.BulkReadRecord<StudentPrograms>("STUDENT.PROGRAMS", "STPR.STUDENT EQ '0000894' AND STPR.STATUS EQ 'A'")).Returns(StudentProgramResponseData);

            //rqs = dataAccessor.BulkReadRecord<AcadReqmts>("ACAD.REQMTS", requirementCodes.ToArray());  (actual call in repo)
            // This does not mirror reality, but initialize sets up requirements using code, so this setup is needed for some tests, whereas the ID mock
            // just below is needed for others.
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqNameArrayA, true)).Returns(Task.FromResult(areqResponseA));
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqNameArrayB, true)).Returns(Task.FromResult(areqResponseB));
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqNameArrayC, true)).Returns(Task.FromResult(areqResponseC));
            // The get for ACAD.REQMTS will use requirement ID
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqArrayA, true)).Returns(Task.FromResult(areqResponseA));
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqArrayB, true)).Returns(Task.FromResult(areqResponseB));
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqArrayC, true)).Returns(Task.FromResult(areqResponseC));
            dataAccessorMock.Setup<Task<Collection<AcadReqmts>>>(acc => acc.BulkReadRecordAsync<AcadReqmts>("ACAD.REQMTS", reqArrayD, true)).Returns(Task.FromResult(default(Collection<AcadReqmts>)));

            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", reqArrayA, true)).Returns(Task.FromResult(reqResponseA));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", reqArrayB, true)).Returns(Task.FromResult(reqResponseB));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", reqArrayC, true)).Returns(Task.FromResult(reqResponseC));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", new string[]{}, true)).Returns(Task.FromResult(default(Collection<AcadReqmtBlocks>)));

            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", subArrayA, false)).Returns(Task.FromResult(subResponseA));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", subArrayB, false)).Returns(Task.FromResult(subResponseB));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", subArrayC, false)).Returns(Task.FromResult(subResponseC));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", new string[] { }, false)).Returns(Task.FromResult(default(Collection<AcadReqmtBlocks>)));

            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", grpArrayA, true)).Returns(Task.FromResult(grpResponseA));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", grpArrayB, true)).Returns(Task.FromResult(grpResponseB));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", grpArrayC, true)).Returns(Task.FromResult(grpResponseC));
            dataAccessorMock.Setup<Task<Collection<AcadReqmtBlocks>>>(acc => acc.BulkReadRecordAsync<AcadReqmtBlocks>("ACAD.REQMT.BLOCKS", new string[] { }, true)).Returns(Task.FromResult(default(Collection<AcadReqmtBlocks>)));

            // Mock DADefaults response
            DaDefaults daDefaults = new DaDefaults();
            dataAccessorMock.Setup<Task<DaDefaults>>(acc => acc.ReadRecordAsync<DaDefaults>("ST.PARMS", "DA.DEFAULTS", true)).Returns(Task.FromResult(new DaDefaults() { DaIncludeFailures = "N" }));

            // Mocking to get RequirementTypes
            ApplValcodes acadReqmtTypesResponse = new ApplValcodes()
            {
                ValsEntityAssociation = new List<ApplValcodesVals>() {new ApplValcodesVals() { ValInternalCodeAssocMember = "MAJ", ValExternalRepresentationAssocMember = "Major", ValActionCode1AssocMember = "1" }, // Sequence == 1
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "GEN", ValExternalRepresentationAssocMember = "General", ValActionCode1AssocMember = "2"},   // Sequence == 2
                                                                   new ApplValcodesVals() { ValInternalCodeAssocMember = "OTH", ValExternalRepresentationAssocMember = "Other", ValActionCode1AssocMember = ""}}      // Sequence == Max Int
            };
            dataAccessorMock.Setup<Task<ApplValcodes>>(cacc => cacc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "ACAD.REQMT.TYPES", true)).ReturnsAsync(acadReqmtTypesResponse);


            // Construct repository
            requirementRepo = new RequirementRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, new TestGradeRepository(), new TestRuleRepository2());

            return requirementRepo;
        }

        private Collection<AcadReqmts> BuildAcadReqmtsResponse(Requirement req)
        {
            return BuildAcadReqmtsResponse(new List<Requirement>() { req });
        }


        private Collection<AcadReqmts> BuildAcadReqmtsResponse(IEnumerable<Requirement> reqs)
        {
            Collection<AcadReqmts> reqmts = new Collection<AcadReqmts>();

            foreach (Requirement r in reqs)
            {
                AcadReqmts ac = new AcadReqmts();
                ac.Recordkey = r.Code;
                ac.AcrDesc = r.Description;
                ac.AcrGradeScheme = r.GradeSchemeCode;
                ac.AcrTopReqmtBlock = r.Id;
                if (r.RequirementType == null) // Case of a requisite requirement
                {
                    ac.AcrType = string.Empty;
                }
                else
                {
                    ac.AcrType = r.RequirementType.Code;
                }
                ac.AcrAcadProgramReqmts = new List<string>();  // unused.

                reqmts.Add(ac);
            }

            return reqmts;
        }

        private Collection<AcadReqmtBlocks> BuildBlockResponse(string blktype, Requirement req)
        {
            return BuildBlockResponse(blktype, new List<Requirement>() { req });
        }


        private Collection<AcadReqmtBlocks> BuildBlockResponse(string blktype, IEnumerable<Requirement> reqs)
        {
            Collection<AcadReqmtBlocks> respBlocks = new Collection<AcadReqmtBlocks>();
            foreach (var req in reqs)
            {
                // Requirement level
                var blkdata = new AcadReqmtBlocks();
                string topblockid = req.Id;
                blkdata.Recordkey = req.Id;

                blkdata.AcrbAcadCredRules = new List<string>();
                foreach (var ruletext in req.AcademicCreditRules)
                {
                    blkdata.AcrbAcadCredRules.Add(ruletext.ToString());
                }
                blkdata.AcrbAcadReqmt = topblockid;
                //blkdata.AcrbParentBlock = 

                blkdata.AcrbAllowedGrades = new List<string>();
                foreach (var grd in req.AllowedGrades)
                {
                    blkdata.AcrbAllowedGrades.Add(grd.Id);
                }
                blkdata.AcrbSubblocks = new List<string>();
                foreach (var sr in req.SubRequirements)
                {
                    blkdata.AcrbSubblocks.Add(sr.Id);
                }

                blkdata.AcrbMinNoSubblocks = req.MinSubRequirements;
                AddSVToBlock(req, blkdata);
                AddSVToBlockBase(req, blkdata);
                if (blktype == "R")
                {
                    respBlocks.Add(blkdata);
                }

                List<Group> grplist = new List<Group>(); //collect groups while looping through Subrequirements

                // Subrequirement level
                foreach (var sr in req.SubRequirements)
                {
                    var srblkdata = new AcadReqmtBlocks();

                    grplist.AddRange(sr.Groups);
                    srblkdata.AcrbSubblocks = new List<string>();
                    srblkdata.AcrbSubblocks.AddRange(sr.Groups.Select(g => g.Id));


                    AddSVToBlockBase(sr, srblkdata);
                    AddSVToBlock(sr, srblkdata);

                    srblkdata.AcrbParentBlock = req.Id;  
                    // I think this is the only time we need this, the
                    // parent-child relationship is gleaned elsewhere except
                    // in this case.
                    srblkdata.AcrbLabel = sr.Code;

                    srblkdata.Recordkey = sr.Id;
                    if (blktype == "S")
                    {
                        respBlocks.Add(srblkdata);
                    }
                }

                // Group level
                foreach (var gr in grplist)
                {
                    var grblkdata = new AcadReqmtBlocks();
                    grblkdata.AcrbAcadCredRules = new List<string>();
                    grblkdata.AcrbAllowedGrades = new List<string>();
                    grblkdata.AcrbCourses = new List<string>();
                    grblkdata.AcrbSubblocks = new List<string>();
                    grblkdata.AcrbStudentDaExcpts = new List<string>();
                    grblkdata.AcrbFromCourses = new List<string>();
                    grblkdata.AcrbButNotCourses = new List<string>();
                    grblkdata.AcrbFromDepts = new List<string>();
                    grblkdata.AcrbButNotDepts = new List<string>();
                    grblkdata.AcrbFromSubjects = new List<string>();
                    grblkdata.AcrbFromCrsLevels = new List<string>();
                    grblkdata.AcrbButNotCrsLevels = new List<string>();
                    grblkdata.AcrbNoLevelCredLevels = new List<string>();
                    grblkdata.AcrbNoLevelCoursesLevels = new List<string>();
                    grblkdata.AcrbButNotSubjects = new List<string>();
                    grblkdata.AcrbExclReqmtTypes = new List<string>();
                    grblkdata.AcrbExclFirstOnlyFlags = new List<string>();
                    

                    AddSVToBlockBase(gr, grblkdata);
                    grblkdata.Recordkey = gr.Id;
                    grblkdata.AcrbLabel = gr.Code;

                    grblkdata.AcrbMaxCoursesPerDept = gr.MaxCoursesPerDepartment;
                    grblkdata.AcrbMaxCoursesPerRule = gr.MaxCoursesPerRule;
                    grblkdata.AcrbMaxCoursesRules = gr.MaxCoursesRule == null ? "" : gr.MaxCoursesRule.ToString();
                    grblkdata.AcrbMaxCoursesPerSubject = gr.MaxCoursesPerSubject;
                    grblkdata.AcrbMaxCred = gr.MaxCredits;
                    grblkdata.AcrbMaxCredPerCourse = gr.MaxCreditsPerCourse;
                    grblkdata.AcrbMaxCredPerSubject = gr.MaxCreditsPerSubject;
                    grblkdata.AcrbMaxCredPerRule = gr.MaxCreditsPerRule;
                    grblkdata.AcrbMaxCredPerDept = gr.MaxCreditsPerDepartment;
                    grblkdata.AcrbMaxCredRules = gr.MaxCreditsRule == null ? "" : gr.MaxCreditsRule.ToString();
                    grblkdata.AcrbMaxNoCourses = gr.MaxCourses;
                    grblkdata.AcrbMaxNoDepts = gr.MaxDepartments;
                    grblkdata.AcrbMaxNoSubjects = gr.MaxSubjects;

                    grblkdata.AcrbMinCoursesPerDept = gr.MinCoursesPerDepartment;
                    grblkdata.AcrbMinCoursesPerSubject = gr.MinCoursesPerSubject;
                    grblkdata.AcrbMinCred = gr.MinCredits;
                    grblkdata.AcrbMinCredPerCourse = gr.MinCreditsPerCourse;
                    grblkdata.AcrbMinCredPerDept = gr.MinCreditsPerDepartment;
                    grblkdata.AcrbMinCredPerSubject = gr.MinCreditsPerSubject;
                    grblkdata.AcrbMinNoCourses = gr.MinCourses;
                    grblkdata.AcrbMinNoDepts = gr.MinDepartments;
                    grblkdata.AcrbMinNoSubjects = gr.MinSubjects;
                    List<string> crslist = (List<string>)gr.Courses;
                    grblkdata.AcrbCourses = crslist.ConvertAll<string>(x => x.ToString());
                    List<string> fcrslist = (List<string>)gr.FromCourses;
                    grblkdata.AcrbFromCourses = fcrslist.ConvertAll<string>(x => x.ToString());
                    List<string> acruleslist = gr.AcademicCreditRules.Select(acr => acr.CreditRule.Id).ToList();
                    grblkdata.AcrbAcadCredRules = acruleslist.ConvertAll<string>(x => x.ToString());


                    List<string> gradeList = new List<string>();
                    foreach (var grd in gr.AllowedGrades)
                    {
                        gradeList.Add(grd.Id);
                    }
                    grblkdata.AcrbAllowedGrades = gradeList;
                    List<string> bcrslist = (List<string>)gr.ButNotCourses;
                    grblkdata.AcrbButNotCourses = bcrslist.ConvertAll<string>(x => x.ToString());
                    grblkdata.AcrbFromDepts = (List<string>)gr.FromDepartments;
                    grblkdata.AcrbButNotDepts = (List<string>)gr.ButNotDepartments;
                    grblkdata.AcrbFromSubjects = (List<string>)gr.FromSubjects;
                    grblkdata.AcrbFromCrsLevels = (List<string>)gr.FromLevels;
                    grblkdata.AcrbButNotCrsLevels = (List<string>)gr.ButNotCourseLevels;
                    if (gr.MaxCoursesAtLevels != null)
                    {
                        grblkdata.AcrbNoLevelCoursesLevels = (List<string>)gr.MaxCoursesAtLevels.Levels;
                        grblkdata.AcrbNoLevelNoCourses = gr.MaxCoursesAtLevels.MaxCourses;
                    }
                    if (gr.MaxCreditsAtLevels != null)
                    {
                        grblkdata.AcrbNoLevelCredLevels = (List<string>)gr.MaxCreditsAtLevels.Levels;
                        grblkdata.AcrbNoLevelCred = gr.MaxCreditsAtLevels.MaxCredit;
                    }
                    grblkdata.AcrbButNotSubjects = (List<string>)gr.ButNotSubjects;


                    if (blktype == "G")
                    {
                        respBlocks.Add(grblkdata);
                    }
                }

            }
            return respBlocks;
        }

        private void AddSVToBlockBase(BlockBase r, AcadReqmtBlocks block)
        {
            switch (r.ExtraCourseDirective)
            {
                case ExtraCourses.Ignore:
                    block.AcrbExtraCode = _IgnoreCode;
                    break;
                case ExtraCourses.Display:
                    block.AcrbExtraCode = _DisplayCode;
                    break;
                case ExtraCourses.Apply:
                    block.AcrbExtraCode = _ApplyCode;
                    break;
                default:
                    block.AcrbExtraCode = _ApplyCode;
                    break;
            }

            block.AcrbIncludeFailures = r.IncludeLowGradesInGpa ? "" : "N";
            block.AcrbInstitutionCred = r.MinInstitutionalCredits;
            block.AcrbMinGpa = r.MinGpa;
            if (r.MinGrade != null)
            block.AcrbMinGrade = r.MinGrade.Id;
            if (r.AllowedGrades != null)
            block.AcrbAllowedGrades = r.AllowedGrades.Select(a=>a.Id).ToList();

            block.AcrbType = r.InternalType;
        }

        private void AddSVToBlock(RequirementBlock r, AcadReqmtBlocks block)
        {
            block.AcrbCourseReuseFlag = r.AllowsCourseReuse ? "Y" : "N";
            block.AcrbMergeMethod = r.WaitToMerge ? "Y" : "";
        }

        private async Task<Collection<Grades>> BuildGradeResp()
        {
            var grades = new Collection<Grades>();
            IEnumerable<Grade> AllGrades =(await  new TestGradeRepository().GetAsync());
            foreach (var grd in AllGrades)
            {
                Grades g = new Grades();

                g.Recordkey = grd.Id;
                g.GrdGrade = grd.LetterGrade;
                g.GrdLegend = grd.Description;
                g.GrdGradeScheme = grd.GradeSchemeCode;
                g.GrdValue = grd.GradeValue;
                g.GrdComparisonGrade = (grd.ComparisonGrade == null) ? null : grd.ComparisonGrade.ComparisonGradeId;
                grades.Add(g);

            }
            return grades;
        }

    }

}
