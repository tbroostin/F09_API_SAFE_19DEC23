// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class GenderIdentityTypesControllerTests
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
        Mock<IGenderIdentityTypeService> genderIdentityTypeServiceMock;
        Mock<ILogger> loggerMock;

        GenderIdentityTypesController genderIdentityTypesController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GenderIdentityType> genderIdentityTypeEntityItems;
        List<Ellucian.Colleague.Dtos.Base.GenderIdentityType> genderIdentityTypeDtoItems = new List<Ellucian.Colleague.Dtos.Base.GenderIdentityType>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            genderIdentityTypeServiceMock = new Mock<IGenderIdentityTypeService>();
            loggerMock = new Mock<ILogger>();

            genderIdentityTypeEntityItems = new List<Domain.Base.Entities.GenderIdentityType>
            {
                new Domain.Base.Entities.GenderIdentityType("1", "FEM", "Female" ),
                new Domain.Base.Entities.GenderIdentityType("2", "MAL", "Male" ),
                new Domain.Base.Entities.GenderIdentityType("3", "TFM", "Transexual (F/M)" ),
            };
            foreach (var genderIdentityItem in genderIdentityTypeEntityItems)
            {
                genderIdentityTypeDtoItems.Add(new Dtos.Base.GenderIdentityType() { Code = genderIdentityItem.Code, Description = genderIdentityItem.Description });
            }

            genderIdentityTypesController = new GenderIdentityTypesController(genderIdentityTypeServiceMock.Object, loggerMock.Object);
            genderIdentityTypesController.Request = new HttpRequestMessage();
            genderIdentityTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            genderIdentityTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            genderIdentityTypeEntityItems = null;
            genderIdentityTypeDtoItems = null;
            genderIdentityTypesController = null;
        }

        [TestMethod]
        public async Task GenderIdentityTypesController_GetAsync()
        {
            //Arrange
            genderIdentityTypeServiceMock.Setup(e => e.GetBaseGenderIdentityTypesAsync(It.IsAny<bool>())).ReturnsAsync(genderIdentityTypeDtoItems);
            //Act
            var genderIdentityTypesResults = (await genderIdentityTypesController.GetAsync()).ToList();
            //Assert
            int count = genderIdentityTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = genderIdentityTypeDtoItems[i];
                var actual = genderIdentityTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

    }
}
