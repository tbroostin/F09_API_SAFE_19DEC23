//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonExternalEducationControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IPersonExternalEducationService> PersonExternalEducationServiceMock;
        private Mock<ILogger> loggerMock;

        private PersonExternalEducationController PersonExternalEducationController;
        private List<Dtos.PersonExternalEducation> PersonExternalEducationCollection;
        private Tuple<IEnumerable<Dtos.PersonExternalEducation>, int> PersonExternalEducationTuple;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            PersonExternalEducationServiceMock = new Mock<IPersonExternalEducationService>();
            loggerMock = new Mock<ILogger>();
            PersonExternalEducationCollection = BuildData();

            PersonExternalEducationTuple = new Tuple<IEnumerable<Dtos.PersonExternalEducation>, int>(PersonExternalEducationCollection, PersonExternalEducationCollection.Count);

            PersonExternalEducationController = new PersonExternalEducationController(PersonExternalEducationServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            PersonExternalEducationController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            PersonExternalEducationController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            PersonExternalEducationController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        private List<Dtos.PersonExternalEducation> BuildData()
        {
            List<Dtos.PersonExternalEducation> personExternalEducation = new List<Dtos.PersonExternalEducation>()
                {
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Person = new GuidObject2("e214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("ab1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "3.900",
                        
                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                        Person = new GuidObject2("f214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("bb1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "2.900",
                        ClassSize = 100,
                        ClassRank = 5,
                        TotalCredits = 10,
                        AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                        {
                            new ExternalEducationAttendancePeriods()
                                {
                                StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month = 10, Year = 1970},
                                EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 1976}
                                }
                        }
                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                         Person = new GuidObject2("b214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("cb1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "1.900",
                        ClassSize = 320,
                        ClassRank = 52,
                        TotalCredits = 1,
                        AttendancePeriods = new List<ExternalEducationAttendancePeriods>()
                        {
                            new ExternalEducationAttendancePeriods()
                                {
                                StartDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 10, Month = 10, Year = 2010},
                                EndDate = new Dtos.DtoProperties.DateDtoProperty () { Day = 30, Month = 5, Year = 2014}
                                }
                        }

                    },
                    new Dtos.PersonExternalEducation()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                         Person = new GuidObject2("1214b7b0-aced-451b-aece-f97b1f5dfb10"),
                        Institution = new GuidObject2("db1f9756-f5b4-4355-903e-513ff179a338"),
                        PerformanceMeasure = "2.900",

                    },
                };
            return personExternalEducation;
        }

        [TestCleanup]
        public void Cleanup()
        {
            PersonExternalEducationController = null;
            //allInstitutionsAttend = null;
            PersonExternalEducationCollection = null;
            loggerMock = null;
            PersonExternalEducationServiceMock = null;
        }

        [TestMethod]
        public async Task PersonExternalEducationController_GetPersonExternalEducation_ValidateFields_Nocache()
        {
            PersonExternalEducationController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            PersonExternalEducationServiceMock.Setup(x => x.GetPersonExternalEducationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonExternalEducation>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonExternalEducationTuple);

            var outcomes = await PersonExternalEducationController.GetPersonExternalEducationAsync(new Paging(3, 0), criteriaFilter, null);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await outcomes.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonExternalEducation> sourceContexts = ((ObjectContent<IEnumerable<Dtos.PersonExternalEducation>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.PersonExternalEducation>;

            Assert.AreEqual(PersonExternalEducationCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = PersonExternalEducationCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());

            }
        }

        [TestMethod]
        public async Task PersonExternalEducationController_GetPersonExternalEducation_ValidateFields_Cache()
        {
            PersonExternalEducationController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            PersonExternalEducationServiceMock.Setup(x => x.GetPersonExternalEducationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonExternalEducation>(), 
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(PersonExternalEducationTuple);

            var outcomes = (await PersonExternalEducationController.GetPersonExternalEducationAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter, null));

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await outcomes.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonExternalEducation> sourceContexts = ((ObjectContent<IEnumerable<Dtos.PersonExternalEducation>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.PersonExternalEducation>;

            Assert.AreEqual(PersonExternalEducationCollection.Count, sourceContexts.Count());
            for (var i = 0; i < sourceContexts.Count(); i++)
            {
                var expected = PersonExternalEducationCollection[i];
                var actual = sourceContexts.ElementAt(i);
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task PersonExternalEducationController_GetPersonExternalEducationByGuidAsync_ValidateFields()
        {
            var expected = PersonExternalEducationCollection.FirstOrDefault();
            PersonExternalEducationServiceMock.Setup(x => x.GetPersonExternalEducationByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await PersonExternalEducationController.GetPersonExternalEducationByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationController_GetPersonExternalEducation_Exception()
        {
            PersonExternalEducationServiceMock.Setup(x => x.GetPersonExternalEducationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PersonExternalEducation>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await PersonExternalEducationController.GetPersonExternalEducationAsync(new Web.Http.Models.Paging(3, 0), criteriaFilter, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationController_GetPersonExternalEducationByGuidAsync_Exception()
        {
            PersonExternalEducationServiceMock.Setup(x => x.GetPersonExternalEducationByGuidAsync(It.IsAny<string>(), false)).Throws<Exception>();
            await PersonExternalEducationController.GetPersonExternalEducationByGuidAsync(string.Empty);
        }

        [TestMethod]
        public async Task PersonExternalEducationController_PostPersonExternalEducationAsync_ValidateFields()
        {
            var expected = PersonExternalEducationCollection.FirstOrDefault();
            PersonExternalEducationServiceMock.Setup(x => x.CreateUpdatePersonExternalEducationAsync(expected, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await PersonExternalEducationController.PostPersonExternalEducationAsync(expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        public async Task PersonExternalEducationController_PutPersonExternalEducationAsync_ValidateFields()
        {
            var expected = PersonExternalEducationCollection.FirstOrDefault();
            PersonExternalEducationServiceMock.Setup(x => x.CreateUpdatePersonExternalEducationAsync(expected, It.IsAny<bool>())).ReturnsAsync(expected);
            var actual = await PersonExternalEducationController.PutPersonExternalEducationAsync(expected.Id, expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationController_PostPersonExternalEducationAsync_Exception()
        {
            var sourceContext = PersonExternalEducationCollection.FirstOrDefault();
            PersonExternalEducationServiceMock.Setup(x => x.CreateUpdatePersonExternalEducationAsync(sourceContext, It.IsAny<bool>())).Throws<Exception>();
            await PersonExternalEducationController.PostPersonExternalEducationAsync(sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationController_PutPersonExternalEducationAsync_Exception()
        {
            var sourceContext = PersonExternalEducationCollection.FirstOrDefault();
            PersonExternalEducationServiceMock.Setup(x => x.CreateUpdatePersonExternalEducationAsync(sourceContext, It.IsAny<bool>())).Throws<Exception>();
            await PersonExternalEducationController.PutPersonExternalEducationAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonExternalEducationController_DeletePersonExternalEducationAsync_Exception()
        {
            await PersonExternalEducationController.DeletePersonExternalEducationAsync(PersonExternalEducationCollection.FirstOrDefault().Id);
        }
    }
}