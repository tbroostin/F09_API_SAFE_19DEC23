/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
//using Ellucian.Colleague.Domain.Base.;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
//using Ellucian.Web.Http.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class AccountsPayableSourcesServiceTests : ColleagueFinanceServiceTestsSetup
    {
        public Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        AccountsPayableSourcesService accountsPayableSourcesService;
        List<Dtos.AccountsPayableSources> accountsPayableSourceDtoList = new List<Dtos.AccountsPayableSources>();
        List<Domain.ColleagueFinance.Entities.AccountsPayableSources> accountsPayableSourceEntityList = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>();
        string id = "03ef76f3-61be-4990-8a99-9a80282fc420";

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            BuildData();
            referenceDataRepository = new Mock<IColleagueFinanceReferenceDataRepository>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            accountsPayableSourcesService = new AccountsPayableSourcesService(referenceDataRepository.Object, _configurationRepositoryMock.Object, 
                adapterRegistryMock.Object, GLCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            referenceDataRepository = null;
            accountsPayableSourcesService = null;
            accountsPayableSourceDtoList = null;
            accountsPayableSourceEntityList = null;
        }

        [TestMethod]
        public async Task AccountsPayableSources_GetAll()
        {
            referenceDataRepository.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(accountsPayableSourceEntityList);

            var actuals = await accountsPayableSourcesService.GetAccountsPayableSourcesAsync(It.IsAny<bool>());

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AccountsPayableSources_GetAll_True()
        {
            referenceDataRepository.Setup(i => i.GetAccountsPayableSourcesAsync(true)).ReturnsAsync(accountsPayableSourceEntityList);

            var actuals = await accountsPayableSourcesService.GetAccountsPayableSourcesAsync(true);

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Description, actual.Description);
                Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
                Assert.IsNull(actual.Description);
            }
        }

        [TestMethod]
        public async Task AccountsPayableSources_GetById()
        {
            var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            referenceDataRepository.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(accountsPayableSourceEntityList);

            var actual = await accountsPayableSourcesService.GetAccountsPayableSourcesByGuidAsync(id);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Code, actual.Code);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
            Assert.IsNull(actual.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AccountsPayableSources_GetById_InvalidOperationException()
        {
            referenceDataRepository.Setup(i => i.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(accountsPayableSourceEntityList);
            var actual = await accountsPayableSourcesService.GetAccountsPayableSourcesByGuidAsync("abc");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AccountsPayableSources_GetById_Exception()
        {
            referenceDataRepository.Setup(i => i.GetAccountsPayableSourcesAsync(true)).ThrowsAsync(new Exception());
            var actual = await accountsPayableSourcesService.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>());
        }

        private void BuildData()
        {
            accountsPayableSourceEntityList = new List<Domain.ColleagueFinance.Entities.AccountsPayableSources>() 
            {
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("03ef76f3-61be-4990-8a99-9a80282fc420", "AP", "Regular Vendor Payments"),
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("d2f4f0af-6714-48c7-88d5-1c40cb407b6c", "AP2", "Accounts Payable 2"),
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", "CAD", "Canadian Account Payable"),
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", "EUR", "Euro Account Payable"),
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("81cd5b52-9705-4b1b-8eed-669c63db05e2", "PA", "Payroll"),
                new Domain.ColleagueFinance.Entities.AccountsPayableSources("164dc1ad-4d72-4dae-9875-52f761bb0132", "S2", "Student refunds test"),
            };
            foreach (var entity in accountsPayableSourceEntityList)
            {
                accountsPayableSourceDtoList.Add(new Dtos.AccountsPayableSources()
                {
                    Id = entity.Guid,
                    Code = entity.Code,
                    Title = entity.Description,
                    Description = null,
                    DirectDeposit = Dtos.EnumProperties.DirectDeposit.Disabled
                   
                });
            }
            accountsPayableSourceEntityList.FirstOrDefault().directDeposit = "Y";
            accountsPayableSourceDtoList.FirstOrDefault().DirectDeposit = Dtos.EnumProperties.DirectDeposit.Enabled;
        }
    }
}
