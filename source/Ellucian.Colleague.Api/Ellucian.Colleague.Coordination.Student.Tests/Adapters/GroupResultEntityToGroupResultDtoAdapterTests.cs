// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
    public class GroupResultEntityToGroupResultDtoAdapterTests
    {
        public Ellucian.Colleague.Domain.Student.Entities.Requirements.GroupResult groupResultEntity;
        public GroupResult groupResultDto;
        public GroupResultDtoAdapter adapter;

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
            adapter = new GroupResultDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

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

            AcademicCredit ac = tacr.GetAsync("1").Result;
            Ellucian.Colleague.Domain.Student.Entities.Requirements.AcadResult ar = new Ellucian.Colleague.Domain.Student.Entities.Requirements.CreditResult(ac) { Result = Ellucian.Colleague.Domain.Student.Entities.Requirements.Result.Applied };
            groupResultEntity.Results.Add(ar);

            AcademicCredit ac1 = tacr.GetAsync("2").Result;
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
            Assert.IsTrue(groupResultDto.AppliedAcademicCreditIds.Contains("1"));
            Assert.IsTrue(groupResultDto.AppliedPlannedCourseIds.Contains("139"));
            Assert.IsTrue(groupResultDto.AcademicCreditIdsIncludedInGPA.Contains("2"));
            Assert.AreEqual(1, groupResultDto.AppliedPlannedCredits.Count());
            Assert.AreEqual(termCode, groupResultDto.AppliedPlannedCredits.ElementAt(0).TermCode);
            Assert.AreEqual(co.Id, groupResultDto.AppliedPlannedCredits.ElementAt(0).CourseId);
            Assert.AreEqual(CompletionStatus.PartiallyCompleted, groupResultDto.CompletionStatus);
            Assert.AreEqual(PlanningStatus.PartiallyPlanned, groupResultDto.PlanningStatus);
            Assert.IsTrue(groupResultDto.MinGpaIsNotMet);
            Assert.IsTrue(groupResultDto.MinInstitutionalCreditsIsNotMet);
        }

    }
}
