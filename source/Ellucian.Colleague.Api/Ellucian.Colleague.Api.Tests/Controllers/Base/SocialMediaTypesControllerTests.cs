// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class SocialMediaTypesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IDemographicService> demographicServiceMock;
        private Mock<ILogger> loggerMock;
        private SocialMediaTypesController socialMediaTypesController;      
        private IEnumerable<Domain.Base.Entities.SocialMediaType> allSocialMediaTypes;
        private List<Dtos.SocialMediaType> socialMediaTypesCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            
            demographicServiceMock = new Mock<IDemographicService>();
            loggerMock = new Mock<ILogger>();
            socialMediaTypesCollection = new List<Dtos.SocialMediaType>();

            allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
            
            foreach (var source in allSocialMediaTypes)
            {
                var socialMediaType = new Ellucian.Colleague.Dtos.SocialMediaType
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                socialMediaTypesCollection.Add(socialMediaType);
            }

            socialMediaTypesController = new SocialMediaTypesController(demographicServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            socialMediaTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            socialMediaTypesController = null;
            allSocialMediaTypes = null;
            socialMediaTypesCollection = null;
            loggerMock = null;
            demographicServiceMock = null;
        }

        [TestMethod]
        public async Task SocialMediaTypesController_GetSocialMediaTypes_ValidateFields_Nocache()
        {
            socialMediaTypesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            demographicServiceMock.Setup(x => x.GetSocialMediaTypesAsync(false)).ReturnsAsync(socialMediaTypesCollection);
       
            var socialMediaTypes = (await socialMediaTypesController.GetSocialMediaTypesAsync()).ToList();
            Assert.AreEqual(socialMediaTypesCollection.Count, socialMediaTypes.Count);
            for (var i = 0; i < socialMediaTypes.Count; i++)
            {
                var expected = socialMediaTypesCollection[i];
                var actual = socialMediaTypes[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SocialMediaTypesController_GetSocialMediaTypes_ValidateFields_Cache()
        {
            socialMediaTypesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            demographicServiceMock.Setup(x => x.GetSocialMediaTypesAsync(true)).ReturnsAsync(socialMediaTypesCollection);

            var socialMediaTypes = (await socialMediaTypesController.GetSocialMediaTypesAsync()).ToList();
            Assert.AreEqual(socialMediaTypesCollection.Count, socialMediaTypes.Count);
            for (var i = 0; i < socialMediaTypes.Count; i++)
            {
                var expected = socialMediaTypesCollection[i];
                var actual = socialMediaTypes[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SocialMediaTypesController_GetSocialMediaTypeById_ValidateFields()
        {
            var expected = socialMediaTypesCollection.FirstOrDefault();
            demographicServiceMock.Setup(x => x.GetSocialMediaTypeByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await socialMediaTypesController.GetSocialMediaTypeByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SocialMediaTypesController_GetSocialMediaTypes_Exception()
        {
            demographicServiceMock.Setup(x => x.GetSocialMediaTypesAsync(false)).Throws<Exception>();
            await socialMediaTypesController.GetSocialMediaTypesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SocialMediaTypesController_GetSocialMediaTypeById_Exception()
        {
            demographicServiceMock.Setup(x => x.GetSocialMediaTypeByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await socialMediaTypesController.GetSocialMediaTypeByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SocialMediaTypesController_PostSocialMediaType_Exception()
        {
            await socialMediaTypesController.PostSocialMediaTypeAsync(socialMediaTypesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SocialMediaTypesController_PutSocialMediaType_Exception()
        {
            var socialMediaType = socialMediaTypesCollection.FirstOrDefault();
            await socialMediaTypesController.PutSocialMediaTypeAsync(socialMediaType.Id, socialMediaType);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SocialMediaTypesController_DeleteSocialMediaType_Exception()
        {
            await socialMediaTypesController.DeleteSocialMediaTypeAsync(socialMediaTypesCollection.FirstOrDefault().Id);
        }
    }
}