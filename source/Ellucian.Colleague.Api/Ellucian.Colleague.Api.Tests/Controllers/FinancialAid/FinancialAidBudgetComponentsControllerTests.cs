/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class FinancialAidBudgetComponentsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        public TestFinancialAidReferenceDataRepository dataRepository;
        public FinancialAidBudgetComponentsController budgetComponentsController;

        public void BudgetComponentControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            referenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

            dataRepository = new TestFinancialAidReferenceDataRepository();
        }

        [TestClass]
        public class GetBudgetComponentsTests : FinancialAidBudgetComponentsControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                BudgetComponentControllerTestsInitialize();

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                referenceDataRepositoryMock.Setup(r => r.BudgetComponents)
                    .Returns(() => dataRepository.BudgetComponents);

                adapterRegistryMock.Setup(r => r.GetAdapter<Colleague.Domain.FinancialAid.Entities.BudgetComponent, Colleague.Dtos.FinancialAid.BudgetComponent>())
                    .Returns(() => new AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.BudgetComponent, Colleague.Dtos.FinancialAid.BudgetComponent>(adapterRegistryMock.Object, loggerMock.Object));

                budgetComponentsController = new FinancialAidBudgetComponentsController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                var budgetDtos = budgetComponentsController.GetBudgetComponents();

                foreach (var budgetDto in budgetDtos)
                {
                    var budgetDomain = dataRepository.BudgetComponents.FirstOrDefault(b => b.Code == budgetDto.Code && b.AwardYear == budgetDto.AwardYear);
                    Assert.IsNotNull(budgetDomain);

                    Assert.AreEqual(budgetDomain.Description, budgetDto.Description);
                    Assert.AreEqual(budgetDomain.ShoppingSheetGroup.HasValue, budgetDto.ShoppingSheetGroup.HasValue);
                    if (budgetDomain.ShoppingSheetGroup.HasValue)
                    {
                        Assert.AreEqual(budgetDomain.ShoppingSheetGroup.Value.ToString(), budgetDto.ShoppingSheetGroup.Value.ToString());
                    }
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void UnexpectedExceptionThrown_LogMessageThrowHttpResponseExceptionTest()
            {
                referenceDataRepositoryMock.Setup(r => r.BudgetComponents).Throws(new NotImplementedException());

                try
                {
                    budgetComponentsController.GetBudgetComponents();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<NotImplementedException>(), It.IsAny<string>()));

                    Assert.AreEqual(hre.Response.StatusCode, HttpStatusCode.BadRequest);

                    throw;
                }
            }


        }
    }
}
