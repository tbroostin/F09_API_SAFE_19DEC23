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
    }
}
