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
using Ellucian.Colleague.Domain.Finance.Tests;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class AccountsReceivableServiceTests : FinanceCoordinationTests
    {
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IAccountsReceivableRepository> arRepoMock;
        private IAccountsReceivableRepository arRepo;
        private Mock<IFinanceConfigurationRepository> configRepoMock;
        private IFinanceConfigurationRepository configRepo;
        private Mock<ITermRepository> termRepoMock;
        private ITermRepository termRepo;
        private Mock<IStaffRepository> staffRepoMock;
        private IStaffRepository staffRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private AccountsReceivableService service;

        private Domain.Finance.Entities.DueDateOverrides dueDateOverrides;
        private List<Domain.Finance.Entities.ReceivableType> receivableTypes;
        private List<Domain.Finance.Entities.DepositType> depositTypes;
        private List<Domain.Finance.Entities.DepositDue> depositsDue;
        private Domain.Finance.Entities.AccountHolder accountHolder;
        private List<Domain.Finance.Entities.Invoice> invoices;
        private List<Domain.Finance.Entities.InvoicePayment> invoicePayments;
        private Domain.Finance.Entities.Configuration.FinanceConfiguration financeConfiguration;
        private List<Domain.Finance.Entities.ReceivablePayment> receivablePayments;
        private Dtos.Finance.ReceivableInvoice invoiceDto;
        private Ellucian.Colleague.Domain.Entities.Role invoiceCreatorRole;
        private Ellucian.Colleague.Domain.Entities.Role viewAccountRole;
        private List<Domain.Finance.Entities.ChargeCode> chargeCodes;
        private List<Domain.Finance.Entities.InvoiceType> invoiceTypes;
        private List<Ellucian.Colleague.Domain.Student.Entities.Term> terms;
        private List<Domain.Finance.Entities.Deposit> deposits;
        private Domain.Base.Entities.Staff staff;

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
            service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
        }

        [TestCleanup]
        public void Cleanup()
        {
            roleRepoMock = null;
            roleRepo = null;
            arRepoMock = null;
            arRepo = null;
            configRepoMock = null;
            configRepo = null;
            userFactory = null;
            service = null;

            dueDateOverrides = null;
            receivableTypes = null;
            depositTypes = null;
            depositsDue = null;
            accountHolder = null;
            invoices = null;
            invoicePayments = null;
            financeConfiguration = null;
            receivablePayments = null;
            invoiceDto = null;
            invoiceCreatorRole = null;
            viewAccountRole = null;
            chargeCodes = null;
            invoiceTypes = null;
            terms = null;
        }

        [TestClass]
        public class AccountsReceivableService_GetReceivableTypes : AccountsReceivableServiceTests
        {
            [TestMethod]
            public void AccountsReceivableService_GetReceivableTypes_Valid()
            {
                var types = service.GetReceivableTypes();
                Assert.AreEqual(receivableTypes.Count, types.Count());
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetDepositTypes : AccountsReceivableServiceTests
        {
            [TestMethod]
            public void AccountsReceivableService_GetDepositTypes_Valid()
            {
                var types = service.GetDepositTypes();
                Assert.AreEqual(depositTypes.Count, types.Count());
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetAccountHolder : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder_UnauthorizedUser()
            {
                var accountHolder = service.GetAccountHolder("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder_Valid()
            {
                var accountHolder = service.GetAccountHolder(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder_UserIsAdmin()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var accountHolder = service.GetAccountHolder("0000895");
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                BuildService();
                service.GetAccountHolder("0000895");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder_UserIsProxy()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                var accountHolder = service.GetAccountHolder("0003315");
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder_UserIsProxyDifferentPerson()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                service.GetAccountHolder("0000895");
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetAccountHolder2 : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder2_UnauthorizedUser()
            {
                var accountHolder = service.GetAccountHolder2("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder2_Valid()
            {
                var accountHolder = service.GetAccountHolder2(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder2_UserIsAdmin()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var accountHolder = service.GetAccountHolder2("0000895");
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder2_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                BuildService();
                service.GetAccountHolder2("0000895");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetAccountHolder2_UserIsProxy()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                var accountHolder = service.GetAccountHolder2("0003315");
                Assert.IsNotNull(accountHolder);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetAccountHolder2_UserIsProxyDifferentPerson()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                service.GetAccountHolder2("0000895");
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetInvoice : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetInvoice_UnauthorizedUser()
            {
                var invoice = service.GetInvoice(invoices.Where(inv => inv.PersonId != userFactory.CurrentUser.PersonId).FirstOrDefault().Id);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoice_Valid()
            {
                dueDateOverrides.NonTermOverride = null;
                var invoice = service.GetInvoice("6");
                Assert.IsNotNull(invoice);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoice_NontermDueDateOverride()
            {
                dueDateOverrides.NonTermOverride = DateTime.Today.AddDays(7);
                var invoice = service.GetInvoice("6");
                Assert.IsNotNull(invoice);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoice_TermDueDateOverride()
            {
                dueDateOverrides.NonTermOverride = DateTime.Today.AddDays(7);
                var invoice = service.GetInvoice("5");
                Assert.IsNotNull(invoice);
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetInvoices : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetInvoices_UnauthorizedUser()
            {
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                service.GetInvoices(unauthorizedInvoiceIds);
            }

            /// <summary>
            /// The person ids on the returned invoices are for different users, permissions exception is expected if the current user does not 
            /// have access to each one of them
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetInvoices_MixedPersonInvoicesTest()
            {
                var authorizedInvoiceId = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id).First();
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id).ToList();
                List<string> invoiceIds = new List<string>() { authorizedInvoiceId };
                invoiceIds.AddRange(unauthorizedInvoiceIds);
                service.GetInvoices(invoiceIds);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetInvoices_ProxyCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = invoices.Where(i => i.PersonId == personId).Select(inv => inv.Id);
                var personInvoices = service.GetInvoices(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetInvoices_ProxyForDifferentPersonCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = new List<string>() { invoices.First().Id };
                service.GetInvoices(invoiceIds);
            }

            /// <summary>
            /// User is finance admin
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetInvoices_AdminCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                var personInvoices = service.GetInvoices(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is finance admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetInvoices_AdminWithNoPermissionsCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                service.GetInvoices(invoiceIds);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoices_Valid_DueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = service.GetInvoices(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoices_Valid_NoDueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                dueDateOverrides.CurrentPeriodOverride = null;
                dueDateOverrides.FuturePeriodOverride = null;
                dueDateOverrides.PastPeriodOverride = null;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = service.GetInvoices(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoices_NullIds()
            {
                var invs = service.GetInvoices(null);
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoices_EmptyIds()
            {
                var invs = service.GetInvoices(new List<string>());
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            public void AccountsReceivableService_GetInvoices_NoInvoicesForUser()
            {
                var invs = service.GetInvoices(new List<string>() { "abc" });
                Assert.AreEqual(0, invs.Count());
            }
        }

        [TestClass]
        public class AccountsReceivableService_QueryInvoicesAsync : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicesAsync_UnauthorizedUser()
            {
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicesAsync(unauthorizedInvoiceIds);
            }

            /// <summary>
            /// The person ids on the returned invoices are for different users, permissions exception is expected if the current user does not 
            /// have access to each one of them
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicesAsync_MixedPersonInvoicesTest()
            {
                var authorizedInvoiceId = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id).First();
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id).ToList();
                List<string> invoiceIds = new List<string>() { authorizedInvoiceId };
                invoiceIds.AddRange(unauthorizedInvoiceIds);
                await service.QueryInvoicesAsync(invoiceIds);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_ProxyCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = invoices.Where(i => i.PersonId == personId).Select(inv => inv.Id);
                var personInvoices = await service.QueryInvoicesAsync(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicesAsync_ProxyForDifferentPersonCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = new List<string>() { invoices.First().Id };
                await service.QueryInvoicesAsync(invoiceIds);
            }

            /// <summary>
            /// User is finance admin
            /// </summary>
            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_AdminCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                var personInvoices = await service.QueryInvoicesAsync(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is finance admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicesAsync_AdminWithNoPermissionsCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                await service.QueryInvoicesAsync(invoiceIds);
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_Valid_DueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicesAsync(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
                foreach (var inv in invs)
                {
                    Assert.AreEqual(inv.GetType(), typeof(Ellucian.Colleague.Dtos.Finance.Invoice));
                }
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_Valid_NoDueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                dueDateOverrides.CurrentPeriodOverride = null;
                dueDateOverrides.FuturePeriodOverride = null;
                dueDateOverrides.PastPeriodOverride = null;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicesAsync(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
                foreach (var inv in invs)
                {
                    Assert.AreEqual(inv.GetType(), typeof(Ellucian.Colleague.Dtos.Finance.Invoice));
                }
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_NullIds()
            {
                var invs = await service.QueryInvoicesAsync(null);
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicesAsync_EmptyIds()
            {
                var invs = await service.QueryInvoicesAsync(new List<string>());
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_RepositoryThrowsArgumentNullException()
            {
                var invs = await service.QueryInvoicePaymentsAsync(new List<string>() { "abc" });
            }

        }

        [TestClass]
        public class AccountsReceivableService_QueryInvoicePaymentsAsync : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentAsync_UnauthorizedUser()
            {
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicePaymentsAsync(unauthorizedInvoiceIds);
            }

            /// <summary>
            /// The person ids on the returned invoices are for different users, permissions exception is expected if the current user does not 
            /// have access to each one of them
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_MixedPersonInvoicesTest()
            {
                var authorizedInvoiceId = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id).First();
                var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id).ToList();
                List<string> invoiceIds = new List<string>() { authorizedInvoiceId };
                invoiceIds.AddRange(unauthorizedInvoiceIds);
                await service.QueryInvoicePaymentsAsync(invoiceIds);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_ProxyCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = invoices.Where(i => i.PersonId == personId).Select(inv => inv.Id);
                var personInvoices = await service.QueryInvoicePaymentsAsync(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_ProxyForDifferentPersonCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                string personId = userFactory.CurrentUser.ProxySubjects.First().PersonId;
                var invoiceIds = new List<string>() { invoices.First().Id };
                await service.QueryInvoicePaymentsAsync(invoiceIds);
            }

            /// <summary>
            /// User is finance admin
            /// </summary>
            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_AdminCanAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                var personInvoices = await service.QueryInvoicePaymentsAsync(invoiceIds);
                Assert.IsTrue(personInvoices != null && personInvoices.Any());
            }

            /// <summary>
            /// User is finance admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_AdminWithNoPermissionsCannotAccessDataTest()
            {
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                var invoiceIds = new List<string>() { invoices.First().Id, invoices.Last().Id };
                await service.QueryInvoicePaymentsAsync(invoiceIds);
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_Valid_DueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicePaymentsAsync(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
                foreach (var inv in invs)
                {
                    Assert.AreEqual(inv.GetType(), typeof(Ellucian.Colleague.Dtos.Finance.InvoicePayment));
                }
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_Valid_NoDueDateOverrides()
            {
                financeConfiguration.PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByPeriod;
                dueDateOverrides.CurrentPeriodOverride = null;
                dueDateOverrides.FuturePeriodOverride = null;
                dueDateOverrides.PastPeriodOverride = null;
                var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
                var invs = await service.QueryInvoicePaymentsAsync(authorizedInvoiceIds);
                Assert.IsNotNull(invs);
                foreach (var inv in invs)
                {
                    Assert.AreEqual(inv.GetType(), typeof(Ellucian.Colleague.Dtos.Finance.InvoicePayment));
                }
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_NullIds()
            {
                var invs = await service.QueryInvoicePaymentsAsync(null);
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_EmptyIds()
            {
                var invs = await service.QueryInvoicePaymentsAsync(new List<string>());
                Assert.AreEqual(0, invs.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public async Task AccountsReceivableService_QueryInvoicePaymentsAsync_RepositoryThrowsArgumentNullException()
            {
                var invs = await service.QueryInvoicePaymentsAsync(new List<string>() { "abc" });
            }

        }

        [TestClass]
        public class AccountsReceivableService_GetPayment : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetPayment_UnauthorizedUser()
            {
                var unauthorizedPaymentIds = receivablePayments.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(pmt => pmt.Id);
                var pmts = service.GetPayment(receivablePayments[1].Id);
            }

            [TestMethod]
            public void AccountsReceivableService_GetPayment_Valid()
            {
                var authorizedPaymentIds = receivablePayments.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(pmt => pmt.Id);
                var pmts = service.GetPayment(receivablePayments[0].Id);
                Assert.IsNotNull(pmts);
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetPayments : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetPayments_UnauthorizedUser()
            {
                var unauthorizedPaymentIds = receivablePayments.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(pmt => pmt.Id);
                var pmts = service.GetPayments(unauthorizedPaymentIds);
            }

            [TestMethod]
            public void AccountsReceivableService_GetPayments_Valid()
            {
                var authorizedPaymentIds = receivablePayments.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(pmt => pmt.Id);
                var pmts = service.GetPayments(authorizedPaymentIds);
                Assert.IsNotNull(pmts);
            }

            [TestMethod]
            public void AccountsReceivableService_GetPayments_NullIds()
            {
                var pmts = service.GetPayments(null);
                Assert.AreEqual(0, pmts.Count());
            }

            [TestMethod]
            public void AccountsReceivableService_GetPayments_EmptyIds()
            {
                var pmts = service.GetPayments(new List<string>());
                Assert.AreEqual(0, pmts.Count());
            }

            [TestMethod]
            public void AccountsReceivableService_GetPayments_NoPaymentsForUser()
            {
                var pmts = service.GetPayments(new List<string>() { "abc" });
                Assert.AreEqual(0, pmts.Count());
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetDepositsDue : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor admin, nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDepositsDue_UnauthorizedUser()
            {
                var depositsDue = service.GetDepositsDue("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDepositsDue_Valid()
            {
                var depositsDue = service.GetDepositsDue(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(depositsDue);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDepositsDue_UserIsAdmin()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var depositsDue = service.GetDepositsDue("00000895");
                Assert.IsNotNull(depositsDue);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDepositsDue_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                BuildService();
                service.GetDepositsDue("00000895");          
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDepositsDue_UserIsProxy()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                var depositsDue = service.GetDepositsDue("0003315");
                Assert.IsNotNull(depositsDue);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDepositsDue_UserIsProxyDifferentPerson()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                service.GetDepositsDue("00000895");
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetDistributions : AccountsReceivableServiceTests
        {
            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDistributions_UnauthorizedUser()
            {
                var distributions = service.GetDistributions("0001234", new List<string>() { "01", "02" }, "WMPT");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDistributions_Valid()
            {
                var distributions = service.GetDistributions(userFactory.CurrentUser.PersonId, new List<string>() { "01", "02" }, "WMPT").ToList();
                Assert.IsNotNull(distributions);
                Assert.AreEqual("BANK", distributions[0]);
                Assert.AreEqual("TRAV", distributions[1]);
            }

            /// <summary>
            /// User is admin
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDistributions_UserIsAdmin()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                BuildService();
                var depositsDue = service.GetDistributions("0001234", new List<string>() { "01", "02" }, "WMPT");
                Assert.IsNotNull(depositsDue);
            }

            /// <summary>
            /// User is admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDistributions_UserIsAdminNoPermissions()
            {
                userFactory = new FinanceConfigurationServiceTests.CurrentUserFactory();
                BuildService();
                service.GetDistributions("0001234", new List<string>() { "01", "02" }, "WMPT");
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void AccountsReceivableService_GetDistributions_UserIsProxy()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                var depositsDue = service.GetDistributions("0003315", new List<string>() { "01", "02" }, "WMPT");
                Assert.IsNotNull(depositsDue);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_GetDistributions_UserIsProxyDifferentPerson()
            {
                userFactory = new FinanceConfigurationServiceTests.StudentUserFactoryWithProxy();
                BuildService();
                service.GetDistributions("00000895", new List<string>() { "01", "02" }, "WMPT");
            }
        }

        [TestClass]
        public class AccountsReceivableService_PostReceivableInvoice : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void AccountsReceivableService_PostReceivableInvoice_UnauthorizedUser()
            {
                var postedInvoice = service.PostReceivableInvoice(invoiceDto);
            }

            [TestMethod]
            public void AccountsReceivableService_PostReceivableInvoice_Valid()
            {
                invoiceCreatorRole.AddPermission(new Permission("CREATE.AR.INVOICES"));
                userFactory = new FinanceCoordinationTests.InvoiceUserFactory();
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var postedInvoice = service.PostReceivableInvoice(invoiceDto);
                Assert.IsNotNull(postedInvoice);
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetDeposit : AccountsReceivableServiceTests
        {
            [TestMethod]
            public void AccountsReceivableService_GetDeposit_Valid()
            {
                var result = service.GetDeposit(deposits[0].Id);
                Assert.IsNotNull(result);
                Assert.AreEqual(deposits[0].Amount, result.Amount);
                Assert.AreEqual(deposits[0].Date, result.Date);
                Assert.AreEqual(deposits[0].DepositType, result.DepositType);
                Assert.AreEqual(deposits[0].ExternalIdentifier, result.ExternalIdentifier);
                Assert.AreEqual(deposits[0].ExternalSystem, result.ExternalSystem);
                Assert.AreEqual(deposits[0].Id, result.Id);
                Assert.AreEqual(deposits[0].PersonId, result.PersonId);
                Assert.AreEqual(deposits[0].ReceiptId, result.ReceiptId);
                Assert.AreEqual(deposits[0].TermId, result.TermId);
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetDeposits : AccountsReceivableServiceTests
        {
            [TestMethod]
            public void AccountsReceivableService_GetDeposits_Valid()
            {
                var result = service.GetDeposits(deposits.Select(d => d.Id));
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.Count());
                var resultList = result.ToList();
                for (int i = 0; i < resultList.Count; i++)
                {
                    Assert.AreEqual(deposits[i].Amount, resultList[i].Amount);
                    Assert.AreEqual(deposits[i].Date, resultList[i].Date);
                    Assert.AreEqual(deposits[i].DepositType, resultList[i].DepositType);
                    Assert.AreEqual(deposits[i].ExternalIdentifier, resultList[i].ExternalIdentifier);
                    Assert.AreEqual(deposits[i].ExternalSystem, resultList[i].ExternalSystem);
                    Assert.AreEqual(deposits[i].Id, resultList[i].Id);
                    Assert.AreEqual(deposits[i].PersonId, resultList[i].PersonId);
                    Assert.AreEqual(deposits[i].ReceiptId, resultList[i].ReceiptId);
                    Assert.AreEqual(deposits[i].TermId, resultList[i].TermId);
                }
            }
        }

        [TestClass]
        public class AccountsReceivableService_SearchAccountHoldersAsync : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableService_SearchAccountHoldersAsync_NullCriteria()
            {
                var result = await service.SearchAccountHoldersAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableService_SearchAccountHoldersAsync_EmptyCriteria()
            {
                var result = await service.SearchAccountHoldersAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AccountsReceivableService_SearchAccountHoldersAsync_NoPermission()
            {
                userFactory = new FinanceCoordinationTests.InvoiceUserFactory();
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
                var result = await service.SearchAccountHoldersAsync("Good Criteria");
            }

            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHoldersAsync_BadCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Bad Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>());
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (await service.SearchAccountHoldersAsync("Bad Criteria")).ToList();
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHoldersAsync_GoodCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Good Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>()
                    {
                        new Domain.Finance.Entities.AccountHolder("0000001", "Smith", null) 
                    });
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (await service.SearchAccountHoldersAsync("Good Criteria")).ToList();
                Assert.AreEqual(1, result.Count());
            }

       }

        [TestClass]
        public class AccountsReceivableService_SearchAccountHolders2Async : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchAccountHolders2Async_ThrowsArgumentNullExceptionTest()
            {
                await service.SearchAccountHoldersAsync2(null);
            }

            /// <summary>
            /// User does not have the right permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SearchAccountHolders2Async_ThrowsPermissionsExceptionTests()
            {
                userFactory = new FinanceCoordinationTests.InvoiceUserFactory();
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
                await service.SearchAccountHoldersAsync2("foo");
            }

            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHolders2Async_BadCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Bad Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>());
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (IEnumerable<Dtos.Finance.AccountHolder>)((await service.SearchAccountHoldersAsync2("Bad Criteria")).Dto);
                Assert.AreEqual(0, result.Count());
            }

            /// <summary>
            /// User has the right permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHolders2Async_GoodCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Good Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>()
                    {
                        new Domain.Finance.Entities.AccountHolder("0000001", "Smith", null) 
                    });
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (IEnumerable<Dtos.Finance.AccountHolder>)((await service.SearchAccountHoldersAsync2("Good Criteria")).Dto);
                Assert.AreEqual(1, result.Count());
            }            
        }

        [TestClass]
        public class AccountsReceivableService_SearchAccountHolders3Async : AccountsReceivableServiceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task SearchAccountHolders3Async_ThrowsArgumentNullExceptionTest()
            {
                await service.SearchAccountHolders3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SearchAccountHolders3Async_ThrowsArgumentExceptionTest()
            {
                await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria());
            }           

            /// <summary>
            /// User does not have correct permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task SearchAccountHolders3Async_ThrowsPermissionsExceptionTests()
            {
                userFactory = new FinanceCoordinationTests.InvoiceUserFactory();
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
                await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "foo" });
            }

            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHolders3Async_BadCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Bad Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>());
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (IEnumerable<Dtos.Finance.AccountHolder>)((await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "Bad Criteria" })).Dto);
                Assert.AreEqual(0, result.Count());
            }

            /// <summary>
            /// User has correct permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task AccountsReceivableService_SearchAccountHolders3Async_GoodCriteria()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Good Criteria"))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>()
                    {
                        new Domain.Finance.Entities.AccountHolder("0000001", "Smith", null) 
                    });
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (IEnumerable<Dtos.Finance.AccountHolder>)((await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "Good Criteria" })).Dto);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            public async Task SearchAccountHolders3Async_ReturnsExpectedResultByIdsTest()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByIdsAsync(new List<string>() { "0000001" }))
                    .ReturnsAsync(new List<Domain.Finance.Entities.AccountHolder>()
                    {
                        new Domain.Finance.Entities.AccountHolder("0000001", "Smith", null) 
                    });
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);

                var result = (IEnumerable<Dtos.Finance.AccountHolder>)((await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = null, Ids = new List<string>() { "0000001" } })).Dto);
                Assert.AreEqual(1, result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SearchAccountHolders3Async_ErrorSearchingByKeyword_ThrowsArgumentExceptionTest()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByKeywordAsync("Good Criteria")).ThrowsAsync(new Exception());
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
                await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = "Good Criteria", Ids = new List<string>() });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task SearchAccountHolders3Async_ErrorSearchingByIds_ThrowsArgumentExceptionTest()
            {
                viewAccountRole.AddPermission(new Permission("VIEW.STUDENT.ACCOUNT.ACTIVITY"));
                userFactory = new FinanceCoordinationTests.ViewAccountUserFactory();
                arRepoMock.Setup(repo => repo.SearchAccountHoldersByIdsAsync(new List<string>() { "000001" })).ThrowsAsync(new Exception());
                service = new AccountsReceivableService(adapterRegistry, arRepo, configRepo, termRepo, userFactory, roleRepo, logger, staffRepo);
                await service.SearchAccountHolders3Async(new AccountHolderQueryCriteria() { QueryKeyword = null, Ids = new List<string>() { "000001" } });
            }
        }

        [TestClass]
        public class AccountsReceivableService_GetChargeCodesAsync : AccountsReceivableServiceTests
        {
            [TestMethod]
            public async Task AccountsReceivableService_GetChargeCodesAsync_Valid()
            {
                var types = await service.GetChargeCodesAsync();
                Assert.AreEqual(chargeCodes.Count, types.Count());
            }
        }

        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var receivableTypeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.ReceivableType, Dtos.Finance.ReceivableType>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivableType, Dtos.Finance.ReceivableType>()).Returns(receivableTypeAdapter);

            var depositTypeAdapter = new AutoMapperAdapter<Domain.Finance.Entities.DepositType, Dtos.Finance.DepositType>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositType, Dtos.Finance.DepositType>()).Returns(depositTypeAdapter);

            var accountHolderAdapter = new AccountHolderEntityAdapter(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountHolder, Dtos.Finance.AccountHolder>()).Returns(accountHolderAdapter);
            
            var depositAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Dtos.Finance.Deposit>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Dtos.Finance.Deposit>()).Returns(depositAdapter);

            var invoiceAdapter = new InvoiceEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Invoice, Dtos.Finance.Invoice>()).Returns(invoiceAdapter);

            var invoicePaymentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.InvoicePayment, Dtos.Finance.InvoicePayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.InvoicePayment, Dtos.Finance.InvoicePayment>()).Returns(invoicePaymentAdapter);

            var chargeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Dtos.Finance.Charge>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Charge, Dtos.Finance.Charge>()).Returns(chargeAdapter);

            var receivablePaymentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivablePayment, Dtos.Finance.ReceivablePayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivablePayment, Dtos.Finance.ReceivablePayment>()).Returns(receivablePaymentAdapter);

            var allocationSourceAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocationSource, Ellucian.Colleague.Dtos.Finance.PaymentAllocationSource>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.PaymentAllocationSource, Dtos.Finance.PaymentAllocationSource>()).Returns(allocationSourceAdapter);
            
            var depositDueAdapter = new DepositDueEntityAdapter(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, Dtos.Finance.DepositDue>()).Returns(depositDueAdapter);

            var receivableInvoiceDtoAdapter = new ReceivableInvoiceDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.ReceivableInvoice, Ellucian.Colleague.Domain.Finance.Entities.ReceivableInvoice>()).Returns(receivableInvoiceDtoAdapter);
            
            var receivableChargeDtoAdapter = new ReceivableChargeDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.ReceivableCharge, Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge>()).Returns(receivableChargeDtoAdapter);

            var receivableInvoiceAdapter = new ReceivableInvoiceEntityAdapter(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivableInvoice, Dtos.Finance.ReceivableInvoice>()).Returns(receivableInvoiceAdapter);
            
            var receivableChargeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge, Ellucian.Colleague.Dtos.Finance.ReceivableCharge>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge, Dtos.Finance.ReceivableCharge>()).Returns(receivableChargeAdapter);

            var chargeCodeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.ChargeCode, Dtos.Finance.ChargeCode>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.ChargeCode, Dtos.Finance.ChargeCode>()).Returns(chargeCodeAdapter);

        }

        private void SetupData()
        {
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

            receivableTypes = TestReceivableTypesRepository.ReceivableTypes.ToList();

            depositTypes = TestDepositTypesRepository.DepositTypes.ToList();

            depositsDue = new List<Domain.Finance.Entities.DepositDue>()
            {
                new Domain.Finance.Entities.DepositDue("123", "0000895", 500m, "MEALS", DateTime.Today.AddDays(7))
                {
                    TermId = "2014/FA"
                },
                new Domain.Finance.Entities.DepositDue("124", "0003315", 300m, "RESHL", DateTime.Today.AddDays(14))
                {
                    TermId = "2014/FA"
                }
            };
            depositsDue[0].AddDeposit(new Domain.Finance.Entities.Deposit("123", "0000895", DateTime.Today.AddDays(-7), "MEALS", 300m) { TermId = "2014/FA" });
            depositsDue[1].AddDeposit(new Domain.Finance.Entities.Deposit("124", "0003315", DateTime.Today.AddDays(-14), "RESHL", 200m) { TermId = "2014/FA" });

            accountHolder = new Domain.Finance.Entities.AccountHolder("0000895", "Smith", null)
            {
                BirthDate = DateTime.Today.AddYears(-18),
                DeceasedDate = null,
                EthnicCodes = new List<string>() { "NHS" },
                Ethnicities = new List<Domain.Base.Entities.EthnicOrigin>() { Domain.Base.Entities.EthnicOrigin.White },
                FirstName = "Firstname",
                Gender = "M",
                GovernmentId = "123-45-6789",
                Guid = "6c1091e2-4e54-4dbb-8b3c-6d186ae71d59",
                MaritalStatus = Domain.Base.Entities.MaritalState.Single,
                MaritalStatusCode = "S",
                MiddleName = "Middlename",
                Nickname = "Nickname",
                PreferredAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                PreferredName = "Nickname Lastname",
                Prefix = "Mr.",
                RaceCodes = new List<string>() { "WH" },
                Suffix = "III"
            };
            accountHolder.AddDepositDue(depositsDue[0]);
            accountHolder.AddEmailAddress(new Domain.Base.Entities.EmailAddress("firstname.lastname@ellucian.edu", "PRI") { IsPreferred = true });
            accountHolder.AddPersonAlt(new Domain.Base.Entities.PersonAlt("0001235", "ALT"));

            invoices = TestInvoiceRepository.Invoices.ToList();
            invoicePayments = new List<Ellucian.Colleague.Domain.Finance.Entities.InvoicePayment>();
            foreach (var inv in invoices)
            {
                decimal amountPaid = inv.BaseAmount > 20 ? inv.BaseAmount - 10 : 0;
                var invPay = new Ellucian.Colleague.Domain.Finance.Entities.InvoicePayment(inv.Id, inv.PersonId, inv.ReceivableTypeCode, inv.TermId, inv.ReferenceNumber, inv.Date, inv.DueDate, inv.BillingStart, inv.BillingEnd, inv.Description, inv.Charges, amountPaid);
                invoicePayments.Add(invPay);
            }

            financeConfiguration = new Domain.Finance.Entities.Configuration.FinanceConfiguration()
            {
                ActivityDisplay = Domain.Finance.Entities.Configuration.ActivityDisplay.DisplayByTerm,
                ECommercePaymentsAllowed = true,
                IncludeDetail = true,
                IncludeHistory = true,
                IncludeSchedule = true,
                InstitutionName = "Ellucian University",
                NotificationText = "This is a notification.",
                PartialAccountPaymentsAllowed = true,
                PartialDepositPaymentsAllowed = true,
                PartialPlanPaymentsAllowed = Domain.Finance.Entities.Configuration.PartialPlanPayments.Allowed,
                PaymentDisplay = Domain.Finance.Entities.Configuration.PaymentDisplay.DisplayByTerm,
                PaymentMethods = new List<Domain.Finance.Entities.Configuration.AvailablePaymentMethod>()
                {
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "Web Credit Card",
                        InternalCode = "WEBCC",
                        Type = "CC"
                    },
                    new Domain.Finance.Entities.Configuration.AvailablePaymentMethod()
                    {
                        Description = "Web Check",
                        InternalCode = "ECHK",
                        Type = "CK"
                    }
                },
                PaymentReviewMessage = "Review your payment.",
                Periods = new List<Domain.Finance.Entities.FinancialPeriod>()
                {
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Past, null, DateTime.Today.AddMonths(-2)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Current, DateTime.Today.AddMonths(-2).AddDays(1), DateTime.Today.AddMonths(2)),
                    new Domain.Finance.Entities.FinancialPeriod(Domain.Base.Entities.PeriodType.Future, DateTime.Today.AddMonths(2).AddDays(1), null),
                },
                RemittanceAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                SelfServicePaymentsAllowed = true,
                ShowCreditAmounts = true,
                StatementTitle = "Student Statement",
                SupportEmailAddress = "support@ellucian.edu",
                UseGuaranteedChecks = false
            };

            receivablePayments = new List<Domain.Finance.Entities.ReceivablePayment>()
            {
                new Domain.Finance.Entities.ReceivablePayment("123", "REF123", "0000895", "01", "2014/FA", DateTime.Today.AddDays(-3), 1000m)
                {
                    IsArchived = false,
                    Location = "MC"
                },
                new Domain.Finance.Entities.ReceivablePayment("124", "REF124", "0003315", "01", "2014/FA", DateTime.Today.AddDays(-3), 1000m)
                {
                    IsArchived = false,
                    Location = "MC"
                }
            };
            receivablePayments[0].AddAllocation(new Domain.Finance.Entities.PaymentAllocation("1234", "123", Domain.Finance.Entities.PaymentAllocationSource.System, 1000m));
            receivablePayments[0].AddExternalSystemAndId("EXT", "EXT123");
            receivablePayments[1].AddAllocation(new Domain.Finance.Entities.PaymentAllocation("1235", "124", Domain.Finance.Entities.PaymentAllocationSource.System, 1000m));
            receivablePayments[1].AddExternalSystemAndId("EXT", "EXT124");

            invoiceDto = new Ellucian.Colleague.Dtos.Finance.ReceivableInvoice()
            {
                AdjustedByInvoices = new List<string>() { "124", "125" },
                AdjustmentToInvoice = "122",
                Amount = 1650,
                BaseAmount = 1500,
                BillingEnd = DateTime.Today.AddMonths(2),
                BillingStart = DateTime.Today.AddMonths(-2),
                Charges = new List<Ellucian.Colleague.Dtos.Finance.ReceivableCharge>()
                {
                    new Ellucian.Colleague.Dtos.Finance.ReceivableCharge(){
                        Id = "LINE1",
                        InvoiceId = "123",
                        Description = new List<string>(){"LINE1 description line 1", "LINE1 Description line 2"},
                        Code = "ACTFE",
                        BaseAmount = 100,
                        TaxAmount = 10,
                        AllocationIds = new List<string>(){"LINE1 Allocation 1", "LINE1 Allocation 2"},
                        PaymentPlanIds = new List<string>(){"LINE1 Payment Plan 1", "LINE1 Payment Plan 2"}
                    },
                    new Ellucian.Colleague.Dtos.Finance.ReceivableCharge() {
                        Id = "LINE2",
                        InvoiceId = "123",
                        Description = new List<string>(){"LINE2 description line 1", "LINE2 Description line 2"},
                        Code = "ADDFE",
                        BaseAmount = 200,
                        TaxAmount = 20,
                        AllocationIds = new List<string>(){"LINE2 Allocation 1", "LINE2 Allocation 2"},
                        PaymentPlanIds = new List<string>(){"LINE2 Payment Plan 1", "LINE2 Payment Plan 2"}
                    },
                    new Ellucian.Colleague.Dtos.Finance.ReceivableCharge() {
                        Id = "LINE3",
                        InvoiceId = "123",
                        Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                        Code = "APPFE",
                        BaseAmount = 300,
                        TaxAmount = 30,
                        AllocationIds = new List<string>(){"LINE3 Allocation 1", "LINE3 Allocation 2"},
                        PaymentPlanIds = new List<string>(){"LINE3 Payment Plan 1", "LINE3 Payment Plan 2"}
                    },
                        new Ellucian.Colleague.Dtos.Finance.ReceivableCharge() {
                        Id = "LINE4",
                        InvoiceId = "123",
                        Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                        Code = "ATHFE",
                        BaseAmount = 400,
                        TaxAmount = 40,
                        AllocationIds = null,
                        PaymentPlanIds = null
                    },
                        new Ellucian.Colleague.Dtos.Finance.ReceivableCharge() {
                        Id = "LINE5",
                        InvoiceId = "123",
                        Description = new List<string>(){"LINE3 description line 1", "LINE3 Description line 2"},
                        Code = "AUXFE",
                        BaseAmount = 500,
                        TaxAmount = 50,
                        AllocationIds = new List<string>(),
                        PaymentPlanIds = new List<string>()
                    }
                },
                Date = DateTime.Today.AddDays(-7),
                Description = "Invoice Description",
                DueDate = DateTime.Today.AddDays(7),
                ExternalIdentifier = "EXT123",
                ExternalSystem = "EXT",
                Id = "123",
                InvoiceType = "EXTRL",
                IsArchived = false,
                Location = "MC",
                PersonId = "0000895",
                ReceivableType = "01",
                ReferenceNumber = "REF123",
                TaxAmount = 150m,
                TermId = "2014/FA"
            };

            invoiceCreatorRole = new Ellucian.Colleague.Domain.Entities.Role(1, "Invoice Creator");
            viewAccountRole = new Ellucian.Colleague.Domain.Entities.Role(2, "View Accounts");

            chargeCodes = TestChargeCodesRepository.ChargeCodes.ToList();

            invoiceTypes = TestInvoiceTypeRepository.InvoiceTypes.ToList();

            terms = new List<Domain.Student.Entities.Term>()
            {
                new Domain.Student.Entities.Term("2014/SP", "2014 Spring Term", DateTime.Today.AddMonths(-6), DateTime.Today.AddMonths(-2), 2013, 2, false, false, "2014RSP", false) { FinancialPeriod = Domain.Base.Entities.PeriodType.Past },
                new Domain.Student.Entities.Term("2014/FA", "2014 Fall Term", DateTime.Today.AddMonths(-2).AddDays(1), DateTime.Today.AddMonths(2), 2014, 1, false, false, "2014RFA", false) { FinancialPeriod = Domain.Base.Entities.PeriodType.Current },
                new Domain.Student.Entities.Term("2015/SP", "2015 Spring Term", DateTime.Today.AddMonths(2).AddDays(1), DateTime.Today.AddMonths(6), 2014, 2, false, false, "2015RSP", false) { FinancialPeriod = Domain.Base.Entities.PeriodType.Future },           
            };

            deposits = new List<Domain.Finance.Entities.Deposit>()
            {
                new Domain.Finance.Entities.Deposit("101", "0001234", DateTime.Today, "MEALS", 300m) { TermId = "2015/SP" },
                new Domain.Finance.Entities.Deposit("102", "0001234", DateTime.Today.AddDays(-100), "RESHL", 500m) { TermId = "2014/FA" },
                new Domain.Finance.Entities.Deposit("103", "0001235", DateTime.Today.AddDays(-14), "PRKNG", 100m) { ReceiptId = "123" }
            };
            deposits[1].AddExternalSystemAndId("EXTSYS", "EXT102");

            staff = new Domain.Base.Entities.Staff("0000895", "Smith") { PrivacyCodes = new List<string>() };
        }

        private void SetupRepositories()
        {
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;
            roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Ellucian.Colleague.Domain.Entities.Role>()
            {
                invoiceCreatorRole,
                viewAccountRole,
            });

            arRepoMock = new Mock<IAccountsReceivableRepository>();
            arRepo = arRepoMock.Object;
            arRepoMock.Setup(repo => repo.ReceivableTypes).Returns(receivableTypes);
            arRepoMock.Setup(repo => repo.DepositTypes).Returns(depositTypes);
            foreach (var dep in deposits)
            {
                arRepoMock.Setup(repo => repo.GetDeposit(dep.Id)).Returns(deposits.Where(d => d.Id == dep.Id).FirstOrDefault());
            }
            var authorizedDepositIds = deposits.Where(d => d.PersonId == userFactory.CurrentUser.PersonId).Select(dep => dep.Id);
            var unauthorizedDepositIds = deposits.Where(d => d.PersonId != userFactory.CurrentUser.PersonId).Select(dep => dep.Id);
            arRepoMock.Setup(repo => repo.GetDeposits(authorizedDepositIds)).Returns(deposits.Where(d => d.PersonId == userFactory.CurrentUser.PersonId));
            arRepoMock.Setup(repo => repo.GetDeposits(unauthorizedDepositIds)).Returns(deposits.Where(d => d.PersonId != userFactory.CurrentUser.PersonId));
            arRepoMock.Setup(repo => repo.GetAccountHolder(It.IsAny<string>())).Returns(accountHolder);
            foreach (var inv in invoices)
            {
                arRepoMock.Setup(repo => repo.GetInvoice(inv.Id)).Returns(invoices.Where(i => i.Id == inv.Id).FirstOrDefault());
            }
            var authorizedInvoiceIds = invoices.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
            var unauthorizedInvoiceIds = invoices.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
            arRepoMock.Setup(repo => repo.GetInvoices(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>((ids) => invoices.Where(i => ids.Contains(i.Id)));
            
            arRepoMock.Setup(repo => repo.QueryInvoicePaymentsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<InvoiceDataSubset>()))
                .Returns<IEnumerable<string>, InvoiceDataSubset>((ids, b) => Task.FromResult(invoicePayments.Where(i => ids.Contains(i.Id))));
            
            arRepoMock.Setup(repo => repo.QueryInvoicePaymentsAsync(new List<string>() { "abc" }, It.IsAny<InvoiceDataSubset>())).Throws(new ArgumentOutOfRangeException());
            foreach (var pmt in receivablePayments)
            {
                arRepoMock.Setup(repo => repo.GetPayment(pmt.Id)).Returns(receivablePayments.Where(i => i.Id == pmt.Id).FirstOrDefault());
            }
            var authorizedPaymentIds = receivablePayments.Where(i => i.PersonId == userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
            var unauthorizedPaymentIds = receivablePayments.Where(i => i.PersonId != userFactory.CurrentUser.PersonId).Select(inv => inv.Id);
            arRepoMock.Setup(repo => repo.GetPayments(authorizedPaymentIds)).Returns(receivablePayments.Where(i => i.PersonId == userFactory.CurrentUser.PersonId));
            arRepoMock.Setup(repo => repo.GetPayments(unauthorizedPaymentIds)).Returns(receivablePayments.Where(i => i.PersonId != userFactory.CurrentUser.PersonId));
            arRepoMock.Setup(repo => repo.GetPayments(new List<string>() { "abc" })).Returns(new List<Domain.Finance.Entities.ReceivablePayment>());
            arRepoMock.Setup(repo => repo.GetDepositsDue(It.IsAny<string>())).Returns(depositsDue);
            arRepoMock.Setup(repo => repo.GetDistributions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>())).Returns(new List<string>() { "BANK", "TRAV"});
            arRepoMock.Setup(repo => repo.ChargeCodes).Returns(chargeCodes);
            arRepoMock.Setup(repo => repo.GetChargeCodesAsync()).ReturnsAsync(chargeCodes);
            arRepoMock.Setup(repo => repo.InvoiceTypes).Returns(invoiceTypes);
            var invoiceEntity = new ReceivableInvoiceDtoAdapter(adapterRegistry, logger).MapToType(invoiceDto);
            arRepoMock.Setup(repo => repo.PostReceivableInvoice(It.IsAny<Domain.Finance.Entities.ReceivableInvoice>())).Returns(invoiceEntity);

            configRepoMock = new Mock<IFinanceConfigurationRepository>();
            configRepo = configRepoMock.Object;
            configRepoMock.Setup(repo => repo.GetDueDateOverrides()).Returns(dueDateOverrides);
            configRepoMock.Setup(repo => repo.GetFinanceConfiguration()).Returns(financeConfiguration);

            termRepoMock = new Mock<ITermRepository>();
            termRepo = termRepoMock.Object;
            termRepoMock.Setup(repo => repo.Get()).Returns(terms);
            foreach (var termId in terms.Select(t => t.Code))
            {
                termRepoMock.Setup(repo => repo.Get(termId)).Returns(terms.Where(t => t.Code == termId).FirstOrDefault());
            }

            staffRepoMock = new Mock<IStaffRepository>();
            staffRepo = staffRepoMock.Object;
            staffRepoMock.Setup(repo => repo.Get(It.IsAny<string>())).Returns(staff);

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
        }
    }
}
