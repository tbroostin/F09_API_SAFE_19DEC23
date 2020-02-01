// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Student.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class DegreePlanEntitytoDegreePlan2DtoAdapterTests
    {
        int planId = 1;
        string personId = "0000001";
        int version = 2;
        Domain.Student.Entities.DegreePlans.PlannedCourse plannedCourse1;
        Domain.Student.Entities.DegreePlans.PlannedCourse plannedCourse2;
        Domain.Student.Entities.DegreePlans.PlannedCourse plannedCourse3;
        Domain.Student.Entities.DegreePlans.PlannedCourse plannedCourse4;
        Domain.Student.Entities.DegreePlans.PlannedCourseWarning warning1;
        Domain.Student.Entities.DegreePlans.PlannedCourseWarning warning2;
        Domain.Student.Entities.DegreePlans.PlannedCourseWarning warning3;
        Domain.Student.Entities.DegreePlans.PlannedCourseWarning warning4;
        Domain.Student.Entities.Requisite coreq;
        Domain.Student.Entities.Requisite prereq;
        Domain.Student.Entities.SectionRequisite sectionreq;
        Dtos.Student.DegreePlans.DegreePlan2 degreePlanDto;
        DateTime date;

        Domain.Student.Entities.DegreePlans.DegreePlan degreePlanEntity;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();

            var degreePlanAdapter = new DegreePlanEntity2Adapter(adapterRegistryMock.Object, loggerMock.Object);

            var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<System.DateTimeOffset, System.DateTime>()).Returns(datetimeOffsetAdapter);            
            
            var plannedCourseWarningDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, PlannedCourseWarning>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, PlannedCourseWarning>()).Returns(plannedCourseWarningDtoAdapter);

            var degreePlanApprovalDtoAdapter = new DegreePlanApprovalEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, DegreePlanApproval>()).Returns(degreePlanApprovalDtoAdapter);
            
            var requisiteDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Requisite, Requisite>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Requisite, Requisite>()).Returns(requisiteDtoAdapter);
            
            var sectionRequisiteDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionRequisite, SectionRequisite>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionRequisite, SectionRequisite>()).Returns(sectionRequisiteDtoAdapter);

            // Create the entity to convert
            degreePlanEntity = new Domain.Student.Entities.DegreePlans.DegreePlan(planId, personId, version);
            // Add term course with message
            prereq = new Domain.Student.Entities.Requisite("PREREQ", true, Domain.Student.Entities.RequisiteCompletionOrder.Previous, false);
            warning1 = new Domain.Student.Entities.DegreePlans.PlannedCourseWarning(Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { Requisite = prereq };
            coreq = new Domain.Student.Entities.Requisite("999", false);
            warning3 = new Domain.Student.Entities.DegreePlans.PlannedCourseWarning(Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { Requisite = coreq };
            plannedCourse1 = new Domain.Student.Entities.DegreePlans.PlannedCourse("111", null) { Credits = 3.0m };
            plannedCourse1.AddWarning(warning1);
            plannedCourse1.AddWarning(warning3);
            degreePlanEntity.AddCourse(plannedCourse1, "2012/FA");
            // Add term course and section
            plannedCourse2 = new Domain.Student.Entities.DegreePlans.PlannedCourse("222", "10002") { Credits = 4.0m };
            degreePlanEntity.AddCourse(plannedCourse2, "2012/FA");
            sectionreq = new Domain.Student.Entities.SectionRequisite("221");
            warning2 = new Domain.Student.Entities.DegreePlans.PlannedCourseWarning(Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { SectionRequisite = sectionreq };
            // Add nonterm course section with message
            plannedCourse3 = new Domain.Student.Entities.DegreePlans.PlannedCourse("333", "10003") { Credits = 2.0m };
            plannedCourse3.AddWarning(warning2);
            degreePlanEntity.AddCourse(plannedCourse3, "");
            // Add second nonterm course section with message
            plannedCourse4 = new Domain.Student.Entities.DegreePlans.PlannedCourse("444", "10004") { Credits = 2.0m };
            var coreq2 = new Domain.Student.Entities.Requisite("999", true);
            warning4 = new Domain.Student.Entities.DegreePlans.PlannedCourseWarning(Domain.Student.Entities.DegreePlans.PlannedCourseWarningType.UnmetRequisite) { Requisite = coreq };
            plannedCourse4.AddWarning(warning4);
            degreePlanEntity.AddCourse(plannedCourse4, "");

            // Add approval statuses
            var approvals = new List<Domain.Student.Entities.DegreePlans.DegreePlanApproval>();
            date = new DateTime(2012, 10, 1, 10, 15, 00);
            var approval1 = new Domain.Student.Entities.DegreePlans.DegreePlanApproval(personId, Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Approved, date, "111", "2012/FA");
            approvals.Add(approval1);
            var approval2 = new Domain.Student.Entities.DegreePlans.DegreePlanApproval("1234567", Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Denied, date.AddDays(1), "222", "2012/FA");
            approvals.Add(approval2);
            degreePlanEntity.Approvals = approvals;

            // Add notes
            var notes = new List<Domain.Student.Entities.DegreePlans.DegreePlanNote>() {
                new Domain.Student.Entities.DegreePlans.DegreePlanNote(1, "0000001", date, "note1"),
                new Domain.Student.Entities.DegreePlans.DegreePlanNote(2, "0000456", date, "note2")
            };
            degreePlanEntity.Notes = notes;

            // Convert to DTO
            degreePlanDto = degreePlanAdapter.MapToType(degreePlanEntity);

        }

        [TestMethod]
        public void DegreePlanDtoAdapter_Id()
        {
            Assert.AreEqual(planId, degreePlanDto.Id);

        }

        [TestMethod]
        public void DegreePlanDtoAdapter_PersonId()
        {
            Assert.AreEqual(personId, degreePlanDto.PersonId);
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_Version()
        {
            Assert.AreEqual(2, degreePlanDto.Version);
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_TermCourses()
        {
            Assert.AreEqual(1, degreePlanDto.Terms.Count());
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_CoursesInATerm()
        {
            var tc = degreePlanDto.Terms.ElementAt(0);
            Assert.AreEqual(2, tc.GetPlannedCourseIds().Count());
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_PlannedCourseProperties()
        {
            foreach (var domainTermId in degreePlanEntity.TermIds)
            {
                // Ensure the DTO has a term to match the domain entity
                var dtoTerm = degreePlanDto.Terms.Where(t => t.TermId == domainTermId).First();

                // Now check each term in the DTO and domain entity for matching planned courses
                var domainTermPlannedCourses = degreePlanEntity.GetPlannedCourses(domainTermId);
                foreach (var domainPlannedCourse in domainTermPlannedCourses)
                {
                    // There should only be one planned course within any given term that matches on ALL attributes
                    // TODO: change to Where.First and explicitly assert?
                    var dtoPlannedCourse = dtoTerm.PlannedCourses.Where(pc => pc.CourseId == domainPlannedCourse.CourseId &&
                                                                              pc.SectionId == domainPlannedCourse.SectionId &&
                                                                              pc.Credits == domainPlannedCourse.Credits &&
                                                                              pc.GradingType.ToString() == domainPlannedCourse.GradingType.ToString() &&
                                                                              pc.SectionWaitlistStatus.ToString() == domainPlannedCourse.WaitlistedStatus.ToString() &&
                                                                              pc.AddedBy == domainPlannedCourse.AddedBy &&
                                                                              pc.AddedOn == domainPlannedCourse.AddedOn);
                    Assert.IsTrue(dtoPlannedCourse.Count() == 1, "There should only ever be one and only one planned course that matches on all criteria.");
                }
            }
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_PlannedCourseWarnings()
        {
            var tc = degreePlanDto.Terms.ElementAt(0);
            var pc = tc.PlannedCourses.ElementAt(0);
            Assert.AreEqual(2, pc.Warnings.Count());
            var pcWarning1 = pc.Warnings.ElementAt(0);
            Assert.AreEqual(warning1.Requisite.RequirementCode, pcWarning1.Requisite.RequirementCode);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pcWarning1.Type);
            var pcWarning2 = pc.Warnings.ElementAt(1);
            Assert.AreEqual(warning3.SectionId, pcWarning2.SectionId);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pcWarning2.Type);
            Assert.AreEqual(warning3.Requisite.CorequisiteCourseId, pcWarning2.Requisite.CorequisiteCourseId);
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_NonTermPlannedCourse()
        {
            var ntpc = degreePlanDto.NonTermPlannedCourses.ElementAt(0);
            Assert.AreEqual(plannedCourse3.CourseId, ntpc.CourseId);
            Assert.AreEqual(plannedCourse3.SectionId, ntpc.SectionId);
            Assert.AreEqual(plannedCourse3.Credits, ntpc.Credits);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, ntpc.Warnings.ElementAt(0).Type);
            Assert.AreEqual(plannedCourse3.Warnings.Count(), ntpc.Warnings.Count());
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_NonTerm_PlannedCourseWarnings()
        {
            var pc = degreePlanDto.NonTermPlannedCourses.ElementAt(1);
            Assert.AreEqual(1, pc.Warnings.Count());
            var pcWarning1 = pc.Warnings.ElementAt(0);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pcWarning1.Type);
            Assert.AreEqual(warning4.Requisite.CorequisiteCourseId, pcWarning1.Requisite.CorequisiteCourseId);
        }


        [TestMethod]
        public void DegreePlanDtoAdapter_Approvals()
        {
            Assert.AreEqual(2, degreePlanDto.Approvals.Count());
            var approval = degreePlanDto.Approvals.Where(a => a.PersonId == personId).First();
            Assert.AreEqual(personId, approval.PersonId);
            Assert.AreEqual(date, approval.Date);
            Assert.AreEqual("2012/FA", approval.TermCode);
        }

        [TestMethod]
        public void DegreePlanDtoAdapter_Notes()
        {
            Assert.AreEqual(2, degreePlanDto.Notes.Count());
            var note = degreePlanDto.Notes.ElementAt(0);
            Assert.AreEqual(1, note.Id);
            Assert.AreEqual("0000001", note.PersonId);
            Assert.AreEqual(date, note.Date);
            Assert.AreEqual("note1", note.Text);
            Assert.AreEqual(PersonType.Student, note.PersonType);
            note = degreePlanDto.Notes.ElementAt(1);
            Assert.AreEqual(2, note.Id);
            Assert.AreEqual("0000456", note.PersonId);
            Assert.AreEqual(date, note.Date);
            Assert.AreEqual("note2", note.Text);
            Assert.AreEqual(PersonType.Advisor, note.PersonType);
        }
    }
}
