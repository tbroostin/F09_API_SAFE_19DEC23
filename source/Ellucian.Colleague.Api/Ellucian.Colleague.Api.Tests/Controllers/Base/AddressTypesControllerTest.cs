// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
    public class AddressTypesControllerTest
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
        Mock<IAddressTypeService> addressTypeServiceMock;
        Mock<ILogger> loggerMock;

        AddressTypesController addressTypeController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.AddressType2> addressTypeEntityItems;

        //List<Ellucian.Colleague.Dtos.Base> emailTypeDtoBaseItems = new List<Ellucian.Colleague.Dtos.Base.EmailType>();
        List<Ellucian.Colleague.Dtos.AddressType2> addressTypeDtoItems = new List<Ellucian.Colleague.Dtos.AddressType2>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            addressTypeServiceMock = new Mock<IAddressTypeService>();
            loggerMock = new Mock<ILogger>();

            addressTypeEntityItems = new TestAddressTypeRepository().Get();

            foreach (var addressItem in addressTypeEntityItems)
            {
                addressTypeDtoItems.Add(new AddressType2() { Id = addressItem.Guid, Code = addressItem.Code, Description = addressItem.Description, Title = addressItem.Description });
                //emailTypeDtoBaseItems.Add(new Dtos.Base.EmailType() { Code = addressItem.Code, Description = addressItem.Description });
            }

            addressTypeController = new AddressTypesController(adapterRegistryMock.Object, addressTypeServiceMock.Object, loggerMock.Object);
            addressTypeController.Request = new HttpRequestMessage();
            addressTypeController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            addressTypeController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            //emailTypeDtoBaseItems = null;
            addressTypeEntityItems = null;
            addressTypeDtoItems = null;
            addressTypeController = null;
        }

        [TestMethod]
        public async Task AddressTypesController_GetAddressTypesAsync()
        {
            //Arrange
            addressTypeServiceMock.Setup(e => e.GetAddressTypesAsync(It.IsAny<bool>())).ReturnsAsync(addressTypeDtoItems);
            //Act
            var addressTypesResults = (await addressTypeController.GetAddressTypesAsync()).ToList();
            //Assert
            int count = addressTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = addressTypeDtoItems[i];
                var actual = addressTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task AddressTypesController_GetAddressTypeByIdAsync()
        {
            //Arrange
            Ellucian.Colleague.Dtos.AddressType2 addressTypeDtoItem = addressTypeDtoItems.FirstOrDefault();
            addressTypeServiceMock.Setup(e => e.GetAddressTypeByGuidAsync(It.IsAny<string>())).ReturnsAsync(addressTypeDtoItem);
            //Act
            var addressTypesResult = await addressTypeController.GetAddressTypeByIdAsync(addressTypeDtoItem.Id);
            //Assert
            Assert.AreEqual(addressTypeDtoItem.Id, addressTypesResult.Id);
            Assert.AreEqual(addressTypeDtoItem.Code, addressTypesResult.Code);
            Assert.AreEqual(addressTypeDtoItem.Description, addressTypesResult.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressTypesController_GetAddressTypesAsync_Exception()
        {
            //Arrange
            addressTypeServiceMock.Setup(x => x.GetAddressTypesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            //Act
            var result = await addressTypeController.GetAddressTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressTypesController_GetAddressTypeByGuidAsync_Exception()
        {
            //Arrange
            addressTypeServiceMock.Setup(x => x.GetAddressTypeByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            //Act
            var result = await addressTypeController.GetAddressTypeByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressTypesController_DeleteAddressTypesAsync()
        {
            await addressTypeController.DeleteAddressTypesAsync(addressTypeDtoItems.FirstOrDefault().Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AddressTypesController_PutAddressTypesAsync()
        {
            await addressTypeController.PutAddressTypesAsync(addressTypeDtoItems.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmailTypesController_PostEmailTypesAsync()
        {
            await addressTypeController.PostAddressTypesAsync(It.IsAny<AddressType2>());
        }
    }
}
