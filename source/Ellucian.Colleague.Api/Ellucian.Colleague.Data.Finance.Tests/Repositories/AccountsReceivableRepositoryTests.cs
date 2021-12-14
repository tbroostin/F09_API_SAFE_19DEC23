// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;
using System.Threading.Tasks;
using System.Threading;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Finance.Tests;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class AccountsReceivableRepositoryTests : BasePersonSetup
    {
        private AccountsReceivableRepository repository;
        private TestAccountsReceivableRepository expectedRepository;
        private readonly Collection<ArDeposits> arDeposits = TestArDepositsRepository.ArDeposits;
        private readonly Collection<ArDepositsDue> arDepositsDue = TestArDepositsDueRepository.ArDepositsDue;
        private readonly Collection<ArInvoices> arInvoices = TestArInvoicesRepository.ArInvoices;
        private readonly Collection<ArInvoiceItems> arInvoiceItems = TestArInvoiceItemsRepository.ArInvoiceItems;
        private readonly Collection<ArPayInvoiceItems> arPayInvoiceItems = TestArPayInvoiceItemsRepository.ArPayInvoiceItems;
        private readonly Collection<ArCodeTaxGlDistr> arCodeTaxGlDistrs = TestArCodeTaxGlDistrRepository.ArCodeTaxGlDistrs;
        private readonly Collection<ArPayments> arPayments = TestArPaymentsRepository.ArPayments;
        private readonly Collection<CashRcpts> cashRcpts = TestCashRcptsRepository.CashRcpts;
        private readonly Collection<ArCodes> arCodes = TestArCodesRepository.ArCodes;
        private readonly Collection<ArTypes> arTypes = TestArTypesRepository.ArTypes;
        private readonly Collection<ArDepositTypes> arDepositTypes = TestArDepositTypesRepository.ArDepositTypes;
        private readonly ApplValcodes invoiceTypes = TestInvoiceTypesRepository.InvoiceTypes;
        private readonly ApplValcodes externalSystems = TestExternalSystemsRepository.ExternalSystems;


        [TestInitialize]
        public void Initialize()
        {
            // Initialize person setup and Mock framework
            MockInitialize();

            // Build the test repository
            repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);

        }

        #region Get Account holder tests

        [TestClass]
        public class GetAccountHolder : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetAccountHolder_Initialize()
            {
                // Initialize person setup and Mock framework
                PersonSetupInitialize();
                SetupArDeposits();
                SetupArDepositsDue();
                // Build the test repository
                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetAccountHolder_NullId()
            {
                var result = Task.Run(async() => await repository.GetAccountHolderAsync(null)).GetAwaiter().GetResult();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetAccountHolder_EmptyId()
            {
                var result = Task.Run(async() => await repository.GetAccountHolderAsync(String.Empty)).GetAwaiter().GetResult();
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000001()
            {
                // 2 deposits due
                string personId = "0000001";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(2, result.DepositsDue.Count());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000002()
            {
                // 2 deposits due
                string personId = "0000002";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(2, result.DepositsDue.Count());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000003()
            {
                // 2 deposits due
                string personId = "0000003";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(2, result.DepositsDue.Count());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000004()
            {
                // 3 deposits due
                string personId = "0000004";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(3, result.DepositsDue.Count());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000005()
            {
                // 2 deposits due
                string personId = "0000005";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(2, result.DepositsDue.Count());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetAccountHolder_0000006()
            {
                // No deposits due
                string personId = "0000006";
                var result = Task.Run(async () => await repository.GetAccountHolderAsync(personId)).GetAwaiter().GetResult();

                Assert.AreEqual(personId, result.Id);
                Assert.AreEqual(0, result.DepositsDue.Count());
            }
        }

        #endregion

        #region Get Deposits Due tests

        [TestClass]
        public class GetDepositsDue : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetDepositsDue_Initialize()
            {
                PersonSetupInitialize();
                SetupArDeposits();
                SetupArDepositsDue();
                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDepositsDue_NullId()
            {
                IEnumerable<DepositDue> depositsDue = this.repository.GetDepositsDue(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDepositsDue_EmptyId()
            {
                IEnumerable<DepositDue> depositsDue = this.repository.GetDepositsDue(String.Empty);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000001()
            {
                // $5000 TUI deposit due, $4000 paid; $500 ROOM deposit due, $500 paid
                CheckDepositsDue("0000001");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000002()
            {
                // $5000 TUI deposit due, $5000 paid; $500 ROOM deposit due, $500 paid
                CheckDepositsDue("0000002");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000003()
            {
                // $5000 TUI deposit due, $2000 paid; $500 ROOM deposit due, $500 paid
                CheckDepositsDue("0000003");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000004()
            {
                // $5000 TUI deposit due, $5000 paid; $500 ROOM deposit due, $0 paid
                CheckDepositsDue("0000004");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000005()
            {
                // $5000 TUI deposit due, $500 paid; $500 ROOM deposit due, $0 paid
                CheckDepositsDue("0000005");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_0000006()
            {
                // No deposits due
                CheckDepositsDue("0000006");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_AllScenarios()
            {
                foreach (var personId in personIds)
                {
                    var depsDue = repository.GetDepositsDue(personId);
                    foreach (var result in depsDue)
                    {
                        string id = result.Id;
                        var source = arDepositsDue.FirstOrDefault(dd => dd.Recordkey == id);
                        var deposits = arDeposits.Where(d => d.ArdDepositsDue == id);
                        VerifyDepositsDue(source, deposits, result);
                    }
                }
            }
        }

        #endregion

        #region Get Deposit tests

        [TestClass]
        public class GetDeposit : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetDeposits_Initialize()
            {
                PersonSetupInitialize();
                SetupArDeposits();
                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDeposit_NullId()
            {
                var deposits = repository.GetDeposits(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDeposit_EmptyId()
            {
                var deposits = repository.GetDeposit(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetDeposit_BadId()
            {
                var deposits = repository.GetDeposit("BadId");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_Id()
            {
                var expected = arDeposits.Select(x => x.Recordkey).ToList();
                var result = expected.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Id).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_PersonId()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdPersonId).ToList();
                var result = ids.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.PersonId).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_DepositType()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdDepositType).ToList();
                var result = ids.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.DepositType).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_Date()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdDate.GetValueOrDefault()).ToList();
                var result = ids.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Date).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_Amount()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdAmt.GetValueOrDefault() - x.ArdReversalAmt.GetValueOrDefault()).ToList();
                var result = ids.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Amount).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposit_TermId()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdTerm).ToList();
                var result = ids.Select(x => repository.GetDeposit(x)).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.TermId).ToList());
            }
        }

        #endregion

        #region Get Deposits tests

        [TestClass]
        public class GetDeposits : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetDeposits_Initialize()
            {
                PersonSetupInitialize();
                SetupArDeposits();
                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDeposits_NullId()
            {
                var deposits = repository.GetDeposits(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDeposits_EmptyId()
            {
                var deposits = repository.GetDeposits(new List<string>());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_Id()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.Recordkey).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Id).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_PersonId()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdPersonId).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.PersonId).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_DepositType()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdDepositType).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.DepositType).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_Date()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdDate.GetValueOrDefault()).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Date).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_Amount()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdAmt.GetValueOrDefault() - x.ArdReversalAmt.GetValueOrDefault()).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.Amount).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDeposits_TermId()
            {
                var ids = arDeposits.Select(x => x.Recordkey).ToList();
                var expected = arDeposits.Select(x => x.ArdTerm).ToList();
                var result = repository.GetDeposits(ids).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result.Select(x => x.TermId).ToList());
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDepositsDue_AllScenarios()
            {
                foreach (var personId in personIds)
                {
                    var depsDue = repository.GetDepositsDue(personId);
                    foreach (var result in depsDue)
                    {
                        string id = result.Id;
                        var source = arDepositsDue.FirstOrDefault(dd => dd.Recordkey == id);
                        var deposits = arDeposits.Where(d => d.ArdDepositsDue == id);
                        VerifyDepositsDue(source, deposits, result);
                    }
                }
            }
        }

        #endregion

        #region Get Invoice tests

        [TestClass]
        public class GetInvoice : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetInvoice_Initialize()
            {
                Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetInvoice_NullId()
            {
                var invoice = repository.GetInvoice(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetInvoice_EmptyId()
            {
                var invoice = repository.GetInvoice(String.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetInvoice_InvalidId()
            {
                var invoice = repository.GetInvoice("InvalidId");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyId()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyPersonId()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvPersonId, result.PersonId, "Person IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyAccountTypeCode()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvArType, result.ReceivableTypeCode, "AR Types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyTermId()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvTerm, result.TermId, "Terms do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyReferenceNumber()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvNo, result.ReferenceNumber, "Numbers do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyDate()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvDate, result.Date, "Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyBillingStart()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvBillingStartDate, result.BillingStart, "Billing Period Start Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyBillingEnd()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvBillingEndDate, result.BillingEnd, "Billing Period End Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoice_VerifyDescription()
            {
                for (int i = 0; i < arInvoices.Count; i++)
                {
                    var source = arInvoices.ElementAt(i);
                    var result = repository.GetInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvDesc, result.Description, "Descriptions do not match for Invoice " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Invoices tests

        [TestClass]
        public class GetInvoices : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetInvoices_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetInvoices_NullIds()
            {
                var invoice = this.repository.GetInvoices(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetInvoices_NoIds()
            {
                var invoice = this.repository.GetInvoices(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetInvoices_InvalidId()
            {
                var invoice = this.repository.GetInvoices(new List<string>() { "InvalidId" });
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetInvoices_CountMismatch()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                ids.Add("Invalid");
                var invoice = this.repository.GetInvoices(ids);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyPersonIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvPersonId, result.PersonId, "Person IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyAccountTypeCodes()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvArType, result.ReceivableTypeCode, "AR Types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyTermIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvTerm, result.TermId, "Terms do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyReferenceNumbers()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvNo, result.ReferenceNumber, "Numbers do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyDates()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvDate, result.Date, "Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyBillingStarts()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvBillingStartDate, result.BillingStart, "Billing Period Start Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyBillingEnds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvBillingEndDate, result.BillingEnd, "Billing Period End Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetInvoices_VerifyDescriptions()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = this.repository.GetInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvDesc, result.Description, "Descriptions do not match for Invoice " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Query Invoices tests

        [TestClass]
        public class QueryInvoicePaymentsAsync : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void QueryInvoicePaymentsAsync_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_NullIds()
            {
                var invoice = await this.repository.QueryInvoicePaymentsAsync(null, InvoiceDataSubset.InvoiceOnly);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_NoIds()
            {
                var invoice = await this.repository.QueryInvoicePaymentsAsync(new List<string>(), InvoiceDataSubset.InvoiceOnly);
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_InvalidId()
            {
                var invoice = await this.repository.QueryInvoicePaymentsAsync(new List<string>() { "InvalidId" }, InvoiceDataSubset.InvoiceOnly);
                Assert.AreEqual(0, invoice.Count());
            }


            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_CountMismatch()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                ids.Add("Invalid");
                var invoice = await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly);
                Assert.AreEqual(ids.Count() - 1, invoice.Count());
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly);
                var invoicesList = invoices.ToList();
                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoicesList[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyPersonIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvPersonId, result.PersonId, "Person IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyAccountTypeCodes()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvArType, result.ReceivableTypeCode, "AR Types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyTermIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvTerm, result.TermId, "Terms do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyReferenceNumbers()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvNo, result.ReferenceNumber, "Numbers do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyDates()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvDate, result.Date, "Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyBillingStarts()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvBillingStartDate, result.BillingStart, "Billing Period Start Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyBillingEnds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvBillingEndDate, result.BillingEnd, "Billing Period End Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyDescriptions()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvDesc, result.Description, "Descriptions do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoiceAmount()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var items = arInvoiceItems.Where(ai => ai.InviInvoice == source.Recordkey);
                    var result = invoices[i];
                    if (items != null && items.Any())
                    {
                        Assert.AreEqual(items.Sum(aii => aii.InviExtChargeAmt), result.BaseAmount, "Invoice Base Amount does not match for Invoice " + source.Recordkey);
                    }
                    else
                    {
                        Assert.AreEqual(0, result.Amount);
                    }
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoiceItems()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvInvoiceItems.Count(), result.Charges.Count(), "Number of Invoice Items do not match for Invoice " + source.Recordkey);
                    if (source.InvInvoiceItems != null && source.InvInvoiceItems.Any())
                    {
                        foreach (var sourceItemId in source.InvInvoiceItems)
                        {
                            var sourceItem = this.arInvoiceItems.Where(aii => aii.Recordkey == sourceItemId).FirstOrDefault();
                            var resultItem = result.Charges.Where(c => c.Id == sourceItemId).FirstOrDefault();
                            Assert.AreEqual(sourceItem.InviArCode, resultItem.Code);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoiceItem_TaxAmount()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoiceOnly)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvInvoiceItems.Count(), result.Charges.Count(), "Number of Invoice Items do not match for Invoice " + source.Recordkey);
                    if (source.InvInvoiceItems != null && source.InvInvoiceItems.Any())
                    {
                        foreach (var sourceItemId in source.InvInvoiceItems)
                        {
                            var sourceItem = this.arInvoiceItems.Where(aii => aii.Recordkey == sourceItemId).FirstOrDefault();
                            var distrItem = this.arCodeTaxGlDistrs.Where(tg => tg.ArctdArInvoiceItem == sourceItem.Recordkey).FirstOrDefault();
                            var resultItem = result.Charges.Where(c => c.Id == sourceItemId).FirstOrDefault();
                            if (distrItem != null && distrItem.ArctdGlTaxAmt.HasValue)
                            {
                                Assert.AreEqual(distrItem.ArctdGlTaxAmt, resultItem.TaxAmount);
                            }

                        }
                    }
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoiceAmount_Paid()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoicePayment)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var items = arInvoiceItems.Where(ai => ai.InviInvoice == source.Recordkey);
                    var result = invoices[i];
                    if (items != null && items.Any())
                    {
                        Assert.AreEqual(items.Sum(aii => aii.InviExtChargeAmt), result.BaseAmount, "Invoice Base Amount does not match for Invoice " + source.Recordkey);
                    }
                    else
                    {
                        Assert.AreEqual(0, result.Amount);
                    }
                    Assert.AreEqual(0, result.AmountPaid);
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoiceItems_Paid()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoicePayment)).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = invoices[i];
                    Assert.AreEqual(source.InvInvoiceItems.Count(), result.Charges.Count(), "Number of Invoice Items do not match for Invoice " + source.Recordkey);
                    if (source.InvInvoiceItems != null && source.InvInvoiceItems.Any())
                    {
                        foreach (var sourceItemId in source.InvInvoiceItems)
                        {
                            var sourceItem = this.arInvoiceItems.Where(aii => aii.Recordkey == sourceItemId).FirstOrDefault();
                            var resultItem = result.Charges.Where(c => c.Id == sourceItemId).FirstOrDefault();
                            Assert.AreEqual(sourceItem.InviArCode, resultItem.Code);
                        }
                    }
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoicePaidAmount_Paid()
            {
                // ARRANGE
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                GetInvoicePaymentAmountsRequest ctxRequest = new GetInvoicePaymentAmountsRequest();
                ctxRequest.InvoiceIds = ids;
                GetInvoicePaymentAmountsResponse ctxResponse = new GetInvoicePaymentAmountsResponse();
                ctxResponse.InvoicePaymentItems = new List<InvoicePaymentItems>();
                foreach (var invoiceId in ids)
                {
                    // Get payment items for the particular invoice to build a response
                    var items = arInvoiceItems.Where(ai => ai.InviInvoice == invoiceId);
                    var amountCharged = items == null || !items.Any() ? 0 : items.Sum(ii => ii.InviExtChargeAmt);
                    var amountPaid = amountCharged > 10 ? amountCharged - 10 : 0;
                    var invoicePaymentItem = new InvoicePaymentItems() { InvoicePaymentId = invoiceId, InvoicePaymentAmount = amountPaid };
                    ctxResponse.InvoicePaymentItems.Add(invoicePaymentItem);
                }

                // Mock the Colleague TX response
                transManagerMock.Setup<Task<GetInvoicePaymentAmountsResponse>>(
                    trans => trans.ExecuteAsync<GetInvoicePaymentAmountsRequest, GetInvoicePaymentAmountsResponse>(It.IsAny<GetInvoicePaymentAmountsRequest>()))
                        .Returns<GetInvoicePaymentAmountsRequest>(request =>
                        {
                            return Task.FromResult(ctxResponse);
                        });

                // ACT
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoicePayment)).ToList();

                // ASSESS THE RESULTS
                for (int i = 0; i < ids.Count(); i++)
                {
                    // Get paid amounts
                    var source = arInvoices[i];
                    var invoicePaymentItem = ctxResponse.InvoicePaymentItems.Where(ip => ip.InvoicePaymentId == source.Recordkey).FirstOrDefault();
                    if (invoicePaymentItem != null)
                    {
                        var result = invoices[i];
                        Assert.AreEqual(invoicePaymentItem.InvoicePaymentAmount, result.AmountPaid, "Invoice Amount Paid does not match for Invoice " + source.Recordkey);
                    }
                }
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_QueryInvoicePaymentsAsync_VerifyInvoicePaidAmount_Paid_BalanceAmount()
            {
                // ARRANGE
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                GetInvoicePaymentAmountsRequest ctxRequest = new GetInvoicePaymentAmountsRequest();
                ctxRequest.InvoiceIds = ids;
                GetInvoicePaymentAmountsResponse ctxResponse = new GetInvoicePaymentAmountsResponse();
                ctxResponse.InvoicePaymentItems = new List<InvoicePaymentItems>();
                foreach (var invoiceId in ids)
                {
                    // Get payment items for the particular invoice to build a response
                    var items = arInvoiceItems.Where(ai => ai.InviInvoice == invoiceId);
                    var amountCharged = items == null || !items.Any() ? 0 : items.Sum(ii => ii.InviExtChargeAmt);
                    var amountPaid = amountCharged > 10 ? amountCharged - 10 : 0;
                    var balanceAmount = amountCharged - amountPaid;
                    var invoicePaymentItem = new InvoicePaymentItems() { InvoicePaymentId = invoiceId, InvoicePaymentAmount = amountPaid, AlInvoiceBalanceAmount=balanceAmount };
                    ctxResponse.InvoicePaymentItems.Add(invoicePaymentItem);
                }

                // Mock the Colleague TX response
                transManagerMock.Setup<Task<GetInvoicePaymentAmountsResponse>>(
                    trans => trans.ExecuteAsync<GetInvoicePaymentAmountsRequest, GetInvoicePaymentAmountsResponse>(It.IsAny<GetInvoicePaymentAmountsRequest>()))
                        .Returns<GetInvoicePaymentAmountsRequest>(request =>
                        {
                            return Task.FromResult(ctxResponse);
                        });

                // ACT
                var invoices = (await this.repository.QueryInvoicePaymentsAsync(ids, InvoiceDataSubset.InvoicePayment)).ToList();

                // ASSESS THE RESULTS
                for (int i = 0; i < ids.Count(); i++)
                {
                    // Get paid amounts
                    var source = arInvoices[i];
                    var invoicePaymentItem = ctxResponse.InvoicePaymentItems.Where(ip => ip.InvoicePaymentId == source.Recordkey).FirstOrDefault();
                    if (invoicePaymentItem != null)
                    {
                        var result = invoices[i];
                        Assert.AreEqual(invoicePaymentItem.InvoicePaymentAmount, result.AmountPaid, "Invoice Amount Paid does not match for Invoice " + source.Recordkey);
                        Assert.AreEqual(invoicePaymentItem.AlInvoiceBalanceAmount, result.BalanceAmount, "Invoice Balance Amount does not match for Invoice " + source.Recordkey);
                    }
                }
            }
        }

        #endregion

        #region Get Cash Receipt Payment tests

        [TestClass]
        public class GetPayment : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetPayment_Initialize()
            {
                base.Initialize();
                SetupArPayments();
                SetupCashReceipts();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetPayment_NullIds()
            {
                var invoice = this.repository.GetPayment(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetPayment_EmptyId()
            {
                var invoice = this.repository.GetPayment(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetPayment_InvalidId()
            {
                var invoice = this.repository.GetPayment("InvalidId");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyId()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyDate()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpDate, result.Date, "Dates do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyPersonId()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpPersonId, result.PersonId, "Person IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyReceivableType()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpArType, result.ReceivableType, "Receivable Types do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyTermId()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpTerm, result.TermId, "Term IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyReferenceNumber()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var crSource = this.cashRcpts.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(crSource.RcptNo, result.ReferenceNumber, "Reference Numbers do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayment_VerifyAmount()
            {
                for (int i = 0; i < this.arPayments.Count - 1; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpAmt - source.ArpReversalAmt, result.Amount, "Amounts do not match for Payment " + source.Recordkey);
                }
            }
        }
        #endregion

        #region Get Payments tests

        [TestClass]
        public class GetPayments : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetPayments_Initialize()
            {
                base.Initialize();
                SetupArPayments();
                SetupCashReceipts();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetPayments_NullIds()
            {
                var invoice = this.repository.GetPayments(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetPayment_NoIds()
            {
                var invoice = this.repository.GetPayments(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetPayments_InvalidId()
            {
                var invoice = this.repository.GetPayments(new List<string>() { "InvalidId" });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetPayments_CountMismatch()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.Add("Invalid");
                var invoice = this.repository.GetPayments(ids);
            }

            [TestMethod]
            [ExpectedException(typeof(NotImplementedException))]
            public void AccountsReceivableRepository_GetPayments_NonCashReceiptPayment()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                var pmts = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < this.arPayments.Count; i++)
                {
                    var source = this.arPayments.ElementAt(i);
                    var result = this.repository.GetPayment(source.Recordkey);
                    Assert.AreEqual(source.ArpAmt - source.ArpReversalAmt, result.Amount, "Amounts do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyIds()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyDates()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.ArpDate, result.Date, "Dates do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyPersonIds()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.ArpPersonId, result.PersonId, "Person IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyReceivableTypes()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.ArpArType, result.ReceivableType, "Receivable Types do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyTermIds()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.ArpTerm, result.TermId, "Term IDs do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyReferenceNumbers()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();
                var cashRcpts = this.cashRcpts.Select(x => x.RcptNo);

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var crSource = this.cashRcpts[i];
                    var result = payments[i];
                    Assert.AreEqual(crSource.RcptNo, result.ReferenceNumber, "Reference Numbers do not match for Payment " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetPayments_VerifyAmounts()
            {
                var ids = this.arPayments.Select(x => x.Recordkey).ToList();
                ids.RemoveAt(ids.Count - 1);
                var payments = this.repository.GetPayments(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arPayments[i];
                    var result = payments[i];
                    Assert.AreEqual(source.ArpAmt - source.ArpReversalAmt, result.Amount, "Amounts do not match for Payment " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Charge tests

        [TestClass]
        public class GetCharge : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetCharge_Initialize()
            {
                Initialize();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetCharge_NullId()
            {
                var invoice = repository.GetCharge(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetCharge_EmptyId()
            {
                var invoice = repository.GetCharge(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetCharge_InvalidId()
            {
                var invoice = repository.GetCharge("Invalid1");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharge_VerifyId()
            {
                foreach (var source in arInvoiceItems)
                {
                    var result = repository.GetCharge(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharge_VerifyInvoiceId()
            {
                foreach (var source in arInvoiceItems)
                {
                    var result = repository.GetCharge(source.Recordkey);
                    Assert.AreEqual(source.InviInvoice, result.InvoiceId, "Invoice IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharge_VerifyDescription()
            {
                foreach (var source in arInvoiceItems)
                {
                    var result = repository.GetCharge(source.Recordkey);
                    var inviDesc = new List<string>(source.InviDesc.Split(DmiString._VM));
                    CollectionAssert.AreEqual(inviDesc, result.Description.ToList(), "Descriptions do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharge_VerifyCode()
            {
                foreach (var source in arInvoiceItems)
                {
                    var result = repository.GetCharge(source.Recordkey);
                    Assert.AreEqual(source.InviArCode, result.Code, "AR Codes do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharge_VerifyBaseAmount()
            {
                foreach (var source in arInvoiceItems)
                {
                    var result = repository.GetCharge(source.Recordkey);
                    Assert.AreEqual(source.InviExtChargeAmt, result.BaseAmount, "Base Amounts do not match for Invoice Item " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Charges tests

        [TestClass]
        public class GetCharges : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetCharges_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetCharges_NullIds()
            {
                var invoice = this.repository.GetCharges(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetCharges_NoIds()
            {
                var invoice = this.repository.GetCharges(new List<String>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetCharges_InvalidIds()
            {
                List<String> invalidIds = new List<String>() { "Invalid" };
                var invoice = this.repository.GetCharges(invalidIds);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceivableRepository_GetCharges_CountMismatch()
            {
                // Get a random invoice from the test bed
                int idx = new Random().Next(this.arInvoices.Count);
                var arInvoice = this.arInvoices[idx];
                var ids = this.arInvoices.SelectMany(i => i.InvInvoiceItems).ToList();
                ids.Add("Invalid");
                var invoice = this.repository.GetCharges(ids);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetCharges_VerifyInvoiceCharges()
            {
                // Get a random invoice from the test bed
                int idx = new Random().Next(this.arInvoices.Count);
                var arInvoice = this.arInvoices[idx];
                var ids = arInvoice.InvInvoiceItems.ToList();
                var items = this.arInvoiceItems.Where(x => x.InviInvoice == arInvoice.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = items[i];
                    var result = charges[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetCharges_VerifyIds()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetCharges_VerifyInvoiceIds()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviInvoice, result.InvoiceId, "Invoice IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetCharges_VerifyDescriptions()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    var inviDesc = new List<string>(source.InviDesc.Split(DmiString._VM));
                    CollectionAssert.AreEqual(inviDesc, result.Description.ToList(), "Descriptions do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetCharges_VerifyCodes()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviArCode, result.Code, "AR Codes do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetCharges_VerifyBaseAmounts()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviExtChargeAmt, result.BaseAmount, "Base Amounts do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void AccountsReceiveableRepository_GetCharges_EmptyCollection()
            {
                dataReaderMock.Setup<Collection<ArInvoiceItems>>(
                    accessor => accessor.BulkReadRecord<ArInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .Returns<string[], bool>((ids, flag) =>
                        {
                            return new Collection<ArInvoiceItems>();
                        }
                    );
                var chargeIds = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(chargeIds).ToList();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceiveableRepository_GetCharges_NullCharge()
            {
                dataReaderMock.Setup<Collection<ArInvoiceItems>>(
                    accessor => accessor.BulkReadRecord<ArInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .Returns<string[], bool>((ids, flag) =>
                        {
                            var items = new Collection<ArInvoiceItems>();
                            foreach (var id in ids)
                            {
                                var invItem = this.arInvoiceItems.Where(x => x.Recordkey == id).FirstOrDefault();
                                if (invItem != null) items.Add(invItem);
                            }
                            items[0] = null;
                            return items;
                        }
                    );
                var chargeIds = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetCharges(chargeIds).ToList();
            }
        }

        #endregion

        #region Get Distribution(s) tests

        [TestClass]
        public class AccountsReceivableRepository_GetDistribution : AccountsReceivableRepositoryTests
        {
            string studentId;
            List<string> accountTypes;
            string callingProcess;
            TxDetermineGlDistributionRequest singleArTypeRequest;
            TxDetermineGlDistributionResponse singleArTypeResponse;
            TxDetermineGlDistributionRequest multiArTypeRequest;
            TxDetermineGlDistributionResponse multiArTypeResponse;

            [TestInitialize]
            public void AccountsReceivableRepository_GetDistribution_Initialize()
            {
                studentId = "0003315";
                accountTypes = new List<string>() { "01", "02" };
                callingProcess = "SFIPC";
                singleArTypeRequest = new TxDetermineGlDistributionRequest()
                {
                    InPersonId = studentId,
                    InArType = new List<string>() { accountTypes[0] },
                    InCallingProcess = callingProcess
                };
                singleArTypeResponse = new TxDetermineGlDistributionResponse()
                {
                    OutDistribution = new List<string>() { "BANK" }
                };
                multiArTypeRequest = new TxDetermineGlDistributionRequest()
                {
                    InPersonId = studentId,
                    InArType = accountTypes,
                    InCallingProcess = callingProcess
                };
                multiArTypeResponse = new TxDetermineGlDistributionResponse()
                {
                    OutDistribution = new List<string>() { "BANK", "TRAV" }
                };

                transManagerMock.Setup<TxDetermineGlDistributionResponse>(
                    trans => trans.Execute<TxDetermineGlDistributionRequest, TxDetermineGlDistributionResponse>(It.IsAny<TxDetermineGlDistributionRequest>()))
                        .Returns<TxDetermineGlDistributionRequest>(request =>
                        {
                            if (request.InArType != null && request.InArType.Count == 1)
                            {
                                return singleArTypeResponse;
                            }
                            else
                            {
                                return multiArTypeResponse;
                            }
                        });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDistribution_NullStudentId()
            {
                var dist = this.repository.GetDistribution(null, accountTypes[0], callingProcess);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetDistribution_EmptyStudentId()
            {
                var dist = this.repository.GetDistribution(string.Empty, accountTypes[0], callingProcess);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDistribution_VerifyDistribution()
            {
                var dist = this.repository.GetDistribution(studentId, accountTypes[0], callingProcess);
                Assert.AreEqual(singleArTypeResponse.OutDistribution[0], dist);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetDistributions_VerifyDistributions()
            {
                var dists = this.repository.GetDistributions(studentId, accountTypes, callingProcess).ToList();
                CollectionAssert.AreEqual(multiArTypeResponse.OutDistribution, dists);
            }
        }

        #endregion

        #region AR Deposits Due

        protected void SetupARDepositsDue()
        {
            dataReaderMock.Setup<Collection<ArDepositsDue>>(
                accessor => accessor.BulkReadRecord<ArDepositsDue>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((criteria, flag) =>
                    {
                        // Get the person ID from the selection criteria
                        string id = criteria.Split('\'')[1];
                        // Return the deposits due collection for the desired person
                        return new Collection<ArDepositsDue>(this.arDepositsDue.Where(depDue => depDue.ArddPersonId == id).ToList());
                    }
            );
        }

        #endregion

        #region Get Receivable Charge tests

        [TestClass]
        public class GetReceivableCharge : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetReceivableCharge_Initialize()
            {
                base.Initialize();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableCharge_NullId()
            {
                var invoice = this.repository.GetReceivableCharge(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableCharge_EmptyId()
            {
                var invoice = this.repository.GetReceivableCharge(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetReceivableCharge_InvalidId()
            {
                var invoice = this.repository.GetReceivableCharge("Invalid1");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharge_VerifyId()
            {
                foreach (var source in this.arInvoiceItems)
                {
                    var result = this.repository.GetCharge(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharge_VerifyInvoiceId()
            {
                foreach (var source in this.arInvoiceItems)
                {
                    var result = this.repository.GetReceivableCharge(source.Recordkey);
                    Assert.AreEqual(source.InviInvoice, result.InvoiceId, "Invoice IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharge_VerifyDescription()
            {
                foreach (var source in this.arInvoiceItems)
                {
                    var result = this.repository.GetReceivableCharge(source.Recordkey);
                    var inviDesc = new List<string>(source.InviDesc.Split(DmiString._VM));
                    CollectionAssert.AreEqual(inviDesc, result.Description.ToList(), "Descriptions do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharge_VerifyCode()
            {
                foreach (var source in this.arInvoiceItems)
                {
                    var result = this.repository.GetReceivableCharge(source.Recordkey);
                    Assert.AreEqual(source.InviArCode, result.Code, "AR Codes do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharge_VerifyBaseAmount()
            {
                foreach (var source in this.arInvoiceItems)
                {
                    var result = this.repository.GetReceivableCharge(source.Recordkey);
                    Assert.AreEqual(source.InviExtChargeAmt, result.BaseAmount, "Base Amounts do not match for Invoice Item " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Receivable Charges tests

        [TestClass]
        public class GetReceivableCharges : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetReceivableCharges_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableCharges_NullIds()
            {
                var invoice = this.repository.GetReceivableCharges(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableCharges_NoIds()
            {
                var invoice = this.repository.GetReceivableCharges(new List<String>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetReceivableCharges_InvalidIds()
            {
                List<String> invalidIds = new List<String>() { "2313", "Invalid" };
                var invoice = this.repository.GetReceivableCharges(invalidIds);
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableCharges_VerifyInvoiceReceivableCharges()
            {
                // Get a random invoice from the test bed
                int idx = new Random().Next(this.arInvoices.Count);
                var arInvoice = this.arInvoices[idx];
                var ids = arInvoice.InvInvoiceItems.ToList();
                var items = this.arInvoiceItems.Where(x => x.InviInvoice == arInvoice.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = items[i];
                    var result = charges[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetReceivableCharges_VerifyIds()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetReceivableCharges_VerifyInvoiceIds()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviInvoice, result.InvoiceId, "Invoice IDs do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetReceivableCharges_VerifyDescriptions()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    var inviDesc = new List<string>(source.InviDesc.Split(DmiString._VM));
                    CollectionAssert.AreEqual(inviDesc, result.Description.ToList(), "Descriptions do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetReceivableCharges_VerifyCodes()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviArCode, result.Code, "AR Codes do not match for Invoice Item " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceiveableRepository_GetReceivableCharges_VerifyBaseAmounts()
            {
                var ids = this.arInvoiceItems.Select(x => x.Recordkey).ToList();
                var charges = this.repository.GetReceivableCharges(ids).ToList();

                for (int i = 0; i < ids.Count; i++)
                {
                    var source = arInvoiceItems[i];
                    var result = charges[i];
                    Assert.AreEqual(source.InviExtChargeAmt, result.BaseAmount, "Base Amounts do not match for Invoice Item " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Get Receivable Invoice tests

        [TestClass]
        public class GetReceivableInvoice : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetReceivableInvoice_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableInvoice_NullId()
            {
                var invoice = this.repository.GetReceivableInvoice(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableInvoice_EmptyId()
            {
                var invoice = this.repository.GetReceivableInvoice(String.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetReceivableInvoice_InvalidId()
            {
                var invoice = this.repository.GetReceivableInvoice("InvalidId");
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyId()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyPersonId()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvPersonId, result.PersonId, "Person IDs do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyAccountTypeCode()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvArType, result.ReceivableType, "AR Types do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyTermId()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvTerm, result.TermId, "Terms do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyReferenceNumber()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvNo, result.ReferenceNumber, "Numbers do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyDate()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvDate, result.Date, "Dates do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyDueDate()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvDueDate, result.DueDate, "Due dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyBillingStart()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvBillingStartDate, result.BillingStart, "Billing Period Start Dates do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyBillingEnd()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvBillingEndDate, result.BillingEnd, "Billing Period End Dates do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyDescription()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvDesc, result.Description, "Descriptions do not match for Receivable Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyInvoiceType()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvType, result.InvoiceType, "Invoice types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyArchivedFlag()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(!String.IsNullOrEmpty(source.InvArchive), result.IsArchived, "Archived flags do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyAdjustedInvoice()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    Assert.AreEqual(source.InvAdjToInvoice, result.AdjustmentToInvoice, "Adjusted invoices do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoice_VerifyAdjustingInvoicesCount()
            {
                for (int i = 0; i < this.arInvoices.Count; i++)
                {
                    var source = this.arInvoices.ElementAt(i);
                    var result = this.repository.GetReceivableInvoice(source.Recordkey);
                    if (source.InvAdjByInvoices != null && result.AdjustedByInvoices != null)
                    {
                        Assert.AreEqual(source.InvAdjByInvoices.Count(), result.AdjustedByInvoices.Count());
                    }
                }
            }
        }

        #endregion

        #region Get Receivable Invoices tests

        [TestClass]
        public class GetReceivableInvoices : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetReceivableInvoices_Initialize()
            {
                base.Initialize();
                SetupArInvoices();
                SetupArInvoiceItems();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableInvoices_NullIds()
            {
                var receivableInvoice = this.repository.GetReceivableInvoices(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountsReceivableRepository_GetReceivableInvoices_NoIds()
            {
                var receivableInvoice = this.repository.GetReceivableInvoices(new List<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetReceivableInvoices_InvalidId()
            {
                var receivableInvoice = this.repository.GetReceivableInvoices(new List<string>() { "InvalidId" });
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void AccountsReceivableRepository_GetReceivableInvoices_CountMismatch()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey).ToList();
                ids.Add("InvalidId");
                var receivableInvoice = this.repository.GetReceivableInvoices(new List<string>() { "InvalidId" });
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyPersonIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvPersonId, result.PersonId, "Person IDs do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyAccountTypeCodes()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvArType, result.ReceivableType, "AR Types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyTermIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvTerm, result.TermId, "Terms do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyReferenceNumbers()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvNo, result.ReferenceNumber, "Numbers do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyDates()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvDate, result.Date, "Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyDueDates()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvDueDate, result.DueDate, "Due dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyBillingStarts()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvBillingStartDate, result.BillingStart, "Billing Period Start Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyBillingEnds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvBillingEndDate, result.BillingEnd, "Billing Period End Dates do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyDescriptions()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvDesc, result.Description, "Descriptions do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyInvoiceTypes()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvType, result.InvoiceType, "Invoice types do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyArchivedFlag()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(!String.IsNullOrEmpty(source.InvArchive), result.IsArchived, "Archived flags do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyAdjustedinvoice()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    Assert.AreEqual(source.InvAdjToInvoice, result.AdjustmentToInvoice, "Adjusted invoices do not match for Invoice " + source.Recordkey);
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyAdjustingInvoicesCount()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];
                    if (source.InvAdjByInvoices != null && result.AdjustedByInvoices != null)
                    {
                        Assert.AreEqual(source.InvAdjByInvoices.Count(), result.AdjustedByInvoices.Count(), "Adjusting invoices do not match for Invoice " + source.Recordkey);
                    }
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_VerifyAdjustingInvoicesIds()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];

                    if (source.InvAdjByInvoices != null && result.AdjustedByInvoices != null)
                    {
                        for (int j = 0; j < source.InvAdjByInvoices.Count(); j++)
                        {
                            var sourceAdjByInvoice = source.InvAdjByInvoices[j];
                            Assert.IsTrue(result.AdjustedByInvoices.Contains(sourceAdjByInvoice), "Adjusting invoices do not match for Invoice " + source.Recordkey);
                        }
                    }
                }
            }

            [TestMethod]
            public void AccountsReceivableRepository_GetReceivableInvoices_ReceivableCharges_VerifyReceivableChargesCount()
            {
                var ids = this.arInvoices.Select(x => x.Recordkey);
                var receivableInvoices = this.repository.GetReceivableInvoices(ids).ToList();

                for (int i = 0; i < ids.Count(); i++)
                {
                    var source = arInvoices[i];
                    var result = receivableInvoices[i];

                    if (source.InvInvoiceItems != null && result.Charges != null)
                    {
                        Assert.AreEqual(source.InvInvoiceItems.Count(), result.Charges.Count(), "Receivable charges do not match for Invoice " + source.Recordkey);
                    }
                }
            }
        }

        #endregion

        #region Search Accountholders tests

        [TestClass]
        public class AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_Tests : AccountsReceivableRepositoryTests
        {
            GetPersonLookupStringRequest lookupStringRequest = new GetPersonLookupStringRequest();
            private string lastName;

            [TestInitialize]
            public void AccountActivityRepository_SearchAccountHoldersAsync_Tests_Initialize()
            {
                base.Initialize();
                expectedRepository = new TestAccountsReceivableRepository();

                
                
                // This statement mocks the search string that would be returned by the transaction manager, based on the input
                transManagerMock.Setup(
                    trans => trans.ExecuteAsync<GetPersonLookupStringRequest, GetPersonLookupStringResponse>(It.IsAny<GetPersonLookupStringRequest>()))
                    .Returns<GetPersonLookupStringRequest>(
                        (request) =>
                        {
                            lastName = request.SearchString.Split(',')[0];
                            return Task.FromResult(new GetPersonLookupStringResponse() { IndexString = ";PARTIAL.NAME.INDEX " + lastName.ToUpper() + "_", ErrorMessage = "" });
                        });
                // This mocks the select that would use the string that was returned by the call to the transaction manager, above.
                // It contains an extra id that will be filtered out by next mock, which simulates a call to PERSON.AR
                dataReaderMock.Setup(dr => dr.SelectAsync("PERSON", It.IsAny<string>()))
                    .Returns<string, string>(
                        (fileName, searchString) => {
                        int ndx = searchString.IndexOf("EQ \"");
                        lastName = searchString.Substring(ndx + 4).Replace("_", "").Replace("\"", "");
                        return Task.FromResult(expectedRepository.personData.Where(p => p.lastName.ToLower().Contains(lastName.ToLower())).Select(p => p.id).ToArray());
                });
                // This mocks the select that would return the ids from above that exist in PERSON.AR.  It 'strips out' the third id.
                dataReaderMock.Setup(dr => dr.SelectAsync("PERSON.AR", It.IsAny<string[]>(), It.IsAny<string>()))
                    .ReturnsAsync(new string[]
                    {
                        "0000001",
                        "0000002"
                    });

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Person>("PERSON", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, string, bool>((file, id, b) =>
                    {
                        var contract = expectedRepository.personData.Where(p => id == p.id).First();
                        return Task.FromResult(new Data.Base.DataContracts.Person()
                        {
                            Recordkey = contract.id,
                            LastName = contract.lastName,
                            FirstName = contract.firstName,
                            PrivacyFlag = contract.privacyCode
                        });

                    });
                
                
                // This mocks the filtering by corporations, defaulting to no IDs being removed
                dataReaderMock.Setup(dr => dr.SelectAsync("PERSON", It.IsAny<string[]>(), "WITH PERSON.CORP.INDICATOR NE 'Y'"))
                    .Returns((string file, string[] ids, string criteria) => Task.FromResult(ids));
                dataReaderMock.Setup<Task<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>>(a =>
                    a.ReadRecordAsync<Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy>("NAME.ADDR.HIERARCHY", "PREFERRED", true))
                    .ReturnsAsync(new Ellucian.Colleague.Data.Base.DataContracts.NameAddrHierarchy()
                    {
                        Recordkey = "PREFERRED",
                        NahNameHierarchy = new List<string>() { "PF" }
                    });

                dataReaderMock.Setup<Task<Base.DataContracts.Dflts>>(dr => dr.ReadRecordAsync<Base.DataContracts.Dflts>("CORE.PARMS", "DEFAULTS", true))
               .Returns<string, string, bool>((file, key, b) =>
               {
                   return Task.FromResult(new Base.DataContracts.Dflts() { DfltsFixedLenPerson = "7" });
               });

                // Needed to for GetOrAddToCacheAsync 
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                    null,
                    new SemaphoreSlim(1, 1)
                )));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_NullCriteria()
            {
                var result = await repository.SearchAccountHoldersByKeywordAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_EmptyCriteria()
            {
                var result = await repository.SearchAccountHoldersByKeywordAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_NoFirstName()
            {
                var result = await repository.SearchAccountHoldersByKeywordAsync("Smith");
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_PaddedId()
            {
                var result = (await repository.SearchAccountHoldersByKeywordAsync("0000001")).ToList();
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("Smith", result[0].LastName);
                Assert.AreEqual("Joe", result[0].FirstName);
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_UnpaddedId()
            {
                var result = (await repository.SearchAccountHoldersByKeywordAsync("1")).ToList();
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual("Smith", result[0].LastName);
                Assert.AreEqual("Joe", result[0].FirstName);
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_MultipleNames()
            {
                var result = (await repository.SearchAccountHoldersByKeywordAsync("Smith, J")).ToList();
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.Count());
                Assert.AreEqual("Smith", result[0].LastName);
                Assert.AreEqual("Joe", result[0].FirstName);
                Assert.AreEqual("Smith", result[1].LastName);
                Assert.AreEqual("John", result[1].FirstName);
                Assert.AreEqual("Smithson", result[2].LastName);
                Assert.AreEqual("Jane", result[2].FirstName);
            }

            [TestMethod]
            public async Task AccountsReceivableRepository_SearchAccountHoldersByKeywordAsync_MultipleNames_FilterOutCorps()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("PERSON", It.IsAny<string[]>(), "WITH PERSON.CORP.INDICATOR NE 'Y'"))
                    .ReturnsAsync(new string[]
                    {
                        "0000001",
                        "0000002"
                    });

                var result = (await repository.SearchAccountHoldersByKeywordAsync("Smith, J")).ToList();
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("Smith", result[0].LastName);
                Assert.AreEqual("Joe", result[0].FirstName);
                Assert.AreEqual("Smith", result[1].LastName);
                Assert.AreEqual("John", result[1].FirstName);
            }

            [TestMethod]
            public async Task LastNameWithSpecialCharacter_SearchFinancialAidPersonsByKeywordAsync_ReturnsExpectedResultTest()
            {
                var result = await repository.SearchAccountHoldersByKeywordAsync("O'Hruska, N");
                Assert.IsTrue(result.Any());
                Assert.AreEqual("0002345", result.First().Id);
            }

            [TestMethod]
            public async Task IdLengthLongerThanDefault_SearchFinancialAidPersonsByKeywordAsync_ReturnsExpectedResultTest()
            {
                var actualPersons = await repository.SearchAccountHoldersByKeywordAsync("123456789");
                Assert.IsTrue(actualPersons.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AlphaNumericId_SearchFinancialAidPersonsByKeywordAsync_ThrowsExceptionTest()
            {
                await repository.SearchAccountHoldersByKeywordAsync("AB12345");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task IdWithSpecialCharacters_SearchFinancialAidPersonsByKeywordAsync_ThrowsExceptionTest()
            {
                await repository.SearchAccountHoldersByKeywordAsync("00_12345");
            }

            [TestMethod]
            public async Task NoDefaultIdLength_SearchFinancialAidPersonsByKeywordAsync_ReturnsExpectedResultTest()
            {
                dataReaderMock.Setup<Task<Base.DataContracts.Dflts>>(dr => dr.ReadRecordAsync<Base.DataContracts.Dflts>("CORE.PARMS", "DEFAULTS", true))
                   .Returns<string, string, bool>((file, key, b) =>
                   {
                       return Task.FromResult(new Base.DataContracts.Dflts() { DfltsFixedLenPerson = "" });
                   });
                var actualPersons = await repository.SearchAccountHoldersByKeywordAsync("123456789");
                var expectedPerson = expectedRepository.personData.Where(p => p.id == "123456789").First();
                Assert.IsTrue(actualPersons.Any());
                Assert.AreEqual(expectedPerson.id, actualPersons.First().Id);
                Assert.AreEqual(expectedPerson.lastName, actualPersons.First().LastName);
                Assert.AreEqual(expectedPerson.firstName, actualPersons.First().FirstName);
            }
        }

        [TestClass]
        public class AccountsReceivableRepository_SearchAccountHoldersByIdsAsyncTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void TestInitialize()
            {
                base.Initialize();

                //To read person records with specified ids
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Person>("PERSON", It.IsAny<string[]>(), true))
                    .Returns<string, string[], bool>((x, y, z) => Task.FromResult(new Collection<Person>()
                {
                    new Person(){
                        Recordkey = "0000001",
                        LastName = "Smith",
                        FirstName = "John",
                        MiddleName = "Jacob Jingleheimer"
                    },
                    new Person(){
                        Recordkey = "0000002",
                        LastName = "Doe",
                        FirstName = "John",
                        MiddleName = "None"
                    },
                    new Person(){
                        Recordkey = "0000003",
                        LastName = "Corporation",
                        FirstName = "XYZ",
                        MiddleName = "None"
                    }
                }));

                // Mocking out the corporation filtering to return the values that are not corps for given inputs
                dataReaderMock.Setup(dr => dr.SelectAsync("PERSON", It.IsAny<string[]>(), "WITH PERSON.CORP.INDICATOR NE 'Y'"))
                    .Returns((string file, string[] ids, string criteria) => Task.FromResult(ids.Where(id => id != "0000003").ToArray()));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullIds_SearchAccountHoldersByIdsAsync_ThrowsArgumentNullExceptionTest()
            {
                await repository.SearchAccountHoldersByIdsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EmptyIdsList_SearchAccountHoldersByIdsAsync_ThrowsArgumentNullExceptionTest()
            {
                await repository.SearchAccountHoldersByIdsAsync(new List<string>());
            }

            [TestMethod]
            public async Task ThreeIdsPassedIn_SearchAccountHoldersByIdsAsync_ReturnsExpectedNumberOfHoldersTest()
            {
                var acctHolders = await repository.SearchAccountHoldersByIdsAsync(new List<string>() { "0000001", "0000002", "0000003" });
                Assert.IsTrue(acctHolders.Count() == 2);
            }

            [TestMethod]
            public async Task TwoIdsPassedIn_SearchAccountHoldersByIdsAsync_ReturnsExpectedNumberOfHoldersTest()
            {
                var acctHolders = await repository.SearchAccountHoldersByIdsAsync(new List<string>() { "0000001", "0000002" });
                Assert.IsTrue(acctHolders.Count() == 2);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task OneCorpIdPassedIn_SearchAccountHoldersByIdsAsync_ThrowsApplicationExceptionTest()
            {
                await repository.SearchAccountHoldersByIdsAsync(new List<string>() { "0000003" });
            }
        }
        #endregion

        #region AR Types tests

        [TestClass]
        public class ArTypesTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void ArTypesTests_Initialize()
            {
                Initialize();
                SetupArTypes();
            }

            [TestMethod]
            public void ArTypesTests_Get()
            {
                var result = repository.ReceivableTypes.ToList();
                Assert.AreEqual(arTypes.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ReceivableType));
            }

            [TestMethod]
            public void ArTypesTests_Constructor_ValidCode()
            {
                var expected = arTypes.Select(x => x.Recordkey).ToList();
                var result = repository.ReceivableTypes.Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public void ArTypesTests_Constructor_ValidDescription()
            {
                var expected = arTypes.Select(x => x.ArtDesc).ToList();
                var result = repository.ReceivableTypes.Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }

        #endregion

        #region AR Deposit Types tests

        [TestClass]
        public class ArDepositTypesTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void ArDepositTypesTests_Initialize()
            {
                Initialize();
                SetupArDepositTypes();
            }

            [TestMethod]
            public void ArDepositTypesTests_Get()
            {
                var result = repository.DepositTypes.ToList();
                Assert.AreEqual(arDepositTypes.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(DepositType));
            }

            [TestMethod]
            public void ArDepositTypesTests_Constructor_ValidCode()
            {
                var expected = arDepositTypes.Select(x => x.Recordkey).ToList();
                var result = repository.DepositTypes.Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public void ArDepositTypesTests_Constructor_ValidDescription()
            {
                var expected = arDepositTypes.Select(x => x.ArdtDesc).ToList();
                var result = repository.DepositTypes.Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }

        #endregion

        #region AR Codes tests

        [TestClass]
        public class ArCodesTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void ArCodesTests_Initialize()
            {
                Initialize();
                SetupArCodes();
            }

            [TestMethod]
            public void ArCodesTests_Get()
            {
                var result = repository.ChargeCodes.ToList();
                Assert.AreEqual(arCodes.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ChargeCode));
            }

            [TestMethod]
            public void ArCodesTests_Constructor_ValidCode()
            {
                var expected = arCodes.Select(x => x.Recordkey).ToList();
                var result = repository.ChargeCodes.Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public void ArCodesTests_Constructor_ValidDescription()
            {
                var expected = arCodes.Select(x => x.ArcDesc).ToList();
                var result = repository.ChargeCodes.Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }

        [TestClass]
        public class GetChargeCodesAsyncTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void GetChargeCodesAsyncTests_Initialize()
            {
                Initialize();
                SetupArCodes();
            }

            [TestMethod]
            public async Task GetChargeCodesAsyncTests_Get()
            {
                var result = (await repository.GetChargeCodesAsync()).ToList();
                Assert.AreEqual(arCodes.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ChargeCode));
            }

            [TestMethod]
            public async Task GetChargeCodesAsyncTests_Constructor_ValidCode()
            {
                var expected = arCodes.Select(x => x.Recordkey).ToList();
                var result = (await repository.GetChargeCodesAsync()).Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public async Task GetChargeCodesAsyncTests_Constructor_ValidDescription()
            {
                var expected = arCodes.Select(x => x.ArcDesc).ToList();
                var result = (await repository.GetChargeCodesAsync()).Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }


        #endregion

        #region Invoice Types tests

        [TestClass]
        public class InvoiceTypesTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void InvoiceTypesTests_Initialize()
            {
                Initialize();
                SetupInvoiceTypes();
            }

            [TestMethod]
            public void InvoiceTypesTests_Get()
            {
                var result = repository.InvoiceTypes.ToList();
                Assert.AreEqual(invoiceTypes.ValsEntityAssociation.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(InvoiceType));
            }

            [TestMethod]
            public void InvoiceTypesTests_Constructor_ValidCode()
            {
                var expected = invoiceTypes.ValsEntityAssociation.Select(x => x.ValInternalCodeAssocMember).ToList();
                var result = repository.InvoiceTypes.Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public void InvoiceTypesTests_Constructor_ValidDescription()
            {
                var expected = invoiceTypes.ValsEntityAssociation.Select(x => x.ValExternalRepresentationAssocMember).ToList();
                var result = repository.InvoiceTypes.Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }

        #endregion

        #region External Systems tests

        [TestClass]
        public class ExternalSystemsTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void ExternalSystemsTests_Initialize()
            {
                Initialize();
                SetupExternalSystems();
            }

            [TestMethod]
            public void ExternalSystemsTests_Get()
            {
                var result = repository.ExternalSystems.ToList();
                Assert.AreEqual(externalSystems.ValsEntityAssociation.Count, result.Count);
                CollectionAssert.AllItemsAreInstancesOfType(result, typeof(ExternalSystem));
            }

            [TestMethod]
            public void ExternalSystemsTests_Constructor_ValidCode()
            {
                var expected = externalSystems.ValsEntityAssociation.Select(x => x.ValInternalCodeAssocMember).ToList();
                var result = repository.ExternalSystems.Select(x => x.Code).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }

            [TestMethod]
            public void ExternalSystemsTests_Constructor_ValidDescription()
            {
                var expected = externalSystems.ValsEntityAssociation.Select(x => x.ValExternalRepresentationAssocMember).ToList();
                var result = repository.ExternalSystems.Select(x => x.Description).ToList();
                Assert.AreEqual(expected.Count, result.Count);
                CollectionAssert.AreEqual(expected, result);
            }
        }


        #endregion

        #region Post Receivable Invoice tests

        [TestClass]
        public class PostReceivableInvoiceTests : AccountsReceivableRepositoryTests
        {
            [TestInitialize]
            public void PostReceivableInvoiceTests_Initialize()
            {
                Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PostReceivableInvoiceTests_NullSource()
            {
                var result = repository.PostReceivableInvoice(null);
            }

            [TestMethod]
            public void PostReceivableInvoiceTests_Valid()
            {
                transManagerMock.Setup<CreateReceivableInvoiceResponse>(
                    trans => trans.Execute<CreateReceivableInvoiceRequest, CreateReceivableInvoiceResponse>(It.IsAny<CreateReceivableInvoiceRequest>()))
                        .Returns<CreateReceivableInvoiceRequest>(request =>
                        {
                            return new CreateReceivableInvoiceResponse()
                            {
                                InvoiceId = "123"
                            };
                        });
                dataReaderMock.Setup(dr => dr.ReadRecord<ArInvoices>(It.IsAny<string>(), It.IsAny<bool>())).Returns(new ArInvoices()
                {
                    InvArchive = "N",
                    InvArPostedFlag = "Y",
                    InvArType = "01",
                    InvBillingEndDate = DateTime.Today.AddDays(30),
                    InvBillingStartDate = DateTime.Today.AddDays(-30),
                    InvChargeAmt = 300m,
                    InvDate = DateTime.Today,
                    InvDesc = "123DESC",
                    InvDueDate = DateTime.Today.AddDays(7),
                    InvExternalId = "EXT123",
                    InvExternalSystem = "EXTSYS",
                    InvInvoiceItems = new List<string>() { "124" },
                    InvLocation = "MC",
                    InvNo = "INV123",
                    InvPersonId = "0001234",
                    InvReferenceNos = new List<string>() { "REF123" },
                    InvTerm = "2014/FA",
                    InvType = "MISC",
                    Recordkey = "123"
                });
                dataReaderMock.Setup(dr => dr.BulkReadRecord<ArInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>())).Returns(new Collection<ArInvoiceItems>()
                {
                    new ArInvoiceItems()
                    {
                        InviArCode = "CHG",
                        InviDesc = "CHG124DESC",
                        InviExtChargeAmt = 100m,
                        InviInvoice = "123",
                        Recordkey = "124",
                    }
                });

                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
                var result = repository.PostReceivableInvoice(new ReceivableInvoice("123", "REF123", "0001234", "01", "2014/FA", DateTime.Today,
                    DateTime.Today.AddDays(7), DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "123DESC", new List<ReceivableCharge>()
                    {
                        new ReceivableCharge("124", "123", new List<string>() { "CHG124DESC" }, "CHG", 100m),
                    }));
                Assert.AreEqual("123", result.Id);
                Assert.AreEqual(1, result.Charges.Count);
                Assert.AreEqual(100m, result.Amount);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PostReceivableInvoiceTests_Error()
            {
                transManagerMock.Setup<CreateReceivableInvoiceResponse>(
                    trans => trans.Execute<CreateReceivableInvoiceRequest, CreateReceivableInvoiceResponse>(It.IsAny<CreateReceivableInvoiceRequest>()))
                        .Returns<CreateReceivableInvoiceRequest>(request =>
                        {
                            return new CreateReceivableInvoiceResponse()
                            {
                                ErrorInd = "Y",
                                ErrorMessage = new List<string>() { "Error occurred." }
                            };
                        });
                repository = new AccountsReceivableRepository(cacheProvider, transFactory, logger, apiSettings);
                var result = repository.PostReceivableInvoice(new ReceivableInvoice("123", "REF123", "0001234", "01", "2014/FA", DateTime.Today,
                    DateTime.Today.AddDays(7), DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30), "123DESC", new List<ReceivableCharge>()
                    {
                        new ReceivableCharge("124", "123", new List<string>() { "CHG124DESC" }, "CHG", 100m),
                    }));
            }
        }

        #endregion

        #region Private verification methods

        private void CheckDepositsDue(string personId)
        {
            ArDepositsDue source;
            IEnumerable<ArDeposits> deposits;
            DepositDue result;
            IEnumerable<DepositDue> depositsDue;

            depositsDue = repository.GetDepositsDue(personId);

            // TUI deposit due
            var test = this.arDepositsDue.Where(x => x.ArddPersonId == personId && x.ArddDepositType == "TUI");
            source = this.arDepositsDue.Where(x => x.ArddPersonId == personId && x.ArddDepositType == "TUI").SingleOrDefault();
            deposits = this.arDeposits.Where(x => x.ArdDepositsDue == source.Recordkey);
            result = depositsDue.Where(x => x.DepositType == "TUI").SingleOrDefault();
            VerifyDepositsDue(source, deposits, result);

            // ROOM deposit due
            source = this.arDepositsDue.Where(x => x.ArddPersonId == personId && x.ArddDepositType == "ROOM").SingleOrDefault();
            deposits = this.arDeposits.Where(x => x.ArdDepositsDue == source.Recordkey);
            result = depositsDue.Where(x => x.DepositType == "ROOM").SingleOrDefault();
            VerifyDepositsDue(source, deposits, result);
        }

        private void VerifyDepositsDue(ArDepositsDue source, IEnumerable<ArDeposits> deposits, DepositDue result)
        {
            // If both source and result are null, then there's nothing else to do
            if ((source == null) && (result == null))
            {
                return;
            }
            Assert.AreEqual(result.Id, source.Recordkey, "IDs do not match for deposit due " + source.Recordkey);
            Assert.AreEqual(result.PersonId, source.ArddPersonId, "Person IDs do not match for deposit due " + source.Recordkey);
            Assert.AreEqual(result.DepositType, source.ArddDepositType, "Deposit types do not match for deposit due " + source.Recordkey);
            Assert.AreEqual(result.TermId, source.ArddTerm, "Terms do not match for deposit due " + source.Recordkey);
            Assert.AreEqual(result.DueDate, source.ArddDueDate, "Due dates do not match for deposit due " + source.Recordkey);
            Assert.AreEqual(result.Amount, source.ArddAmount, "Amounts do not match for deposit due " + source.Recordkey);

            decimal payment = (deposits == null) ? 0m : deposits.Sum(d => (d.ArdAmt.GetValueOrDefault(0) - d.ArdReversalAmt.GetValueOrDefault(0)));
            Assert.AreEqual(result.Balance, (source.ArddAmount.GetValueOrDefault(0) - payment), "Balances do not match for deposit due " + source.Recordkey);
        }

        #endregion

        #region Private data definition setup

        #region AR Invoices

        protected void SetupArInvoices()
        {
            MockRecords("AR.INVOICES", arInvoices);
        }

        #endregion

        #region AR Invoice Items

        protected void SetupArInvoiceItems()
        {
            MockRecords("AR.INVOICE.ITEMS", arInvoiceItems);
            MockRecords("AR.CODE.TAX.GL.DISTR", arCodeTaxGlDistrs);
        }

        #endregion

        #region AR Payments

        protected void SetupArPayments()
        {
            MockRecords("AR.PAYMENTS", arPayments);
        }

        #endregion

        #region Cash Receipts

        protected void SetupCashReceipts()
        {
            MockRecords("CASH.RCPTS", cashRcpts);
        }

        #endregion

        #region AR Deposits

        protected void SetupArDeposits()
        {
            MockRecords("AR.DEPOSITS", arDeposits, (criteria, records) =>
            {
                IEnumerable<ArDeposits> deposits = null;
                if (!string.IsNullOrEmpty(criteria))
                {
                    // Split the criteria up so we can look at its separate parts
                    string[] criteriaArray = criteria.Split('\'');
                    // What gets returned depends on the field in the selection criteria
                    string selection = criteriaArray[0].Split(' ')[1];
                    // Get the selection ID from the selection criteria
                    string id = criteria.Split('\'')[1];
                    // Get and return the deposits collection for the desired selection
                    if (selection == "ARD.PERSON.ID")
                    {
                        deposits = records.Where(dep => dep.ArdPersonId == id);
                    }
                    else if (selection == "ARD.DEPOSITS.DUE")
                    {
                        deposits = records.Where(dep => dep.ArdDepositsDue == id);
                    }
                }
                return deposits == null ? new Collection<ArDeposits>() : new Collection<ArDeposits>(deposits.ToList());
            }
            );
        }

        #endregion

        #region AR Deposits Due

        protected void SetupArDepositsDue()
        {
            MockRecords("AR.DEPOSITS.DUE", arDepositsDue, (criteria, records) =>
            {
                IEnumerable<ArDepositsDue> depositsDue = null;
                if (!string.IsNullOrEmpty(criteria))
                {
                    // Get the person ID from the selection criteria
                    var ids = criteria.Split('\'');
                    string id = ids.Length > 1 ? ids[1] : null;
                    // Return the deposits due collection for the desired person
                    depositsDue = string.IsNullOrEmpty(id) ? null : records.Where(depDue => depDue.ArddPersonId == id);
                }
                return depositsDue == null ? new List<ArDepositsDue>() : depositsDue.ToList();
            }
            );
        }

        #endregion

        #region AR Types

        protected void SetupArTypes()
        {
            MockRecords("AR.TYPES", arTypes);
        }

        #endregion

        #region AR Codes

        protected void SetupArCodes()
        {
            MockRecords("AR.CODES", arCodes);
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<ArCodes>("AR.CODES", "", true))
                .ReturnsAsync(arCodes);
        }

        #endregion

        #region AR Deposit Types

        protected void SetupArDepositTypes()
        {
            MockRecords("AR.DEPOSIT.TYPES", arDepositTypes);
        }

        #endregion

        #region Invoice Types

        protected void SetupInvoiceTypes()
        {
            MockRecord("ST.VALCODES", invoiceTypes);
        }

        #endregion

        #region External Systems

        protected void SetupExternalSystems()
        {
            MockRecord("ST.VALCODES", externalSystems);
        }

        #endregion

        #endregion

    }
}
