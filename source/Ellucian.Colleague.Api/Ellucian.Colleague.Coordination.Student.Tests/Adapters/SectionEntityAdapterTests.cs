// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
    public class SectionEntityAdapterTests
    {
        private Ellucian.Colleague.Domain.Student.Entities.Section section;
        private Ellucian.Colleague.Dtos.Student.Section sectionDto;

        [TestInitialize]
        public void Initialize()
        {
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            var adapter = new SectionEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Section, Ellucian.Colleague.Dtos.Student.Section>()).Returns(adapter);

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
                        
            section.Requisites = new List<Requisite>() 
            {
                new Requisite("req1", true, RequisiteCompletionOrder.Concurrent, false),   // requirement--will not be converted
                new Requisite("12", true),                                          // course requisite
                new Requisite("13", false),                                         // course requisite
                new Requisite("req2", false, RequisiteCompletionOrder.Previous, false),    // requirement--will not be converted
            };
            section.SectionRequisites = new List<SectionRequisite>() 
            {
                new SectionRequisite("2"),
                new SectionRequisite("3"),
                new SectionRequisite("4"),                     // recommended section requisites
                new SectionRequisite(new List<string>(){"5","6","7","8"}, 3),              // required section requisites
            };
            sectionDto = adapter.MapToType(section);
        }

        [TestMethod]
        public void SectionEntityAdapter_BaseSection()
        {
             Assert.AreEqual(section.ActiveStudentIds, sectionDto.ActiveStudentIds);
            Assert.AreEqual(section.AllowAudit, sectionDto.AllowAudit);
            Assert.AreEqual(section.AllowPassNoPass, sectionDto.AllowPassNoPass);
            Assert.AreEqual(section.Available, sectionDto.Available);
            Assert.AreEqual(section.Books.Count(), sectionDto.Books.Count());
            Assert.AreEqual(section.Capacity, sectionDto.Capacity);
            Assert.AreEqual(section.Ceus, sectionDto.Ceus);
            Assert.AreEqual(section.CourseId, sectionDto.CourseId);
            Assert.AreEqual(section.EndDate, sectionDto.EndDate);
            Assert.AreEqual(section.FacultyIds.Count(), sectionDto.FacultyIds.Count());
            Assert.AreEqual(section.Id, sectionDto.Id);
            Assert.AreEqual(section.IsActive, sectionDto.IsActive);
            Assert.AreEqual(section.LearningProvider, sectionDto.LearningProvider);
            Assert.AreEqual(section.LearningProviderSiteId, sectionDto.LearningProviderSiteId);
            Assert.AreEqual(section.Location, sectionDto.Location);
            Assert.AreEqual(section.MaximumCredits, sectionDto.MaximumCredits);
            Assert.AreEqual(section.Meetings.Count(), sectionDto.Meetings.Count());
            Assert.AreEqual(section.MinimumCredits, sectionDto.MinimumCredits);
            Assert.AreEqual(section.Number, sectionDto.Number);
            Assert.AreEqual(section.OnlyPassNoPass, sectionDto.OnlyPassNoPass);
            Assert.AreEqual(section.PrimarySectionId, sectionDto.PrimarySectionId);
            Assert.AreEqual(section.StartDate, sectionDto.StartDate);
            Assert.AreEqual(section.TermId, sectionDto.TermId);
            Assert.AreEqual(section.Title, sectionDto.Title);
            Assert.AreEqual(section.VariableCreditIncrement, sectionDto.VariableCreditIncrement);
            Assert.AreEqual(section.WaitlistAvailable, sectionDto.WaitlistAvailable);
            Assert.AreEqual(section.Waitlisted, sectionDto.Waitlisted);
        }

        [TestMethod]
        public void SectionCourseCorequistes()
        {
            Assert.AreEqual(2, sectionDto.CourseCorequisites.Count());
            var coreq = sectionDto.CourseCorequisites.Where(c => c.Id == "12").First();
            Assert.AreEqual(true, coreq.Required);
            coreq = sectionDto.CourseCorequisites.Where(c => c.Id == "13").First();
            Assert.AreEqual(false, coreq.Required);
        }

        [TestMethod]
        public void SectionSectionCorequisites()
        {
            Assert.AreEqual(7, sectionDto.SectionCorequisites.Count());
            // Spot-checking 4 of the 7
            var coreq = sectionDto.SectionCorequisites.Where(c => c.Id == "2").First();
            Assert.AreEqual(false, coreq.Required);
            coreq = sectionDto.SectionCorequisites.Where(c => c.Id == "4").First();
            Assert.AreEqual(false, coreq.Required);
            coreq = sectionDto.SectionCorequisites.Where(c => c.Id == "5").First();
            Assert.AreEqual(true, coreq.Required);
            coreq = sectionDto.SectionCorequisites.Where(c => c.Id == "8").First();
            Assert.AreEqual(true, coreq.Required);
        }
    }
}
