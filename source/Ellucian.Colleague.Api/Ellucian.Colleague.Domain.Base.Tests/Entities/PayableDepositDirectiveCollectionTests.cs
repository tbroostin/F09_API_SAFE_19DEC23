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
    public class PayableDepositDirectiveCollectionTests : TestPayableDepositDirectiveRepository
    {
        string payeeId;
        PayableDepositDirectiveCollection testCollection;

        public async Task GetAsynchronousData()
        {
            payeeId = "0003914";
            testCollection = await base.GetPayableDepositDirectivesAsync(payeeId);
        }

        [TestInitialize]
        public void Initialize()
        {
            if (!GetAsynchronousData().IsCompleted)
            {
                GetAsynchronousData().RunSynchronously();
            }
        }

        //
        // tests for expected functions
        //
        [TestMethod]
        public void NoDirectivesInConstructorCreatesEmptyListTest()
        {
            var collection = new PayableDepositDirectiveCollection(payeeId);
            Assert.IsNotNull(collection);
        }

        [TestMethod]
        public void CanAddDirectiveToEmptyCollectionTest()
        {
            var collection = new PayableDepositDirectiveCollection(payeeId);
            var directiveToAdd = new PayableDepositDirective(
                        "001", // id 
                        payeeId,
                        "051009364", // routingId
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234", // accountIdLastFour
                        "Acct Nickname",
                        false, // isVerified
                        null, // addressId
                        new DateTime(2017, 1, 1), // startDate
                        null, // endDate
                        true, // isElectronicPaymentRequested
                        new Timestamp(
                            "ABC",
                            new DateTime(2016,12,15,10,30,00),
                            "ABC",
                            new DateTime(2016,12,30,12,30,00)
                            )
                        );
            collection.Add(directiveToAdd);
            Assert.IsTrue(collection.Contains(directiveToAdd));
        }

        [TestMethod]
        public void CanAddDirectiveToNonEmptyCollectionTest()
        {
            var directiveToAdd = new PayableDepositDirective(
                        "001", // id 
                        payeeId,
                        "051009364", // routingId
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234", // accountIdLastFour
                        "Acct Nickname",
                        false, // isVerified
                        null, // addressId
                        new DateTime(2017, 1, 1), // startDate
                        null, // endDate
                        true, // isElectronicPaymentRequested
                        new Timestamp(
                            "ABC",
                            new DateTime(2016, 12, 15, 10, 30, 00),
                            "ABC",
                            new DateTime(2016, 12, 30, 12, 30, 00)
                            )
                        );
            testCollection.Add(directiveToAdd);
            Assert.IsTrue(testCollection.Contains(directiveToAdd));
        }

        [TestMethod]
        public void DirectiveCollectionCountIsCorrectTest()
        {
            //var collectionCount = testCollection.Count;
            //var directiveCount = testCollection.depositDirectives.Count;
            //Assert.AreEqual(collectionCount, directiveCount);
        }

        [TestMethod]
        public void CollectionsAreEqualWhenPropertiesAreEqualTest()
        {
            var testCollection1 = testCollection;
            var testCollection2 = testCollection;
            Assert.IsTrue(testCollection1.Equals(testCollection2));
        }

        [TestMethod]
        public void CollectionsAreNotEqualWhenPropertiesAreNotEqualTest()
        {
            var testCollection1 = new PayableDepositDirectiveCollection(payeeId);
            var directiveToAdd = new PayableDepositDirective(
                        "001", // id 
                        payeeId,
                        "051009364", // routingId
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234", // accountIdLastFour
                        "Acct Nickname",
                        false, // isVerified
                        null, // addressId
                        new DateTime(2017, 1, 1), // startDate
                        null, // endDate
                        true, // isElectronicPaymentRequested
                        new Timestamp(
                            "ABC",
                            new DateTime(2016, 12, 15, 10, 30, 00),
                            "ABC",
                            new DateTime(2016, 12, 30, 12, 30, 00)
                            )
                        );
            testCollection1.Add(directiveToAdd);

            var testCollection2 = new PayableDepositDirectiveCollection(payeeId);
            directiveToAdd = new PayableDepositDirective(
                        "002", // different Id 
                        payeeId,
                        "051009364",
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234",
                        "Acct Nickname",
                        false,
                        null,
                        new DateTime(2017, 1, 1),
                        null,
                        true,
                        new Timestamp(
                            "ABC",
                            new DateTime(2016, 12, 15, 10, 30, 00),
                            "ABC",
                            new DateTime(2016, 12, 30, 12, 30, 00)
                            )
                        );
            testCollection2.Add(directiveToAdd);

            Assert.IsFalse(testCollection1.Equals(testCollection2));
        }

        [TestMethod]
        public void CollectionsAreNotEqualIfArgumentIsNullTest()
        {
            var testCollection = new PayableDepositDirectiveCollection(payeeId);
            var directiveToAdd = new PayableDepositDirective(
                        "001", // id 
                        payeeId,
                        "051009364", // routingId
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234", // accountIdLastFour
                        "Acct Nickname",
                        false, // isVerified
                        null, // addressId
                        new DateTime(2017, 1, 1), // startDate
                        null, // endDate
                        true, // isElectronicPaymentRequested
                        new Timestamp(
                            "ABC",
                            new DateTime(2016, 12, 15, 10, 30, 00),
                            "ABC",
                            new DateTime(2016, 12, 30, 12, 30, 00)
                            )
                        );
            testCollection.Add(directiveToAdd);
            Assert.IsFalse(testCollection.Equals(null));
        }

        
        //
        // tests for expected exceptions
        //
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullPayeeIdExceptionTest()
        {
            new PayableDepositDirectiveCollection(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CannotAddNullDirectiveToCollectionTest()
        {
            testCollection.Add(null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CannotAddDuplicateDirectiveTest()
        {
            testCollection.Add(testCollection[0]);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CannotAddDirectiveForOtherPayeeToCollectionTest()
        {
            var directiveToAdd = new PayableDepositDirective(
                        "001", 
                        "1234567", // different payeeId
                        "051009364", 
                        "Bank Name",
                        convertRecordColumnToBankAccountType("C"),
                        "1234", 
                        "Acct Nickname",
                        false, 
                        null, 
                        new DateTime(2017, 1, 1), 
                        null, 
                        true, 
                        new Timestamp(
                            "ABC",
                            new DateTime(2016, 12, 15, 10, 30, 00),
                            "ABC",
                            new DateTime(2016, 12, 30, 12, 30, 00)
                            )
                        );
            testCollection.Add(directiveToAdd);
        }

    }
}
