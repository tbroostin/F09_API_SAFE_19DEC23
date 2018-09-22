// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class ProgramRequirementsTests
    {
        [TestClass]
        public class Constructor
        {
            private ProgramRequirements progReq;
            private string programCode;
            private string catalogCode;

            [TestInitialize]
            public void Initialize()
            {
                programCode = "BA.ENGL";
                catalogCode = "2013";
                progReq = new ProgramRequirements(programCode, catalogCode);
            }

            [TestMethod]
            public void ProgramCode()
            {
                Assert.AreEqual(progReq.ProgramCode, programCode);
            }

            [TestMethod]
            public void CatalogCode()
            {
                Assert.AreEqual(progReq.CatalogCode, catalogCode);
            }

            [TestMethod]
            public void RequirementsEmpty()
            {
                Assert.AreEqual(0, progReq.Requirements.Count());    
            }

            [TestMethod]
            public void ActivityElibilityRulesEmpty()
            {
                Assert.AreEqual(0, progReq.ActivityEligibilityRules.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfProgramCodeEmpty()
            {
                new ProgramRequirements(string.Empty, catalogCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfProgramCodeNull()
            {
                new ProgramRequirements(null, catalogCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCatalogCodeEmpty()
            {
                new ProgramRequirements(programCode, string.Empty);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionIfCatalogCodeNull()
            {
                new ProgramRequirements(programCode, null);
            }
        }

        [TestClass]
        public class GetAllRules
        {
            private ProgramRequirements progReq;
            private string programCode;
            private string catalogCode;
            private Requirement r1;
            private string id;
            private string requirementCode;
            private string description;
            private string gradeSchemeCode;
            private RequirementType requirementTypeCode;
            private Subrequirement sr1;
            private Group g1;
            private List<RequirementType> requirementTypes = new TestRequirementRepository().RequirementTypes;

            [TestInitialize]
            public void Initialize()
            {
                // Create program requirement
                programCode = "BA.ENGL";
                catalogCode = "2013";
                progReq = new ProgramRequirements(programCode, catalogCode);
                progReq.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("PRULE1")));
                progReq.ActivityEligibilityRules.Add(new RequirementRule(new Rule<AcademicCredit>("PRULE2")));
                // Create and add requirement
                id = "10000";
                requirementCode = "CORE.MATH";
                description = "Core requirements for all undergraduate degrees in math";
                gradeSchemeCode = "UG";
                requirementTypeCode = requirementTypes.First(rt=>rt.Code == "MAJ");
                r1 = new Requirement(id, requirementCode, description, gradeSchemeCode, requirementTypeCode);
                r1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("RULE1")));
                progReq.Requirements.Add(r1);
                // Add subrequirement to requirement
                sr1 = new Subrequirement("2", "SUBREQ1");
                sr1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("SRULE1")));
                sr1.Requirement = r1;
                r1.SubRequirements.Add(sr1);
                // Add group to subrequirement
                g1 = new Group("G1", "GROUP1", sr1);
                g1.AcademicCreditRules.Add(new RequirementRule(new Rule<AcademicCredit>("GRULE1")));
                sr1.Groups.Add(g1);
            }

            [TestMethod]
            public void GetsRulesAtAllLevels()
            {
                var rules = progReq.GetAllRules();
                Assert.AreEqual(5, rules.Count());
                Assert.IsTrue(rules.Contains(progReq.ActivityEligibilityRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(progReq.ActivityEligibilityRules.ElementAt(1)));
                Assert.IsTrue(rules.Contains(r1.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(sr1.AcademicCreditRules.ElementAt(0)));
                Assert.IsTrue(rules.Contains(g1.AcademicCreditRules.ElementAt(0)));
            }

            [TestMethod]
            public void ReturnsEmptyListIfNoRulesOrRequirements()
            {
                progReq = new ProgramRequirements(programCode, catalogCode);
                var rules = progReq.GetAllRules();
                Assert.AreEqual(0, rules.Count());
            }
        }

        [TestClass]
        public class ProgramRequirements_Properties
        {
            private ProgramRequirements progReq;
            private string programCode;
            private string catalogCode;

            [TestInitialize]
            public void Initialize()
            {
                programCode = "BA.ENGL";
                catalogCode = "2013";
                progReq = new ProgramRequirements(programCode, catalogCode);
            }

            [TestMethod]
            public void ProgramRequirements_RequirementToPrintCcdsAfter_Get_Set()
            {
                Assert.IsNull(progReq.RequirementToPrintCcdsAfter);
                progReq.RequirementToPrintCcdsAfter = "ABCD";
                Assert.AreEqual("ABCD", progReq.RequirementToPrintCcdsAfter);
            }

            [TestMethod]
            public void ProgramRequirements_RequirementToPrintMajorsAfter_Get_Set()
            {
                Assert.IsNull(progReq.RequirementToPrintMajorsAfter);
                progReq.RequirementToPrintMajorsAfter = "ABCD";
                Assert.AreEqual("ABCD", progReq.RequirementToPrintMajorsAfter);
            }

            [TestMethod]
            public void ProgramRequirements_RequirementToPrintMinorsAfter_Get_Set()
            {
                Assert.IsNull(progReq.RequirementToPrintMinorsAfter);
                progReq.RequirementToPrintMinorsAfter = "ABCD";
                Assert.AreEqual("ABCD", progReq.RequirementToPrintMinorsAfter);
            }

            [TestMethod]
            public void ProgramRequirements_RequirementToPrintSpecializationsAfter_Get_Set()
            {
                Assert.IsNull(progReq.RequirementToPrintSpecializationsAfter);
                progReq.RequirementToPrintSpecializationsAfter = "ABCD";
                Assert.AreEqual("ABCD", progReq.RequirementToPrintSpecializationsAfter);
            }
        }
    }
}
