//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AccountingStringComponentValues = Ellucian.Colleague.Domain.ColleagueFinance.Entities.AccountingStringComponentValues;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class AccountingStringsServiceTests : ColleagueFinanceServiceTestsSetup
    {
        private const string AccountingStringComponentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string AccountingStringComponentsCode = "GL.ACCT";
        private ICollection<AccountComponents> _accountComponentsCollection;
        private ICollection<AccountingFormat> _accountFormatCollection;
        private ICollection<Domain.ColleagueFinance.Entities.AccountingStringComponentValues> _accountStringComponentValuesCollection;
        private List<Domain.ColleagueFinance.Entities.FiscalYear> fiscalYears;
        private AccountingStringService _accountingStringService;
        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAccountingStringRepository> _accountingStringsRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        private Mock<IGrantsRepository> _grantRepositoryMock;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewBudgetAdjustments;
        private Domain.Entities.Permission permissionViewAccountingStrings;

        protected Domain.Entities.Role userRoleAllPermissions = new Domain.Entities.Role(333, "Faculty");
        protected Domain.Entities.Role userAcctStringsRole = new Domain.Entities.Role(334, "VIEW.ACCOUNTING.STRINGS");

        List<string> grantList = new List<string>();

        private const int offset = 0;
        private const int limit = 100;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _accountingStringsRepositoryMock = new Mock<IAccountingStringRepository>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _grantRepositoryMock = new Mock<IGrantsRepository>();

            // Create permission domain entities for create/update, view and delete.
            permissionViewBudgetAdjustments = new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewBudgetAdjustments);
            permissionViewAccountingStrings = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewAccountingStrings);
            // Assign all three permissions to the role that has all permissions.
            userRoleAllPermissions.AddPermission(permissionViewBudgetAdjustments);
            userAcctStringsRole.AddPermission(permissionViewAccountingStrings);

            // Mock the repository for the role that has all permissions.
            roleRepositoryMock = new Mock<IRoleRepository>();
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { userRoleAllPermissions });
            roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { userAcctStringsRole });
            roleRepository = roleRepositoryMock.Object;

            _accountComponentsCollection = new List<AccountComponents>()
                {
                    new AccountComponents("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "GL.ACCT", "Desc1"),
                    new AccountComponents("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "PROJECT", "Desc2")
                };

            _accountFormatCollection = new List<AccountingFormat>()
            {
                new AccountingFormat("b4d710f5-08e1-406d-ab4e-aa5f5b0a2491", "F1", "FormatDesc1")
            };

            _accountStringComponentValuesCollection = new List<AccountingStringComponentValues>()
            {
                new AccountingStringComponentValues()
                {
                    AccountDef = "GL",
                    AccountNumber = "11_00_01_00_00000_10110",
                    Description = "Contribution Checking : General",
                    Status = "available",
                    Guid = "6f5e7bdb-7998-456c-9436-c77eaca180da",
                    Type = "asset",
                    PooleeAccounts = new Dictionary<string, string>(),
                    GrantIds = new List<string>() { "1" }
                },
                 new AccountingStringComponentValues() {
                    AccountDef = "GL",
                    AccountNumber = "11_00_01_00_00000_10111",
                    Description = "Contribution Checking",
                    Status = "unavailable",
                    Guid = "7f5e7bdb-7998-456c-9436-c77eaca180da",
                    Type = "liability"},
                 new AccountingStringComponentValues() {
                    AccountDef = "Project",
                    AccountNumber = "11_00_01_00_00000_10112",
                    Description = "Checking",
                    Status = "unavailable",
                    Guid = "8f5e7bdb-7998-456c-9436-c77eaca180da",
                    Type = "fundBalance"},
                new AccountingStringComponentValues() {
                    AccountDef = "Project",
                    AccountNumber = "11_00_01_00_00000_10113",
                    Description = "Checking2",
                    Status = "unavailable",
                    Guid = "9f5e7bdb-7998-456c-9436-c77eaca180da",
                    Type = "revenue"},
                 new AccountingStringComponentValues() {
                    AccountDef = "Project",
                    AccountNumber = "11_00_01_00_00000_10114",
                    Description = "Checking3",
                    Status = "unavailable",
                    Guid = "0f5e7bdb-7998-456c-9436-c77eaca180da",
                    Type = "expense"}
            };
            fiscalYears = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
            {
                new Domain.ColleagueFinance.Entities.FiscalYear("1f5e7bdb-7998-456c-9436-c77eaca180db", "1")
            };

            grantList.Add("1");
            _grantRepositoryMock.Setup(i => i.GetProjectCFIdsAsync( It.IsAny<string[]>())).ReturnsAsync(grantList);

            var pooleeAccounts = new Dictionary<string, string>();
            pooleeAccounts.Add("1", "1");
            _accountStringComponentValuesCollection.First().PooleeAccounts = new Dictionary<string, string>();
            _accountStringComponentValuesCollection.First().PooleeAccounts = pooleeAccounts;
            _referenceRepositoryMock.Setup(repo => repo.GetGuidsForPooleeGLAcctsInFiscalYearsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pooleeAccounts);
            _referenceRepositoryMock.Setup(repo => repo.GetAccountFormatsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_accountFormatCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>())).ReturnsAsync(fiscalYears);

            _referenceRepositoryMock.Setup(repo => repo.GetAccountComponentsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_accountComponentsCollection);

            Tuple<IEnumerable<AccountingStringComponentValues>, int> accountStringComponentValuesTuple =
                new Tuple<IEnumerable<AccountingStringComponentValues>, int>(_accountStringComponentValuesCollection, _accountStringComponentValuesCollection.Count);

            //_referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            //    .ReturnsAsync(accountStringComponentValuesTuple);

            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(accountStringComponentValuesTuple);

            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<List<string>>(), default(DateTime?))).ReturnsAsync(accountStringComponentValuesTuple);



            _accountingStringService = new AccountingStringService(
                _accountingStringsRepositoryMock.Object, _referenceRepositoryMock.Object, _grantRepositoryMock.Object,
                adapterRegistryMock.Object, _configurationRepositoryMock.Object,
                GLCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _accountingStringService = null;
            _accountComponentsCollection = null;
            _referenceRepositoryMock = null;
            _accountingStringsRepositoryMock = null;
        }

        #region Accounting String Components

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsAsync()
        {
            var results = await _accountingStringService.GetAccountingStringComponentsAsync(true);
            Assert.IsTrue(results is IEnumerable<AccountingStringComponent>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsAsync_Count()
        {
            var results = await _accountingStringService.GetAccountingStringComponentsAsync(true);
            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsAsync_Properties()
        {
            var result =
                (await _accountingStringService.GetAccountingStringComponentsAsync(true)).FirstOrDefault(x => x.Code == AccountingStringComponentsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsAsync_Expected()
        {
            var expectedResults = _accountComponentsCollection.FirstOrDefault(c => c.Guid == AccountingStringComponentsGuid);
            var actualResult =
                (await _accountingStringService.GetAccountingStringComponentsAsync(true)).FirstOrDefault(x => x.Id == AccountingStringComponentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentsByGuidAsync_Empty()
        {
            await _accountingStringService.GetAccountingStringComponentsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentsByGuidAsync_Null()
        {
            await _accountingStringService.GetAccountingStringComponentsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAccountComponentsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _accountingStringService.GetAccountingStringComponentsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsByGuidAsync_Expected()
        {
            var expectedResults =
                _accountComponentsCollection.First(c => c.Guid == AccountingStringComponentsGuid);
            var actualResult =
                await _accountingStringService.GetAccountingStringComponentsByGuidAsync(AccountingStringComponentsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentsByGuidAsync_Properties()
        {
            var result =
                await _accountingStringService.GetAccountingStringComponentsByGuidAsync(AccountingStringComponentsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }

        #endregion

        #region Accounting String Formats

        [TestMethod]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsAsync()
        {
            var results = await _accountingStringService.GetAccountingStringFormatsAsync(true);
            Assert.IsTrue(results is IEnumerable<AccountingStringFormats>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsAsync_Count()
        {
            var results = await _accountingStringService.GetAccountingStringFormatsAsync(true);
            Assert.AreEqual(1, results.Count());
        }

        [TestMethod]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsAsync_Expected()
        {
            var expectedResults = _accountFormatCollection.FirstOrDefault(c => c.Guid == "b4d710f5-08e1-406d-ab4e-aa5f5b0a2491");
            var actualResult =
                (await _accountingStringService.GetAccountingStringFormatsAsync(true));
            var expectedComponents = _accountComponentsCollection.ToList();
            var guid = new GuidObject2("b4d710f5-08e1-406d-ab4e-aa5f5b0a2491");
            foreach (var actual in actualResult)
            {
                Assert.AreEqual(guid.Id.ToString(), actual.Id);
                Assert.AreEqual("*", actual.Delimiter);
                Assert.AreEqual(expectedComponents.Count, actual.Components.Count);
                for (int i = 0; i < actual.Components.Count; i++)
                {
                    Assert.AreEqual(expectedComponents[i].Guid, actual.Components[i].Component.Id);
                    Assert.AreEqual(i + 1, actual.Components[i].order);
                }
            }

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsByGuidAsync_Empty()
        {
            await _accountingStringService.GetAccountingStringFormatsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsByGuidAsync_Null()
        {
            await _accountingStringService.GetAccountingStringFormatsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAccountComponentsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _accountingStringService.GetAccountingStringFormatsByGuidAsync("99");
        }

        [TestMethod]
        public async Task AccountingStringFormatsService_GetAccountingStringFormatsByGuidAsync_Expected()
        {
            var expectedResults =
                _accountFormatCollection.First(c => c.Guid == "b4d710f5-08e1-406d-ab4e-aa5f5b0a2491");
            var actualResult =
                await _accountingStringService.GetAccountingStringFormatsByGuidAsync("b4d710f5-08e1-406d-ab4e-aa5f5b0a2491");
            var expectedComponents = _accountComponentsCollection.ToList();
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual("*", actualResult.Delimiter);
            Assert.AreEqual(expectedComponents.Count, actualResult.Components.Count);
            for (int i = 0; i < actualResult.Components.Count; i++)
            {
                Assert.AreEqual(expectedComponents[i].Guid, actualResult.Components[i].Component.Id);
                Assert.AreEqual(i + 1, actualResult.Components[i].order);
            }

        }

        #endregion

        //#region Accounting String Component Values

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValuesAsync(offset, limit, "", "", "", "", false);

            Assert.IsNotNull(actualsTuple);

            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = _accountStringComponentValuesCollection.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);

            }
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues_ComponentFilter_invalid()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValuesAsync(offset, limit, "invalid", "", "", "", false);

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues_ComponentFilter_Valid()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValuesAsync(offset, limit, "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "", "", "", false);

            Assert.AreEqual(_accountStringComponentValuesCollection.Count, actualsTuple.Item1.Count());
        }


        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues_TypeFund_invalid()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValuesAsync(offset, limit, "", "", "", "invalid", false);

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }


        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValuesByGuid()
        {
            var id = "6f5e7bdb-7998-456c-9436-c77eaca180da";
            var expected = _accountStringComponentValuesCollection.FirstOrDefault(x => x.Guid == id);
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValueByGuid(It.IsAny<string>()))
                 .ReturnsAsync(expected);
            var actual = await _accountingStringService.GetAccountingStringComponentValuesByGuidAsync(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Guid, actual.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentValuesByGuid_Invalid()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValueByGuid(It.IsAny<string>()))
                  .Throws<KeyNotFoundException>();
            await _accountingStringService.GetAccountingStringComponentValuesByGuidAsync("invalid");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentValuesByGuid_InvalidOperation()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValueByGuid(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();
            await _accountingStringService.GetAccountingStringComponentValuesByGuidAsync("invalid");
        }

        //V12 tests

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues2()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues2Async(offset, limit, "", "", "", "", false);

            Assert.IsNotNull(actualsTuple);

            int count = actualsTuple.Item1.Count();

            for (int i = 0; i < count; i++)
            {
                var expected = _accountStringComponentValuesCollection.ToList()[i];
                var actual = actualsTuple.Item1.ToList()[i];

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Guid, actual.Id);
                if (actual.BudgetPools != null && actual.BudgetPools.Any())
                {
                    Assert.AreEqual(actual.BudgetPools.Count(), 1);
                }
            }
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues2_With_TypeFund()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues2Async(offset, limit, "", "", "", "ABC", false);

            Assert.AreEqual(actualsTuple.Item2, 0);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues2_With_Component_NotFound()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues2Async(offset, limit, "ABC", "", "", "", false);

            Assert.AreEqual(actualsTuple.Item2, 0);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues2ByGuidAsync()
        {
            var id = "6f5e7bdb-7998-456c-9436-c77eaca180da";
            var expected = _accountStringComponentValuesCollection.FirstOrDefault(x => x.Guid == id);
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValue2ByGuid(It.IsAny<string>()))
                .ReturnsAsync(expected);
            var result = await _accountingStringService.GetAccountingStringComponentValues2ByGuidAsync(id, It.IsAny<bool>());
            Assert.IsNotNull(result);
            Assert.AreEqual(result.BudgetPools.Count(), 1);
        }

        //V15
        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3()
        {
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, It.IsAny<AccountingStringComponentValues3>(), It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);          
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3_WithTypeFund()
        {
            AccountingStringComponentValues3 criteria = new AccountingStringComponentValues3()
            {
                Component = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                Status = Dtos.EnumProperties.Status.Active,
                Type = new AccountingStringComponentValuesType()
                {
                    Account = Dtos.EnumProperties.AccountingTypeAccount.asset,
                    Fund = "Fund"
                },
                Grants = new List<GuidObject2>() { new GuidObject2("1f5e7bdb-7998-456c-9436-c77eaca180da") }
            };
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, criteria, It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3_WithTypeFundInactive()
        {
            AccountingStringComponentValues3 criteria = new AccountingStringComponentValues3()
            {
                Component = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                Status = Dtos.EnumProperties.Status.Inactive
            };
            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, criteria, It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
            Assert.AreEqual(5, actualsTuple.Item2);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3_WithNoGrants()
        {
            AccountingStringComponentValues3 criteria = new AccountingStringComponentValues3()
            {
                Component = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"),
                Status = Dtos.EnumProperties.Status.Active,
                Type = new AccountingStringComponentValuesType()
                {
                    Account = Dtos.EnumProperties.AccountingTypeAccount.asset,
                },
                Grants = new List<GuidObject2>() { new GuidObject2("1f5e7bdb-7998-456c-9436-c77eaca180db") }
            };
            _grantRepositoryMock.Setup(i => i.GetProjectCFIdsAsync(It.IsAny<string[]>())).ReturnsAsync(() => null);

            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, criteria, It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3_WithComponentNotFound()
        {
            AccountingStringComponentValues3 criteria = new AccountingStringComponentValues3()
            {
                Component = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbb")
            };

            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, criteria, It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
            Assert.AreEqual(0, actualsTuple.Item2);
        }

        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3_WithComponent()
        {
            AccountingStringComponentValues3 criteria = new AccountingStringComponentValues3()
            {
                Component = new GuidObject2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc")
            };

            var actualsTuple =
                await
                    _accountingStringService.GetAccountingStringComponentValues3Async(offset, limit, criteria, It.IsAny<DateTime?>(), It.IsAny<bool>());

            Assert.IsNotNull(actualsTuple);
        }



        [TestMethod]
        public async Task AccountingStringService_GetAccountingStringComponentValues3ByGuidAsync()
        {
            var id = "6f5e7bdb-7998-456c-9436-c77eaca180da";
            var expected = _accountStringComponentValuesCollection.FirstOrDefault(x => x.Guid == id);
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValue3ByGuid(It.IsAny<string>()))
                .ReturnsAsync(expected);
            var result = await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync(id, It.IsAny<bool>());
            Assert.IsNotNull(result);
            Assert.AreEqual(result.BudgetPools.Count(), 1);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountingStringService_GetAccountingStringComponentValues3ByGuidAsync_ArgumentNullException()
        {
            var result = await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync("", It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentValues3ByGuidAsync_KeyNotFoundException()
        {
            var id = "6f5e7bdb-7998-456c-9436-c77eaca180da";
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValue3ByGuid(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            var result = await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync(id, It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountingStringService_GetAccountingStringComponentValues3ByGuidAsync_InvalidOperationException()
        {
            var id = "6f5e7bdb-7998-456c-9436-c77eaca180da";
            _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringComponentValue3ByGuid(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
            var result = await _accountingStringService.GetAccountingStringComponentValues3ByGuidAsync(id, It.IsAny<bool>());
        }
    }
}