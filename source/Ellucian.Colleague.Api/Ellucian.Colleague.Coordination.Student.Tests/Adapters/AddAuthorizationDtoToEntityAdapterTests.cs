// Copyright 2018 Ellucian Company L.P. and its affiliates.
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
    public class AddAuthorizationDtoToEntityAdapterTests
    {
        AddAuthorization dto;
        AddAuthorizationDtoToEntityAdapter adapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapter = new AddAuthorizationDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            dto = new AddAuthorization()
            {
                Id = "AuthId",
                RevokedBy = "FacultyId",
                RevokedTime = DateTime.Now.AddHours(-1),
                AssignedTime = DateTime.Now.AddHours(-3),
                SectionId = "12345",
                StudentId = "0001234",
                AssignedBy = "0001234",
                AddAuthorizationCode = "AbCd1234"
            };
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddAuthorizationDtoToEntityAdapter_Null_source_throws_Exception()
        {
            var entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void AddAuthorizationDtoToEntityAdapter_Valid()
        {
            var entity = adapter.MapToType(dto);
            Assert.AreEqual(dto.Id, entity.Id);
            Assert.AreEqual(dto.RevokedBy, entity.RevokedBy);
            Assert.AreEqual(dto.RevokedTime, entity.RevokedTime);
            Assert.AreEqual(dto.AssignedTime, entity.AssignedTime);
            Assert.AreEqual(dto.SectionId, entity.SectionId);
            Assert.AreEqual(dto.StudentId, entity.StudentId);
            Assert.AreEqual(dto.AssignedBy, entity.AssignedBy);
            Assert.AreEqual(dto.AddAuthorizationCode, entity.AddAuthorizationCode);

        }
    }
}
