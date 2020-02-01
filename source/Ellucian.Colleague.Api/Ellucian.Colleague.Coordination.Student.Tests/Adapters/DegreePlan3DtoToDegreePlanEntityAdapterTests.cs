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
    public class DegreePlan3DtoToDegreePlanEntityAdapterTests
    {
        DegreePlan3 degreePlanDto;
        Domain.Student.Entities.DegreePlans.DegreePlan degreePlanEntity;
        int planId = 1;
        string personId = "0000001";
        DateTimeOffset date1;
        DateTimeOffset date2;
        PlannedCourseWarning2 plannedCourseWarning;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var degreePlanDtoAdapter = new DegreePlanDto3Adapter(adapterRegistryMock.Object, loggerMock.Object);

            // Create the DegreePlan3 DTO
            degreePlanDto = new DegreePlan3();
            degreePlanDto.Id = planId;
            degreePlanDto.PersonId = personId;
            degreePlanDto.Version = 1;
            var degreePlanTerm1 = new DegreePlanTerm3();
            degreePlanTerm1.TermId = "2012/FA";
            degreePlanTerm1.PlannedCourses = new List<PlannedCourse3>(){
                new PlannedCourse3(){CourseId = "111", Credits = 3.0m},
                new PlannedCourse3(){CourseId = "222", SectionId = "10001", Credits = 4.0m},
                new PlannedCourse3(){CourseId = "333", SectionId = "333", Credits = 3.0m, GradingType = Dtos.Student.GradingType.PassFail, SectionWaitlistStatus = WaitlistStatus.Active},
                new PlannedCourse3(){CourseId = "444", SectionId = "444", Credits = 3.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.PermissionToRegister, AddedBy = "SMITH", AddedOn = DateTimeOffset.Now }
            };
            var degreePlanTerm2 = new DegreePlanTerm3();
            degreePlanTerm2.TermId = "2013/FA";
            degreePlanTerm2.PlannedCourses = new List<PlannedCourse3>(){
                new PlannedCourse3(){CourseId = "111", Credits = 3.0m},
                new PlannedCourse3(){CourseId = "222", SectionId = "10001", Credits = 4.0m}};
            plannedCourseWarning = new PlannedCourseWarning2() { SectionId = "3331", Type = PlannedCourseWarningType.TimeConflict };
            foreach (var pc in degreePlanTerm1.PlannedCourses)
            {
                pc.Warnings = new List<PlannedCourseWarning2>() { plannedCourseWarning };
            }

            degreePlanDto.Terms = new List<DegreePlanTerm3>();
            degreePlanDto.NonTermPlannedCourses = new List<PlannedCourse3>();
            degreePlanDto.Terms.Add(degreePlanTerm1);
            degreePlanDto.Terms.Add(degreePlanTerm2);
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse3() { CourseId = "333", SectionId = "3332", Credits = 2.0m });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse3() { CourseId = "444", SectionId = "4442", Credits = 2.0m, GradingType = Dtos.Student.GradingType.PassFail, SectionWaitlistStatus = WaitlistStatus.Active });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse3() { CourseId = "555", SectionId = "5552", Credits = 2.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.PermissionToRegister });
            degreePlanDto.NonTermPlannedCourses.Add(new PlannedCourse3() { CourseId = "666", SectionId = "6662", Credits = 2.0m, GradingType = Dtos.Student.GradingType.Audit, SectionWaitlistStatus = WaitlistStatus.NotWaitlisted, AddedBy = "SMITH", AddedOn = DateTimeOffset.Now });

            date1 = new DateTimeOffset(2012, 10, 1, 23, 30, 20, new TimeSpan(-7, 0, 0));
            date2 = new DateTimeOffset(2012, 10, 2, 9, 15, 00, new TimeSpan(-7, 0, 0));
            degreePlanDto.Approvals = new List<DegreePlanApproval2>()
            {
                new DegreePlanApproval2() { PersonId = personId, Date = date1, Status = DegreePlanApprovalStatus.Denied, CourseId = "111", TermCode="2012/FA" },
                new DegreePlanApproval2() { PersonId = "123", Date = date2, Status = DegreePlanApprovalStatus.Approved, CourseId = "222", TermCode="2012/FA" }
            };
            degreePlanDto.Notes = new List<DegreePlanNote2>()
            {
                new DegreePlanNote2() {Id = 1, PersonId = "123", Date = date1, Text = "note1"},
                new DegreePlanNote2() {Id = 0, PersonId = "456", Text = "note2"}
            };
            degreePlanEntity = degreePlanDtoAdapter.MapToType(degreePlanDto);
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_Id()
        {
            Assert.AreEqual(planId, degreePlanEntity.Id);
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PersonId()
        {
            Assert.AreEqual(personId, degreePlanEntity.PersonId);
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_Version()
        {
            Assert.AreEqual(1, degreePlanEntity.Version);
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PlannedCoursesInTerm()
        {
            foreach (var termFromDto in degreePlanDto.Terms)
            {
                var countFromDto = termFromDto.PlannedCourses.Count();
                var countFromEntity = degreePlanEntity.GetPlannedCourses(termFromDto.TermId).Count();
                Assert.AreEqual(countFromDto, countFromEntity);
            }
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PlannedCoursesNoTerm()
        {
            var courses = degreePlanEntity.GetPlannedCourses(null);
            Assert.AreEqual(degreePlanDto.NonTermPlannedCourses.Count(), courses.Count());
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PlannedCourseNonTermProperties()
        {
            var dtoPlannedCourse = degreePlanDto.NonTermPlannedCourses.Where(c => c.CourseId == "666").First();
            var courses = degreePlanEntity.GetPlannedCourses(string.Empty);
            var plannedCourse = courses.Where(c => c.CourseId == "666").First();
            Assert.AreEqual(dtoPlannedCourse.AddedBy, plannedCourse.AddedBy);
            Assert.AreEqual(dtoPlannedCourse.AddedOn, plannedCourse.AddedOn);
            Assert.AreEqual(dtoPlannedCourse.CourseId, plannedCourse.CourseId);
            Assert.AreEqual(dtoPlannedCourse.Credits, plannedCourse.Credits);
            Assert.AreEqual(dtoPlannedCourse.GradingType.ToString(), plannedCourse.GradingType.ToString());
            Assert.AreEqual(dtoPlannedCourse.SectionId, plannedCourse.SectionId);
            Assert.AreEqual(dtoPlannedCourse.SectionWaitlistStatus.ToString(), plannedCourse.WaitlistedStatus.ToString());
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PlannedCourseProperties()
        {
            var dtoPlannedCourses = degreePlanDto.Terms.Where(t => t.TermId == "2012/FA").SelectMany(t => t.PlannedCourses);
            var dtoPlannedCourse = dtoPlannedCourses.Where(c => c.CourseId == "444").First();
            var courses = degreePlanEntity.GetPlannedCourses("2012/FA");
            var plannedCourse = courses.Where(c => c.CourseId == "444").First();
            Assert.AreEqual(dtoPlannedCourse.CourseId, plannedCourse.CourseId);
            Assert.AreEqual(dtoPlannedCourse.Credits, plannedCourse.Credits);
            Assert.AreEqual(dtoPlannedCourse.GradingType.ToString(), plannedCourse.GradingType.ToString());
            Assert.AreEqual(dtoPlannedCourse.SectionId, plannedCourse.SectionId);
            Assert.AreEqual(dtoPlannedCourse.SectionWaitlistStatus.ToString(), plannedCourse.WaitlistedStatus.ToString());
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_PlannedCourseMessagesNotConverted()
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
        public void DegreePlanDto3Adapter_Approvals()
        {
            Assert.AreEqual(2, degreePlanEntity.Approvals.Count());
            var approval = degreePlanEntity.Approvals.ElementAt(0);
            Assert.AreEqual(date1, approval.Date);
            Assert.AreEqual(personId, approval.PersonId);
            Assert.AreEqual(Domain.Student.Entities.DegreePlans.DegreePlanApprovalStatus.Denied, approval.Status);
            Assert.AreEqual("111", approval.CourseId);
        }

        [TestMethod]
        public void DegreePlanDto3Adapter_Notes()
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
        public void DegreePlanDto3Adapter_GradingType()
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
        public void DegreePlanDto3Adapter_WaitlistStatus()
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
