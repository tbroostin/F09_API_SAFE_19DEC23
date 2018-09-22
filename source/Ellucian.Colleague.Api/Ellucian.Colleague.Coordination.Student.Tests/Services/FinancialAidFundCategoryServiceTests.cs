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
    public class FinancialAidFundCategoryServiceTests
    {
        [TestClass]
        public class GetFinancialAidFundCategories
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            //private IFinancialAidReferenceDataRepository referenceRepository;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private FinancialAidFundCategoryService financialAidFundCategoryService;
            private ICollection<Domain.Student.Entities.FinancialAidFundCategory> financialAidFundCategoryCollection = new List<Domain.Student.Entities.FinancialAidFundCategory>();
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "DESC1"));
                financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "DESC2"));
                financialAidFundCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "DESC3"));
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundCategoriesAsync(true)).ReturnsAsync(financialAidFundCategoryCollection);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundCategoriesAsync(false)).ReturnsAsync(financialAidFundCategoryCollection);

                financialAidFundCategoryService = new FinancialAidFundCategoryService(adapterRegistry, referenceRepositoryMock.Object, baseConfigurationRepository, 
                    userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                referenceRepositoryMock = null;
                financialAidFundCategoryCollection = null;
                adapterRegistry = null;
                userFactoryMock = null;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = null;
                logger = null;
                baseConfigurationRepository = null;

                financialAidFundCategoryService = null;
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService__FinancialAidFundCategories()
            {
                var results = await financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync();
                Assert.IsTrue(results is IEnumerable<Dtos.FinancialAidFundCategory>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService_FinancialAidFundCategories_Count()
            {
                var results = await financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync();
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService_FinancialAidFundCategories_Properties()
            {
                var results = await financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync();
                var financialAidFundCategory = results.Where(x => x.Code == "CODE1").FirstOrDefault();
                Assert.IsNotNull(financialAidFundCategory.Id);
                Assert.IsNotNull(financialAidFundCategory.Code);
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService_FinancialAidFundCategories_Expected()
            {
                var expectedResults = financialAidFundCategoryCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
                var results = await financialAidFundCategoryService.GetFinancialAidFundCategoriesAsync();
                var financialAidFundCategory = results.Where(s => s.Code == "CODE2").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, financialAidFundCategory.Id);
                Assert.AreEqual(expectedResults.Code, financialAidFundCategory.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidFundCategoryService_GetFinancialAidFundCategoryByGuid_Empty()
            {
                await financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task FinancialAidFundCategoryService_GetFinancialAidFundCategoryByGuid_Null()
            {
                await financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync(null);
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService_GetFinancialAidFundCategoryByGuid_Expected()
            {
                var expectedResults = financialAidFundCategoryCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidFundCategory = await financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, financialAidFundCategory.Id);
                Assert.AreEqual(expectedResults.Code, financialAidFundCategory.Code);
            }

            [TestMethod]
            public async Task FinancialAidFundCategoryService_GetFinancialAidFundCategoryByGuid_Properties()
            {
                var expectedResults = financialAidFundCategoryCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidFundCategory = await financialAidFundCategoryService.GetFinancialAidFundCategoryByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(financialAidFundCategory.Id);
                Assert.IsNotNull(financialAidFundCategory.Code);
            }
        }

    }
}
