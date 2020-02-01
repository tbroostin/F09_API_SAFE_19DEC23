// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.Student.DegreePlans;
using Ellucian.Colleague.Coordination.Student.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class Degree2PlanDtoToDegreePlanEntityAdapterTests
    {
        DegreePlan2 degreePlanDto;
        Domain.Student.Entities.DegreePlans.DegreePlan degreePlanEntity;
        int planId = 1;
        string personId = "0000001";
        DateTime date1;
        DateTime date2;
        PlannedCourseWarning plannedCourseWarning;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var degreePlanDtoAdapter = new DegreePlanDto2Adapter(adapterRegistryMock.Object, loggerMock.Object);
            
            // Create the DegreePlan2 DTO
            degreePlanDto = new DegreePlan2();
            degreePlanDto.Id = planId;
            degreePlanDto.PersonId = personId;
            degreePlanDto.Version = 1;
            var tc1 = new DegreePlanTerm2();
            tc1.TermId = "2012/FA";
            tc1.PlannedCourses = new List<PlannedCourse2>(){
                new PlannedCourse2(){CourseId = "111", Credits = 3.0m},
                new PlannedCourse2(){CourseId = "222", SectionId = "10001", Credits = 4.0m},
                new PlannedCourse2(){CourseId = "333", SectionId = "333", Credits = 3.0m, GradingType = Dtos.Student.GradingType.PassFail, SectionWaitlistStatus = WaitlistStatus.Active},
                new PlannedCourse2(){CourseId = "444", SectionId = "444", Credits = 3.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.PermissionToRegister}
            };
            var tc2 = new DegreePlanTerm2();
            tc2.TermId = "2013/FA";
            tc2.PlannedCourses = new List<PlannedCourse2>(){
                new PlannedCourse2(){CourseId = "111", Credits = 3.0m},
                new PlannedCourse2(){CourseId = "222", SectionId = "10001", Credits = 4.0m}};
            plannedCourseWarning = new PlannedCourseWarning() { SectionId = "3331", Type = PlannedCourseWarningType.TimeConflict };
            foreach (var pc in tc1.PlannedCourses)
            {
                pc.Warnings = new List<PlannedCourseWarning>() { plannedCourseWarning };
            }

            degreePlanDto.Terms = new List<DegreePlanTerm2>();
            degreePlanDto.NonTermPlannedCourses = new List<PlannedCourse2>();
            degreePlanDto.Terms.Add(tc1);
            degreePlanDto.Terms.Add(tc2);
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse2() { CourseId = "333", SectionId = "3332", Credits = 2.0m });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse2() { CourseId = "444", SectionId = "4442", Credits = 2.0m, GradingType = Dtos.Student.GradingType.PassFail, SectionWaitlistStatus = WaitlistStatus.Active });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse2() { CourseId = "555", SectionId = "5552", Credits = 2.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.PermissionToRegister });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse2() { CourseId = "666", SectionId = "6662", Credits = 2.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.NotWaitlisted });

            date1 = new DateTime(2012, 10, 1, 23, 30, 20);
            date2 = new DateTime(2012, 10, 2, 9, 15, 00);
            degreePlanDto.Approvals = new List<DegreePlanApproval>()
            {
                new DegreePlanApproval() { PersonId = personId, Date = date1, Status = DegreePlanApprovalStatus.Denied, CourseId = "111", TermCode="2012/FA" },
                new DegreePlanApproval() { PersonId = "123", Date = date2, Status = DegreePlanApprovalStatus.Approved, CourseId = "222", TermCode="2012/FA" }
            };
            degreePlanDto.Notes = new List<DegreePlanNote>()
            {
                new DegreePlanNote() {Id = 1, PersonId = "123", Date = date1, Text = "note1"},
                new DegreePlanNote() {Id = 0, PersonId = "456", Text = "note2"}
            };
            degreePlanEntity = degreePlanDtoAdapter.MapToType(degreePlanDto);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_Id()
        {
            Assert.AreEqual(planId, degreePlanEntity.Id);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_PersonId()
        {
            Assert.AreEqual(personId, degreePlanEntity.PersonId);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_Version()
        {
            Assert.AreEqual(1, degreePlanEntity.Version);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_PlannedCoursesInTerm()
        {
            foreach (var termFromDto in degreePlanDto.Terms)
            {
                var countFromDto = termFromDto.PlannedCourses.Count();
                var countFromEntity = degreePlanEntity.GetPlannedCourses(termFromDto.TermId).Count();
                Assert.AreEqual(countFromDto, countFromEntity);
            }
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_PlannedCoursesNoTerm()
        {
            var courses = degreePlanEntity.GetPlannedCourses(null);
            Assert.AreEqual(degreePlanDto.NonTermPlannedCourses.Count(), courses.Count());
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_PlannedCourseProperties()
        {
            var courses = degreePlanEntity.GetPlannedCourses("2012/FA");
            Assert.AreEqual("10001", courses.ElementAt(1).SectionId);
            var cs1 = courses.ElementAt(0);
            Assert.AreEqual(3.0m, cs1.Credits);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_PlannedCourseMessagesNotConverted()
        {
            var plannedCourses = degreePlanEntity.GetPlannedCourses("2012/FA");
            foreach (var item in plannedCourses)
            {
                Assert.IsTrue(item.Warnings.Count() == 0);
            }
            foreach (var item in degreePlanEntity.NonTermPlannedCourses)
            {
                Assert.IsTrue(item.Warnings.Count() == 0);
            }
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_Approvals()
        {
            Assert.AreEqual(2, degreePlanEntity.Approvals.Count());
            var approval = degreePlanEntity.Approvals.ElementAt(0);
            Assert.AreEqual(date1, approval.Date);
            Assert.AreEqual(personId, approval.PersonId);
            Assert.AreEqual(Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Denied, approval.Status);
            Assert.AreEqual("111", approval.CourseId);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_Notes()
        {
            Assert.AreEqual(2, degreePlanEntity.Notes.Count());
            var note = degreePlanEntity.Notes.ElementAt(0);
            Assert.AreEqual(1, note.Id);
            Assert.AreEqual("123", note.PersonId);
            Assert.AreEqual(date1, note.Date);
            Assert.AreEqual("note1", note.Text);
            note = degreePlanEntity.Notes.ElementAt(1);
            Assert.AreEqual(0, note.Id);
            Assert.AreEqual("note2", note.Text);
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_GradingType()
        {
            // Test the grading type in the term-based planned courses
            foreach (var termFromDto in degreePlanDto.Terms)
            {
                foreach (var courseFromDto in termFromDto.PlannedCourses)
                {
                    var coursesFromEntity = degreePlanEntity.GetPlannedCourses(termFromDto.TermId);
                    Assert.IsTrue(coursesFromEntity.Any(x => x.GradingType.ToString() == courseFromDto.GradingType.ToString()));
                }
            }

            // Test the grading type in the non-term-based planned courses
            foreach (var courseFromDto in degreePlanDto.NonTermPlannedCourses)
            {
                var coursesFromEntity = degreePlanEntity.NonTermPlannedCourses.Where(x => x.CourseId == courseFromDto.CourseId);
                Assert.IsTrue(coursesFromEntity.Any(x => x.GradingType.ToString() == courseFromDto.GradingType.ToString()));
            }
        }

        [TestMethod]
        public void DegreePlanDto2Adapter_WaitlistStatus()
        {
            // Test the waitlist status in the term-based planned courses
            foreach (var termFromDto in degreePlanDto.Terms)
            {
                foreach (var courseFromDto in termFromDto.PlannedCourses)
                {
                    var coursesFromEntity = degreePlanEntity.GetPlannedCourses(termFromDto.TermId);
                    Assert.IsTrue(coursesFromEntity.Any(x => x.WaitlistedStatus.ToString() == courseFromDto.SectionWaitlistStatus.ToString()));
                }
            }

            // Test the waitlist status in the non-term-based planned courses
            foreach (var courseFromDto in degreePlanDto.NonTermPlannedCourses)
            {
                var coursesFromEntity = degreePlanEntity.NonTermPlannedCourses.Where(x => x.CourseId == courseFromDto.CourseId);
                Assert.IsTrue(coursesFromEntity.Any(x => x.WaitlistedStatus.ToString() == courseFromDto.SectionWaitlistStatus.ToString()));
            }
        }
    }
}
