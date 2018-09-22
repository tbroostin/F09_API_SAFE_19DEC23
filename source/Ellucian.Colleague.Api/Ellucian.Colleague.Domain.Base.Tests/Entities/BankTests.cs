// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class BankTests
    {
        #region FIELDS
        public Bank usBank;
        public Bank canadaBank;
        #endregion

        #region INITIALIZATION
        public void BankTestsInitialize()
        {
            usBank = new Bank("011000015", "Federal Reserve Bank", "011000015");
            canadaBank = new Bank("177-12345", "Bank of Canada","177", "12345");
        }
        #endregion

        #region TESTS
        [TestClass]
        public class UsBank_ConstructorTests : BankTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.BankTestsInitialize();
            }

            // tests for expected functions
            [TestMethod]
            public void NameIsSetTest()
            {
                var bank = new Bank("011000015", "Some Name", "011000015");
                Assert.AreEqual("Some Name", bank.Name);
            }

            [TestMethod]
            public void UsIdIsSetTest()
            {
                var bank = new Bank("011000028","US bank", "011000028");
                Assert.AreEqual("011000028", bank.Id);
            }

            [TestMethod]
            public void RoutingIdIsSetTest()
            {
                var bank = new Bank("011000028", "US bank", "011000028");
                Assert.AreEqual("011000028", bank.RoutingId);
            }

            [TestMethod]
            public void CanadianDataIsNullTest()
            {
                var bank = new Bank("011000028", "US bank", "011000028");
                Assert.IsNull(bank.InstitutionId);
                Assert.IsNull(bank.BranchTransitNumber);
            }


            // tests for expected exceptions
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForEmptyNameTest()
            {
                new Bank("011000015", "", "011000015");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForNullNameTest()
            {
                new Bank("011000015", null, "011000015");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForEmptyIdTest()
            {
                new Bank("", "Bank name", "011000015");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForNullIdTest()
            {
                new Bank(null, "Bank name", "011000015");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForEmptyRoutingIdTest()
            {
                new Bank("011000015", "Bank name", "");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForNullRoutingIdTest()
            {
                new Bank("011000015", "Bank name", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ThrowsExceptionForWhitespaceNameTest()
            {
                new Bank("011000015", "    ", "123");
            }
        }

        [TestClass]
        public class RoutingIdValidationTests : BankTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.BankTestsInitialize();
            }

            // tests for expected functions
            [TestMethod]
            public void ValidUSIdIsReturnedTest()
            {
                var bank = new Bank("011000015","Bank Name", "011000015");
                Assert.AreEqual("011000015", bank.Id);
            }

            // tests for expected exceptions
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsExceptionForInvalidCharacterTest()
            {
                new Bank("011000015", "Bank name", "01100*015");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ThrowsExceptionForChecksumError()
            {
                new Bank("123456789", "Bank name", "123456789");
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ThrowsExceptionForInvalidIdLengthTest()
            {
                new Bank("011000015", "Bank name", "01100001");
            }

        }

        [TestClass]
        public class CanadaBank_constructorTests: BankTests
        {
            public string id;
            public string name;
            public string institutionId;
            public string branchNumber;

            public Bank testBank
            {
                get
                {
                    return new Bank(id, name, institutionId, branchNumber);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                id = "123-12345";
                name = "Canada Bank";
                institutionId = "123";
                branchNumber = "12345";
            }

            [TestMethod]
            public void IdTest()
            {
                Assert.AreEqual(id, testBank.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IdRequiredTest()
            {
                id = null;
                var fail = testBank;
            }

            [TestMethod]
            public void NameTest()
            {
                Assert.AreEqual(name, testBank.Name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NameRequiredTest()
            {
                name = "";
                var fail = testBank;
            }

            [TestMethod]
            public void InstitutionIdTest()
            {
                Assert.AreEqual(institutionId, testBank.InstitutionId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionIdRequiredTest()
            {
                institutionId = "   ";
                var fail = testBank;
            }

            [TestMethod]
            public void BranchTransitNumberTest()
            {
                Assert.AreEqual(branchNumber, testBank.BranchTransitNumber);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BranchNumberRequiredTest()
            {
                branchNumber = null;
                var fail = testBank;
            }

            //validations
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void InstitutionIdMustBeLengthOfThreeTest()
            {
                institutionId = "12345";
                var fail = testBank;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void InstituionIdMustBeNumericTest()
            {
                institutionId = "12!";
                var fail = testBank;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void BranchNumberMustBeLengthOfFiveTest()
            {
                branchNumber = "123456";
                var fail = testBank;
            }
        }

        [TestClass]
        public class EqualsTests : BankTests
        {
            public string id;
            public string name;
            public string institutionId;
            public string branchNumber;
            public string routingId;

            [TestInitialize]
            public void Initialize()
            {
                id = "011000015";
                name = "Bank";
                institutionId = "123";
                branchNumber = "12345";
                routingId = "011000015";
            }

            [TestMethod]
            public void AreEqualWhenIdsAreEqualTest()
            {
                //contrived scenario, obviously a US bank wouldn't have the same id as a Canadian Bank
                var bank1 = new Bank(id, name, institutionId, branchNumber);
                var bank2 = new Bank(id, name, routingId);

                Assert.IsTrue(bank1.Equals(bank2));
                Assert.AreEqual(bank1.GetHashCode(), bank2.GetHashCode());
            }

            

            [TestMethod]
            public void AreNotEqualWhenIdsAreNotEqualTest()
            {
                var bank1 = new Bank(id, name, routingId);
                var bank2 = new Bank("123-12345", name, institutionId, branchNumber);

                Assert.IsFalse(bank1.Equals(bank2));
                Assert.AreNotEqual(bank1.GetHashCode(), bank2.GetHashCode());
            }

            [TestMethod]
            public void AreNotEqualIfArgumentIsNullTest()
            {
                var bank1 = new Bank(id, name, routingId);
                Assert.IsFalse(bank1.Equals(null));
            }

            [TestMethod]
            public void AreNotEqualIfArgumentIsDifferentTypeTest()
            {
                var bank1 = new Bank(id, name, routingId);
                Assert.IsFalse(bank1.Equals(StringComparison.CurrentCulture));
            }            
        }


        #endregion
    }
}
