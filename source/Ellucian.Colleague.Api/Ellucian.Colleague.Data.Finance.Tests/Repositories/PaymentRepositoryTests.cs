// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class PaymentRepositoryTests : BaseRepositorySetup
    {
        private PaymentRepository repository;
        ReviewPaymentInfoResponse validConfResponse;
        ReviewPaymentInfoResponse errorConfResponse;
        StartStudentPaymentResponse validStartResponse;
        StartStudentPaymentResponse errorStartResponse;
        GetCashReceiptAckInfoResponse validCrResponse;
        GetCashReceiptAckInfoResponse errorCrResponse;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            MockInitialize();

            // Build the test repository
            repository = new PaymentRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestClass]
        public class PaymentRepository_GetConfirmation_Tests : PaymentRepositoryTests
        {
            [TestInitialize]
            public void PaymentRepository_GetConfirmation_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecutePaymentConfirmationCTX();

                // Build the test repository
                repository = new PaymentRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            public void PaymentRepository_GetConfirmation_Valid()
            {
                var result = repository.GetConfirmation("BANK", "ECHK", "100.00");
                Assert.AreEqual(validConfResponse.OutConfirmText, result.ConfirmationText);
                Assert.AreEqual(validConfResponse.OutConvFeeAmt, result.ConvenienceFeeAmount);
                Assert.AreEqual(validConfResponse.OutConvFeeCode, result.ConvenienceFeeCode);
                Assert.AreEqual(validConfResponse.OutConvFeeDesc, result.ConvenienceFeeDescription);
                Assert.AreEqual(validConfResponse.OutConvFeeGlNo, result.ConvenienceFeeGeneralLedgerNumber);
                Assert.AreEqual(validConfResponse.OutProviderAcct, result.ProviderAccount);
            }

            [TestMethod]
            public void PaymentRepository_GetConfirmation_Error()
            {
                var result = repository.GetConfirmation("BANK", "CC", "100.00");
                Assert.AreEqual(errorConfResponse.OutErrorMsg, result.ErrorMessage);
            }
        }

        [TestClass]
        public class PaymentRepository_StartPaymentProvider_Tests : PaymentRepositoryTests
        {
            [TestInitialize]
            public void PaymentRepository_StartPaymentProvider_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteStartPaymentCTX();

                // Build the test repository
                repository = new PaymentRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            public void PaymentRepository_StartPaymentProvider_Valid()
            {
                var result = repository.StartPaymentProvider(new Domain.Finance.Entities.Payments.Payment()
                    {
                        AmountToPay = 500m,
                        CheckDetails = new Domain.Finance.Entities.Payments.CheckPayment()
                        {
                            AbaRoutingNumber = "111111118",
                            BankAccountNumber = "1234554321",
                            BillingAddress1 = "123 Main Street",
                            CheckNumber = "101",
                            City = "Fairfax",
                            EmailAddress = "john.smith@ellucianuniversity.edu",
                            FirstName = "John",
                            LastName = "Smith",
                            State = "VA",
                            ZipCode = "22033"
                        },
                        ConvenienceFee = "1% Fee",
                        ConvenienceFeeAmount = 5m,
                        ConvenienceFeeGeneralLedgerNumber = "110101000000010100",
                        Distribution = "BANK",
                        PaymentItems = new List<Domain.Finance.Entities.Payments.PaymentItem>()
                        {
                            new Domain.Finance.Entities.Payments.PaymentItem()
                            {
                                AccountType = "01",
                                Description = "Charge",
                                InvoiceId = "123",
                                Overdue = false,
                                PaymentAmount = 500m,
                                PaymentComplete = true,
                                Term = "2014/FA"
                            }
                        },
                        PayMethod = "ECHK",
                        PersonId = "0001234",
                        ProviderAccount = "PPCC",
                        ReturnToOriginUrl = "http://www.ellucianuniversity.edu/payments",
                        ReturnUrl = "http://www.ellucianuniversity.edu/payments"
                    });
                Assert.AreEqual(validStartResponse.OutStartUrl, result.RedirectUrl);
            }

            [TestMethod]
            public void PaymentRepository_StartPaymentProvider_Error()
            {
                var result = repository.StartPaymentProvider(new Domain.Finance.Entities.Payments.Payment()
                {
                    AmountToPay = 500m,
                    CheckDetails = new Domain.Finance.Entities.Payments.CheckPayment()
                    {
                        AbaRoutingNumber = "111111118",
                        BankAccountNumber = "1234554321",
                        BillingAddress1 = "123 Main Street",
                        CheckNumber = "101",
                        City = "Fairfax",
                        EmailAddress = "john.smith@ellucianuniversity.edu",
                        FirstName = "John",
                        LastName = "Smith",
                        State = "VA",
                        ZipCode = "22033"
                    },
                    ConvenienceFee = "1% Fee",
                    ConvenienceFeeAmount = 5m,
                    ConvenienceFeeGeneralLedgerNumber = "110101000000010100",
                    Distribution = "BANK",
                    PaymentItems = new List<Domain.Finance.Entities.Payments.PaymentItem>()
                        {
                            new Domain.Finance.Entities.Payments.PaymentItem()
                            {
                                AccountType = "01",
                                Description = "Charge",
                                InvoiceId = "123",
                                Overdue = false,
                                PaymentAmount = 500m,
                                PaymentComplete = true,
                                Term = "2014/FA"
                            }
                        },
                    PayMethod = "ECHK",
                    PersonId = "0001235",
                    ProviderAccount = "PPCC",
                    ReturnToOriginUrl = "http://www.ellucianuniversity.edu/payments",
                    ReturnUrl = "http://www.ellucianuniversity.edu/payments"
                });
                Assert.AreEqual(errorStartResponse.OutErrorMsg, result.ErrorMessage);
            }
        }

        [TestClass]
        public class PaymentRepository_GetCashReceipt_Tests : PaymentRepositoryTests
        {
            [TestInitialize]
            public void PaymentRepository_GetCashReceipt_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteCashReceiptCTX();

                // Build the test repository
                repository = new PaymentRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            public void PaymentRepository_GetCashReceipt_Valid()
            {
                var result = repository.GetCashReceipt("4321", "1234");
                Assert.AreEqual(validCrResponse.IoCashRcptsId, result.CashReceiptsId);
                Assert.AreEqual(validCrResponse.OutArDeposits.Count, result.Deposits.Count);
                for (int i = 0; i < result.Deposits.Count; i++)
                {
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdDepDesc, result.Deposits[i].Description);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdDepType, result.Deposits[i].Type);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdLocation, result.Deposits[i].Location);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdLocationDesc, result.Deposits[i].LocationDescription);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdNetAmt, result.Deposits[i].NetAmount);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdPersonId, result.Deposits[i].PersonId);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdPersonName, result.Deposits[i].PersonName);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdTerm, result.Deposits[i].Term);
                    Assert.AreEqual(validCrResponse.OutArDeposits[i].OutArdTermDesc, result.Deposits[i].TermDescription);
                }
                Assert.AreEqual(validCrResponse.OutArPayments.Count, result.Payments.Count);
                for (int i = 0; i < result.Deposits.Count; i++)
                {
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpArDesc, result.Payments[i].Description);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpArType, result.Payments[i].Type);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpLocation, result.Payments[i].Location);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpLocationDesc, result.Payments[i].LocationDescription);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpNetAmt, result.Payments[i].NetAmount);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpPersonId, result.Payments[i].PersonId);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpPersonName, result.Payments[i].PersonName);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpPmtDesc, result.Payments[i].PaymentDescription);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutIpcRegControlId, result.Payments[i].PaymentControlId);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpTerm, result.Payments[i].Term);
                    Assert.AreEqual(validCrResponse.OutArPayments[i].OutArpTermDesc, result.Payments[i].TermDescription);
                }
                Assert.AreEqual(validCrResponse.OutConvenienceFees.Count, result.ConvenienceFees.Count);
                for (int i = 0; i < result.ConvenienceFees.Count; i++)
                {
                    Assert.AreEqual(validCrResponse.OutConvenienceFees[i].OutConvFeeAmt, result.ConvenienceFees[i].Amount);
                    Assert.AreEqual(validCrResponse.OutConvenienceFees[i].OutConvFeeCode, result.ConvenienceFees[i].Code);
                    Assert.AreEqual(validCrResponse.OutConvenienceFees[i].OutConvFeeDesc, result.ConvenienceFees[i].Description);
                }
                Assert.AreEqual(validCrResponse.OutErrorMsg, result.ErrorMessage);
                Assert.AreEqual(validCrResponse.OutIpcReturnUrl, result.ReturnUrl);
                Assert.AreEqual(validCrResponse.OutMerchantEmail, result.MerchantEmail);
                Assert.AreEqual(validCrResponse.OutMerchantNameAddr, result.MerchantNameAddress);
                Assert.AreEqual(validCrResponse.OutMerchantPhone, result.MerchantPhone);
                Assert.AreEqual(validCrResponse.OutNonArItems.Count, result.OtherItems.Count);
                for (int i = 0; i < result.OtherItems.Count; i++)
                {
                    Assert.AreEqual(validCrResponse.OutNonArItems[i].OutNonArCode, result.OtherItems[i].Code);
                    Assert.AreEqual(validCrResponse.OutNonArItems[i].OutNonArDesc, result.OtherItems[i].Description);
                    Assert.AreEqual(validCrResponse.OutNonArItems[i].OutNonArLocation, result.OtherItems[i].Location);
                    Assert.AreEqual(validCrResponse.OutNonArItems[i].OutNonArLocationDesc, result.OtherItems[i].LocationDescription);
                    Assert.AreEqual(validCrResponse.OutNonArItems[i].OutNonArNetAmt, result.OtherItems[i].NetAmount);
                }
                Assert.AreEqual(validCrResponse.OutPaymentMethods.Count, result.PaymentMethods.Count);
                for (int i = 0; i < result.Deposits.Count; i++)
                {
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptConfirmNos, result.PaymentMethods[i].ConfirmationNumber);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptControlNos, result.PaymentMethods[i].ControlNumber);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptPayMethodDescs, result.PaymentMethods[i].PayMethodDescription);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptPayMethods, result.PaymentMethods[i].PayMethodCode);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptTransAmts, result.PaymentMethods[i].TransactionAmount);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptTransDescs, result.PaymentMethods[i].TransactionDescription);
                    Assert.AreEqual(validCrResponse.OutPaymentMethods[i].OutRcptTransNos, result.PaymentMethods[i].TransactionNumber);
                }
                Assert.AreEqual(validCrResponse.OutRcptAckText, result.ReceiptAcknowledgeText);
                Assert.AreEqual(validCrResponse.OutRcptChangeReturnedAmt, result.ChangeReturned);
                Assert.AreEqual(validCrResponse.OutRcptDate, result.ReceiptDate);
                Assert.AreEqual(new Uri(validCrResponse.OutRcptFooterImage), result.AcknowledgeFooterImageUrl);
                Assert.AreEqual(validCrResponse.OutRcptFooterText, result.AcknowledgeFooterText);
                Assert.AreEqual(validCrResponse.OutRcptNo, result.ReceiptNo);
                Assert.AreEqual(validCrResponse.OutRcptPayerId, result.ReceiptPayerId);
                Assert.AreEqual(validCrResponse.OutRcptPayerName, result.ReceiptPayerName);
                Assert.AreEqual(validCrResponse.OutRcptTime, result.ReceiptTime);
                Assert.AreEqual(validCrResponse.OutRcptDate, result.ReceiptDate);
            }
            [Ignore]
            [TestMethod]
            public void PaymentRepository_GetCashReceipt_Error()
            {
                var result = repository.GetCashReceipt("4321", "1235");
                Assert.AreEqual(errorCrResponse.OutErrorMsg, result.ErrorMessage);
            }
        }

        #region Private Data Definition setup

        private void SetupExecutePaymentConfirmationCTX()
        {
            validConfResponse = new ReviewPaymentInfoResponse()
            {
                OutConfirmText = new List<string>() { "Confirmed" },
                OutConvFeeAmt = 5m,
                OutConvFeeCode = "CF5",
                OutConvFeeDesc = "$5.00 Convenience Fee",
                OutConvFeeGlNo = "110101000000010100",
                OutProviderAcct = "PPCC"
            };
            errorConfResponse = new ReviewPaymentInfoResponse()
            {
                OutErrorMsg = "Error occurred."
            };
            transManagerMock.Setup<ReviewPaymentInfoResponse>(
                trans => trans.Execute<ReviewPaymentInfoRequest, ReviewPaymentInfoResponse>(It.IsAny<ReviewPaymentInfoRequest>()))
                    .Returns<ReviewPaymentInfoRequest>(request =>
                    {
                        if (request.InPaymentMethod == "ECHK")
                        {
                            return validConfResponse;
                        }
                        return errorConfResponse;
                    });
        }

        private void SetupExecuteStartPaymentCTX()
        {
            validStartResponse = new StartStudentPaymentResponse()
            {
                OutStartUrl = "http://www.paymentprocessor.com/"
            };
            errorStartResponse = new StartStudentPaymentResponse()
            {
                OutErrorMsg = "Error occurred."
            };
            transManagerMock.Setup<StartStudentPaymentResponse>(
                trans => trans.Execute<StartStudentPaymentRequest, StartStudentPaymentResponse>(It.IsAny<StartStudentPaymentRequest>()))
                    .Returns<StartStudentPaymentRequest>(request =>
                    {
                        if (request.InPersonId == "0001234")
                        {
                            return validStartResponse;
                        }
                        return errorStartResponse;
                    });
        }

        private void SetupExecuteCashReceiptCTX()
        {
            validCrResponse = new GetCashReceiptAckInfoResponse()
            {
                OutArPayments = new List<OutArPayments>()
                {
                    new OutArPayments() { OutArpArDesc = "Student Receivables", OutArpArType = "01", OutArpLocation = "MC", OutArpLocationDesc = "Main Campus",
                    OutArpNetAmt = 500m, OutArpPersonId = "0001234", OutArpPersonName = "John Smith", OutArpPmtDesc = "Payment", OutArpTerm = "2014/FA",
                    OutArpTermDesc = "2014 Fall Term" }
                },
                OutArDeposits = new List<OutArDeposits>()
                {
                    new OutArDeposits() { OutArdDepDesc = "Meal Plan", OutArdDepType = "MEALS", OutArdLocation = "MC", OutArdLocationDesc = "Main Campus",
                    OutArdNetAmt = 100m, OutArdPersonId = "0001234", OutArdPersonName = "John Smith", OutArdTerm = "2014/FA", OutArdTermDesc = "2014 Fall Term" }
                },
                OutConvenienceFees = new List<OutConvenienceFees>()
                {
                    new OutConvenienceFees() { OutConvFeeAmt = 5m, OutConvFeeCode = "CF5", OutConvFeeDesc = "$5.00 Convenience Fee"}
                },
                OutIpcReturnUrl = "http://www.ellucianuniversity.edu/payments",
                OutMerchantEmail = "info@paymentprocessor.com",
                OutMerchantNameAddr = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                OutMerchantPhone = "(703) 123-3210",
                OutNonArItems = new List<OutNonArItems>()
                {
                    new OutNonArItems() { OutNonArCode = "TIX", OutNonArDesc = "Sports Tickets", OutNonArLocation = "MC", OutNonArLocationDesc = "Main Campus",
                    OutNonArNetAmt = 50m }
                },
                OutPaymentMethods = new List<OutPaymentMethods>()
                {
                    new OutPaymentMethods() { OutRcptConfirmNos = "12345", OutRcptControlNos = "1234", OutRcptPayMethodDescs = "Check", OutRcptPayMethods = "ECHK",
                    OutRcptTransAmts = 655m, OutRcptTransDescs = "Student Payment", OutRcptTransNos = "2345"}
                },
                OutRcptAckSubject = "Acknowledgement.",
                OutRcptAckText = new List<string>() { "Ack Text 1", "Ack Text 2" },
                OutRcptChangeReturnedAmt = 5m,
                OutRcptDate = DateTime.Today,
                OutRcptFooterImage = "http://www.paymentprocessor.com/image.png",
                OutRcptFooterText = new List<string>() { "Processed by PaymentProcessor" },
                OutRcptNo = "1234",
                OutRcptPayerId = "0001234",
                OutRcptPayerName = "John Smith",
                OutRcptTime = DateTime.Now,
            };
            errorCrResponse = new GetCashReceiptAckInfoResponse()
            {
                OutErrorMsg = "Error occurred."
            };
            transManagerMock.Setup<GetCashReceiptAckInfoResponse>(
                trans => trans.Execute<GetCashReceiptAckInfoRequest, GetCashReceiptAckInfoResponse>(It.IsAny<GetCashReceiptAckInfoRequest>()))
                    .Returns<GetCashReceiptAckInfoRequest>(request =>
                    {
                        if (request.IoCashRcptsId == "1234")
                        {
                            return validCrResponse;
                        }
                        return errorCrResponse;
                    });
        }

        #endregion
    }
}
