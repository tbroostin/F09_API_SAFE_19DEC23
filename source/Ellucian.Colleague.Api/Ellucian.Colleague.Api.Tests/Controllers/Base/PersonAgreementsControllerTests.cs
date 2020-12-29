// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonAgreementControllerTests
    {
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

        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private Mock<IPersonAgreementsService> personAgreementsServiceMock;
        private IPersonAgreementsService personAgreementsService;
        private ILogger logger = new Mock<ILogger>().Object;

        private PersonAgreementsController controller;

        private PersonAgreementQueryCriteria criteria;
        private List<Dtos.Base.PersonAgreement> personAgreementDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            personAgreementsServiceMock = new Mock<IPersonAgreementsService>();
            personAgreementsService = personAgreementsServiceMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>()).Returns(new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>(adapterRegistry, logger));

            criteria = new PersonAgreementQueryCriteria()
            {
                PersonId = "0001234"
            };
            personAgreementDtos = new List<PersonAgreement>()
            {
                new PersonAgreement()
                {
                    ActionTimestamp = DateTimeOffset.Now.AddDays(-3),
                    AgreementCode = "AGR1",
                    AgreementPeriodCode = "PER1",
                    Title = "Agreement 1",
                    DueDate = DateTime.Today.AddDays(5),
                    Id = "1",
                    PersonCanDeclineAgreement = true,
                    PersonId = "0001234",
                    Status = PersonAgreementStatus.Declined,
                    Text = new List<string>() {"Agreement 1 Text" }
                },
                new PersonAgreement()
                {
                    ActionTimestamp = DateTimeOffset.Now.AddDays(-2),
                    AgreementCode = "AGR2",
                    AgreementPeriodCode = "PER2",
                    Title = "Agreement 2",
                    DueDate = DateTime.Today.AddDays(3),
                    Id = "2",
                    PersonCanDeclineAgreement = true,
                    PersonId = "0002234",
                    Status = PersonAgreementStatus.Accepted,
                    Text = new List<string>() {"Agreement 2 Text" }
                },
                new PersonAgreement()
                {
                    ActionTimestamp = null,
                    AgreementCode = "AGR3",
                    AgreementPeriodCode = "PER3",
                    Title = "Agreement 3",
                    DueDate = DateTime.Today.AddDays(1),
                    Id = "3",
                    PersonCanDeclineAgreement = true,
                    PersonId = "0003334",
                    Status = null,
                    Text = new List<string>() {"Agreement 3 Text" }
                },
            };

            controller = new PersonAgreementsController(adapterRegistry, personAgreementsService, logger);
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            personAgreementsService = null;
        }

        [TestClass]
        public class PersonAgreementsController_QueryPersonAgreementsByPost_Tests : PersonAgreementControllerTests
        {
            [TestInitialize]
            public void PersonAgreementsController_QueryPersonAgreementsByPost_Async_Tests_Initialize()
            {
                base.Initialize();

                personAgreementsServiceMock.Setup(repo => repo.GetPersonAgreementsByPersonIdAsync(It.IsAny<string>())).ReturnsAsync(personAgreementDtos);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsController_QueryPersonAgreementsByPostAsync_null_criteria_throws_ArgumentNullException()
            {
                var aps = await controller.QueryPersonAgreementsByPostAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsController_QueryPersonAgreementsByPostAsync_null_criteria_PersonId_throws_ArgumentNullException()
            {
                var aps = await controller.QueryPersonAgreementsByPostAsync(new PersonAgreementQueryCriteria() { PersonId = null });
            }

            [TestMethod]
            public async Task PersonAgreementsController_QueryPersonAgreementsByPostAsync_ValidateFields()
            {
                var aps = (await controller.QueryPersonAgreementsByPostAsync(criteria)).ToList();
                Assert.AreEqual(personAgreementDtos.Count, aps.Count);
                for (int i = 0; i < aps.Count; i++)
                {
                    var expected = personAgreementDtos[i];
                    var actual = aps[i];
                    Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                    Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                    Assert.AreEqual(expected.ActionTimestamp, actual.ActionTimestamp, "ActionTimestamp, Index=" + i.ToString());
                    Assert.AreEqual(expected.AgreementCode, actual.AgreementCode, "AgreementCode, Index=" + i.ToString());
                    Assert.AreEqual(expected.AgreementPeriodCode, actual.AgreementPeriodCode, "AgreementPeriodCode, Index=" + i.ToString());
                    Assert.AreEqual(expected.DueDate, actual.DueDate, "DueDate, Index=" + i.ToString());
                    Assert.AreEqual(expected.PersonCanDeclineAgreement, actual.PersonCanDeclineAgreement, "PersonCanDeclineAgreement, Index=" + i.ToString());
                    Assert.AreEqual(expected.PersonId, actual.PersonId, "ActionTimestamp, Index=" + i.ToString());
                    Assert.AreEqual(expected.Status.ToString(), actual.Status.ToString(), "Status, Index=" + i.ToString());
                    Assert.AreEqual(expected.Text, actual.Text, "ActionTimestamp, Index=" + i.ToString());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonAgreementsController_QueryPersonAgreementsByPostAsync_handles_caught_PermissionsException()
            {
                personAgreementsServiceMock.Setup(repo => repo.GetPersonAgreementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var aps = await controller.QueryPersonAgreementsByPostAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonAgreementsController_QueryPersonAgreementsByPostAsync_handles_caught_Exception()
            {
                personAgreementsServiceMock.Setup(repo => repo.GetPersonAgreementsByPersonIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                var aps = await controller.QueryPersonAgreementsByPostAsync(criteria);
            }
        }

        [TestClass]
        public class PersonAgreementsController_UpdatePersonAgreementAsync_Tests : PersonAgreementControllerTests
        {
            [TestInitialize]
            public void PersonAgreementsController_UpdatePersonAgreementAsync_Tests_Initialize()
            {
                base.Initialize();

                personAgreementsServiceMock.Setup(svc => svc.UpdatePersonAgreementAsync(It.IsAny<PersonAgreement>())).ReturnsAsync(personAgreementDtos[0]);

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonAgreementsController_UpdatePersonAgreementAsyncAsync_null_criteria_throws_ArgumentNullException()
            {
                var aps = await controller.UpdatePersonAgreementAsync(null);
            }

            [TestMethod]
            public async Task PersonAgreementsController_UpdatePersonAgreementAsyncAsync_ValidateFields()
            {
                var actual = await controller.UpdatePersonAgreementAsync(personAgreementDtos[0]);
                Assert.IsNotNull(actual);
                Assert.AreEqual(personAgreementDtos[0].Id, actual.Id);
                Assert.AreEqual(personAgreementDtos[0].Title, actual.Title);
                Assert.AreEqual(personAgreementDtos[0].ActionTimestamp, actual.ActionTimestamp);
                Assert.AreEqual(personAgreementDtos[0].AgreementCode, actual.AgreementCode);
                Assert.AreEqual(personAgreementDtos[0].AgreementPeriodCode, actual.AgreementPeriodCode);
                Assert.AreEqual(personAgreementDtos[0].DueDate, actual.DueDate);
                Assert.AreEqual(personAgreementDtos[0].PersonCanDeclineAgreement, actual.PersonCanDeclineAgreement);
                Assert.AreEqual(personAgreementDtos[0].PersonId, actual.PersonId);
                Assert.AreEqual(personAgreementDtos[0].Status.ToString(), actual.Status.ToString());
                CollectionAssert.AreEqual(personAgreementDtos[0].Text.ToList(), actual.Text.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonAgreementsController_UpdatePersonAgreementAsync_handles_caught_PermissionsException()
            {
                personAgreementsServiceMock.Setup(repo => repo.UpdatePersonAgreementAsync(It.IsAny<PersonAgreement>())).ThrowsAsync(new PermissionsException());
                var aps = await controller.UpdatePersonAgreementAsync(personAgreementDtos[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonAgreementsController_UpdatePersonAgreementAsync_handles_caught_Exception()
            {
                personAgreementsServiceMock.Setup(repo => repo.UpdatePersonAgreementAsync(It.IsAny<PersonAgreement>())).ThrowsAsync(new PermissionsException());
                var aps = await controller.UpdatePersonAgreementAsync(personAgreementDtos[0]);
            }
        }

    }
}