//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
        int offset = 0;
        int limit = 100;
        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

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
                        DecidedOn = new DateTime(2017,10,11),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "3f67b180-ce1d-4552-8d81-feb96b9fea5b",
                       Application = new Dtos.GuidObject2("b2ee3ff9-8613-40ff-9c32-4aa7bedc607d"),
                        DecidedOn = new DateTime(2017,9,13),
                        DecisionType = new Dtos.GuidObject2("67534c13-5d55-4d9d-a03f-1acf23598158"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "bf67e156-8f5d-402b-8101-81b0a2796873",
                        Application = new Dtos.GuidObject2("8559e317-dc8e-4635-8ca7-6dd5c0924647"),
                        DecidedOn = new DateTime(2017,1,1),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                    new Dtos.AdmissionDecisions()
                    {
                        Id = "0111d6ef-5a86-465f-ac58-4265a997c136",
                        Application = new Dtos.GuidObject2("8559e317-dc8e-4635-8ca7-6dd5c0924647"),
                        DecidedOn = new DateTime(2017,10,1),
                        DecisionType = new Dtos.GuidObject2("3d737f01-9624-4306-9200-04153bebab4d"),
                    },
                };
            admissionDecisionDTO = new Dtos.AdmissionDecisions()
            {
                Id = Guid.Empty.ToString(),
                Application = new Dtos.GuidObject2("b2ee3ff9-8613-40ff-9c32-4aa7bedc607d"),
                DecidedOn = new DateTime(2017, 10, 11),
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
            //admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, "", true)).ReturnsAsync(tuple);
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit,  It.IsAny<string>() , It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);

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
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, "",false)).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);

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

            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);

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

            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);

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
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, "", false)).ReturnsAsync(tuple);
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(null, criteriaFilter);


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
            admissionDecisionsServiceMock.Setup(ci => ci.GetAdmissionDecisionsAsync(offset, limit, string.Empty, false)).ThrowsAsync(new Exception());
            var admissionDecisions = await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
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
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_PermissionsException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_ArgumentNullException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentNullException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_RepositoryException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AdmissionDecisionsController_GetAdmissionDecisions_IntegrationApiException()
        {
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await admissionDecisionsController.GetAdmissionDecisionsAsync(new Paging(limit, offset), criteriaFilter);
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
            admissionDecisionsServiceMock.Setup(x => x.GetAdmissionDecisionsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
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
                DecidedOn = default(DateTime)
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
    }
}