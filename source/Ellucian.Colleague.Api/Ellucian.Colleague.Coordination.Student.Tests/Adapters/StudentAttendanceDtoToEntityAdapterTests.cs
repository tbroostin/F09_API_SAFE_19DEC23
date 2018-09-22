// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class StudentAttendanceDtoToEntityAdapterTests
    {
        StudentAttendance dto;
        StudentAttendanceDtoToEntityAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapter = new StudentAttendanceDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            dto = new StudentAttendance()
            {
                AttendanceCategoryCode = "A",
                Comment = "Excused",
                EndTime = DateTime.Now.AddHours(-1),
                StartTime = DateTime.Now.AddHours(-3),
                InstructionalMethod = "LEC",
                MeetingDate = DateTime.Today,
                SectionId = "12345",
                StudentCourseSectionId = "67890",
                StudentId = "0001234"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StudentAttendanceDtoToEntityAdapter_Null_source_throws_Exception()
        {
            var entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void StudentAttendanceDtoToEntityAdapter_Valid_AttendanceCategoryCode()
        {
            var entity = adapter.MapToType(dto);
            Assert.AreEqual(dto.AttendanceCategoryCode, entity.AttendanceCategoryCode);
            Assert.AreEqual(dto.Comment, entity.Comment);
            Assert.AreEqual(dto.EndTime, entity.EndTime);
            Assert.AreEqual(dto.StartTime, entity.StartTime);
            Assert.AreEqual(dto.InstructionalMethod, entity.InstructionalMethod);
            Assert.AreEqual(dto.MeetingDate, entity.MeetingDate);
            Assert.AreEqual(dto.SectionId, entity.SectionId);
            Assert.AreEqual(dto.StudentCourseSectionId, entity.StudentCourseSectionId);
            Assert.AreEqual(dto.StudentId, entity.StudentId);
            Assert.AreEqual(dto.MinutesAttended, entity.MinutesAttended);
        }


        [TestMethod]
        public void StudentAttendanceDtoToEntityAdapter_Valid_MinutesAttended()
        {
            dto.AttendanceCategoryCode = null;
            dto.MinutesAttended = 60;
            var entity = adapter.MapToType(dto);
            Assert.AreEqual(dto.AttendanceCategoryCode, entity.AttendanceCategoryCode);
            Assert.AreEqual(dto.Comment, entity.Comment);
            Assert.AreEqual(dto.EndTime, entity.EndTime);
            Assert.AreEqual(dto.StartTime, entity.StartTime);
            Assert.AreEqual(dto.InstructionalMethod, entity.InstructionalMethod);
            Assert.AreEqual(dto.MeetingDate, entity.MeetingDate);
            Assert.AreEqual(dto.SectionId, entity.SectionId);
            Assert.AreEqual(dto.StudentCourseSectionId, entity.StudentCourseSectionId);
            Assert.AreEqual(dto.StudentId, entity.StudentId);
            Assert.AreEqual(dto.MinutesAttended, entity.MinutesAttended);
        }
    }
}
