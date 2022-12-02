/*Copyright 2019-2021 Ellucian Company L.P. and its affiliates.*/
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
using Ellucian.Data.Colleague.Exceptions;

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

        public Dtos.BudgetManagement.WorkingBudget returnedWorkingBudgetDto;
        public Dtos.BudgetManagement.WorkingBudget2 returnedWorkingBudget2Dto;
        public List<Dtos.BudgetManagement.BudgetLineItem> returnedBudgetLineItemsDto;

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

            returnedWorkingBudgetDto = new Dtos.BudgetManagement.WorkingBudget()
            {
                BudgetLineItems = new List<Dtos.BudgetManagement.BudgetLineItem>()
            };
            returnedWorkingBudget2Dto = new Dtos.BudgetManagement.WorkingBudget2()
            {
                LineItems = new List<Dtos.BudgetManagement.LineItem>()
            };
            returnedBudgetLineItemsDto = new List<Dtos.BudgetManagement.BudgetLineItem>
            {
                new Dtos.BudgetManagement.BudgetLineItem()
            };

            adapter = adapterRegistry.Object.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>();
            buDevServiceMock.Setup(s => s.GetBudgetDevelopmentWorkingBudgetAsync(0, 999))
                .Returns(Task.Run(() =>
                {
                    return returnedWorkingBudgetDto;
                }));
            buDevServiceMock.Setup(s => s.QueryWorkingBudget2Async(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Returns(Task.Run(() =>
                {
                    return returnedWorkingBudget2Dto;
                }));
            buDevServiceMock.Setup(s => s.UpdateBudgetDevelopmentWorkingBudgetAsync(It.IsAny<List<Dtos.BudgetManagement.BudgetLineItem>>()))
                .Returns(Task.Run(() =>
                {
                    return returnedBudgetLineItemsDto;
                }));
            buDevServiceMock.Setup(s => s.QueryWorkingBudgetAsync(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Returns(Task.Run(() =>
                {
                    return returnedWorkingBudgetDto;
                }));
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

        [TestMethod]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_ReturnsExpectedResultTest()
        {
            var expectedDto = returnedWorkingBudgetDto;
            var actualDto = await actualController.GetBudgetDevelopmentWorkingBudgetAsync(0, 999);
            Assert.AreEqual(expectedDto.TotalLineItems, actualDto.TotalLineItems);
            Assert.AreEqual(expectedDto.BudgetLineItems.Count, actualDto.BudgetLineItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetBudgetDevelopmentWorkingBudgetAsync_ExceptionTest()
        {
            buDevServiceMock.Setup(s => s.GetBudgetDevelopmentWorkingBudgetAsync(0, 999))
                .Throws(new Exception());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.GetBudgetDevelopmentWorkingBudgetAsync(0, 999);
        }



        [TestMethod]
        public async Task QueryWorkingBudgetByPost2Async_ReturnsExpectedResultTest()
        {
            var expectedDto = returnedWorkingBudget2Dto;
            var actualDto = await actualController.QueryWorkingBudgetByPost2Async(new Dtos.BudgetManagement.WorkingBudgetQueryCriteria());
            Assert.AreEqual(expectedDto.TotalLineItems, actualDto.TotalLineItems);
            Assert.AreEqual(expectedDto.LineItems.Count, actualDto.LineItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryWorkingBudgetByPost2Async_ExceptionTest()
        {
            buDevServiceMock.Setup(s => s.QueryWorkingBudget2Async(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Throws(new Exception());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.QueryWorkingBudgetByPost2Async(new Dtos.BudgetManagement.WorkingBudgetQueryCriteria());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryWorkingBudgetByPost2Async_NullArgumentTest()
        {
            buDevServiceMock.Setup(s => s.QueryWorkingBudget2Async(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Throws(new ArgumentNullException());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.QueryWorkingBudgetByPost2Async(null);
        }



        [TestMethod]
        public async Task QueryWorkingBudgetByPostAsync_ReturnsExpectedResultTest()
        {
            var expectedDto = returnedWorkingBudgetDto;
            var actualDto = await actualController.QueryWorkingBudgetByPostAsync(new Dtos.BudgetManagement.WorkingBudgetQueryCriteria());
            Assert.AreEqual(expectedDto.TotalLineItems, actualDto.TotalLineItems);
            Assert.AreEqual(expectedDto.BudgetLineItems.Count, actualDto.BudgetLineItems.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryWorkingBudgetByPostAsync_ExceptionTest()
        {
            buDevServiceMock.Setup(s => s.QueryWorkingBudgetAsync(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Throws(new Exception());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.QueryWorkingBudgetByPostAsync(new Dtos.BudgetManagement.WorkingBudgetQueryCriteria());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryWorkingBudgetByPostAsync_NullArgumentTest()
        {
            buDevServiceMock.Setup(s => s.QueryWorkingBudgetAsync(It.IsAny<Dtos.BudgetManagement.WorkingBudgetQueryCriteria>()))
                .Throws(new ArgumentNullException());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.QueryWorkingBudgetByPostAsync(null);
        }



        [TestMethod]
        public async Task UpdateBudgetDevelopmentWorkingBudgetAsync_ReturnsExpectedResultTest()
        {
            var expectedDto = returnedBudgetLineItemsDto;
            var actualDto = await actualController.UpdateBudgetDevelopmentWorkingBudgetAsync(new List<Dtos.BudgetManagement.BudgetLineItem>());
            Assert.AreEqual(expectedDto.Count, actualDto.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateBudgetDevelopmentWorkingBudgetAsync_ExceptionTest()
        {
            buDevServiceMock.Setup(s => s.UpdateBudgetDevelopmentWorkingBudgetAsync(new List<Dtos.BudgetManagement.BudgetLineItem>()))
                .Throws(new Exception());
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.UpdateBudgetDevelopmentWorkingBudgetAsync(new List<Dtos.BudgetManagement.BudgetLineItem>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task UpdateBudgetDevelopmentWorkingBudgetAsync_SessionExceptionTest()
        {
            buDevServiceMock.Setup(s => s.UpdateBudgetDevelopmentWorkingBudgetAsync(new List<Dtos.BudgetManagement.BudgetLineItem>()))
                .Throws(new ColleagueSessionExpiredException("session expired"));
            actualController = new BudgetDevelopmentController(buDevServiceMock.Object, loggerMock.Object);
            var actualDto = await actualController.UpdateBudgetDevelopmentWorkingBudgetAsync(new List<Dtos.BudgetManagement.BudgetLineItem>());
        }
    }
}
