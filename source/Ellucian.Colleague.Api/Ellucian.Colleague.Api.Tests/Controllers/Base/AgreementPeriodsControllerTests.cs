// Copyright 2019 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
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
    public class AgreementPeriodControllerTests
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
        private Mock<IAgreementsRepository> agreementsRepositoryMock;
        private IAgreementsRepository agreementsRepository;
        private ILogger logger = new Mock<ILogger>().Object;

        private AgreementPeriodsController controller;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod> cachedAgreementPeriodEntities;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod> nonCachedAgreementPeriodEntities;

        private List<Dtos.Base.AgreementPeriod> cachedAgreementPeriodDtos;
        private List<Dtos.Base.AgreementPeriod> nonCachedAgreementPeriodDtos;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            agreementsRepositoryMock = new Mock<IAgreementsRepository>();
            agreementsRepository = agreementsRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            adapterRegistryMock.Setup(ar => ar.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>()).Returns(new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>(adapterRegistry, logger));

            cachedAgreementPeriodEntities = new List<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod>()
            {
                new Domain.Base.Entities.AgreementPeriod("2018FA", "2018 Fall"),
                new Domain.Base.Entities.AgreementPeriod("2019FA", "2019 Fall"),
            };
            nonCachedAgreementPeriodEntities = new List<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod>()
            {
                new Domain.Base.Entities.AgreementPeriod("2018FA", "2018 Fall"),
                new Domain.Base.Entities.AgreementPeriod("2019FA", "2019 Fall"),
                new Domain.Base.Entities.AgreementPeriod("2020FA", "2020 Fall"),
                new Domain.Base.Entities.AgreementPeriod("2021FA", "2021 Fall"),
            };

            agreementsRepositoryMock.Setup(repo => repo.GetAgreementPeriodsAsync(true)).ReturnsAsync(nonCachedAgreementPeriodEntities);
            agreementsRepositoryMock.Setup(repo => repo.GetAgreementPeriodsAsync(false)).ReturnsAsync(cachedAgreementPeriodEntities);
            cachedAgreementPeriodDtos = new List<AgreementPeriod>();
            nonCachedAgreementPeriodDtos = new List<AgreementPeriod>();

            Mapper.CreateMap<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>();
            foreach (var ap in cachedAgreementPeriodEntities)
            {
                AgreementPeriod target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>(ap);
                target.Code = ap.Code;
                target.Description = ap.Description;
                cachedAgreementPeriodDtos.Add(target);
            }
            foreach (var ap in nonCachedAgreementPeriodEntities)
            {
                AgreementPeriod target = Mapper.Map<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>(ap);
                target.Code = ap.Code;
                target.Description = ap.Description;
                nonCachedAgreementPeriodDtos.Add(target);
            }

            controller = new AgreementPeriodsController(adapterRegistry, agreementsRepository, logger);
            controller.Request = new HttpRequestMessage();
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            controller = null;
            agreementsRepository = null;
        }

        [TestMethod]
        public async Task AgreementPeriodsController_GetAgreementPeriodsAsync_ValidateFields()
        {
            var aps = (await controller.GetAgreementPeriodsAsync()).ToList();
            Assert.AreEqual(cachedAgreementPeriodDtos.Count, aps.Count);
            for (int i = 0; i < aps.Count; i++)
            {
                var expected = cachedAgreementPeriodDtos[i];
                var actual = aps[i];
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Description, actual.Description, "Desc, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AgreementPeriodsController_GetAgreementPeriodsAsync_CacheControlNotNull()
        {
            controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();

            var aps = await controller.GetAgreementPeriodsAsync();
            Assert.AreEqual(aps.Count(), cachedAgreementPeriodEntities.Count());

            int count = cachedAgreementPeriodEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = cachedAgreementPeriodDtos[i];
                var actual = cachedAgreementPeriodEntities.ToList()[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task AgreementPeriodsController_GetAgreementPeriodsAsync_NoCache()
        {
            controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            controller.Request.Headers.CacheControl.NoCache = true;

            var aps = await controller.GetAgreementPeriodsAsync();
            Assert.AreEqual(aps.Count(), nonCachedAgreementPeriodEntities.Count());

            int count = nonCachedAgreementPeriodEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = nonCachedAgreementPeriodDtos[i];
                var actual = nonCachedAgreementPeriodEntities.ToList()[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        public async Task AgreementPeriodsController_GetAgreementPeriodsAsync_Cache()
        {
            controller.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
            controller.Request.Headers.CacheControl.NoCache = false;

            var aps = await controller.GetAgreementPeriodsAsync();
            Assert.AreEqual(aps.Count(), cachedAgreementPeriodEntities.Count());

            int count = cachedAgreementPeriodEntities.Count();
            for (int i = 0; i < count; i++)
            {
                var expected = cachedAgreementPeriodDtos[i];
                var actual = cachedAgreementPeriodEntities.ToList()[i];

                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AgreementPeriodsController_GetAgreementPeriodsAsync_handles_caught_exceptions()
        {
            agreementsRepositoryMock.Setup(repo => repo.GetAgreementPeriodsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
            var aps = await controller.GetAgreementPeriodsAsync();
        }
    }
}