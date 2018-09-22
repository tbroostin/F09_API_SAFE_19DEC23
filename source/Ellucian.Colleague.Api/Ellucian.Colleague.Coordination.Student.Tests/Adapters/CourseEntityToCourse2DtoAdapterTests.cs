// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class CourseEntityToCourse2DtoAdapterTests
    {
        private Ellucian.Colleague.Domain.Student.Entities.Course course;
        private Ellucian.Colleague.Dtos.Student.Course2 course2Dto;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            var adapter = new CourseEntityToCourse2DtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            var meetingAdapter = new SectionMeetingEntityToSectionMeetingDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>()).Returns(meetingAdapter);

            course = new Course("1", "Title", "LongTitle", new List<OfferingDepartment>() { new OfferingDepartment("MATH", 50m) }, "MATH", "100", "UG", new List<string>() { "Level1" }, 3, null, new List<CourseApproval>() { new CourseApproval("A", DateTime.Today.AddDays(-1), "Agency", "0004032", DateTime.Today.AddDays(-1)) });
            course.AddInstructionalMethodCode("LEC");
            course.StartDate = new DateTime(2014, 01, 01);
            course.EndDate = new DateTime(2099, 05, 15);
            course.AddEquatedCourseId("139");
            course.AddLocationCycleRestriction(new LocationCycleRestriction("LOC1", "F", "EY"));
            course.AddLocationCycleRestriction(new LocationCycleRestriction("LOC2", "S", "OY"));
            course.AllowAudit = true;
            course.AllowPassNoPass = true;
            course.MaximumCredits = 5;
            course.AllowWaitlist = true;
            course.VariableCreditIncrement = 2;
            course.AddType("CourseType1");
            course.AddType("CourseType2");
            var requisite = new Requisite("1234", true, RequisiteCompletionOrder.Concurrent, true);
            course.Requisites.Add(requisite);
            course.Description = "Course Description";
            course.FederalCourseClassification = "FedCode";
            course.GradeSchemeCode = "UG";
            course.Guid = "Guid";
            course.IsInstructorConsentRequired = true;
            course.IsPseudoCourse = true;
            course.LocalCourseClassifications = new List<string>() {"Classification"};
            course.LocalCreditType = "LocalCreditType";
            course.LocationCodes.Add("MAIN");
            course.OnlyPassNoPass = false;
            course.TermSessionCycle = "SessionCycle";
            course.TermYearlyCycle = "YearlyCycle";
            course.TopicCode = "Topic";
            course.WaitlistRatingCode = "Rule";
            course2Dto = adapter.MapToType(course);
        }

        [TestMethod]
        public void CourseEntityToCourse2DtoAdapterMapToTypeTest()
        {
            Assert.AreEqual(course.Id, course2Dto.Id);
            Assert.AreEqual(course.SubjectCode, course2Dto.SubjectCode);
            Assert.AreEqual(course.Number, course2Dto.Number);
            Assert.AreEqual(course.MinimumCredits, course2Dto.MinimumCredits);
            Assert.AreEqual(course.MaximumCredits, course2Dto.MaximumCredits);
            Assert.AreEqual(course.VariableCreditIncrement, course2Dto.VariableCreditIncrement);
            Assert.AreEqual(course.Ceus, course2Dto.Ceus);
            Assert.AreEqual(course.VariableCreditIncrement, course2Dto.VariableCreditIncrement);
            Assert.AreEqual(course.Title, course2Dto.Title);
            Assert.AreEqual(course.Description, course2Dto.Description);
            Assert.AreEqual(course.TermSessionCycle, course2Dto.TermSessionCycle);
            Assert.AreEqual(course.TermYearlyCycle, course2Dto.TermYearlyCycle);
            Assert.AreEqual(course.YearsOffered, course2Dto.YearsOffered);
            Assert.AreEqual(course.LocationCodes.Count(), course2Dto.LocationCodes.Count());
            Assert.AreEqual(course.IsPseudoCourse, course2Dto.IsPseudoCourse);
            Assert.AreEqual(course.EquatedCourseIds.Count(), course2Dto.EquatedCourseIds.Count());
            Assert.AreEqual(course.Requisites.Count(), course2Dto.Requisites.Count());
            var sourceReq = course.Requisites.FirstOrDefault();
            var targetReq = course2Dto.Requisites.FirstOrDefault();
            Assert.AreEqual((int)sourceReq.CompletionOrder, (int)targetReq.CompletionOrder);
            Assert.AreEqual(sourceReq.CorequisiteCourseId, targetReq.CorequisiteCourseId);
            Assert.AreEqual(sourceReq.IsProtected, targetReq.IsProtected);
            Assert.AreEqual(sourceReq.IsRequired, targetReq.IsRequired);
            Assert.AreEqual(sourceReq.RequirementCode, targetReq.RequirementCode);
            Assert.AreEqual(course.LocationCycleRestrictions.Count(), course2Dto.LocationCycleRestrictions.Count());
            var sourceLocCycle = course.LocationCycleRestrictions.FirstOrDefault();
            var targetLocCycle = course2Dto.LocationCycleRestrictions.FirstOrDefault();
            Assert.AreEqual(sourceLocCycle.Location, targetLocCycle.Location);
            Assert.AreEqual(sourceLocCycle.SessionCycle, targetLocCycle.SessionCycle);
            Assert.AreEqual(sourceLocCycle.YearlyCycle, targetLocCycle.YearlyCycle);
        }
    }
}
