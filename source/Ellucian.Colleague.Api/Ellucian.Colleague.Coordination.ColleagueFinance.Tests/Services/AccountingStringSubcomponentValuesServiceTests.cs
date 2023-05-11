//Copyright 2018 Ellucian Company L.P. and its affiliates.


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
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class AccountingStringSubcomponentValuesServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "ColleagueFinancesAdministrator");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Cfa",
                            Roles = new List<string>() { "ColleagueFinancesAdministrator" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class AccountingStringSubcomponentValuesServiceUnitTests : CurrentUserSetup
        {

            private const string accountingStringSubcomponentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string accountingStringSubcomponentsCode = "Fund";
            private ICollection<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> _accountingStringSubcomponentsCollection;
            private ICollection<Domain.ColleagueFinance.Entities.AcctStructureIntg> _acctStructureIntgCollection;

            private Tuple<IEnumerable<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>, int> _accountingStringSubcomponentsTuple;
            private AccountingStringSubcomponentValuesService _accountingStringSubcomponentValuesService;

            private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;

            private ICurrentUserFactory currentUserFactory;
            private IRoleRepository roleRepo;
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "ColleagueFinancesAdministrator");
            private Domain.Entities.Permission permissionViewAccountingStrings;

            [TestInitialize]
            public async void Initialize()
            {
                _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepo = _roleRepositoryMock.Object;
                _configurationRepoMock = new Mock<IConfigurationRepository>();

                _accountingStringSubcomponentsCollection = new List<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>()
                {
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FUND", "Fund", "Type1") { Explanation = "Explanation" },
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "GL.CLASS", "GL Class", "Type2"),
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("d2253ac7-9931-4560-b42f-1fccd43c952e", "PROGRAM", "Program", "Type3")

                };
                _acctStructureIntgCollection = new List<Domain.ColleagueFinance.Entities.AcctStructureIntg>()
                {
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("4a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FUND", "Title1") { Type = "Type1", Length = "FUND".Length,
                        ParentSubComponent =  "FUND"},
                new Domain.ColleagueFinance.Entities.AcctStructureIntg("5a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "Type2", "Title2"){ Type = "Type2", Length = "GL.CLASS".Length },
                new Domain.ColleagueFinance.Entities.AcctStructureIntg("6a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "Type3", "Title3"){ Type = "Type3", Length = "PROGRAM".Length },
               };
                _accountingStringSubcomponentsTuple = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>, int>(_accountingStringSubcomponentsCollection, _accountingStringSubcomponentsCollection.Count);
                _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(_accountingStringSubcomponentsTuple);


                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAccountingStrings = new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountingStrings);
                personRole.AddPermission(permissionViewAccountingStrings);

                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole});

                _referenceRepositoryMock.Setup(i => i.GetGuidFromEntityInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(accountingStringSubcomponentsGuid);
                _referenceRepositoryMock.Setup(i => i.GetAcctStructureIntgAsync(It.IsAny<bool>())).ReturnsAsync(_acctStructureIntgCollection);

                _accountingStringSubcomponentValuesService = new AccountingStringSubcomponentValuesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, currentUserFactory,
                    roleRepo, _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _accountingStringSubcomponentValuesService = null;
                _accountingStringSubcomponentsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesAsync()
            {
                var results = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                Assert.IsTrue(results is Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues>, int>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesAsync_Count()
            {
                var results = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
                Assert.AreEqual(3, results.Item1.Count());
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesAsync_Properties()
            {
                var result = (await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Item1.FirstOrDefault(x => x.Title == accountingStringSubcomponentsCode);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Title);
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesAsync_Expected()
            {
                var expectedResults = _accountingStringSubcomponentsCollection.FirstOrDefault(c => c.Guid == accountingStringSubcomponentsGuid);
                var actualResult =
                    (await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Item1.FirstOrDefault(x => x.Id == accountingStringSubcomponentsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.Description, actualResult.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesAsync_Exception()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ThrowsAsync(new Exception());

                var results = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AccountingStringSubcomponentValuesService_GetASSVAsync_ArgumentException()
            {

                _referenceRepositoryMock.Setup(repo => repo.GetGuidFromEntityInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentException());

                var results = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AccountingStringSubcomponentValuesService_GetASSVAsync_KeyNotFoundException()
            {

                _referenceRepositoryMock.Setup(repo => repo.GetGuidFromEntityInfoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync("");

                var results = await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesByGuidAsync_KeyNotFoundException()
            {
                _referenceRepositoryMock.Setup(i => i.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesByGuidAsync_InvalidOperationException()
            {
                _referenceRepositoryMock.Setup(i => i.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync(null);
            }

            //[TestMethod]
            //[ExpectedException(typeof(KeyNotFoundException))]
            //public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesByGuidAsync_InvalidId()
            //{
            //    _referenceRepositoryMock.Setup(repo => repo.GetAcctStructureIntgAsync(It.IsAny<bool>()))
            //        .Throws<KeyNotFoundException>();

            //    await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync("99");
            //}

            //[TestMethod]
            //public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesByGuidAsync_Expected()
            //{
            //    var expectedResults =
            //        _accountingStringSubcomponentsCollection.First(c => c.Guid == accountingStringSubcomponentsGuid);
            //    var actualResult =
            //        await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync(accountingStringSubcomponentsGuid);
            //    Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            //    Assert.AreEqual(expectedResults.Description, actualResult.Title);

            //}

            //[TestMethod]
            //public async Task AccountingStringSubcomponentValuesService_GetAccountingStringSubcomponentValuesByGuidAsync_Properties()
            //{
            //    var result =
            //        await _accountingStringSubcomponentValuesService.GetAccountingStringSubcomponentValuesByGuidAsync(accountingStringSubcomponentsGuid);
            //    Assert.IsNotNull(result.Id);
            //    Assert.IsNotNull(result.Title);
            //}
        }
    }
}