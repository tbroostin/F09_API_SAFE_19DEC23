/*Copyright 2018 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentFinancialAidOfficesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentFinancialAidOfficeService> studentFinancialAidOfficeServiceMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ILogger> loggerMock;
        private StudentFinancialAidOfficesController studentFinancialAidOfficesController;
        private IEnumerable<Dtos.FinancialAidOffice> allFinancialAidOffice;
        private List<Dtos.FinancialAidOffice> financialAidOfficeCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentFinancialAidOfficeServiceMock = new Mock<IStudentFinancialAidOfficeService>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            financialAidOfficeCollection = new List<Dtos.FinancialAidOffice>();

            allFinancialAidOffice = new List<Dtos.FinancialAidOffice>()
                {
                    new Dtos.FinancialAidOffice()
                    {
                        Id = "MAIN",
                        Name = "Main Office",
                        AddressLines = new List<string>() {"2375 Fair Lakes Court", "Fairfax, VA 22033"},
                        PhoneNumber = new Dtos.DtoProperties.NumberDtoProperty() { Number = "555-555-5555" },
                        EmailAddress = "mainfaoffice@ellucian.edu",
                        AidAdministrator = "Cindy Lou"
                    },
                    new Dtos.FinancialAidOffice()
                    {
                        Id = "LAW",
                        Name = "Law Office",
                        AddressLines = new List<string>() {"444 MadeUp Dr.", "Whatever, ST 54321"},
                        PhoneNumber = new Dtos.DtoProperties.NumberDtoProperty() { Number = "666-666-6666" },
                        EmailAddress = "lawfaoffice@ellucian.edu",
                        AidAdministrator = "JD Director"
                    }
                };

            foreach (var source in allFinancialAidOffice)
            {
                var financialAidOffice = new Ellucian.Colleague.Dtos.FinancialAidOffice
                {
                    Id = source.Id,
                    Code = source.Code,
                    Description = null,
                    Name = source.Name
                };
                financialAidOfficeCollection.Add(financialAidOffice);
            }

            studentFinancialAidOfficesController = new StudentFinancialAidOfficesController(adapterRegistryMock.Object, studentFinancialAidOfficeServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentFinancialAidOfficesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentFinancialAidOfficesController = null;
            allFinancialAidOffice = null;
            financialAidOfficeCollection = null;
            loggerMock = null;
            studentFinancialAidOfficeServiceMock = null;
        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_ValidateFields_Nocache()
        {
            studentFinancialAidOfficesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).ReturnsAsync(financialAidOfficeCollection);

            var sourceContexts = (await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync()).ToList();
            Assert.AreEqual(financialAidOfficeCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidOfficeCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_ValidateFields_Cache()
        {
            studentFinancialAidOfficesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(true)).ReturnsAsync(financialAidOfficeCollection);

            var sourceContexts = (await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync()).ToList();
            Assert.AreEqual(financialAidOfficeCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidOfficeCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_ValidateFields()
        {
            studentFinancialAidOfficesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = financialAidOfficeCollection.FirstOrDefault();
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_PermissionsException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<PermissionsException>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_KeyNotFoundException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<KeyNotFoundException>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_ArgumentNullException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<ArgumentNullException>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_RepositoryException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<RepositoryException>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_IntgApiException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<IntegrationApiException>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOffice_Exception()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficesAsync(false)).Throws<Exception>();
            await studentFinancialAidOfficesController.GetEedmFinancialAidOfficesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_PermissionsException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_KeyNotFoundException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_ArgumentNullException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentNullException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_RepositoryException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_IntgApiException()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_EmptyOrNullGuid()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_GetFinancialAidOfficesByIdAsync_Exception()
        {
            studentFinancialAidOfficeServiceMock.Setup(x => x.GetFinancialAidOfficeByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await studentFinancialAidOfficesController.GetFinancialAidOfficeByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_PostFinancialAidOfficesAsync_Exception()
        {
            await studentFinancialAidOfficesController.PostFinancialAidOfficeAsync(financialAidOfficeCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_PutFinancialAidOfficesAsync_Exception()
        {
            var sourceContext = financialAidOfficeCollection.FirstOrDefault();
            await studentFinancialAidOfficesController.PutFinancialAidOfficeAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentFinancialAidOfficeController_DeleteFinancialAidOfficesAsync_Exception()
        {
            await studentFinancialAidOfficesController.DeleteFinancialAidOfficeAsync(financialAidOfficeCollection.FirstOrDefault().Id);
        }
    }
}