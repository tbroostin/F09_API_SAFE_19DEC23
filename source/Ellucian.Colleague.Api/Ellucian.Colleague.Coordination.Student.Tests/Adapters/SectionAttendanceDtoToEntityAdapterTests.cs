// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student;
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
    public class SectionAttendanceDtoToEntityAdapterTests
    {
        SectionAttendance sectionAttendanceDto;
        SectionMeetingInstance meetingInstance;
        StudentSectionAttendance studentSectionAttendance;
        StudentSectionAttendance studentSectionAttendanceMinutes;
        List<StudentSectionAttendance> expectedSsas;
        SectionAttendanceDtoToEntityAdapter sectionAttendanceAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            sectionAttendanceAdapter = new SectionAttendanceDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            meetingInstance = new SectionMeetingInstance()
            {
                MeetingDate = DateTime.Today,
                SectionId = "SectionId",
                StartTime = DateTime.Now.AddDays(-10),
                EndTime = DateTime.Now,
                InstructionalMethod = "IN"
            };
            studentSectionAttendance = new StudentSectionAttendance()
            {
                StudentCourseSectionId = "StudentCourseSecId",
                AttendanceCategoryCode = "L",
                Comment = "Comment"
            };
            studentSectionAttendanceMinutes = new StudentSectionAttendance()
            {
                StudentCourseSectionId = "StudentCourseSecId2",
                AttendanceCategoryCode = null,
                MinutesAttended = 60
 
            };
            sectionAttendanceDto = new SectionAttendance()
            {
                SectionId = "SectionId",
                MeetingInstance = meetingInstance
            };
            expectedSsas = new List<StudentSectionAttendance>() { studentSectionAttendance, studentSectionAttendanceMinutes };
            sectionAttendanceDto.StudentAttendances = expectedSsas;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SectionAttendance_NullSource()
        {
            var sectionAttendanceEntity = sectionAttendanceAdapter.MapToType(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SectionAttendance_NullSourceMeetingInstance()
        {
            sectionAttendanceDto.MeetingInstance = null;
            var sectionAttendanceEntity = sectionAttendanceAdapter.MapToType(sectionAttendanceDto);
        }

        [TestMethod]
        public void SectionAttendance_SectionId()
        {
            var sectionAttendanceEntity = sectionAttendanceAdapter.MapToType(sectionAttendanceDto);
            Assert.AreEqual(sectionAttendanceDto.SectionId, sectionAttendanceEntity.SectionId);
        }

        [TestMethod]
        public void SectionAttendance_MeetingInstance()
        {
            var sectionAttendanceEntity = sectionAttendanceAdapter.MapToType(sectionAttendanceDto);

            Assert.IsInstanceOfType(sectionAttendanceEntity.MeetingInstance, typeof(Domain.Student.Entities.SectionMeetingInstance));

            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.Id, sectionAttendanceEntity.MeetingInstance.Id);
            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.InstructionalMethod, sectionAttendanceEntity.MeetingInstance.InstructionalMethod);
            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.MeetingDate, sectionAttendanceEntity.MeetingInstance.MeetingDate);
            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.SectionId, sectionAttendanceEntity.MeetingInstance.SectionId);
            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.StartTime, sectionAttendanceEntity.MeetingInstance.StartTime);
            Assert.AreEqual(sectionAttendanceDto.MeetingInstance.EndTime, sectionAttendanceEntity.MeetingInstance.EndTime);
            
        }

        [TestMethod]
        public void SectionAttendance_StudentSectionAttendance()
        {
            var sectionAttendanceEntity = sectionAttendanceAdapter.MapToType(sectionAttendanceDto);
            Assert.AreEqual(2, sectionAttendanceEntity.StudentAttendances.Count);
            foreach (var item in sectionAttendanceEntity.StudentAttendances)
            {
                Assert.IsInstanceOfType(item, typeof(Domain.Student.Entities.StudentSectionAttendance));
                var expectedSsa = expectedSsas.Where(scs => scs.StudentCourseSectionId == item.StudentCourseSectionId).FirstOrDefault();
                Assert.AreEqual(item.StudentCourseSectionId, expectedSsa.StudentCourseSectionId);
                Assert.AreEqual(item.AttendanceCategoryCode, expectedSsa.AttendanceCategoryCode);
                Assert.AreEqual(item.MinutesAttended, expectedSsa.MinutesAttended);
                Assert.AreEqual(item.Comment, expectedSsa.Comment);
            }
            

        }
    }
}
