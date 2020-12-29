// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
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
    public class FinancialAidFundServiceTests
    {
        [TestClass]
        public class GetFinancialAidFunds
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IStudentReferenceDataRepository> referenceRepositoryMock;
            private IStudentReferenceDataRepository referenceRepository;
            private Mock<IFinancialAidFundRepository> fundRepositoryMock;
            private IFinancialAidFundRepository fundRepository;
            private Mock<IStudentFinancialAidOfficeRepository> officeRepositoryMock;
            private IStudentFinancialAidOfficeRepository officeRepository;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<ICurrentUserFactory> userFactoryMock;
            private ICurrentUserFactory userFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private FinancialAidFundsService financialAidFundService;
            private ICollection<Domain.Student.Entities.FinancialAidFund> financialAidFundCollection = new List<Domain.Student.Entities.FinancialAidFund>();
            private ICollection<Domain.Student.Entities.FinancialAidYear> financialAidYearCollection = new List<Domain.Student.Entities.FinancialAidYear>();
            private ICollection<Domain.Student.Entities.FinancialAidOfficeItem> financialAidOfficeCollection = new List<Domain.Student.Entities.FinancialAidOfficeItem>();
            private ICollection<Domain.Student.Entities.FinancialAidFundCategory> financialAidCategoryCollection = new List<Domain.Student.Entities.FinancialAidFundCategory>();
            private ICollection<Domain.Student.Entities.FinancialAidFundClassification> financialAidClassificationCollection = new List<Domain.Student.Entities.FinancialAidFundClassification>();
            private ICollection<Domain.Student.Entities.FinancialAidFundsFinancialProperty> financialAidFundFinancialCollection = new List<Domain.Student.Entities.FinancialAidFundsFinancialProperty>();
            Tuple<IEnumerable<Domain.Student.Entities.FinancialAidFund>, int> financialAidFundsEntityTuple;
            private Dtos.Filters.FinancialAidFundsFilter criteriaFilter = new Dtos.Filters.FinancialAidFundsFilter();

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceRepository = referenceRepositoryMock.Object;
                fundRepositoryMock = new Mock<IFinancialAidFundRepository>();
                fundRepository = fundRepositoryMock.Object;
                officeRepositoryMock = new Mock<IStudentFinancialAidOfficeRepository>();
                officeRepository = officeRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                userFactoryMock = new Mock<ICurrentUserFactory>();
                userFactory = userFactoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                financialAidFundCollection.Add(new Domain.Student.Entities.FinancialAidFund("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "2001") { CategoryCode = "CODE1", FundingType = "CODE2" });
                financialAidFundCollection.Add(new Domain.Student.Entities.FinancialAidFund("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "2002") { CategoryCode = "CODE2", FundingType = "CODE3" });
                financialAidFundCollection.Add(new Domain.Student.Entities.FinancialAidFund("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "2003") { CategoryCode = "CODE3", FundingType = "CODE1" });
                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2001", "DESC1", "STATUS1") { });
                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2002", "DESC2", "STATUS2") { });
                financialAidYearCollection.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2003", "DESC3", "STATUS3") { });
                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "OFFICE1", "DESC1", "NAME1") { });
                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("73244057-D1EC-4094-A0B7-DE602533E3A6", "OFFICE2", "DESC2", "NAME2") { });
                financialAidOfficeCollection.Add(new Domain.Student.Entities.FinancialAidOfficeItem("1df164eb-8178-4321-a9f7-24f12d3991d8", "OFFICE3", "DESC3", "NAME3") { });
                financialAidFundFinancialCollection.Add(new Domain.Student.Entities.FinancialAidFundsFinancialProperty("2001", "OFFICE1", (decimal)10000.00, "CODE1", 60) { });
                financialAidFundFinancialCollection.Add(new Domain.Student.Entities.FinancialAidFundsFinancialProperty("2002", "OFFICE2", (decimal)20000.00, "CODE2", 60) { });
                financialAidFundFinancialCollection.Add(new Domain.Student.Entities.FinancialAidFundsFinancialProperty("2003", "OFFICE3", (decimal)30000.00, "CODE3", 60) { });
                financialAidCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "2001", Domain.Student.Entities.AwardCategoryType.Scholarship, Domain.Student.Entities.FinancialAidFundAidCategoryType.AcademicCompetitivenessGrant, true) { });
                financialAidCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "2002", Domain.Student.Entities.AwardCategoryType.Loan, Domain.Student.Entities.FinancialAidFundAidCategoryType.FederalSubsidizedLoan, false) { });
                financialAidCategoryCollection.Add(new Domain.Student.Entities.FinancialAidFundCategory("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "2003", Domain.Student.Entities.AwardCategoryType.Work, Domain.Student.Entities.FinancialAidFundAidCategoryType.GraduatePlusLoan, true) { });
                financialAidClassificationCollection.Add(new Domain.Student.Entities.FinancialAidFundClassification("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "CODE1", "2001") { });
                financialAidClassificationCollection.Add(new Domain.Student.Entities.FinancialAidFundClassification("73244057-D1EC-4094-A0B7-DE602533E3A6", "CODE2", "2002") { });
                financialAidClassificationCollection.Add(new Domain.Student.Entities.FinancialAidFundClassification("1df164eb-8178-4321-a9f7-24f12d3991d8", "CODE3", "2003") { });
                financialAidFundsEntityTuple = new Tuple<IEnumerable<Domain.Student.Entities.FinancialAidFund>, int>(financialAidFundCollection, financialAidFundCollection.Count());
                fundRepositoryMock.Setup(repo => repo.GetFinancialAidFundsAsync(true)).ReturnsAsync(financialAidFundCollection);
                fundRepositoryMock.Setup(repo => repo.GetFinancialAidFundsAsync(false)).ReturnsAsync(financialAidFundCollection);
                fundRepositoryMock.Setup(repo => repo.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), true)).ReturnsAsync(financialAidFundsEntityTuple);
                fundRepositoryMock.Setup(repo => repo.GetFinancialAidFundsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), false)).ReturnsAsync(financialAidFundsEntityTuple);
                fundRepositoryMock.Setup(i => i.GetFinancialAidFundByIdAsync(It.IsAny<string>())).ReturnsAsync(financialAidFundCollection.ToList()[2]);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(financialAidYearCollection);
                officeRepositoryMock.Setup(repo => repo.GetFinancialAidOfficesAsync(It.IsAny<bool>())).ReturnsAsync(financialAidOfficeCollection);
                referenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("USA");
                fundRepositoryMock.Setup(repo => repo.GetFinancialAidFundFinancialsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).ReturnsAsync(financialAidFundFinancialCollection);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundCategoriesAsync(It.IsAny<bool>())).ReturnsAsync(financialAidCategoryCollection);
                referenceRepositoryMock.Setup(repo => repo.GetFinancialAidFundClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(financialAidClassificationCollection);

                financialAidFundService = new FinancialAidFundsService(referenceRepository, fundRepository, officeRepository, baseConfigurationRepository, adapterRegistry, userFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                financialAidFundCollection = null;
                referenceRepository = null;
                officeRepository = null;
                fundRepository = null;
                financialAidFundService = null;
            }

            [TestMethod]
            public async Task FinancialAidFundService__FinancialAidFunds()
            {
                var results = await financialAidFundService.GetFinancialAidFundsAsync(0,10, criteriaFilter);
                Assert.IsTrue(results is Tuple<IEnumerable<Dtos.FinancialAidFunds>, int>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task FinancialAidFundService_FinancialAidFunds_Count()
            {
                var results = await financialAidFundService.GetFinancialAidFundsAsync(0, 10, criteriaFilter);
                Assert.AreEqual(3, results.Item2);
            }

            [TestMethod]
            public async Task FinancialAidFundService_FinancialAidFunds_Properties()
            {
                var results = await financialAidFundService.GetFinancialAidFundsAsync(0, 10, criteriaFilter);
                var financialAidFund = results.Item1.Where(x => x.Code == "CODE1").FirstOrDefault();
                Assert.IsNotNull(financialAidFund.Id);
                Assert.IsNotNull(financialAidFund.Code);
            }

            [TestMethod]
            public async Task FinancialAidFundService_FinancialAidFunds_Expected()
            {
                var expectedResults = financialAidFundCollection.Where(c => c.Code == "CODE2").FirstOrDefault();
                var results = await financialAidFundService.GetFinancialAidFundsAsync(0, 10, criteriaFilter);
                var financialAidFund = results.Item1.Where(s => s.Code == "CODE2").FirstOrDefault();
                Assert.AreEqual(expectedResults.Guid, financialAidFund.Id);
                Assert.AreEqual(expectedResults.Code, financialAidFund.Code);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FinancialAidFundService_GetFinancialAidFundByGuid_Empty()
            {
                fundRepositoryMock.Setup(i => i.GetFinancialAidFundByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                try
                {
                    await financialAidFundService.GetFinancialAidFundsByGuidAsync("");
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Integration API exception", ex.Message);
                    Assert.IsTrue(ex.Errors.Count > 0);
                    Assert.AreEqual("financial-aid-funds not found for GUID ''", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task FinancialAidFundService_GetFinancialAidFundByGuid_Null()
            {
                fundRepositoryMock.Setup(i => i.GetFinancialAidFundByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                try
                {
                    await financialAidFundService.GetFinancialAidFundsByGuidAsync(null);
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("Integration API exception", ex.Message);
                    Assert.IsTrue(ex.Errors.Count > 0);
                    Assert.AreEqual("financial-aid-funds not found for GUID ''", ex.Errors[0].Message);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task FinancialAidFundService_GetFinancialAidFundByGuid_Expected()
            {
                var expectedResults = financialAidFundCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidFund = await financialAidFundService.GetFinancialAidFundsByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.AreEqual(expectedResults.Guid, financialAidFund.Id);
                Assert.AreEqual(expectedResults.Code, financialAidFund.Code);
            }

            [TestMethod]
            public async Task FinancialAidFundService_GetFinancialAidFundByGuid_Properties()
            {
                var expectedResults = financialAidFundCollection.Where(c => c.Guid == "1df164eb-8178-4321-a9f7-24f12d3991d8").FirstOrDefault();
                var financialAidFund = await financialAidFundService.GetFinancialAidFundsByGuidAsync("1df164eb-8178-4321-a9f7-24f12d3991d8");
                Assert.IsNotNull(financialAidFund.Id);
                Assert.IsNotNull(financialAidFund.Code);
            }
        }
    }
}