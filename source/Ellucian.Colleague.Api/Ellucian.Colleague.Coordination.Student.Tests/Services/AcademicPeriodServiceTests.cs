// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos;
using System.Threading.Tasks;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Student.Tests;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AcademicPeriodServiceTests
    {

        [TestClass]
        public class GetAcademicPeriods
        {
            private Mock<IAcademicPeriodRepository> academicRepositoryMock;
            private IAcademicPeriodRepository academicRepository;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private ILogger logger;
            private AcademicPeriodService academicPeriodService;
            private ICollection<Domain.Student.Entities.AcademicPeriod> academicPeriodCollection = new List<Domain.Student.Entities.AcademicPeriod>();
            private ICollection<Domain.Student.Entities.Term> termCollection = new List<Domain.Student.Entities.Term>();

            [TestInitialize]
            public void Initialize()
            {
                academicRepositoryMock = new Mock<IAcademicPeriodRepository>();
                academicRepository = academicRepositoryMock.Object;
                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;

                academicPeriodCollection = new TestAcademicPeriodRepository().Get() as List<Domain.Student.Entities.AcademicPeriod>;
                termCollection = new TestTermRepository().GetAsync().Result as List<Domain.Student.Entities.Term>;

                academicRepositoryMock.Setup(repo => repo.GetAcademicPeriods(termCollection)).Returns(academicPeriodCollection);
                termRepositoryMock.Setup(repo => repo.GetAsync()).ReturnsAsync(termCollection);
                termRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<bool>())).ReturnsAsync(termCollection);
                //academicRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(true)).ReturnsAsync(academicPeriodCollection);
                //academicRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(false)).ReturnsAsync(academicPeriodCollection);

                academicPeriodService = new AcademicPeriodService(academicRepository, termRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicPeriodCollection = null;
                academicRepository = null;
                academicPeriodService = null;
            }

            [TestMethod]
            public async Task AcademicPeriodService__AcademicPeriods()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync();
                Assert.IsTrue(results is IEnumerable<AcademicPeriod>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AcademicPeriodService_AcademicPeriods_Count()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicPeriodService_AcademicPeriods_Properties()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync();
                var academicPeriod = results.Where(x => x.Abbreviation == "2000RSU").FirstOrDefault();
                Assert.IsNotNull(academicPeriod.Guid);
                Assert.IsNotNull(academicPeriod.Abbreviation);
            }

            [TestMethod]
            public async Task AcademicPeriodService_AcademicPeriods_Expected()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Code == "2000RSU").FirstOrDefault();
                var results = await academicPeriodService.GetAcademicPeriodsAsync();
                var academicPeriod = results.Where(s => s.Abbreviation == "2000RSU").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, academicPeriod.Guid);
                Assert.AreEqual(expectedResults.Code, academicPeriod.Abbreviation);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicPeriodService_GetAcademicPeriodByGuid_Empty()
            {
                await academicPeriodService.GetAcademicPeriodByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicPeriodService_GetAcademicPeriodByGuid_Null()
            {
                await academicPeriodService.GetAcademicPeriodByGuidAsync(null);
            }

            [TestMethod]
            public async Task AcademicPeriodService_GetAcademicPeriodByGuid_Expected()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Guid == "d1ef94c1-759c-4870-a3f4-34065bb522fe").FirstOrDefault();
                var academicLevel = await academicPeriodService.GetAcademicPeriodByGuidAsync("d1ef94c1-759c-4870-a3f4-34065bb522fe");
                Assert.AreEqual(expectedResults.Guid, academicLevel.Guid);
                Assert.AreEqual(expectedResults.Code, academicLevel.Abbreviation);
            }

            [TestMethod]
            public async Task AcademicPeriodService_GetAcademicPeriodByGuid_Properties()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Guid == "d1ef94c1-759c-4870-a3f4-34065bb522fe").FirstOrDefault();
                var academicPeriod = await academicPeriodService.GetAcademicPeriodByGuidAsync("d1ef94c1-759c-4870-a3f4-34065bb522fe");
                Assert.IsNotNull(academicPeriod.Guid);
                Assert.IsNotNull(academicPeriod.Abbreviation);
            }

            [TestMethod]
            public async Task AcademicPeriodService__AcademicPeriods2()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync2(false);
                Assert.IsTrue(results is IEnumerable<AcademicPeriod2>);
                Assert.IsNotNull(results);
            }

            public async Task AcademicPeriodService_AcademicPeriods2_Count()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync2(false);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AcademicPeriodService_AcademicPeriods2_Properties()
            {
                var results = await academicPeriodService.GetAcademicPeriodsAsync2(true);
                var academicLevel = results.Where(x => x.Code == "2000RSU").FirstOrDefault();
                Assert.IsNotNull(academicLevel.Id);
                Assert.IsNotNull(academicLevel.Code);
            }

            [TestMethod]
            public async Task AcademicPeriodService_AcademicPeriods2_Expected()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Code == "2000RSU").FirstOrDefault();
                var results = await academicPeriodService.GetAcademicPeriodsAsync2(true);
                var academicPeriod = results.Where(s => s.Code == "2000RSU").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, academicPeriod.Id);
                Assert.AreEqual(expectedResults.Code, academicPeriod.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicPeriodService_GetAcademicPeriodById2_Empty()
            {
                await academicPeriodService.GetAcademicPeriodByGuidAsync2("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AcademicPeriodService_GetAcademicPeriodById2_Null()
            {
                await academicPeriodService.GetAcademicPeriodByGuidAsync2(null);
            }

            [TestMethod]
            public async Task AcademicPeriodService_GetAcademicPeriodById2_Expected()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Guid == "d1ef94c1-759c-4870-a3f4-34065bb522fe").FirstOrDefault();
                var academicPeriod = await academicPeriodService.GetAcademicPeriodByGuidAsync2("d1ef94c1-759c-4870-a3f4-34065bb522fe");
                Assert.AreEqual(expectedResults.Guid, academicPeriod.Id);
                Assert.AreEqual(expectedResults.Code, academicPeriod.Code);
            }

            [TestMethod]
            public async Task AcademicPeriodService_GetAcademicPeriodById2_Properties()
            {
                var expectedResults = academicPeriodCollection.Where(c => c.Guid == "d1ef94c1-759c-4870-a3f4-34065bb522fe").FirstOrDefault();
                var academicPeriod = await academicPeriodService.GetAcademicPeriodByGuidAsync2("d1ef94c1-759c-4870-a3f4-34065bb522fe");
                Assert.IsNotNull(academicPeriod.Id);
                Assert.IsNotNull(academicPeriod.Code);
            }
        }
    }
}
