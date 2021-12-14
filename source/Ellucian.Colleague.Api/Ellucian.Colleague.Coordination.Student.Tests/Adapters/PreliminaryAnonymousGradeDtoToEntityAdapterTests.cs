// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Tests.Adapters
{
    [TestClass]
    public class PreliminaryAnonymousGradeDtoToEntityAdapterTests
    {
        Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade source;
        PreliminaryAnonymousGradeDtoToEntityAdapter adapter;
        Mock<IAdapterRegistry> adapterRegistryMock;
        IAdapterRegistry adapterRegistry;
        Mock<ILogger> loggerMock;
        ILogger logger;

        [TestInitialize]
        public void PreliminaryAnonymousGradeDtoToEntityAdapterTests_Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            source = new Dtos.Student.AnonymousGrading.PreliminaryAnonymousGrade()
            {
                AnonymousGradingId = "12345",
                CourseSectionId = "23456",
                StudentCourseSectionId = "34567",
                FinalGradeExpirationDate = DateTime.Today.AddDays(30),
                FinalGradeId = "1"
            };

            adapter = new PreliminaryAnonymousGradeDtoToEntityAdapter(adapterRegistry, logger);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PreliminaryAnonymousGradeDtoToEntityAdapter_null_source_throws_ArgumentNullException()
        {
            var entity = adapter.MapToType(null);
        }

        [TestMethod]
        public void PreliminaryAnonymousGradeDtoToEntityAdapter_valid()
        {
            var entity = adapter.MapToType(source);
            Assert.AreEqual(source.AnonymousGradingId, entity.AnonymousGradingId);
            Assert.AreEqual(source.CourseSectionId, entity.CourseSectionId);
            Assert.AreEqual(source.FinalGradeExpirationDate, entity.FinalGradeExpirationDate);
            Assert.AreEqual(source.FinalGradeId, entity.FinalGradeId);
            Assert.AreEqual(source.StudentCourseSectionId, entity.StudentCourseSectionId);
        }
    }
}
