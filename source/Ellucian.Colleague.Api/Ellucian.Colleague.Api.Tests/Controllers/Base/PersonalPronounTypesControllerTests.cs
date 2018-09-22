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
    public class PersonalPronounTypesControllerTests
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
        Mock<IPersonalPronounTypeService> personalPronounTypeServiceMock;
        Mock<ILogger> loggerMock;

        PersonalPronounTypesController personalPronounTypesController;
        IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonalPronounType> personalPronounTypeEntityItems;
        List<Ellucian.Colleague.Dtos.Base.PersonalPronounType> personalPronounTypeDtoItems = new List<Ellucian.Colleague.Dtos.Base.PersonalPronounType>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personalPronounTypeServiceMock = new Mock<IPersonalPronounTypeService>();
            loggerMock = new Mock<ILogger>();

            personalPronounTypeEntityItems = new List<Domain.Base.Entities.PersonalPronounType>
            {
                new Domain.Base.Entities.PersonalPronounType("1", "HE", "He/Him/His" ),
                new Domain.Base.Entities.PersonalPronounType("2", "SHE", "She/Her/Hers" ),
                new Domain.Base.Entities.PersonalPronounType("3", "ZE", "Ze/Zir/Zirs" ),
            };
            foreach (var personalPronounItem in personalPronounTypeEntityItems)
            {
                personalPronounTypeDtoItems.Add(new Dtos.Base.PersonalPronounType() { Code = personalPronounItem.Code, Description = personalPronounItem.Description });
            }

            personalPronounTypesController = new PersonalPronounTypesController(personalPronounTypeServiceMock.Object, loggerMock.Object);
            personalPronounTypesController.Request = new HttpRequestMessage();
            personalPronounTypesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personalPronounTypesController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
        }

        [TestCleanup]
        public void Cleanup()
        {
            personalPronounTypeEntityItems = null;
            personalPronounTypeDtoItems = null;
            personalPronounTypesController = null;
        }

        [TestMethod]
        public async Task PersonalPronounTypesController_GetAsync()
        {
            //Arrange
            personalPronounTypeServiceMock.Setup(e => e.GetBasePersonalPronounTypesAsync(It.IsAny<bool>())).ReturnsAsync(personalPronounTypeDtoItems);
            //Act
            var personalPronounTypesResults = (await personalPronounTypesController.GetAsync()).ToList();
            //Assert
            int count = personalPronounTypesResults.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = personalPronounTypeDtoItems[i];
                var actual = personalPronounTypesResults[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

    }
}
