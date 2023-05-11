// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class ReceiptRepositoryTests : BaseRepositorySetup
    {
        private ReceiptRepository repository;

        public void Initialize()
        {
            MockInitialize();

            repository = new ReceiptRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestClass]
        public class CreateReceipt : ReceiptRepositoryTests
        {
            private List<Deposit> inDeposits;
            private Deposit inDeposit;
            private List<Deposit> outDeposits;
            private Deposit outDeposit;
            private List<string> outDepositIds;
            private string depositId;
            private NonCashPayment inNonCashPayment;
            private List<NonCashPayment> nonCashPayments;
            private string payMethod;
            private Receipt inReceipt;
            private Receipt outReceipt;
            private string receiptId;
            private string receiptNumber;
            private string accountHolderId;
            private DateTime paymentDate;
            private string distribution;
            private string depositTypeCode;
            private decimal pmtAmount;
            private CreateCashReceiptRequest request;
            private CreateCashReceiptResponse response;
            private CreateCashReceiptResponse depositCrResponse;
            private CreateCashReceiptResponse errorCrResponse;

            [TestInitialize]
            public void CreateReceipt_Initialize()
            {
                Initialize();

                accountHolderId = "1234567";
                paymentDate = DateTime.Today;
                pmtAmount = 100m;
                receiptId = "0123456789";
                receiptNumber = "0000065432";

                depositId = "123";
                depositTypeCode = "ROOM";
                inDeposits = new List<Deposit>();
                inDeposit = new Deposit(null, accountHolderId, paymentDate, depositTypeCode, pmtAmount);
                inDeposits.Add(inDeposit);
                outDeposit = new Deposit(depositId, accountHolderId, paymentDate, depositTypeCode, pmtAmount);
                outDeposits = new List<Deposit> {outDeposit};
                outDepositIds = outDeposits.Select(x => x.Id).ToList();

                payMethod = "CC";
                inNonCashPayment = new NonCashPayment(payMethod, pmtAmount);
                nonCashPayments = new List<NonCashPayment>();
                nonCashPayments.Add(inNonCashPayment);

                distribution = "BANK";
                inReceipt = new Receipt(null, null, paymentDate, accountHolderId, distribution, null, nonCashPayments);
                outReceipt = new Receipt(receiptId, receiptNumber, paymentDate, accountHolderId, distribution, outDepositIds, nonCashPayments);

                var depInfo = new DepositInformation
                {
                    DepositHolderId = inDeposit.PersonId,
                    DepositAmount = inDeposit.Amount
                };
                var depInfoList = new List<DepositInformation> {depInfo};
                var payInfo = new NonCashInformation
                {
                    NonCashAmount = pmtAmount,
                    NonCashPayMethodCode = payMethod
                };
                var payInfoList = new List<NonCashInformation> {payInfo};

                request = new CreateCashReceiptRequest
                {
                    PayerId = inReceipt.PayerId,
                    PayerName = inReceipt.PayerName,
                    WebPaymentInd = false,
                    ReceiptDate = inReceipt.Date,
                    CashierId = "",
                    CashierSession = "",
                    Distribution = inReceipt.DistributionCode,
                    ExternalSystemCode = inReceipt.ExternalSystem,
                    ExternalIdentifier = inReceipt.ExternalIdentifier,
                    Mnemonic = "",
                    NonCashInformation = payInfoList,
                    DepositInformation = depInfoList
                };

                response = new CreateCashReceiptResponse
                {
                    CashReceiptId = receiptId,
                    DepositIds = outDepositIds
                };

                depositCrResponse = new CreateCashReceiptResponse()
                {
                    CashReceiptId = "123",
                    DepositIds = new List<string>() { "124" },
                };

                errorCrResponse = new CreateCashReceiptResponse()
                {
                    ErrorIndicator = true,
                    Messages = new List<string>() { "Error occurred." }
                };

                transManagerMock.Setup(tm => tm.Execute<CreateCashReceiptRequest, CreateCashReceiptResponse>(request)).Returns(response);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreateReceipt_SourceReceiptNull()
            {
                var result = repository.CreateReceipt(null, null, inDeposits);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CreateReceipt_SourcePaymentsNullDepositsNull()
            {
                var result = repository.CreateReceipt(inReceipt, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CreateReceipt_SourcePaymentsNullDepositsZero()
            {
                var result = repository.CreateReceipt(inReceipt, null, new List<Deposit>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CreateReceipt_SourcePaymentsZeroDepositsNull()
            {
                var result = repository.CreateReceipt(inReceipt, new List<ReceiptPayment>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void CreateReceipt_SourcePaymentsZeroDepositsZero()
            {
                var result = repository.CreateReceipt(inReceipt, new List<ReceiptPayment>(), new List<Deposit>());
            }

            [TestMethod]
            public void CreateReceipt_Deposits()
            {
                transManagerMock.Setup<CreateCashReceiptResponse>(
                    trans => trans.Execute<CreateCashReceiptRequest, CreateCashReceiptResponse>(It.IsAny<CreateCashReceiptRequest>()))
                        .Returns<CreateCashReceiptRequest>(request =>
                        {
                            return depositCrResponse;
                        });
                dataReaderMock.Setup(dr => dr.ReadRecord<RcptSessions>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new RcptSessions()
                {
                    RcptsDate = DateTime.Today,
                    RcptsEcommerceFlag = "Y",
                    RcptsEndDate = DateTime.Today.AddDays(1),
                    RcptsEndTime = DateTime.Today.AddDays(1),
                    RcptSessionsAdddate = DateTime.Today,
                    RcptSessionsAddopr = "JPM2",
                    RcptsLocation = "MC",
                    RcptsStartTime = DateTime.Today.AddHours(-4),
                    RcptsStatus = "O",
                    Recordkey = "126"
                });

                dataReaderMock.Setup(dr => dr.ReadRecord<CashRcpts>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new CashRcpts()
                    {
                        RcptConfirmNos = new List<string>() { "CONF123" },
                        RcptControlNos = new List<string>() { "CTRL123" },
                        RcptDate = DateTime.Today,
                        RcptDeposits = new List<string>() { "124" },
                        RcptEcommPayMethods = new List<string>() { "ECHK" },
                        RcptEcPayTransIds = new List<string>() { "125" },
                        RcptEncryptedControlNos = new List<string>() { "ENCR123" },
                        RcptEncryptedExpireDates = new List<string>() { "ENCR1117" },
                        RcptExpireDates = new List<DateTime?>() { new DateTime(2017, 11, 30) },
                        RcptExternalId = "EXT123",
                        RcptExternalSystem = "EXTSYS",
                        RcptNo = "123",
                        RcptNonCashAmts = new List<decimal?>() { 100m },
                        RcptNonCashConvFees = new List<decimal?>() { 1m },
                        RcptNonCashEntityAssociation = new List<CashRcptsRcptNonCash>()
                        {
                            new CashRcptsRcptNonCash() 
                            {
                                RcptConfirmNosAssocMember = "CONF123",
                                RcptControlNosAssocMember = "CTRL123",
                                RcptEcommPayMethodsAssocMember = "ECHK",
                                RcptEcPayTransIdsAssocMember = "125",
                                RcptEncryptedControlNosAssocMember = "ENCR123",
                                RcptEncryptedExpireDatesAssocMember = "ENCR1117",
                                RcptExpireDatesAssocMember = new DateTime(2017, 11, 30),
                                RcptNonCashAmtsAssocMember = 100m,
                                RcptNonCashConvFeesAssocMember = 1m,
                                RcptNonCashGlNosAssocMember = "110101000000010100",
                                RcptPayMethodsAssocMember = "ECHK",
                                RcptProviderAcctsAssocMember = "PPCC",
                                RcptUsedPayMethodsAssocMember = "ECHK"
                            }
                        },
                        RcptNonCashGlNos = new List<string>() { "110101000000010100"},
                        RcptSession = "126",
                        RcptPayerId = "0001234",
                        RcptPayerDesc = "John Smith",
                        RcptPayMethods = new List<string>() { "ECHK" },
                        RcptProviderAccts = new List<string>() { "PPCC" },
                        RcptUsedPayMethods = new List<string>() { "ECHK" },
                        RcptTenderGlDistrCode = "BANK",
                        Recordkey = "123"
                    });

                repository = new ReceiptRepository(cacheProvider, transFactory, logger, apiSettings);
                var result = repository.CreateReceipt(inReceipt, new List<ReceiptPayment>(), inDeposits);
                Assert.AreEqual(depositCrResponse.CashReceiptId, result.Id);
                Assert.IsTrue(result.DepositIds.Any());
                Assert.AreEqual(depositCrResponse.DepositIds[0], result.DepositIds[0]);
                Assert.AreEqual("JPM2", result.CashierId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void CreateReceipt_CTXError()
            {
                transManagerMock.Setup<CreateCashReceiptResponse>(
                    trans => trans.Execute<CreateCashReceiptRequest, CreateCashReceiptResponse>(It.IsAny<CreateCashReceiptRequest>()))
                        .Returns<CreateCashReceiptRequest>(request =>
                        {
                            throw new ColleagueWebApiException();
                        });
                repository = new ReceiptRepository(cacheProvider, transFactory, logger, apiSettings);
                var result = repository.CreateReceipt(inReceipt, new List<ReceiptPayment>(), inDeposits);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void CreateReceipt_ErrorMessages()
            {
                transManagerMock.Setup<CreateCashReceiptResponse>(
                    trans => trans.Execute<CreateCashReceiptRequest, CreateCashReceiptResponse>(It.IsAny<CreateCashReceiptRequest>()))
                        .Returns<CreateCashReceiptRequest>(request =>
                        {
                            return errorCrResponse;
                        });
                repository = new ReceiptRepository(cacheProvider, transFactory, logger, apiSettings);
                var result = repository.CreateReceipt(inReceipt, new List<ReceiptPayment>(), inDeposits);
            }
        }

        [TestClass]
        public class GetReceipt : ReceiptRepositoryTests
        {
            private Collection<CashRcpts> cashRcpts;
            private Collection<RcptSessions> rcptSessions;
                
            [TestInitialize]
            public void GetReceipt_Initialize()
            {
                Initialize();

                cashRcpts = TestCashRcptsRepository.CashRcpts;
                MockRecords("CASH.RCPTS", cashRcpts);
                rcptSessions = TestRcptSessionsRepository.RcptSessions;
                MockRecords("RCPT.SESSIONS", rcptSessions);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NullId()
            {
                var result = repository.GetReceipt(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_EmptyId()
            {
                var result = repository.GetReceipt(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void GetReceipt_InvalidId()
            {
                var result = repository.GetReceipt("BAD-ID");
            }

            [TestMethod]
            public void GetReceipt_ValidId()
            {
                var rcpt = cashRcpts.First();
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);

                Assert.AreEqual(id, result.Id);
            }

            [TestMethod]
            public void GetReceipt_ValidReferenceNumber()
            {
                var rcpt = cashRcpts.First();
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);

                Assert.AreEqual(rcpt.RcptNo, result.ReferenceNumber);
            }

            [TestMethod]
            public void GetReceipt_ValidDate()
            {
                var rcpt = cashRcpts.First();
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);

                Assert.AreEqual(rcpt.RcptDate.Value, result.Date);
            }

            [TestMethod]
            public void GetReceipt_ValidDistributionCode()
            {
                var rcpt = cashRcpts.First();
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);

                Assert.AreEqual(rcpt.RcptTenderGlDistrCode, result.DistributionCode);
            }

            [TestMethod]
            public void GetReceipt_ValidPayerId()
            {
                var rcpt = cashRcpts.First();
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);

                Assert.AreEqual(rcpt.RcptPayerDesc, result.PayerName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NullExternalSystemNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NullExternalSystemEmptyExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(null, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NullExternalSystemNonNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(null, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_EmptyExternalSystemNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(string.Empty, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_EmptyExternalSystemEmptyExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(string.Empty, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_EmptyExternalSystemNonNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId(string.Empty, "123");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NonNullExternalSystemNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId("456", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetReceipt_NonNullExternalSystemEmptyExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId("456", string.Empty);
            }

            [TestMethod]
            public void GetReceipt_NonNullExternalSystemNonNullExternalId()
            {
                var rcpt = cashRcpts.First(cr => string.IsNullOrEmpty(cr.RcptExternalSystem));
                var id = rcpt.Recordkey;
                var result = repository.GetReceipt(id);
                result.AddExternalSystemAndId("456", "123");
                Assert.AreEqual("456", result.ExternalSystem);
                Assert.AreEqual("123", result.ExternalIdentifier);
            }
        }

        [TestClass]
        public class GetCashier : ReceiptRepositoryTests
        {
            private Collection<Cashiers> cashiers;
            private Dictionary<string, string> logins;

            [TestInitialize]
            public void GetCashier_Initialize()
            {
                Initialize();
                cashiers = TestCashiersRepository.Cashiers;
                logins = TestCashiersRepository.Logins;

                MockRecords("CASHIERS", cashiers);
                dataReaderMock.Setup<string[]>(r => r.Select("UT.OPERS", Moq.It.IsAny<string>())).Returns<string, string>((file, criteria) =>
                    {
                        var outList = new List<string>();
                        var splitCriteria = criteria.Split('\'');
                        if (splitCriteria.Length == 3)
                        {
                            var id = splitCriteria[1];
                            string val;
                            if (logins.TryGetValue(id, out val))
                            {
                                outList.Add(val);
                            }
                        }
                        return outList.ToArray();
                    });

                repository = new ReceiptRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetCashier_NullCashierId()
            {
                var result = repository.GetCashier(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetCashier_EmptyCashierId()
            {
                var result = repository.GetCashier(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void GetCashier_CashierNotFound()
            {
                var result = repository.GetCashier("0000001");
            }

            [TestMethod]
            public void GetCashier_ValidCashier_Id()
            {
                var cashier = cashiers[0];
                var id = cashier.Recordkey;
                var result = repository.GetCashier(id);

                Assert.AreEqual(id, result.Id);
            }

            [TestMethod]
            public void GetCashier_ValidCashier_Login()
            {
                var cashier = cashiers[0];
                var id = cashier.Recordkey;
                var result = repository.GetCashier(id);
                var login = logins[id];

                Assert.AreEqual(login, result.Login);
            }

            [TestMethod]
            public void GetCashier_ValidCashier_IsECommerceEnabled()
            {
                var cashier = cashiers[0];
                var id = cashier.Recordkey;
                var result = repository.GetCashier(id);

                Assert.AreEqual(cashier.CshrEcommerceFlag == "Y", result.IsECommerceEnabled);
            }

            [TestMethod]
            public void GetCashier_ValidCashier_CreditCardLimitAmount()
            {
                var cashier = cashiers[0];
                var id = cashier.Recordkey;
                var result = repository.GetCashier(id);

                Assert.AreEqual(cashier.CshrCrCardAmt, result.CreditCardLimitAmount);
            }

            [TestMethod]
            public void GetCashier_ValidCashier_CheckLimitAmount()
            {
                var cashier = cashiers[0];
                var id = cashier.Recordkey;
                var result = repository.GetCashier(id);

                Assert.AreEqual(cashier.CshrCheckAmt, result.CheckLimitAmount);
            }

            [TestMethod]
            public void GetCashier_ValidateAll()
            {
                foreach (var cashier in cashiers)
                {
                    var id = cashier.Recordkey;
                    var result = repository.GetCashier(id);
                    var login = logins[id];

                    Assert.AreEqual(id, result.Id);
                    Assert.AreEqual(login, result.Login, "ID = " + id);
                    Assert.AreEqual(cashier.CshrEcommerceFlag == "Y", result.IsECommerceEnabled, "ID = " + id);
                    Assert.AreEqual(cashier.CshrCrCardAmt, result.CreditCardLimitAmount, "ID = " + id);
                    Assert.AreEqual(cashier.CshrCheckAmt, result.CheckLimitAmount, "ID = " + id);
                }
            }
        }

        [TestClass]
        public class ConvertSessionStatusCodeToSessionStatus : ReceiptRepositoryTests
        {
            [TestMethod]
            public void ConvertSessionStatusCodeToSessionStatus_Open()
            {
                string status = "O";
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
                Assert.AreEqual(SessionStatus.Open, result);
            }

            [TestMethod]
            public void ConvertSessionStatusCodeToSessionStatus_Closed()
            {
                string status = "C";
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
                Assert.AreEqual(SessionStatus.Closed, result);
            }

            [TestMethod]
            public void ConvertSessionStatusCodeToSessionStatus_Reconciled()
            {
                string status = "R";
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
                Assert.AreEqual(SessionStatus.Reconciled, result);
            }

            [TestMethod]
            public void ConvertSessionStatusCodeToSessionStatus_Voided()
            {
                string status = "V";
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
                Assert.AreEqual(SessionStatus.Voided, result);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ConvertSessionStatusCodeToSessionStatus_Invalid_NullStatus()
            {
                string status = null;
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ConvertSessionStatusCodeToSessionStatus_Invalid_EmptyStatus()
            {
                string status = string.Empty;
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ConvertSessionStatusCodeToSessionStatus_Invalid_OtherStatus()
            {
                string status = "A";
                var result = ReceiptRepository.ConvertSessionStatusCodeToSessionStatus(status);
            }
        }

    }
}
