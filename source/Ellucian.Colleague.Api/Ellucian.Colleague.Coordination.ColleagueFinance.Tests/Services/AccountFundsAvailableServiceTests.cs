// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Security;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using System;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// This class tests the AccountFundsAvailableService class.
    /// </summary>
    [TestClass]
    public class AccountFundsAvailableServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        protected Ellucian.Colleague.Domain.Entities.Role viewAccountFundsAvailableRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.ACCOUNT.FUNDS.AVAILABLE");

        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IAccountFundsAvailableRepository> accountFundsAvailableRepositoryMock;
        private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
        private Mock<IColleagueFinanceReferenceDataRepository> colleagueFinanceReferenceDataRepositoryMock;
        private Mock<IGeneralLedgerUserRepository> generalLedgerUserRepositoryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;

        private AccountFundsAvailableService accountFundsAvailableService;
        private Mock<ILogger> loggerMock;
        ICurrentUserFactory curntUserFactory;

        private GeneralLedgerAccountStructure testGlAccountStructure;
        private Dtos.AccountFundsAvailable_Transactions acctFundsAvailabaleTransDto;
        private Dtos.AccountFundsAvailable_Transactions2 acctFundsAvailabaleTrans2Dto;
        List<GlSourceCodes> glSourceCodes;
        private UserFactorySubset currentUserFactory = new GeneralLedgerCurrentUser.UserFactorySubset();
        GeneralLedgerFiscalYearConfiguration glFiscYrConfig = new Domain.ColleagueFinance.Entities.GeneralLedgerFiscalYearConfiguration(12, "2017", DateTime.Now.Month, 11, "Y");
        private GeneralLedgerClassConfiguration testGlClassConfiguration;
        private GeneralLedgerUser glUser;


        [TestInitialize]
        public void Initialize()
        {
            accountFundsAvailableRepositoryMock = new Mock<IAccountFundsAvailableRepository>();
            generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
            colleagueFinanceReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            generalLedgerUserRepositoryMock = new Mock<IGeneralLedgerUserRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            loggerMock = new Mock<ILogger>();
            personRepositoryMock = new Mock<IPersonRepository>();

            curntUserFactory = new GeneralLedgerCurrentUser.AccountFundsAvailableUser();

            BuildData();

            accountFundsAvailableService = new AccountFundsAvailableService(personRepositoryMock.Object, accountFundsAvailableRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object,
                generalLedgerUserRepositoryMock.Object, colleagueFinanceReferenceDataRepositoryMock.Object, adapterRegistryMock.Object, curntUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        private void BuildData()
        {
            acctFundsAvailabaleTransDto = new AccountFundsAvailable_Transactions()
            {
                Transactions = new List<AccountFundsAvailable_Transactionstransactions>() 
                {
                    new AccountFundsAvailable_Transactionstransactions()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        Type = Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.generalEncumbranceCreate,
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines()
                           {
                               AccountingString = "11_00_01_00_20603_52010",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = Dtos.EnumProperties.CurrencyCodes.USD,
                                   Value = 100
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = Dtos.EnumProperties.CreditOrDebit.Credit,
                               
                           }                           
                        }
                    },
                     new AccountFundsAvailable_Transactionstransactions()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        Type = Dtos.EnumProperties.AccountFundsAvailable_TransactionsType.purchaseJournal,
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines()
                           {
                               AccountingString = "11_00_01_00_20603_52011",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = Dtos.EnumProperties.CurrencyCodes.CAD,
                                   Value = 50
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = Dtos.EnumProperties.CreditOrDebit.Debit
                           }
                        }
                    }
                }
            };

            acctFundsAvailabaleTrans2Dto = new AccountFundsAvailable_Transactions2()
            {
                Transactions = new List<AccountFundsAvailable_Transactionstransactions2>()
                {
                    new AccountFundsAvailable_Transactionstransactions2()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        Type = Dtos.EnumProperties.AccountFundsAvailable_TransactionsType2.generalEncumbranceCreate,
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines2>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines2()
                           {
                               AccountingString = "11_00_01_00_20603_52010",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = Dtos.EnumProperties.CurrencyCodes.USD,
                                   Value = 100
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = Dtos.EnumProperties.CreditOrDebit.Credit,
                               ReferenceDocument = new Dtos.ReferenceDocumentDtoProperty()
                               {
                                   ItemNumber = "123"
                               }

                           }
                        }
                    },
                     new AccountFundsAvailable_Transactionstransactions2()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        Type = Dtos.EnumProperties.AccountFundsAvailable_TransactionsType2.purchaseJournal,
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines2>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines2()
                           {
                               AccountingString = "11_00_01_00_20603_52011",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = Dtos.EnumProperties.CurrencyCodes.CAD,
                                   Value = 50
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = Dtos.EnumProperties.CreditOrDebit.Debit,
                               ReferenceDocument = new Dtos.ReferenceDocumentDtoProperty()
                               {
                                   ItemNumber = "123"
                               }
                           }
                        }
                    }
                }
            };


            glSourceCodes = new List<GlSourceCodes>() 
            {
                new GlSourceCodes("6e274e84-2cba-4f11-8404-be7a23e65663", "generalEncumbranceCreate", "Desc 1", "generalEncumbranceCreate"),
                new GlSourceCodes("2137e2e2-21d5-49e3-a676-c429da9bbc38", "purchaseJournal", "Desc 2", "purchaseJournal")
            };
            colleagueFinanceReferenceDataRepositoryMock.Setup(i => i.GetGlSourceCodesValcodeAsync(It.IsAny<bool>())).ReturnsAsync(glSourceCodes);
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountFundsAvailableRepositoryMock = null;
            colleagueFinanceReferenceDataRepositoryMock = null;
            generalLedgerConfigurationRepositoryMock = null;
            generalLedgerUserRepositoryMock = null;
            roleRepositoryMock  = null;
            adapterRegistryMock = null;
            currentUserFactoryMock = null;
            loggerMock = null;
            accountFundsAvailableService = null;
            glFiscYrConfig = null;
            testGlAccountStructure = null;
        }
        #endregion

        [TestMethod]
        public async Task AccountFundsAvailableService_GetAccountFundsAvailableByFilterCriteriaAsync()
        {
            var af = new List<FundsAvailable>()
                {
                    new FundsAvailable("11_00_01_00_20603_52010")
                    {
                        TransactionDate = DateTime.Now.AddDays(1),
                        AvailableStatus = FundsAvailableStatus.Override
                    }
                };
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync("1")).ReturnsAsync("GUID123");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(af);
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableService_UserHasAccess_Expense_MoreThan_Budget()
        {
            var af = new List<FundsAvailable>()
                {
                    new FundsAvailable("11_00_01_00_20603_52010")
                    {
                        TransactionDate = DateTime.Now.AddDays(1),
                        AvailableStatus = FundsAvailableStatus.NotAvailable
                    }
                };
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync("1")).ReturnsAsync("GUID123");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(af);
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 100, new System.DateTime(2016, 12, 31), "1");
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableService_AccountingStringWithProjectNumber_NoBudgetAmount()
        {
            var af = new List<FundsAvailable>()
                {
                    new FundsAvailable("11_00_01_00_20603_52010")
                    {
                        TransactionDate = DateTime.Now.AddDays(1),
                        AvailableStatus = FundsAvailableStatus.Override
                    }
                };
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync("1")).ReturnsAsync("GUID123");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(af);

            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010*1234", 10, DateTime.Now.AddDays(-5), "1");
            Assert.IsNotNull(actual);
            Assert.AreEqual(Dtos.EnumProperties.FundsAvailable.OverrideAvailable, actual.FundsAvailable);
        }
        
        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountFundsAvailableService_NoUserAccess()
        {
            
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync("1")).ReturnsAsync("GUID123");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new RepositoryException());
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
        }

        [TestMethod]
        public async Task AccountFundsAvailableService_NoSubmittedBy()
        {
            var af = new List<FundsAvailable>()
                {
                    new FundsAvailable("11_00_01_00_20603_52010")
                    {
                        TransactionDate = DateTime.Now.AddDays(1),
                        AvailableStatus = FundsAvailableStatus.Override
                    }
                };
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(af);
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "");
            Assert.IsNotNull(actual);
        }
        

        [TestMethod]
        public async Task AccountFundsAvailableService_CheckAccountFundsAvailable_Transactions2Async()
        {
            testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            testGlClassConfiguration = await new TestGeneralLedgerConfigurationRepository().GetClassConfigurationAsync();
            glUser = await new TestGeneralLedgerUserRepository().GetGeneralLedgerUserAsync(curntUserFactory.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole,
                testGlClassConfiguration.ClassificationName, testGlClassConfiguration.ExpenseClassValues);

            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });

           // generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetFiscalYearConfigurationAsync()).ReturnsAsync(glFiscYrConfig);
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            generalLedgerConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(testGlClassConfiguration);
            generalLedgerUserRepositoryMock.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(glUser);
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            
            List<FundsAvailable> fundsAvailableResult = new List<FundsAvailable>() { };
            fundsAvailableResult.Add(new FundsAvailable("11-00-02-67-60000-54004")
            {
                Amount = 100,
                AvailableStatus = FundsAvailableStatus.Available,
                TransactionDate = new DateTime(2017,9,1),
                CurrencyCode = "USD",
                Sequence = "0"               
            });
            fundsAvailableResult.Add(new FundsAvailable("11-00-02-67-60000-54006")
            {
                Amount = 100,
                AvailableStatus = FundsAvailableStatus.NotAvailable,
                TransactionDate = new DateTime(2017, 9, 1),
                CurrencyCode = "USD",
                Sequence = "1"
            });

            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(),"","","", "", "", null))
                .ReturnsAsync(fundsAvailableResult);


            var result = await accountFundsAvailableService.CheckAccountFundsAvailable_Transactions2Async(acctFundsAvailabaleTransDto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableService_CheckAccountFundsAvailable_Transactions3Async()
        {
            testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            testGlClassConfiguration = await new TestGeneralLedgerConfigurationRepository().GetClassConfigurationAsync();
            glUser = await new TestGeneralLedgerUserRepository().GetGeneralLedgerUserAsync(curntUserFactory.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole,
                testGlClassConfiguration.ClassificationName, testGlClassConfiguration.ExpenseClassValues);

            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });

            // generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetFiscalYearConfigurationAsync()).ReturnsAsync(glFiscYrConfig);
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            generalLedgerConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(testGlClassConfiguration);
            generalLedgerUserRepositoryMock.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(glUser);
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            List<FundsAvailable> fundsAvailableResult = new List<FundsAvailable>() { };
            fundsAvailableResult.Add(new FundsAvailable("11-00-02-67-60000-54004")
            {
                Amount = 100,
                AvailableStatus = FundsAvailableStatus.Available,
                TransactionDate = new DateTime(2017, 9, 1),
                CurrencyCode = "USD",
                Sequence = "0",
                ItemId = "1234"
            });
            fundsAvailableResult.Add(new FundsAvailable("11-00-02-67-60000-54006")
            {
                Amount = 100,
                AvailableStatus = FundsAvailableStatus.NotAvailable,
                TransactionDate = new DateTime(2017, 9, 1),
                CurrencyCode = "USD",
                Sequence = "1",
                ItemId = "456"
                
            });

            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), "", "", "", "", "", null))
                .ReturnsAsync(fundsAvailableResult);


            var result = await accountFundsAvailableService.CheckAccountFundsAvailable_Transactions3Async(acctFundsAvailabaleTrans2Dto);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountFundsAvailableService_CheckAccountFundsAvailable_Transactions2Async_RepoExeception()
        {
            testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            testGlClassConfiguration = await new TestGeneralLedgerConfigurationRepository().GetClassConfigurationAsync();
            glUser = await new TestGeneralLedgerUserRepository().GetGeneralLedgerUserAsync(curntUserFactory.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole,
                testGlClassConfiguration.ClassificationName, testGlClassConfiguration.ExpenseClassValues);

            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });

            //generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetFiscalYearConfigurationAsync()).ReturnsAsync(glFiscYrConfig);
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            generalLedgerConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(testGlClassConfiguration);
            generalLedgerUserRepositoryMock.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(glUser);
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), "", "", "", "", "", null))
                .ThrowsAsync(new RepositoryException());

            await accountFundsAvailableService.CheckAccountFundsAvailable_Transactions2Async(acctFundsAvailabaleTransDto);
            
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountFundsAvailableService_CheckAccountFundsAvailable_Transactions3Async_RepoExeception()
        {
            testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            testGlClassConfiguration = await new TestGeneralLedgerConfigurationRepository().GetClassConfigurationAsync();
            glUser = await new TestGeneralLedgerUserRepository().GetGeneralLedgerUserAsync(curntUserFactory.CurrentUser.PersonId, testGlAccountStructure.FullAccessRole,
                testGlClassConfiguration.ClassificationName, testGlClassConfiguration.ExpenseClassValues);

            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });

            //generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetFiscalYearConfigurationAsync()).ReturnsAsync(glFiscYrConfig);
            generalLedgerConfigurationRepositoryMock.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            generalLedgerConfigurationRepositoryMock.Setup(x => x.GetClassConfigurationAsync()).ReturnsAsync(testGlClassConfiguration);
            generalLedgerUserRepositoryMock.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(glUser);
            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");

            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), "", "", "", "", "", null))
                .ThrowsAsync(new RepositoryException());

            await accountFundsAvailableService.CheckAccountFundsAvailable_Transactions3Async(acctFundsAvailabaleTrans2Dto);

        }

        //[TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        //public async Task AccountFundsAvailableService_CheckAccountFundsAvailable_Transactions2Async_PO_Status_InvalidOperationException()
        //{
        //    acctFundsAvailabaleTransDto.Transactions.FirstOrDefault().ReferenceDocument = new ReferenceDocumentDtoProperty() { ItemNumber = "1" };
        //    viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
        //    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
        //    accountFundsAvailableRepositoryMock.Setup(i => i.GetPOStatusByItemNumber(It.IsAny<string>())).ReturnsAsync("ABCD");
        //    var result = await accountFundsAvailableService.CheckAccountFundsAvailable_Transactions2Async(acctFundsAvailabaleTransDto);
        //}

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableService_UserNotound_KeyNotFoundException()
        {
            var af = new List<FundsAvailable>()
                {
                    new FundsAvailable("11_00_01_00_20603_52010")
                    {
                        TransactionDate = DateTime.Now.AddDays(1),
                        AvailableStatus = FundsAvailableStatus.Override
                    }
                };
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(af);
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableService_ArgumentNullException()
        {
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });

            personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task AccountFundsAvailableService_Exception()
        {
            viewAccountFundsAvailableRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountFundsAvailable));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewAccountFundsAvailableRole });
            personRepositoryMock.Setup(rep => rep.GetPersonIdFromGuidAsync("1")).ReturnsAsync("GUID123");
            accountFundsAvailableRepositoryMock.Setup(repo => repo.CheckAvailableFundsAsync(It.IsAny<List<FundsAvailable>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
                .ThrowsAsync(new Exception());
            
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task AccountFundsAvailableService_PermissionsException()
        {
            var actual = await accountFundsAvailableService.GetAccountFundsAvailableByFilterCriteriaAsync("11_00_01_00_20603_52010", 10, new System.DateTime(2016, 12, 31), "1");
        }
    }
}