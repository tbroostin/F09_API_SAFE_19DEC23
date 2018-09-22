//Copyright 2017-2018 Ellucian Company L.P.and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Security;
using System.Net.Http.Headers;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentAdvisorRelationshipsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IStudentAdvisorRelationshipsService> studentAdvisorRelationshipsServiceMock;
        private Mock<ILogger> loggerMock;
        private StudentAdvisorRelationshipsController studentAdvisorRelationshipsController;
        private IEnumerable<StudentAdvisorRelationships> allStudentAdvisorRelationships;
        private List<Dtos.StudentAdvisorRelationships> studentAdvisorRelationshipsCollection;

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            studentAdvisorRelationshipsServiceMock = new Mock<IStudentAdvisorRelationshipsService>();
            loggerMock = new Mock<ILogger>();
            studentAdvisorRelationshipsCollection = new List<Dtos.StudentAdvisorRelationships>();

            allStudentAdvisorRelationships = new List<StudentAdvisorRelationships>()
                {
                    new StudentAdvisorRelationships()
                    { Id = "3632ece0-8b9e-495f-a697-b5c9e053aad5",
                        Advisor = new GuidObject2("f760386c-df0b-4312-8c42-dbdf85f3e73e"),
                        Student = new GuidObject2("12625695-bcb0-47a3-b2d7-e40fa6e8730b"),
                        AdvisorType =new GuidObject2("41bc27f2-8468-4946-a69c-c6d4898315ef"),
                        StartOn = new DateTime(2001, 10,15)
                    },
                    new StudentAdvisorRelationships()
                    { Id = "176d35fb-5f7a-4c06-b3ae-65a7662c8b43",
                        Advisor = new GuidObject2("5a11b1e6-1232-4e3a-807b-9a896e24a4de"),
                        Student = new GuidObject2("76912ce5-00cd-45c6-86cf-f9ccf92a9901"),
                        StartOn = new DateTime(2001, 09,01),
                        EndOn = new DateTime(2004, 05,15)
                    },
                    new StudentAdvisorRelationships()
                    { Id = "635a3ad5-59ab-47ca-af87-8538c2ad727f",
                        Advisor = new GuidObject2("fdbf8e8f-eef9-4d57-8a77-6b9baed0bf91"),
                        Student = new GuidObject2("99e94906-2fe0-4078-9510-720eaeb67d77"),
                        AdvisorType =new GuidObject2("41bc27f2-8468-4946-a69c-c6d4898315ef"),
                        Program =new GuidObject2("aee55f94-390a-460c-a88d-f6297687636f"),
                        StartOn = new DateTime(2009, 07,17)
                    },
                };

            foreach (var source in allStudentAdvisorRelationships)
            {
                var studentAdvisorRelationships = new Ellucian.Colleague.Dtos.StudentAdvisorRelationships
                {
                    Id = source.Id,
                    Advisor = source.Advisor,
                    AdvisorType = source.AdvisorType,
                    Program = source.Program,
                    StartOn = source.StartOn,
                    EndOn = source.EndOn
                };
                studentAdvisorRelationshipsCollection.Add(studentAdvisorRelationships);
            }

            studentAdvisorRelationshipsController = new StudentAdvisorRelationshipsController(studentAdvisorRelationshipsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            studentAdvisorRelationshipsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            studentAdvisorRelationshipsController = null;
            allStudentAdvisorRelationships = null;
            studentAdvisorRelationshipsCollection = null;
            loggerMock = null;
            studentAdvisorRelationshipsServiceMock = null;
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_ValidateFields_Nocache()
        {
            studentAdvisorRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentAdvisorRelationshipsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            studentAdvisorRelationshipsController.Request.Headers.CacheControl.NoCache = true;

            Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int> studentAdvRelTuple = new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 3);

            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            var sourceContexts = (await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>()));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAdvisorRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAdvisorRelationships>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(studentAdvisorRelationshipsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
                Assert.AreEqual(expected.Advisor, result.Advisor);
                Assert.AreEqual(expected.AdvisorType, result.AdvisorType);
                Assert.AreEqual(expected.Program, result.Program);
                Assert.AreEqual(expected.StartOn, result.StartOn);
                Assert.AreEqual(expected.EndOn, result.EndOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_ValidateFields_Cache()
        {
            studentAdvisorRelationshipsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            studentAdvisorRelationshipsController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            studentAdvisorRelationshipsController.Request.Headers.CacheControl.NoCache = false;


            Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int> studentAdvRelTuple = new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 3);

            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            var sourceContexts = (await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>()));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAdvisorRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAdvisorRelationships>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(studentAdvisorRelationshipsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
                Assert.AreEqual(expected.Advisor, result.Advisor);
                Assert.AreEqual(expected.AdvisorType, result.AdvisorType);
                Assert.AreEqual(expected.Program, result.Program);
                Assert.AreEqual(expected.StartOn, result.StartOn);
                Assert.AreEqual(expected.EndOn, result.EndOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_StudentFilter()
        {
            Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int> studentAdvRelTuple = new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 3);

            var filterGroupName = "criteria";

            studentAdvisorRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            studentAdvisorRelationshipsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };

            studentAdvisorRelationshipsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.StudentAdvisorRelationshipFilter() { Student = new GuidObject2("12625695-bcb0-47a3-b2d7-e40fa6e8730b") });
            
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), "12625695-bcb0-47a3-b2d7-e40fa6e8730b", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            QueryStringFilter criteria = new QueryStringFilter("criteria", "{'student':{'id': '12625695-bcb0-47a3-b2d7-e40fa6e8730b'}}");

            var sourceContexts = (await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), criteria));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAdvisorRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAdvisorRelationships>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(studentAdvisorRelationshipsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
                Assert.AreEqual(expected.Advisor, result.Advisor);
                Assert.AreEqual(expected.AdvisorType, result.AdvisorType);
                Assert.AreEqual(expected.Program, result.Program);
                Assert.AreEqual(expected.StartOn, result.StartOn);
                Assert.AreEqual(expected.EndOn, result.EndOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_AdvisorFilter()
        {
            Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int> studentAdvRelTuple = new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 3);

            var filterGroupName = "criteria";

            studentAdvisorRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            studentAdvisorRelationshipsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            
            studentAdvisorRelationshipsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.StudentAdvisorRelationshipFilter() { Advisor = new GuidObject2("f760386c-df0b-4312-8c42-dbdf85f3e73e") });
            
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), It.IsAny<string>(), "f760386c-df0b-4312-8c42-dbdf85f3e73e", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            QueryStringFilter criteria = new QueryStringFilter("criteria", "{'advisor':{'id': 'f760386c-df0b-4312-8c42-dbdf85f3e73e'}}");

            var sourceContexts = (await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), criteria));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAdvisorRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAdvisorRelationships>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(studentAdvisorRelationshipsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
                Assert.AreEqual(expected.Advisor, result.Advisor);
                Assert.AreEqual(expected.AdvisorType, result.AdvisorType);
                Assert.AreEqual(expected.Program, result.Program);
                Assert.AreEqual(expected.StartOn, result.StartOn);
                Assert.AreEqual(expected.EndOn, result.EndOn);
            }
        }

        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_AdvisorTypeFilter()
        {
            Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int> studentAdvRelTuple = new Tuple<IEnumerable<Dtos.StudentAdvisorRelationships>, int>(studentAdvisorRelationshipsCollection, 3);

            var filterGroupName = "criteria";

            studentAdvisorRelationshipsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            studentAdvisorRelationshipsController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };

            studentAdvisorRelationshipsController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.Filters.StudentAdvisorRelationshipFilter() { AdvisorType = new GuidObject2("41bc27f2-8468-4946-a69c-c6d4898315ef") });

            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), "41bc27f2-8468-4946-a69c-c6d4898315ef", It.IsAny<string>())).ReturnsAsync(studentAdvRelTuple);

            QueryStringFilter criteria = new QueryStringFilter("criteria", "{'advisorType':{'id': '41bc27f2-8468-4946-a69c-c6d4898315ef'}}");

            var sourceContexts = (await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), criteria));

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.StudentAdvisorRelationships> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentAdvisorRelationships>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.StudentAdvisorRelationships>;

            Assert.IsTrue(sourceContexts is IHttpActionResult);
            Assert.AreEqual(studentAdvisorRelationshipsCollection.Count, results.ToList().Count());
            foreach (var result in results.ToList())
            {
                var expected = studentAdvisorRelationshipsCollection.FirstOrDefault(s => s.Id == result.Id);
                Assert.AreEqual(expected.Id, result.Id);
                Assert.AreEqual(expected.Advisor, result.Advisor);
                Assert.AreEqual(expected.AdvisorType, result.AdvisorType);
                Assert.AreEqual(expected.Program, result.Program);
                Assert.AreEqual(expected.StartOn, result.StartOn);
                Assert.AreEqual(expected.EndOn, result.EndOn);
            }
        }


        [TestMethod]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationshipsByGuidAsync_ValidateFields()
        {
            var expected = studentAdvisorRelationshipsCollection.FirstOrDefault();
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Advisor, actual.Advisor);
            Assert.AreEqual(expected.AdvisorType, actual.AdvisorType);
            Assert.AreEqual(expected.Program, actual.Program);
            Assert.AreEqual(expected.StartOn, actual.StartOn);
            Assert.AreEqual(expected.EndOn, actual.EndOn);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_Exception()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<Exception>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_KeyNotFoundException()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<KeyNotFoundException>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_PermissionsException()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<PermissionsException>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_ArgumentException()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<ArgumentException>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_RepositoryException()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<RepositoryException>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationships_IntegrationApiException()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsAsync(0, 100, true, null, null, null, null)).Throws<IntegrationApiException>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsAsync(new Paging(100, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_GetStudentAdvisorRelationshipsByGuidAsync_Exception()
        {
            studentAdvisorRelationshipsServiceMock.Setup(x => x.GetStudentAdvisorRelationshipsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await studentAdvisorRelationshipsController.GetStudentAdvisorRelationshipsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_PostStudentAdvisorRelationshipsAsync_Exception()
        {
            await studentAdvisorRelationshipsController.PostStudentAdvisorRelationshipsAsync(studentAdvisorRelationshipsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_PutStudentAdvisorRelationshipsAsync_Exception()
        {
            var sourceContext = studentAdvisorRelationshipsCollection.FirstOrDefault();
            await studentAdvisorRelationshipsController.PutStudentAdvisorRelationshipsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentAdvisorRelationshipsController_DeleteStudentAdvisorRelationshipsAsync_Exception()
        {
            await studentAdvisorRelationshipsController.DeleteStudentAdvisorRelationshipsAsync(studentAdvisorRelationshipsCollection.FirstOrDefault().Id);
        }
    }
}