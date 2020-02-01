// Copyright 2017-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.DegreePlans;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.DegreePlans
{
    [TestClass]
    public class ProgramEvaluationTests
    {
        private List<AcademicCredit> academicCredits;
        private string programCode;
        private string catalogCode;
        private Course course;
        private RequirementType requirementType;
        private Group group;
        private Subrequirement subrequirement;
        private Requirement requirement;
        private GroupResult groupResult;
        private SubrequirementResult subrequirementResult;
        private RequirementResult requirementResult;

        private ProgramEvaluation programEvaluation;

        [TestInitialize]
        public async void Initialize()
        {
            academicCredits = (await new TestAcademicCreditRepository().GetAsync()).ToList();
            programCode = "BUSN.BA";
            catalogCode = DateTime.Today.Year.ToString();
            course = new TestCourseRepository().Biol100;
            requirementType = new RequirementType("MAJ", "Major Requirement", "1");
            programEvaluation = new ProgramEvaluation(academicCredits, programCode, catalogCode);
        }

        [TestClass]
        public class ProgramEvaluation_Properties_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_Properties_InstitutionalCredits()
            {
                programEvaluation.InstitutionalCredits = 12m;
                Assert.AreEqual(12m, programEvaluation.InstitutionalCredits);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_Credits()
            {
                programEvaluation.Credits = 12m;
                Assert.AreEqual(12m, programEvaluation.Credits);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_InProgressCredits()
            {
                programEvaluation.InProgressCredits = 12m;
                Assert.AreEqual(12m, programEvaluation.InProgressCredits);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_PlannedCredits()
            {
                programEvaluation.PlannedCredits = 12m;
                Assert.AreEqual(12m, programEvaluation.PlannedCredits);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_ProgramRequirements()
            {
                programEvaluation.ProgramRequirements = new ProgramRequirements(programCode, catalogCode);
                Assert.AreEqual(programCode, programEvaluation.ProgramRequirements.ProgramCode);
                Assert.AreEqual(catalogCode, programEvaluation.ProgramRequirements.CatalogCode);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_OverallCreditsModificationMessage()
            {
                string message = "Message";
                programEvaluation.OverallCreditsModificationMessage = message;
                Assert.AreEqual(message, programEvaluation.OverallCreditsModificationMessage);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_InstitutionalCreditsModificationMessage()
            {
                string message = "Message";
                programEvaluation.InstitutionalCreditsModificationMessage = message;
                Assert.AreEqual(message, programEvaluation.InstitutionalCreditsModificationMessage);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_OverallGpaModificationMessage()
            {
                string message = "Message";
                programEvaluation.OverallGpaModificationMessage = message;
                Assert.AreEqual(message, programEvaluation.OverallGpaModificationMessage);
            }

            [TestMethod]
            public void ProgramEvaluation_Properties_InstitutionalGpaModificationMessage()
            {
                string message = "Message";
                programEvaluation.InstitutionalGpaModificationMessage = message;
                Assert.AreEqual(message, programEvaluation.InstitutionalGpaModificationMessage);
            }
        }

        [TestClass]
        public class ProgramEvaluation_Constructor_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProgramEvaluation_Constructor_Null_AcademicCredits()
            {
                var programEvaluation = new ProgramEvaluation(null);
            }

            [TestMethod]
            public void ProgramEvaluation_Constructor_Valid_Defaults()
            {
                var programEvaluation = new ProgramEvaluation(academicCredits);

                Assert.IsNotNull(programEvaluation.RequirementResults);
                Assert.IsFalse(programEvaluation.RequirementResults.Any());
                Assert.IsNotNull(programEvaluation.Explanations);
                Assert.IsFalse(programEvaluation.Explanations.Any());
                CollectionAssert.AreEqual(academicCredits, programEvaluation.AllCredit);
                Assert.IsNull(programEvaluation.ProgramCode);
                Assert.IsNull(programEvaluation.CatalogCode);
                Assert.IsFalse(programEvaluation.IsSatisfied);
                Assert.IsFalse(programEvaluation.IsPlannedSatisfied);
            }

            [TestMethod]
            public void ProgramEvaluation_Constructor_Valid_No_Defaults()
            {
                Assert.AreEqual(programCode, programEvaluation.ProgramCode);
                Assert.AreEqual(catalogCode, programEvaluation.CatalogCode);
            }
        }

        [TestClass]
        public class ProgramEvaluation_OtherPlannedCredits_Tests : ProgramEvaluationTests
        {
            [TestInitialize]
            public void ProgramEvaluation_OtherPlannedCredits_Initialize()
            {
                base.Initialize();
                subrequirement = new Subrequirement("234", "SUBREQ");
                group = new Group("012", "GROUP", subrequirement);
                requirement = new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType)
                {
                    SubRequirements = new List<Subrequirement>() { subrequirement }
                };
                groupResult = new GroupResult(group);
                groupResult.Results = new List<AcadResult>()
                {
                    new CreditResult(academicCredits[0]) { Result = Result.PlannedApplied },
                    new CreditResult(academicCredits[1]) { Result = Result.PlannedApplied },
                    new CreditResult(academicCredits[2]) { Result = Result.PlannedApplied },
                };
                subrequirementResult = new SubrequirementResult(subrequirement)
                {
                    GroupResults = new List<GroupResult>()
                    {
                        groupResult
                    }
                };
                requirementResult = new RequirementResult(requirement)
                {
                    SubRequirementResults = new List<SubrequirementResult>()
                    {
                        subrequirementResult
                    }
                };

                programEvaluation = new ProgramEvaluation(academicCredits, programCode, catalogCode)
                {
                    AllPlannedCredits = new List<PlannedCredit>()
                    {
                        new PlannedCredit(course, "2017/FA")
                    },
                    RequirementResults = new List<RequirementResult>()
                    {
                        requirementResult
                    }
                };
            }

            [TestMethod]
            public void ProgramEvaluation_OtherPlannedCredits_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.IsNotNull(programEvaluation.OtherPlannedCredits);
                Assert.IsFalse(programEvaluation.OtherPlannedCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherPlannedCredits_Null_RequirementResults()
            {
                var programEvaluation = new ProgramEvaluation(academicCredits);
                programEvaluation.RequirementResults = null;

                Assert.IsNotNull(programEvaluation.OtherPlannedCredits);
                Assert.IsFalse(programEvaluation.OtherPlannedCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherPlannedCredits_Empty_RequirementResults()
            {
                var programEvaluation = new ProgramEvaluation(academicCredits);
                programEvaluation.RequirementResults = new List<RequirementResult>();

                Assert.IsNotNull(programEvaluation.OtherPlannedCredits);
                Assert.IsFalse(programEvaluation.OtherPlannedCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherPlannedCredits_With_RequirementResults_No_PlannedApplied()
            {
                Assert.IsNotNull(programEvaluation.OtherPlannedCredits);
                Assert.AreEqual(1, programEvaluation.OtherPlannedCredits.Count);
            }

            [TestMethod]
            public void ProgramEvaluation_OtherPlannedCredits_With_RequirementResults_PlannedApplied()
            {
                // Evaluation group results contains the planned credit for the evaluation
                // All planned credits are "planned applied" giving us no OtherPlannedCredits
                programEvaluation.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results = new List<AcadResult>()
                {
                    new CreditResult(academicCredits[0]) { Result = Result.PlannedApplied },
                };
                programEvaluation.AllPlannedCredits = new List<PlannedCredit>()
                {
                    new PlannedCredit(academicCredits[0].Course, academicCredits[0].TermCode) { SectionId = academicCredits[0].SectionId }
                };

                Assert.IsNotNull(programEvaluation.OtherPlannedCredits);
                Assert.IsFalse(programEvaluation.OtherPlannedCredits.Any());
            }
        }

        [TestClass]
        public class ProgramEvaluation_OtherAcademicCredits_Tests : ProgramEvaluationTests
        {
            [TestInitialize]
            public void ProgramEvaluation_OtherAcademicCredits_Initialize()
            {
                base.Initialize();
                subrequirement = new Subrequirement("234", "SUBREQ");
                group = new Group("012", "GROUP", subrequirement);
                requirement = new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType)
                {
                    SubRequirements = new List<Subrequirement>() { subrequirement }
                };
                groupResult = new GroupResult(group);
                groupResult.Results = new List<AcadResult>()
                {
                    new CreditResult(academicCredits[0]) { Result = Result.Applied },
                    new CreditResult(academicCredits[1]) { Result = Result.Applied },
                    new CreditResult(academicCredits[2]) { Result = Result.Applied },
                };
                subrequirementResult = new SubrequirementResult(subrequirement)
                {
                    GroupResults = new List<GroupResult>()
                    {
                        groupResult
                    }
                };
                requirementResult = new RequirementResult(requirement)
                {
                    SubRequirementResults = new List<SubrequirementResult>()
                    {
                        subrequirementResult
                    }
                };
                programEvaluation = new ProgramEvaluation(academicCredits, programCode, catalogCode)
                {
                    RequirementResults = new List<RequirementResult>()
                    {
                        requirementResult
                    }
                };
            }

            [TestMethod]
            public void ProgramEvaluation_OtherAcademicCredits_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.IsNotNull(programEvaluation.OtherAcademicCredits);
                Assert.IsFalse(programEvaluation.OtherAcademicCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherAcademicCredits_Null_RequirementResults()
            {
                var programEvaluation = new ProgramEvaluation(academicCredits);
                programEvaluation.RequirementResults = null;

                Assert.IsNotNull(programEvaluation.OtherAcademicCredits);
                Assert.IsFalse(programEvaluation.OtherAcademicCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherAcademicCredits_Empty_RequirementResults()
            {
                var programEvaluation = new ProgramEvaluation(academicCredits);
                programEvaluation.RequirementResults = new List<RequirementResult>();

                Assert.IsNotNull(programEvaluation.OtherAcademicCredits);
                Assert.IsFalse(programEvaluation.OtherAcademicCredits.Any());
            }

            [TestMethod]
            public void ProgramEvaluation_OtherAcademicCredits_With_RequirementResults_No_Applied()
            {
                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { academicCredits[0] })
                {
                    RequirementResults = new List<RequirementResult>()
                    {
                        requirementResult
                    }
                };
                programEvaluation.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results = new List<AcadResult>()
                {
                    // Evaluation was built with a different academic credit than the one in the group results
                    // This gives a "not applied" academic credit that will fall into OtherAcademicCredits
                    new CreditResult(academicCredits[1]) { Result = Result.Applied },
                };
                Assert.IsNotNull(programEvaluation.OtherAcademicCredits);
                Assert.AreEqual(1, programEvaluation.OtherAcademicCredits.Count);
            }

            [TestMethod]
            public void ProgramEvaluation_OtherAcademicCredits_With_RequirementResults_Applied()
            {
                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { academicCredits[0] })
                {
                    RequirementResults = new List<RequirementResult>()
                    {
                        requirementResult
                    }
                };
                programEvaluation.RequirementResults[0].SubRequirementResults[0].GroupResults[0].Results = new List<AcadResult>()
                {
                    new CreditResult(academicCredits[0]) { Result = Result.Applied },
                };

                Assert.IsNotNull(programEvaluation.OtherAcademicCredits);
                Assert.IsFalse(programEvaluation.OtherAcademicCredits.Any());
            }
        }

        [TestClass]
        public class ProgramEvaluation_GetInProgressCredits_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_GetInProgressCredits_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.AreEqual(0m, programEvaluation.GetInProgressCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetInProgressCredits_Only_VerifiedGrade_Credits()
            {
                // Academic credits with a verified grade are not considered in-progress
                academicCredits[0].VerifiedGrade = new Grade("A","A","UG");
                academicCredits[0].Type = CreditType.Institutional;
                var programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { academicCredits[0] });
                Assert.AreEqual(0m, programEvaluation.GetInProgressCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetInProgressCredits_Only_Non_Institutional_Credits()
            {
                // Academic credits with type != Institutional are not considered in-progress
                academicCredits[0].VerifiedGrade = null;
                academicCredits[0].Type = CreditType.Other;
                var programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { academicCredits[0] });
                Assert.AreEqual(0m, programEvaluation.GetInProgressCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetInProgressCredits_With_InProgress_Credits()
            {
                // Academic credits with no verified grade and type = Institutional are considered in-progress
                academicCredits[0].VerifiedGrade = null;
                academicCredits[0].Type = CreditType.Institutional;
                var programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { academicCredits[0] });
                Assert.AreEqual(academicCredits[0].Credit, programEvaluation.GetInProgressCredits());
            }
        }

        [TestClass]
        public class ProgramEvaluation_GetCredits_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_GetCredits_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.AreEqual(0m, programEvaluation.GetCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetCredits_With_AllCredit()
            {
                academicCredits[0].AdjustedCredit = null;
                var expected = academicCredits.Sum(ac => ac.AdjustedCredit ?? 0m);
                programEvaluation = new ProgramEvaluation(academicCredits, programCode, catalogCode);

                Assert.AreEqual(expected, programEvaluation.GetCredits());
            }
        }

        [TestClass]
        public class ProgramEvaluation_GetInstCredits_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_GetInstCredits_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.AreEqual(0m, programEvaluation.GetInstCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetInstCredits_With_Institutional_Credits()
            {
                academicCredits[0].AdjustedCredit = null;
                var expected = academicCredits.Where(ac => ac.IsInstitutional()).Sum(ac => ac.AdjustedCredit ?? 0m);
                programEvaluation = new ProgramEvaluation(academicCredits, programCode, catalogCode);

                Assert.AreEqual(expected, programEvaluation.GetInstCredits());
            }

            [TestMethod]
            public void ProgramEvaluation_GetInstCredits_Without_Institutional_Credits()
            {
                var nonInstAcademicCredits = academicCredits.Where(ac => !ac.IsInstitutional()).ToList();
                programEvaluation = new ProgramEvaluation(nonInstAcademicCredits, programCode, catalogCode);

                Assert.AreEqual(0m, programEvaluation.GetInstCredits());
            }
        }

        [TestClass]
        public class ProgramEvaluation_CumGpa_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_CumGpa_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.IsNull(programEvaluation.CumGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_CumGpa_No_Credits()
            {
                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>(), programCode, catalogCode);
                Assert.IsNull(programEvaluation.CumGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_CumGpa_AdjustedGpaCredit_Sum_is_Zero()
            {
                var zeroAdjustedGpaCredit = academicCredits[0];
                zeroAdjustedGpaCredit.AdjustedGpaCredit = 0m;

                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { zeroAdjustedGpaCredit }, programCode, catalogCode);
                Assert.IsNull(programEvaluation.CumGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_CumGpa_AdjustedGpaCredit_Sum_is_less_than_Zero()
            {
                var zeroAdjustedGpaCredit = academicCredits[0];
                zeroAdjustedGpaCredit.AdjustedGpaCredit = -10m;

                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { zeroAdjustedGpaCredit }, programCode, catalogCode);
                Assert.AreEqual(0m, programEvaluation.CumGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_CumGpa_AdjustedGpaCredit_Sum_is_greater_than_Zero()
            {
                var expectedAdjustedGradePoints = academicCredits.Sum(ac => ac.AdjustedGradePoints);
                var expectedAdjustedGpaCredit = academicCredits.Sum(ac => ac.AdjustedGpaCredit);
                var expectedCumGpa = expectedAdjustedGradePoints / expectedAdjustedGpaCredit;

                Assert.AreEqual(expectedCumGpa, programEvaluation.CumGpa);
            }
        }

        [TestClass]
        public class ProgramEvaluation_InstGpa_Tests : ProgramEvaluationTests
        {
            [TestMethod]
            public void ProgramEvaluation_InstGpa_With_Null_AllCredit()
            {
                programEvaluation.AllCredit = null;
                Assert.IsNull(programEvaluation.InstGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_InstGpa_No_Credits()
            {
                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>(), programCode, catalogCode);
                Assert.IsNull(programEvaluation.InstGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_InstGpa_AdjustedGpaCredit_Sum_is_Zero()
            {
                var zeroAdjustedGpaCredit = academicCredits[0];
                // InstGpa only looks at institutional credits; set the type manually to ensure the academic credit is included
                zeroAdjustedGpaCredit.Type = CreditType.Institutional;
                zeroAdjustedGpaCredit.AdjustedGpaCredit = 0m;

                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { zeroAdjustedGpaCredit }, programCode, catalogCode);
                Assert.IsNull(programEvaluation.InstGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_InstGpa_AdjustedGpaCredit_Sum_is_less_than_Zero()
            {
                var zeroAdjustedGpaCredit = academicCredits[0];
                // InstGpa only looks at institutional credits; set the type manually to ensure the academic credit is included
                zeroAdjustedGpaCredit.Type = CreditType.Institutional;
                zeroAdjustedGpaCredit.AdjustedGpaCredit = -10m;

                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { zeroAdjustedGpaCredit }, programCode, catalogCode);
                Assert.AreEqual(0m, programEvaluation.InstGpa);
            }

            [TestMethod]
            public void ProgramEvaluation_InstGpa_AdjustedGpaCredit_Sum_is_greater_than_Zero()
            {
                var zeroAdjustedGpaCredit = academicCredits[0];
                // InstGpa only looks at institutional credits; set the type manually to ensure the academic credit is included
                zeroAdjustedGpaCredit.Type = CreditType.Institutional;
                zeroAdjustedGpaCredit.AdjustedGradePoints = 12m;
                zeroAdjustedGpaCredit.AdjustedGpaCredit = 3m;
                var expectedInstGpa = zeroAdjustedGpaCredit.AdjustedGradePoints / zeroAdjustedGpaCredit.AdjustedGpaCredit;

                programEvaluation = new ProgramEvaluation(new List<AcademicCredit>() { zeroAdjustedGpaCredit }, programCode, catalogCode);
                Assert.AreEqual(expectedInstGpa, programEvaluation.InstGpa);
            }
        }

        [TestClass]
        public class ProgramEvaluation_ToString_Tests : ProgramEvaluationTests
        {
            [TestInitialize]
            public void ProgramEvaluation_ToString_Initialize()
            {
                base.Initialize();
                programEvaluation.Explanations = new HashSet<ProgramRequirementsExplanation>()
                {
                    ProgramRequirementsExplanation.Satisfied
                };
            }

            [TestMethod]
            public void ProgramEvaluation_ToString_No_RequirementResults()
            {
                programEvaluation.RequirementResults = new List<RequirementResult>();
                // Build expected value
                var expected = new StringBuilder();
                expected.AppendLine("Program Result: " + string.Join(",", programEvaluation.Explanations.Select(ex => ex.ToString())));

                Assert.AreEqual(expected.ToString(), programEvaluation.ToString());
            }

            [TestMethod]
            public void ProgramEvaluation_ToString_with_RequirementResults_No_SubRequirementResults()
            {
                // Set up requirement results
                requirementResult = new RequirementResult(new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType));             
                requirementResult.SubRequirementResults = new List<SubrequirementResult>();
                programEvaluation.RequirementResults = new List<RequirementResult>() { requirementResult };
                // Build expected value
                var expected = new StringBuilder();
                expected.AppendLine("Program Result: " + string.Join(",", programEvaluation.Explanations.Select(ex => ex.ToString())));
                expected.AppendLine("\tRequirement: " + requirementResult.Requirement.Code + "\t " + " Status: " + requirementResult.CompletionStatus.ToString()
                                                                                  + ",  " + requirementResult.PlanningStatus.ToString());

                Assert.AreEqual(expected.ToString(), programEvaluation.ToString());
            }

            [TestMethod]
            public void ProgramEvaluation_ToString_with_RequirementResults_with_SubRequirementResults_no_GroupResults()
            {
                // Set up subrequirement results
                subrequirementResult = new SubrequirementResult(new Subrequirement("234", "SUBREQ"));
                subrequirementResult.GroupResults = new List<GroupResult>();
                // Set up requirement results
                requirementResult = new RequirementResult(new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType));
                requirementResult.SubRequirementResults = new List<SubrequirementResult>() { subrequirementResult };
                programEvaluation.RequirementResults = new List<RequirementResult>() { requirementResult };
                // Build expected value
                var expected = new StringBuilder();
                expected.AppendLine("Program Result: " + string.Join(",", programEvaluation.Explanations.Select(ex => ex.ToString())));
                expected.AppendLine("\tRequirement: " + requirementResult.Requirement.Code + "\t " + " Status: " + requirementResult.CompletionStatus.ToString()
                                                                                  + ",  " + requirementResult.PlanningStatus.ToString());
                expected.AppendLine("\t\tSubrequirement: " + subrequirementResult.SubRequirement.Code + "\t " + " Status: " + subrequirementResult.CompletionStatus.ToString()
                                                                                              + ", " + subrequirementResult.PlanningStatus.ToString());
                Assert.AreEqual(expected.ToString(), programEvaluation.ToString());
            }

            [TestMethod]
            public void ProgramEvaluation_ToString_with_RequirementResults_with_SubRequirementResults_with_GroupResults_no_EvalDebug()
            {
                subrequirement = new Subrequirement("234", "SUBREQ");
                // Set up group results
                groupResult = new GroupResult(new Group("012", "GROUP", subrequirement));
                groupResult.EvalDebug = new List<string>();
                groupResult.Explanations = new HashSet<GroupExplanation>()
                {
                    GroupExplanation.Satisfied
                };
                // Set up subrequirement results
                subrequirementResult = new SubrequirementResult(subrequirement);
                subrequirementResult.GroupResults = new List<GroupResult>() { groupResult };
                // Set up requirement results
                requirementResult = new RequirementResult(new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType));
                requirementResult.SubRequirementResults = new List<SubrequirementResult>() { subrequirementResult };
                programEvaluation.RequirementResults = new List<RequirementResult>() { requirementResult };
                // Build expected value
                var expected = new StringBuilder();
                expected.AppendLine("Program Result: " + string.Join(",", programEvaluation.Explanations.Select(ex => ex.ToString())));
                expected.AppendLine("\tRequirement: " + requirementResult.Requirement.Code + "\t " + " Status: " + requirementResult.CompletionStatus.ToString()
                                                                                  + ",  " + requirementResult.PlanningStatus.ToString());
                expected.AppendLine("\t\tSubrequirement: " + subrequirementResult.SubRequirement.Code + "\t " + " Status: " + subrequirementResult.CompletionStatus.ToString()
                                                                                              + ", " + subrequirementResult.PlanningStatus.ToString());
                expected.AppendLine("\t\t\tGroup: " + groupResult.Group.Id + " " + groupResult.Group.Code + "\t " + string.Join(",", groupResult.Explanations.Select(ex => ex.ToString())));

                Assert.AreEqual(expected.ToString(), programEvaluation.ToString());
            }

            [TestMethod]
            public void ProgramEvaluation_ToString_with_RequirementResults_with_SubRequirementResults_with_GroupResults_with_EvalDebug()
            {
                subrequirement = new Subrequirement("234", "SUBREQ");
                // Set up group results
                groupResult = new GroupResult(new Group("012", "GROUP", subrequirement));
                // Set up eval debug
                var evalDebugString = "Group satisfied due to Exception";
                groupResult.EvalDebug = new List<string>()
                {
                    evalDebugString
                };
                groupResult.Explanations = new HashSet<GroupExplanation>()
                {
                    GroupExplanation.Satisfied
                };
                // Set up subrequirement results
                subrequirementResult = new SubrequirementResult(subrequirement);
                subrequirementResult.GroupResults = new List<GroupResult>() { groupResult };
                // Set up requirement results
                requirementResult = new RequirementResult(new Requirement("123", "REQCODE", "Requirement Description", "GSC", requirementType));
                requirementResult.SubRequirementResults = new List<SubrequirementResult>() { subrequirementResult };
                programEvaluation.RequirementResults = new List<RequirementResult>() { requirementResult };
                // Build expected value
                var expected = new StringBuilder();
                expected.AppendLine("Program Result: " + string.Join(",", programEvaluation.Explanations.Select(ex => ex.ToString())));
                expected.AppendLine("\tRequirement: " + requirementResult.Requirement.Code + "\t " + " Status: " + requirementResult.CompletionStatus.ToString()
                                                                                  + ",  " + requirementResult.PlanningStatus.ToString());
                expected.AppendLine("\t\tSubrequirement: " + subrequirementResult.SubRequirement.Code + "\t " + " Status: " + subrequirementResult.CompletionStatus.ToString()
                                                                                              + ", " + subrequirementResult.PlanningStatus.ToString());
                expected.AppendLine("\t\t\tGroup: " + groupResult.Group.Id + " " + groupResult.Group.Code + "\t " + string.Join(",", groupResult.Explanations.Select(ex => ex.ToString())));
                expected.AppendLine("\t\t\t\t\t" + evalDebugString);
                Assert.AreEqual(expected.ToString(), programEvaluation.ToString());
            }
        }
    }
}