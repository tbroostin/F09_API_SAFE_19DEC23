// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class CampusInvolvementsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<ICampusOrganizationService> campusOrganizationServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            CampusInvolvementsController campusInvolvementsController;
            List<Dtos.CampusInvolvement> campusInvolvementDtos;
            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                campusOrganizationServiceMock = new Mock<ICampusOrganizationService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                campusInvolvementDtos = BuildData();

                campusInvolvementsController = new CampusInvolvementsController(adapterRegistryMock.Object, campusOrganizationServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                campusInvolvementsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                campusInvolvementsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private List<Dtos.CampusInvolvement> BuildData()
            {
                List<Dtos.CampusInvolvement> campusInvolvements = new List<Dtos.CampusInvolvement>() 
                {
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a", 
                        InvolvementStartOn = new DateTime(2016, 09, 03), 
                        InvolvementEndOn = new DateTime(2016, 11, 30), 
                        InvolvementRole = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        PersonId = new Dtos.GuidObject2("e0c0c94c-53a7-46b7-96c4-76b12512c323")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("d190d4b5-03b5-41aa-99b8-b8286717c956"), 
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b", 
                        InvolvementStartOn = new DateTime(2016, 09, 01), 
                        InvolvementEndOn = new DateTime(2016, 11, 30),
                        InvolvementRole = new Dtos.GuidObject2("b90812ee-b573-4acb-88b0-6999a050be4f"), 
                        PersonId = new Dtos.GuidObject2("0bbb15f2-bb03-4056-bb9b-57a0ddf057ff")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873", 
                        InvolvementStartOn = new DateTime(2016, 04, 01), 
                        InvolvementEndOn = new DateTime(2016, 09, 30),  
                        InvolvementRole = null, 
                        PersonId = new Dtos.GuidObject2("0ac28907-5a9b-4102-a0d7-5d3d9c585512")
                    },
                    new Dtos.CampusInvolvement()
                    {
                        CampusOrganizationId = new Dtos.GuidObject2("cecdce5a-54a7-45fb-a975-5392a579e5bf"), 
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136", 
                        InvolvementStartOn = new DateTime(2016, 02, 01), 
                        InvolvementEndOn = new DateTime(2016, 06, 30), 
                        InvolvementRole = new Dtos.GuidObject2("f9871d1d-a7c0-4239-b4e3-6ee6b5bc9d52"), 
                        PersonId = new Dtos.GuidObject2("bb6c261c-3818-4dc3-b693-eb3e64d70d8b")
                    },
                };
                return campusInvolvements;
            }

            [TestCleanup]
            public void Cleanup()
            {
                campusInvolvementsController = null;
                campusInvolvementDtos = null;
                campusOrganizationServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task CampusInvolvementsController_GetAll_NoCache_True()
            {
                campusInvolvementsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementDtos, 4);
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var campusInvolvements = await campusInvolvementsController.GetCampusInvolvementsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await campusInvolvements.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.CampusInvolvement> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvement>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.CampusInvolvement>;


                Assert.AreEqual(campusInvolvementDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
                    Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                    Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
                    if (actual.InvolvementRole != null)
                    {
                        Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
                    }
                    Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
                    Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
                }
            }

            [TestMethod]
            public async Task CampusInvolvementsController_GetAll_NoCache_False()
            {
                campusInvolvementsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementDtos, 4);
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementsAsync(offset, limit, false)).ReturnsAsync(tuple);
                var campusInvolvements = await campusInvolvementsController.GetCampusInvolvementsAsync(new Paging(limit, offset));

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await campusInvolvements.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.CampusInvolvement> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvement>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.CampusInvolvement>;


                Assert.AreEqual(campusInvolvementDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
                    Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                    Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
                    if (actual.InvolvementRole != null)
                    {
                        Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
                    }
                    Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
                    Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
                }
            }

            [TestMethod]
            public async Task CampusInvolvementsController_GetAll_NullPage()
            {
                campusInvolvementsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementDtos, 4);
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementsAsync(offset, limit, true)).ReturnsAsync(tuple);
                var campusInvolvements = await campusInvolvementsController.GetCampusInvolvementsAsync(null);

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await campusInvolvements.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.CampusInvolvement> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvement>>)httpResponseMessage.Content)
                                                                .Value as IEnumerable<Dtos.CampusInvolvement>;


                Assert.AreEqual(campusInvolvementDtos.Count, actuals.Count());

                foreach (var actual in actuals)
                {
                    var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
                    Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                    Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
                    if (actual.InvolvementRole != null)
                    {
                        Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
                    }
                    Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
                    Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
                }
            }

            [TestMethod]
            public async Task CampusInvolvementsController_GetById()
            {
                var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
                var campusInvolvement = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementByGuidAsync(id)).ReturnsAsync(campusInvolvement);

                var actual = await campusInvolvementsController.GetCampusInvolvementByIdAsync(id);

                var expected = campusInvolvementDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod, actual.AcademicPeriod);
                Assert.AreEqual(expected.CampusOrganizationId, actual.CampusOrganizationId);
                Assert.AreEqual(expected.InvolvementEndOn, actual.InvolvementEndOn);
                if (actual.InvolvementRole != null)
                {
                    Assert.AreEqual(expected.InvolvementRole.Id, actual.InvolvementRole.Id);
                }
                Assert.AreEqual(expected.InvolvementStartOn, actual.InvolvementStartOn);
                Assert.AreEqual(expected.PersonId.Id, actual.PersonId.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusOrganizationTypesController_GetAll_Exception()
            {
                campusInvolvementsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var tuple = new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementDtos, 4);
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementsAsync(offset, limit, false)).ThrowsAsync(new Exception());
                var campusInvolvements = await campusInvolvementsController.GetCampusInvolvementsAsync(new Paging(limit, offset));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementsController_GetById_Exception()
            {
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actual = await campusInvolvementsController.GetCampusInvolvementByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementsController_GetById_KeyNotFoundException()
            {
                campusOrganizationServiceMock.Setup(ci => ci.GetCampusInvolvementByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await campusInvolvementsController.GetCampusInvolvementByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementsController_PUT_Not_Supported()
            {
                var actual = await campusInvolvementsController.PutCampusInvolvementAsync(It.IsAny<string>(), It.IsAny<Dtos.CampusInvolvement>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementsController_POST_Not_Supported()
            {
                var actual = await campusInvolvementsController.PostCampusInvolvementAsync(It.IsAny<Dtos.CampusInvolvement>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CampusInvolvementsController_DELETE_Not_Supported()
            {
                await campusInvolvementsController.DeleteCampusInvolvementAsync(It.IsAny<string>());
            }
        }
    }
}
