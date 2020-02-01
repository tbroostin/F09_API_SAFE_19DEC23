// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PhoneNumbersControllerTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }


        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IPhoneNumberRepository> phoneNumberRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IPhoneNumberService> phoneNumberServiceMock;
        private Mock<ILogger> loggerMock;

        private IAdapterRegistry adapterRegistry;
        private IPhoneNumberRepository phoneNumberRepository;
        private IConfigurationRepository configurationRepository;
        private IPhoneNumberService phoneNumberService;
        private ILogger logger;

        private PhoneNumbersController controller;

        [TestInitialize]
        public void PhoneNumbersControllerTests_Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;

            phoneNumberRepositoryMock = new Mock<IPhoneNumberRepository>();
            phoneNumberRepository = phoneNumberRepositoryMock.Object;

            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepository = configurationRepositoryMock.Object;

            phoneNumberServiceMock = new Mock<IPhoneNumberService>();
            phoneNumberService = phoneNumberServiceMock.Object;

            loggerMock = new Mock<ILogger>();
            logger = loggerMock.Object;

            controller = new PhoneNumbersController(adapterRegistry, phoneNumberRepository, configurationRepository, phoneNumberService, logger);
        }

        [TestClass]
        public class PhoneNumbersController_QueryPhoneNumbersAsync_Tests : PhoneNumbersControllerTests
        {
            [TestInitialize]
            public void PhoneNumbersController_QueryPhoneNumbersAsync_Tests_Initialize()
            {
                base.PhoneNumbersControllerTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_null_criteria_throws_exception()
            {
                var numbers = await controller.QueryPhoneNumbersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_null_criteria_PersonIds_throws_exception()
            {
                var numbers = await controller.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = null });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_empty_criteria_PersonIds_throws_exception()
            {
                var numbers = await controller.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_catches_PermissionsException_and_throws_exception()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ThrowsAsync(new PermissionsException());
                var numbers = await controller.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_catches_Exception_and_throws_exception()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ThrowsAsync(new ApplicationException());
                var numbers = await controller.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            public async Task PhoneNumbersController_QueryPhoneNumbersAsync_returns_data_when_service_call_successful()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ReturnsAsync(new List<PhoneNumber>() { new PhoneNumber() { PersonId = "0001234", PhoneNumbers = new List<Phone>() { new Phone() { Number = "123-456-7890", Extension = "123", TypeCode = "H" } } } });
                var numbers = await controller.QueryPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
                Assert.IsNotNull(numbers);
                Assert.AreEqual(1, numbers.Count());
            }
        }

        [TestClass]
        public class PhoneNumbersController_QueryPilotPhoneNumbersAsync_Tests : PhoneNumbersControllerTests
        {
            [TestInitialize]
            public void PhoneNumbersController_QueryPilotPhoneNumbersAsync_Tests_Initialize()
            {
                base.PhoneNumbersControllerTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_null_criteria_throws_exception()
            {
                var numbers = await controller.QueryPilotPhoneNumbersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_null_criteria_PersonIds_throws_exception()
            {
                var numbers = await controller.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = null });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_empty_criteria_PersonIds_throws_exception()
            {
                var numbers = await controller.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_catches_PermissionsException_and_throws_exception()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPilotPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ThrowsAsync(new PermissionsException());
                var numbers = await controller.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_catches_Exception_and_throws_exception()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPilotPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ThrowsAsync(new ApplicationException());
                var numbers = await controller.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
            }

            [TestMethod]
            public async Task PhoneNumbersController_QueryPilotPhoneNumbersAsync_returns_data_when_service_call_successful()
            {
                phoneNumberServiceMock.Setup(svc => svc.QueryPilotPhoneNumbersAsync(It.IsAny<PhoneNumberQueryCriteria>())).ReturnsAsync(new List<PilotPhoneNumber>() { new PilotPhoneNumber() { PersonId = "0001234", PrimaryPhoneNumber = "123-456-7890", SmsPhoneNumber = "234-567-8901" } });
                var numbers = await controller.QueryPilotPhoneNumbersAsync(new Dtos.Base.PhoneNumberQueryCriteria() { PersonIds = new List<string>() { "0001234" } });
                Assert.IsNotNull(numbers);
                Assert.AreEqual(1, numbers.Count());
            }
        }

    }
}
