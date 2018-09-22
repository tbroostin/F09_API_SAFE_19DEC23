// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class TaxFormConsentServiceTests : GenericUserFactory
    {
        #region Initialize and Cleanup
        private TaxFormConsentService service = null;
        private Mock<ITaxFormConsentRepository> mockTaxFormStatementRepository;
        private TestTaxFormConsentRepository testRepository;
        private ICurrentUserFactory currentUserFactory;
        private string personId = "000001";

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the mock repository
            this.mockTaxFormStatementRepository = new Mock<ITaxFormConsentRepository>();

            // Build all service objects to use each of the user factories built above
            BuildTaxFormConsentService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            testRepository = null;
            mockTaxFormStatementRepository = null;
        }
        #endregion

        #region Tests for GetAsync
        [TestMethod]
        public async Task GetAsync_W2_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.FormW2.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_1095_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.Form1095C.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_1098_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1098);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.Form1098);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.Form1098.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_T4_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormT4);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.FormT4);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.FormT4.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_T4A_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormT4A);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.FormT4A);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.FormT4A.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_T2202A_Success()
        {
            var personId = "0003946";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormT2202A);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.FormT2202A);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.FormT2202A.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_1099MI_Success()
        {
            var personId = "000001";
            var expectedConsents = await this.testRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1099MI);
            var actualConsents = await this.service.GetAsync(personId, Dtos.Base.TaxForms.Form1099MI);

            Assert.AreEqual(expectedConsents.Count(), actualConsents.Count(), "The two lists should have the same number of objects.");
            foreach (var expectedConsent in expectedConsents)
            {
                var matchingActualConsents = actualConsents.Where(x =>
                    x.HasConsented == expectedConsent.HasConsented
                    && x.PersonId == expectedConsent.PersonId
                    && x.TaxForm.ToString() == Dtos.Base.TaxForms.Form1099MI.ToString()
                    && x.TimeStamp == expectedConsent.TimeStamp).ToList();
                Assert.AreEqual(1, matchingActualConsents.Count, "Each expected domain entity should have a matching DTO.");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyPersonIdFor1099Mi()
        {
            Dtos.Base.TaxForms taxForm = Dtos.Base.TaxForms.Form1099MI;
            await this.service.GetAsync(string.Empty, taxForm);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullPersonIdfor1099MI()
        {
            Dtos.Base.TaxForms taxForm = Dtos.Base.TaxForms.Form1099MI;
            await this.service.GetAsync(null, taxForm);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullPersonId()
        {
            Dtos.Base.TaxForms taxForm = Dtos.Base.TaxForms.FormW2;
            await this.service.GetAsync(null, taxForm);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyPersonId()
        {
            Dtos.Base.TaxForms taxForm = Dtos.Base.TaxForms.FormW2;
            await this.service.GetAsync(string.Empty, taxForm);
        }
        #endregion

        #region Tests for PostAsync
        [TestMethod]
        public async Task PostAsync_Success()
        {
            bool hasConsented = true;
            DateTimeOffset timeStamp = new DateTimeOffset(new DateTime(2015, 7, 10, 9, 47, 13));
            Dtos.Base.TaxForms taxFormDto = Dtos.Base.TaxForms.FormW2;
            Domain.Base.Entities.TaxForms taxFormEntity = Domain.Base.Entities.TaxForms.FormW2;
            var incomingConsentDto = new Dtos.Base.TaxFormConsent()
            {
                HasConsented = hasConsented,
                PersonId = personId,
                TimeStamp = timeStamp,
                TaxForm = taxFormDto
            };
            var incomingConsentEntity = new Domain.Base.Entities.TaxFormConsent(personId, taxFormEntity, hasConsented, timeStamp);
            var expectedConsent = await this.testRepository.PostAsync(incomingConsentEntity);
            var actualConsent = await this.service.PostAsync(incomingConsentDto);

            Assert.AreEqual(expectedConsent.HasConsented, actualConsent.HasConsented);
            Assert.AreEqual(expectedConsent.PersonId, actualConsent.PersonId);
            Assert.AreEqual(expectedConsent.TaxForm.ToString(), actualConsent.TaxForm.ToString());
            Assert.AreEqual(expectedConsent.TimeStamp, actualConsent.TimeStamp);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PostAsync_NullConsentObject()
        {
            await this.service.PostAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task PostAsync_CurrentUserDifferentFromRequest()
        {
            bool hasConsented = true;
            DateTimeOffset timeStamp = new DateTimeOffset(new DateTime(2015, 7, 10, 9, 47, 13));
            Dtos.Base.TaxForms taxFormDto = Dtos.Base.TaxForms.FormW2;
            Domain.Base.Entities.TaxForms taxFormEntity = Domain.Base.Entities.TaxForms.FormW2;
            var incomingConsentDto = new Dtos.Base.TaxFormConsent()
            {
                HasConsented = hasConsented,
                PersonId = "0000002",
                TimeStamp = timeStamp,
                TaxForm = taxFormDto
            };
            var incomingConsentEntity = new Domain.Base.Entities.TaxFormConsent(personId, taxFormEntity, hasConsented, timeStamp);
            var expectedConsent = await this.testRepository.PostAsync(incomingConsentEntity);
            var actualConsent = await this.service.PostAsync(incomingConsentDto);

            Assert.AreEqual(expectedConsent.HasConsented, actualConsent.HasConsented);
            Assert.AreEqual(expectedConsent.PersonId, actualConsent.PersonId);
            Assert.AreEqual(expectedConsent.TaxForm.ToString(), actualConsent.TaxForm.ToString());
            Assert.AreEqual(expectedConsent.TimeStamp, actualConsent.TimeStamp);
        }

        #endregion

        public void BuildTaxFormConsentService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            var roleRepositoryMock = new Mock<IRoleRepository>();
            var loggerObject = new Mock<ILogger>().Object;
           
            // Set up current user
            currentUserFactory = new GenericUserFactory.TaxInformationUserFactory();

            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.W2");
            role.AddPermission(new Domain.Entities.Permission("VIEW.W2"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.W2"));
            roles.Add(role);

            role = new Domain.Entities.Role(2, "VIEW.1095C");
            role.AddPermission(new Domain.Entities.Permission("VIEW.1095C"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.1095C"));
            roles.Add(role);

            role = new Domain.Entities.Role(3, "VIEW.T4");
            role.AddPermission(new Domain.Entities.Permission("VIEW.T4"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.EMPLOYEE.T4"));
            roles.Add(role);

            role = new Domain.Entities.Role(1, "VIEW.T4A");
            role.AddPermission(new Domain.Entities.Permission("VIEW.T4A"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.RECIPIENT.T4A"));
            roles.Add(role);

            role = new Domain.Entities.Role(1, "VIEW.1098");
            role.AddPermission(new Domain.Entities.Permission("VIEW.1098"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.1098"));
            roles.Add(role);

            role = new Domain.Entities.Role(1, "VIEW.T2202A");
            role.AddPermission(new Domain.Entities.Permission("VIEW.T2202A"));
            role.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.T2202A"));
            roles.Add(role);

            role = new Domain.Entities.Role(1, "VIEW.1099MISC");
            role.AddPermission(new Domain.Entities.Permission("VIEW.1099MISC"));
            roles.Add(role);

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            testRepository = new TestTaxFormConsentRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();

            // Set up the entity to DTO adapter
            var entityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormConsent, Dtos.Base.TaxFormConsent>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormConsent, Dtos.Base.TaxFormConsent>()).Returns(entityToDtoAdapter);

            // Set up the DTO to entity adapter
            var dtoToEntityAdapter = new AutoMapperAdapter<Dtos.Base.TaxFormConsent, Domain.Base.Entities.TaxFormConsent>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.Base.TaxFormConsent, Domain.Base.Entities.TaxFormConsent>()).Returns(dtoToEntityAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new TaxFormConsentService(testRepository, adapterRegistry.Object, currentUserFactory, roleRepositoryMock.Object, loggerObject);
        }
    }
}
