// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonContactsControllerTests
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

        private PersonContactsController personContactsController;
        Mock<ILogger> loggerMock = new Mock<ILogger>();
        IAdapterRegistry AdapterRegistry;
        Mock<IEmergencyInformationService> emergencyInformationServiceMock = new Mock<IEmergencyInformationService>();

        Ellucian.Colleague.Dtos.PersonContactSubject personContact;
        string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            BuildPersonContactDto();

            personContactsController = new PersonContactsController(AdapterRegistry, emergencyInformationServiceMock.Object, loggerMock.Object);
            personContactsController.Request = new HttpRequestMessage();
            personContactsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        private void BuildPersonContactDto()
        {
            personContact = new Dtos.PersonContactSubject()
            {
                Id = id,
                Person = new GuidObject2(Guid.NewGuid().ToString()),
                Contacts = new List<Dtos.PersonContact>()
                {
                    new Dtos.PersonContact()
                    {
                        ContactName = new PersonContactName()
                        {
                            FullName = "Dr. Gary J. Johnson, III",
                            FirstName = "Gary",
                            MiddleName = "J.",
                            LastName = "Johnson"
                        },
                        ContactPriority = 1,
                        ContactAddress = new PersonContactAddress()
                        {
                            ContactFullAddress = new List<string>() { "123 Main Street, Fairfax, VA  20122" }
                        },
                        ContactRelationship = "Father",
                        PersonContactPhone = new List<Dtos.DtoProperties.PersonContactPhoneDtoProperty>()
                        {
                            new Dtos.DtoProperties.PersonContactPhoneDtoProperty() 
                            { 
                                Number = "(703) 659-9950",
                                ContactAvailability = Dtos.EnumProperties.BestContactTimeType.Anytime
                            }
                        },
                        Types = new List<Dtos.EnumProperties.PersonContactType>()
                        {
                            Dtos.EnumProperties.PersonContactType.Emergency
                        }
                    }
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            personContactsController = null;
            personContact = null;
        }

        #region Exceptions Testing

        #region GET
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_GetPersonEmergencyContactsByIdAsyncAsync_ArgumentNullException()
        {
            emergencyInformationServiceMock.Setup(i => i.GetPersonEmergencyContactByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await personContactsController.GetPersonEmergencyContactsByIdAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_GetPersonEmergencyContactsByIdAsyncAsync_KeyNotFoundException()
        {
            emergencyInformationServiceMock.Setup(i => i.GetPersonEmergencyContactByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await personContactsController.GetPersonEmergencyContactsByIdAsync("1234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_GetPersonEmergencyContactsByIdAsyncAsync_Exception()
        {
            emergencyInformationServiceMock.Setup(i => i.GetPersonEmergencyContactByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await personContactsController.GetPersonEmergencyContactsByIdAsync("1234");
        }
        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_PostPersonContactAsync_InvalidOperationException()
        {
            await personContactsController.PostPersonContactAsync(personContact);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_PutPersonContactAsync_IdNotSame_InvalidOperationException()
        {
            personContact.Id = "475ef15b-f2d2-40ed-ac47-f0d2d45260d1";
            await personContactsController.PutPersonContactAsync("375ef15b-f2d2-40ed-ac47-f0d2d45260f0", personContact);
        }

        #endregion

        #region DELETE

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task PersonContactsController_DeletePersonContactAsync_KeyNotFoundException()
        {
            await personContactsController.DeletePersonContactAsync("1234");
        }

        #endregion

        #endregion Exceptions Testing

        #region All GETS

        [TestMethod]
        public async Task PersonContactsController_GetPersonEmergencyContactsByIdAsync()
        {
            string id = "375ef15b-f2d2-40ed-ac47-f0d2d45260f0";
            emergencyInformationServiceMock.Setup(i => i.GetPersonEmergencyContactByIdAsync(id)).ReturnsAsync(personContact);
            var result = await personContactsController.GetPersonEmergencyContactsByIdAsync(id);
            Assert.AreEqual(personContact.Id, result.Id);
            Assert.AreEqual(personContact.Person.Id, result.Person.Id);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).ContactName, result.Contacts.ElementAt(0).ContactName);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).ContactAddress, result.Contacts.ElementAt(0).ContactAddress);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).ContactPriority, result.Contacts.ElementAt(0).ContactPriority);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).ContactRelationship, result.Contacts.ElementAt(0).ContactRelationship);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).Types, result.Contacts.ElementAt(0).Types);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).PersonContactPhone.ElementAt(0).Number, result.Contacts.ElementAt(0).PersonContactPhone.ElementAt(0).Number);
            Assert.AreEqual(personContact.Contacts.ElementAt(0).PersonContactPhone.ElementAt(0).ContactAvailability, result.Contacts.ElementAt(0).PersonContactPhone.ElementAt(0).ContactAvailability);
        }
        
        [TestMethod]
        public async Task PersonContactsController_GetPersonEmergencyContactsAsync()
        {
            var page = new Paging(200, 0);
            var tupple = new Tuple<IEnumerable<Dtos.PersonContactSubject>, int>(new List<Dtos.PersonContactSubject>() { personContact}, 1);
            emergencyInformationServiceMock.Setup(i => i.GetPersonEmergencyContactsAsync(0, 200, false, It.IsAny<string>())).ReturnsAsync(tupple);
            var results = await personContactsController.GetPersonEmergencyContactsAsync(page);

            Assert.IsTrue(results is IHttpActionResult);
        }

        #endregion
    }
}
