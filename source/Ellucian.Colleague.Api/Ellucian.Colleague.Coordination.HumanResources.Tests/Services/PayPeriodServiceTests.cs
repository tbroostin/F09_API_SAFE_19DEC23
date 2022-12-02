//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
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
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayPeriodServiceTests_V12 : CurrentUserSetup
    {
        Mock<IPayCycleRepository> payCycleRepositoryMock;
        Mock<IPayPeriodsRepository> payPeriodRepositoryMock;
        Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;

        PayPeriodsService payPeriodService;
        IEnumerable<Domain.HumanResources.Entities.PayPeriod> payPeriodEntities;
        Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int> payPeriodEntityTuple;

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
        IEnumerable<Domain.HumanResources.Entities.PayCycle2> payCycleEntities;

        int offset = 0;
        int limit = 4;

        [TestInitialize]
        public void Initialize()
        {
            payCycleRepositoryMock = new Mock<IPayCycleRepository>();
            payPeriodRepositoryMock = new Mock<IPayPeriodsRepository>();
            hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            BuildData();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            payPeriodService = new PayPeriodsService(payPeriodRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                           adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            payPeriodEntityTuple = null;
            payPeriodEntities = null;
            employmentClassificationEntities = null;
            payCycleEntities = null;
            payPeriodRepositoryMock = null;
            hrReferenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            referenceDataRepositoryMock = null;
        }

        [TestMethod]
        public async Task PayPeriods_GETAllAsync()
        {
            var actualsTuple =
                await
                    payPeriodService.GetPayPeriodsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);

            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = payPeriodEntities.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task PayPeriods_GETAllFilterAsync()
        {
            var actualsTuple =
                await
                    payPeriodService.GetPayPeriodsAsync(offset, limit, "cd385d31-75ed-4d93-9a1b-4776a951396d", "2000-01-01 00:00:00.000",
                    "2020-12-31 00:00:00.000", It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);

            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = payPeriodEntities.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }
        }

        [TestMethod]
        public async Task PayPeriods_GETAllAsync_EmptyTuple()
        {
            payPeriodEntities = new List<Domain.HumanResources.Entities.PayPeriod>()
            {

            };
            payPeriodEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int>(payPeriodEntities, 0);
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payPeriodEntityTuple);
            var actualsTuple = await payPeriodService.GetPayPeriodsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task PayPeriods_GETAllAsync_EmptyTuple_InvalidPayCycleFilter()
        {
            payPeriodEntities = new List<Domain.HumanResources.Entities.PayPeriod>()
            {

            };
            payPeriodEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int>(payPeriodEntities, 0);
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payPeriodEntityTuple);
            var actualsTuple = await payPeriodService.GetPayPeriodsAsync(offset, limit, "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task PayPeriods_GETAllAsync_EmptyTuple_InvalidStartDateFilter()
        {
            payPeriodEntities = new List<Domain.HumanResources.Entities.PayPeriod>()
            {

            };
            payPeriodEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int>(payPeriodEntities, 0);
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payPeriodEntityTuple);
            var actualsTuple = await payPeriodService.GetPayPeriodsAsync(offset, limit, It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task PayPeriods_GETAllAsync_EmptyTuple_InvalidEndDateFilter()
        {
            payPeriodEntities = new List<Domain.HumanResources.Entities.PayPeriod>()
            {

            };
            payPeriodEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int>(payPeriodEntities, 0);
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payPeriodEntityTuple);
            var actualsTuple = await payPeriodService.GetPayPeriodsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task PayPeriods_GET_ById()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            var expected = payPeriodEntities.ToList()[0];
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(id)).ReturnsAsync(expected);
            var actual = await payPeriodService.GetPayPeriodsByGuidAsync(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Id, actual.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PayPeriods_GET_ById_NullId_ArgumentNullException()
        {
            await payPeriodService.GetPayPeriodsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayPeriods_GET_ById_ReturnsNullEntity_KeyNotFoundException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(id)).Throws<KeyNotFoundException>();
            await payPeriodService.GetPayPeriodsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayPeriods_GET_ById_ReturnsNullEntity_InvalidOperationException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(id)).Throws<InvalidOperationException>();
            await payPeriodService.GetPayPeriodsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task PayPeriods_GET_ById_ReturnsNullEntity_RepositoryException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(id)).Throws<RepositoryException>();
            await payPeriodService.GetPayPeriodsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PayPeriods_GET_ById_ReturnsNullEntity_Exception()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(id)).Throws<Exception>();
            await payPeriodService.GetPayPeriodsByGuidAsync(id);
        }

        private void BuildData()
        {
            payCycleEntities = new List<Domain.HumanResources.Entities.PayCycle2>()
                {
                    new Domain.HumanResources.Entities.PayCycle2("cd385d31-75ed-4d93-9a1b-4776a951396d", "Admissions", "Admissions"),
                    new Domain.HumanResources.Entities.PayCycle2("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.HumanResources.Entities.PayCycle2("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.HumanResources.Entities.PayCycle2("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.HumanResources.Entities.PayCycle2("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
            hrReferenceDataRepositoryMock.Setup(i => i.GetPayCyclesAsync(It.IsAny<bool>())).ReturnsAsync(payCycleEntities);

            //hrReferenceDataRepositoryMock.Setup(i => i.GetPayCyclesAsync(It.IsAny<bool>())).ReturnsAsync(new List<PayCycle2>() { new PayCycle2("cd385d31-75ed-4d93-9a1b-4776a951396d", "CODE", "DESC") });


            payPeriodEntities = new List<Domain.HumanResources.Entities.PayPeriod>()
                {
                    new Domain.HumanResources.Entities.PayPeriod("ce4d68f6-257d-4052-92c8-17eed0f088fa", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Admissions")
                    {
                        TimeEntryEndOn = DateTime.Now,
                    },
                    new Domain.HumanResources.Entities.PayPeriod("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Agriculture Business")
                    {
                        
                    },
                    new Domain.HumanResources.Entities.PayPeriod("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Agriculture Mechanics")
                    {
                        TimeEntryEndOn = DateTime.Now,
                    },
                    new Domain.HumanResources.Entities.PayPeriod("db8f690b-071f-4d98-8da8-d4312511a4c1", "DESC", DateTime.Now, DateTime.Now, DateTime.Now, "Animal Science")
                    {
                        
                    }
                };
            payPeriodEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.PayPeriod>, int>(payPeriodEntities, payPeriodEntities.Count());
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(payPeriodEntityTuple);
            payPeriodRepositoryMock.Setup(i => i.GetPayPeriodByIdAsync(It.IsAny<string>())).ReturnsAsync(payPeriodEntities.ToList()[0]);
            hrReferenceDataRepositoryMock.Setup(i => i.GetPayCyclesAsync(It.IsAny<bool>())).ReturnsAsync(payCycleEntities);

        }
    }

}