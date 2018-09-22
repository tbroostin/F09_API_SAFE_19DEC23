// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Finance.Services;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Finance;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class AccountDueServiceTests : FinanceCoordinationTests
    {
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IAccountDueRepository> adRepoMock;
        private IAccountDueRepository adRepo;
        private Mock<IAccountsReceivableRepository> arRepoMock;
        private IAccountsReceivableRepository arRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IFinanceConfigurationRepository> configRepoMock;
        private IFinanceConfigurationRepository configRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private AccountDueService service;

        private Domain.Finance.Entities.AccountDue.AccountDue pastAccountDue;
        private Domain.Finance.Entities.AccountDue.AccountDue currentAccountDue;
        private Domain.Finance.Entities.AccountDue.AccountDue futureAccountDue;
        private IEnumerable<Domain.Finance.Entities.FinancialPeriod> financialPeriods;
        private Domain.Finance.Entities.DueDateOverrides dueDateOverrides;
        private List<Domain.Finance.Entities.DepositDue> depositsDue;
        private List<Ellucian.Colleague.Domain.Student.Entities.Term> terms;
        private Domain.Finance.Entities.AccountDue.AccountDuePeriod accountDuePeriod;

        [TestInitialize]
        public void Initialize()
        {
            SetupData();
            SetupRepositories();
            SetupAdapters();

            userFactory = new FinanceCoordinationTests.StudentUserFactory();
            BuildService();
        }

        private void BuildService()
        {
            service = new AccountDueService(adapterRegistry, adRepo, arRepo, termRepo, configRepo, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            roleRepoMock = null;
            roleRepo = null;
            adRepoMock = null;
            adRepo = null;
            arRepoMock = null;
            arRepo = null;
            termRepoMock = null;
            termRepo = null;
            configRepoMock = null;
            configRepo = null;
            userFactory = null;
            service = null;

            pastAccountDue = null;
            currentAccountDue = null;
            futureAccountDue = null;
            financialPeriods = null;
            dueDateOverrides = null;
            depositsDue = null;
            terms = null;
            accountDuePeriod = null;
        }

        [TestClass]
        public class AccountDueService_GetAccountDue : AccountDueServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDue_UnauthorizedUser()
            {
                var accountDue = service.GetAccountDue("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDue_Valid_DepositsDue()
            {
                dueDateOverrides.NonTermOverride = null;
                var accountDue = service.GetAccountDue(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDue);
                Assert.IsNotNull(accountDue.AccountTerms);
                Assert.IsTrue(accountDue.AccountTerms.Count == 5);
                Assert.IsTrue(accountDue.AccountTerms[0].TermId == "2014/FA");
                Assert.IsTrue(accountDue.AccountTerms[1].TermId == "2015/SP");
                Assert.IsTrue(accountDue.AccountTerms[2].TermId == null);   
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDue_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.GetAccountDue("0001234"));
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDue_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                service.GetAccountDue("0001234");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDue_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.GetAccountDue("0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDue_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetAccountDue("0003315");
            }

            [TestMethod]
            public void AccountDueService_GetAccountDue_Valid_NoDepositsDue()
            {
                arRepoMock.Setup(repo => repo.GetDepositsDue(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.DepositDue>());
                var accountDue = service.GetAccountDue(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDue);
            }

            [TestMethod]
            public void AccountDueService_GetAccountDue_Valid_BalanceMatchTermsSum()
            {
                dueDateOverrides.NonTermOverride = null;
                var accountDue = service.GetAccountDue(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDue);
                Assert.IsNotNull(accountDue.AccountTerms);
                Assert.AreEqual(5, accountDue.AccountTerms.Count);
                var termsSum = 0m;
                foreach (var acctTerm in accountDue.AccountTerms)
                {
                    termsSum += acctTerm.Amount;
        }
                Assert.AreEqual(termsSum, accountDue.Balance);
            }

        }

        [TestClass]
        public class AccountDueService_GetAccountDuePeriod : AccountDueServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDuePeriod_UnauthorizedUser()
            {
                var accountDuePeriod = service.GetAccountDuePeriod("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDuePeriod_Valid_DepositsDue()
            {
                dueDateOverrides.PastPeriodOverride = null;
                var accountDuePeriod = service.GetAccountDuePeriod(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDuePeriod);
                Assert.IsNotNull(accountDuePeriod.Past);
                Assert.IsNotNull(accountDuePeriod.Past.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms.Count == 2);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms[0].DepositDueItems.Count == 0);
                Assert.IsNotNull(accountDuePeriod.Current);
                Assert.IsNotNull(accountDuePeriod.Current.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms.Count == 3);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].DepositDueItems.Count == 1);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[1].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[1].DepositDueItems.Count == 1);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[2].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[2].DepositDueItems.Count == 1); 
                Assert.IsNotNull(accountDuePeriod.Future);
                Assert.IsNotNull(accountDuePeriod.Future.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms.Count == 2);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms[0].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms[0].DepositDueItems.Count == 0);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDuePeriod_UserIsAdmin()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                Assert.IsNotNull(service.GetAccountDuePeriod("0001234"));
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDuePeriod_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                service.GetAccountDuePeriod("0001234");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountDueService_GetAccountDuePeriod_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                Assert.IsNotNull(service.GetAccountDuePeriod("0003315"));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountDueService_GetAccountDuePeriod_UserIsProxyForDifferentPerson()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetAccountDuePeriod("0003315");
            }

            [TestMethod]
            public void AccountDueService_GetAccountDuePeriod_Valid_NoDepositsDue()
            {
                arRepoMock.Setup(repo => repo.GetDepositsDue(It.IsAny<string>())).Returns(new List<Domain.Finance.Entities.DepositDue>());
                var accountDue = service.GetAccountDuePeriod(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDue);
            }

            [TestMethod]
            public void AccountDueService_GetAccountDuePeriod_Valid_BalanceMatchPeriodsSum()
            {
                dueDateOverrides.PastPeriodOverride = null;
                var accountDuePeriod = service.GetAccountDuePeriod(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountDuePeriod);
                Assert.IsNotNull(accountDuePeriod.Past);
                Assert.IsNotNull(accountDuePeriod.Past.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms.Count == 2);
                Assert.IsTrue(accountDuePeriod.Past.AccountTerms[0].DepositDueItems.Count == 0);
                Assert.IsNotNull(accountDuePeriod.Current);
                Assert.IsNotNull(accountDuePeriod.Current.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms.Count == 3);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[0].DepositDueItems.Count == 1);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[1].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[1].DepositDueItems.Count == 1);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[2].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Current.AccountTerms[2].DepositDueItems.Count == 1);
                Assert.IsNotNull(accountDuePeriod.Future);
                Assert.IsNotNull(accountDuePeriod.Future.AccountTerms);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms.Count == 2);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms[0].DepositDueItems != null);
                Assert.IsTrue(accountDuePeriod.Future.AccountTerms[0].DepositDueItems.Count == 0);
                var pastTermsSum = 0m;
                foreach (var acctTerm in accountDuePeriod.Past.AccountTerms)
                {
                    pastTermsSum += acctTerm.Amount;
                }
                var currentTermsSum = 0m;
                foreach (var acctTerm in accountDuePeriod.Current.AccountTerms)
                {
                    currentTermsSum += acctTerm.Amount;
                }
                var futureTermsSum = 0m;
                foreach (var acctTerm in accountDuePeriod.Future.AccountTerms)
                {
                    futureTermsSum += acctTerm.Amount;
                }
                var totalSum = pastTermsSum + currentTermsSum + futureTermsSum;
                Assert.AreEqual(pastTermsSum, accountDuePeriod.Past.Balance);
                Assert.AreEqual(currentTermsSum, accountDuePeriod.Current.Balance);
                Assert.AreEqual(futureTermsSum, accountDuePeriod.Future.Balance);
                Assert.AreEqual(totalSum, accountDuePeriod.Balance);
            }

        }

        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var accountDueAdapter = new AccountDueEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue>()).Returns(accountDueAdapter);

            var accountsReceivableDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, Dtos.Finance.AccountDue.AccountsReceivableDueItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, Dtos.Finance.AccountDue.AccountsReceivableDueItem>()).Returns(accountsReceivableDueItemAdapter);

            var invoiceDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, Dtos.Finance.AccountDue.InvoiceDueItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, Dtos.Finance.AccountDue.InvoiceDueItem>()).Returns(invoiceDueItemAdapter);

            var paymentPlanDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, Dtos.Finance.AccountDue.PaymentPlanDueItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, Dtos.Finance.AccountDue.PaymentPlanDueItem>()).Returns(paymentPlanDueItemAdapter);
            
            var detailedAccountPeriodEntityAdapter = new DetailedAccountPeriodEntityAdapter(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountActivity.DetailedAccountPeriod, Ellucian.Colleague.Dtos.Finance.AccountActivity.DetailedAccountPeriod>()).Returns(detailedAccountPeriodEntityAdapter);

            var depositDueAdapter = new DepositDueEntityAdapter(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, Ellucian.Colleague.Dtos.Finance.DepositDue>()).Returns(depositDueAdapter);

            var depositAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Ellucian.Colleague.Dtos.Finance.Deposit>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Ellucian.Colleague.Dtos.Finance.Deposit>()).Returns(depositAdapter);

            var accountDuePeriodAdapter = new AccountDuePeriodEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod>()).Returns(accountDuePeriodAdapter);       
        }

        private void SetupData()
        {
            pastAccountDue = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountTerm()
                    {
                        AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                        {
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 10000m,
                                Description = "Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-93),
                                Overdue = true,
                                Period = "PAST",
                                PeriodDescription = "Past",
                                Term = "2014/SP",
                                TermDescription = "2014 Spring Term"
                            },
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -5000m,
                                Description = "Credit Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-87),
                                Overdue = true,
                                Period = "PAST",
                                PeriodDescription = "Past",
                                Term = "2014/SP",
                                TermDescription = "2014 Spring Term"
                            },
                            new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Plan Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-93),
                                Overdue = true,
                                PaymentPlanCurrent = true,
                                PaymentPlanId = "124",
                                Period = "PAST",
                                PeriodDescription = "Past",
                                Term = "2014/SP",
                                TermDescription = "2014 Spring Term",
                                UnpaidAmount = 1000m
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-87),
                                InvoiceId = "000001234",
                                Overdue = true,
                                Period = "PAST",
                                PeriodDescription = "Past",
                                Term = "2014/SP",
                                TermDescription = "2014 Spring Term",
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -1000m,
                                Description = "Credit Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-93),
                                InvoiceId = "000001234",
                                Overdue = true,
                                Period = "PAST",
                                PeriodDescription = "Past",
                                Term = null,
                                TermDescription = null,
                            }
                        },
                        Amount = 10000m,
                        Description = "2014 Spring Term",
                        TermId = "2014/SP"
                    }
                },
                EndDate = DateTime.Today.AddMonths(-2).AddDays(-1),
                PersonId = "0000895",
                PersonName = "Johnny",
                StartDate = null
            };

            currentAccountDue = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountTerm()
                    {
                        AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                        {
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 10000m,
                                Description = "Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-3),
                                Overdue = true,
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2014/FA",
                                TermDescription = "2014 Fall Term"
                            },
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -5000m,
                                Description = "Credit Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(3),
                                Overdue = false,
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2015/SP",
                                TermDescription = "2014 Fall Term"
                            },
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -10000m,
                                Description = "Credit Charge 2",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(3),
                                Overdue = false,
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2014/FA",
                                TermDescription = "2014 Fall Term"
                            },
                            new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Plan Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-3),
                                Overdue = true,
                                PaymentPlanCurrent = true,
                                PaymentPlanId = "123",
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2014/FA",
                                TermDescription = "2014 Fall Term",
                                UnpaidAmount = 1000m
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(3),
                                InvoiceId = "000001234",
                                Overdue = false,
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2014/FA",
                                TermDescription = "2014 Fall Term",
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -1000m,
                                Description = "Credit Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(-3),
                                InvoiceId = "000001234",
                                Overdue = true,
                                Period = "CUR",
                                PeriodDescription = "Current",
                                Term = "2014/FA",
                                TermDescription = "2014 Fall Term",
                            }
                        },
                        Amount = 10000m,
                        Description = "2014 Fall Term",
                        TermId = "2014/FA"
                    }
                },
                EndDate = DateTime.Today.AddMonths(2),
                PersonId = "0000895",
                PersonName = "Johnny",
                StartDate = DateTime.Today.AddMonths(-2)
            };

            futureAccountDue = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountTerm()
                    {
                        AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                        {
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 10000m,
                                Description = "Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(93),
                                Overdue = false,
                                Period = "FTR",
                                PeriodDescription = "Future",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term"
                            },
                            new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -5000m,
                                Description = "Credit Charge 1",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(87),
                                Overdue = false,
                                Period = "FTR",
                                PeriodDescription = "Future",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term"
                            },
                            new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Plan Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(93),
                                Overdue = false,
                                PaymentPlanCurrent = true,
                                PaymentPlanId = "124",
                                Period = "FTR",
                                PeriodDescription = "Future",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term",
                                UnpaidAmount = 1000m
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = 1000m,
                                Description = "Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(87),
                                InvoiceId = "000001234",
                                Overdue = false,
                                Period = "FTR",
                                PeriodDescription = "Future",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term",
                            },
                            new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                            {
                                AccountDescription = "Student Receivables",
                                AccountType = "01",
                                AmountDue = -1000m,
                                Description = "Credit Invoice Charge",
                                Distribution = "BANK",
                                DueDate = DateTime.Today.AddDays(93),
                                InvoiceId = "000001234",
                                Overdue = false,
                                Period = "FTR",
                                PeriodDescription = "Future",
                                Term = "2015/SP",
                                TermDescription = "2015 Spring Term",
                            }
                        },
                        Amount = 10000m,
                        Description = "2015 Spring Term",
                        TermId = "2015/SP"
                    }
                },
                EndDate = null,
                PersonId = "0000895",
                PersonName = "Johnny",
                StartDate = DateTime.Today.AddMonths(2).AddDays(1)
            };
            financialPeriods = new List<Domain.Finance.Entities.FinancialPeriod>()
            {
                new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddMonths(-2).AddDays(-1)),
                new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2)),
                new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddMonths(2).AddDays(1), null)
            };

            dueDateOverrides = new Domain.Finance.Entities.DueDateOverrides()
            {
                CurrentPeriodOverride = DateTime.Today.AddDays(3),
                FuturePeriodOverride = DateTime.Today.AddMonths(3),
                NonTermOverride = DateTime.Today,
                PastPeriodOverride = DateTime.Today.AddMonths(-3),
                TermOverrides = new Dictionary<string, DateTime>()
                {
                    { "2014/FA", DateTime.Today.AddDays(-7) },
                }
            };

            depositsDue = new List<Domain.Finance.Entities.DepositDue>()
            {
                new Domain.Finance.Entities.DepositDue("123", "0000895", 1000m, "TUITN", DateTime.Today.AddDays(30))
                {
                    TermId = "2015/SP"
                },
                new Domain.Finance.Entities.DepositDue("124", "0000895", 150m, "DAMAG", DateTime.Today.AddDays(35))
                {
                    TermId = "2015/SP"
                },
                new Domain.Finance.Entities.DepositDue("125", "0000895", 500m, "MEALS", DateTime.Today.AddDays(7))
                {
                    TermId = "2014/FA"
                },
                new Domain.Finance.Entities.DepositDue("126", "0000895", 400m, "RESHL", DateTime.Today.AddDays(14)),
                new Domain.Finance.Entities.DepositDue("127", "0000895", 400m, "RESHL", DateTime.Today.AddMonths(-3)),
                new Domain.Finance.Entities.DepositDue("128", "0000895", 400m, "RESHL", DateTime.Today.AddMonths(3)),
            };
            depositsDue[0].AddDeposit(new Domain.Finance.Entities.Deposit("123", "0000895", DateTime.Today.AddDays(30), "TUITN", 1000m) { TermId = "2015/SP" });
            depositsDue[2].AddDeposit(new Domain.Finance.Entities.Deposit("125", "0000895", DateTime.Today.AddDays(5), "MEALS", 200m) { TermId = "2014/FA" });

            terms = new List<Domain.Student.Entities.Term>()
            {
                new Domain.Student.Entities.Term("2014/SP", "2014 Spring Term", DateTime.Today.AddMonths(-6), DateTime.Today.AddMonths(-2), 2013, 2, false, false, "2014RSP", false),
                new Domain.Student.Entities.Term("2014/FA", "2014 Fall Term", DateTime.Today.AddMonths(-2).AddDays(1), DateTime.Today.AddMonths(2), 2014, 1, false, false, "2014RFA", false),
                new Domain.Student.Entities.Term("2015/SP", "2015 Spring Term", DateTime.Today.AddMonths(2).AddDays(1), DateTime.Today.AddMonths(6), 2014, 2, false, false, "2015RSP", false),           
            };

            accountDuePeriod = new Domain.Finance.Entities.AccountDue.AccountDuePeriod()
            {
                Past = pastAccountDue,
                Current = currentAccountDue,
                Future = futureAccountDue,
                PersonName = "Johnny"
            };
        }

        private void SetupRepositories()
        {
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            adRepoMock = new Mock<IAccountDueRepository>();
            adRepo = adRepoMock.Object;
            adRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns(currentAccountDue);
            adRepoMock.Setup(repo => repo.GetPeriods(It.IsAny<string>())).Returns(accountDuePeriod);

            arRepoMock = new Mock<IAccountsReceivableRepository>();
            arRepo = arRepoMock.Object;
            arRepoMock.Setup(repo => repo.GetDepositsDue(It.IsAny<string>())).Returns(depositsDue);
            arRepoMock.Setup(repo => repo.GetDistribution(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("BANK");

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            termRepoMock.Setup(repo => repo.Get()).Returns(terms);
            foreach (var termId in terms.Select(t => t.Code))
            {
                termRepoMock.Setup(repo => repo.Get(termId)).Returns(terms.Where(term => term.Code == termId).FirstOrDefault());
            }

            configRepoMock = new Mock<IFinanceConfigurationRepository>();
            configRepo = configRepoMock.Object;
            configRepoMock.Setup(repo => repo.GetFinancialPeriods()).Returns(financialPeriods);
            configRepoMock.Setup(repo => repo.GetDueDateOverrides()).Returns(dueDateOverrides);

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
        }
    }
}
