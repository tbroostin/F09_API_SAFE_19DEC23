// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Planning.Tests.Entities
{
    [TestClass]
    public class BlockResultTests
    {

        private AcademicCredit ac1 = new AcademicCredit("1");
        private AcademicCredit ac2 = new AcademicCredit("2");
        private AcademicCredit ac3 = new AcademicCredit("3");
        private AcademicCredit ac4 = new AcademicCredit("4");
        private AcademicCredit ac5 = new AcademicCredit("5");
        private AcademicCredit ac6 = new AcademicCredit("6");
        private AcademicCredit ac7 = new AcademicCredit("7");
        private AcademicCredit ac8 = new AcademicCredit("8");
        private AcademicCredit ac9 = new AcademicCredit("9");
        private AcademicCredit acA = new AcademicCredit("A");
        private AcademicCredit acB = new AcademicCredit("B");

        private GroupResult gr1;
        private GroupResult gr2;
        private GroupResult gr3;
        private GroupResult gr4;
        private GroupResult gr5;
        private GroupResult gr6;

        private Subrequirement Subrequirement1;
        private Subrequirement Subrequirement21;
        private Subrequirement Subrequirement22;

        private SubrequirementResult sr1;
        private SubrequirementResult sr21;
        private SubrequirementResult sr22;

        private Requirement requirement1;
        private Requirement requirement2;

        private RequirementResult rr1;
        private RequirementResult rr2;

        private ProgramEvaluation pr1;
        private ProgramEvaluation pr2;

        private Group group1;
        private Group group2;
        private Group group3;
        private Group group4;
        private Group group5;
        private Group group6;
        //private Student s;
        //private StudentProgram sp;
        private CreditResult cr1, cr2, cr3, cr4, cr5, cr6, cr7, cr8, cr9, crA, crB;

        [TestInitialize]
        public void Initialize()
        {
            string program = "MATH.BS";
            string catalog = "2012";

            ProgramRequirements programrequirements1 = new ProgramRequirements(program, catalog);
            ProgramRequirements programrequirements2 = new ProgramRequirements(program, catalog);

            var reqtype = new RequirementType("MAJ", "Requirement", "1");
            string grdsch = "UG";
            requirement1 = new Requirement("Requirement 1", "REQ.1", "Requirement 1", grdsch, reqtype);
            requirement2 = new Requirement("Requirement 2", "REQ.2", "Requirement 2", grdsch, reqtype);

            Subrequirement1 = new Subrequirement("1", "1");
            Subrequirement1.Requirement = requirement1;
            Subrequirement21 = new Subrequirement("21", "21");
            Subrequirement21.Requirement = requirement1;
            Subrequirement22 = new Subrequirement("22", "22");
            Subrequirement22.Requirement = requirement2;

            requirement1.SubRequirements.Add(Subrequirement1);
            requirement1.SubRequirements.Add(Subrequirement21);
            requirement2.SubRequirements.Add(Subrequirement22);

            programrequirements1.Requirements.Add(requirement1);
            programrequirements1.Requirements.Add(requirement2);

            programrequirements2.Requirements.Add(requirement2);

            group1 = new Group("1", "1", Subrequirement1); // Parental references may need to be changed, 
            group2 = new Group("2", "2", Subrequirement1); // they are not currently in use.
            group3 = new Group("3", "3", Subrequirement1);
            group4 = new Group("4", "4", Subrequirement21);
            group5 = new Group("5", "5", Subrequirement22);
            group6 = new Group("6", "6", Subrequirement22);

            gr1 = new GroupResult(group1);
            gr2 = new GroupResult(group2);
            gr3 = new GroupResult(group3);
            gr4 = new GroupResult(group4);
            gr5 = new GroupResult(group5);
            gr6 = new GroupResult(group6);

            ac1.AdjustedGpaCredit = 3; ac1.AdjustedCredit = 3; ac1.CompletedCredit = 3;
            ac2.AdjustedGpaCredit = 3; ac2.AdjustedCredit = 3; ac2.CompletedCredit = 3;
            ac3.AdjustedGpaCredit = 3; ac3.AdjustedCredit = 3; ac3.CompletedCredit = 3;
            ac4.AdjustedGpaCredit = 3; ac4.AdjustedCredit = 3; ac4.CompletedCredit = 3;
            ac5.AdjustedGpaCredit = 3; ac5.AdjustedCredit = 3; ac5.CompletedCredit = 3;
            ac6.AdjustedGpaCredit = 3; ac6.AdjustedCredit = 3; ac6.CompletedCredit = 3;
            ac7.AdjustedGpaCredit = 3; ac7.AdjustedCredit = 3; ac7.CompletedCredit = 3;
            ac8.AdjustedGpaCredit = 3; ac8.AdjustedCredit = 3; ac8.CompletedCredit = 3;
            ac9.AdjustedGpaCredit = 3; ac9.AdjustedCredit = 3; ac9.CompletedCredit = 3;
            acA.AdjustedGpaCredit = 3; acA.AdjustedCredit = 3; acA.CompletedCredit = 3;
            acB.AdjustedGpaCredit = 0; acB.AdjustedCredit = 0; acB.CompletedCredit = 0;

            ac1.Type = CreditType.Institutional;
            ac2.Type = CreditType.Transfer;
            ac3.Type = CreditType.Institutional;
            ac4.Type = CreditType.Institutional;
            ac5.Type = CreditType.Institutional;
            ac6.Type = CreditType.Institutional;
            ac7.Type = CreditType.Transfer;
            ac8.Type = CreditType.Institutional;
            ac9.Type = CreditType.Institutional;
            acA.Type = CreditType.Institutional;
            acB.Type = CreditType.Institutional;

            // Must be a 1:1 relationship between academic credit and credit result because for
            // all GPA calculations, the same academic credit, if applied multiple times, is included
            // in the calculation only once.
            ac1.AdjustedGradePoints = 12;  // A  - 4.0
            ac2.AdjustedGradePoints = 9;  // B  - 3.0
            ac3.AdjustedGradePoints = 6;  // C  - 2.0
            ac4.AdjustedGradePoints = 3;  // D  - 1.0
            ac5.AdjustedGradePoints = 0;  // F  - 0.0
            ac6.AdjustedGradePoints = 12;  // A  - 4.0
            ac7.AdjustedGradePoints = 9;  // B  - 3.0
            ac8.AdjustedGradePoints = 3;  // D  - 1.0
            ac9.AdjustedGradePoints = 0;  // F  - 0.0
            acA.AdjustedGradePoints = 0;  // F  - 0.0
            acB.AdjustedGradePoints = 0; // Null - no credits have been taken
            
            cr1 = new CreditResult(ac1);
            cr2 = new CreditResult(ac2);

            cr3 = new CreditResult(ac3);
            cr4 = new CreditResult(ac4);
            cr5 = new CreditResult(ac5);

            cr6 = new CreditResult(ac6);
            cr7 = new CreditResult(ac7);
            cr8 = new CreditResult(ac8);

            cr9 = new CreditResult(ac9);  
            crA = new CreditResult(acA);
            crB = new CreditResult(acB);

            cr1.Result = cr2.Result = cr3.Result = Result.Applied;
            cr4.Result = cr5.Result = cr6.Result = Result.Applied;
            cr7.Result = cr8.Result = cr9.Result = Result.Applied;
            crA.Result = Result.Applied;
            crB.Result = Result.Applied;

            gr1.Results.Add(cr1); // group 1 - 27 grade points - 9 credits - 3.0000 Gpa
            gr1.Results.Add(cr2);
            gr1.Results.Add(cr3); // included in group gpa, but not duplicated in subreq, req gpa

            gr2.Results.Add(cr3); // group 2 -  9 grade points - 9 credits - 1.0000 Gpa
            gr2.Results.Add(cr4);
            gr2.Results.Add(cr5);

            gr3.Results.Add(cr6); // group 3 - 24 grade points - 9 credits - 2.6667 Gpa
            gr3.Results.Add(cr7);
            gr3.Results.Add(cr8);

            gr4.Results.Add(cr9); // group 4 -  6 grade points - 9 credits - 0.6667 Gpa
            gr4.Results.Add(crA);
            gr4.Results.Add(cr3); // included in group gpa, but not duplicated in subreq, req gpa

            gr5.Results.Add(cr9); // group 5 - 0 grade points - 3 credits - 0.0000 Gpa

            gr6.Results.Add(crB); // group 6 - null grade points - 0 credits - null Gpa

            sr1 = new SubrequirementResult(Subrequirement1);
            sr21 = new SubrequirementResult(Subrequirement21);
            sr22 = new SubrequirementResult(Subrequirement22);
            
            sr1.GroupResults.Add(gr1); // Subrequirement 1 - 54 grade points - 24 credits - 2.2500 Gpa (does not count cr3 twice)
            sr1.GroupResults.Add(gr2);
            sr1.GroupResults.Add(gr3);

            sr21.GroupResults.Add(gr4); // Subrequirement 21 -  6 grade points -  9 credits - 0.6667 Gpa

            sr22.GroupResults.Add(gr5); // Subrequirement 22 - 0 grade points - 3 credits - 0.0000 Gpa

            rr1 = new RequirementResult(requirement1);
            rr2 = new RequirementResult(requirement2);

            rr1.SubRequirementResults.Add(sr1); // requirement 1 - 54 grade points /30 credits = 1.8000 Gpa
            rr1.SubRequirementResults.Add(sr21);

            rr2.SubRequirementResults.Add(sr22); // requirement 2 -  0 grade points / 3 credits = 0.0000 Gpa

            var acadCredits = new List<AcademicCredit>() { ac1, ac2, ac3, ac4, ac5, ac6, ac7, ac8, ac9, acA };
            pr1 = new ProgramEvaluation(acadCredits, "", "");

            pr1.ProgramRequirements = programrequirements1;
            pr1.RequirementResults.Add(rr1); // studentprogram 1 - 54 grade points / 30 credits = 1.8 Gpa     instgpa: 36/24=1.5
            pr1.RequirementResults.Add(rr2);

            pr2 = new ProgramEvaluation(new List<AcademicCredit>() { ac9 }, "", "");
            
            pr2.ProgramRequirements = programrequirements2;
            pr2.RequirementResults.Add(rr2); // studentprogram 2 -  0 grade points /  3 credits = 0.0000 Gpa   instgpa: 0/3=0.0000

            // All academic credits are worth 3 credits. Numbers to the right of each cr indicate grade points for that credit.
            // *T == "Transfer" credit, excluded from institutional GPA
            // *R == Duplicate apply of this credit will be excluded from subreq/requirement gpa calculation. (Program gpa
            //       calculation is based on the list of AllCredits, which does not have duplicate credits.)
            //
            // Requirement 1 --   subreq 1 --          group1 -- cr1(12)   cr2(9)*T  cr3(6)    -- gpa:27/9=3.0000   igpa:18/6=3.0000
            //  gpa:54/30=1.8000  gpa:54/24=2.2500     group2 -- cr3(6)*R  cr4(3)    cr5(0)    -- gpa: 9/9=1.0000   igpa: 9/9=1.0000
            // igpa:36/24=1.5000 igpa:36/18=2.0000     group3 -- cr6(12)   cr7(9)*T  cr8(3)    -- gpa:24/9=2.6667   igpa:15/6=2.5000
            //
            //                    subreq 21--          group4 -- cr9(0)    crA(0)    cr3(6)*R  -- gpa: 6/9=0.6667   igpa: 6/9=0.6667
            //                    gpa:6/9=0.6667
            //                   igpa:6/9=0.6667
            //
            // Requirement 2 --   subreq 22 --         group5 -- cr9(0)                        -- gpa: 0/3=0.0000   igpa: 0/3=0.0000
            //  gpa:0/3=0.0000    gpa:0/3=0.0000
            // igpa:0/3=0.0000   igpa:0/3=0.0000

        }
        [TestClass]
        public class GpaCalculations : BlockResultTests
        {
            [TestMethod]
            public void GroupGpaCalculatesUsingUniqueAppliedCredits()
            {
                Assert.IsNotNull(gr1);
                Assert.AreEqual(decimal.Parse("3.0"), gr1.Gpa);
                Assert.IsNotNull(gr2);
                Assert.AreEqual(decimal.Parse("1.0"), gr2.Gpa);
                Assert.IsNotNull(gr3);
                Assert.AreEqual(decimal.Parse("2.6667"), decimal.Round(gr3.Gpa ?? 0, 4));
                Assert.IsNotNull(gr4);
                Assert.AreEqual(decimal.Parse("0.6667"), decimal.Round(gr4.Gpa ?? 0, 4));
            }

            [TestMethod]
            public void GroupGpaCalculatesCorrectlyForZeroGpa()
            {
                Assert.IsNotNull(gr5.Gpa);
                Assert.AreEqual(decimal.Parse("0"), decimal.Round(gr5.Gpa ?? 0, 4));
            }

            [TestMethod]
            public void GroupGpaCalculatesCorrectlyForNullGpa()
            {
                Assert.IsNull(gr6.Gpa);
                Assert.IsNull(gr6.InstGpa);
            }

            [TestMethod]
            public void SubrequirementGpaCalculatesUsingUniqueAppliedCredits()
            {
                Assert.IsNotNull(sr1);
                Assert.AreEqual(decimal.Parse("2.25"), sr1.Gpa);
            }
            [TestMethod]
            public void SubrequirementGpaCalculatesCorrectlyForZeroGpa()
            {
                Assert.IsNotNull(sr22);
                Assert.AreEqual(decimal.Parse("0.0000"), sr22.Gpa);
            }

            [TestMethod]
            public void RequirementGpaCalculatesUsingUniqueAppliedCredits()
            {
                Assert.IsNotNull(rr1);
                Assert.AreEqual(decimal.Parse("1.8"), rr1.Gpa);
            }

            [TestMethod]
            public void RequirementGpaCalculatesCorrectlyForZeroGpa()
            {
                Assert.IsNotNull(rr2);
                Assert.AreEqual(decimal.Parse("0.0000"), rr2.Gpa);
            }

            [TestMethod]
            public void CumGpaCalculatesUsingAllCredits()
            {
                Assert.AreEqual(decimal.Parse("1.8"), pr1.CumGpa);
            }

            [TestMethod]
            public void CumGpaCalculatesCorrectlyForZeroCumGpa()
            {
                Assert.AreEqual(decimal.Parse("0"), pr2.CumGpa);
            }
        }

        [TestClass]
        public class InstitutionalCredits : BlockResultTests
        {
            [TestMethod]
            public void GroupReturnsInstCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(6, gr1.GetAppliedInstCredits());
            }
            [TestMethod]
            public void SubrequirementReturnsInstCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(21, sr1.GetInstCredits());
            }
            [TestMethod]
            public void RequirementReturnsInstCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(30, rr1.GetInstCredits());
            }
            [TestMethod]
            public void ProgramReturnsInstCreditFromAllCredits()
            {
                Assert.AreEqual(24, pr1.GetInstCredits());
            }



        }

        [TestClass]
        public class InstitutionalGpaCalculations : BlockResultTests
        {
            [TestMethod]
            public void GroupCalculatesInstGpaCorrectly()
            {
                Assert.AreEqual(3m, gr1.InstGpa);
            }
            [TestMethod]
            public void SubrequirementCalculatesInstGpaCorrectly()
            {
                Assert.AreEqual(2.0m, sr1.InstGpa);
            }
            [TestMethod]
            public void RequirementCalculatesInstGpaCorrectly()
            {
                Assert.AreEqual(1.5m, rr1.InstGpa);
            }
            [TestMethod]
            public void ProgramCalculatesInstGpaCorrectly()
            {
                Assert.AreEqual(1.5m, pr1.InstGpa);
            }



        }

        [TestClass]
        public class GradePointsCalculations : BlockResultTests
        {
            [TestMethod]
            public void GroupSumsGradePointsCorrectly()
            {
                Assert.AreEqual(27, gr1.GetGradePoints(gr1.GetCreditsToIncludeInGpa()));
            }
            [TestMethod]
            public void SubrequirementSumsGradePointsCorrectly()
            {
                Assert.AreEqual(54, sr1.GetGradePoints(sr1.GetCreditsToIncludeInGpa()));
            }
            [TestMethod]
            public void RequirementSumsGradePointsCorrectly()
            {
                Assert.AreEqual(54, rr1.GetGradePoints(rr1.GetCreditsToIncludeInGpa()));
            }
            



        }
        [TestClass]
        public class CompletedCredits : BlockResultTests
        {
            [TestMethod]
            public void GroupSumsCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(9, gr1.GetCompletedCredits());
            }
            [TestMethod]
            public void SubrequirementSumsCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(27, sr1.GetCompletedCredits());
            }
            [TestMethod]
            public void RequirementSumsCreditFromAllAppliedCredits()
            {
                Assert.AreEqual(36, rr1.GetCompletedCredits());
            }
            [TestMethod]
            public void ProgramSumsAdjustedCreditFromAllCredits()
            {
                Assert.AreEqual(30, pr1.GetCredits());
            }
        }
    }
}
