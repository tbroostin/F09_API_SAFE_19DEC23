using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollDepositDirectiveCollectionTests : TestPayrollDepositDirectivesRepository
    {
        string employeeId;
        PayrollDepositDirectiveCollection testCollection;

        public async Task GetAsynchronousData()
        {
            employeeId = "24601";
            testCollection = await base.GetPayrollDepositDirectivesAsync(employeeId);
        }

        [TestInitialize]
        public void Initialize()
        {
            if(!GetAsynchronousData().IsCompleted)
                GetAsynchronousData().RunSynchronously(); 
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullEmployeeIdExceptionTest()
        {
            new PayrollDepositDirectiveCollection(null);
        }
        [TestMethod]
        public void NoDirectivesInConstructorCreatesEmptyListTest()
        {
            var collection = new PayrollDepositDirectiveCollection(employeeId);
            Assert.IsNotNull(collection);
        }
        [TestMethod]
        public void DirectivesInConstructorAreSetTest()
        {
            var collection = new PayrollDepositDirectiveCollection(employeeId, testCollection);
            Assert.AreEqual(collection, testCollection);
        }

        [TestMethod]        
        public void WhereReturnsNewCollectionTest()
        {
            var result = testCollection.Where(directive => directive.PersonId == employeeId);
            Assert.IsInstanceOfType(result, typeof(PayrollDepositDirectiveCollection));
        }

        [TestMethod]
        public void AddAddsADirectiveToCollectionTest()
        {
            var directiveToAdd = new PayrollDepositDirective("ohno",employeeId,"091000019","whatever",Base.Entities.BankAccountType.Savings,"down","south",false,7,99.4M,DateTime.Now,null,new Base.Entities.Timestamp(employeeId,DateTime.Now,employeeId,DateTime.Now));
            testCollection.Add(directiveToAdd);
            Assert.IsTrue(testCollection.Contains(directiveToAdd));
        }

        [TestMethod]
        public void AccountCountIsCorrectTest()
        {
            var collectionCount = testCollection.Count;
            var directiveCount = testCollection.Count;
            Assert.AreEqual(collectionCount, directiveCount);
        }
        [TestMethod,ExpectedException(typeof(ArgumentException))]
        public void CannotAddDuplicateDirectiveTest()
        {
            testCollection.Add(testCollection[0]);
        }
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CannotAddAdditionalRemainderTest()
        {
            var directiveToAdd = new PayrollDepositDirective("obrother", employeeId, "091000019", "whatever", Base.Entities.BankAccountType.Savings, "down", "south", false, 999, 99.4M, DateTime.Now, null, new Base.Entities.Timestamp(employeeId, DateTime.Now, employeeId, DateTime.Now));
            testCollection.Add(directiveToAdd);
        }
        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void CannotAddForOtherPersonTest()
        {
            var directiveToAdd = new PayrollDepositDirective("inspector", "JAVER", "091000019", "whatever", Base.Entities.BankAccountType.Savings, "down", "south", false, 11, 99.4M, DateTime.Now, null, new Base.Entities.Timestamp(employeeId, DateTime.Now, employeeId, DateTime.Now));
            testCollection.Add(directiveToAdd);
        }


        [TestMethod]
        public void DeletingFutureEntireBalanceDoesntThrowException()
        {
            var testFutureEntireBalanceCollection = new PayrollDepositDirectiveCollection(employeeId);
            // Remainder account being reopened
            var remainderAccountToReopen = new PayrollDepositDirective("1", employeeId, "091016647", "Remainder Bank 1", BankAccountType.Checking, "0001", "Test Acc 1", false, 999, null, new DateTime(2021, 4, 1), null, new Base.Entities.Timestamp(employeeId, DateTime.Now, employeeId, DateTime.Now)) { };
            var futureEntireBalanceToClose = new PayrollDepositDirective("2", employeeId, "021000089", "Future Entire Balance Delete", BankAccountType.Checking, "0001", "Test Acc 1", false, 999, null, new DateTime(2021, 7, 1), new DateTime(2021, 1, 1), new Base.Entities.Timestamp(employeeId, DateTime.Now, employeeId, DateTime.Now)) { };
            testFutureEntireBalanceCollection.Add(remainderAccountToReopen);
            testFutureEntireBalanceCollection.Add(futureEntireBalanceToClose);
            Assert.IsTrue(testFutureEntireBalanceCollection.Contains(remainderAccountToReopen));
            Assert.IsTrue(testFutureEntireBalanceCollection.Contains(futureEntireBalanceToClose));
        }
    }
}
