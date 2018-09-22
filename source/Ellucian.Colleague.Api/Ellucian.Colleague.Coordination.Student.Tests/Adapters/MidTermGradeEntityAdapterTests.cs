// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    [TestClass]
    public class MidTermGradeEntityAdapterTests
    {
        [TestMethod]
        public void MidTermGradeEntityAdapter_MapToTypeTest()
        {
            // Arrange
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var loggerMock = new Mock<ILogger>();
            var mapperAdapterMock = new Mock<ITypeAdapter>();

            
            // Auto adapter used to map most properties
            var autoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg=>reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.MidTermGrade, Ellucian.Colleague.Dtos.Student.MidTermGrade>()).Returns(autoAdapter);
            
            var midTermGradeEntity = new Domain.Student.Entities.MidTermGrade(1,"12", DateTimeOffset.Now);

            // Custom adapter invokes automapper adapter and overrides datetime
            var midTermGradeAdapter = new MidTermGradeEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            // Act
            Ellucian.Colleague.Dtos.Student.MidTermGrade midTermGradeDto = midTermGradeAdapter.MapToType(midTermGradeEntity);
            
            // Assert
            Assert.AreEqual(midTermGradeEntity.GradeId, midTermGradeDto.GradeId);
            Assert.AreEqual(midTermGradeEntity.Position, midTermGradeDto.Position);
            Assert.AreEqual(midTermGradeEntity.GradeTimestamp.Value.DateTime, midTermGradeDto.GradeTimestamp);
        }
    }
}