// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class IpedsInstitutionsControllerTests
    {
        [TestClass]
        public class QueryByPostIpedsInstitutionsByOpeIdTests
        {
            #region Test Context
            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }
            #endregion

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IIpedsInstitutionRepository> ipedsInstitutionRepositoryMock;

            private List<string> inputOpeIds;

            private List<IpedsInstitution> expectedIpedsInstitutions;
            private IEnumerable<IpedsInstitution> actualIpedsInstitutions;

            private IpedsInstitutionsController actualController;

            private TestIpedsInstitutionRepository testRepository;
            private IEnumerable<Domain.FinancialAid.Entities.IpedsInstitution> inputIpedsInsitutions;

            [TestInitialize]
            public async void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                ipedsInstitutionRepositoryMock = new Mock<IIpedsInstitutionRepository>();

                testRepository = new TestIpedsInstitutionRepository();
                inputOpeIds = testRepository.ipedsDbDataList.Select(i => i.opeId).ToList();
                inputIpedsInsitutions = testRepository.GetIpedsInstitutionsAsync(inputOpeIds).Result;
                expectedIpedsInstitutions = new List<IpedsInstitution>();

                var ipedsInstitutionsDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.IpedsInstitution, IpedsInstitution>(adapterRegistryMock.Object, loggerMock.Object);
                foreach (var ipedsInstitutionEntity in inputIpedsInsitutions)
                {
                    expectedIpedsInstitutions.Add(ipedsInstitutionsDtoAdapter.MapToType(ipedsInstitutionEntity));
                }

                ipedsInstitutionRepositoryMock.Setup(r => r.GetIpedsInstitutionsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(inputIpedsInsitutions);
                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.IpedsInstitution, IpedsInstitution>()).Returns(ipedsInstitutionsDtoAdapter);

                actualController = new IpedsInstitutionsController(adapterRegistryMock.Object, ipedsInstitutionRepositoryMock.Object, loggerMock.Object);
                actualIpedsInstitutions = await actualController.QueryByPostIpedsInstitutionsByOpeIdAsync(inputOpeIds);
            }

            [TestMethod]
            public void IpedsInstitutionTest()
            {
                Assert.IsNotNull(actualIpedsInstitutions);
                Assert.AreEqual(expectedIpedsInstitutions.Count(), actualIpedsInstitutions.Count());
            }

            [TestMethod]
            public async Task NullInputOpeIdsReturnsEmptyListTest()
            {
                actualIpedsInstitutions = await actualController.QueryByPostIpedsInstitutionsByOpeIdAsync(null);
                Assert.IsNotNull(actualIpedsInstitutions);
                Assert.AreEqual(0, actualIpedsInstitutions.Count());
            }

            [TestMethod]
            public async Task EmptyInputOpeIdsReturnsEmptyListTest()
            {
                actualIpedsInstitutions = await actualController.QueryByPostIpedsInstitutionsByOpeIdAsync(new List<string>());
                Assert.IsNotNull(actualIpedsInstitutions);
                Assert.AreEqual(0, actualIpedsInstitutions.Count());
            }


        }
    }
}
