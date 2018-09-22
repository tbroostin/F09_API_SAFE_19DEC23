// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Finance.Services;
using Moq;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using System.Linq;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Finance;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Services
{
    [TestClass]
    public class PaymentServiceTests : FinanceCoordinationTests
    {
        private Mock<IRoleRepository> roleRepoMock;
        private IRoleRepository roleRepo;
        private Mock<IPaymentRepository> payRepoMock;
        private IPaymentRepository payRepo;
        private Mock<IAccountDueRepository> adRepoMock;
        private IAccountDueRepository adRepo;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private IAdapterRegistry adapterRegistry;
        private ILogger logger;
        private Mock<ICurrentUserFactory> userFactoryMock;
        private ICurrentUserFactory userFactory;
        private PaymentService service;

        private Dtos.Finance.Payments.Payment paymentDto;
        private Domain.Finance.Entities.Payments.ElectronicCheckProcessingResult eCheckResult;
        private Domain.Finance.Entities.Payments.PaymentProvider provider;
        private Domain.Finance.Entities.Payments.PaymentConfirmation confirmation;
        private Domain.Finance.Entities.Payments.PaymentReceipt receipt;
        private IEnumerable<Domain.Finance.Entities.Payments.AccountsReceivablePayment> receiptPayments;
        private IEnumerable<Domain.Finance.Entities.Payments.AccountsReceivableDeposit> receiptDeposits;
        private IEnumerable<Domain.Finance.Entities.Payments.GeneralPayment> receiptGeneralPayments;
        private Domain.Finance.Entities.Payments.ElectronicCheckPayer checkPayer;

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
            service = new PaymentService(adapterRegistry, payRepo, adRepo, userFactory, roleRepo, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            roleRepoMock = null;
            roleRepo = null;
            payRepoMock = null;
            payRepo = null;
            adRepoMock = null;
            adRepo = null;
            userFactory = null;
            service = null;

            paymentDto = null;
            eCheckResult = null;
            provider = null;
            confirmation = null;
            receipt = null;
            checkPayer = null;
        }

        [TestClass]
        public class PaymentService_GetPaymentConfirmation : PaymentServiceTests
        {
            [TestMethod]
            public void PaymentService_GetPaymentConfirmation_Valid()
            {
                var conf = service.GetPaymentConfirmation("BANK", "CC", "5");
                Assert.IsNotNull(conf);
            }
        }

        [TestClass]
        public class PaymentService_PostPaymentProvider : PaymentServiceTests
        {
            /// <summary>
            /// User is neither self nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_PostPaymentProvider_UnauthorizedUser()
            {
                var prov = service.PostPaymentProvider(paymentDto);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentService_PostPaymentProvider_Valid()
            {
                paymentDto.PersonId = userFactory.CurrentUser.PersonId;

                var prov = service.PostPaymentProvider(paymentDto);
                Assert.IsNotNull(prov);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentService_PostPaymentProvider_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                paymentDto.PersonId = userFactory.CurrentUser.ProxySubjects.First().PersonId;

                var result = service.PostPaymentProvider(paymentDto);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// Admin cannot make a payment
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_PostPaymentProvider_AdminCantMakePayment()
            {
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                service.PostPaymentProvider(paymentDto);
            }
        }

        [TestClass]
        public class PaymentService_GetPaymentReceipt : PaymentServiceTests
        {
            /// <summary>
            /// User is self - ar payment receipt
            /// </summary>
            [TestMethod]
            public void PaymentService_GetPaymentReceipt_PaymentTest()
            {
                receiptPayments.First().PersonId = userFactory.CurrentUser.PersonId;
                receipt.Payments.AddRange(receiptPayments);
                var rcpt = service.GetPaymentReceipt(null, "000001234");
                Assert.IsNotNull(rcpt);
            }

            /// <summary>
            /// User is self - ar deposit receipt
            /// </summary>
            [TestMethod]
            public void PaymentService_GetPaymentReceipt_DepositTest()
            {
                receiptDeposits.First().PersonId = userFactory.CurrentUser.PersonId;
                receipt.Deposits.AddRange(receiptDeposits);
                var rcpt = service.GetPaymentReceipt(null, "000001234");
                Assert.IsNotNull(rcpt);
            }

            /// <summary>
            /// User is self - other payment receipt
            /// </summary>
            [TestMethod]
            public void PaymentService_GetPaymentReceipt_OtherPaymentTest()
            {
                receipt.ReceiptPayerId = userFactory.CurrentUser.PersonId;
                receipt.OtherItems.AddRange(receiptGeneralPayments);
                var rcpt = service.GetPaymentReceipt(null, "000001234");
                Assert.IsNotNull(rcpt);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentService_GetPaymentReceipt_ProxyCanAccessDataTest()
            {
                receipt.Payments.AddRange(receiptPayments);
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                var rcpt = service.GetPaymentReceipt(null, "000001234");
                Assert.IsNotNull(rcpt);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_GetPaymentReceipt_ProxyForDifferentPersonCannotAccessDataTest()
            {
                receipt.Payments.AddRange(receiptPayments);

                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithDifferentProxy();
                BuildService();
                service.GetPaymentReceipt(null, "000001234");
            }

            /// <summary>
            /// User is finance admin
            /// </summary>
            [TestMethod]
            public void PaymentService_GetPaymentReceipt_AdminCanAccessDataTest()
            {
                receipt.Payments.AddRange(receiptPayments);                
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                var rcpt = service.GetPaymentReceipt(null, "000001234");
                Assert.IsNotNull(rcpt);
            }

            /// <summary>
            /// User is finance admin with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_GetPaymentReceipt_AdminNoPermissionsCannotAccessDataTest()
            {
                receipt.Payments.AddRange(receiptPayments);
                
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                service.GetPaymentReceipt(null, "000001234");
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_GetPaymentReceipt_CannotAccessDataTest()
            {
                receipt.Payments.AddRange(receiptPayments);
                userFactory = new FinanceCoordinationTests.StudentUserFactory();
                BuildService();
                service.GetPaymentReceipt(null, "000001234");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PaymentService_GetPaymentReceipt_NoValidId_ExceptionThrownTest()
            {
                service.GetPaymentReceipt(null, "000001234");
            }

            [TestMethod]            
            public void NullReceiptEntityReceived_NullReturnedTest()
            {
                payRepoMock.Setup(repo => repo.GetCashReceipt(It.IsAny<string>(), It.IsAny<string>())).Returns((Domain.Finance.Entities.Payments.PaymentReceipt)null);
                BuildService();
                Assert.IsNull(service.GetPaymentReceipt(null, "000001234"));
            }
        }

        [TestClass]
        public class PaymentService_PostProcessElectronicCheck : PaymentServiceTests
        {
            /// <summary>
            /// User is neither self nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_PostProcessElectronicCheck_UnauthorizedUser()
            {
                var result = service.PostProcessElectronicCheck(paymentDto);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentService_PostProcessElectronicCheck_Valid()
            {
                paymentDto.PersonId = userFactory.CurrentUser.PersonId;

                var result = service.PostProcessElectronicCheck(paymentDto);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentService_PostProcessElectronicCheck_UserIsProxy()
            {
                userFactory = new FinanceCoordinationTests.StudentUserFactoryWithProxy();
                BuildService();
                paymentDto.PersonId = userFactory.CurrentUser.ProxySubjects.First().PersonId;

                var result = service.PostProcessElectronicCheck(paymentDto);
                Assert.IsNotNull(result);
            }

            /// <summary>
            /// Admin cannot make a payment
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_PostProcessElectronicCheck_AdminCantMakePayment()
            {
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                service.PostProcessElectronicCheck(paymentDto);
            }
        }

        [TestClass]
        public class PaymentService_GetCheckPayerInformation : PaymentServiceTests
        {
            /// <summary>
            /// User is neither self nor proxy
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_GetCheckPayerInformation_UnauthorizedUser()
            {
                var info = service.GetCheckPayerInformation("0001234");
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void PaymentService_GetCheckPayerInformation_Self()
            {
                var info = service.GetCheckPayerInformation(userFactory.CurrentUser.PersonId);
                Assert.IsNotNull(info);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            [TestMethod]
            public void PaymentService_GetCheckPayerInformation_Proxy()
            {
                var info = service.GetCheckPayerInformation("0001233");
            }

            /// <summary>
            /// Admin cannot get this data
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PaymentService_GetCheckPayerInformation_AdminHasNoAccess()
            {
                financeAdminRole.AddPermission(new Permission(FinancePermissionCodes.ViewStudentAccountActivity));
                roleRepoMock.Setup(r => r.Roles).Returns(new List<Domain.Entities.Role>() { financeAdminRole });
                userFactory = new FinanceCoordinationTests.CurrentUserFactory();
                BuildService();
                service.GetCheckPayerInformation("0001233");
            }
        }

        private void SetupAdapters()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            var paymentConfirmationAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentConfirmation, Dtos.Finance.Payments.PaymentConfirmation>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentConfirmation, Dtos.Finance.Payments.PaymentConfirmation>()).Returns(paymentConfirmationAdapter);

            var paymentDtoAdapter = new PaymentDtoAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.Payments.Payment, Ellucian.Colleague.Domain.Finance.Entities.Payments.Payment>()).Returns(paymentDtoAdapter);

            var checkPaymentDtoAdapter = new AutoMapperAdapter<Dtos.Finance.Payments.CheckPayment, Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment>(adapterRegistryMock.Object, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.Payments.CheckPayment, Ellucian.Colleague.Domain.Finance.Entities.Payments.CheckPayment>()).Returns(checkPaymentDtoAdapter);

            var paymentItemDtoAdapter = new AutoMapperAdapter<Dtos.Finance.Payments.PaymentItem, Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Finance.Payments.PaymentItem, Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentItem>()).Returns(paymentItemDtoAdapter);

            var paymentProviderAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentProvider, Dtos.Finance.Payments.PaymentProvider>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentProvider, Dtos.Finance.Payments.PaymentProvider>()).Returns(paymentProviderAdapter);

            var paymentReceiptAdapter = new PaymentReceiptEntityAdapter(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentReceipt, Dtos.Finance.Payments.PaymentReceipt>()).Returns(paymentReceiptAdapter);

            var accountsReceivablePaymentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivablePayment, Dtos.Finance.Payments.AccountsReceivablePayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivablePayment, Dtos.Finance.Payments.AccountsReceivablePayment>()).Returns(accountsReceivablePaymentAdapter);

            var accountsReceivableDepositAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivableDeposit, Dtos.Finance.Payments.AccountsReceivableDeposit>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.AccountsReceivableDeposit, Dtos.Finance.Payments.AccountsReceivableDeposit>()).Returns(accountsReceivableDepositAdapter);

            var generalPaymentAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.GeneralPayment, Dtos.Finance.Payments.GeneralPayment>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.GeneralPayment, Dtos.Finance.Payments.GeneralPayment>()).Returns(generalPaymentAdapter);

            var convenienceFeeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ConvenienceFee, Dtos.Finance.Payments.ConvenienceFee>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ConvenienceFee, Dtos.Finance.Payments.ConvenienceFee>()).Returns(convenienceFeeAdapter);

            var paymentMethodAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentMethod, Dtos.Finance.Payments.PaymentMethod>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.PaymentMethod, Dtos.Finance.Payments.PaymentMethod>()).Returns(paymentMethodAdapter);

            var electronicCheckProcessingResultAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckProcessingResult, Dtos.Finance.Payments.ElectronicCheckProcessingResult>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckProcessingResult, Dtos.Finance.Payments.ElectronicCheckProcessingResult>()).Returns(electronicCheckProcessingResultAdapter);

            var electronicCheckPayerAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckPayer, Dtos.Finance.Payments.ElectronicCheckPayer>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Payments.ElectronicCheckPayer, Dtos.Finance.Payments.ElectronicCheckPayer>()).Returns(electronicCheckPayerAdapter);
        }

        private void SetupData()
        {
            paymentDto = new Dtos.Finance.Payments.Payment()
            {
                AmountToPay = 100m,
                CheckDetails = new Dtos.Finance.Payments.CheckPayment()
                {
                    AbaRoutingNumber = "111111118",
                    BankAccountNumber = "1234567890",
                    BillingAddress1 = "123 Main Street",
                    BillingAddress2 = "Apt. C",
                    CheckNumber = "1001",
                    City = "Fairfax",
                    DriversLicenseNumber = "B12345678",
                    DriversLicenseState = "VA",
                    EmailAddress = "john.smith@ellucianuniversity.edu",
                    FirstName = "John",
                    LastName = "Smith",
                    State = "VA",
                    ZipCode = "22033"
                },
                ConvenienceFee = "CF15",
                ConvenienceFeeAmount = 1.5m,
                ConvenienceFeeGeneralLedgerNumber = "110101000000010001",
                Distribution = "BANK",
                PaymentItems = new List<Dtos.Finance.Payments.PaymentItem>()
                {
                    new Dtos.Finance.Payments.PaymentItem()
                    {
                        AccountType = "01",
                        DepositDueId = "123",
                        Description = "Deposit Payment",
                        InvoiceId = null,
                        Overdue = false,
                        PaymentAmount = 100m,
                        PaymentComplete = true,
                        PaymentControlId = null,
                        PaymentPlanId = null,
                        Term = "2014/FA"
                    }
                },
                PayMethod = "CC",
                PersonId = "0003315",
                ProviderAccount = "ECOMMCC",
                ReturnToOriginUrl = null,
                ReturnUrl = null
            };

            provider = new Domain.Finance.Entities.Payments.PaymentProvider()
            {
                RedirectUrl = "http://www.ecommerce.com/credit-card"
            };

            confirmation = new Domain.Finance.Entities.Payments.PaymentConfirmation()
            {
                ConfirmationText = new List<string>() { "You have successfully", "made a payment." },
                ConvenienceFeeAmount = 1.50m,
                ConvenienceFeeCode = "CF15",
                ConvenienceFeeDescription = "1.5% Convenience Fee",
                ConvenienceFeeGeneralLedgerNumber = "110101000000010001",
                ProviderAccount = "ECOMMCC"
            };

            receipt = new Domain.Finance.Entities.Payments.PaymentReceipt()
             {
                 AcknowledgeFooterImageUrl = new Uri("http://ellucianuniversity.edu/footer.png"),
                 AcknowledgeFooterText = new List<string>() { "This is some...", "footer text." },
                 CashReceiptsId = "123",
                 ConvenienceFees = new List<Domain.Finance.Entities.Payments.ConvenienceFee>()
                {
                    new Domain.Finance.Entities.Payments.ConvenienceFee()
                    {
                        Amount = 1m,
                        Code = "1FLAT",
                        Description = "One Dollar Flat Fee"
                    },
                    new Domain.Finance.Entities.Payments.ConvenienceFee()
                    {
                        Amount = 2.5m,
                        Code = "25PCT",
                        Description = "2.5 Percent Fee"
                    }
                },
                 ErrorMessage = null,
                 MerchantEmail = "merchant@ellucian.edu",
                 MerchantPhone = "123-456-7890",
                 MerchantNameAddress = new List<string>() { "Ellucian University", "123 Main Street", "Fairfax, VA 22033" },                 
                 PaymentMethods = new List<Domain.Finance.Entities.Payments.PaymentMethod>()
                {
                    new Domain.Finance.Entities.Payments.PaymentMethod()
                    {
                        ConfirmationNumber = "1234567890",
                        ControlNumber = "2345",
                        PayMethodCode = "CC",
                        PayMethodDescription = "Credit Card",
                        TransactionAmount = 401m,
                        TransactionDescription = "Transaction 1",
                        TransactionNumber = "2345678901"
                    },
                    new Domain.Finance.Entities.Payments.PaymentMethod()
                    {
                        ConfirmationNumber = "1357924680",
                        ControlNumber = "3456",
                        PayMethodCode = "ECHK",
                        PayMethodDescription = "Electronic Check",
                        TransactionAmount = 452.5m,
                        TransactionDescription = "Transaction 2",
                        TransactionNumber = "3456789012"
                    }
                },                 
                 ReceiptAcknowledgeText = new List<string>() { "This is some...", "acknowledgment text." },
                 ReceiptDate = DateTime.Today,
                 ReceiptNo = "0001234",
                 ReceiptPayerId = "0003315",
                 ReceiptPayerName = "John Smith",
                 ReceiptTime = DateTime.Now.AddMinutes(-3),
                 ReturnUrl = "http://www.ellucian.edu/payments"
             };

            receiptDeposits = new List<Domain.Finance.Entities.Payments.AccountsReceivableDeposit>()
                {
                    new Domain.Finance.Entities.Payments.AccountsReceivableDeposit()
                    {
                        Description = "Meals Deposit",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 300m,
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "MEALS"
                    },
                    new Domain.Finance.Entities.Payments.AccountsReceivableDeposit()
                    {
                        Description = "Rooms Deposit",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 250m,
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "RESHL"
                    }
                };

            receiptGeneralPayments = new List<Domain.Finance.Entities.Payments.GeneralPayment>()
                {
                    new Domain.Finance.Entities.Payments.GeneralPayment()
                    {
                        Code = "CODE",
                        Description = "General Payment Code",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 100m
                    },
                    new Domain.Finance.Entities.Payments.GeneralPayment()
                    {
                        Code = "CODE2",
                        Description = "General Payment Code 2",
                        Location = "SC",
                        LocationDescription = "South Campus",
                        NetAmount = 200m
                    }
                };

            receiptPayments = new List<Domain.Finance.Entities.Payments.AccountsReceivablePayment>()
                {
                    new Domain.Finance.Entities.Payments.AccountsReceivablePayment()
                    {
                        Description = "Payment 1",
                        Location = "MC",
                        LocationDescription = "Main Campus",
                        NetAmount = 401m,
                        PaymentControlId = null,
                        PaymentDescription = "Payment Description 1",
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "01"
                    },
                    new Domain.Finance.Entities.Payments.AccountsReceivablePayment()
                    {
                        Description = "Payment 2",
                        Location = "SC",
                        LocationDescription = "South Campus",
                        NetAmount = 452.5m,
                        PaymentControlId = null,
                        PaymentDescription = "Payment Description 2",
                        PersonId = "0003315",
                        PersonName = "John Smith",
                        Term = "2014/FA",
                        TermDescription = "2014 Fall Term",
                        Type = "02"
                    }
                };

            eCheckResult = new Domain.Finance.Entities.Payments.ElectronicCheckProcessingResult()
            {
                CashReceiptsId = "000001234",
            };

            checkPayer = new Domain.Finance.Entities.Payments.ElectronicCheckPayer()
            {
                City = "Fairfax",
                Country = "United States",
                Email = "johnny@ellucianuniversity.edu",
                FirstName = "Johnny",
                LastName = "Smith",
                MiddleName = "Tobias",
                PostalCode = "22033",
                State = "VA",
                Street = "123 Main Street",
                Telephone = "703-123-4567"
            };
        }

        private void SetupRepositories()
        {
            roleRepoMock = new Mock<IRoleRepository>();
            roleRepo = roleRepoMock.Object;

            payRepoMock = new Mock<IPaymentRepository>();
            payRepo = payRepoMock.Object;
            payRepoMock.Setup(repo => repo.GetConfirmation(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(confirmation);
            payRepoMock.Setup(repo => repo.StartPaymentProvider(It.IsAny<Domain.Finance.Entities.Payments.Payment>())).Returns(provider);
            payRepoMock.Setup(repo => repo.GetCashReceipt(It.IsAny<string>(), It.IsAny<string>())).Returns(receipt);

            adRepoMock = new Mock<IAccountDueRepository>();
            adRepo = adRepoMock.Object;
            adRepoMock.Setup(repo => repo.ProcessCheck(It.IsAny<Domain.Finance.Entities.Payments.Payment>())).Returns(eCheckResult);
            adRepoMock.Setup(repo => repo.GetCheckPayerInformation(It.IsAny<string>())).Returns(checkPayer);

            userFactoryMock = new Mock<ICurrentUserFactory>();
            userFactory = userFactoryMock.Object;
        }
    }
}
