// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Dtos;
using System;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentClassificationsControllerTests
    {
        [TestClass]
        public class EmploymentClassificationsControllerGet
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

            private EmploymentClassificationsController EmploymentClassificationsController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.EmploymentClassification> allEmploymentClassificationEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicsServiceMock;
            private IDemographicService demographicsService;
            List<EmploymentClassification> EmploymentClassificationList;
            private string employmentClassificationsGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmploymentClassification, EmploymentClassification>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicsServiceMock = new Mock<IDemographicService>();
                demographicsService = demographicsServiceMock.Object;

                allEmploymentClassificationEntities = new TestEmploymentClassRepository().GetEmploymentClassifications();
                EmploymentClassificationList = new List<EmploymentClassification>();

                EmploymentClassificationsController = new EmploymentClassificationsController(demographicsService, logger);
                EmploymentClassificationsController.Request = new HttpRequestMessage();
                EmploymentClassificationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var employmentClassification in allEmploymentClassificationEntities)
                {
                    EmploymentClassification target = ConvertEmploymentClassificationEntityToDto(employmentClassification);
                    EmploymentClassificationList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(allEmploymentClassificationEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                EmploymentClassificationsController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetEmploymentClassificationsByGuidAsync_Validate()
            {
                var thisEmploymentClassification = EmploymentClassificationList.Where(m => m.Id == employmentClassificationsGuid).FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetEmploymentClassificationByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmploymentClassification);

                var employmentClassification = await EmploymentClassificationsController.GetEmploymentClassificationByIdAsync(employmentClassificationsGuid);
                Assert.AreEqual(thisEmploymentClassification.Id, employmentClassification.Id);
                Assert.AreEqual(thisEmploymentClassification.Code, employmentClassification.Code);
                Assert.AreEqual(thisEmploymentClassification.Description, employmentClassification.Description);
                Assert.AreEqual(thisEmploymentClassification.employeeClassificationType, employmentClassification.employeeClassificationType);
            }

            [TestMethod]
            public async Task EmploymentClassificationsController_GetHedmAsync()
            {
                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentClassificationList);

                var result = await EmploymentClassificationsController.GetEmploymentClassificationsAsync();
                Assert.AreEqual(result.Count(), allEmploymentClassificationEntities.Count());

                int count = allEmploymentClassificationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentClassificationList[i];
                    var actual = allEmploymentClassificationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentClassificationsController_GetHedmAsync_CacheControlNotNull()
            {
                EmploymentClassificationsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentClassificationList);

                var result = await EmploymentClassificationsController.GetEmploymentClassificationsAsync();
                Assert.AreEqual(result.Count(), allEmploymentClassificationEntities.Count());

                int count = allEmploymentClassificationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentClassificationList[i];
                    var actual = allEmploymentClassificationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentClassificationsController_GetHedmAsync_NoCache()
            {
                EmploymentClassificationsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmploymentClassificationsController.Request.Headers.CacheControl.NoCache = true;

                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentClassificationList);

                var result = await EmploymentClassificationsController.GetEmploymentClassificationsAsync();
                Assert.AreEqual(result.Count(), allEmploymentClassificationEntities.Count());

                int count = allEmploymentClassificationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentClassificationList[i];
                    var actual = allEmploymentClassificationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentClassificationsController_GetHedmAsync_Cache()
            {
                EmploymentClassificationsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmploymentClassificationsController.Request.Headers.CacheControl.NoCache = false;

                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentClassificationList);

                var result = await EmploymentClassificationsController.GetEmploymentClassificationsAsync();
                Assert.AreEqual(result.Count(), allEmploymentClassificationEntities.Count());

                int count = allEmploymentClassificationEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentClassificationList[i];
                    var actual = allEmploymentClassificationEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentClassificationsController_GetByIdHedmAsync()
            {
                var thisEmploymentClassification = EmploymentClassificationList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                demographicsServiceMock.Setup(x => x.GetEmploymentClassificationByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmploymentClassification);

                var employmentClassification = await EmploymentClassificationsController.GetEmploymentClassificationByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisEmploymentClassification.Id, employmentClassification.Id);
                Assert.AreEqual(thisEmploymentClassification.Code, employmentClassification.Code);
                Assert.AreEqual(thisEmploymentClassification.Description, employmentClassification.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentClassificationsController_GetThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationsAsync(It.IsAny<bool>())).Throws<Exception>();

                await EmploymentClassificationsController.GetEmploymentClassificationsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentClassificationsController_GetByIdThrowsIntAppiExc()
            {
                demographicsServiceMock.Setup(gc => gc.GetEmploymentClassificationByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await EmploymentClassificationsController.GetEmploymentClassificationByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentClassificationsController_PostThrowsIntAppiExc()
            {
                await EmploymentClassificationsController.PostEmploymentClassificationAsync(EmploymentClassificationList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentClassificationsController_PutThrowsIntAppiExc()
            {
                var result = await EmploymentClassificationsController.PutEmploymentClassificationAsync(EmploymentClassificationList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentClassificationsController_DeleteThrowsIntAppiExc()
            {
                await EmploymentClassificationsController.DeleteEmploymentClassificationAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a EmploymentClassification domain entity to its corresponding EmploymentClassification DTO
            /// </summary>
            /// <param name="source">EmploymentClassification domain entity</param>
            /// <returns>EmploymentClassification DTO</returns>
            private Ellucian.Colleague.Dtos.EmploymentClassification ConvertEmploymentClassificationEntityToDto(Ellucian.Colleague.Domain.Base.Entities.EmploymentClassification source)
            {
                var employmentClassification = new Ellucian.Colleague.Dtos.EmploymentClassification();
                employmentClassification.Id = source.Guid;
                employmentClassification.Code = source.Code;
                employmentClassification.Title = source.Description;
                employmentClassification.Description = null;

                return employmentClassification;
            }
        }
    }
}