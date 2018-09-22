// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicStandingsServiceTests
    {
        [TestClass]
        public class GET
        {
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<ILogger> loggerMock;

            AcademicStandingsService academicStandingsService;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding2> academicStandings;

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                loggerMock = new Mock<ILogger>();

                academicStandings = new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false).Result;

                academicStandingsService = new AcademicStandingsService(studentReferenceDataRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicStandingsService = null;
                academicStandings = null;
            }

            [TestMethod]
            public async Task AcademicStandingsService__GetAllAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(academicStandings);

                var results = await academicStandingsService.GetAcademicStandingsAsync(It.IsAny<bool>());
                Assert.AreEqual(academicStandings.ToList().Count, (results.Count()));

                foreach (var academicStanding in academicStandings)
                {
                    var result = results.FirstOrDefault(i => i.Id == academicStanding.Guid);

                    Assert.AreEqual(academicStanding.Code, result.Code);
                    Assert.AreEqual(academicStanding.Description, result.Title);
                    Assert.AreEqual(academicStanding.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task AcademicStandingsService__GetByIdAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(It.IsAny<bool>())).ReturnsAsync(academicStandings);

                string id = "9C3B805D-CFE6-483B-86C3-4C20562F8C15".ToLower();
                var academicStanding = academicStandings.FirstOrDefault(i => i.Guid == id);

                var result = await academicStandingsService.GetAcademicStandingByIdAsync(id);

                Assert.AreEqual(academicStanding.Code, result.Code);
                Assert.AreEqual(academicStanding.Description, result.Title);
                Assert.AreEqual(academicStanding.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicStandingsService__GetByIdAsync_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAcademicStandings2Async(true)).ReturnsAsync(academicStandings);
                var result = await academicStandingsService.GetAcademicStandingByIdAsync("123");
            }
        }
    }
}
