//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.IO;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Models;
using System.Reflection;
using Ellucian.Colleague.Dtos.Attributes;
using Newtonsoft.Json;
using System.Web.Http.Routing;
using System.Collections;
using System.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AdmissionDecisionsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAdmissionDecisionsService> admissionDecisionsServiceMock;
        private Mock<ILogger> loggerMock;
        private AdmissionDecisionsController admissionDecisionsController;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        List<Dtos.AdmissionDecisions> admissionDecisionsDtos;
        Dtos.AdmissionDecisions admissionDecisionDTO;
        readonly int offset = 0;
        readonly int limit = 100;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            admissionDecisionsServiceMock = new Mock<IAdmissionDecisionsService>();
            loggerMock = new Mock<ILogger>();

            BuildData();

            admissionDecisionsController = new AdmissionDecisionsController(admissionDecisionsServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            admissionDecisionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            admissionDecisionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
        }
        private void BuildData()
        {
            admissionDecisionsDtos = new List<Dtos.AdmissionDecisions>()
                {
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a",
                        Application = new Dtos.GuidObject2("b2ee3ff9-8613-40ff-9c32-4aa7bedc607d"),
                        DecidedOn = new DateTimeOffset(2017,10,11, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                       Application = new Dtos.GuidObject2("b2ee3ff9-8613-40ff-9c32-4aa7bedc607d"),
                        DecidedOn = new DateTimeOffset(2017,9,13, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                        DecisionType = new Dtos.GuidObject2("67534c13-5d55-4d9d-a03f-1acf23598158"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Application = new Dtos.GuidObject2("8559e317-dc8e-4635-8ca7-6dd5c0924647"),
                        DecidedOn = new DateTimeOffset(2017,1,1, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Application = new Dtos.GuidObject2("8559e317-dc8e-4635-8ca7-6dd5c0924647"),
                        DecidedOn = new DateTimeOffset(2017,10,1, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                };
            admissionDecisionDTO = new Dtos.AdmissionDecisions()
            {
                Id = Guid.Empty.ToString(),
                Application = new Dtos.GuidObject2("b2ee3ff9-8613-40ff-9c32-4aa7bedc607d"),
                DecidedOn = new DateTimeOffset(2017, 10, 11, 00, 00, 00, new TimeSpan(-4, 0, 0)),
                DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d")
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            admissionDecisionsController = null;
            admissionDecisionsDtos = null;
            admissionDecisionDTO = null;
            loggerMock = null;
            admissionDecisionsServiceMock = null;
        }

      
        [TestMethod]
        public async Task AdmissionDecisionsController_GetAll_NoCache_True()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);
           
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit,  It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(),
                It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionDecisions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.AdmissionDecisions> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.AdmissionDecisions>;

            Assert.AreEqual(admissionDecisionsDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
                Assert.AreEqual(expected.DecisionType, actual.DecisionType);
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionsController_GetAll_NoCache_False()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, "", It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), false)).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionDecisions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.AdmissionDecisions> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.AdmissionDecisions>;

            Assert.AreEqual(admissionDecisionsDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
                Assert.AreEqual(expected.DecisionType, actual.DecisionType);              
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionsController_GetAll_NoCache_ValidFilter()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);

            string criteria = @"{'application':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";

            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionDecisions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.AdmissionDecisions> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.AdmissionDecisions>;

            Assert.AreEqual(admissionDecisionsDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
                Assert.AreEqual(expected.DecisionType, actual.DecisionType);
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionsController_GetAll_NoCache_InValidFilter()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);

            string criteria = @"{'invalid':{'id':'70479f3b-bb79-4c0b-a0db-c240cd51e300'}}";

            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionDecisions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.AdmissionDecisions> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.AdmissionDecisions>;

            Assert.AreEqual(admissionDecisionsDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
                Assert.AreEqual(expected.DecisionType, actual.DecisionType);
            }
        }

        [TestMethod]
        public async Task AdmissionDecisionsController_GetAll_NullPage()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, "", It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), false)).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(null, criteriaFilter, personFilter);


            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await admissionDecisions.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.AdmissionDecisions> actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisions>>)httpResponseMessage.Content)
                                                            .Value as IEnumerable<Dtos.AdmissionDecisions>;

            Assert.AreEqual(admissionDecisionsDtos.Count, actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Application.Id, actual.Application.Id);
                Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
                Assert.AreEqual(expected.DecisionType, actual.DecisionType);

            }
        }

        [TestMethod]
        public void AdmissionDecisionsController_DecidedOn_SupportedFilterOperators()
        {

            var modelType = typeof(Dtos.AdmissionDecisions);
            var key = new List<string>() { "criteria" };
            var memberName = "decidedOn";

            var filterAttributes = new List<string>();
            var supportedFilterDict = new Dictionary<List<string>, List<string>>();

            var properties = modelType.GetProperties();

            PropertyInfo matchingProperty = properties
               .FirstOrDefault(p => Attribute.IsDefined(p, typeof(JsonPropertyAttribute))
               && (((JsonPropertyAttribute)Attribute.GetCustomAttribute(
                                p, typeof(JsonPropertyAttribute))).PropertyName != null)
               && (((JsonPropertyAttribute)Attribute.GetCustomAttribute(
                                p, typeof(JsonPropertyAttribute))).PropertyName.ToLower() == memberName.ToLower()
                                ));

            var matchingFilterAttributes = matchingProperty.GetCustomAttributes(typeof(FilterPropertyAttribute), false);

            foreach (FilterPropertyAttribute matchingFilterAttribute in matchingFilterAttributes)
            {

                filterAttributes = (matchingFilterAttribute.Name).ToList();

                if (matchingFilterAttribute.Name[0] == key[0] && (matchingFilterAttribute.SupportedOperators != null))
                {
                    supportedFilterDict.Add((matchingFilterAttribute.Name).ToList(), (matchingFilterAttribute.SupportedOperators).ToList());
                    break;
                }
            }

            var expected = supportedFilterDict.FirstOrDefault(x => x.Key[0] == key[0]);

            Assert.IsNotNull(expected, "expected");
            Assert.IsTrue(expected.Value.Contains("$eq"), "Contains $eq");
            Assert.IsTrue(expected.Value.Contains("$gte"), "Contains $gte");
            Assert.IsTrue(expected.Value.Contains("$lte"), "Contains $lte");
            Assert.IsFalse(expected.Value.Contains("$ne"), "Contains $ne");

        }
        
        [TestMethod]
        public async Task AdmissionDecisionsController_GetById()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                Public = true
            };
            var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            var admissionDecisions = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsByGuidAsync(id, false)).ReturnsAsync(admissionDecisions);

            var actual = await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(id);

            var expected = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Application.Id, actual.Application.Id);
            Assert.AreEqual(expected.DecidedOn, actual.DecidedOn);
            Assert.AreEqual(expected.DecisionType, actual.DecisionType);
        }

        [TestMethod]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ReturnsAsync(admissionDecisionsDtos[0]);
            var result = await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Null_Object_Passed_As_Parameter()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAll_Exception()
        {
            admissionDecisionsController.Request.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = false,
                Public = true
            };
            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 4);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, string.Empty, It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), false)).ThrowsAsync(new Exception());
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetById_Exception()
        {
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            var actual = await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetById_KeyNotFoundException()
        {
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(),It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

            var actual = await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>());
        }
      
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_KeyNotFoundException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_PermissionsException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_ArgumentNullException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentNullException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_RepositoryException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_IntegrationApiException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
        } 

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuidAsync_Exception()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_KeyNotFoundException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_PermissionsException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_ArgumentException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_RepositoryException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_IntegrationApiException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_ArgumentNullException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentNullException>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuid_Exception()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PutAdmissionDecisionsAsync_Exception()
        {
            var source = this.admissionDecisionsDtos.FirstOrDefault();
            await admissionDecisionsController.PutAdmissionDecisionsAsync(source.Id, source);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_DeleteAdmissionDecisionsAsync_Exception()
        {
            await admissionDecisionsController.DeleteAdmissionDecisionsAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Id_Not_Nil_Guid_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions() { Id = "1234" });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Id_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Application_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions()
            {
                Id = "00000000-0000-0000-0000-000000000000"
            });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Application_Id_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions()
            {
                Id = "00000000-0000-0000-0000-000000000000",
                Application = new Dtos.GuidObject2()
            });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_DecisionType_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions()
            {
                Id = "00000000-0000-0000-0000-000000000000",
                Application = new Dtos.GuidObject2("1234"),
                DecisionType = null
            });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_DecisionType_Id_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions()
            {
                Id = "00000000-0000-0000-0000-000000000000",
                Application = new Dtos.GuidObject2("1234"),
                DecisionType = new Dtos.GuidObject2()
            });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_DecidedOn_Null_Exception()
        {
            await admissionDecisionsController.PostAdmissionDecisionsAsync(new Dtos.AdmissionDecisions()
            {
                Id = "00000000-0000-0000-0000-000000000000",
                Application = new Dtos.GuidObject2("1234"),
                DecisionType = new Dtos.GuidObject2("1234"),
                DecidedOn = default(DateTimeOffset)
            });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_KeyNoyFound()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new KeyNotFoundException());
            await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_PermissionsException()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new PermissionsException());
            await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_RepositoryException()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new RepositoryException());
            await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_IntegrationApiException()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new IntegrationApiException());
            await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Exception()
        {
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new Exception());
            await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
        }

        // Permissions tests

        //Get
        //Version 11 / 11.1.0
        //GetAdmissionDecisionsAsync

        //Example success 
        [TestMethod]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsAsync_Permissions()
        {
            Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");

            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "GetAdmissionDecisionsAsync" }
                };
            HttpRoute route = new HttpRoute("admission-decisions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);
            admissionDecisionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions });

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 5);
            admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, string.Empty, It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), false)).ReturnsAsync(tuple);
            var resp = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);

            Object filterObject;
            admissionDecisionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAdmissionDecisions));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAdmissionDecisions));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsAsync_Invalid_Permissions()
        {
            Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
            Ellucian.Web.Http.Models.QueryStringFilter personFilter = new Web.Http.Models.QueryStringFilter("personFilter", "");
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "GetAdmissionDecisionsAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<DateTimeOffset?>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-decisions."));
                var resp = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter, personFilter);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Get by Id
        //Version 11 / 11.1.0
        //GetAdmissionDecisionsByGuidAsync
        //Example success 
        [TestMethod]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuidAsync_Permissions()
        {
            var id = "bbd216fb-0fc5-4f44-ae45-42d3cdd1e89a";
            var admissionDecisions = admissionDecisionsDtos.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "GetAdmissionDecisionsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("admission-decisions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);
            admissionDecisionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(new string[] { StudentPermissionCodes.ViewAdmissionDecisions, StudentPermissionCodes.UpdateAdmissionDecisions });

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 5);
            admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsByGuidAsync(id, false)).ReturnsAsync(admissionDecisions);
            var resp = await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(id);

            Object filterObject;
            admissionDecisionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.ViewAdmissionDecisions));
            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAdmissionDecisions));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisionsByGuidAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "GetAdmissionDecisionsByGuidAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));
                admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to view admission-decisions."));
                var resp = await admissionDecisionsController.GetAdmissionDecisionsByGuidAsync(expectedGuid);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }

        //Post
        //Version 11 / 11.1.0
        //PostAdmissionDecisionsAsync
        //Example success 
        [TestMethod]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Permissions()
        {
            var contextPropertyName = "PermissionsFilter";

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "PostAdmissionDecisionsAsync" }
                };
            HttpRoute route = new HttpRoute("admission-decisions", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);
            admissionDecisionsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            var permissionsFilter = new PermissionsFilter(StudentPermissionCodes.UpdateAdmissionDecisions );

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

            var tuple = new Tuple<IEnumerable<Dtos.AdmissionDecisions>, int>(admissionDecisionsDtos, 5);
            admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>())).Returns(true);
            admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ReturnsAsync(admissionDecisionsDtos[0]);
            var result = await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);

            Object filterObject;
            admissionDecisionsController.ActionContext.Request.Properties.TryGetValue(contextPropertyName, out filterObject);
            var cancelToken = new System.Threading.CancellationToken(false);
            Assert.IsNotNull(filterObject);

            var permissionsCollection = ((IEnumerable)filterObject).Cast<object>()
                                 .Select(x => x.ToString())
                                 .ToArray();

            Assert.IsTrue(permissionsCollection.Contains(StudentPermissionCodes.UpdateAdmissionDecisions));

        }

        //Example exception
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_PostAdmissionDecisionsAsync_Invalid_Permissions()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary
                {
                    { "controller", "AdmissionDecisions" },
                    { "action", "PostAdmissionDecisionsAsync" }
                };
            HttpRoute route = new HttpRoute("admission-applications", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            admissionDecisionsController.Request.SetRouteData(data);

            var permissionsFilter = new PermissionsFilter("invalid");

            var controllerContext = admissionDecisionsController.ControllerContext;
            var actionDescriptor = admissionDecisionsController.ActionContext.ActionDescriptor
                     ?? new Mock<HttpActionDescriptor>() { CallBase = true }.Object;

            var _context = new HttpActionContext(controllerContext, actionDescriptor);
            try
            {
                await permissionsFilter.OnActionExecutingAsync(_context, new System.Threading.CancellationToken(false));

                admissionDecisionsServiceMock.Setup(i => i.CreateAdmissionDecisionAsync(It.IsAny<Dtos.AdmissionDecisions>())).ThrowsAsync(new PermissionsException());
                admissionDecisionsServiceMock.Setup(s => s.ValidatePermissions(It.IsAny<Tuple<string[], string, string>>()))
                    .Throws(new PermissionsException("User 'npuser' does not have permission to create admission-applications."));
                var resp = await admissionDecisionsController.PostAdmissionDecisionsAsync(admissionDecisionDTO);
            }
            catch (PermissionsException ex)
            {
                throw ex;
            }
        }


    }
}