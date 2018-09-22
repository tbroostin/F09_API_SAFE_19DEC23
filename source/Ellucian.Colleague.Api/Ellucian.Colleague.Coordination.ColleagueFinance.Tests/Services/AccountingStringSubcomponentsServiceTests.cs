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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class AccountingStringSubcomponentsServiceTests
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
        public class AccountingStringSubcomponentsServiceUnitTests : CurrentUserSetup
        {

            private const string accountingStringSubcomponentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string accountingStringSubcomponentsCode = "Fund";
            private ICollection<AcctStructureIntg> _accountingStringSubcomponentsCollection;
            private AccountingStringSubcomponentsService _accountingStringSubcomponentsService;

            private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private ICurrentUserFactory currentUserFactory;

            private IRoleRepository roleRepo;


            private Mock<IConfigurationRepository> _configurationRepoMock;
            //private IEnumerable<Domain.Entities.Role> roles;
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "ColleagueFinancesAdministrator");
            private Domain.Entities.Permission permissionViewAccountingStrings;

            [TestInitialize]
            public void Initialize()
            {
                _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepo = _roleRepositoryMock.Object;
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                

                _accountingStringSubcomponentsCollection = new List<AcctStructureIntg>()
                {
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FUND", "Fund")
                    {Type = "FD",ParentSubComponent = "GL.CLASS" },
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "GL.CLASS", "GL Class")
                    {Type = "FC" },
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "PROGRAM", "Program")
                    {Type = "OB" }

                };

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                
                // Mock permissions
                permissionViewAccountingStrings = new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountingStrings);
                personRole.AddPermission(permissionViewAccountingStrings);      
                
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                _referenceRepositoryMock.Setup(repo => repo.GetAcctStructureIntgAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_accountingStringSubcomponentsCollection);

                _accountingStringSubcomponentsService = new AccountingStringSubcomponentsService(_referenceRepositoryMock.Object,
                    _adapterRegistryMock.Object, currentUserFactory,
                    roleRepo, _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _accountingStringSubcomponentsService = null;
                _accountingStringSubcomponentsCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsAsync()
            {
                var results = await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsAsync(true);
                Assert.IsTrue(results is IEnumerable<AccountingStringSubcomponents>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsAsync_Count()
            {
                var results = await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsAsync(true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsAsync_Properties()
            {
                var result =
                    (await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsAsync(true)).FirstOrDefault(x => x.Title == accountingStringSubcomponentsCode);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Title);
                Assert.IsNull(result.Description);

            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsAsync_Expected()
            {
                var expectedResults = _accountingStringSubcomponentsCollection.FirstOrDefault(c => c.Guid == accountingStringSubcomponentsGuid);
                var actualResult =
                    (await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsAsync(true)).FirstOrDefault(x => x.Id == accountingStringSubcomponentsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);


            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsByGuidAsync_Empty()
            {
                await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsByGuidAsync_Null()
            {
                await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsByGuidAsync_InvalidId()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetAcctStructureIntgAsync(It.IsAny<bool>()))
                    .Throws<KeyNotFoundException>();

                await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync("99");
            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsByGuidAsync_Expected()
            {
                var expectedResults =
                    _accountingStringSubcomponentsCollection.First(c => c.Guid == accountingStringSubcomponentsGuid);
                var actualResult =
                    await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync(accountingStringSubcomponentsGuid);
                Assert.AreEqual(expectedResults.Guid, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);


            }

            [TestMethod]
            public async Task AccountingStringSubcomponentsService_GetAccountingStringSubcomponentsByGuidAsync_Properties()
            {
                var result =
                    await _accountingStringSubcomponentsService.GetAccountingStringSubcomponentsByGuidAsync(accountingStringSubcomponentsGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Title);

            }
        }
    }
}