/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Api.Controllers.BudgetManagement;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Coordination.BudgetManagement.Adapters;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.BudgetManagement.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Api.Tests.Controllers.BudgetManagement
{
    [TestClass]
    public class BudgetDevelopmentControllerTests
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

        public Mock<IBudgetDevelopmentService> buDevServiceMock;
        public IBudgetDevelopmentService buDevService;
        public TestBudgetDevelopmentRepository buDevRepository;
        public BudgetDevelopmentController actualController;
        public TestBudgetDevelopmentRepository testRepository;
        public List<String> majorComponentStartPositions;

        public Mock<ILogger> loggerMock;
        public Mock<IAdapterRegistry> adapterRegistry;

        public ITypeAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget> adapter;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            testRepository = new TestBudgetDevelopmentRepository();
            majorComponentStartPositions = new List<string>() { "1", "4", "7", "10", "13", "19" };
            adapterRegistry = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>())
                .Returns(() => new WorkingBudgetDtoAdapter(adapterRegistry.Object, loggerMock.Object));
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>())
                .Returns(() => new BudgetLineItemDtoAdapter(adapterRegistry.Object, loggerMock.Object));
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>())
                .Returns(() => new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>(adapterRegistry.Object, loggerMock.Object));
            adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>())
                .Returns(() => new AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>(adapterRegistry.Object, loggerMock.Object));
            //adapterRegistry.Setup(x => x.GetAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>())
            //     .Returns(() => new BudgetOfficerEntityToDtoAdapter(adapterRegistry.Object, loggerMock.Object));
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.BudgetManagement.WorkingBudgetQueryCriteria, Domain.BudgetManagement.Entities.WorkingBudgetQueryCriteria>())
                .Returns(() => new WorkingBudgetQueryCriteriaDtoToEntityAdapter(adapterRegistry.Object, loggerMock.Object));

            buDevServiceMock = new Mock<IBudgetDevelopmentService>();
            buDevRepository = new TestBudgetDevelopmentRepository();

            adapter = adapterRegistry.Object.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>();
            buDevServiceMock.Setup(s => s.GetBudgetDevelopmentWorkingBudgetAsync(0, 999))
                .ReturnsAsync(adapter.MapToType((buDevRepository.GetBudgetDevelopmentWorkingBudgetAsync(It.IsAny<string>(), testRepository.budgetConfiguration.BudgetConfigurationComparables, It.IsAny<WorkingBudgetQueryCriteria>(), It.IsAny<string>(), majorComponentStartPositions, It.IsAny<int>(), It.IsAny<int>())).Result));

            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            adapterRegistry = null;
            loggerMock = null;
            buDevServiceMock = null;
            actualController = null;
            testRepository = null;
        }

        //[TestMethod]
        //public async Task GetBudgetDevelopmentWorkingBudgetAsync_ReturnsExpectedResultTest()
        //{
        //    var expectedDto = adapter.MapToType((buDevRepository.GetBudgetDevelopmentWorkingBudgetAsync(It.IsAny<string>(), testRepository.budgetConfiguration.BudgetConfigurationComparables, It.IsAny<WorkingBudgetQueryCriteria>(), It.IsAny<string>(), majorComponentStartPositions, It.IsAny<int>(), It.IsAny<int>())).Result);
        //    var actualDto = await actualController.GetBudgetDevelopmentWorkingBudgetAsync(0, 999);
        //    Assert.AreEqual(expectedDto.TotalLineItems, actualDto.TotalLineItems);
        //    Assert.AreEqual(expectedDto.BudgetLineItems.Count, actualDto.BudgetLineItems.Count);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task GetBudgetDevelopmentWorkingBudgetAsync_ExceptionTest()
        //{
        //    buDevServiceMock.Setup(s => s.GetBudgetDevelopmentWorkingBudgetAsync(0, 999))
        //        .Throws(new Exception());
        //    actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
        //    var actualDto = await actualController.GetBudgetDevelopmentWorkingBudgetAsync(0, 999);
        //}
    }
}
