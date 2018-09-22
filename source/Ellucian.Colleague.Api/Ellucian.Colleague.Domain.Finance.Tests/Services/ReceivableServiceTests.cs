// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class ReceivableServiceTests
    {
        static string invId = "123456789";
        string personId = "1234567";
        string referenceNumber = "000067890";
        DateTime date = DateTime.Parse("07/31/2013");
        DateTime dueDate = DateTime.Parse("08/15/2013");
        DateTime billingEnd = DateTime.Parse("12/31/2013");
        DateTime billingStart = DateTime.Parse("08/01/2013");
        string description = "Registration - 2013/FA";

        // invoice items
        static ReceivableCharge goodCharge1 = new ReceivableCharge("24680", invId, new List<String>() { "Full-Time Tuition", "BIOL-101-01" }, "TUIFT", 1875);
        static ReceivableCharge goodCharge2 = new ReceivableCharge("13579", invId, new List<String>() { "Activities Fee" }, "ACTFE", 175);
        static ReceivableCharge badCharge = new ReceivableCharge("14495", invId, new List<String>() { "Made Up Charge" }, "MADEUP", 999);
        static List<ReceivableCharge> goodCharges = new List<ReceivableCharge> {
            goodCharge1,
            goodCharge2
        };
        static List<ReceivableCharge> badCharges = new List<ReceivableCharge>() {badCharge};
        // AR Types
        List<ReceivableType> goodReceivableTypes = new List<ReceivableType>() {
            new ReceivableType("01", "Description 1"),
            new ReceivableType("02", "Description 2") 
        };
        string goodRcvTypeCode = "01";
        // invoice types
        List<InvoiceType> goodInvTypes = new List<InvoiceType>() {
            new InvoiceType("BK", "Inv Type Description 1"),
            new InvoiceType("REG", "Inv Type Description 2") 
        };
        string goodInvTypeCode = "BK";
        // Charge codes
        List<ChargeCode> goodChargeCodes = new List<ChargeCode>() {
            new ChargeCode("TUIFT", "TUIT Description", 1),
            new ChargeCode("ACTFE", "ACTFE Description", 2)
        };
        // terms
        List<Term> goodTerms = new List<Term>() {
            new Term("2013/SP", "2013 Spring Term", DateTime.Now, DateTime.Now.AddDays(100), 9999, 1, false, true, "2013RSP", false),
            new Term("2013/FA", "2013 Fall Term", DateTime.Now.AddDays(101), DateTime.Now.AddDays(200), 9999, 1, false, true, "2014RFA", false)
        };
        string goodTermId = "2013/FA";
        // deposit types
        static string depositType1 = "DEP1";
        static string depositType2 = "DEP2";
        List<DepositType> depositTypes = new List<DepositType>(){
            new DepositType(depositType1, "Deposit Type 1"),
            new DepositType(depositType2, "Deposit type 2")
        };
        // external systems
        static string extSys1 = "SYS1";
        static string extSys2 = "SYS2";
        string externalId = "External Id 1";
        List<ExternalSystem> systems = new List<ExternalSystem>(){
            new ExternalSystem(extSys1, "External System 1"),
            new ExternalSystem(extSys2, "External System 2")
        };

        ReceivableInvoice goodInvoice;
        Deposit goodDeposit;


        [TestInitialize]
        public void Initialize()
        {
            goodInvoice = new ReceivableInvoice(invId, referenceNumber, personId, goodRcvTypeCode, goodTermId, date,
                dueDate, billingStart, billingEnd, description, goodCharges);
            goodInvoice.InvoiceType = goodInvTypeCode;
            //deposit
            goodDeposit = new Deposit(null, personId, DateTime.Now.Date, depositType1, 10000)
            {
                ReceiptId = "Some receipt id",
                TermId = goodTermId
            };
            goodDeposit.AddExternalSystemAndId(extSys1, externalId);
        }

        #region Invoices
        // test valid invoice for success
        [TestMethod]
        public void ValidateInvoice_ValidInvoice()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, goodChargeCodes , goodInvTypes, goodTerms);
        }

        // test for null invoice
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_NullInvoice()
        {
            ReceivableService.ValidateInvoice(null, goodReceivableTypes, goodChargeCodes, goodInvTypes, goodTerms);
        }

        // test for null receivable types
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_NullReceivableType()
        {
            ReceivableService.ValidateInvoice(goodInvoice, null, goodChargeCodes, goodInvTypes, goodTerms);
        }

        // test for empty receivable types
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_EmptyReceivableType()
        {
            ReceivableService.ValidateInvoice(goodInvoice, new List<ReceivableType>(), goodChargeCodes, goodInvTypes, goodTerms);
        }

        // test for null Charge Codes
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_NullChargeCodes()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, null, goodInvTypes, goodTerms);
        }

        // test for empty charge codes
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_EmptyChargeCodes()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, new List<ChargeCode>(), goodInvTypes, goodTerms);
        }

        // test for null Invoice Types
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_NullInvoiceTypes()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, goodChargeCodes, null, goodTerms);
        }

        // test for empty invoice types
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_EmptyInvoiceTypes()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, goodChargeCodes, new List<InvoiceType>(), goodTerms);
        }

        // test for null terms
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_NullTerms()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, goodChargeCodes, goodInvTypes, null);
        }

        // test for empty terms
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateInvoice_EmptyTerms()
        {
            ReceivableService.ValidateInvoice(goodInvoice, goodReceivableTypes, goodChargeCodes, goodInvTypes, new List<Term>());
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateInvoice_InvalidReceivableType()
        {
            var result = new ReceivableInvoice(invId, personId, "BAD", goodTermId, referenceNumber, date,
                dueDate, billingStart, billingEnd, description, goodCharges);

            ReceivableService.ValidateInvoice(result, goodReceivableTypes, goodChargeCodes, goodInvTypes, goodTerms);
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateInvoice_InvalidInvoicetype()
        {
            var result = new ReceivableInvoice(invId, personId, "BAD", goodTermId, referenceNumber, date,
                dueDate, billingStart, billingEnd, description, goodCharges);
            result.InvoiceType = "BAD";

            ReceivableService.ValidateInvoice(result, goodReceivableTypes, goodChargeCodes, goodInvTypes, goodTerms);
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateInvoice_InvalidChargeCode()
        {
            var result = new ReceivableInvoice(invId, personId, goodRcvTypeCode, goodTermId, referenceNumber, date,
                dueDate, billingStart, billingEnd, description, badCharges);

            ReceivableService.ValidateInvoice(result, goodReceivableTypes, goodChargeCodes, goodInvTypes, goodTerms);
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateInvoice_InvalidTermId()
        {
            var result = new ReceivableInvoice(invId, personId, goodRcvTypeCode, "BAD", referenceNumber, date,
                dueDate, billingStart, billingEnd, description, badCharges);

            ReceivableService.ValidateInvoice(result, goodReceivableTypes, goodChargeCodes, goodInvTypes, goodTerms);
        }
        #endregion


        #region Deposits
        // valid deposit passes
        [TestMethod]
        public void ValidateDeposit_ValidDeposit()
        {
            ReceivableService.ValidateDeposit(goodDeposit, depositTypes, systems, goodTerms);
        }
        // null deposit fails
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateDeposit_NullDeposit()
        {
            ReceivableService.ValidateDeposit(null, depositTypes, systems, goodTerms);
        }
        // null deposit types
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateDeposit_NullDepositTypes()
        {
            ReceivableService.ValidateDeposit(goodDeposit, null, systems, goodTerms);
        }
        // null systems
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateDeposit_NullSystems()
        {
            ReceivableService.ValidateDeposit(goodDeposit, depositTypes, null, goodTerms);
        }
        // null terms
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateDeposit_NullTerms()
        {
            ReceivableService.ValidateDeposit(goodDeposit, depositTypes, systems, null);
        }
        // bad deposit type
        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void ValidateDeposit_BadDepositType()
        {
            Deposit badDepositType = new Deposit(null, personId, DateTime.Now.Date, "BAD", 10000);
            ReceivableService.ValidateDeposit(badDepositType, depositTypes, systems, goodTerms);
        }
        #endregion
    }
}
