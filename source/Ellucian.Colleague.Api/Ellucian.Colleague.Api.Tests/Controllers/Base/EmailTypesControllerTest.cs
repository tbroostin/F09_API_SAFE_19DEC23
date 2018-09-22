// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EmailTypesControllerTest
    {
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

        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        Mock<IEmailTypeService> emailTypeServiceMock;
        Mock<ILogger> loggerMock;

        EmailTypesController emailTypeController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.EmailType> emailTypeEntityItems;

        List<Ellucian.Colleague.Dtos.Base.EmailType> emailTypeDtoBaseItems = new List<Ellucian.Colleague.Dtos.Base.EmailType>();
        List<Ellucian.Colleague.Dtos.EmailType> emailTypeDtoItems = new List<Ellucian.Colleague.Dtos.EmailType>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            emailTypeServiceMock = new Mock<IEmailTypeService>();
            loggerMock = new Mock<ILogger>();

            emailTypeEntityItems = new TestEmailTypeRepository().Get();

            foreach (var emailItem in emailTypeEntityItems)
            {
                emailTypeDtoItems.Add(new EmailType() { Id = emailItem.Guid, Code = emailItem.Code, Description = emailItem.Description, Title = emailItem.Description });
                emailTypeDtoBaseItems.Add(new Dtos.Base.EmailType() { Code = emailItem.Code, Description = emailItem.Description });
            }

            emailTypeController = new EmailTypesController(adapterRegistryMock.Object, referenceDataRepositoryMock.Object, emailTypeServiceMock.Object, loggerMock.Object);
            emailTypeController.Request = new HttpRequestMessage();
            emailTypeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            emailTypeController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            emailTypeDtoBaseItems = null;
            emailTypeEntityItems = null;
            emailTypeDtoItems = null;
            emailTypeController = null;
        }

        [TestMethod]
        public async Task EmailTypesController_GetEmailTypesAsync()
        {
            //Arrange
            emailTypeServiceMock.Setup(e => e.GetEmailTypesAsync(It.IsAny<bool>())).ReturnsAsync(emailTypeDtoItems);
            //Act
            var emailTypesResults = (await emailTypeController.GetEmailTypesAsync()).ToList();
            //Assert
            int count = emailTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = emailTypeDtoItems[i];
                var actual = emailTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task EmailTypesController_GetAsync()
        {
            //Arrange
            emailTypeServiceMock.Setup(e => e.GetBaseEmailTypesAsync()).ReturnsAsync(emailTypeDtoBaseItems);
            //Act
            var emailTypesResults = (await emailTypeController.GetAsync()).ToList();
            //Assert
            int count = emailTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = emailTypeDtoItems[i];
                var actual = emailTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task EmailTypesController_GetEmailTypeByGuidAsync()
        {
            //Arrange
            Ellucian.Colleague.Dtos.EmailType emailTypeDtoItem = emailTypeDtoItems.FirstOrDefault();
            emailTypeServiceMock.Setup(e => e.GetEmailTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(emailTypeDtoItem);
            //Act
            var emailTypesResult = await emailTypeController.GetEmailTypeByIdAsync(emailTypeDtoItem.Id);
            //Assert
            Assert.AreEqual(emailTypeDtoItem.Id, emailTypesResult.Id);
            Assert.AreEqual(emailTypeDtoItem.Code, emailTypesResult.Code);
            Assert.AreEqual(emailTypeDtoItem.Description, emailTypesResult.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_GetEmailTypesAsync_Exception()
        {
            //Arrange
            emailTypeServiceMock.Setup(x => x.GetEmailTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            //Act
            var result = await emailTypeController.GetEmailTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_GetAsync_Exception()
        {
            //Arrange
            emailTypeServiceMock.Setup(x => x.GetBaseEmailTypesAsync()).ThrowsAsync(new Exception());
            //Act
            var result = await emailTypeController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_GetEmailTypeByGuidAsync_Exception()
        {
            //Arrange
            emailTypeServiceMock.Setup(x => x.GetEmailTypeByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            //Act
            var result = await emailTypeController.GetEmailTypeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_DeleteEmailTypesAsync()
        {
            await emailTypeController.DeleteEmailTypesAsync(emailTypeDtoItems.FirstOrDefault().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_PutEmailTypesAsync()
        {
            await emailTypeController.PutEmailTypesAsync(emailTypeDtoItems.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_PostEmailTypesAsync()
        {
            await emailTypeController.PostEmailTypesAsync(It.IsAny<EmailType>());
        }
    }
}
