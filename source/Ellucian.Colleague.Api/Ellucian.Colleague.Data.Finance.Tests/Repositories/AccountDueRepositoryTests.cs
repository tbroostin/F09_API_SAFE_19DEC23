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
    public class AccountDueRepositoryTests : BaseRepositorySetup
    {
        private AccountDueRepository repository;
        StudentFinPaymentsDueAdminResponse validResponse;
        StudentFinPaymentsDueAdminResponse emptyResponse;
        GetEcheckPayerResponse validCheckResponse;
        GetEcheckPayerResponse emptyCheckResponse;
        ProcessECheckResponse validProcessResponse;
        ProcessECheckResponse errorProcessResponse;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            MockInitialize();

            // Build the test repository
            repository = new AccountDueRepository(cacheProvider, transFactory, logger);
        }

        [TestClass]
        public class AccountDueRepository_Get_Tests : AccountDueRepositoryTests
        {
            [TestInitialize]
            public void AccountDueRepository_Get_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecutePaymentsDueByTermAdminCTX();

                // Build the test repository
                repository = new AccountDueRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountDueRepository_Get_Valid()
            {
                var result = repository.Get("0001234");
                Assert.IsNotNull(result.AccountTerms);
                Assert.IsTrue(result.AccountTerms.Any());
                Assert.AreEqual("John Smith", result.PersonName);
            }

            [TestMethod]
            public void AccountDueRepository_Get_Empty()
            {
                var result = repository.Get("0001235");
                Assert.IsFalse(result.AccountTerms.Any());
            }
        }

        [TestClass]
        public class AccountDueRepository_GetPeriods_Tests : AccountDueRepositoryTests
        {
            [TestInitialize]
            public void AccountDueRepository_GetPeriods_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecutePaymentsDueByPeriodAdminCTX();

                // Build the test repository
                repository = new AccountDueRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountDueRepository_GetPeriods_Valid()
            {
                var result = repository.GetPeriods("0001234");
                Assert.IsNotNull(result.Current);
                Assert.IsNotNull(result.Future);
                Assert.IsNotNull(result.Past);
                Assert.AreEqual("John Smith", result.PersonName);
            }

            [TestMethod]
            public void AccountDueRepository_GetPeriods_Empty()
            {
                var result = repository.GetPeriods("0001235");
                Assert.IsNotNull(result.Current);
                Assert.IsNotNull(result.Future);
                Assert.IsNotNull(result.Past);
                Assert.AreEqual("John Smith", result.PersonName);
            }
        }

        [TestClass]
        public class AccountDueRepository_GetCheckPayerInformation_Tests : AccountDueRepositoryTests
        {
            [TestInitialize]
            public void AccountDueRepository_GetCheckPayerInformation_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteCheckPayerInformationCTX();

                // Build the test repository
                repository = new AccountDueRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountDueRepository_GetCheckPayerInformation_Valid()
            {
                var result = repository.GetCheckPayerInformation("0001234");
                Assert.AreEqual(validCheckResponse.OutCity, result.City);
                Assert.AreEqual(validCheckResponse.OutCountry, result.Country);
                Assert.AreEqual(validCheckResponse.OutEmail, result.Email);
                Assert.AreEqual(validCheckResponse.OutFirstName, result.FirstName);
                Assert.AreEqual(validCheckResponse.OutLastName, result.LastName);
                Assert.AreEqual(validCheckResponse.OutMiddleName, result.MiddleName);
                Assert.AreEqual(validCheckResponse.OutPostalCode, result.PostalCode);
                Assert.AreEqual(validCheckResponse.OutState, result.State);
                Assert.AreEqual(validCheckResponse.OutStreet, result.Street);
                Assert.AreEqual(validCheckResponse.OutTelephone, result.Telephone);
            }

            [TestMethod]
            public void AccountDueRepository_GetCheckPayerInformation_Empty()
            {
                var result = repository.GetCheckPayerInformation("0001235");
                Assert.AreEqual(emptyCheckResponse.OutCity, result.City);
                Assert.AreEqual(emptyCheckResponse.OutCountry, result.Country);
                Assert.AreEqual(emptyCheckResponse.OutEmail, result.Email);
                Assert.AreEqual(emptyCheckResponse.OutFirstName, result.FirstName);
                Assert.AreEqual(emptyCheckResponse.OutLastName, result.LastName);
                Assert.AreEqual(emptyCheckResponse.OutMiddleName, result.MiddleName);
                Assert.AreEqual(emptyCheckResponse.OutPostalCode, result.PostalCode);
                Assert.AreEqual(emptyCheckResponse.OutState, result.State);
                Assert.AreEqual(emptyCheckResponse.OutStreet, result.Street);
                Assert.AreEqual(emptyCheckResponse.OutTelephone, result.Telephone);
            }
        }

        [TestClass]
        public class AccountDueRepository_ProcessCheck_Tests : AccountDueRepositoryTests
        {
            [TestInitialize]
            public void AccountDueRepository_ProcessCheck_Initialize()
            {
                // Initialize person setup and Mock framework
                SetupExecuteProcessCheckCTX();

                // Build the test repository
                repository = new AccountDueRepository(cacheProvider, transFactory, logger);
            }

            [TestMethod]
            public void AccountDueRepository_ProcessCheck_Valid()
            {
                var result = repository.ProcessCheck(new Domain.Finance.Entities.Payments.Payment()
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
                Assert.AreEqual(validProcessResponse.OutCashRcptsId, result.CashReceiptsId);
            }

            [TestMethod]
            public void AccountDueRepository_ProcessCheck_Error()
            {
                var result = repository.ProcessCheck(new Domain.Finance.Entities.Payments.Payment()
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
                Assert.AreEqual(errorProcessResponse.OutErrorMsg, result.ErrorMessage);
            }
        }

        #region Private Data Definition setup

        private void SetupExecutePaymentsDueByTermAdminCTX()
        {
            validResponse = new StudentFinPaymentsDueAdminResponse()
            {
                PaymentsDue = new List<PaymentsDue>() 
                {
                    new PaymentsDue() { ArTypeBals = 100m, ArTypeDescs = "Charge", ArTypeDist = "BANK", ArTypeDueDates = DateTime.Today, ArTypeOverdue = false,
                    ArTypes = "01", RelatedTerms = "2014/FA", RelatedTermDescs = "2014 Fall Term", Periods = "PAST", PeriodDescs = "Past" },
                    new PaymentsDue() { Invoices = "1234", InvoiceBals = 100m, InvoiceDescs = "Invoice", InvoicesDueDate = DateTime.Today, InvoicesOverdue = false,
                    RelatedTerms = "2015/SP", RelatedTermDescs = "2015 Spring Term", InvoiceDist = "TRAV", InvoiceArTypes = "02"},
                    new PaymentsDue() { Invoices = "1235", InvoiceBals = 200m, InvoiceDescs = "Invoice", InvoicesDueDate = DateTime.Today.AddDays(-3), InvoicesOverdue = true,
                    RelatedTerms = "NON-TERM", InvoiceDist = "BANK", InvoiceArTypes = "01"}
                },
                PaymentPlansDue = new List<PaymentPlansDue>() 
                {
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 1", PaymentPlanDueDates = DateTime.Today.AddDays(7), 
                    PaymentPlanOverdue = false, PaymentPlanCurrent = true, PaymentPlanIds = "1", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "2014/FA", PaymentPlanTermDescs = "2014 Fall Term"},
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 1", PaymentPlanDueDates = DateTime.Today.AddDays(14), 
                    PaymentPlanOverdue = false, PaymentPlanCurrent = true, PaymentPlanIds = "1", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "2014/FA", PaymentPlanTermDescs = "2014 Fall Term"},
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 2", PaymentPlanDueDates = DateTime.Today.AddDays(-14), 
                    PaymentPlanOverdue = true, PaymentPlanCurrent = false, PaymentPlanIds = "2", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "NON-TERM" },
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 2", PaymentPlanDueDates = DateTime.Today.AddDays(-7), 
                    PaymentPlanOverdue = true, PaymentPlanCurrent = false, PaymentPlanIds = "2", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "NON-TERM" }
                },
                PersonName = "John Smith",
                TermList = new List<string>() { "2014/FA", "2015/SP", "NON-TERM"}
            };
            emptyResponse = new StudentFinPaymentsDueAdminResponse()
            {
                PersonName = "John Smith"
            };
            transManagerMock.Setup<StudentFinPaymentsDueAdminResponse>(
                trans => trans.Execute<StudentFinPaymentsDueAdminRequest, StudentFinPaymentsDueAdminResponse>(It.IsAny<StudentFinPaymentsDueAdminRequest>()))
                    .Returns<StudentFinPaymentsDueAdminRequest>(request =>
                    {
                        if (request.PersonId == "0001234")
                        {
                            return validResponse;
                        }
                        return emptyResponse;
                    });
        }

        private void SetupExecutePaymentsDueByPeriodAdminCTX()
        {
            validResponse = new StudentFinPaymentsDueAdminResponse()
            {
                PaymentsDue = new List<PaymentsDue>() 
                {
                    new PaymentsDue() { ArTypeBals = 100m, ArTypeDescs = "Charge", ArTypeDist = "BANK", ArTypeDueDates = DateTime.Today, ArTypeOverdue = false,
                    ArTypes = "01", RelatedTerms = "2014/FA", RelatedTermDescs = "2014 Fall Term", Periods = FinanceTimeframeCodes.PastPeriod, PeriodDescs = "Past",
                    PeriodEndDates = DateTime.Today.AddDays(-31)},
                    new PaymentsDue() { Invoices = "1234", InvoiceBals = 100m, InvoiceDescs = "Invoice", InvoicesDueDate = DateTime.Today, InvoicesOverdue = false,
                    RelatedTerms = "2015/SP", RelatedTermDescs = "2015 Spring Term", InvoiceDist = "TRAV", InvoiceArTypes = "02", Periods = FinanceTimeframeCodes.CurrentPeriod, PeriodDescs = "Current",
                    PeriodBeginDates = DateTime.Today.AddDays(-30), PeriodEndDates = DateTime.Today.AddDays(30)},
                    new PaymentsDue() { InvoiceBals = 200m, InvoiceDescs = "Invoice", InvoicesDueDate = DateTime.Today.AddDays(-3), InvoicesOverdue = true,
                    RelatedTerms = "NON-TERM", InvoiceDist = "BANK", InvoiceArTypes = "01", Periods = FinanceTimeframeCodes.FuturePeriod, PeriodDescs = "Future", 
                    PeriodBeginDates = DateTime.Today.AddDays(31)},
                    new PaymentsDue() { InvoiceDescs = "Invoice", InvoicesDueDate = DateTime.Today.AddDays(-3), InvoicesOverdue = true,
                    RelatedTerms = "NON-TERM", InvoiceDist = "BANK", InvoiceArTypes = "01", Periods = FinanceTimeframeCodes.FuturePeriod, PeriodDescs = "Future", 
                    PeriodBeginDates = DateTime.Today.AddDays(31)}
                },
                PaymentPlansDue = new List<PaymentPlansDue>() 
                {
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 1", PaymentPlanDueDates = DateTime.Today.AddDays(7), 
                    PaymentPlanOverdue = false, PaymentPlanCurrent = true, PaymentPlanIds = "1", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "2014/FA", PaymentPlanTermDescs = "2014 Fall Term",  PaymentPlanPeriods = FinanceTimeframeCodes.PastPeriod, PaymentPlanPeriodDescs = "Past",
                    PaymentPlanPeriodEndDates = DateTime.Today.AddDays(-31)},
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 1", PaymentPlanDueDates = DateTime.Today.AddDays(14), 
                    PaymentPlanOverdue = false, PaymentPlanCurrent = true, PaymentPlanIds = "1", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "2014/FA", PaymentPlanTermDescs = "2014 Fall Term",  PaymentPlanPeriods = FinanceTimeframeCodes.CurrentPeriod, PaymentPlanPeriodDescs = "Current",
                    PaymentPlanPeriodBeginDates = DateTime.Today.AddDays(-30), PaymentPlanPeriodEndDates = DateTime.Today.AddDays(-30)},
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 2", PaymentPlanDueDates = DateTime.Today.AddDays(-14), 
                    PaymentPlanOverdue = true, PaymentPlanCurrent = false, PaymentPlanIds = "2", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "NON-TERM", PaymentPlanTermDescs = "Non-Term",  PaymentPlanPeriods = FinanceTimeframeCodes.FuturePeriod, PaymentPlanPeriodDescs = "Future",
                    PaymentPlanPeriodBeginDates = DateTime.Today.AddDays(31)},
                    new PaymentPlansDue() { PaymentPlanUnpaidAmts = 500m, PaymentPlanDescs = "Plan 2", PaymentPlanDueDates = DateTime.Today.AddDays(-7), 
                    PaymentPlanOverdue = true, PaymentPlanCurrent = false, PaymentPlanIds = "2", PaymentPlanArTypes = "01", PaymentPlanArTypeDescs = "Student Receivables",
                    PaymentPlanDist = "BANK", PaymentPlanTerms = "NON-TERM", PaymentPlanTermDescs = "Non-Term", PaymentPlanPeriods = FinanceTimeframeCodes.FuturePeriod, PaymentPlanPeriodDescs = "Future",
                    PaymentPlanPeriodBeginDates = DateTime.Today.AddDays(31)}
                },
                PersonName = "John Smith",
                TermList = null
            };
            emptyResponse = new StudentFinPaymentsDueAdminResponse()
            {
                PersonName = "John Smith"
            };
            transManagerMock.Setup<StudentFinPaymentsDueAdminResponse>(
                trans => trans.Execute<StudentFinPaymentsDueAdminRequest, StudentFinPaymentsDueAdminResponse>(It.IsAny<StudentFinPaymentsDueAdminRequest>()))
                    .Returns<StudentFinPaymentsDueAdminRequest>(request =>
                    {
                        if (request.PersonId == "0001234")
                        {
                            return validResponse;
                        }
                        return emptyResponse;
                    });
            transManagerMock.Setup<TxGetCurrentPeriodDatesResponse>(
                trans => trans.Execute<TxGetCurrentPeriodDatesRequest, TxGetCurrentPeriodDatesResponse>(It.IsAny<TxGetCurrentPeriodDatesRequest>()))
                    .Returns<TxGetCurrentPeriodDatesRequest>(request =>
                    {
                        return new TxGetCurrentPeriodDatesResponse()
                            {
                                OutCurrentPeriodStartDate = DateTime.Today.AddDays(-30),
                                OutCurrentPeriodEndDate = DateTime.Today.AddDays(30),
                            };
                    });
        }

        private void SetupExecuteCheckPayerInformationCTX()
        {
            validCheckResponse = new GetEcheckPayerResponse()
            {
               OutCity = "Fairfax",
               OutCountry = "USA",
               OutEmail = "john.smith@ellucianuniversity.edu",
               OutFirstName = "John",
               OutLastName = "Smith",
               OutMiddleName = "Patrick",
               OutPostalCode = "22033",
               OutState = "VA",
               OutStreet = "123 Main Street",
               OutTelephone = "(703) 123-4567"
            };
            emptyCheckResponse = new GetEcheckPayerResponse()
            {
            };
            transManagerMock.Setup<GetEcheckPayerResponse>(
                trans => trans.Execute<GetEcheckPayerRequest, GetEcheckPayerResponse>(It.IsAny<GetEcheckPayerRequest>()))
                    .Returns<GetEcheckPayerRequest>(request =>
                    {
                        if (request.InPersonId == "0001234")
                        {
                            return validCheckResponse;
                        }
                        return emptyCheckResponse;
                    });
        }

        private void SetupExecuteProcessCheckCTX()
        {
            validProcessResponse = new ProcessECheckResponse()
            {
                OutCashRcptsId = "1234"
            };
            errorProcessResponse = new ProcessECheckResponse()
            {
                OutErrorMsg = "Error occurred."
            };
            transManagerMock.Setup<ProcessECheckResponse>(
                trans => trans.Execute<ProcessECheckRequest, ProcessECheckResponse>(It.IsAny<ProcessECheckRequest>()))
                    .Returns<ProcessECheckRequest>(request =>
                    {
                        if (request.InPersonId == "0001234")
                        {
                            return validProcessResponse;
                        }
                        return errorProcessResponse;
                    });

        }

        #endregion
    }
}
