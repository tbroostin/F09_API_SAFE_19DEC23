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
    public class PhoneTypesControllerTests
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
        Mock<IPhoneTypeService> phoneTypeServiceMock;
        Mock<ILogger> loggerMock;

        PhoneTypesController phoneTypeController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PhoneType> phoneTypeEntityItems;

        List<Ellucian.Colleague.Dtos.PhoneType2> phoneTypeDtoItems = new List<Ellucian.Colleague.Dtos.PhoneType2>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            phoneTypeServiceMock = new Mock<IPhoneTypeService>();
            loggerMock = new Mock<ILogger>();

            phoneTypeEntityItems = new TestPhoneTypeRepository().Get();

            foreach (var phoneItem in phoneTypeEntityItems)
            {
                phoneTypeDtoItems.Add(new PhoneType2() { Id = phoneItem.Guid, Code = phoneItem.Code, Description = phoneItem.Description, Title = phoneItem.Description });
            }

            phoneTypeController = new PhoneTypesController(adapterRegistryMock.Object, phoneTypeServiceMock.Object, loggerMock.Object);
            phoneTypeController.Request = new HttpRequestMessage();
            phoneTypeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            phoneTypeController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            phoneTypeEntityItems = null;
            phoneTypeDtoItems = null;
            phoneTypeController = null;
        }

        [TestMethod]
        public async Task PhoneTypesController_GetPhoneTypesAsync()
        {
            //Arrange
            phoneTypeServiceMock.Setup(e => e.GetPhoneTypesAsync(It.IsAny<bool>())).ReturnsAsync(phoneTypeDtoItems);
            //Act
            var phoneTypesResults = (await phoneTypeController.GetPhoneTypesAsync()).ToList();
            //Assert
            int count = phoneTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = phoneTypeDtoItems[i];
                var actual = phoneTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task PhoneTypesController_GetPhoneTypeByIdAsync()
        {
            //Arrange
            Ellucian.Colleague.Dtos.PhoneType2 phoneTypeDtoItem = phoneTypeDtoItems.FirstOrDefault();
            phoneTypeServiceMock.Setup(e => e.GetPhoneTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(phoneTypeDtoItem);
            //Act
            var phoneTypesResult = await phoneTypeController.GetPhoneTypeByIdAsync(phoneTypeDtoItem.Id);
            //Assert
            Assert.AreEqual(phoneTypeDtoItem.Id, phoneTypesResult.Id);
            Assert.AreEqual(phoneTypeDtoItem.Code, phoneTypesResult.Code);
            Assert.AreEqual(phoneTypeDtoItem.Description, phoneTypesResult.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PhoneTypesController_GetPhoneTypesAsync_Exception()
        {
            //Arrange
            phoneTypeServiceMock.Setup(x => x.GetPhoneTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            //Act
            var result = await phoneTypeController.GetPhoneTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PhoneTypesController_GetPhoneTypeByGuidAsync_Exception()
        {
            //Arrange
            phoneTypeServiceMock.Setup(x => x.GetPhoneTypeByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            //Act
            var result = await phoneTypeController.GetPhoneTypeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PhoneTypesController_DeletePhoneTypesAsync()
        {
            await phoneTypeController.DeletePhoneTypesAsync(phoneTypeDtoItems.FirstOrDefault().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PhoneTypesController_PutPhoneTypesAsync()
        {
            await phoneTypeController.PutPhoneTypesAsync(phoneTypeDtoItems.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PhoneTypesController_PostPhoneTypesAsync()
        {
            await phoneTypeController.PostPhoneTypesAsync(It.IsAny<PhoneType2>());
        }
    }
}