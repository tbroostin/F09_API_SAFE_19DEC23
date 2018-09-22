/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/

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

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentFinancialAidOfficeServiceTests
    {           
        [TestClass]
        public class GetFinancialAidOfficesEedm
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IStudentFinancialAidOfficeRepository> studentFinancialAidOfficeRepositoryMock;
            private IStudentFinancialAidOfficeRepository referenceRepository;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private StudentFinancialAidOfficeService financialAidOfficeService;
            private ICollection<Domain.Student.Entities.FinancialAidOfficeItem> financialAidOfficeCollection = new List<Domain.Student.Entities.FinancialAidOfficeItem>();
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                studentFinancialAidOfficeRepositoryMock = new Mock<IStudentFinancialAidOfficeRepository>();
                referenceRepository = studentFinancialAidOfficeRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2001", "CODE1", "NAME1"));
                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("73244057-D1EC-4094-A0B7-DE602533E3A6", "2002", "CODE2", "NAME2"));
                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("1df164eb-8178-4321-a9f7-24f12d3991d8", "2003", "CODE3", "NAME3"));
                studentFinancialAidOfficeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(true)).ReturnsAsync(financialAidOfficeCollection);
                studentFinancialAidOfficeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(false)).ReturnsAsync(financialAidOfficeCollection);

                financialAidOfficeService = new StudentFinancialAidOfficeService(adapterRegistry, referenceRepository, baseConfigurationRepository, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidOfficeCollection = null;
                referenceRepository = null;
                financialAidOfficeService = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService__FinancialAidOffices()
            {
                var results = await financialAidOfficeService.GetFinancialAidOfficesAsync(false);
                Assert.IsTrue(results is IEnumerable<Dtos.FinancialAidOffice>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService_FinancialAidOffices_Count()
            {
                var results = await financialAidOfficeService.GetFinancialAidOfficesAsync(false);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService_FinancialAidOffices_Properties()
            {
                var results = await financialAidOfficeService.GetFinancialAidOfficesAsync(false);
                var financialAidOffice = results.Where(x => x.Code == "2001").FirstOrDefault();
                Assert.IsNotNull(financialAidOffice.Id);
                Assert.IsNotNull(financialAidOffice.Code);
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService_FinancialAidOffices_Expected()
            {
                var expectedResults = financialAidOfficeCollection.Where(c => c.Code == "2002").FirstOrDefault();
                var results = await financialAidOfficeService.GetFinancialAidOfficesAsync(false);
                var financialAidOffice = results.Where(s => s.Code == "2002").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, financialAidOffice.Id);
                Assert.AreEqual(expectedResults.Code, financialAidOffice.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficedByGuid_Empty()
            {
                await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficedByGuid_InvalidOperationException()
            {
                studentFinancialAidOfficeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
                await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficedByGuid_ArgumentNullException()
            {
                studentFinancialAidOfficeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("123");
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficedByGuid_Exception()
            {
                studentFinancialAidOfficeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("123");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficeByGuid_Null()
            {
                await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync(null);
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficeByGuid_Expected()
            {
                var expectedResults = financialAidOfficeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidOffice = await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, financialAidOffice.Id);
                Assert.AreEqual(expectedResults.Code, financialAidOffice.Code);
            }

            [TestMethod]
            public async Task StudentFinancialAidOfficeService_GetFinancialAidOfficeByGuid_Properties()
            {
                var expectedResults = financialAidOfficeCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidOffice = await financialAidOfficeService.GetFinancialAidOfficeByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(financialAidOffice.Id);
                Assert.IsNotNull(financialAidOffice.Code);
            }
        }
    }
}
