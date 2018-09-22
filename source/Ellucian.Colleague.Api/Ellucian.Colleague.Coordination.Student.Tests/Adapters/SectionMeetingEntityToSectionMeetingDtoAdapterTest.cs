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
    public class SectionMeetingEntityToSectionMeetingDtoAdapterTest
    {
        [TestMethod]
        public void SectionMeetingEntityToSectionMeetingDtoAdapter_MapToTypeTest()
        {
            //Arrange
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            var secMeetingAdapter = new SectionMeetingEntityToSectionMeetingDtoAdapter(
                adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(
                reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionMeeting, 
                    Ellucian.Colleague.Dtos.Student.SectionMeeting>()).Returns(secMeetingAdapter);

            var sourceMeeting = new SectionMeeting("11", "1", "LEC", new DateTime(12, 8, 23), new DateTime(12, 12, 12), "W");
            sourceMeeting.StartTime = new DateTimeOffset(new DateTime(1, 1, 1, 10, 0, 0));
            sourceMeeting.EndTime = new DateTimeOffset(new DateTime(1, 1, 1, 12, 0, 0));
            sourceMeeting.Room = "A201";
            sourceMeeting.IsOnline = false;
            sourceMeeting.Days = new List<DayOfWeek>() { DayOfWeek.Monday, DayOfWeek.Wednesday };
            
            //Act
            Ellucian.Colleague.Dtos.Student.SectionMeeting targetMeeting = secMeetingAdapter.MapToType(sourceMeeting);
            
            //Assert
            Assert.AreEqual(sourceMeeting.InstructionalMethodCode, targetMeeting.InstructionalMethodCode);
            Assert.AreEqual(sourceMeeting.Frequency, targetMeeting.Frequency);
            CollectionAssert.AreEqual(sourceMeeting.Days, targetMeeting.Days.ToList<DayOfWeek>());
            Assert.AreEqual(sourceMeeting.IsOnline, targetMeeting.IsOnline);
            Assert.AreEqual(sourceMeeting.StartDate, targetMeeting.StartDate);
            Assert.AreEqual(sourceMeeting.EndDate, targetMeeting.EndDate);
            Assert.AreEqual(sourceMeeting.StartTime.Value.DateTime.TimeOfDay.ToString(), targetMeeting.StartTime);
            Assert.AreEqual(sourceMeeting.EndTime.Value.DateTime.TimeOfDay.ToString(), targetMeeting.EndTime);   
        }
    }
}
