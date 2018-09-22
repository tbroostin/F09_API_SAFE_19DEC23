//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class ContractTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IContractTypesService> contractTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private ContractTypesController contractTypesController;
        private IEnumerable<Domain.HumanResources.Entities.HrStatuses> allHrStatuses;
        private List<Dtos.ContractTypes> contractTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            contractTypesServiceMock = new Mock<IContractTypesService>();
            loggerMock = new Mock<ILogger>();
            contractTypesCollection = new List<Dtos.ContractTypes>();

            allHrStatuses = new List<Domain.HumanResources.Entities.HrStatuses>()
                {
                    new Domain.HumanResources.Entities.HrStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.HrStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.HrStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

            foreach (var source in allHrStatuses)
            {
                var contractTypes = new Ellucian.Colleague.Dtos.ContractTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                contractTypesCollection.Add(contractTypes);
            }

            contractTypesController = new ContractTypesController(contractTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            contractTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            contractTypesController = null;
            allHrStatuses = null;
            contractTypesCollection = null;
            loggerMock = null;
            contractTypesServiceMock = null;
        }

        [TestMethod]
        public async Task ContractTypesController_GetContractTypes_ValidateFields_Nocache()
        {
            contractTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false)).ReturnsAsync(contractTypesCollection);

            var sourceContexts = (await contractTypesController.GetContractTypesAsync()).ToList();
            Assert.AreEqual(contractTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = contractTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task ContractTypesController_GetContractTypes_ValidateFields_Cache()
        {
            contractTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(true)).ReturnsAsync(contractTypesCollection);

            var sourceContexts = (await contractTypesController.GetContractTypesAsync()).ToList();
            Assert.AreEqual(contractTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = contractTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_KeyNotFoundException()
        {
            //
            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_PermissionsException()
        {

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false))
                .Throws<PermissionsException>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_ArgumentException()
        {

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false))
                .Throws<ArgumentException>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_RepositoryException()
        {

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false))
                .Throws<RepositoryException>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_IntegrationApiException()
        {

            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false))
                .Throws<IntegrationApiException>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        public async Task ContractTypesController_GetContractTypesByGuidAsync_ValidateFields()
        {
            var expected = contractTypesCollection.FirstOrDefault();
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await contractTypesController.GetContractTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        public async Task ContractTypesController_GetContractTypesByGuidAsync_ValidateFields_With_Cache()
        {
            contractTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = contractTypesCollection.FirstOrDefault();
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await contractTypesController.GetContractTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypes_Exception()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesAsync(false)).Throws<Exception>();
            await contractTypesController.GetContractTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuidAsync_Exception()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await contractTypesController.GetContractTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_KeyNotFoundException()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_PermissionsException()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_ArgumentException()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_RepositoryException()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_IntegrationApiException()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_GetContractTypesByGuid_Exception()
        {
            contractTypesServiceMock.Setup(x => x.GetContractTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await contractTypesController.GetContractTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_PostContractTypesAsync_Exception()
        {
            await contractTypesController.PostContractTypesAsync(contractTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_PutContractTypesAsync_Exception()
        {
            var sourceContext = contractTypesCollection.FirstOrDefault();
            await contractTypesController.PutContractTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task ContractTypesController_DeleteContractTypesAsync_Exception()
        {
            await contractTypesController.DeleteContractTypesAsync(contractTypesCollection.FirstOrDefault().Id);
        }
    }
}