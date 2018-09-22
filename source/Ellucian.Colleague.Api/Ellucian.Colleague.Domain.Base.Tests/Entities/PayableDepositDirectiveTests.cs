/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PayableDepositDirectiveTests : TestPayableDepositDirectiveRepository
    {
        #region FIELDS
        string payeeId;
        PayableDepositDirective directiveToTest;
        public PayableDepositDirectiveRecord usData;
        public PayableDepositDirectiveRecord caData;
        #endregion

        #region INITIALIZATION
        public async Task ReallyInitialize()
        {
            payeeId = "0003914";
            directiveToTest = (await base.GetPayableDepositDirectivesAsync(payeeId)).FirstOrDefault();

        }
        #endregion

        #region TESTS
        [TestClass]
        public class UsDirectiveConstructorTests : PayableDepositDirectiveTests
        {
            [TestInitialize]
            public void Initialize()
            {
                if (!ReallyInitialize().IsCompleted)
                {
                    ReallyInitialize().RunSynchronously();
                }

                usData = base.payableDepositDirectiveRecords.FirstOrDefault(d => !string.IsNullOrEmpty(d.routingId));
            }

            //
            // tests for expected functions
            //
            [TestMethod]
            public void USPayableDepositDirectiveIsCreatedTest()
            {
                var usPayableDepositDirective = new PayableDepositDirective(
                    usData.id,
                    usData.personId,
                    usData.routingId,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
                Assert.IsNotNull(usPayableDepositDirective);
                Assert.AreEqual(usData.id, usPayableDepositDirective.Id);
                Assert.AreEqual(usData.personId, usPayableDepositDirective.PayeeId);
                Assert.AreEqual(usData.routingId, usPayableDepositDirective.RoutingId);
                Assert.AreEqual(convertRecordColumnToBankAccountType(usData.accountType), usPayableDepositDirective.BankAccountType);
                Assert.AreEqual(usData.accountIdLastFour, usPayableDepositDirective.AccountIdLastFour);
                Assert.AreEqual(usData.nickname, usPayableDepositDirective.Nickname);
                Assert.AreEqual(usData.isVerified == "Y", usPayableDepositDirective.IsVerified);
                Assert.IsNull(usPayableDepositDirective.AddressId);
                Assert.AreEqual(usData.startDate.Value, usPayableDepositDirective.StartDate);
                Assert.AreEqual(usData.eCheckFlag == "Y", usPayableDepositDirective.IsElectronicPaymentRequested);
                Assert.AreEqual(new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        ), usPayableDepositDirective.Timestamp);
                Assert.IsNull(usPayableDepositDirective.InstitutionId);
                Assert.IsNull(usPayableDepositDirective.BranchNumber);
            }

            //
            // tests for expected exceptions
            //
            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void PayeeIdIsRequiredforUSDirectiveTest()
            {
                new PayableDepositDirective(
                    usData.id,
                    null, // Payee Id missing
                    usData.routingId,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void RoutingIdIsRequiredForUSDirectiveTest()
            {
                new PayableDepositDirective(
                    usData.id,
                    usData.personId,
                    null, // Routing Id missing,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void RoutingIdIsLengthOf9Test()
            {
                new PayableDepositDirective(
                    usData.id,
                    usData.personId,
                    "051009364123", // Routing Id too long
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentException))]
            public void RoutingIdIsNumericTest()
            {
                new PayableDepositDirective(
                    usData.id,
                    usData.personId,
                    "051OO9364", // Routine Id contains alpha "O"
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ApplicationException))]
            public void RoutingIdHasValidCheckSumTest()
            {
                new PayableDepositDirective(
                    usData.id,
                    usData.personId,
                    "051009369", // Routing Id has bad check sum
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(usData.accountType),
                    usData.accountIdLastFour,
                    usData.nickname,
                    usData.isVerified == "Y",
                    usData.addressId,
                    usData.startDate.Value,
                    null, // End Date
                    usData.eCheckFlag == "Y",
                    new Timestamp(
                        usData.addOperator,
                        usData.addTime.ToPointInTimeDateTimeOffset(usData.addDate, colleagueTimeZone).Value,
                        usData.changeOperator,
                        usData.changeTime.ToPointInTimeDateTimeOffset(usData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }
        }

        [TestClass]
        public class CanadianDirectiveConstructorTests : PayableDepositDirectiveTests
        {
            [TestInitialize]
            public void Initialize()
            {
                if (!ReallyInitialize().IsCompleted)
                {
                    ReallyInitialize().RunSynchronously();
                }

                caData = base.payableDepositDirectiveRecords.FirstOrDefault(d => string.IsNullOrEmpty(d.routingId));
            }

            //
            // tests for expected functions
            //
            [TestMethod]
            public void CAPayableDepositDirectiveIsCreatedTest()
            {
                var caPayableDepositDirective = new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                Assert.IsNotNull(caPayableDepositDirective);
                Assert.AreEqual(caData.id, caPayableDepositDirective.Id);
                Assert.AreEqual(caData.personId, caPayableDepositDirective.PayeeId);
                Assert.AreEqual(caData.institutionId, caPayableDepositDirective.InstitutionId);
                Assert.AreEqual(caData.branchNumber, caPayableDepositDirective.BranchNumber);
                Assert.AreEqual(convertRecordColumnToBankAccountType(caData.accountType), caPayableDepositDirective.BankAccountType);
                Assert.AreEqual(caData.accountIdLastFour, caPayableDepositDirective.AccountIdLastFour);
                Assert.AreEqual(caData.nickname, caPayableDepositDirective.Nickname);
                Assert.AreEqual(caData.isVerified == "Y", caPayableDepositDirective.IsVerified);
                Assert.IsNull(caPayableDepositDirective.AddressId);
                Assert.AreEqual(caData.startDate.Value, caPayableDepositDirective.StartDate);
                Assert.AreEqual(caData.eCheckFlag == "Y", caPayableDepositDirective.IsElectronicPaymentRequested);
                Assert.AreEqual(new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        ), caPayableDepositDirective.Timestamp);
                Assert.IsNull(caPayableDepositDirective.RoutingId);
            }


            //
            // tests for expected exceptions
            //

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void PayeeIdIsRequiredforCanadianDirectiveTest()
            {
                new PayableDepositDirective(
                caData.id,
                null, // PayeeId missing
                caData.institutionId,
                caData.branchNumber,
                null, // Bank Name,
                convertRecordColumnToBankAccountType(caData.accountType),
                caData.accountIdLastFour,
                caData.nickname,
                caData.isVerified == "Y",
                caData.addressId,
                caData.startDate.Value,
                null, // End Date
                caData.eCheckFlag == "Y",
                new Timestamp(
                    caData.addOperator,
                    caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                    caData.changeOperator,
                    caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                    )
                );
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionIdRequiredForCanadianDirectiveTest()
            {
                new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    null, // Institution Id missing
                    caData.branchNumber,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void InstitutionIdIsLengthOf3Test()
            {
                new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    "4444", // Institution Id too long
                    caData.branchNumber,
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void BranchNumberRequiredForCanadianDirectiveTest()
            {
                new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    caData.institutionId,
                    null, // Branch Number missing
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

            [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void BranchNumberIsLengthOf5Test()
            {
                new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    caData.institutionId,
                    "4444", // Branch Number too long
                    null, //Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }


        }

        [TestClass]
        public class GeneralDirectiveConstructorTests : PayableDepositDirectiveTests
        {
            [TestInitialize]
            public void Initialize()
            {
                if (!ReallyInitialize().IsCompleted)
                {
                    ReallyInitialize().RunSynchronously();
                }

                caData = base.payableDepositDirectiveRecords.FirstOrDefault(d => string.IsNullOrEmpty(d.routingId));
            }

            //
            // tests for expected functions
            //
            [TestMethod]
            public void AccountIdCanBeSetTest()
            {
                Initialize();
                var someAccountId = "adgkskfds78sa745r";
                directiveToTest.SetNewAccountId(someAccountId);
                Assert.AreEqual(someAccountId, directiveToTest.NewAccountId);
            }

            [TestMethod, ExpectedException(typeof(ArgumentNullException))]
            public void NullAccountIdSetterErrorTest()
            {
                Initialize();
                var someAccountId = string.Empty;
                directiveToTest.SetNewAccountId(someAccountId);
            }

            [TestMethod]
            public void DirectivesAreEqualWhenIdsAreEqualTest()
            {
                var caPayableDepositDirective1 = new PayableDepositDirective(
                    "123", // Id
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber, 
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                var caPayableDepositDirective2 = new PayableDepositDirective(
                    "123", // Id
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber, // Branch Number missing
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                Assert.IsTrue(caPayableDepositDirective1.Equals(caPayableDepositDirective2));
                Assert.AreEqual(caPayableDepositDirective1.GetHashCode(), caPayableDepositDirective2.GetHashCode());
            }

            [TestMethod]
            public void DirectivesAreNotEqualWhenIdsAreNotEqualTest()
            {
                var caPayableDepositDirective1 = new PayableDepositDirective(
                    "123", // Id
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber, // Branch Number missing
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                var caPayableDepositDirective2 = new PayableDepositDirective(
                    "456", // Id
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber, // Branch Number missing
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                Assert.IsFalse(caPayableDepositDirective1.Equals(caPayableDepositDirective2));
                Assert.AreNotEqual(caPayableDepositDirective1.GetHashCode(), caPayableDepositDirective2.GetHashCode());
            }

            [TestMethod]
            public void DirectivesAreNotEqualIfArgumentIsNullTest()
            {
                var caPayableDepositDirective = new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber, // Branch Number missing
                    null, // Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    caData.nickname,
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
                Assert.IsFalse(caPayableDepositDirective.Equals(null));
            }

            ///
            // tests for expected exceptions
            ///
            [TestMethod, ExpectedException(typeof(FormatException))]
            public void NoErrorIfInvalidCharactersInNicknameTest()
            {
                new PayableDepositDirective(
                    caData.id,
                    caData.personId,
                    caData.institutionId,
                    caData.branchNumber,
                    null, //Bank Name,
                    convertRecordColumnToBankAccountType(caData.accountType),
                    caData.accountIdLastFour,
                    "Nick<Name", // Invalid character in Nickname
                    caData.isVerified == "Y",
                    caData.addressId,
                    caData.startDate.Value,
                    null, // End Date
                    caData.eCheckFlag == "Y",
                    new Timestamp(
                        caData.addOperator,
                        caData.addTime.ToPointInTimeDateTimeOffset(caData.addDate, colleagueTimeZone).Value,
                        caData.changeOperator,
                        caData.changeTime.ToPointInTimeDateTimeOffset(caData.changeTime, colleagueTimeZone).Value
                        )
                    );
            }

        }
        #endregion
    }
}