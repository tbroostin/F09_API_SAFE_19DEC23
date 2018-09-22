// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AccountingCodesServiceTests
    {
        [TestClass]
        public class GET : CurrentUserSetup
        {
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<ILogger> loggerMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            AccountingCodesService accountingCodesService;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountingCode> accountingCodes;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountReceivableType> accountRecievableTypes;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountReceivableDepositType> accountRecievableDepositTypes;
            List<Ellucian.Colleague.Domain.Base.Entities.Distribution2> distributions;

            [TestInitialize]
            public void Initialize()
            {
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepoMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                accountingCodes = new TestAccountingCodesRepository().Get();
                accountRecievableTypes = new TestAccountReceivableTypeRepository().Get();
                accountRecievableDepositTypes = new TestAccountReceivableDepositTypeRepository().Get();
                distributions = new TestDistributionRepository().Get();

                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodes);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountRecievableTypes);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableDepositTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountRecievableDepositTypes);
                studentReferenceDataRepositoryMock.Setup(i => i.GetDistributionsAsync(It.IsAny<bool>())).ReturnsAsync(distributions);

                accountingCodesService = new AccountingCodesService(studentReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, baseConfigurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, 
                    roleRepoMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                accountingCodesService = null;
                accountingCodes = null;
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodes);

                var results = await accountingCodesService.GetAccountingCodesAsync(It.IsAny<bool>());
                Assert.AreEqual(accountingCodes.Count, (results.Count()));

                foreach (var accountingCode in accountingCodes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Title);
                    Assert.AreEqual(accountingCode.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task AccountingCodesService__GetByIdAsync()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodes);

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountingCode = accountingCodes.FirstOrDefault(i => i.Guid == id);

                var result = await accountingCodesService.GetAccountingCodeByIdAsync(id);

                Assert.AreEqual(accountingCode.Code, result.Code);
                Assert.AreEqual(accountingCode.Description, result.Title);
                Assert.AreEqual(accountingCode.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingCodesService__GetByIdAsync_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(true)).ReturnsAsync(accountingCodes);
                var result = await accountingCodesService.GetAccountingCodeByIdAsync("123");
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(It.IsAny<AccountingCodeCategoryDtoProperty>(), It.IsAny<bool>());
                Assert.AreEqual(accountingCodes.Count + accountRecievableTypes.Count + accountRecievableDepositTypes.Count + distributions.Count, (results.Count()));

                foreach (var accountingCode in accountingCodes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Title);
                    Assert.AreEqual(accountingCode.Guid, result.Id);
                }

                //foreach (var accountReceivableType in accountRecievableTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                //    Assert.AreEqual(accountReceivableType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                //}

                //foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                //    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                //}

                //foreach (var distribution in distributions)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == distribution.Guid);

                //    Assert.AreEqual(distribution.Code, result.Code);
                //    Assert.AreEqual(distribution.Description, result.Title);
                //    Assert.AreEqual(distribution.Guid, result.Id);
                //}
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11_Filter1_Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(new AccountingCodeCategoryDtoProperty() { AccountingCodeCategory = Dtos.EnumProperties.AccountingCodeCategoryType.AccountsReceivableType}, It.IsAny<bool>());
                Assert.AreEqual(accountRecievableTypes.Count, (results.Count()));

                //foreach (var accountingCode in accountingCodes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountingCode.Guid);

                //    Assert.AreEqual(accountingCode.Code, result.Code);
                //    Assert.AreEqual(accountingCode.Description, result.Title);
                //    Assert.AreEqual(accountingCode.Guid, result.Id);
                //}

                foreach (var accountReceivableType in accountRecievableTypes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                    Assert.AreEqual(accountReceivableType.Code, result.Code);
                    Assert.AreEqual(accountReceivableType.Description, result.Title);
                    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                }

                //foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                //    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                //}

                //foreach (var distribution in distributions)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == distribution.Guid);

                //    Assert.AreEqual(distribution.Code, result.Code);
                //    Assert.AreEqual(distribution.Description, result.Title);
                //    Assert.AreEqual(distribution.Guid, result.Id);
                //}
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11_Filter2_Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(new AccountingCodeCategoryDtoProperty() { AccountingCodeCategory = Dtos.EnumProperties.AccountingCodeCategoryType.DistributionCode }, It.IsAny<bool>());
                Assert.AreEqual(distributions.Count, (results.Count()));

                //foreach (var accountingCode in accountingCodes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountingCode.Guid);

                //    Assert.AreEqual(accountingCode.Code, result.Code);
                //    Assert.AreEqual(accountingCode.Description, result.Title);
                //    Assert.AreEqual(accountingCode.Guid, result.Id);
                //}

                //foreach (var accountReceivableType in accountRecievableTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                //    Assert.AreEqual(accountReceivableType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                //}

                //foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                //    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                //}

                foreach (var distribution in distributions)
                {
                    var result = results.FirstOrDefault(i => i.Id == distribution.Guid);

                    Assert.AreEqual(distribution.Code, result.Code);
                    Assert.AreEqual(distribution.Description, result.Title);
                    Assert.AreEqual(distribution.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11_Filter3_Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(new AccountingCodeCategoryDtoProperty() { AccountingCodeCategory = Dtos.EnumProperties.AccountingCodeCategoryType.DepositType }, It.IsAny<bool>());
                Assert.AreEqual(accountingCodes.Count, (results.Count()));

                foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                }

                //foreach (var accountReceivableType in accountRecievableTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                //    Assert.AreEqual(accountReceivableType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                //}

                //foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                //    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                //}

                //foreach (var distribution in distributions)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == distribution.Guid);

                //    Assert.AreEqual(distribution.Code, result.Code);
                //    Assert.AreEqual(distribution.Description, result.Title);
                //    Assert.AreEqual(distribution.Guid, result.Id);
                //}
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11_Filter4_Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(new AccountingCodeCategoryDtoProperty() { AccountingCodeCategory = Dtos.EnumProperties.AccountingCodeCategoryType.AccountsReceivableCode }, It.IsAny<bool>());
                Assert.AreEqual(accountingCodes.Count, (results.Count()));

                foreach (var accountingCode in accountingCodes)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCode.Guid);

                    Assert.AreEqual(accountingCode.Code, result.Code);
                    Assert.AreEqual(accountingCode.Description, result.Title);
                    Assert.AreEqual(accountingCode.Guid, result.Id);
                }

                //foreach (var accountReceivableType in accountRecievableTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableType.Guid);

                //    Assert.AreEqual(accountReceivableType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableType.Guid, result.Id);
                //}

                //foreach (var accountReceivableDepositType in accountRecievableDepositTypes)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == accountReceivableDepositType.Guid);

                //    Assert.AreEqual(accountReceivableDepositType.Code, result.Code);
                //    Assert.AreEqual(accountReceivableDepositType.Description, result.Title);
                //    Assert.AreEqual(accountReceivableDepositType.Guid, result.Id);
                //}

                //foreach (var distribution in distributions)
                //{
                //    var result = results.FirstOrDefault(i => i.Id == distribution.Guid);

                //    Assert.AreEqual(distribution.Code, result.Code);
                //    Assert.AreEqual(distribution.Description, result.Title);
                //    Assert.AreEqual(distribution.Guid, result.Id);
                //}
            }

            [TestMethod]
            public async Task AccountingCodesService__GetAllV11_Filter5_Async()
            {
                var results = await accountingCodesService.GetAccountingCodes2Async(new AccountingCodeCategoryDtoProperty() { AccountingCodeCategory = Dtos.EnumProperties.AccountingCodeCategoryType.DistributionCode, Detail = new Dtos.GuidObject2("djsk") }, It.IsAny<bool>());
                Assert.AreEqual(0, (results.Count()));
            }

            [TestMethod]
            public async Task AccountingCodesService__GetByIdV11_1_Async()
            {
                referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(new GuidLookupResult() { Entity = "AR.CODES" });

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountingCode = accountingCodes.FirstOrDefault(i => i.Guid == id);

                var result = await accountingCodesService.GetAccountingCode2ByIdAsync(id, true);

                Assert.AreEqual(accountingCode.Code, result.Code);
                Assert.AreEqual(accountingCode.Description, result.Title);
                Assert.AreEqual(accountingCode.Guid, result.Id);
            }

            [TestMethod]
            public async Task AccountingCodesService__GetByIdV11_2_Async()
            {
                referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                                .ReturnsAsync(new GuidLookupResult() { Entity = "AR.TYPES" });

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountReceivableType = accountRecievableTypes.FirstOrDefault(i => i.Guid == id);

                var result = await accountingCodesService.GetAccountingCode2ByIdAsync(id, true);

                Assert.AreEqual(accountReceivableType.Code, result.Code);
                Assert.AreEqual(accountReceivableType.Description, result.Title);
                Assert.AreEqual(accountReceivableType.Guid, result.Id);
            }

            [TestMethod]
            public async Task AccountingCodesService__GetByIdV11_3_Async()
            {
                referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                                .ReturnsAsync(new GuidLookupResult() { Entity = "AR.DEPOSIT.TYPES" });

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountRecievableDepositType = accountRecievableDepositTypes.FirstOrDefault(i => i.Guid == id);

                var result = await accountingCodesService.GetAccountingCode2ByIdAsync(id, true);

                Assert.AreEqual(accountRecievableDepositType.Code, result.Code);
                Assert.AreEqual(accountRecievableDepositType.Description, result.Title);
                Assert.AreEqual(accountRecievableDepositType.Guid, result.Id);
            }

            [TestMethod]
            public async Task AccountingCodesService__GetByIdV11_4_Async()
            {
                referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                                .ReturnsAsync(new GuidLookupResult() { Entity = "DISTRIBUTION" });

                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var distribution = distributions.FirstOrDefault(i => i.Guid == id);

                var result = await accountingCodesService.GetAccountingCode2ByIdAsync(id, true);

                Assert.AreEqual(distribution.Code, result.Code);
                Assert.AreEqual(distribution.Description, result.Title);
                Assert.AreEqual(distribution.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountingCodesService__GetByIdAsyncV11_KeyNotFoundException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(true)).ReturnsAsync(accountingCodes);
                var result = await accountingCodesService.GetAccountingCode2ByIdAsync("", true);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingCodesService__GetByIdAsyncV11_KeyNotFoundException2()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(true)).ReturnsAsync(accountingCodes);
                var result = await accountingCodesService.GetAccountingCode2ByIdAsync("fsf", true);
            }
        }
    }
}
