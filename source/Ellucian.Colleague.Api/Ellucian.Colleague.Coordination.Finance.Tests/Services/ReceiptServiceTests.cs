// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Coordination.Finance.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using AutoMapper;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class ReceiptServiceTests
    {
        FinanceReceiptServiceTestUserFactory _currentUserFactory;

        // mockable
        Mock<IAccountsReceivableRepository> _arRepository;

        //
        Mock<IAdapterRegistry> _adapterRegistry;
        Mock<IReceiptRepository> _receiptRepository;
        Mock<ITermRepository> _termRepository;
        Mock<IECommerceRepository> _eCommRepository;
        Mock<IRoleRepository> _roleRepository;
        Mock<ILogger> _logger;

        ReceiptService _receiptService;

        Role financeApiRole;

        // Literal data
        public static string id = "0000011";
        public static string financeApiRoleName = "FINANCE.API";
        string _payerId = "Payer1";
        string _holderId = "Holder1";
        string _receiptId = "Receipt1";
        string _distribution = "Dist1";
        string _payMethod = "PayMeth1";
        string _externalSystem = "ExternalSystem1";
        string _termId = "TERM1";
        string _depositType = "DEPTYPE1";


        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();

            // AR Repository
            _arRepository = new Mock<IAccountsReceivableRepository>();
            _arRepository.Setup(x => x.IsPerson(id)).Returns(true);
            _arRepository.Setup(x => x.IsPerson(_payerId)).Returns(true);
            _arRepository.Setup(x => x.IsPerson(_holderId)).Returns(true);
            _arRepository.Setup(x => x.ExternalSystems)
                .Returns(new List<Domain.Finance.Entities.ExternalSystem>() { new Domain.Finance.Entities.ExternalSystem(_externalSystem, _externalSystem) });
            _arRepository.Setup(x => x.DepositTypes)
                .Returns(new List<Domain.Finance.Entities.DepositType>() { new Domain.Finance.Entities.DepositType(_depositType, _depositType) });

            // adapters
            var dummyAdapterRegistry = new Mock<IAdapterRegistry>().Object;
            _adapterRegistry = new Mock<IAdapterRegistry>();
            _adapterRegistry.Setup(x => x.GetAdapter<Dtos.Finance.Deposit, Domain.Finance.Entities.Deposit>())
                .Returns(new DepositDtoAdapter(dummyAdapterRegistry, _logger.Object));
            _adapterRegistry.Setup(x => x.GetAdapter<Dtos.Finance.ReceiptPayment, Domain.Finance.Entities.ReceiptPayment>())
                .Returns(new AutoMapperAdapter<Dtos.Finance.ReceiptPayment, Domain.Finance.Entities.ReceiptPayment>(dummyAdapterRegistry, _logger.Object));
            _adapterRegistry.Setup(x => x.GetAdapter<Dtos.Finance.Receipt, Domain.Finance.Entities.Receipt>())
                .Returns(new ReceiptDtoAdapter(dummyAdapterRegistry, _logger.Object));
            _adapterRegistry.Setup(x => x.GetAdapter<Dtos.Finance.NonCashPayment, Domain.Finance.Entities.NonCashPayment>())
                .Returns(new NonCashPaymentDtoAdapter(dummyAdapterRegistry, _logger.Object));
            _adapterRegistry.Setup(x => x.GetAdapter<Domain.Finance.Entities.Receipt, Dtos.Finance.Receipt>())
                .Returns(new ReceiptEntityAdapter(dummyAdapterRegistry, _logger.Object));
            _adapterRegistry.Setup(x => x.GetAdapter<Domain.Finance.Entities.NonCashPayment, Dtos.Finance.NonCashPayment>())
                .Returns(new NonCashPaymentEntityAdapter(dummyAdapterRegistry, _logger.Object));

            // eComm repository
            _eCommRepository = new Mock<IECommerceRepository>();
            var distList =
            _eCommRepository.Setup(x => x.Distributions)
                .Returns(new List<Domain.Base.Entities.Distribution>() { new Domain.Base.Entities.Distribution(_distribution, _distribution) });
            _eCommRepository.Setup(x => x.PaymentMethods)
                .Returns(new List<Domain.Base.Entities.PaymentMethod>() 
                    { new Domain.Base.Entities.PaymentMethod(_payMethod, _payMethod, Domain.Base.Entities.PaymentMethodCategory.CreditCard, true, true)}
                );

            // roles
            financeApiRole = new Role(1, financeApiRoleName);
            financeApiRole.AddPermission(
                new Ellucian.Colleague.Domain.Entities.Permission(FinancePermissionCodes.CreateReceipts));
            _roleRepository = new Mock<IRoleRepository>();
            _roleRepository.Setup(x => x.Roles).Returns(new List<Role>(){financeApiRole});

            // receipt repository
            var outReceipt = new Domain.Finance.Entities.Receipt(_receiptId, "REF1", DateTime.Now.Date,_payerId, _distribution, new List<string>() {"Deposit1"},
                    new List<Domain.Finance.Entities.NonCashPayment>() {new Domain.Finance.Entities.NonCashPayment(_payMethod, 10000)});
            var outReceipt2 = new Domain.Finance.Entities.Receipt(null, "REF2", DateTime.Now.Date, _payerId, _distribution, new List<string>(),
                new List<Domain.Finance.Entities.NonCashPayment>() { new Domain.Finance.Entities.NonCashPayment(_payMethod, 10000) });

            _receiptRepository = new Mock<IReceiptRepository>();
            _receiptRepository.Setup(x => x.CreateReceipt(
                It.Is<Domain.Finance.Entities.Receipt>(r => r != null),
                It.Is<List<Domain.Finance.Entities.ReceiptPayment>>(p=>(p==null || !p.Any())),
                It.Is<List<Domain.Finance.Entities.Deposit>>       (d=>(d!=null &&  d.Any()))
                )).Returns(outReceipt);
            _receiptRepository.Setup(x => x.CreateReceipt(
                It.Is<Domain.Finance.Entities.Receipt>(r => r != null),
                It.Is<List<Domain.Finance.Entities.ReceiptPayment>>(p => (p != null && p.Any())),
                It.Is<List<Domain.Finance.Entities.Deposit>>(d => (d == null || !d.Any()))
                )).Returns(outReceipt2);

            // term repository
            _termRepository = new Mock<ITermRepository>();
            _termRepository.Setup(x => x.Get())
                .Returns(new List<Domain.Student.Entities.Term>() 
                { 
                    new Domain.Student.Entities.Term(_termId, _termId, DateTime.Now.Date, DateTime.Now.Date,
                        DateTime.Now.Year, 1, false, false, _termId, false) 
                });
        }

        //Successfully create an AR payment receipt
        [TestMethod]
        public void ReceiptService_CreateReceipt_Payments()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>();

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>()
                {
                    new ReceiptPayment()
                    {
                        Id = null,
                        Amount = 10000,
                        Date = DateTime.Now.Date,
                        Location = "MC",
                        ReceivableType = "01",
                        ReceiptId = "123",
                        ExternalIdentifier = "ExtDeposit1",
                        ExternalSystem = _externalSystem,
                        PersonId = _holderId,
                        TermId = _termId,
                    }
                };

            // good user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            // the call
            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
        }

        //Successfully create a deposit receipt
        [TestMethod]
        public void ReceiptService_CreateReceipt_Deposits()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // good user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            // the call
            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
            Assert.AreEqual(_receiptId, outReceipt.Id);
        }

        //Successfully create an AR payment receipt
        [TestMethod]
        public void ReceiptService_CreateReceipt_Payments_NullSourceDeposits()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = null;

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>()
                {
                    new ReceiptPayment()
                    {
                        Id = null,
                        Amount = 10000,
                        Date = DateTime.Now.Date,
                        Location = "MC",
                        ReceivableType = "01",
                        ReceiptId = "123",
                        ExternalIdentifier = "ExtDeposit1",
                        ExternalSystem = _externalSystem,
                        PersonId = _holderId,
                        TermId = _termId,
                    }
                };

            // good user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            // the call
            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
        }

        //Successfully create a deposit receipt
        [TestMethod]
        public void ReceiptService_CreateReceipt_Deposits_NullSourcePayments()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = null;

            // good user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            // the call
            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
            Assert.AreEqual(_receiptId, outReceipt.Id);
        }

        // User has no permissions
        [TestMethod]
        [ExpectedException(typeof(Ellucian.Web.Security.PermissionsException))]
        public void ReceiptService_InsufficientPermissions()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // bad user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(false);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
            Assert.AreEqual(_receiptId, outReceipt.Id);
        }
        // Unknown Payer
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiptService_InvalidPayerIdentifier()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = "FOO",
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // bad user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
            Assert.AreEqual(_receiptId, outReceipt.Id);
        }
        // Unknown Payer
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiptService_InvalidDepositHolderIdentifier()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = "FOO",
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // bad user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
            Assert.IsNotNull(outReceipt);
            Assert.AreEqual(_receiptId, outReceipt.Id);
        }

        // Entity-to-DTO mapping exception
        [TestMethod]
        [ExpectedException(typeof(AutoMapperMappingException))]
        public void ReceiptService_AutoMapperMappingException_NullInnerException()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // bad user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // Set up to throw auto mapper exception
            _adapterRegistry.Setup(x => x.GetAdapter<Domain.Finance.Entities.Receipt, Dtos.Finance.Receipt>())
                            .Throws(new AutoMapperMappingException());
            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
        }

        // Entity-to-DTO mapping exception
        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public void ReceiptService_AutoMapperMappingException_InnerException()
        {
            // payments
            List<NonCashPayment> nonCashPayments = new List<NonCashPayment>(){
                new NonCashPayment(){PaymentMethodCode = _payMethod, Amount = 10000},
            };

            // receipt
            Receipt sourceReceipt = new Receipt()
            {
                Id = null,
                PayerId = _payerId,
                PayerName = "Payer Name",
                Date = DateTime.Now.Date,
                CashierId = "Cashier1",
                DistributionCode = _distribution,
                ExternalSystem = _externalSystem,
                ExternalIdentifier = "ExtReceiptId1",
                NonCashPayments = nonCashPayments,
                TotalNonCashPaymentAmount = nonCashPayments.Sum(x => x.Amount),
            };

            // deposits
            List<Deposit> sourceDeposits = new List<Deposit>()
            {
                new Deposit()
                {
                    Id = null,
                    Amount = 10000,
                    Date = DateTime.Now.Date,
                    DepositType = _depositType,
                    ExternalIdentifier = "ExtDeposit1",
                    ExternalSystem = _externalSystem,
                    PersonId = _holderId,
                    TermId = _termId,
                }
            };

            // payments
            List<ReceiptPayment> sourcePayments = new List<ReceiptPayment>();

            // bad user
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);

            // Set up to throw auto mapper exception
            _adapterRegistry.Setup(x => x.GetAdapter<Domain.Finance.Entities.Receipt, Dtos.Finance.Receipt>())
                            .Throws(new AutoMapperMappingException("Message", new Exception("Auto mapper exception")));
            // the service
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var outReceipt = _receiptService.CreateReceipt(sourceReceipt, sourcePayments, sourceDeposits);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptService_GetCashier_NullId()
        {
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var result = _receiptService.GetCashier(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReceiptService_GetCashier_EmptyId()
        {
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var result = _receiptService.GetCashier(string.Empty);
        }

        [TestMethod]
        public void ReceiptService_GetCashier_ValidId()
        {
            _currentUserFactory = new FinanceReceiptServiceTestUserFactory(true);
            Domain.Finance.Entities.Cashier cashier = new Domain.Finance.Entities.Cashier("0001234", "john_smith", true) { CheckLimitAmount = 5000m, CreditCardLimitAmount = 2500m }; 
            _receiptRepository.Setup(x => x.GetCashier(It.IsAny<string>())).Returns(cashier);
            _receiptService = new ReceiptService(_adapterRegistry.Object, _receiptRepository.Object,
                _arRepository.Object, _termRepository.Object, _eCommRepository.Object, _currentUserFactory,
                _roleRepository.Object, _logger.Object);

            var result = _receiptService.GetCashier("0001234");
            Assert.AreEqual(cashier.Id, result.Id);
            Assert.AreEqual(cashier.IsECommerceEnabled, result.IsECommerceEnabled);
            Assert.AreEqual(cashier.Login, result.Login);
            Assert.AreEqual(cashier.CheckLimitAmount, result.CheckLimitAmount);
            Assert.AreEqual(cashier.CreditCardLimitAmount, result.CreditCardLimitAmount);
        }
    }

    public class FinanceReceiptServiceTestUserFactory : ICurrentUserFactory
    {
        private bool _goodPermissions;
        public ICurrentUser CurrentUser    
        {
            get{
                return new CurrentUser(new Claims(){
                    ControlId = "123",
                    Name = "Jimmy J. Johns, Jr.",
                    PersonId = ReceiptServiceTests.id,
                    SecurityToken = "321",
                    SessionTimeout = 30,
                    UserName = "Receipt Creator",
                    Roles = _goodPermissions ? new List<string>() { ReceiptServiceTests.financeApiRoleName }
                                             : new List<string>(),
                    SessionFixationId = "abc123",
                });
            }
        }

        public FinanceReceiptServiceTestUserFactory(bool goodPermissions)
        {
            _goodPermissions = goodPermissions;
        }
    }
}
