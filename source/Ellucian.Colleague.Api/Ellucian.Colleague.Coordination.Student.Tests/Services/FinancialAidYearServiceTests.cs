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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FinancialAidYearServiceTests
    {
        [TestClass]
        public class GetFinancialAidYears
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
            private FinancialAidYearService financialAidYearService;
            private ICollection<Domain.Student.Entities.FinancialAidYear> financialAidYearCollection = new List<Domain.Student.Entities.FinancialAidYear>();
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


                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2001", "CODE1", "STATUS1") { HostCountry = "USA" });
                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2002", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" });
                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2003", "CODE3", "STATUS3") { HostCountry = "USA" });
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(true)).ReturnsAsync(financialAidYearCollection);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(false)).ReturnsAsync(financialAidYearCollection);

                financialAidYearService = new FinancialAidYearService(adapterRegistry, referenceRepository, baseConfigurationRepository, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidYearCollection = null;
                referenceRepository = null;
                financialAidYearService = null;
            }

            [TestMethod]
            public async Task FinancialAidYearService__FinancialAidYears()
            {
                var results = await financialAidYearService.GetFinancialAidYearsAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.FinancialAidYear>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task FinancialAidYearService_FinancialAidYears_Count()
            {
                var results = await financialAidYearService.GetFinancialAidYearsAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task FinancialAidYearService_FinancialAidYears_Properties()
            {
                var results = await financialAidYearService.GetFinancialAidYearsAsync();
                var financialAidYear = results.Where(x => x.Code == "2001").FirstOrDefault();
                Assert.IsNotNull(financialAidYear.Id);
                Assert.IsNotNull(financialAidYear.Code);
            }

            [TestMethod]
            public async Task FinancialAidYearService_FinancialAidYears_Expected()
            {
                var expectedResults = financialAidYearCollection.Where(c => c.Code == "2002").FirstOrDefault();
                var results = await financialAidYearService.GetFinancialAidYearsAsync();
                var financialAidYear = results.Where(s => s.Code == "2002").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, financialAidYear.Id);
                Assert.AreEqual(expectedResults.Code, financialAidYear.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidYearService_GetFinancialAidYeardByGuid_Empty()
            {
                await financialAidYearService.GetFinancialAidYearByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidYearService_GetFinancialAidYearByGuid_Null()
            {
                await financialAidYearService.GetFinancialAidYearByGuidAsync(null);
            }

            [TestMethod]
            public async Task FinancialAidYearService_GetFinancialAidYearByGuid_Expected()
            {
                var expectedResults = financialAidYearCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidYear = await financialAidYearService.GetFinancialAidYearByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, financialAidYear.Id);
                Assert.AreEqual(expectedResults.Code, financialAidYear.Code);
            }

            [TestMethod]
            public async Task FinancialAidYearService_GetFinancialAidYearByGuid_Properties()
            {
                var expectedResults = financialAidYearCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidYear = await financialAidYearService.GetFinancialAidYearByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(financialAidYear.Id);
                Assert.IsNotNull(financialAidYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task FinancialAidYearService_GetFinancialAidYeardByGuid_InvalidYear()
            {
                var source = new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "InvalidYear", "CODE1", "STATUS1") { HostCountry = "USA" };
                List<Domain.Student.Entities.FinancialAidYear> collection = new List<Domain.Student.Entities.FinancialAidYear>() { source };
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(true)).ReturnsAsync(collection);

                await financialAidYearService.GetFinancialAidYearByGuidAsync("9C3B805D-CFE6-483B-86C3-4C20562F8C15");
            }
        }
    }
}
