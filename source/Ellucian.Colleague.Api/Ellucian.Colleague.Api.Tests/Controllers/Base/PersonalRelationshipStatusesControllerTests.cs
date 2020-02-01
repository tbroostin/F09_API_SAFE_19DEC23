// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public class PersonalRelationshipStatusesControllerTests
    {
        [TestClass]
        public class PersonalRelationshipStatusesControllerGet
        {
            #region Test Context

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

            #endregion

            private PersonalRelationshipStatusesController PersonalRelationshipStatusesController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RelationshipStatus> allPersonalRelationshipStatusEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IPersonalRelationshipTypeService> personalRelationshipServiceMock;
            private IPersonalRelationshipTypeService personalRelationshipService;
            List<PersonalRelationshipStatus> PersonalRelationshipStatusList;
            private string personalRelationshipStatussGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonalRelationshipStatus, PersonalRelationshipStatus>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                personalRelationshipServiceMock = new Mock<IPersonalRelationshipTypeService>();
                personalRelationshipService = personalRelationshipServiceMock.Object;

                allPersonalRelationshipStatusEntities = new TestPersonalRelationshipStatusRepository().GetPersonalRelationshipStatuses();
                PersonalRelationshipStatusList = new List<PersonalRelationshipStatus>();

                PersonalRelationshipStatusesController = new PersonalRelationshipStatusesController(personalRelationshipService, logger);
                PersonalRelationshipStatusesController.Request = new HttpRequestMessage();
                PersonalRelationshipStatusesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var personalRelationshipStatus in allPersonalRelationshipStatusEntities)
                {
                    PersonalRelationshipStatus target = ConvertPersonalRelationshipStatusEntityToDto(personalRelationshipStatus);
                    PersonalRelationshipStatusList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(allPersonalRelationshipStatusEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                PersonalRelationshipStatusesController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetPersonalRelationshipStatusesByGuidAsync_Validate()
            {
                var thisPersonalRelationshipStatus = PersonalRelationshipStatusList.Where(m => m.Id == personalRelationshipStatussGuid).FirstOrDefault();

                personalRelationshipServiceMock.Setup(x => x.GetPersonalRelationshipStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPersonalRelationshipStatus);

                var personalRelationshipStatus = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusByIdAsync(personalRelationshipStatussGuid);
                Assert.AreEqual(thisPersonalRelationshipStatus.Id, personalRelationshipStatus.Id);
                Assert.AreEqual(thisPersonalRelationshipStatus.Code, personalRelationshipStatus.Code);
                Assert.AreEqual(thisPersonalRelationshipStatus.Description, personalRelationshipStatus.Description);
            }

            [TestMethod]
            public async Task PersonalRelationshipStatusesController_GetHedmAsync()
            {
                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipStatusList);

                var result = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(result.Count(), allPersonalRelationshipStatusEntities.Count());

                int count = allPersonalRelationshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonalRelationshipStatusList[i];
                    var actual = allPersonalRelationshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipStatusesController_GetHedmAsync_CacheControlNotNull()
            {
                PersonalRelationshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipStatusList);

                var result = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(result.Count(), allPersonalRelationshipStatusEntities.Count());

                int count = allPersonalRelationshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonalRelationshipStatusList[i];
                    var actual = allPersonalRelationshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipStatussController_GetHedmAsync_NoCache()
            {
                PersonalRelationshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PersonalRelationshipStatusesController.Request.Headers.CacheControl.NoCache = true;

                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipStatusList);

                var result = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(result.Count(), allPersonalRelationshipStatusEntities.Count());

                int count = allPersonalRelationshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonalRelationshipStatusList[i];
                    var actual = allPersonalRelationshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipStatussController_GetHedmAsync_Cache()
            {
                PersonalRelationshipStatusesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PersonalRelationshipStatusesController.Request.Headers.CacheControl.NoCache = false;

                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).ReturnsAsync(PersonalRelationshipStatusList);

                var result = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusesAsync();
                Assert.AreEqual(result.Count(), allPersonalRelationshipStatusEntities.Count());

                int count = allPersonalRelationshipStatusEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonalRelationshipStatusList[i];
                    var actual = allPersonalRelationshipStatusEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonalRelationshipStatusesController_GetByIdHedmAsync()
            {
                var thisPersonalRelationshipStatus = PersonalRelationshipStatusList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                personalRelationshipServiceMock.Setup(x => x.GetPersonalRelationshipStatusByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPersonalRelationshipStatus);

                var personalRelationshipStatus = await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisPersonalRelationshipStatus.Id, personalRelationshipStatus.Id);
                Assert.AreEqual(thisPersonalRelationshipStatus.Code, personalRelationshipStatus.Code);
                Assert.AreEqual(thisPersonalRelationshipStatus.Description, personalRelationshipStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonalRelationshipStatussController_GetThrowsIntAppiExc()
            {
                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusesAsync(It.IsAny<bool>())).Throws<Exception>();

                await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonalRelationshipStatusesController_GetByIdThrowsIntAppiExc()
            {
                personalRelationshipServiceMock.Setup(gc => gc.GetPersonalRelationshipStatusByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await PersonalRelationshipStatusesController.GetPersonalRelationshipStatusByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonalRelationshipStatusesController_PostThrowsIntAppiExc()
            {
                await PersonalRelationshipStatusesController.PostPersonalRelationshipStatusAsync(PersonalRelationshipStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonalRelationshipStatusesController_PutThrowsIntAppiExc()
            {
                var result = await PersonalRelationshipStatusesController.PutPersonalRelationshipStatusAsync(PersonalRelationshipStatusList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonalRelationshipStatusesController_DeleteThrowsIntAppiExc()
            {
                await PersonalRelationshipStatusesController.DeletePersonalRelationshipStatusAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a PersonalRelationshipStatus domain entity to its corresponding PersonalRelationshipStatus DTO
            /// </summary>
            /// <param name="source">PersonalRelationshipStatus domain entity</param>
            /// <returns>PersonalRelationshipStatus DTO</returns>
            private Ellucian.Colleague.Dtos.PersonalRelationshipStatus ConvertPersonalRelationshipStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.RelationshipStatus source)
            {
                var personalRelationshipStatus = new Ellucian.Colleague.Dtos.PersonalRelationshipStatus();
                personalRelationshipStatus.Id = source.Guid;
                personalRelationshipStatus.Code = source.Code;
                personalRelationshipStatus.Title = source.Description;
                personalRelationshipStatus.Description = null;

                return personalRelationshipStatus;
            }
        }
    }
}