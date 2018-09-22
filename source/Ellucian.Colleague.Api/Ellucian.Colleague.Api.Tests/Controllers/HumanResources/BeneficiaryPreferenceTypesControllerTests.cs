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
    public class BeneficiaryPreferenceTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IBeneficiaryPreferenceTypesService> beneficiaryPreferenceTypesServiceMock;
        private Mock<ILogger> loggerMock;
        private BeneficiaryPreferenceTypesController beneficiaryPreferenceTypesController;      
        private IEnumerable<Domain.HumanResources.Entities.BeneficiaryTypes> allBeneficiaryTypes;
        private List<Dtos.BeneficiaryPreferenceTypes> beneficiaryPreferenceTypesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            beneficiaryPreferenceTypesServiceMock = new Mock<IBeneficiaryPreferenceTypesService>();
            loggerMock = new Mock<ILogger>();
            beneficiaryPreferenceTypesCollection = new List<Dtos.BeneficiaryPreferenceTypes>();

            allBeneficiaryTypes  = new List<Domain.HumanResources.Entities.BeneficiaryTypes>()
                {
                    new Domain.HumanResources.Entities.BeneficiaryTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.BeneficiaryTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.BeneficiaryTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allBeneficiaryTypes)
            {
                var beneficiaryPreferenceTypes = new Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                beneficiaryPreferenceTypesCollection.Add(beneficiaryPreferenceTypes);
            }

            beneficiaryPreferenceTypesController = new BeneficiaryPreferenceTypesController(beneficiaryPreferenceTypesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            beneficiaryPreferenceTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            beneficiaryPreferenceTypesController = null;
            allBeneficiaryTypes = null;
            beneficiaryPreferenceTypesCollection = null;
            loggerMock = null;
            beneficiaryPreferenceTypesServiceMock = null;
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_ValidateFields_Nocache()
        {
            beneficiaryPreferenceTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false)).ReturnsAsync(beneficiaryPreferenceTypesCollection);
       
            var sourceContexts = (await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync()).ToList();
            Assert.AreEqual(beneficiaryPreferenceTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = beneficiaryPreferenceTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_ValidateFields_Cache()
        {
            beneficiaryPreferenceTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(true)).ReturnsAsync(beneficiaryPreferenceTypesCollection);

            var sourceContexts = (await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync()).ToList();
            Assert.AreEqual(beneficiaryPreferenceTypesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = beneficiaryPreferenceTypesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_KeyNotFoundException()
        {
            //
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false))
                .Throws<KeyNotFoundException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_PermissionsException()
        {
            
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false))
                .Throws<PermissionsException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_ArgumentException()
        {
            
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false))
                .Throws<ArgumentException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_RepositoryException()
        {
            
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false))
                .Throws<RepositoryException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_IntegrationApiException()
        {
            
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false))
                .Throws<IntegrationApiException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();
        }

        [TestMethod]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuidAsync_ValidateFields()
        {
            var expected = beneficiaryPreferenceTypesCollection.FirstOrDefault();
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypes_Exception()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesAsync(false)).Throws<Exception>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuidAsync_Exception()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_KeyNotFoundException()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_PermissionsException()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_ArgumentException()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_RepositoryException()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_IntegrationApiException()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_GetBeneficiaryPreferenceTypesByGuid_Exception()
        {
            beneficiaryPreferenceTypesServiceMock.Setup(x => x.GetBeneficiaryPreferenceTypesByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await beneficiaryPreferenceTypesController.GetBeneficiaryPreferenceTypesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_PostBeneficiaryPreferenceTypesAsync_Exception()
        {
            await beneficiaryPreferenceTypesController.PostBeneficiaryPreferenceTypesAsync(beneficiaryPreferenceTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_PutBeneficiaryPreferenceTypesAsync_Exception()
        {
            var sourceContext = beneficiaryPreferenceTypesCollection.FirstOrDefault();
            await beneficiaryPreferenceTypesController.PutBeneficiaryPreferenceTypesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task BeneficiaryPreferenceTypesController_DeleteBeneficiaryPreferenceTypesAsync_Exception()
        {
            await beneficiaryPreferenceTypesController.DeleteBeneficiaryPreferenceTypesAsync(beneficiaryPreferenceTypesCollection.FirstOrDefault().Id);
        }
    }
}