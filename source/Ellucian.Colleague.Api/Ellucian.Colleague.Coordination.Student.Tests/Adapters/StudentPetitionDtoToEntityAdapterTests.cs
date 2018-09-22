// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class StudentPetitionDtoToEntityAdapterTests
    {
        StudentPetition studentPetitionDto;
        StudentPetitionDtoToEntityAdapter studentPetitionAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            studentPetitionAdapter = new StudentPetitionDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            DateTimeOffset dateTimeChanged = new DateTimeOffset(2014, 2, 11, 3, 10, 15, new TimeSpan(-3, 0, 0));
            studentPetitionDto = new StudentPetition()
            {
                Id = "2",
                StudentId = "0000002",
                SectionId = "SEC1",
                ReasonCode = "REASON",
                Comment = "Multiline\nComment\r\nBy User",
                DateTimeChanged = dateTimeChanged,
                StatusCode = "A",
                UpdatedBy = "CRS", 
                Type = StudentPetitionType.FacultyConsent
            };
        }

        [TestMethod]
        public void StudentPetition_Id()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.Id, petitionEntity.Id);
        }

        [TestMethod]
        public void StudentPetition_StudentId()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.StudentId, petitionEntity.StudentId);
        }

        [TestMethod]
        public void StudentPetition_SectionId()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.SectionId, petitionEntity.SectionId);
        }

        [TestMethod]
        public void StudentPetition_ReasonCode()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.ReasonCode, petitionEntity.ReasonCode);
        }

        [TestMethod]
        public void StudentPetition_StatusCode()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.StatusCode, petitionEntity.StatusCode);
        }

        [TestMethod]
        public void StudentPetition_UpdatedBy()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.UpdatedBy, petitionEntity.UpdatedBy);
        }

        [TestMethod]
        public void StudentPetition_DateTimeChanged()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(studentPetitionDto.DateTimeChanged, petitionEntity.DateTimeChanged);
        }
        [TestMethod]
        public void StudentPetition_Type()
        {
            var petitionEntity = studentPetitionAdapter.MapToType(studentPetitionDto);
            Assert.AreEqual(Domain.Student.Entities.StudentPetitionType.FacultyConsent, petitionEntity.Type);
        }
    }
}
