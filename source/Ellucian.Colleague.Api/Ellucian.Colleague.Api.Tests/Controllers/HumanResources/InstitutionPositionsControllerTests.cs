// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class InstitutionPositionsControllerTests
    {
        
        [TestClass]
        public class InstitutionPositionsControllerGetV7
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

            private InstitutionPositionsController institutionPositionsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IInstitutionPositionService> institutionPositionsService;
            List<Dtos.InstitutionPosition> institutionPositionDtoList;
            Tuple<IEnumerable<Dtos.InstitutionPosition>, int> institutionPositionDtoTuple;
            
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                institutionPositionsService = new Mock<IInstitutionPositionService>();

                BuildData();

                institutionPositionsController = new InstitutionPositionsController(logger, institutionPositionsService.Object);
                institutionPositionsController.Request = new HttpRequestMessage();
                institutionPositionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                institutionPositionsController = null;
                institutionPositionsService = null;
                institutionPositionDtoList = null;
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetAll_Async()
            {
               
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;

                int count = institutionPositionDtoList.Count;
                for (int i = 0; i < count; i++)
                {
                    var expected = institutionPositionDtoList[i];
                    var actual = institutionPositionResults[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    if(expected.AccountingStrings!= null)
                    {
                        for (int j = 0; j < actual.AccountingStrings.Count; j++)
                        {
                            var expectedAccountingString = expected.AccountingStrings[j];
                            var actualAccountingString = actual.AccountingStrings[j];
                            Assert.AreEqual(expectedAccountingString, actualAccountingString);
                        }
                    }

                    Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                    if(actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                    if(expected.Departments !=null)
                    {
                        for (int a = 0; a < actual.Departments.Count; a++)
                        {
                            var expectedDepartment = expected.Departments[a];
                            var actualDepartment = actual.Departments[a];

                            Assert.AreEqual(expectedDepartment.Detail.Id, actualDepartment.Detail.Id);
                            Assert.AreEqual(expectedDepartment.Name, actualDepartment.Name);
                        }
                    }
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                    Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                    Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                    if (actual.ReportsTo != null)
                    {
                        for (int b = 0; b < actual.ReportsTo.Count; b++)
                        {
                            var expectedReportsTo = expected.ReportsTo[b];
                            var actualReportsTo = actual.ReportsTo[b];

                            Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                            Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                        }
                    }

                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetyId_Async()
            {
                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
                
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.AccountingStrings != null)
                {
                    for (int j = 0; j < actual.AccountingStrings.Count; j++)
                    {
                        var expectedAccountingString = expected.AccountingStrings[j];
                        var actualAccountingString = actual.AccountingStrings[j];
                        Assert.AreEqual(expectedAccountingString, actualAccountingString);
                    }
                }

                Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                if (actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                if (expected.Departments != null)
                {
                    for (int a = 0; a < actual.Departments.Count; a++)
                    {
                        var expectedDepartment = expected.Departments[a];
                        var actualDepartment = actual.Departments[a];

                        Assert.AreEqual(expectedDepartment.Detail.Id, actualDepartment.Detail.Id);
                        Assert.AreEqual(expectedDepartment.Name, actualDepartment.Name);
                    }
                }
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                if (actual.ReportsTo != null)
                {
                    for (int b = 0; b < actual.ReportsTo.Count; b++)
                    {
                        var expectedReportsTo = expected.ReportsTo[b];
                        var actualReportsTo = actual.ReportsTo[b];

                        Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                        Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                    }
                }

                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetyId_Async_NoCache()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());

                Assert.IsNotNull(actual);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetAll_Async_ArgumentException()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>(), "", "invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetAll_Async_ArgumentException2()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>(), "", "frozen");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;
                Assert.AreEqual(0, institutionPositionResults.Count);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All_ArgumentException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());

                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;
                Assert.AreEqual(0, institutionPositionResults.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;
                Assert.AreEqual(0, institutionPositionResults.Count);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;
                Assert.AreEqual(0, institutionPositionResults.Count);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;
                Assert.AreEqual(0, institutionPositionResults.Count);

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_KeyNotFoundException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_ArgumentNullException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PostThrowsIntAppiExc()
            {
                await institutionPositionsController.CreateInstitutionPositionsAsync(It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PutThrowsIntAppiExc()
            {
                var result = await institutionPositionsController.UpdateInstitutionPositionsAsync(It.IsAny<string>(), It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_DeleteThrowsIntAppiExc()
            {
                await institutionPositionsController.DefaultDeleteInstitutionPositions(It.IsAny<string>());
            }

            private void BuildData()
            {
                institutionPositionDtoList = new List<InstitutionPosition>() 
                {
                    new InstitutionPosition()
                    {
                        Id = "f0b4ee37-a939-47bd-af01-60ea40c73b11", 
                        Title = "Assistant Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ReportsTo = new List<Dtos.DtoProperties.ReportsToDtoProperty>()
                        {
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("0541108f-086b-4887-a06d-7b96f47ba10e"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Primary
                            },
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("505dc2f7-3ad7-4d8b-8b55-e45c903173a0"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Alternative
                            }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 07, 01)
                    },
                    new InstitutionPosition()
                    {
                        Id = "2091962f-bc2b-44d9-af03-56f94c453475", 
                        Title = "Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 173.33m },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080m }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 08, 01)
                    },
                    new InstitutionPosition()
                    {
                        Id = "ce4d68f6-257d-4052-92c8-17eed0f088fa", 
                        Title = "Associate Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 09, 01)
                    },
                     new InstitutionPosition()
                    {
                        Id = "c1b91008-ba77-4b5b-8b77-84f5a7ae1632", 
                        Title = "Solfeggio Coach",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Music", Detail = new GuidObject2("cb50855a-efe7-44e6-87a4-2412fddbcd17") }
                        },
                        AccountingStrings = new List<string>(){"11-01-01-00-10408-51001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 40 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 1040 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.NonExempt,
                        StartOn = new DateTime(2012, 10, 01)
                    }
                };
                institutionPositionDtoTuple = new Tuple<IEnumerable<InstitutionPosition>, int>(institutionPositionDtoList, institutionPositionDtoList.Count());
            }

            //Permissions

            //Success
            //Get 7
            //GetInstitutionPositionsAsync

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositionsAsync_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsAsync" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get 7
            //GetInstitutionPositionsAsync

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositionsAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsAsync" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                                                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                                                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); 
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositionsAsync(It.IsAny<Paging>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Success
            //Get By Id 7
            //GetInstitutionPositionsByGuidAsync

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuidAsync_Permissions()
            {
                var expected = institutionPositionDtoList[0];
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get By Id 7
            //GetInstitutionPositionsByGuidAsync

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuidAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuidAsync" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositionsByGuidAsync(It.IsAny<string>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

        }

        [TestClass]
        public class InstitutionPositionsControllerGetV11
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

            private InstitutionPositionsController institutionPositionsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IInstitutionPositionService> institutionPositionsService;
            List<Dtos.InstitutionPosition> institutionPositionDtoList;
            Tuple<IEnumerable<Dtos.InstitutionPosition>, int> institutionPositionDtoTuple;
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                institutionPositionsService = new Mock<IInstitutionPositionService>();

                BuildData();

                institutionPositionsController = new InstitutionPositionsController(logger, institutionPositionsService.Object);
                institutionPositionsController.Request = new HttpRequestMessage();
                institutionPositionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                institutionPositionsController = null;
                institutionPositionsService = null;
                institutionPositionDtoList = null;
            }
            
            [TestMethod]
            public async Task InstitutionPositionsController_GetAll2_Async()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition>;

                int count = institutionPositionDtoList.Count;
                for (int i = 0; i < count; i++)
                {
                    var expected = institutionPositionDtoList[i];
                    var actual = institutionPositionResults[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    if (expected.AccountingStrings != null)
                    {
                        for (int j = 0; j < actual.AccountingStrings.Count; j++)
                        {
                            var expectedAccountingString = expected.AccountingStrings[j];
                            var actualAccountingString = actual.AccountingStrings[j];
                            Assert.AreEqual(expectedAccountingString, actualAccountingString);
                        }
                    }

                    Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                    if (actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                    if (expected.Departments != null)
                    {
                        for (int a = 0; a < actual.Departments.Count; a++)
                        {
                            var expectedDepartment = expected.Departments[a];
                            var actualDepartment = actual.Departments[a];

                            Assert.AreEqual(expectedDepartment.Detail.Id, actualDepartment.Detail.Id);
                            Assert.AreEqual(expectedDepartment.Name, actualDepartment.Name);
                        }
                    }
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                    Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                    Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                    if (actual.ReportsTo != null)
                    {
                        for (int b = 0; b < actual.ReportsTo.Count; b++)
                        {
                            var expectedReportsTo = expected.ReportsTo[b];
                            var actualReportsTo = actual.ReportsTo[b];

                            Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                            Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                        }
                    }

                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }
            
            [TestMethod]
            public async Task InstitutionPositionsController_GetyId2_Async()
            {
                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.AccountingStrings != null)
                {
                    for (int j = 0; j < actual.AccountingStrings.Count; j++)
                    {
                        var expectedAccountingString = expected.AccountingStrings[j];
                        var actualAccountingString = actual.AccountingStrings[j];
                        Assert.AreEqual(expectedAccountingString, actualAccountingString);
                    }
                }

                Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                if (actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                if (expected.Departments != null)
                {
                    for (int a = 0; a < actual.Departments.Count; a++)
                    {
                        var expectedDepartment = expected.Departments[a];
                        var actualDepartment = actual.Departments[a];

                        Assert.AreEqual(expectedDepartment.Detail.Id, actualDepartment.Detail.Id);
                        Assert.AreEqual(expectedDepartment.Name, actualDepartment.Name);
                    }
                }
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                if (actual.ReportsTo != null)
                {
                    for (int b = 0; b < actual.ReportsTo.Count; b++)
                    {
                        var expectedReportsTo = expected.ReportsTo[b];
                        var actualReportsTo = actual.ReportsTo[b];

                        Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                        Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                    }
                }

                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetyId2_Async_NoCache()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());

                Assert.IsNotNull(actual);
            }
         
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All2_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All2_ArgumentException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All2_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All2_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All2_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_KeyNotFoundException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_ArgumentNullException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }
           
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById2_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PostThrowsIntAppiExc()
            {
                await institutionPositionsController.CreateInstitutionPositionsAsync(It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PutThrowsIntAppiExc()
            {
                var result = await institutionPositionsController.UpdateInstitutionPositionsAsync(It.IsAny<string>(), It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_DeleteThrowsIntAppiExc()
            {
                await institutionPositionsController.DefaultDeleteInstitutionPositions(It.IsAny<string>());
            }

            private void BuildData()
            {
                institutionPositionDtoList = new List<InstitutionPosition>() 
                {
                    new InstitutionPosition()
                    {
                        Id = "f0b4ee37-a939-47bd-af01-60ea40c73b11", 
                        Title = "Assistant Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ReportsTo = new List<Dtos.DtoProperties.ReportsToDtoProperty>()
                        {
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("0541108f-086b-4887-a06d-7b96f47ba10e"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Primary
                            },
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("505dc2f7-3ad7-4d8b-8b55-e45c903173a0"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Alternative
                            }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 07, 01)
                    },
                    new InstitutionPosition()
                    {
                        Id = "2091962f-bc2b-44d9-af03-56f94c453475", 
                        Title = "Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 173.33m },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080m }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 08, 01)
                    },
                    new InstitutionPosition()
                    {
                        Id = "ce4d68f6-257d-4052-92c8-17eed0f088fa", 
                        Title = "Associate Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Records and Registration", Detail = new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") }
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 09, 01)
                    },
                     new InstitutionPosition()
                    {
                        Id = "c1b91008-ba77-4b5b-8b77-84f5a7ae1632", 
                        Title = "Solfeggio Coach",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<Dtos.DtoProperties.NameDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.NameDetailDtoProperty(){ Name = "Music", Detail = new GuidObject2("cb50855a-efe7-44e6-87a4-2412fddbcd17") }
                        },
                        AccountingStrings = new List<string>(){"11-01-01-00-10408-51001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 40 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 1040 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.NonExempt,
                        StartOn = new DateTime(2012, 10, 01)
                    }
                };
                institutionPositionDtoTuple = new Tuple<IEnumerable<InstitutionPosition>, int>(institutionPositionDtoList, institutionPositionDtoList.Count());
            }

            //Permissions

            //Success
            //Get 11
            //GetInstitutionPositions2Async

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositions2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositions2Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get 11
            //GetInstitutionPositions2Async

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositions2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositions2Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositions2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositions2Async(It.IsAny<Paging>(), criteriaFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Success
            //Get By Id 11
            //GetInstitutionPositionsByGuid2Async

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuid2Async_Permissions()
            {
                var expected = institutionPositionDtoList[0];
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get By Id 11
            //GetInstitutionPositionsByGuid2Async

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuid2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuid2Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid2Async(It.IsAny<string>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


        }

        [TestClass]
        public class InstitutionPositionsControllerGetV12
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

            private InstitutionPositionsController institutionPositionsController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IInstitutionPositionService> institutionPositionsService;
            List<Dtos.InstitutionPosition2> institutionPositionDtoList;
            Tuple<IEnumerable<Dtos.InstitutionPosition2>, int> institutionPositionDtoTuple;

            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                institutionPositionsService = new Mock<IInstitutionPositionService>();

                BuildData();

                institutionPositionsController = new InstitutionPositionsController(logger, institutionPositionsService.Object);
                institutionPositionsController.Request = new HttpRequestMessage();
                institutionPositionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                institutionPositionsController = null;
                institutionPositionsService = null;
                institutionPositionDtoList = null;
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetAll3_Async()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
                Assert.IsNotNull(actuals);

                var cancelToken = new System.Threading.CancellationToken(false);
                System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
                List<Dtos.InstitutionPosition2> institutionPositionResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition2>>)httpResponseMessage.Content)
                                                                            .Value as List<Dtos.InstitutionPosition2>;

                int count = institutionPositionDtoList.Count;
                for (int i = 0; i < count; i++)
                {
                    var expected = institutionPositionDtoList[i];
                    var actual = institutionPositionResults[i];

                    Assert.AreEqual(expected.Id, actual.Id);
                    if (expected.AccountingStrings != null)
                    {
                        for (int j = 0; j < actual.AccountingStrings.Count; j++)
                        {
                            var expectedAccountingString = expected.AccountingStrings[j];
                            var actualAccountingString = actual.AccountingStrings[j];
                            Assert.AreEqual(expectedAccountingString, actualAccountingString);
                        }
                    }

                    Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                    if (actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                    if (expected.Departments != null)
                    {
                        for (int a = 0; a < actual.Departments.Count; a++)
                        {
                            var expectedDepartment = expected.Departments[a];
                            var actualDepartment = actual.Departments[a];

                            Assert.AreEqual(expectedDepartment.Id, actualDepartment.Id);
                        }
                    }
                    Assert.AreEqual(expected.Description, actual.Description);
                    Assert.AreEqual(expected.EndOn, actual.EndOn);
                    Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                    Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                    Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                    if (actual.ReportsTo != null)
                    {
                        for (int b = 0; b < actual.ReportsTo.Count; b++)
                        {
                            var expectedReportsTo = expected.ReportsTo[b];
                            var actualReportsTo = actual.ReportsTo[b];

                            Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                            Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                        }
                    }

                    Assert.AreEqual(expected.StartOn, actual.StartOn);
                    Assert.AreEqual(expected.Status, actual.Status);
                    Assert.AreEqual(expected.Title, actual.Title);
                }
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetyId3_Async()
            {
                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.AccountingStrings != null)
                {
                    for (int j = 0; j < actual.AccountingStrings.Count; j++)
                    {
                        var expectedAccountingString = expected.AccountingStrings[j];
                        var actualAccountingString = actual.AccountingStrings[j];
                        Assert.AreEqual(expectedAccountingString, actualAccountingString);
                    }
                }

                Assert.AreEqual(expected.AuthorizedOn, actual.AuthorizedOn);
                if (actual.Campus != null) Assert.AreEqual(expected.Campus.Id, actual.Campus.Id);
                if (expected.Departments != null)
                {
                    for (int a = 0; a < actual.Departments.Count; a++)
                    {
                        var expectedDepartment = expected.Departments[a];
                        var actualDepartment = actual.Departments[a];

                        Assert.AreEqual(expectedDepartment.Id, actualDepartment.Id);
                    }
                }
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.EndOn, actual.EndOn);
                Assert.AreEqual(expected.ExemptionType, actual.ExemptionType);
                Assert.AreEqual(expected.FullTimeEquivalent, actual.FullTimeEquivalent);
                Assert.AreEqual(expected.HoursPerPeriod, actual.HoursPerPeriod);

                if (actual.ReportsTo != null)
                {
                    for (int b = 0; b < actual.ReportsTo.Count; b++)
                    {
                        var expectedReportsTo = expected.ReportsTo[b];
                        var actualReportsTo = actual.ReportsTo[b];

                        Assert.AreEqual(expectedReportsTo.Postition.Id, actualReportsTo.Postition.Id);
                        Assert.AreEqual(expectedReportsTo.Type, actualReportsTo.Type);
                    }
                }

                Assert.AreEqual(expected.StartOn, actual.StartOn);
                Assert.AreEqual(expected.Status, actual.Status);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            public async Task InstitutionPositionsController_GetyId3_Async_NoCache()
            {
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                institutionPositionsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var expected = institutionPositionDtoList[0];
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());

                Assert.IsNotNull(actual);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All3_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All3_ArgumentException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All3_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All3_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_All3_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_PermissionsException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_KeyNotFoundException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_ArgumentNullException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_RepositoryException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_IntegrationApiException()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GET_ById3_Exception()
            {
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PostThrowsIntAppiExc()
            {
                await institutionPositionsController.CreateInstitutionPositionsAsync(It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_PutThrowsIntAppiExc()
            {
                var result = await institutionPositionsController.UpdateInstitutionPositionsAsync(It.IsAny<string>(), It.IsAny<Dtos.InstitutionPosition>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_DeleteThrowsIntAppiExc()
            {
                await institutionPositionsController.DefaultDeleteInstitutionPositions(It.IsAny<string>());
            }

            private void BuildData()
            {
                institutionPositionDtoList = new List<InstitutionPosition2>() 
                {
                    new InstitutionPosition2()
                    {
                        Id = "f0b4ee37-a939-47bd-af01-60ea40c73b11", 
                        Title = "Assistant Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<GuidObject2>()
                        {
                            new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9") 
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ReportsTo = new List<Dtos.DtoProperties.ReportsToDtoProperty>()
                        {
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("0541108f-086b-4887-a06d-7b96f47ba10e"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Primary
                            },
                            new Dtos.DtoProperties.ReportsToDtoProperty()
                            {
                                Postition = new GuidObject2("505dc2f7-3ad7-4d8b-8b55-e45c903173a0"),
                                Type = Dtos.EnumProperties.PositionReportsToType.Alternative
                            }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 07, 01)
                    },
                    new InstitutionPosition2()
                    {
                        Id = "2091962f-bc2b-44d9-af03-56f94c453475", 
                        Title = "Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<GuidObject2>()
                        {
                            new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9")
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 173.33m },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080m }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 08, 01)
                    },
                    new InstitutionPosition2()
                    {
                        Id = "ce4d68f6-257d-4052-92c8-17eed0f088fa", 
                        Title = "Associate Registrar",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<GuidObject2>()
                        {
                            new GuidObject2("dc93225d-5319-4779-8cf9-5ae1412e10d9")
                        },
                        AccountingStrings = new List<string>(){"11-00-02-62-40110-52001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 80 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 2080 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.Exempt,
                        StartOn = new DateTime(2012, 09, 01)
                    },
                     new InstitutionPosition2()
                    {
                        Id = "c1b91008-ba77-4b5b-8b77-84f5a7ae1632", 
                        Title = "Solfeggio Coach",
                        Campus = new GuidObject2("69d4639c-f9d0-4393-adaf-b1287b71525e"),
                        Departments = new List<GuidObject2>()
                        {
                            new GuidObject2("cb50855a-efe7-44e6-87a4-2412fddbcd17")
                        },
                        AccountingStrings = new List<string>(){"11-01-01-00-10408-51001"},
                        Status = Dtos.EnumProperties.PositionStatus.Active,
                        HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                        {
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.PayPeriod, Hours = 40 },
                            new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Period = Dtos.EnumProperties.PayPeriods.Year, Hours = 1040 }
                        },
                        ExemptionType = Dtos.EnumProperties.ExemptionType.NonExempt,
                        StartOn = new DateTime(2012, 10, 01)
                    }
                };
                institutionPositionDtoTuple = new Tuple<IEnumerable<InstitutionPosition2>, int>(institutionPositionDtoList, institutionPositionDtoList.Count());
            }

            //Permissions

            //Success
            //Get 12
            //GetInstitutionPositions3Async

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositions3Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositions3Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionPositionDtoTuple);
                var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get 12
            //GetInstitutionPositions3Async

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositions3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositions3Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositions3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositions3Async(It.IsAny<Paging>(), criteriaFilter);
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //Success
            //Get By Id 12
            //GetInstitutionPositionsByGuid3Async

            [TestMethod]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuid3Async_Permissions()
            {
                var expected = institutionPositionDtoList[0];
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);
                institutionPositionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(HumanResourcesPermissionCodes.ViewInstitutionPosition);

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(institutionPositionDtoList, 4);
                institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expected);
                var actual = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());

                Object filterObject;
                institutionPositionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(HumanResourcesPermissionCodes.ViewInstitutionPosition));
            }

            //Exception
            //Get By Id 12
            //GetInstitutionPositionsByGuid3Async

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionPositionsController_GetInstitutionPositionsByGuid3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "InstitutionPositions" },
                    { "action", "GetInstitutionPositionsByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("institution-positions", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                institutionPositionsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = institutionPositionsController.ControllerContext;
                var actionDescriptor = institutionPositionsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    var tuple = new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(institutionPositionDtoList, 5);

                    institutionPositionsService.Setup(x => x.GetInstitutionPositionByGuid3Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    institutionPositionsService.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view institution-positions."));
                    var actuals = await institutionPositionsController.GetInstitutionPositionsByGuid3Async(It.IsAny<string>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }



        }
    }
}