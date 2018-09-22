// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class AcademicCredentialsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAcademicCredentialService> _academicCredentialServiceMock;
        private IAcademicCredentialService _academicCredentialService;
        private readonly ILogger _logger = new Mock<ILogger>().Object;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private IReferenceDataRepository _refRepo;
        private AcademicCredentialsController _academicCredentialsController;
       
        private IEnumerable<Domain.Base.Entities.OtherDegree> _allDegrees;
        private IEnumerable<Domain.Base.Entities.OtherCcd> _allCcds;
        private List<Dtos.AcademicCredential> _academicCredentialsCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _refRepoMock = new Mock<IReferenceDataRepository>();
            _refRepo = _refRepoMock.Object;

            _academicCredentialServiceMock = new Mock<IAcademicCredentialService>();
            _academicCredentialService = _academicCredentialServiceMock.Object;
            _academicCredentialsCollection = new List<Dtos.AcademicCredential>();

            _allDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();
            _allCcds = new TestAcademicCredentialsRepository().GetOtherCcds();

            foreach (var source in _allDegrees)
            {
                var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
                {
                    Id = source.Guid,
                    Abbreviation = source.Code,
                    Title = source.Description,
                    Description = null,
                    AcademicCredentialType = Dtos.AcademicCredentialType.Degree
                };


                _academicCredentialsCollection.Add(academicCredential);
            }

            foreach (var source in _allCcds)
            {
                var academicCredential = new Ellucian.Colleague.Dtos.AcademicCredential
                {
                    Id = source.Guid,
                    Abbreviation = source.Code,
                    Title = source.Description,
                    Description = null,
                    AcademicCredentialType = Dtos.AcademicCredentialType.Certificate
                };

                _academicCredentialsCollection.Add(academicCredential);
            }

            _academicCredentialsController = new AcademicCredentialsController(_academicCredentialService, _logger)
            {
                Request = new HttpRequestMessage()
            };
            _academicCredentialsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _academicCredentialsController = null;
            _refRepoMock = null;
            _refRepo = null;
            _academicCredentialService = null;
            _allDegrees = null;
            _allCcds = null;
            _academicCredentialsCollection = null;
        }

        [TestMethod]
        public async Task AcademicCredentialsController_GetAcademicCredentialsAsync_ValidateFields_Nocache()
        {
            _academicCredentialsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialsAsync(false)).ReturnsAsync(_academicCredentialsCollection);
       
            var academicCredentials = (await _academicCredentialsController.GetAcademicCredentialsAsync()).ToList();
            Assert.AreEqual(_academicCredentialsCollection.Count, academicCredentials.Count);
            for (var i = 0; i < academicCredentials.Count; i++)
            {
                var expected = _academicCredentialsCollection[i];
                var actual = academicCredentials[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Abbreviation, actual.Abbreviation, "Abbreviation, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AcademicCredentialsController_GetAcademicCredentialsAsync_ValidateFields_Cache()
        {
            _academicCredentialsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialsAsync(true)).ReturnsAsync(_academicCredentialsCollection);

            var academicCredentials = (await _academicCredentialsController.GetAcademicCredentialsAsync()).ToList();
            Assert.AreEqual(_academicCredentialsCollection.Count, academicCredentials.Count);
            for (var i = 0; i < academicCredentials.Count; i++)
            {
                var expected = _academicCredentialsCollection[i];
                var actual = academicCredentials[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Abbreviation, actual.Abbreviation, "Abbreviation, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AcademicCredentialsController_GetAcademicCredentialByGuidAsync_ValidateFields()
        {
            _academicCredentialsController.Request.Headers.CacheControl =
               new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = _academicCredentialsCollection.FirstOrDefault();
            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await _academicCredentialsController.GetAcademicCredentialByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Abbreviation, actual.Abbreviation, "Abbreviation");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_GetAcademicCredentialsAsync_Exception()
        {
            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialsAsync(false)).Throws<Exception>();
            await _academicCredentialsController.GetAcademicCredentialsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_GetAcademicCredentialByGuidAsync_Exception()
        {
            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _academicCredentialsController.GetAcademicCredentialByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_GetAcademicCredentialByGuidAsync_KeyNotFoundException()
        {
            _academicCredentialServiceMock.Setup(x => x.GetAcademicCredentialByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await _academicCredentialsController.GetAcademicCredentialByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_PostAcademicCredential()
        {
            await _academicCredentialsController.PostAcademicCredentialAsync(_academicCredentialsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_PutAcademicCredential()
        {
            var academicCredential = _academicCredentialsCollection.FirstOrDefault();
            await _academicCredentialsController.PutAcademicCredentialAsync(academicCredential.Id, academicCredential);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AcademicCredentialsController_DeleteAcademicCredential()
        {
            await _academicCredentialsController.DeleteAcademicCredentialAsync(_academicCredentialsCollection.FirstOrDefault().Id);
        }
    }
}