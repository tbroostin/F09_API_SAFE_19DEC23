//Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class BankingInformationConfigurationsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IBankingInformationConfigurationService> serviceMock;

        public BankingInformationConfigurationsController controllerUnderTest;

        string testTAC = "Better read the fine print";

        public void BankingInformationConfigurationsControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            var configDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.BankingInformationConfiguration, BankingInformationConfiguration>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.BankingInformationConfiguration, BankingInformationConfiguration>()).Returns(configDtoAdapter);
            serviceMock = new Mock<IBankingInformationConfigurationService>();
            serviceMock.Setup(x => x.GetBankingInformationConfigurationAsync())
                .ReturnsAsync(new BankingInformationConfiguration() { AddEditAccountTermsAndConditions = testTAC });

            controllerUnderTest = new BankingInformationConfigurationsController(loggerMock.Object, adapterRegistryMock.Object, serviceMock.Object);
        }

        [TestClass]
        public class GetConfigurationAsyncTests : BankingInformationConfigurationsControllerTests
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

            public BankingInformationConfiguration expectedDto;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                BankingInformationConfigurationsControllerTestsInitialize();
                expectedDto = new BankingInformationConfiguration() { AddEditAccountTermsAndConditions = testTAC };

            }

            [TestMethod]
            public async Task MethodReturnsExpectedDto()
            {
                var actualDto = await controllerUnderTest.GetAsync();
                Assert.AreEqual(expectedDto.AddEditAccountTermsAndConditions, actualDto.AddEditAccountTermsAndConditions);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundCreatesHttpResponseExceptionTest()
            {
                try
                {
                    serviceMock.Setup(x => x.GetBankingInformationConfigurationAsync())
                        .Throws(new KeyNotFoundException());
                    await controllerUnderTest.GetAsync();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw hre;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorCreatesHttpResponseExcpetion()
            {
                try
                {
                    serviceMock.Setup(x => x.GetBankingInformationConfigurationAsync())
                        .Throws(new Exception());
                    await controllerUnderTest.GetAsync();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw hre;
                }
            }
        }
    }
}
