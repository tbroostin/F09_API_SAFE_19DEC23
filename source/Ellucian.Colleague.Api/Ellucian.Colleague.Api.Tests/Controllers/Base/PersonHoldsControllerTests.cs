// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonHoldsControllerTests
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

        private PersonHoldsController personHoldsController;
        private Mock<IPersonHoldsService> personHoldsServiceMock;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        private IAdapterRegistry AdapterRegistry;
        private List<Dtos.PersonHold> personHoldDtoList = new List<PersonHold>();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            personHoldsServiceMock = new Mock<IPersonHoldsService>();

            HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
            AdapterRegistry = new AdapterRegistry(adapters, loggerMock.Object);
            var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Restriction, RestrictionType2>(AdapterRegistry, loggerMock.Object);
            AdapterRegistry.AddAdapter(testAdapter);

            var personHoldTypeList = new List<PersonHoldType>();

            personHoldDtoList = new TestPersonHoldsRepository().GetPersonHolds() as List<Dtos.PersonHold>;

            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);

            personHoldsServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

            personHoldsController = new PersonHoldsController(AdapterRegistry, personHoldsServiceMock.Object,
                loggerMock.Object) {Request = new HttpRequestMessage()};
            personHoldsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            personHoldsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(personHold));

            
        }

        [TestCleanup]
        public void Cleanup()
        {
            personHoldsController = null;
            personHoldDtoList = null;
        }       

        #region Exceptions Testing
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10,10));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.GetPersonsActiveHoldAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

            await personHoldsController.GetPersonsActiveHoldAsync(It.IsAny<string>());
        }

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_ArgumentNullException()
        //{
        //    personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

        //    await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(It.IsAny<string>());
        //}

        //[TestMethod]
        //[ExpectedException(typeof(HttpResponseException))]
        //public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync_Exception()
        //{
        //    personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

        //    await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(It.IsAny<string>());
        //}

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Null_Id()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Null_DTO()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("1234", null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Id_NoMatch_PersonHoldId()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold() { Id = "5678"});
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_NilGUID_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync(Guid.Empty.ToString(), new Dtos.PersonHold() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_NilGuid_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PutPersonHoldAsync(Guid.Empty.ToString(), new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_KeyNotFoundException()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new KeyNotFoundException());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PutPersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(It.IsAny<string>(), It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());

            await personHoldsController.PutPersonHoldAsync("1234", new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_Null_DTO()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PostPersonHoldAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new ArgumentNullException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_InvalidOperationException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new InvalidOperationException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_KeyNotFoundException()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new KeyNotFoundException());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_PostPersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(It.IsAny<Dtos.PersonHold>())).ThrowsAsync(new Exception());

            await personHoldsController.PostPersonHoldAsync(new Dtos.PersonHold());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_ArgumentNullException()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync(It.IsAny<string>())).Throws(new ArgumentNullException());

            await personHoldsController.DeletePersonHoldAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_Id_Null()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync(It.IsAny<string>())).Throws(new ArgumentNullException());

            await personHoldsController.DeletePersonHoldAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_Exception()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Throws(new Exception());

            await personHoldsController.DeletePersonHoldAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonHoldsController_DeletePersonHoldAsync_PermissionsException()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Throws(new PermissionsException());

            await personHoldsController.DeletePersonHoldAsync("1234");
        }
        #endregion

        #region All GETS
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsAsync()
        {
            personHoldsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            var tuple = new Tuple<IEnumerable<Dtos.PersonHold>, int>(personHoldDtoList, 5);
            
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            var personHolds = await personHoldsController.GetPersonsActiveHoldsAsync(new Paging(10,0));
           
            var cancelToken = new System.Threading.CancellationToken(false);

            System.Net.Http.HttpResponseMessage httpResponseMessage = await personHolds.ExecuteAsync(cancelToken);

            IEnumerable<Dtos.PersonHold> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.PersonHold>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.PersonHold>;
            
            var result = results.FirstOrDefault();

            Assert.IsTrue(personHolds is IHttpActionResult);
            
            foreach (var personHold in personHoldDtoList)
            {
                var persHold = results.FirstOrDefault(i => i.Id == personHold.Id);

                Assert.AreEqual(personHold.Id, persHold.Id);
                Assert.AreEqual(personHold.Comment, persHold.Comment);
                Assert.AreEqual(personHold.EndOn, persHold.EndOn);
                Assert.AreEqual(personHold.NotificationIndicator, persHold.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, persHold.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType, persHold.PersonHoldTypeType);
                Assert.AreEqual(personHold.StartOn, persHold.StartOn);
            }
        }
        
        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldAsync()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(id, It.IsAny<bool>())).ReturnsAsync(personHold);
            var result = await personHoldsController.GetPersonsActiveHoldAsync(id);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }

        [TestMethod]
        public async Task PersonHoldsController_GetPersonsActiveHoldsByPersonIdAsync()
        {
            var personId = "895cebf0-e6e8-4169-aac6-e0e14dfefdd4";
            var personHoldsByPersId = personHoldDtoList.Where(i => i.Person.Id.Equals(personId));
            personHoldsServiceMock.Setup(s => s.GetPersonHoldsAsync(personId, It.IsAny<bool>())).ReturnsAsync(personHoldsByPersId);
            var result = await personHoldsController.GetPersonsActiveHoldsByPersonIdAsync(personId);

            Assert.AreEqual(personHoldsByPersId.Count(), result.Count());
            foreach (var personHold in personHoldsByPersId)
            {
                var persHold = result.FirstOrDefault(i => i.Id == personHold.Id);

                Assert.AreEqual(personHold.Id, persHold.Id);
                Assert.AreEqual(personHold.Comment, persHold.Comment);
                Assert.AreEqual(personHold.EndOn, persHold.EndOn);
                Assert.AreEqual(personHold.NotificationIndicator, persHold.NotificationIndicator);
                Assert.AreEqual(personHold.Person.Id, persHold.Person.Id);
                Assert.AreEqual(personHold.PersonHoldTypeType, persHold.PersonHoldTypeType);
                Assert.AreEqual(personHold.StartOn, persHold.StartOn);
            }
        }         
        #endregion

        #region PUT
        [TestMethod]
        public async Task PersonHoldsController_PutPersonHoldAsync()
        {
            var id = "65747675-f4ca-4e8b-91aa-d37c3449a82c";
            var personHold = personHoldDtoList.FirstOrDefault(i => i.Id == id);
            personHoldsServiceMock.Setup(s => s.UpdatePersonHoldAsync(id, It.IsAny<Dtos.PersonHold>())).ReturnsAsync(personHold);
            personHoldsServiceMock.Setup(s => s.GetPersonHoldAsync(id, It.IsAny<bool>())).ReturnsAsync(personHold);
            var result = await personHoldsController.PutPersonHoldAsync(id, personHold);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }
        #endregion

        #region POST
        [TestMethod]
        public async Task PersonHoldsController_PostPersonHoldAsync()
        {
            var id = Guid.Empty.ToString();
            var personHold = personHoldDtoList.First();
            personHold.Id = id;
            personHoldsServiceMock.Setup(s => s.CreatePersonHoldAsync(personHold)).ReturnsAsync(personHold);
            var result = await personHoldsController.PostPersonHoldAsync(personHold);

            Assert.AreEqual(personHold.Id, result.Id);
            Assert.AreEqual(personHold.Comment, result.Comment);
            Assert.AreEqual(personHold.EndOn, result.EndOn);
            Assert.AreEqual(personHold.NotificationIndicator, result.NotificationIndicator);
            Assert.AreEqual(personHold.Person.Id, result.Person.Id);
            Assert.AreEqual(personHold.PersonHoldTypeType, result.PersonHoldTypeType);
            Assert.AreEqual(personHold.StartOn, result.StartOn);
        }
        #endregion

        #region DELETE
        [TestMethod]
        public async Task PersonHoldsController_DeletePersonHoldAsync_HttpResponseMessage()
        {
            personHoldsServiceMock.Setup(s => s.DeletePersonHoldAsync("1234")).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            await personHoldsController.DeletePersonHoldAsync("1234");
        }
        #endregion   
    }
}
