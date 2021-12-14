// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.TestUtil;

using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    [TestClass]
    public class GroupResultEntityToGroupResult3DtoAdapterTests
    {
        public Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult groupResultEntity;
        public GroupResult3 groupResultDto;
        public GroupResultEntityToGroupResult3DtoAdapter adapter;

        public TestCourseRepository tcr;
        public TestAcademicCreditRepository tacr;
        public TestProgramRequirementsRepository tprr;
        public Course co;
        public string termCode;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            adapter = new GroupResultEntityToGroupResult3DtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            tcr = new TestCourseRepository();
            tacr = new TestAcademicCreditRepository();
            tprr = new TestProgramRequirementsRepository();
            Ellucian.Colleague.Domain.Student.Entities.Requirements.ProgramRequirements pr = tprr.Get("x", "x"); // contents don't matter, we just need a Group to seed the GroupResult
            //Requirement r = pr.Requirements.First();
            Ellucian.Colleague.Domain.Student.Entities.Requirements.Subrequirement s = pr.Requirements.First().SubRequirements.First();

            groupResultEntity = new Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult(new Ellucian.Colleague.Domain.Student.Entities.Requirements.Group("10000", "Group1", s)) { CompletionStatus = Ellucian.Colleague.Domain.Student.Entities.Requirements.CompletionStatus.PartiallyCompleted, PlanningStatus = Ellucian.Colleague.Domain.Student.Entities.Requirements.PlanningStatus.PartiallyPlanned };
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.Satisfied);
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinGpa);
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinInstCredits);

            Domain.Student.Entities.AcademicCredit ac = tacr.GetAsync("1").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.Applied };
            groupResultEntity.Results.Add(ar);

            Domain.Student.Entities.AcademicCredit ac1 = tacr.GetAsync("2").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar1 = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac1) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.ReplacedWithGPAValues };
            groupResultEntity.Results.Add(ar1);

            co = tcr.GetAsync("139").Result;
            termCode = "2014/FA";
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar2 = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CourseResult(new Ellucian.Colleague.Domain.Student.Entities.Requirements.PlannedCredit(co, termCode)) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.PlannedApplied };
            groupResultEntity.Results.Add(ar2);
            

            groupResultDto = adapter.MapToType(groupResultEntity);

        }

        [TestMethod]
        public void GroupResultAdapter()
        {
            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
        }
        [TestMethod]
        public void Assign_MinGroupStatus_GroupResultAdapter()
        {
            groupResultEntity.MinGroupStatus = Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResultMinGroupStatus.Extra;
            GroupResult3 groupResultDto = adapter.MapToType(groupResultEntity);
            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.Extra);
        }

        [TestMethod]
        public void Validate_Related_Courses()
        {
            Assert.IsNull(groupResultDto.RelatedAcademicCredits);
        }
        
    }

    [TestClass]
    public class GroupResultEntityToGroupResult3DtoAdapterTests_WithShowRelatedCourses_Flag_InConctructor
    {
        public Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult groupResultEntity;
        public GroupResult3 groupResultDto;
        public GroupResultEntityToGroupResult3DtoAdapter adapter;

        public TestCourseRepository tcr;
        public TestAcademicCreditRepository tacr;
        public TestProgramRequirementsRepository tprr;
        public Course co;
        public string termCode;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            adapter = new GroupResultEntityToGroupResult3DtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            tcr = new TestCourseRepository();
            tacr = new TestAcademicCreditRepository();
            tprr = new TestProgramRequirementsRepository();
            Ellucian.Colleague.Domain.Student.Entities.Requirements.ProgramRequirements pr = tprr.Get("x", "x"); // contents don't matter, we just need a Group to seed the GroupResult
            //Requirement r = pr.Requirements.First();
            Ellucian.Colleague.Domain.Student.Entities.Requirements.Subrequirement s = pr.Requirements.First().SubRequirements.First();

            groupResultEntity = new Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult(new Ellucian.Colleague.Domain.Student.Entities.Requirements.Group("10000", "Group1", s), true) { CompletionStatus = Ellucian.Colleague.Domain.Student.Entities.Requirements.CompletionStatus.PartiallyCompleted, PlanningStatus = Ellucian.Colleague.Domain.Student.Entities.Requirements.PlanningStatus.PartiallyPlanned };
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.Satisfied);
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinGpa);
            groupResultEntity.Explanations.Add(Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupExplanation.MinInstCredits);

            Domain.Student.Entities.AcademicCredit ac = tacr.GetAsync("1").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.Applied };
            groupResultEntity.Results.Add(ar);

            Domain.Student.Entities.AcademicCredit ac1 = tacr.GetAsync("2").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar1 = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac1) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.ReplacedWithGPAValues };
            groupResultEntity.Results.Add(ar1);

            co = tcr.GetAsync("139").Result;
            termCode = "2014/FA";
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar2 = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CourseResult(new Ellucian.Colleague.Domain.Student.Entities.Requirements.PlannedCredit(co, termCode)) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.PlannedApplied };
            groupResultEntity.Results.Add(ar2);



            groupResultDto = adapter.MapToType(groupResultEntity);

        }

        [TestMethod]
        public void GroupResultAdapter_With_EmptyRelatedCourses()
        {
            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
            Assert.IsNotNull(groupResultDto.RelatedAcademicCredits);
            Assert.AreEqual(0, groupResultDto.RelatedAcademicCredits.Count);
            Assert.IsNotNull(groupResultDto.RelatedPlannedCredits);
            Assert.AreEqual(0, groupResultDto.RelatedPlannedCredits.Count);
        }

        [TestMethod]
        public void GroupResultAdapter_With_NonEmptyRelatedCourses()
        {
            Domain.Student.Entities.AcademicCredit ac = tacr.GetAsync("3").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.MaxCourses };
            groupResultEntity.Results.Add(ar);
            groupResultDto = adapter.MapToType(groupResultEntity);

            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
            Assert.IsNotNull(groupResultDto.RelatedAcademicCredits);
            Assert.AreEqual(1, groupResultDto.RelatedAcademicCredits.Count);
            Assert.AreEqual("3", groupResultDto.RelatedAcademicCredits[0].AcademicCreditId);
            Assert.AreEqual(AcadResultExplanation.None, groupResultDto.RelatedAcademicCredits[0].Explanation);
            Assert.IsNotNull(groupResultDto.RelatedPlannedCredits);
            Assert.AreEqual(0, groupResultDto.RelatedPlannedCredits.Count);
        }
        [TestMethod]
        public void GroupResultAdapter_NonEmptyRelatedCourses_With_Extra_Notation()
        {
            Domain.Student.Entities.AcademicCredit ac = tacr.GetAsync("3").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.MaxCourses };
            ar.Explanation = Domain.Student.Entities.Requirements.AcadResultExplanation.Extra;
            groupResultEntity.Results.Add(ar);
            groupResultDto = adapter.MapToType(groupResultEntity);

            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
            Assert.IsNotNull(groupResultDto.RelatedAcademicCredits);
            Assert.AreEqual(1, groupResultDto.RelatedAcademicCredits.Count);
            Assert.AreEqual("3", groupResultDto.RelatedAcademicCredits[0].AcademicCreditId);
            Assert.AreEqual(AcadResultExplanation.Extra, groupResultDto.RelatedAcademicCredits[0].Explanation);
            Assert.IsNotNull(groupResultDto.RelatedPlannedCredits);
            Assert.AreEqual(0, groupResultDto.RelatedPlannedCredits.Count);
        }
        [TestMethod]
        public void GroupResultAdapter_NonEmptyRelatedCourses_With_MinGrade()
        {
            Domain.Student.Entities.AcademicCredit ac = tacr.GetAsync("3").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.MinGrade };
            ar.Explanation = Domain.Student.Entities.Requirements.AcadResultExplanation.Extra;
            groupResultEntity.Results.Add(ar);
            groupResultDto = adapter.MapToType(groupResultEntity);

            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
            Assert.IsNotNull(groupResultDto.RelatedAcademicCredits);
            Assert.AreEqual(1, groupResultDto.RelatedAcademicCredits.Count);
            Assert.AreEqual("3", groupResultDto.RelatedAcademicCredits[0].AcademicCreditId);
            Assert.AreEqual(AcadResultExplanation.MinGrade, groupResultDto.RelatedAcademicCredits[0].Explanation);
            Assert.IsNotNull(groupResultDto.RelatedPlannedCredits);
            Assert.AreEqual(0, groupResultDto.RelatedPlannedCredits.Count);
        }

        [TestMethod]
        public void GroupResultAdapter_NonEmptyRelatedCourses_PlannedCourses_ThatAreRepeated()
        {
            co = tcr.GetAsync("110").Result;
            termCode = "2015/FA";
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar2 = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CourseResult(new Ellucian.Colleague.Domain.Student.Entities.Requirements.PlannedCredit(co, "2015/FA") {ReplacedStatus=ReplacedStatus.ReplaceInProgress }) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.ReplaceInProgress };
            
            groupResultEntity.Results.Add(ar2);
            groupResultDto = adapter.MapToType(groupResultEntity);

            Assert.AreEqual("10000", groupResultDto.GroupId);
            Assert.IsTrue(groupResultDto.AppliedAcademicCredits.FirstOrDefault(a => a.AcademicCreditId.Contains("1")) != null);
            Assert.IsTrue(groupResultDto.AppliedPlannedCourses.FirstOrDefault(a => a.CourseId.Contains("139")) != null);
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual("2014/FA", groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual("139", groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
            Assert.AreEqual(groupResultDto.MinGroupStatus, GroupResultMinGroupStatus.None);
            Assert.IsNotNull(groupResultDto.RelatedPlannedCredits);
            Assert.AreEqual(1, groupResultDto.RelatedPlannedCredits.Count);
            Assert.AreEqual("110", groupResultDto.RelatedPlannedCredits[0].CourseId);
            Assert.AreEqual(Ellucian.Colleague.Dtos.Student.ReplacedStatus.ReplaceInProgress, groupResultDto.RelatedPlannedCredits[0].ReplacedStatus);

        }
    }

}
