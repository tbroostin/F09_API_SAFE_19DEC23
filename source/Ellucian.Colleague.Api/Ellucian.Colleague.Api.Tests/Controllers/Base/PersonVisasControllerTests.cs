// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using System.Net.Http.Headers;
using System.Threading;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonVisasControllerTests
    {
        private TestContext testContextInstance2;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance2;
            }
            set
            {
                testContextInstance2 = value;
            }
        }

        private PersonVisasController personVisasController;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        IAdapterRegistry AdapterRegistry;
        Mock<IPersonVisasService> personVisasServiceMock = new Mock<IPersonVisasService>();

        Ellucian.Colleague.Dtos.PersonVisa personVisa;
        IEnumerable<Dtos.PersonVisa> personVisaDtos;
        Tuple<IEnumerable<Dtos.PersonVisa>, int> personVisaTuple;
        Paging page;
        string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
        int offset = 0;
        int limit = 4;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            BuildPersonVisaDto();

            personVisasController = new PersonVisasController(AdapterRegistry, personVisasServiceMock.Object, loggerMock.Object);
            personVisasController.Request = new HttpRequestMessage();
            personVisasController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personVisasController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(personVisa));

        }

        private void BuildPersonVisaDto()
        {
            personVisaDtos = new List<Dtos.PersonVisa>()
            {
                new Dtos.PersonVisa()
                {
                    Entries = new List<PersonVisaEntry>()
                    {
                        new PersonVisaEntry(){EnteredOn = new DateTimeOffset(2016, 02, 29, 0, 0, 0, new TimeSpan())},
                    },
                    ExpiresOn = new DateTime(2017, 12, 25),
                    Id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0",
                    IssuedOn = new DateTime(2015, 12, 31),
                    Person = new GuidObject2("51f12e6f-b6b1-4cbe-a45b-b4fed77c1dec"),
                    RequestedOn = new DateTime(2015, 09, 17),
                    VisaId = "A123456",
                    VisaStatus = Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current,
                    VisaType = new VisaType2() { Detail = new GuidObject2("df06a9cc-825b-47be-884a-193e89fae76d"), VisaTypeCategory = Ellucian.Colleague.Dtos.VisaTypeCategory.NonImmigrant }
                },
                new Dtos.PersonVisa()
                {
                    Entries = new List<PersonVisaEntry>()
                    {
                        new PersonVisaEntry(){EnteredOn = new DateTimeOffset(2016, 02, 29, 0, 0, 0, new TimeSpan())},
                    },
                    ExpiresOn = new DateTime(2017, 12, 25),
                    Id = "04009aa0-a8f2-4796-a0c9-e4eb30c30389",
                    IssuedOn = new DateTime(2015, 12, 31),
                    Person = new GuidObject2("1a631a73-429f-4a93-9428-58c453901c3d"),
                    RequestedOn = new DateTime(2015, 09, 17),
                    VisaId = "A1",
                    VisaStatus = Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current,
                    VisaType = new VisaType2() { Detail = new GuidObject2("4b42f06f-cc1b-4906-af0b-286bd1bdb007"), VisaTypeCategory = Ellucian.Colleague.Dtos.VisaTypeCategory.NonImmigrant }
                },
                new Dtos.PersonVisa()
                {
                    Entries = new List<PersonVisaEntry>()
                    {
                        new PersonVisaEntry(){EnteredOn = new DateTimeOffset(2016, 02, 29, 0, 0, 0, new TimeSpan())},
                    },
                    ExpiresOn = new DateTime(2017, 12, 25),
                    Id = "fd1bb662-13ee-49f0-94cf-e78b0427ff19",
                    IssuedOn = new DateTime(2015, 12, 31),
                    Person = new GuidObject2("0c1ef800-30ac-47f2-9cf8-ff982a333f63"),
                    RequestedOn = new DateTime(2014, 09, 17),
                    VisaId = "A2",
                    VisaStatus = Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Expired,
                    VisaType = new VisaType2() { Detail = new GuidObject2("5ef2c805-8cf9-432c-a51d-b96c188cc1bd"), VisaTypeCategory = Ellucian.Colleague.Dtos.VisaTypeCategory.Immigrant }
                },
                new Dtos.PersonVisa()
                {
                    Entries = new List<PersonVisaEntry>()
                    {
                        new PersonVisaEntry(){EnteredOn = new DateTimeOffset(2016, 02, 29, 0, 0, 0, new TimeSpan())},
                    },
                    ExpiresOn = new DateTime(2017, 12, 25),
                    Id = "f1dc7704-48dd-46fb-b115-773666954337",
                    IssuedOn = new DateTime(2015, 12, 31),
                    Person = new GuidObject2("07d16adc-bbdf-4244-abbb-7fc404053796"),
                    RequestedOn = new DateTime(2015, 09, 17),
                    VisaId = "A3",
                    VisaStatus = Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current,
                    VisaType = new VisaType2() { Detail = new GuidObject2("b5a9c18c-a092-47ec-9617-40a11e2bdf87"), VisaTypeCategory = Ellucian.Colleague.Dtos.VisaTypeCategory.NonImmigrant }
                }
            };
            personVisaTuple = new Tuple<IEnumerable<Dtos.PersonVisa>, int>(personVisaDtos, personVisaDtos.Count());
            personVisa = new Dtos.PersonVisa()
            {
                Entries = new List<PersonVisaEntry>()
                {
                    new PersonVisaEntry(){EnteredOn = new DateTimeOffset(2016, 02, 29, 0, 0, 0, new TimeSpan())},
                },
                ExpiresOn = new DateTime(2017, 12, 25),
                Id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0",
                IssuedOn = new DateTime(2015, 12, 31),
                Person = new GuidObject2("51f12e6f-b6b1-4cbe-a45b-b4fed77c1dec"),
                RequestedOn = new DateTime(2015, 09, 17),
                VisaId = "A123456",
                VisaStatus = Ellucian.Colleague.Dtos.EnumProperties.VisaStatus.Current,
                VisaType = new VisaType2() { Detail = new GuidObject2("df06a9cc-825b-47be-884a-193e89fae76d"), VisaTypeCategory = Ellucian.Colleague.Dtos.VisaTypeCategory.NonImmigrant }
            };
            page = new Paging(limit, offset);
        }

        [TestCleanup]
        public void Cleanup()
        {
            personVisasController = null;
            personVisa = null;
        }

        #region Exceptions Testing

        #region GET
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByIdAsyncAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.GetPersonVisaByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByIdAsyncAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await personVisasController.GetPersonVisaByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByIdAsyncAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await personVisasController.GetPersonVisaByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByPersonIdAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.GetPersonVisaByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByPersonIdAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await personVisasController.GetPersonVisaByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaByPersonIdAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await personVisasController.GetPersonVisaByIdAsync("1234");
        }
        #endregion

        #region POST
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PostPersonVisaAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.PostPersonVisaAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PostPersonVisaAsync_IdNull_ArgumentNullException()
        {
            personVisa.Id = string.Empty;
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.PostPersonVisaAsync(personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PostPersonVisaAsync_InvalidOperationException()
        {
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PostPersonVisaAsync(personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PostPersonVisaAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new KeyNotFoundException());
            await personVisasController.PostPersonVisaAsync(personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PostPersonVisaAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new Exception());
            await personVisasController.PostPersonVisaAsync(personVisa);
        }
        #endregion

        #region PUT
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.PutPersonVisaAsync(It.IsAny<string>(), personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_PersonNull_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_PersonIdNull_InvalidOperationException()
        {
            personVisa.Id = string.Empty;
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_IdNotSame_InvalidOperationException()
        {
            personVisa.Id = "475ef15b-f2d2-40ed-ac47-f0d2d45260d1";
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new KeyNotFoundException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_PutPersonVisaAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new Exception());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisas_Validate_IdNull_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            personVisa.Person = new GuidObject2() { };
            var result = await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisas_Validate_VisaTypeNull_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            personVisa.VisaType = null;
            var result = await personVisasController.PutPersonVisaAsync(id, personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisas_Validate_VisaTypeCategoryNull_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            personVisa.VisaType.VisaTypeCategory = null;
            var result = await personVisasController.PutPersonVisaAsync(id, personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisas_Validate_VisaTypeDetailIdNUll_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            personVisa.VisaType.Detail.Id = null;
            var result = await personVisasController.PutPersonVisaAsync(id, personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisas_Validate_Update_Entries_MoreThan1_InvalidOperationException()
        {
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new ArgumentNullException());
            personVisa.Entries = new List<PersonVisaEntry>() { new PersonVisaEntry() { EnteredOn = new DateTime(2016, 02, 05) }, new PersonVisaEntry() { EnteredOn = new DateTime(2016, 02, 05) } };
            var result = await personVisasController.PutPersonVisaAsync(id, personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_Validate_RequestedOn_GT_IssuedOn_InvalidOperationException()
        {
            personVisa.RequestedOn = new DateTime(2016, 02, 05);
            personVisa.IssuedOn = new DateTime(2016, 02, 04);
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_Validate_RequestedOn_GT_ExpiresOn_InvalidOperationException()
        {
            personVisa.IssuedOn = new DateTime(2016, 02, 06);
            personVisa.RequestedOn = new DateTime(2016, 02, 05);
            personVisa.ExpiresOn = new DateTime(2016, 02, 04);
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_Validate_IssuedOn_GT_ExpiresOn_InvalidOperationException()
        {
            personVisa.IssuedOn = new DateTime(2016, 02, 05);
            personVisa.ExpiresOn = new DateTime(2016, 02, 04);
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_Validate_EnteredOn_LT_IssuedOn_InvalidOperationException()
        {
            personVisa.IssuedOn = new DateTime(2016, 02, 05);
            personVisa.Entries = new List<PersonVisaEntry>() { new PersonVisaEntry() { EnteredOn = new DateTime(2016, 02, 04) } };
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_Validate_EnteredOn_GT_ExpiresOn_InvalidOperationException()
        {
            personVisa.ExpiresOn = new DateTime(2016, 02, 05);
            personVisa.Entries = new List<PersonVisaEntry>() { new PersonVisaEntry() { EnteredOn = new DateTime(2016, 02, 06) } };
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonVisa>())).ThrowsAsync(new InvalidOperationException());
            await personVisasController.PutPersonVisaAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personVisa);
        }
        #endregion

        #region DELETE
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_DeletePersonVisaAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.DeletePersonVisaAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
            await personVisasController.DeletePersonVisaAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_DeletePersonVisaAsync_RepositoryException()
        {
            personVisasServiceMock.Setup(i => i.DeletePersonVisaAsync(It.IsAny<string>())).Throws(new RepositoryException());
            await personVisasController.DeletePersonVisaAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_DeletePersonVisaAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.DeletePersonVisaAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
            await personVisasController.DeletePersonVisaAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_DeletePersonVisaAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.DeletePersonVisaAsync(It.IsAny<string>())).Throws(new Exception());
            await personVisasController.DeletePersonVisaAsync("1234");
        }

        #endregion

        #endregion

        #region All GETS

        [TestMethod]
        public async Task PersonVisasController_GetPersonVisaAllAsync()
        {
            personVisasController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personVisasController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personVisasServiceMock.Setup(i => i.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personVisaTuple);

            var results = await personVisasController.GetAllPersonVisasAsync(It.IsAny<Paging>(), "51f12e6f-b6b1-4cbe-a45b-b4fed77c1dec");

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonVisa> personVisaResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonVisa>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonVisa>;


            Assert.AreEqual(personVisaDtos.Count(), 4);
            Assert.AreEqual(personVisaResults.Count(), 4);
            int resultCounts = personVisaResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personVisaDtos.ToList()[i];
                var actual = personVisaResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Entries.Count(), actual.Entries.Count());
                Assert.AreEqual(expected.ExpiresOn, actual.ExpiresOn);
                Assert.AreEqual(expected.IssuedOn, actual.IssuedOn);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.RequestedOn, actual.RequestedOn);
                Assert.AreEqual(expected.VisaId, actual.VisaId);
                Assert.AreEqual(expected.VisaStatus, actual.VisaStatus);
                Assert.AreEqual(expected.VisaType, actual.VisaType);
            }
        }

        [TestMethod]
        public async Task PersonVisasController_GetPersonVisaAll2Async()
        {
            personVisasController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            personVisasController.Request.Headers.CacheControl =
             new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            personVisasServiceMock.Setup(i => i.GetAll2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personVisaTuple);

            var results = await personVisasController.GetAllPersonVisas2Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>());

            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.PersonVisa> personVisaResults = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonVisa>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.PersonVisa>;


            Assert.AreEqual(personVisaDtos.Count(), 4);
            Assert.AreEqual(personVisaResults.Count(), 4);
            int resultCounts = personVisaResults.Count();

            for (int i = 0; i < resultCounts; i++)
            {
                var expected = personVisaDtos.ToList()[i];
                var actual = personVisaResults[i];

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Entries.Count(), actual.Entries.Count());
                Assert.AreEqual(expected.ExpiresOn, actual.ExpiresOn);
                Assert.AreEqual(expected.IssuedOn, actual.IssuedOn);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.RequestedOn, actual.RequestedOn);
                Assert.AreEqual(expected.VisaId, actual.VisaId);
                Assert.AreEqual(expected.VisaStatus, actual.VisaStatus);
                Assert.AreEqual(expected.VisaType, actual.VisaType);
            }
        }

        [TestMethod]
        public async Task PersonVisasController_GetPersonVisaByIdAsync()
        {
            string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisa);
            var result = await personVisasController.GetPersonVisaByIdAsync(id);
            Assert.AreEqual(personVisa.Id, result.Id);
            Assert.AreEqual(personVisa.ExpiresOn, result.ExpiresOn);
            Assert.AreEqual(personVisa.IssuedOn, result.IssuedOn);
            Assert.AreEqual(personVisa.Person.Id, result.Person.Id);
            Assert.AreEqual(personVisa.RequestedOn, result.RequestedOn);
            Assert.AreEqual(personVisa.VisaId, result.VisaId);
            Assert.AreEqual(personVisa.VisaStatus, result.VisaStatus);
            Assert.AreEqual(personVisa.VisaType.VisaTypeCategory.Value, result.VisaType.VisaTypeCategory.Value);
            Assert.AreEqual(personVisa.VisaType.Detail.Id, result.VisaType.Detail.Id);
            Assert.AreEqual(personVisa.Entries.Count(), result.Entries.Count());
            Assert.AreEqual(result.Entries.Count(), 1);
            Assert.AreEqual(personVisa.Entries.First().EnteredOn, result.Entries.First().EnteredOn);
        }

        [TestMethod]
        public async Task PersonVisasController_GetPersonVisas2Async_WithFilters()
        {
            var filterGroupName = "criteria";

            personVisasController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };

            personVisasController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost"),

            };
            personVisasController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                new Dtos.PersonVisa() { Person = new GuidObject2() { Id = "51f12e6f-b6b1-4cbe-a45b-b4fed77c1dec" } });

            personVisasServiceMock.Setup(x => x.GetAll2Async(0, 100, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(personVisaTuple);

            QueryStringFilter criteria = new QueryStringFilter("criteria", "{'person':{'id': '51f12e6f-b6b1-4cbe-a45b-b4fed77c1dec'}}");

            var results = await personVisasController.GetAllPersonVisas2Async(new Paging(100, 0), criteria);

            Assert.IsNotNull(results);

            var cancelToken = new CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonVisa> actuals =
                ((ObjectContent<IEnumerable<Dtos.PersonVisa>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonVisa>;

            Assert.AreEqual(personVisaDtos.Count(), actuals.Count());

            foreach (var actual in actuals)
            {
                var expected = personVisaDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaAllAsync_ArgumentNullException()
        {
            personVisasServiceMock.Setup(i => i.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
            var results = await personVisasController.GetAllPersonVisasAsync(new Paging(1,1));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaAllAsync_KeyNotFoundException()
        {
            personVisasServiceMock.Setup(i => i.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
            var results = await personVisasController.GetAllPersonVisasAsync(new Paging(1, 1));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_GetPersonVisaAllAsync_Exception()
        {
            personVisasServiceMock.Setup(i => i.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            var results = await personVisasController.GetAllPersonVisasAsync(new Paging(1, 1));
        }
        #endregion

        #region PUT
        [TestMethod]
        public async Task PersonVisasController_PutPersonVisaAsync()
        {
            string id = personVisa.Id;
            personVisasServiceMock.Setup(i => i.PutPersonVisaAsync(id, It.IsAny<Dtos.PersonVisa>())).ReturnsAsync(personVisa);
            personVisasServiceMock.Setup(i => i.GetPersonVisaByIdAsync(id)).ReturnsAsync(personVisa);
            var result = await personVisasController.PutPersonVisaAsync(id, personVisa);
            Assert.AreEqual(personVisa.Id, result.Id);
            Assert.AreEqual(personVisa.ExpiresOn, result.ExpiresOn);
            Assert.AreEqual(personVisa.IssuedOn, result.IssuedOn);
            Assert.AreEqual(personVisa.Person.Id, result.Person.Id);
            Assert.AreEqual(personVisa.RequestedOn, result.RequestedOn);
            Assert.AreEqual(personVisa.VisaId, result.VisaId);
            Assert.AreEqual(personVisa.VisaStatus, result.VisaStatus);
            Assert.AreEqual(personVisa.VisaType.VisaTypeCategory.Value, result.VisaType.VisaTypeCategory.Value);
            Assert.AreEqual(personVisa.VisaType.Detail.Id, result.VisaType.Detail.Id);
            Assert.AreEqual(personVisa.Entries.Count(), result.Entries.Count());
            Assert.AreEqual(result.Entries.Count(), 1);
            Assert.AreEqual(personVisa.Entries.First().EnteredOn, result.Entries.First().EnteredOn);
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PersonVisasController_PostPersonVisaAsync()
        {
            personVisa.Id = Guid.Empty.ToString();
            personVisasServiceMock.Setup(i => i.PostPersonVisaAsync(personVisa)).ReturnsAsync(personVisa);
            var result = await personVisasController.PostPersonVisaAsync(personVisa);
            //Assert.AreEqual(personVisa.Id, result.Id);
            Assert.AreEqual(personVisa.ExpiresOn, result.ExpiresOn);
            Assert.AreEqual(personVisa.IssuedOn, result.IssuedOn);
            Assert.AreEqual(personVisa.Person.Id, result.Person.Id);
            Assert.AreEqual(personVisa.RequestedOn, result.RequestedOn);
            Assert.AreEqual(personVisa.VisaId, result.VisaId);
            Assert.AreEqual(personVisa.VisaStatus, result.VisaStatus);
            Assert.AreEqual(personVisa.VisaType.VisaTypeCategory.Value, result.VisaType.VisaTypeCategory.Value);
            Assert.AreEqual(personVisa.VisaType.Detail.Id, result.VisaType.Detail.Id);
            Assert.AreEqual(personVisa.Entries.Count(), result.Entries.Count());
            Assert.AreEqual(result.Entries.Count(), 1);
            Assert.AreEqual(personVisa.Entries.First().EnteredOn, result.Entries.First().EnteredOn);
        }
        #endregion

        #region DELETE
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonVisasController_DELETE_Not_Supported()
        {
            await personVisasController.DeletePersonVisaAsync(It.IsAny<string>());
        }
        #endregion   
    }
}
