// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
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

namespace Ellucian.Colleague.Coordination.Planning.Tests.Adapters
{
    [TestClass]
    public class DegreePlanEntitytoDegreePlan4DtoAdapterTests
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
        DegreePlan4 degreePlanDto;
        DateTimeOffset date;

        Domain.Student.Entities.DegreePlans.DegreePlan degreePlanEntity;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();

            var degreePlanAdapter = new DegreePlanEntity4Adapter(adapterRegistryMock.Object, loggerMock.Object);

            var datetimeOffsetAdapter = new DateTimeOffsetToDateTimeAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<DateTimeOffset, DateTime>()).Returns(datetimeOffsetAdapter);            
            
            var plannedCourseWarningDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, PlannedCourseWarning2>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.DegreePlans.PlannedCourseWarning, PlannedCourseWarning2>()).Returns(plannedCourseWarningDtoAdapter);

            var degreePlanApprovalDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, DegreePlanApproval2>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.DegreePlans.DegreePlanApproval, DegreePlanApproval2>()).Returns(degreePlanApprovalDtoAdapter);
            
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
            plannedCourse3 = new Domain.Student.Entities.DegreePlans.PlannedCourse("333", "10003", Domain.Student.Entities.GradingType.Graded, Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted, "fred", DateTimeOffset.Now) { Credits = 2.0m, IsProtected = true };
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
            date = new DateTimeOffset(2012, 10, 1, 10, 15, 00, new TimeSpan(-7, 0, 0));
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

            // Add restricted notes
            var restrictedNotes = new List<Domain.Student.Entities.DegreePlans.DegreePlanNote>() {
                new Domain.Student.Entities.DegreePlans.DegreePlanNote(3, "0000001", date, "restricted note1"),
                new Domain.Student.Entities.DegreePlans.DegreePlanNote(4, "0000456", date, "restricted note2")
            };
            degreePlanEntity.RestrictedNotes = restrictedNotes;

            // Convert to DTO
            degreePlanDto = degreePlanAdapter.MapToType(degreePlanEntity);

        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_Id()
        {
            Assert.AreEqual(planId, degreePlanDto.Id);

        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_PersonId()
        {
            Assert.AreEqual(personId, degreePlanDto.PersonId);
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_Version()
        {
            Assert.AreEqual(2, degreePlanDto.Version);
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_TermCourses()
        {
            Assert.AreEqual(1, degreePlanDto.Terms.Count());
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_CoursesInATerm()
        {
            var tc = degreePlanDto.Terms.ElementAt(0);
            Assert.AreEqual(2, tc.GetPlannedCourseIds().Count());
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_PlannedCourseProperties()
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
                    var dtoPlannedCourse = dtoTerm.PlannedCourses.Where(pc => pc.CourseId == domainPlannedCourse.CourseId &&
                                                                              pc.SectionId == domainPlannedCourse.SectionId).First();
                    Assert.AreEqual(domainPlannedCourse.AddedBy, dtoPlannedCourse.AddedBy);
                    Assert.AreEqual(domainPlannedCourse.AddedOn, dtoPlannedCourse.AddedOn);
                    Assert.AreEqual(domainPlannedCourse.Credits, dtoPlannedCourse.Credits);
                    Assert.AreEqual(domainPlannedCourse.GradingType.ToString(), dtoPlannedCourse.GradingType.ToString());
                    if (domainPlannedCourse.IsProtected.HasValue)
                    {
                        Assert.AreEqual(domainPlannedCourse.IsProtected, dtoPlannedCourse.IsProtected);
                    }
                    else
                    {
                        Assert.IsFalse(dtoPlannedCourse.IsProtected);
                    }
                    Assert.AreEqual(domainPlannedCourse.WaitlistedStatus.ToString(), dtoPlannedCourse.SectionWaitlistStatus.ToString());
                    Assert.AreEqual(domainPlannedCourse.Warnings.Count(), dtoPlannedCourse.Warnings.Count());
                }
            }
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_PlannedCourseWarnings()
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
        public void DegreePlanEntity4Adapter_NonTermPlannedCourse()
        {
            var ntpc = degreePlanDto.NonTermPlannedCourses.ElementAt(0);
            Assert.AreEqual(plannedCourse3.CourseId, ntpc.CourseId);
            Assert.AreEqual(plannedCourse3.SectionId, ntpc.SectionId);
            Assert.AreEqual(plannedCourse3.Credits, ntpc.Credits);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, ntpc.Warnings.ElementAt(0).Type);
            Assert.AreEqual(plannedCourse3.Warnings.Count(), ntpc.Warnings.Count());
            Assert.AreEqual(plannedCourse3.AddedBy, ntpc.AddedBy);
            Assert.AreEqual(plannedCourse3.AddedOn, ntpc.AddedOn);
            Assert.AreEqual(plannedCourse3.IsProtected, ntpc.IsProtected);
            Assert.AreEqual(plannedCourse3.GradingType.ToString(), ntpc.GradingType.ToString());
            Assert.AreEqual(plannedCourse3.WaitlistedStatus.ToString(), ntpc.SectionWaitlistStatus.ToString());
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_NonTerm_PlannedCourseWarnings()
        {
            var pc = degreePlanDto.NonTermPlannedCourses.ElementAt(1);
            Assert.AreEqual(1, pc.Warnings.Count());
            var pcWarning1 = pc.Warnings.ElementAt(0);
            Assert.AreEqual(PlannedCourseWarningType.UnmetRequisite, pcWarning1.Type);
            Assert.AreEqual(warning4.Requisite.CorequisiteCourseId, pcWarning1.Requisite.CorequisiteCourseId);
        }


        [TestMethod]
        public void DegreePlanEntity4Adapter_Approvals()
        {
            Assert.AreEqual(2, degreePlanDto.Approvals.Count());
            var approval = degreePlanDto.Approvals.Where(a => a.PersonId == personId).First();
            Assert.AreEqual(personId, approval.PersonId);
            Assert.AreEqual(date, approval.Date);
            Assert.AreEqual("2012/FA", approval.TermCode);
        }

        [TestMethod]
        public void DegreePlanEntity4Adapter_Notes()
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
        [TestMethod]
        public void DegreePlanEntity4Adapter_RestrictedNotes()
        {
            Assert.AreEqual(2, degreePlanDto.RestrictedNotes.Count());
            var rnote = degreePlanDto.RestrictedNotes.ElementAt(0);
            Assert.AreEqual(3, rnote.Id);
            Assert.AreEqual("0000001", rnote.PersonId);
            Assert.AreEqual(date, rnote.Date);
            Assert.AreEqual("restricted note1", rnote.Text);
            Assert.AreEqual(PersonType.Student, rnote.PersonType);
            rnote = degreePlanDto.RestrictedNotes.ElementAt(1);
            Assert.AreEqual(4, rnote.Id);
            Assert.AreEqual("0000456", rnote.PersonId);
            Assert.AreEqual(date, rnote.Date);
            Assert.AreEqual("restricted note2", rnote.Text);
            Assert.AreEqual(PersonType.Advisor, rnote.PersonType);
        }
    }
}
