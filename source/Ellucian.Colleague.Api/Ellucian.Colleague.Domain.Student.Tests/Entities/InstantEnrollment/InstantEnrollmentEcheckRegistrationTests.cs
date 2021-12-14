// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class InstantEnrollmentRegistrationEcheckRegistrationTests
    {
        private string personId;
        private InstantEnrollmentPersonDemographic demo;
        private InstantEnrollmentPersonDemographic demoNullEmail;
        private InstantEnrollmentPersonDemographic demoEmptyEmail;
        private InstantEnrollmentPersonDemographic demoEmptyCity;
        private InstantEnrollmentPersonDemographic demoEmptyState;
        private InstantEnrollmentPersonDemographic demoEmptyZip;
        private string acadProgram;
        private string catalog;
        private List<InstantEnrollmentRegistrationBaseSectionToRegister> sections;
        private decimal payAmount;
        private string payMethod;
        private string providerAccount;
        private string bankAcctOwner;
        private string routingNumber;
        private string bankAcctNumber;
        private string bankAccountCheckNumber;
        private string bankAcctType;
        private string convFeeDesc;
        private decimal? convFeeAmount;
        private string convFeeGL;
        private string payerEmailAddress;
        private string payerAddress;
        private string payerCity;
        private string payerState;
        private string payerPostal;

        [TestInitialize]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Initialize()
        {
            personId = "0000001";
            demo = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
            {
                EmailAddress = "joe@testclass.com",
                City = "City",
                State = "VA",
                ZipCode = "22033",
            };
            demoNullEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass");
            demoEmptyEmail = new InstantEnrollmentPersonDemographic("Joe", "TestClass") { EmailAddress = String.Empty };
            demoEmptyCity = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
            {
                EmailAddress = "joe@testclass.com",
                City =String.Empty,
                State = "VA",
                ZipCode = "22033",
            };
            demoEmptyState = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
            {
                EmailAddress = "joe@testclass.com",
                City = "City",
                State = String.Empty,
                ZipCode = "22033",
            };
            demoEmptyZip = new InstantEnrollmentPersonDemographic("Joe", "TestClass")
            {
                EmailAddress = "joe@testclass.com",
                City = "City",
                State = "VA",
                ZipCode = String.Empty,
            };

            acadProgram = "Program";
            catalog = "Catalog";
            sections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>()
            {
                new InstantEnrollmentRegistrationBaseSectionToRegister("SECT1", 300),
            };
            payAmount = 10000;
            payMethod = "PAYM";
            providerAccount = "PROV.ACCT";
            bankAcctOwner = "PAYER";
            routingNumber = "ROUTE";
            bankAcctNumber = "ACCT.NO" ;
            bankAccountCheckNumber = "CHECK.NO";
            bankAcctType = "CHECK";
            convFeeDesc = "CONV.DESC";
            convFeeAmount = 1000;
            convFeeGL = "CONV.GL";
            payerEmailAddress = "PAYER@EMAIL.COM";
            payerAddress = "123 MAIN ST";
            payerCity = "PAY CITY";
            payerState = "PAY STATE" ;
            payerPostal = "22033";

    }

    [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPersonNullDemo()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, null, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPersonNullDemo()
        {
            var reg = new InstantEnrollmentEcheckRegistration(String.Empty, null, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullEmail()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demoNullEmail, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyEmail()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demoEmptyEmail, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullAcadProgram()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, null, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyAcadProgram()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, String.Empty, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal); ;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullCatalog()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, null, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyCatalog()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, String.Empty, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullSections()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, null, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                String.Empty, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayMethod()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                null, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyDemoCity()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demoEmptyCity, acadProgram, catalog, sections, payAmount,
                null, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyDemoState()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demoEmptyState, acadProgram, catalog, sections, payAmount,
                null, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyDemoZip()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demoEmptyZip, acadProgram, catalog, sections, payAmount,
                null, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayMethod()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                String.Empty, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullProviderAccount()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, null, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyProviderAccount()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, string.Empty, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullOwner()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, null, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyOwner()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, string.Empty, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullRoute()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, null, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyRoute()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, String.Empty, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullBankAccountNumber()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, null, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyBankAccountNumber()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, String.Empty, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullCheckNumber()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, null,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyCheckNumber()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, String.Empty,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullBankAccountType()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                null, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyBankAccountType()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                String.Empty, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullConvenienceFees()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, null, null, null, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyConvenienceFees()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, null, String.Empty, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);
        }
        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ZeroConvenienceFees()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, 0, String.Empty, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ConvFeeWithNullInformation()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, null, convFeeAmount, null, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ConvFeeWithEmptyInformation()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, convFeeAmount, String.Empty, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ConvFeeGlAccount()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, 0, String.Empty, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);

        }
        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ConvFeesIsNull_ConvFeeGlAccountIsNotNull()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, null, "gl account", payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);

        }
        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ConvFeesIsZero_ConvFeeGlAccountIsNotNull()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, String.Empty, 0, "gl account", payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.IsNotNull(reg);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayerEmail()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, null, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayerEmail()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, String.Empty, payerAddress, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayerAddress()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, null, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayerAddress()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, String.Empty, payerCity, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayerCity()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, null, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayerCity()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, String.Empty, payerState,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayerState()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, null,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayerState()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, String.Empty,
                payerPostal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_NullPayerPostal()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_EmptyPayerPostal()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                String.Empty);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ValidDemo()
        {
            var reg = new InstantEnrollmentEcheckRegistration(null, demo, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.AreEqual(String.Empty, reg.PersonId);
            Assert.AreSame(demo, reg.PersonDemographic);
            Assert.AreEqual(acadProgram, reg.AcademicProgram);
            Assert.AreEqual(catalog, reg.Catalog);
            Assert.AreEqual("SECT1", reg.ProposedSections[0].SectionId);
            Assert.AreEqual(300, reg.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(payAmount, reg.PaymentAmount);
            Assert.AreEqual(payMethod, reg.PaymentMethod);
            Assert.AreEqual(providerAccount, reg.ProviderAccount);
            Assert.AreEqual(bankAcctOwner, reg.BankAccountOwner);
            Assert.AreEqual(routingNumber, reg.BankAccountRoutingNumber);
            Assert.AreEqual(bankAcctNumber, reg.BankAccountNumber);
            Assert.AreEqual(bankAccountCheckNumber, reg.BankAccountCheckNumber);
            Assert.AreEqual(convFeeDesc, reg.ConvenienceFeeDesc);
            Assert.AreEqual(convFeeAmount, reg.ConvenienceFeeAmount);
            Assert.AreEqual(convFeeGL, reg.ConvenienceFeeGlAccount);
            Assert.AreEqual(payerEmailAddress, reg.PayerEmailAddress);
            Assert.AreEqual(payerAddress, reg.PayerAddress);
            Assert.AreEqual(payerCity, reg.PayerCity);
            Assert.AreEqual(payerState, reg.PayerState);
            Assert.AreEqual(payerPostal, reg.PayerPostalCode);
        }

        [TestMethod]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Constructor_ValidPersonId()
        {
            var reg = new InstantEnrollmentEcheckRegistration(personId, null, acadProgram, catalog, sections, payAmount,
                payMethod, providerAccount, bankAcctOwner, routingNumber, bankAcctNumber, bankAccountCheckNumber,
                bankAcctType, convFeeDesc, convFeeAmount, convFeeGL, payerEmailAddress, payerAddress, payerCity, payerState,
                payerPostal);
            Assert.AreEqual(personId, reg.PersonId);
            Assert.AreEqual(acadProgram, reg.AcademicProgram);
            Assert.AreEqual(catalog, reg.Catalog);
            Assert.AreEqual("SECT1", reg.ProposedSections[0].SectionId);
            Assert.AreEqual(300, reg.ProposedSections[0].AcademicCredits);
            Assert.AreEqual(payAmount, reg.PaymentAmount);
            Assert.AreEqual(payMethod, reg.PaymentMethod);
            Assert.AreEqual(providerAccount, reg.ProviderAccount);
            Assert.AreEqual(bankAcctOwner, reg.BankAccountOwner);
            Assert.AreEqual(routingNumber, reg.BankAccountRoutingNumber);
            Assert.AreEqual(bankAcctNumber, reg.BankAccountNumber);
            Assert.AreEqual(bankAccountCheckNumber, reg.BankAccountCheckNumber);
            Assert.AreEqual(convFeeDesc, reg.ConvenienceFeeDesc);
            Assert.AreEqual(convFeeAmount, reg.ConvenienceFeeAmount);
            Assert.AreEqual(convFeeGL, reg.ConvenienceFeeGlAccount);
            Assert.AreEqual(payerEmailAddress, reg.PayerEmailAddress);
            Assert.AreEqual(payerAddress, reg.PayerAddress);
            Assert.AreEqual(payerCity, reg.PayerCity);
            Assert.AreEqual(payerState, reg.PayerState);
            Assert.AreEqual(payerPostal, reg.PayerPostalCode);
        }

        [TestCleanup]
        public void InstantEnrollmentRegistrationEcheckRegistrationTests_Cleanup()
        {
            personId = null;
            demo = null;
            demoNullEmail = null;
            demoEmptyEmail = null;
            demoEmptyCity = null;
            demoEmptyState = null;
            demoEmptyZip = null;
            acadProgram = null;
            catalog = null;
            sections = null;
            payMethod = null;
            providerAccount = null;
            bankAcctOwner = null;
            routingNumber = null;
            bankAcctNumber = null;
            bankAccountCheckNumber = null;
            bankAcctType = null;
            convFeeDesc = null;
            convFeeAmount = null;
            convFeeGL = null;
            payerEmailAddress = null;
            payerAddress = null;
            payerCity = null;
            payerState = null;
            payerPostal = null;
        }
    }
}
