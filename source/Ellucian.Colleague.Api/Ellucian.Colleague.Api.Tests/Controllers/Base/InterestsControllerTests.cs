// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Adapters;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class InterestsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInterestsService> interestsServiceMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;

        private InterestsController interestsController;
        private IEnumerable<Domain.Base.Entities.Interest> allInterests;
        private IEnumerable<Domain.Base.Entities.InterestType> allInterestTypes;
        private List<Dtos.Interest> interestsCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            interestsServiceMock = new Mock<IInterestsService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            interestsCollection = new List<Dtos.Interest>();

            allInterests = new TestInterestsRepository().GetInterests();
            allInterestTypes = new TestInterestTypesRepository().GetInterestTypes();

            foreach (var source in allInterests)
            {
                var interest = new Ellucian.Colleague.Dtos.Interest
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,

                };
                if (!string.IsNullOrEmpty(source.Type))
                {                 
                    var interestArea = new Dtos.GuidObject2();
                    interestArea.Id = allInterestTypes.FirstOrDefault(x=>x.Code == source.Type).Guid;
                    interest.Area = interestArea;
                }
                interestsCollection.Add(interest);
            }

            interestsController = new InterestsController(interestsServiceMock.Object,
                adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            interestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            interestsController = null;
            allInterests = null;
            interestsCollection = null;
            loggerMock = null;
            interestsServiceMock = null;
        }

        [TestMethod]
        public async Task InterestsController_GetHedmInterestsAsync_ValidateFields_Cache()
        {
            interestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            interestsServiceMock.Setup(x => x.GetHedmInterestsAsync(false)).ReturnsAsync(interestsCollection);

            var interests = (await interestsController.GetHedmInterestsAsync()).ToList();
            Assert.AreEqual(interestsCollection.Count, interests.Count);
            for (var i = 0; i < interests.Count; i++)
            {
                var expected = interestsCollection[i];
                var actual = interests[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                if (expected.Area != null)
                {
                    var interestAreaId = allInterestTypes.FirstOrDefault(x => x.Guid == expected.Area.Id).Guid;
                    Assert.AreEqual(expected.Area.Id, interestAreaId, "Area.ID, Index=" + i.ToString());
                }
            }
        }

        [TestMethod]
        public async Task InterestsController_GetHedmInterestsAsync_ValidateFields_BypassCache()
        {
            interestsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            interestsServiceMock.Setup(x => x.GetHedmInterestsAsync(true)).ReturnsAsync(interestsCollection);

            var interests = (await interestsController.GetHedmInterestsAsync()).ToList();
            Assert.AreEqual(interestsCollection.Count, interests.Count);
            for (var i = 0; i < interests.Count; i++)
            {
                var expected = interestsCollection[i];
                var actual = interests[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                if (expected.Area != null)
                {
                    var interestAreaId = allInterestTypes.FirstOrDefault(x => x.Guid == expected.Area.Id).Guid;
                    Assert.AreEqual(expected.Area.Id, interestAreaId, "Area.ID, Index=" + i.ToString());
                }
            }
        }

        [TestMethod]
        public async Task InterestsController_GetHedmInterestsByIdAsync_ValidateFields()
        {
            var expected = interestsCollection.FirstOrDefault();
            interestsServiceMock.Setup(x => x.GetHedmInterestByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await interestsController.GetHedmInterestByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            if (expected.Area != null)
            {
                var interestAreaId = allInterestTypes.FirstOrDefault(x => x.Guid == expected.Area.Id).Guid;
                Assert.AreEqual(expected.Area.Id, interestAreaId, "Area.ID");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_GetHedmInterestAreasAsync_Exception()
        {
            interestsServiceMock.Setup(x => x.GetHedmInterestsAsync(false)).Throws<Exception>();
            await interestsController.GetHedmInterestsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_GetHedmInterestsByIdAsync_Exception()
        {
            interestsServiceMock.Setup(x => x.GetHedmInterestByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await interestsController.GetHedmInterestByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_PostHedmInterest_Exception()
        {
            await interestsController.PostHedmInterestAsync(interestsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_PutHedmInterest_Exception()
        {
            var interestArea = interestsCollection.FirstOrDefault();
            await interestsController.PutHedmInterestAsync(interestArea.Id, interestArea);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_DeleteHedmInterest_Exception()
        {
            await interestsController.DeleteHedmInterestAsync(interestsCollection.FirstOrDefault().Id);
        }
    }

    [TestClass]
    public class InterestAreaControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IInterestsService> interestsServiceMock;
        private Mock<ILogger> loggerMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;

        private InterestsController interestsController;
        private IEnumerable<Domain.Base.Entities.InterestType> allInterestTypes;
        private List<Dtos.InterestArea> interestAreaCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            interestsServiceMock = new Mock<IInterestsService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();

            interestAreaCollection = new List<Dtos.InterestArea>();

            allInterestTypes = new TestInterestTypesRepository().GetInterestTypes();

            foreach (var source in allInterestTypes)
            {
                var interestType = new Ellucian.Colleague.Dtos.InterestArea
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                interestAreaCollection.Add(interestType);
            }

            interestsController = new InterestsController(interestsServiceMock.Object,
                adapterRegistryMock.Object, referenceDataRepositoryMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            interestsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            interestsController = null;
            allInterestTypes = null;
            interestAreaCollection = null;
            loggerMock = null;
            interestsServiceMock = null;
        }

        [TestMethod]
        public async Task InterestsController_GetInterestAreasAsync_ValidateFields_Cache()
        {
            interestsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            interestsServiceMock.Setup(x => x.GetInterestAreasAsync(false)).ReturnsAsync(interestAreaCollection);

            var interestTypes = (await interestsController.GetInterestAreasAsync()).ToList();
            Assert.AreEqual(interestAreaCollection.Count, interestTypes.Count);
            for (var i = 0; i < interestTypes.Count; i++)
            {
                var expected = interestAreaCollection[i];
                var actual = interestTypes[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InterestsController_GetInterestAreasAsync_ValidateFields_BypassCache()
        {
            interestsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            interestsServiceMock.Setup(x => x.GetInterestAreasAsync(true)).ReturnsAsync(interestAreaCollection);

            var interestTypes = (await interestsController.GetInterestAreasAsync()).ToList();
            Assert.AreEqual(interestAreaCollection.Count, interestTypes.Count);
            for (var i = 0; i < interestTypes.Count; i++)
            {
                var expected = interestAreaCollection[i];
                var actual = interestTypes[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task InterestsController_GetInterestAreasByIdAsync_ValidateFields()
        {
            var expected = interestAreaCollection.FirstOrDefault();
            interestsServiceMock.Setup(x => x.GetInterestAreasByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await interestsController.GetInterestAreasByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_GetInterestAreasAsync_Exception()
        {
            interestsServiceMock.Setup(x => x.GetInterestAreasAsync(false)).Throws<Exception>();
            await interestsController.GetInterestAreasAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_GetInterestAreasByIdAsync_Exception()
        {
            interestsServiceMock.Setup(x => x.GetInterestAreasByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await interestsController.GetInterestAreasByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_PostInterestAreas_Exception()
        {
            await interestsController.PostInterestAreasAsync(interestAreaCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_PutInterestAreas_Exception()
        {
            var interestArea = interestAreaCollection.FirstOrDefault();
            await interestsController.PutInterestAreasAsync(interestArea.Id, interestArea);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task InterestsController_DeleteInterestAreas_Exception()
        {
            await interestsController.DeleteInterestAreasAsync(interestAreaCollection.FirstOrDefault().Id);
        }
    }

}