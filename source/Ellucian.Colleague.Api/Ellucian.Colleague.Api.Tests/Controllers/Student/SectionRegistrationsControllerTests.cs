// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Web.Http.Models;
using System.Web.Http.Hosting;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.Filters;
using System.Net;
using Newtonsoft.Json;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using System.Collections;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Http.Filters;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionRegistrationControllerTests
    {
        [TestClass]
        public class Get
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrations3Dtos;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration4> allSectionRegistrations4Dtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                sectionRegistrationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                allSectionRegistrationsDtos = BuildSectionRegistrations();
                allSectionRegistrations3Dtos = BuildSectionRegistrations3();
                allSectionRegistrations4Dtos = BuildSectionRegistrations4();
                string guid = allSectionRegistrationsDtos.ElementAt(0).Id;

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
                allSectionRegistrationsDtos = null;
                allSectionRegistrations3Dtos = null;
                allSectionRegistrations4Dtos = null;
            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationsAsync()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(allSectionRegistrationsDtos, 5);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrationsAsync(new Paging(10, 0), "", "");
               
                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(sectionRegistrations is IHttpActionResult);

                foreach (var sectionRegistrationsDto in allSectionRegistrationsDtos)
                {
                    var sectReg = results.FirstOrDefault(i => i.Id == sectionRegistrationsDto.Id);

                    Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                    Assert.AreEqual(sectionRegistrationsDto.AwardGradeScheme, sectReg.AwardGradeScheme);
                    Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);
                    
                }
            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations2Async()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(allSectionRegistrations3Dtos, 5);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations2Async(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(sectionRegistrations is IHttpActionResult);

                foreach (var sectionRegistrationsDto in allSectionRegistrations3Dtos)
                {
                    var sectReg = results.FirstOrDefault(i => i.Id == sectionRegistrationsDto.Id);

                    Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                    Assert.AreEqual(sectionRegistrationsDto.AwardGradeScheme, sectReg.AwardGradeScheme);
                    Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);

                }
            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync()
            {
                string guid = allSectionRegistrationsDtos.ElementAt(0).Id;
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationAsync(guid)).Returns(Task.FromResult(allSectionRegistrationsDtos.ElementAt(0)));
                var sectionRegistration = await sectionRegistrationsController.GetSectionRegistrationAsync(guid);
                Assert.AreEqual(sectionRegistration.Id, allSectionRegistrationsDtos.ElementAt(0).Id);
                Assert.AreEqual(sectionRegistration.AwardGradeScheme.Id, allSectionRegistrationsDtos.ElementAt(0).AwardGradeScheme.Id);
                Assert.AreEqual(sectionRegistration.Section.Id, allSectionRegistrationsDtos.ElementAt(0).Section.Id);
            }

            //V16.0.0
            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations4Async()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionRegistrationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var filterGroupName = "criteria";
                var academicPeriodFilter = "academicPeriod";
                var sectionInstructorFilter = "sectionInstructor";
                sectionRegistrationsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.SectionRegistration4() {  Registrant = new GuidObject2("f0deb5c6-7e3e-450b-a861-ea07e7340a97"), Section = new GuidObject2("b6e6edae-df08-4081-aa24-c8442fa5fb2d") });

                sectionRegistrationsController.Request.Properties.Add(string.Format("FilterObject{0}", academicPeriodFilter),
                    new AcademicPeriodNamedQueryFilter() { AcademicPeriod = new GuidObject2("f0deb5c6-7e3e-450b-a861-ea07e7340a97") });

                sectionRegistrationsController.Request.Properties.Add(string.Format("FilterObject{0}", sectionInstructorFilter),
                    new SectionInstructorQueryFilter() { SectionInstructorId = new GuidObject2("f0deb5c6-7e3e-450b-a861-ea07e7340a97") });

                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(allSectionRegistrations4Dtos, 2);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(),
                    It.IsAny<bool>())).ReturnsAsync(tuple);

                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration4> results = 
                    ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration4>;

                Assert.IsTrue(sectionRegistrations is IHttpActionResult);
                Assert.AreEqual(allSectionRegistrations4Dtos.Count(), results.Count());

                foreach (var sectionRegistrationsDto in allSectionRegistrations4Dtos)
                {
                    var sectReg = results.FirstOrDefault(i => i.Id == sectionRegistrationsDto.Id);

                    Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);
                    Assert.AreEqual(sectionRegistrationsDto.AcademicLevel.Id, sectReg.AcademicLevel.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Approval.Count(), sectReg.Approval.Count());
                    Assert.AreEqual(sectionRegistrationsDto.Credit.Measure.Value, sectReg.Credit.Measure.Value);
                    Assert.AreEqual(sectionRegistrationsDto.Credit.RegistrationCredit.Value, sectReg.Credit.RegistrationCredit.Value);
                    Assert.AreEqual(sectionRegistrationsDto.GradingOption.GradeScheme.Id, sectReg.GradingOption.GradeScheme.Id);
                    Assert.AreEqual(sectionRegistrationsDto.GradingOption.Mode, sectReg.GradingOption.Mode);
                    Assert.AreEqual(sectionRegistrationsDto.Involvement.EndOn, sectReg.Involvement.EndOn);
                    Assert.AreEqual(sectionRegistrationsDto.Involvement.StartOn, sectReg.Involvement.StartOn);
                    Assert.AreEqual(sectionRegistrationsDto.OriginallyRegisteredOn, sectReg.OriginallyRegisteredOn);
                    Assert.AreEqual(sectionRegistrationsDto.Override.AcademicPeriod.Id, sectReg.Override.AcademicPeriod.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Override.Site.Id, sectReg.Override.Site.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Registrant.Id, sectReg.Registrant.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Status.Detail.Id, sectReg.Status.Detail.Id);
                    Assert.AreEqual(sectionRegistrationsDto.Status.RegistrationStatus, sectReg.Status.RegistrationStatus);
                    Assert.AreEqual(sectionRegistrationsDto.Status.SectionRegistrationStatusReason, sectReg.Status.SectionRegistrationStatusReason);
                    Assert.AreEqual(sectionRegistrationsDto.StatusDate, sectReg.StatusDate);
                }
            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations4Async_EmptyFilterParams()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionRegistrationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var filterGroupName = "criteria";
                sectionRegistrationsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.SectionRegistration4() { Registrant = new GuidObject2("") });
                //EmptyFilterProperties
                sectionRegistrationsController.Request.Properties.Add("EmptyFilterProperties", true);


                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(allSectionRegistrations4Dtos, 2);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(),
                     It.IsAny<bool>())).ReturnsAsync(tuple);

                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), 
                    It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration4> results =
                    ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration4>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration4>;

            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                sectionRegistrationsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var guid = "1d95b329-edbe-4420-909a-df57a962a30c";

                var sectionRegistrationsDto = allSectionRegistrations4Dtos.ElementAt(0);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationByGuid3Async(guid, It.IsAny<bool>())).ReturnsAsync(sectionRegistrationsDto);

                var sectReg = await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("1d95b329-edbe-4420-909a-df57a962a30c");

                Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);
                Assert.AreEqual(sectionRegistrationsDto.AcademicLevel.Id, sectReg.AcademicLevel.Id);
                Assert.AreEqual(sectionRegistrationsDto.Approval.Count(), sectReg.Approval.Count());
                Assert.AreEqual(sectionRegistrationsDto.Credit.Measure.Value, sectReg.Credit.Measure.Value);
                Assert.AreEqual(sectionRegistrationsDto.Credit.RegistrationCredit.Value, sectReg.Credit.RegistrationCredit.Value);
                Assert.AreEqual(sectionRegistrationsDto.GradingOption.GradeScheme.Id, sectReg.GradingOption.GradeScheme.Id);
                Assert.AreEqual(sectionRegistrationsDto.GradingOption.Mode, sectReg.GradingOption.Mode);
                Assert.AreEqual(sectionRegistrationsDto.Involvement.EndOn, sectReg.Involvement.EndOn);
                Assert.AreEqual(sectionRegistrationsDto.Involvement.StartOn, sectReg.Involvement.StartOn);
                Assert.AreEqual(sectionRegistrationsDto.OriginallyRegisteredOn, sectReg.OriginallyRegisteredOn);
                Assert.AreEqual(sectionRegistrationsDto.Override.AcademicPeriod.Id, sectReg.Override.AcademicPeriod.Id);
                Assert.AreEqual(sectionRegistrationsDto.Override.Site.Id, sectReg.Override.Site.Id);
                Assert.AreEqual(sectionRegistrationsDto.Registrant.Id, sectReg.Registrant.Id);
                Assert.AreEqual(sectionRegistrationsDto.Status.Detail.Id, sectReg.Status.Detail.Id);
                Assert.AreEqual(sectionRegistrationsDto.Status.RegistrationStatus, sectReg.Status.RegistrationStatus);
                Assert.AreEqual(sectionRegistrationsDto.Status.SectionRegistrationStatusReason, sectReg.Status.SectionRegistrationStatusReason);
                Assert.AreEqual(sectionRegistrationsDto.StatusDate, sectReg.StatusDate);

            }

            #region Exception Tests

            //V16.0.0
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrations3Async_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), 
                    It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrations3Async_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                    It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrations3Async_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),
                    It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
            }

            //V16.0.0 GET By Id Exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrations3Async()
            {                
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrations3Async()
            {
                await sectionRegistrationsController.PutSectionRegistrations3Async(It.IsAny<string>(), It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            //GET v16.1.0 / v16.0.0
            //Successful
            //GetSectionRegistrations3Async

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations3Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations });

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(allSectionRegistrations4Dtos, 2);

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It
                                    .IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(),
                                    It.IsAny<bool>())).ReturnsAsync(tuple); 
                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.ViewRegistrations));
                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));

            }

            //GET v16.1.0 / v16.0.0
            //Exception
            //GetSectionRegistrations3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrations3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                    
                    sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SectionRegistration4>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RegistrationStatusesByAcademicPeriodFilter>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException()); 
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view section-registrations."));
                    await sectionRegistrationsController.GetSectionRegistrations3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>(),It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET by id v16.1.0 / v16.0.0
            //Successful
            //GetSectionRegistrationByGuid3Async

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_Permissions()
            {
                var guid = "1d95b329-edbe-4420-909a-df57a962a30c";
                var sectionRegistrationsDto = allSectionRegistrations4Dtos.ElementAt(0);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrationByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations });

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(allSectionRegistrations4Dtos, 2);
                
                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationByGuid3Async(guid, It.IsAny<bool>())).ReturnsAsync(sectionRegistrationsDto);

                var sectReg = await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("1d95b329-edbe-4420-909a-df57a962a30c");

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.ViewRegistrations));
                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));

            }

            //GET by id v16.1.0 / v16.0.0
            //Exception
            //GetSectionRegistrationByGuid3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationByGuid3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrationByGuid3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationByGuid3Async("GUID", It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view section-registrations."));
                    await sectionRegistrationsController.GetSectionRegistrationByGuid3Async("GUID");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET and GET by id v7
            //Successful
            //GetSectionRegistrations2Async

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations2Async_Permissions()
            {
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrations2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations });

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(allSectionRegistrations3Dtos, 5);

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);

                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations2Async(new Paging(10, 0), "", "");

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.ViewRegistrations));
                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));

            }

            //GET and GET by id v7
            //Exception
            //GetSectionRegistrations2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrations2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrations2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view section-registrations."));
                    await sectionRegistrationsController.GetSectionRegistrations2Async(It.IsAny<Paging>(), "", "");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            //GET and GET by id v6
            //Successful
            //GetSectionRegistrationAsync

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_Permissions()
            {
                string guid = allSectionRegistrationsDtos.ElementAt(0).Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(new string[] { SectionPermissionCodes.ViewRegistrations, SectionPermissionCodes.UpdateRegistrations });

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration4>, int>(allSectionRegistrations4Dtos, 2);

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationAsync(guid)).Returns(Task.FromResult(allSectionRegistrationsDtos.ElementAt(0)));

                var sectionRegistration = await sectionRegistrationsController.GetSectionRegistrationAsync(guid);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.ViewRegistrations));
                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));

            }

            //GET and GET by id v6
            //Exception
            //GetSectionRegistrationAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "GetSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationAsync("asdf")).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to view section-registrations."));
                    await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion

        }

        [TestClass]
        public class Put
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations();
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionRegistrationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(registration));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task UpdatesSectionRegistrationByGuid()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistrationAsync(guid, It.IsAny<SectionRegistration2>())).ReturnsAsync(registration);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationAsync(guid)).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistrationAsync(guid, registration);
                Assert.AreEqual(result.Id, registration.Id);
                Assert.AreEqual(result.Registrant, registration.Registrant);
                Assert.AreEqual(result.Section, registration.Section);
                Assert.AreEqual(result.Status.RegistrationStatus, registration.Status.RegistrationStatus);
            }                   
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            //PUT v6
            //Successful
            //PutSectionRegistrationAsync

            [TestMethod]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Permissions()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistrationAsync(guid, It.IsAny<SectionRegistration2>())).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistrationAsync(guid, registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));


            }

            //PUT v6
            //Exception
            //PutSectionRegistrationAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update section-registrations."));
                    await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion

        }

        [TestClass]
        public class Put_V7
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations3();
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionRegistrationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(registration));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task UpdatesSectionRegistrationByGuid()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistration2Async(guid, It.IsAny<SectionRegistration3>())).ReturnsAsync(registration);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistration2Async(guid)).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistration2Async(guid, registration);
                Assert.AreEqual(result.Id, registration.Id);
                Assert.AreEqual(result.Registrant, registration.Registrant);
                Assert.AreEqual(result.Section, registration.Section);
                Assert.AreEqual(result.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(result.AcademicLevel, registration.AcademicLevel);
            }
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
            }

            //PUT v7
            //Successful
            //PutSectionRegistration2Async

            [TestMethod]
            public async Task SectionRegistrationsController_PutSectionRegistration2Async_Permissions()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistration2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistration2Async(guid, It.IsAny<SectionRegistration3>())).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistration2Async(guid, registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));


            }

            //PUT v7
            //Exception
            //PutSectionRegistration2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistration2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistration2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.UpdateSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update section-registrations."));
                    await sectionRegistrationsController.PutSectionRegistration2Async("asdf", It.IsAny<SectionRegistration3>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }

            #endregion

        }

        [TestClass]
        public class Put_V16_0_0
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration4> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations4();
                Dtos.SectionRegistration4 registration = allSectionRegistrationsDtos.ElementAt(0);

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionRegistrationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(registration));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task UpdatesSectionRegistrationByGuid()
            {
                Dtos.SectionRegistration4 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistration3Async(guid, It.IsAny<SectionRegistration4>())).ReturnsAsync(registration);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationByGuid3Async(guid, false)).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistrations3Async(guid, registration);
                Assert.AreEqual(result.Id, registration.Id);
                Assert.AreEqual(result.Registrant, registration.Registrant);
                Assert.AreEqual(result.Section, registration.Section);
                Assert.AreEqual(result.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(result.AcademicLevel, registration.AcademicLevel);
            }
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
            }

            //PUT v16.0.0 / v16.1.0
            //Successful
            //PutSectionRegistrations3Async

            [TestMethod]
            public async Task SectionRegistrationsController_PutSectionRegistrations3Async_Permissions()
            {
                Dtos.SectionRegistration4 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                
                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistration3Async(guid, It.IsAny<SectionRegistration4>())).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistrations3Async(guid, registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));


            }

            //PUT v16.0.0 / v16.1.0
            //Exception
            //PutSectionRegistrations3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrations3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PutSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.UpdateSectionRegistration3Async("asdf", It.IsAny<SectionRegistration4>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to update section-registrations."));
                    await sectionRegistrationsController.PutSectionRegistrations3Async("asdf", It.IsAny<SectionRegistration4>());
                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion

        }

        [TestClass]
        public class Post
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations();

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task CreatesSectionRegistration()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistrationAsync(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));

                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistrationAsync(registration);
                Assert.AreEqual(sectionRegistration.Id, registration.Id);
                Assert.AreEqual(sectionRegistration.Registrant, registration.Registrant);
                Assert.AreEqual(sectionRegistration.Section, registration.Section);
                Assert.AreEqual(sectionRegistration.Status.RegistrationStatus, registration.Status.RegistrationStatus);
            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_FormatException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new FormatException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            //POST v6
            //Successful
            //PostSectionRegistrationAsync

            [TestMethod]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Permissions()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistrationAsync(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));
                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistrationAsync(registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));
            }

            //POST v6
            //Exception
            //PostSectionRegistrationAsync
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistrationAsync" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create section-registrations."));
                    await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion
        }

        [TestClass]
        public class PostV7
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations3();

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task CreatesSectionRegistration()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistration2Async(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));

                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistration2Async(registration);
                Assert.AreEqual(sectionRegistration.Id, registration.Id);
                Assert.AreEqual(sectionRegistration.Registrant, registration.Registrant);
                Assert.AreEqual(sectionRegistration.Section, registration.Section);
                Assert.AreEqual(sectionRegistration.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(sectionRegistration.AcademicLevel, registration.AcademicLevel);

            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_FormatException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new FormatException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());
            }

            //POST v7
            //Successful
            //PostSectionRegistration2Async

            [TestMethod]
            public async Task SectionRegistrationsController_PostSectionRegistration2Async_Permissions()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistration2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistration2Async(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));
                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistration2Async(registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));
            }

            //POST v7
            //Exception
            //PostSectionRegistration2Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistration2Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistration2Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                   
                    sectionRegistrationServiceMock.Setup(s => s.CreateSectionRegistration2Async(It.IsAny<SectionRegistration3>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create section-registrations."));
                    await sectionRegistrationsController.PostSectionRegistration2Async(It.IsAny<SectionRegistration3>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion
        }

        [TestClass]
        public class PostV16_0_0
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration4> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations4();

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task CreatesSectionRegistration()
            {
                Dtos.SectionRegistration4 registration = allSectionRegistrationsDtos.ElementAt(0);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistration3Async(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));

                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistrations3Async(registration);
                Assert.AreEqual(sectionRegistration.Id, registration.Id);
                Assert.AreEqual(sectionRegistration.Registrant, registration.Registrant);
                Assert.AreEqual(sectionRegistration.Section, registration.Section);
                Assert.AreEqual(sectionRegistration.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(sectionRegistration.AcademicLevel, registration.AcademicLevel);

            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_FormatException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new FormatException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());
            }

            //POST v16.0.0 / v16.1.0
            //Successful
            //PostSectionRegistrations3Async

            [TestMethod]
            public async Task SectionRegistrationsController_PostSectionRegistrations3Async_Permissions()
            {
                Dtos.SectionRegistration4 registration = allSectionRegistrationsDtos.ElementAt(0);
                var contextPropertyName = "PermissionsFilter";

                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var permissionsFilter = new PermissionsFilter(SectionPermissionCodes.UpdateRegistrations);

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistration3Async(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));
                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistrations3Async(registration);

                Object filterObject;
                sectionRegistrationsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
                var cancelToken = new System.Threading.CancellationToken(false);
                Assert.IsNotNull(filterObject);

                var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                     .Select(x => x.ToString())
                                     .ToArray();

                Assert.IsTrue(permissionsCollection.Contains(SectionPermissionCodes.UpdateRegistrations));
            }

            //POST v16.0.0 / v16.1.0
            //Exception
            //PostSectionRegistrations3Async
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrations3Async_Invalid_Permissions()
            {
                HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "SectionRegistrations" },
                    { "action", "PostSectionRegistrations3Async" }
                };
                HttpRoute route = new HttpRoute("section-registrations", routeValueDict);
                HttpRouteData data = new HttpRouteData(route);
                sectionRegistrationsController.Request.SetRouteData(data);

                var permissionsFilter = new PermissionsFilter("invalid");

                var controllerContext = sectionRegistrationsController.ControllerContext;
                var actionDescriptor = sectionRegistrationsController.ActionContext.ActionDescriptor
                         ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

                var _context = new HttpActionContext(controllerContext, actionDescriptor);
                try
                {
                    await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                    sectionRegistrationServiceMock.Setup(s => s.CreateSectionRegistration3Async(It.IsAny<SectionRegistration4>())).ThrowsAsync(new PermissionsException());
                    sectionRegistrationServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                        .Throws(new PermissionsException("User 'npuser' does not have permission to create section-registrations."));
                    await sectionRegistrationsController.PostSectionRegistrations3Async(It.IsAny<SectionRegistration4>());

                }
                catch (PermissionsException ex)
                {
                    throw ex;
                }
            }


            #endregion
        }

        internal static IEnumerable<Dtos.SectionRegistration2> BuildSectionRegistrations()
        {
            var sectionRegistrationDtos = new List<Dtos.SectionRegistration2>();

            var sectionRegistrationDto1 = new Dtos.SectionRegistration2()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.Registered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            var sectionRegistrationDto2 = new Dtos.SectionRegistration2()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            sectionRegistrationDtos.Add(sectionRegistrationDto1);
            sectionRegistrationDtos.Add(sectionRegistrationDto2);

            return sectionRegistrationDtos;
        }
        internal static IEnumerable<Dtos.SectionRegistration3> BuildSectionRegistrations3()
        {
            var sectionRegistrationDtos = new List<Dtos.SectionRegistration3>();

            var sectionRegistrationDto1 = new Dtos.SectionRegistration3()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.Registered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                AcademicLevel = new GuidObject2(){ Id = "qwertyuiop" },
            };

            var sectionRegistrationDto2 = new Dtos.SectionRegistration3()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                AcademicLevel = new GuidObject2() { Id = "qwertyuiop" },
            };

            sectionRegistrationDtos.Add(sectionRegistrationDto1);
            sectionRegistrationDtos.Add(sectionRegistrationDto2);

            return sectionRegistrationDtos;
        }

        internal static IEnumerable<Dtos.SectionRegistration4> BuildSectionRegistrations4()
        {
            var sectionRegistrationDtos = new List<Dtos.SectionRegistration4>();

            var sectionRegistrationDto1 = new Dtos.SectionRegistration4()
            {
                Id = "1d95b329-edbe-4420-909a-df57a962a30c",
                Section = new Dtos.GuidObject2() { Id = "b6e6edae-df08-4081-aa24-c8442fa5fb2d" },
                Registrant = new Dtos.GuidObject2() { Id = "f0deb5c6-7e3e-450b-a861-ea07e7340a97" },
                Status = new Dtos.DtoProperties.SectionRegistrationStatusDtoProperty
                {
                    RegistrationStatus = Dtos.EnumProperties.RegistrationStatus3.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.EnumProperties.RegistrationStatusReason3.Dropped,
                    Detail = new GuidObject2("7d51cda6-6ade-447a-9271-b57784d64ad4")
                },
                AcademicLevel = new GuidObject2() { Id = "97776a59-5f1e-4a1d-b509-b2206596f2a2" },
                Approval = new List<Approval2>()
                {
                    new Approval2()
                    {
                        ApprovalEntity = ApprovalEntity.System,
                        ApprovalType = ApprovalType2.All
                    }
                },
                Credit = new Dtos.DtoProperties.Credit4DtoProperty()
                {
                    Measure = Dtos.EnumProperties.StudentCourseTransferMeasure.Credit,
                    RegistrationCredit = 3.50m
                },
                GradingOption = new Dtos.DtoProperties.SectionRegistrationTranscript2()
                {
                    GradeScheme = new GuidObject2("b48e621c-2a4e-48cf-a99f-7bcb0d4459a6"),
                    Mode = Dtos.EnumProperties.TranscriptMode2.Standard
                },
                Involvement = new SectionRegistrationInvolvement()
                {
                    StartOn = DateTimeOffset.Now.Date,
                    EndOn = DateTimeOffset.Now.Date.AddMonths(3)
                },
                OriginallyRegisteredOn = DateTime.Now.AddDays(-45),
                Override = new Dtos.DtoProperties.SectionRegistrationsOverrideDtoProperty()
                {
                    AcademicPeriod = new GuidObject2("6bb46125-8462-45c6-a070-ea0eb6474518"),
                    Site = new GuidObject2("a9f666de-8e21-4c04-a670-7109da6a363a")
                },
                StatusDate = DateTime.Now
            };

            var sectionRegistrationDto2 = new Dtos.SectionRegistration4()
            {
                Id = "2d95b329-edbe-4420-909a-df57a962a30d",
                Section = new Dtos.GuidObject2() { Id = "c6e6edae-df08-4081-aa24-c8442fa5fb2e" },
                Registrant = new Dtos.GuidObject2() { Id = "g0deb5c6-7e3e-450b-a861-ea07e7340a98" },
                Status = new Dtos.DtoProperties.SectionRegistrationStatusDtoProperty
                {
                    RegistrationStatus = Dtos.EnumProperties.RegistrationStatus3.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.EnumProperties.RegistrationStatusReason3.Dropped,
                    Detail = new GuidObject2("8d51cda6-6ade-447a-9271-b57784d64ad5")
                },
                AcademicLevel = new GuidObject2() { Id = "07776a59-5f1e-4a1d-b509-b2206596f2a3" },
                Approval = new List<Approval2>()
                {
                    new Approval2()
                    {
                        ApprovalEntity = ApprovalEntity.System,
                        ApprovalType = ApprovalType2.All
                    }
                },
                Credit = new Dtos.DtoProperties.Credit4DtoProperty()
                {
                    Measure = Dtos.EnumProperties.StudentCourseTransferMeasure.Credit,
                    RegistrationCredit = 3.50m
                },
                GradingOption = new Dtos.DtoProperties.SectionRegistrationTranscript2()
                {
                    GradeScheme = new GuidObject2("c48e621c-2a4e-48cf-a99f-7bcb0d4459a7"),
                    Mode = Dtos.EnumProperties.TranscriptMode2.Standard
                },
                Involvement = new SectionRegistrationInvolvement()
                {
                    StartOn = DateTimeOffset.Now.Date,
                    EndOn = DateTimeOffset.Now.Date.AddMonths(3)
                },
                OriginallyRegisteredOn = DateTime.Now.AddDays(-45),
                Override = new Dtos.DtoProperties.SectionRegistrationsOverrideDtoProperty()
                {
                    AcademicPeriod = new GuidObject2("7bb46125-8462-45c6-a070-ea0eb6474519"),
                    Site = new GuidObject2("c9f666de-8e21-4c04-a670-7109da6a363b")
                },
                StatusDate = DateTime.Now
            };

            sectionRegistrationDtos.Add(sectionRegistrationDto1);
            sectionRegistrationDtos.Add(sectionRegistrationDto2);

            return sectionRegistrationDtos;
        }
    }
}
