using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class RequirementTests
    {
        [TestClass]
        public class RequirementConstructor
        {
            private Requirement r;
            private string id;
            private string requirementcode;
            private string description;
            private string gradeschemecode;
            private RequirementType requirementType;
            private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

            [TestInitialize]
            public void Initialize()
            {
                id = "10000";
                requirementcode = "CORE.MATH";
                description = "Core requirements for all undergraduate degrees in math";
                gradeschemecode = "UG";
                requirementType =  requirementTypes.First(rt=>rt.Code == "MAJ");
                r = new Requirement(id, requirementcode, description, gradeschemecode, requirementType);
            }

            [TestMethod]
            public void Id()
            {
                Assert.AreEqual(id, r.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RequirementIdNullException()
            {
                r = new Requirement(null, requirementcode, description, gradeschemecode, requirementType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RequirementCodeNullException()
            {
                r = new Requirement(id, null, description, gradeschemecode, requirementType);
            }

            [TestMethod]
            public void Description()
            {
                Assert.AreEqual(description, r.Description);
            }

            [TestMethod]
            public void GradeSchemeCode()
            {
                Assert.AreEqual(gradeschemecode, r.GradeSchemeCode);
            }

            [TestMethod]
            public void RequirementTypeCode()
            {
                Assert.AreEqual(requirementType, r.RequirementType);
            }

        }

        [TestClass]
        public class GetRules
        {
            private Requirement r;
            private string id;
            private string requirementCode;
            private string description;
            private string gradeSchemeCode;
            private RequirementType requirementType;

            [TestInitialize]
            public void Initialize()
            {
                id = "10000";
                requirementCode = "CORE.MATH";
                description = "Core requirements for all undergraduate degrees in math";
                gradeSchemeCode = "UG";
                requirementType = new TestRequirementRepository().RequirementTypes.Where(rt => rt.Code == "MAJ").FirstOrDefault();
                r = new Requirement(id, requirementCode, description, gradeSchemeCode, requirementType);
            }

            [TestMethod]
            public void ReturnsEmptyListIfEmpty()
            {
                var rules = r.GetRules();
                Assert.AreEqual(0, rules.Count());
            }

            [TestMethod]
            public void ReturnsEmptyListIfNull()
            {
                r.AcademicCreditRules = null;
                var rules = r.GetRules();
                Assert.AreEqual(0, rules.Count());
                
            }
        }

        [TestClass]
        public class GetAllRules
        {
            private Requirement r1;
            private string id;
            private string requirementCode;
            private string description;
            private string gradeSchemeCode;
            private Subrequirement sr1;
            private Subrequirement sr2;
            private Group g1;
            private Group g2;
            private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

            [TestInitialize]
            public void Initialize()
            {
                id = "10000";
                requirementCode = "CORE.MATH";
                description = "Core requirements for all undergraduate degrees in math";
                gradeSchemeCode = "UG";
                r1 = new Requirement(id, requirementCode, description, gradeSchemeCode, requirementTypes.First(rt=>rt.Code == "MAJ"));
                r1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("RULE1")));
                r1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("RULE2")));
                sr1 = new Subrequirement("2", "SUBREQ1");
                sr1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("SRULE1")));
                sr1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("SRULE2")));
                sr1.Requirement = r1;
                r1.SubRequirements.Add(sr1);
                sr2 = new Subrequirement("3", "SUBREQ2");
                sr2.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("SRULE3")));
                sr2.Requirement = r1;
                r1.SubRequirements.Add(sr2);
                g1 = new Group("G1", "GROUP1", sr1);
                g1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("GRULE1")));
                g1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("GRULE2")));
                sr2.Groups.Add(g1);
                g2 = new Group("G2", "GROUP2", sr1);
                g2.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("GRULE3")));
                sr2.Groups.Add(g2);

            }

            [TestMethod]
            public void GetsRulesFromAllLevels()
            {
                var rules = r1.GetAllRules();
                Assert.AreEqual(8, rules.Count());
                Assert.IsTrue(rules.Contains(r1.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(r1.AcademicCreditRules.ElementAt(1)));
                Assert.IsTrue(rules.Contains(sr1.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(sr1.AcademicCreditRules.ElementAt(1)));
                Assert.IsTrue(rules.Contains(sr2.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(g1.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(g1.AcademicCreditRules.ElementAt(1)));
                Assert.IsTrue(rules.Contains(g2.AcademicCreditRules.ElementAt(0)));

            }

            [TestClass]
            public class Requirement_CascadeSortSpecificationWhenNecessary
            {
                private Requirement r;
                private string id;
                private string requirementcode;
                private string description;
                private string gradeschemecode;
                private RequirementType requirementType;
                private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

                [TestInitialize]
                public void Initialize()
                {
                    id = "10000";
                    requirementcode = "CORE.MATH";
                    description = "Core requirements for all undergraduate degrees in math";
                    gradeschemecode = "UG";
                    requirementType = requirementTypes.First(rt => rt.Code == "MAJ");
                    
                }

                [TestCleanup]
                public void Cleanup()
                {
                    r = null;
                }

                [TestMethod]
                public void CascadeSortSpecificationWhenNecessary_ReqHasSort()
                {
                    // Setup
                    r = new Requirement(id, requirementcode, description, gradeschemecode, requirementType) { SortSpecificationId = "RSORT" };
                    Subrequirement s1 = new Subrequirement("S1", "SUBTEST_WITHSORT") { SortSpecificationId = "S1SORT" };
                    s1.Groups.Add(new Group("G1", "GROUP1_NOSORT", s1) { FromSubjects = new List<string>() { "MATH" } });
                    s1.Groups.Add(new Group("G2", "GROUP2_WITHSORT", s1) { FromSubjects = new List<string>() { "MATH" }, SortSpecificationId = "G2SORT" });
                    r.SubRequirements.Add(s1);
                    Subrequirement s2 = new Subrequirement("S2", "SUBTEST_NOSORT");
                    s2.Groups.Add(new Group("G3", "GROUP3_NOSORT", s2) { FromSubjects = new List<string>() { "MATH" } });
                    s2.Groups.Add(new Group("G4", "GROUP4_WITHSORT", s2) { FromSubjects = new List<string>() { "MATH" }, SortSpecificationId = "G4SORT" });
                    r.SubRequirements.Add(s2);

                    // Take Action                 
                    r.CascadeSortSpecificationWhenNecessary();
                    
                    // Verify
                    Assert.AreEqual("RSORT", r.SortSpecificationId);
                    var rS1 = r.SubRequirements.ElementAt(0);
                    Assert.AreEqual("S1SORT", rS1.SortSpecificationId);
                    var s1g1 = rS1.Groups.Where(g => g.Id == "G1").FirstOrDefault();
                    Assert.AreEqual("S1SORT", s1g1.SortSpecificationId);
                    var s1g2 = rS1.Groups.Where(g => g.Id == "G2").FirstOrDefault();
                    Assert.AreEqual("G2SORT", s1g2.SortSpecificationId);
                    var rS2 = r.SubRequirements.ElementAt(1);
                    Assert.AreEqual("RSORT", rS2.SortSpecificationId);
                    var s2g3 = rS2.Groups.Where(g => g.Id == "G3").FirstOrDefault();
                    Assert.AreEqual("RSORT", s2g3.SortSpecificationId);
                    var s2g4 = rS2.Groups.Where(g => g.Id == "G4").FirstOrDefault();
                    Assert.AreEqual("G4SORT", s2g4.SortSpecificationId);
                }
                [TestMethod]
                public void CascadeSortSpecificationWhenNecessary_ReqHasNoSort_StillCascades()
                {
                    // Setup
                    r = new Requirement(id, requirementcode, description, gradeschemecode, requirementType);
                    Subrequirement s1 = new Subrequirement("S1", "SUBTEST_WITHSORT") { SortSpecificationId = "S1SORT" };
                    s1.Groups.Add(new Group("G1", "GROUP1_NOSORT", s1) { FromSubjects = new List<string>() { "MATH" } });
                    s1.Groups.Add(new Group("G2", "GROUP2_WITHSORT", s1) { FromSubjects = new List<string>() { "MATH" }, SortSpecificationId = "G2SORT" });
                    r.SubRequirements.Add(s1);
                    Subrequirement s2 = new Subrequirement("S2", "SUBTEST_NOSORT");
                    s2.Groups.Add(new Group("G3", "GROUP3_NOSORT", s2) { FromSubjects = new List<string>() { "MATH" } });
                    s2.Groups.Add(new Group("G4", "GROUP4_WITHSORT", s2) { FromSubjects = new List<string>() { "MATH" }, SortSpecificationId = "G4SORT" });
                    r.SubRequirements.Add(s2);

                    // Take Action                 
                    r.CascadeSortSpecificationWhenNecessary();

                    // Verify
                    Assert.IsNull(r.SortSpecificationId);
                    var rS1 = r.SubRequirements.ElementAt(0);
                    Assert.AreEqual("S1SORT", rS1.SortSpecificationId);
                    var s1g1 = rS1.Groups.Where(g => g.Id == "G1").FirstOrDefault();
                    Assert.AreEqual("S1SORT", s1g1.SortSpecificationId);
                    var s1g2 = rS1.Groups.Where(g => g.Id == "G2").FirstOrDefault();
                    Assert.AreEqual("G2SORT", s1g2.SortSpecificationId);
                    var rS2 = r.SubRequirements.ElementAt(1);
                    Assert.IsNull(rS2.SortSpecificationId);
                    var s2g3 = rS2.Groups.Where(g => g.Id == "G3").FirstOrDefault();
                    Assert.IsNull(s2g3.SortSpecificationId);
                    var s2g4 = rS2.Groups.Where(g => g.Id == "G4").FirstOrDefault();
                    Assert.AreEqual("G4SORT", s2g4.SortSpecificationId);
                }

                [TestMethod]
                public void CascadeSortSpecificationWhenNecessary_ReqHasNoSubrequirements()
                {
                    // Setup
                    r = new Requirement(id, requirementcode, description, gradeschemecode, requirementType) { SortSpecificationId = "RSORT" };

                    // Take Action                 
                    r.CascadeSortSpecificationWhenNecessary();

                    // Verify
                    Assert.AreEqual("RSORT", r.SortSpecificationId);
                    
                }

                [TestMethod]
                public void CascadeSortSpecificationWhenNecessary_SubReqHasNoGroups()
                {
                    // Setup
                    r = new Requirement(id, requirementcode, description, gradeschemecode, requirementType) { SortSpecificationId = "RSORT" };
                    Subrequirement s1 = new Subrequirement("S1", "SUBTEST_WITHSORT") { SortSpecificationId = "S1SORT" };
                    r.SubRequirements.Add(s1);
                    Subrequirement s2 = new Subrequirement("S2", "SUBTEST_NOSORT");
                    r.SubRequirements.Add(s2);

                    // Take Action                 
                    r.CascadeSortSpecificationWhenNecessary();

                    // Verify
                    Assert.AreEqual("RSORT", r.SortSpecificationId);
                    var rS1 = r.SubRequirements.ElementAt(0);
                    Assert.AreEqual("S1SORT", rS1.SortSpecificationId);
                    var rS2 = r.SubRequirements.ElementAt(1);
                    Assert.AreEqual("RSORT", rS2.SortSpecificationId);

                }
            }
        }
    }
}
