// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Requirements
{
    [TestClass]
    public class GroupResultTests
    {
        private string id;
        private string code;
        private AcademicCredit acadCredit;
        private Course course;
        private Subrequirement subrequirementEntity;
        private Group groupEntity;
        private GroupResult entity;
    
        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            code = "GROUP123";
            course = new TestCourseRepository().Biol100;
            acadCredit = new AcademicCredit("3", course, "123");
            acadCredit.TermCode = "testTerm";
            acadCredit.SubjectCode = "testSubject";
            acadCredit.AddDepartment("DEPT1");
            acadCredit.AddDepartment("DEPT2");
            acadCredit.CourseLevelCode = "100";
            acadCredit.VerifiedGrade = new Grade("B", "Very Good", "UG");
            acadCredit.Type = CreditType.Institutional;
            acadCredit.CompletedCredit = 3m;
            acadCredit.AdjustedGradePoints = 6m;
            acadCredit.AdjustedGpaCredit = 5m;
            acadCredit.AdjustedCredit = 4m;
            acadCredit.CourseName = "CRS_100";
            subrequirementEntity = new Subrequirement(id, code);
            groupEntity = new Group(id, code, subrequirementEntity);
        }

        [TestClass]
        public class GroupResult_Constructor_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_Constructor_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_Constructor()
            {
                Assert.AreEqual(groupEntity, entity.Group);
                CollectionAssert.AreEqual(new List<AcadResult>(), entity.Results);
                Assert.AreEqual(typeof(HashSet<GroupExplanation>), entity.Explanations.GetType());
                Assert.AreEqual(0, entity.Explanations.Count);
                CollectionAssert.AreEqual(new List<string>(), entity.EvalDebug);
                CollectionAssert.AreEqual(new List<string>(), entity.ForceAppliedAcademicCreditIds);
                CollectionAssert.AreEqual(new List<string>(), entity.ForceDeniedAcademicCreditIds);
                Assert.AreEqual(entity.MinGroupStatus, GroupResultMinGroupStatus.None);
            }
        }

        [TestClass]
        public class GroupResult_IsSatisfied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_IsSatisfied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_IsSatisfied_True()
            {
                entity.Explanations.Add(GroupExplanation.Satisfied);
                Assert.IsTrue(entity.IsSatisfied());
            }

            [TestMethod]
            public void GroupResult_IsSatisfied_False_Not_Satisfied()
            {
                entity.Explanations.Add(GroupExplanation.Courses);
                Assert.IsFalse(entity.IsSatisfied());
            }

            [TestMethod]
            public void GroupResult_IsSatisfied_False_Multiple_Explanations()
            {
                entity.Explanations.Add(GroupExplanation.MinGpa);
                entity.Explanations.Add(GroupExplanation.Courses);
                Assert.IsFalse(entity.IsSatisfied());
            }
        }

        [TestClass]
        public class GroupResult_IsPlannedSatisfied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_IsPlannedSatisfied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_IsPlannedSatisfied_True()
            {
                entity.Explanations.Add(GroupExplanation.PlannedSatisfied);
                Assert.IsTrue(entity.IsPlannedSatisfied());
            }

            [TestMethod]
            public void GroupResult_IsPlannedSatisfied_False()
            {
                entity.Explanations.Add(GroupExplanation.Courses);
                Assert.IsFalse(entity.IsPlannedSatisfied());
            }
        }

        [TestClass]
        public class GroupResult_CountApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_CountApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_CountApplied()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied });
                Assert.AreEqual(entity.Results.Count, entity.CountApplied());
            }
        }

        [TestClass]
        public class GroupResult_CountPlannedApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_CountPlannedApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_CountPlannedApplied()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied });
                Assert.AreEqual(entity.Results.Count, entity.CountPlannedApplied());
            }
        }

        [TestClass]
        public class GroupResult_CountNonExtraApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_CountNonExtraApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_CountNonExtraApplied_At_Least_One()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                Assert.AreEqual(entity.Results.Count, entity.CountNonExtraApplied());
            }

            [TestMethod]
            public void GroupResult_CountNonExtraApplied_None()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.Extra });
                Assert.AreEqual(0, entity.CountNonExtraApplied());
            }
        }

        [TestClass]
        public class GroupResult_CountNonExtraPlannedApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_CountNonExtraPlannedApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_CountNonExtraPlannedApplied_At_Least_One()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });
                Assert.AreEqual(entity.Results.Count, entity.CountNonExtraPlannedApplied());
            }

            [TestMethod]
            public void GroupResult_CountNonExtraPlannedApplied_None()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.Extra });
                Assert.AreEqual(0, entity.CountNonExtraPlannedApplied());
            }
        }

        [TestClass]
        public class GroupResult_GetApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetApplied_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetApplied_Results_No_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetApplied_Results_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                CollectionAssert.AreEqual(entity.Results, entity.GetApplied().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetPlannedApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetPlannedApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetPlannedApplied_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedApplied_Results_No_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedApplied_Results_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                CollectionAssert.AreEqual(entity.Results, entity.GetPlannedApplied().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetNonExtraApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetNonExtraApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetNonExtraApplied_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetNonExtraApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraApplied_Results_No_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetNonExtraApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraApplied_Results_No_NonExtra_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.Extra } };
                Assert.IsFalse(entity.GetNonExtraApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraApplied_Results_NonExtra_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.None } };
                CollectionAssert.AreEqual(entity.Results, entity.GetNonExtraApplied().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetNonExtraPlannedApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetNonExtraPlannedApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedApplied_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetNonExtraPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedApplied_Results_No_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetNonExtraPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedApplied_Results_No_NonExtra_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.Extra } };
                Assert.IsFalse(entity.GetNonExtraPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedApplied_Results_NonExtra_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None } };
                CollectionAssert.AreEqual(entity.Results, entity.GetNonExtraPlannedApplied().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetAppliedAndPlannedApplied_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetAppliedAndPlannedApplied_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetAppliedAndPlannedApplied_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetAppliedAndPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedAndPlannedApplied_Results_No_Applied_or_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetAppliedAndPlannedApplied().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedAndPlannedApplied_Results_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                CollectionAssert.AreEqual(entity.Results, entity.GetAppliedAndPlannedApplied().ToList());
            }

            [TestMethod]
            public void GroupResult_GetAppliedAndPlannedApplied_Results_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                CollectionAssert.AreEqual(entity.Results, entity.GetAppliedAndPlannedApplied().ToList());
            }

            [TestMethod]
            public void GroupResult_GetAppliedAndPlannedApplied_Results_Applied_and_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>()
                {
                    new CreditResult(acadCredit) { Result = Result.PlannedApplied },
                    new CreditResult(acadCredit) { Result = Result.Applied }
                };
                CollectionAssert.AreEqual(entity.Results, entity.GetAppliedAndPlannedApplied().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetAppliedSubjects_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetAppliedSubjects_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetAppliedSubjects_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetAppliedSubjects().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedSubjects_Results_No_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetAppliedSubjects().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedSubjects_Results_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                var expected = new List<string>() { acadCredit.SubjectCode };
                CollectionAssert.AreEqual(expected, entity.GetAppliedSubjects().ToList());
            }

            [TestMethod]
            public void GroupResult_GetAppliedSubjects_Applied_AcadResults_Distinct()
            {
                entity.Results = new List<AcadResult>()
                {
                    new CreditResult(acadCredit) { Result = Result.Applied },
                    new CreditResult(acadCredit) { Result = Result.Applied }

                };
                var expectedNonDistinct = entity.Results.Select(res => res.GetSubject()).ToList();
                var expected = expectedNonDistinct.Distinct().ToList();
                CollectionAssert.AreEqual(expected, entity.GetAppliedSubjects().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetPlannedAppliedSubjects_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetPlannedAppliedSubjects_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedSubjects_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetPlannedAppliedSubjects().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedSubjects_Results_No_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetPlannedAppliedSubjects().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedSubjects_Results_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                var expected = new List<string>() { acadCredit.SubjectCode };
                CollectionAssert.AreEqual(expected, entity.GetPlannedAppliedSubjects().ToList());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedSubjects_PlannedApplied_AcadResults_Distinct()
            {
                entity.Results = new List<AcadResult>()
                {
                    new CreditResult(acadCredit) { Result = Result.PlannedApplied },
                    new CreditResult(acadCredit) { Result = Result.PlannedApplied }

                };
                var expectedNonDistinct = entity.Results.Select(res => res.GetSubject()).ToList();
                var expected = expectedNonDistinct.Distinct().ToList();
                CollectionAssert.AreEqual(expected, entity.GetPlannedAppliedSubjects().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetAppliedDepartments_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetAppliedDepartments_Initialize()
            {
                base.Initialize();
                acadCredit.AddDepartment("DEPT1");
                acadCredit.AddDepartment("DEPT2");
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetAppliedDepartments_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetAppliedDepartments().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedDepartments_Results_No_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetAppliedDepartments().Any());
            }

            [TestMethod]
            public void GroupResult_GetAppliedDepartments_Results_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                var expected = acadCredit.DepartmentCodes;
                CollectionAssert.AreEqual(expected, entity.GetAppliedDepartments().ToList());
            }

            [TestMethod]
            public void GroupResult_GetAppliedDepartments_Applied_AcadResults_Distinct()
            {
                entity.Results = new List<AcadResult>()
                {
                    new CreditResult(acadCredit) { Result = Result.Applied },
                    new CreditResult(acadCredit) { Result = Result.Applied }

                };
                var expectedNonDistinct = entity.Results.SelectMany(res => res.GetDepartments()).ToList();
                var expected = expectedNonDistinct.Distinct().ToList();
                CollectionAssert.AreEqual(expected, entity.GetAppliedDepartments().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetPlannedAppliedDepartments_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetPlannedAppliedDepartments_Initialize()
            {
                base.Initialize();
                acadCredit.AddDepartment("DEPT1");
                acadCredit.AddDepartment("DEPT2");
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedDepartments_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetPlannedAppliedDepartments().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedDepartments_Results_No_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetPlannedAppliedDepartments().Any());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedDepartments_Results_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                var expected = acadCredit.DepartmentCodes;
                CollectionAssert.AreEqual(expected, entity.GetPlannedAppliedDepartments().ToList());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedDepartments_PlannedApplied_AcadResults_Distinct()
            {
                entity.Results = new List<AcadResult>()
                {
                    new CreditResult(acadCredit) { Result = Result.PlannedApplied },
                    new CreditResult(acadCredit) { Result = Result.PlannedApplied }

                };
                var expectedNonDistinct = entity.Results.SelectMany(res => res.GetDepartments()).ToList();
                var expected = expectedNonDistinct.Distinct().ToList();
                CollectionAssert.AreEqual(expected, entity.GetPlannedAppliedDepartments().ToList());
            }
        }

        [TestClass]
        public class GroupResult_GetCompletedCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetCompletedCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetCompletedCredits()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied });
                var expected = entity.Results.Sum(res => res.GetCompletedCredits());
                Assert.AreEqual(expected, entity.GetCompletedCredits());
            }
        }

        [TestClass]
        public class GroupResult_GetAppliedCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetAppliedCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetAppliedCredits()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied });
                var expected = entity.Results.Sum(res => res.GetAdjustedCredits());
                Assert.AreEqual(expected, entity.GetAppliedCredits());
            }
        }

        [TestClass]
        public class GroupResult_GetPlannedAppliedCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetPlannedAppliedCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedCredits()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied });
                var expected = entity.Results.Sum(res => res.GetCredits());
                Assert.AreEqual(expected, entity.GetPlannedAppliedCredits());
            }
        }

        [TestClass]
        public class GroupResult_GetNonExtraAppliedCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetNonExtraAppliedCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetNonExtraAppliedCredits_No_NonExtra_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.Extra });
                Assert.AreEqual(0m, entity.GetNonExtraAppliedCredits());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraAppliedCredits_No_Applied_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });
                Assert.AreEqual(0m, entity.GetNonExtraAppliedCredits());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraAppliedCredits_NonExtra_Applied_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                var expected = entity.Results.Sum(res => res.GetAdjustedCredits());
                Assert.AreEqual(expected, entity.GetNonExtraAppliedCredits());
            }
        }

        [TestClass]
        public class GroupResult_GetNonExtraPlannedAppliedCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetNonExtraPlannedAppliedCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedAppliedCredits_No_NonExtra_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.Extra });
                Assert.AreEqual(0m, entity.GetNonExtraPlannedAppliedCredits());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedAppliedCredits_No_PlannedApplied_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.Applied, Explanation = AcadResultExplanation.None });
                Assert.AreEqual(0m, entity.GetNonExtraPlannedAppliedCredits());
            }

            [TestMethod]
            public void GroupResult_GetNonExtraPlannedAppliedCredits_NonExtra_PlannedApplied_Results()
            {
                entity.Results.Add(new CreditResult(acadCredit) { Result = Result.PlannedApplied, Explanation = AcadResultExplanation.None });
                var expected = entity.Results.Sum(res => res.GetCredits());
                Assert.AreEqual(expected, entity.GetNonExtraPlannedAppliedCredits());
            }
        }

        [TestClass]
        public class GroupResult_GetAppliedInstCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetAppliedInstCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetAppliedInstCredits_Results_No_Applied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.AreEqual(0m, entity.GetAppliedInstCredits());
            }

            [TestMethod]
            public void GroupResult_GetAppliedInstCredits_Results_No_Applied_Institutional_AcadResults()
            {
                acadCredit.Type = CreditType.ContinuingEducation;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                var expected = entity.Results.Where(res => res.IsInstitutional()).Sum(ac => ac.GetCompletedCredits());
                Assert.AreEqual(expected, entity.GetAppliedInstCredits());
            }

            [TestMethod]
            public void GroupResult_GetAppliedInstCredits_Results_Applied_Institutional_AcadResults()
            {
                acadCredit.Type = CreditType.Institutional;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                var expected = entity.Results.Where(res => res.IsInstitutional()).Sum(ac => ac.GetCompletedCredits());
                Assert.AreEqual(expected, entity.GetAppliedInstCredits());
            }

        }

        [TestClass]
        public class GroupResult_GetPlannedAppliedInstCredits_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetPlannedAppliedInstCredits_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedInstCredits_Results_No_PlannedApplied_AcadResults()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.AreEqual(0m, entity.GetPlannedAppliedInstCredits());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedInstCredits_Results_No_PlannedApplied_Institutional_AcadResults()
            {
                acadCredit.Type = CreditType.ContinuingEducation;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                var expected = entity.Results.Where(res => res.IsInstitutional()).Sum(ac => ac.GetCompletedCredits());
                Assert.AreEqual(expected, entity.GetPlannedAppliedInstCredits());
            }

            [TestMethod]
            public void GroupResult_GetPlannedAppliedInstCredits_Results_PlannedApplied_Institutional_AcadResults()
            {
                acadCredit.Type = CreditType.Institutional;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.PlannedApplied } };
                var expected = entity.Results.Where(res => res.IsInstitutional()).Sum(ac => ac.GetCompletedCredits());
                Assert.AreEqual(expected, entity.GetPlannedAppliedInstCredits());
            }

        }

        [TestClass]
        public class GroupResult_GetCreditsToIncludeInGpa_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_GetCreditsToIncludeInGpa_Initialize()
            {
                base.Initialize();
                groupEntity.IncludeLowGradesInGpa = true;
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_Null()
            {
                entity.Results = null;
                Assert.IsFalse(entity.GetCreditsToIncludeInGpa().Any());
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_No_Applied_or_ReplacedWithGPAValues_or_MinGrade()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.CourseExcluded } };
                Assert.IsFalse(entity.GetCreditsToIncludeInGpa().Any());
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_Applied()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.Applied } };
                CollectionAssert.AreEqual(entity.Results, entity.GetCreditsToIncludeInGpa().ToList());
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_ReplacedWithGPAValues()
            {
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.ReplacedWithGPAValues } };
                CollectionAssert.AreEqual(entity.Results, entity.GetCreditsToIncludeInGpa().ToList());
            }


            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_MinGrade_GetGpaCredit_Zero()
            {
                acadCredit.AdjustedGpaCredit = 0m;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.MinGrade } };
                Assert.IsFalse(entity.GetCreditsToIncludeInGpa().Any());
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_MinGrade_GetGpaCredit_greater_than_Zero_Group_IncludeLowGradesInGpa_True()
            {
                acadCredit.AdjustedGpaCredit = 10m;
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.MinGrade } };
                CollectionAssert.AreEqual(entity.Results, entity.GetCreditsToIncludeInGpa().ToList());
            }

            [TestMethod]
            public void GroupResult_GetCreditsToIncludeInGpa_Results_MinGrade_GetGpaCredit_greater_than_Zero_Group_IncludeLowGradesInGpa_False()
            {
                acadCredit.AdjustedGpaCredit = 10m;
                groupEntity.IncludeLowGradesInGpa = false;
                entity = new GroupResult(groupEntity);
                entity.Results = new List<AcadResult>() { new CreditResult(acadCredit) { Result = Result.MinGrade } };
                Assert.IsFalse(entity.GetCreditsToIncludeInGpa().Any());
            }
        }

        [TestClass]
        public class GroupResult_ToString_Tests : GroupResultTests
        {
            [TestInitialize]
            public void GroupResult_ToString_Initialize()
            {
                base.Initialize();
                entity = new GroupResult(groupEntity);
            }

            [TestMethod]
            public void GroupResult_ToString()
            {
                var expected = "GroupResult: " + entity.Group.ToString();
                Assert.AreEqual(expected, entity.ToString());
            }
        }

    }
}
