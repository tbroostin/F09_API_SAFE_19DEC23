// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Entities;
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


namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class TaxFormsServiceTests
    {
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;
        private IEnumerable<Domain.Entities.Role> roles;
        private TaxFormsService taxFormsService;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private ICurrentUserFactory currentUserFactory;
        private IEnumerable<TaxForm> taxFormEntities;


        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            currentUserFactory = currentUserFactoryMock.Object;

            taxFormsService = new TaxFormsService(colleagueFinanceReferenceDataRepositoryMock.Object,
            adapterRegistryMock.Object,
            currentUserFactory,
            roleRepositoryMock.Object,
            configurationRepositoryMock.Object,
            loggerMock.Object);

            InitializeTestData();

            InitializeMock();
        }

        [TestCleanup]
        public void Cleanup()
        {
            colleagueFinanceReferenceDataRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            configurationRepositoryMock = null;
            taxFormsService = null;
        }

        private void InitializeTestData()
        {
            taxFormEntities = new List<TaxForm>
            {
                new TaxForm("1098T", "1098-T Taxform"),
                new TaxForm("1098E", "1098-E Taxform"),
                new TaxForm("T4", "T4 Taxform")
            };
        }

        private void InitializeMock()
        {
            roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);

            colleagueFinanceReferenceDataRepositoryMock.Setup(f => f.GetTaxFormsAsync()).ReturnsAsync(taxFormEntities);
            var taxFormAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.TaxForm, Dtos.ColleagueFinance.TaxForm>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.ColleagueFinance.Entities.TaxForm, Dtos.ColleagueFinance.TaxForm>()).Returns(taxFormAdapter);

        }

        #endregion


        #region GET_METHODS
        [TestMethod]
        public async Task TaxFormsService_GetTaxFormsAsync()
        {
            var taxFormDtos = await taxFormsService.GetTaxFormsAsync();
            Assert.AreEqual(taxFormDtos.ToList().Count, taxFormEntities.ToList().Count);
            taxFormEntities = taxFormEntities.OrderBy(x => x.Code);
            Assert.AreEqual(taxFormDtos.ToList()[0].Code, taxFormEntities.ToList()[0].Code);
            Assert.AreEqual(taxFormDtos.ToList()[0].Description, taxFormEntities.ToList()[0].Description);
            Assert.AreEqual(taxFormDtos.ToList()[1].Code, taxFormEntities.ToList()[1].Code);
            Assert.AreEqual(taxFormDtos.ToList()[1].Description, taxFormEntities.ToList()[1].Description);
            Assert.AreEqual(taxFormDtos.ToList()[2].Code, taxFormEntities.ToList()[2].Code);
            Assert.AreEqual(taxFormDtos.ToList()[2].Description, taxFormEntities.ToList()[2].Description);
        }
        #endregion
    }
}

