// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;
using Moq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using slf4net;
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class SectionEntityToSection2DtoAdapterTest
    {
        private Ellucian.Colleague.Domain.Student.Entities.Section section;
        private Ellucian.Colleague.Dtos.Student.Section2 section2Dto;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            var adapter = new SectionEntityToSection2DtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            var meetingAdapter = new SectionMeetingEntityToSectionMeetingDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, Ellucian.Colleague.Dtos.Student.SectionMeeting>()).Returns(meetingAdapter);

            section = new Section("1", "12", "01", new DateTime(2012, 01, 15), 3, 0, "Course1", "IN", new List<OfferingDepartment>() { new OfferingDepartment("HIST", 50m), new OfferingDepartment("ENGL", 50m) }, new List<string>() { "100", "200" }, "UG", new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) }, true, true, true, true, true);
            section.AddActiveStudent("001");
            section.EndDate = new DateTime(2012, 05, 15);
            section.AddFaculty("002");
            section.LearningProvider = "ELLUCIAN";
            section.LearningProviderSiteId = "www.ellucian.com";
            section.Location = "MAIN";
            section.MaximumCredits = 5;
            section.PrimarySectionId = "4";
            section.VariableCreditIncrement = 2;
            section.GlobalCapacity = 10; 
            section.GlobalWaitlistMaximum = 12;
            section.PermittedToRegisterOnWaitlist = 5;
            section.ReservedSeats = 3;
            section.SectionCapacity = 15;

            var secMeeting = new SectionMeeting("11", "1", "LEC", new DateTime(12, 8, 23), new DateTime(12,12,12), "W");
            secMeeting.StartTime = new DateTimeOffset(new DateTime(1, 1, 1, 10, 0, 0));
            secMeeting.EndTime = new DateTimeOffset(new DateTime(1, 1, 1, 12, 0, 0));
            secMeeting.Room = "A201";
            secMeeting.IsOnline = false;
            secMeeting.Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday };
            section.AddSectionMeeting(secMeeting);

            var requisite = new Requisite("1234", true, RequisiteCompletionOrder.Concurrent, true);
            section.Requisites.Add(requisite);

            var secRequisite = new SectionRequisite(new List<string>() { "id1", "id2" }, 1);
            section.SectionRequisites.Add(secRequisite);

            section2Dto = adapter.MapToType(section);
        }

        [TestMethod]
        public void SectionEntityToSection2DtoAdapterMapToTypeTest()
        {
            CollectionAssert.AreEqual(section.ActiveStudentIds, section2Dto.ActiveStudentIds.ToList<string>());
            Assert.AreEqual(section.AllowAudit, section2Dto.AllowAudit);
            Assert.AreEqual(section.AllowPassNoPass, section2Dto.AllowPassNoPass);
            Assert.AreEqual(section.Available, section2Dto.Available);
            Assert.AreEqual(section.Books.Count(), section2Dto.Books.Count());
            Assert.AreEqual(section.Capacity, section2Dto.Capacity);
            Assert.AreEqual(section.Ceus, section2Dto.Ceus);
            Assert.AreEqual(section.CourseId, section2Dto.CourseId);
            Assert.AreEqual(section.EndDate, section2Dto.EndDate);
            Assert.AreEqual(section.FacultyIds.Count(), section2Dto.FacultyIds.Count());
            Assert.AreEqual(section.Id, section2Dto.Id);
            Assert.AreEqual(section.IsActive, section2Dto.IsActive);
            Assert.AreEqual(section.LearningProvider, section2Dto.LearningProvider);
            Assert.AreEqual(section.LearningProviderSiteId, section2Dto.LearningProviderSiteId);
            Assert.AreEqual(section.Location, section2Dto.Location);
            Assert.AreEqual(section.MaximumCredits, section2Dto.MaximumCredits);
                        
            Assert.AreEqual(section.Meetings.Count(), section2Dto.Meetings.Count());
            var sourceMeeting = section.Meetings.FirstOrDefault();
            var targetMeeting = section2Dto.Meetings.FirstOrDefault();
            Assert.AreEqual(sourceMeeting.InstructionalMethodCode, targetMeeting.InstructionalMethodCode);
            Assert.AreEqual(sourceMeeting.Frequency, targetMeeting.Frequency);
            CollectionAssert.AreEqual(sourceMeeting.Days, targetMeeting.Days.ToList<DayOfWeek>());
            Assert.AreEqual(sourceMeeting.IsOnline, targetMeeting.IsOnline);
            Assert.AreEqual(sourceMeeting.StartDate, targetMeeting.StartDate);
            Assert.AreEqual(sourceMeeting.EndDate, targetMeeting.EndDate);
            Assert.AreEqual(sourceMeeting.StartTime.Value.DateTime.TimeOfDay.ToString(), targetMeeting.StartTime);
            Assert.AreEqual(sourceMeeting.EndTime.Value.DateTime.TimeOfDay.ToString(), targetMeeting.EndTime);
            
            Assert.AreEqual(section.MinimumCredits, section2Dto.MinimumCredits);
            Assert.AreEqual(section.Number, section2Dto.Number);
            Assert.AreEqual(section.OnlyPassNoPass, section2Dto.OnlyPassNoPass);
            Assert.AreEqual(section.PrimarySectionId, section2Dto.PrimarySectionId);
            Assert.AreEqual(section.StartDate, section2Dto.StartDate);
            Assert.AreEqual(section.TermId, section2Dto.TermId);
            Assert.AreEqual(section.Title, section2Dto.Title);
            Assert.AreEqual(section.VariableCreditIncrement, section2Dto.VariableCreditIncrement);
            Assert.AreEqual(section.WaitlistAvailable, section2Dto.WaitlistAvailable);
            Assert.AreEqual(section.Waitlisted, section2Dto.Waitlisted);
            Assert.AreEqual(section.OverridesCourseRequisites, section2Dto.OverridesCourseRequisites);

            Assert.AreEqual(section.Requisites.Count(), section2Dto.Requisites.Count());
            var sourceReq = section.Requisites.FirstOrDefault();
            var targetReq = section2Dto.Requisites.FirstOrDefault();
            Assert.AreEqual((int)sourceReq.CompletionOrder, (int)targetReq.CompletionOrder);
            Assert.AreEqual(sourceReq.CorequisiteCourseId, targetReq.CorequisiteCourseId);
            Assert.AreEqual(sourceReq.IsProtected, targetReq.IsProtected);
            Assert.AreEqual(sourceReq.IsRequired, targetReq.IsRequired);
            Assert.AreEqual(sourceReq.RequirementCode, targetReq.RequirementCode);

            Assert.AreEqual(section.SectionRequisites.Count(), section2Dto.SectionRequisites.Count());
            var sourceSecReq = section.SectionRequisites.FirstOrDefault();
            var targetSecReq = section2Dto.SectionRequisites.FirstOrDefault();
            CollectionAssert.AreEqual(sourceSecReq.CorequisiteSectionIds, targetSecReq.CorequisiteSectionIds.ToList<string>());
            Assert.AreEqual(sourceSecReq.IsRequired, targetSecReq.IsRequired);
            Assert.AreEqual(sourceSecReq.NumberNeeded, sourceSecReq.NumberNeeded);

        }
    }
}
