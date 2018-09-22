// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FinancialAidAwardPeriodServiceTests
    {
        [TestClass]
        public class GetFinancialAidAwardPeriods
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private FinancialAidAwardPeriodService financialAidAwardPeriodService;
            private ICollection<Domain.Student.Entities.FinancialAidAwardPeriod> financialAidAwardPeriodCollection = new List<Domain.Student.Entities.FinancialAidAwardPeriod>();
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "DESC1", "STATUS1"));
                financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "DESC2", "STATUS2"));
                financialAidAwardPeriodCollection.Add(new Domain.Student.Entities.FinancialAidAwardPeriod("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "DESC3", "STATUS3"));
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidAwardPeriodsAsync(true)).ReturnsAsync(financialAidAwardPeriodCollection);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidAwardPeriodsAsync(false)).ReturnsAsync(financialAidAwardPeriodCollection);

                financialAidAwardPeriodService = new FinancialAidAwardPeriodService(adapterRegistry, referenceRepository, baseConfigurationRepository, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidAwardPeriodCollection = null;
                referenceRepository = null;
                financialAidAwardPeriodService = null;
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService__FinancialAidAwardPeriods()
            {
                var results = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.FinancialAidAwardPeriod>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService_FinancialAidAwardPeriods_Count()
            {
                var results = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService_FinancialAidAwardPeriods_Properties()
            {
                var results = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync();
                var financialAidAwardPeriod = results.Where(x => x.Code == "CODE1").FirstOrDefault();
                Assert.IsNotNull(financialAidAwardPeriod.Id);
                Assert.IsNotNull(financialAidAwardPeriod.Code);
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService_FinancialAidAwardPeriods_Expected()
            {
                var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
                var results = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodsAsync();
                var financialAidAwardPeriod = results.Where(s => s.Code == "CODE2").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, financialAidAwardPeriod.Id);
                Assert.AreEqual(expectedResults.Code, financialAidAwardPeriod.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidAwardPeriodService_GetFinancialAidAwardPerioddByGuid_Empty()
            {
                await financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidAwardPeriodService_GetFinancialAidAwardPeriodByGuid_Null()
            {
                await financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync(null);
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService_GetFinancialAidAwardPeriodByGuid_Expected()
            {
                var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidAwardPeriod = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, financialAidAwardPeriod.Id);
                Assert.AreEqual(expectedResults.Code, financialAidAwardPeriod.Code);
            }

            [TestMethod]
            public async Task FinancialAidAwardPeriodService_GetFinancialAidAwardPeriodByGuid_Properties()
            {
                var expectedResults = financialAidAwardPeriodCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidAwardPeriod = await financialAidAwardPeriodService.GetFinancialAidAwardPeriodByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(financialAidAwardPeriod.Id);
                Assert.IsNotNull(financialAidAwardPeriod.Code);
            }
        }
    }
}
