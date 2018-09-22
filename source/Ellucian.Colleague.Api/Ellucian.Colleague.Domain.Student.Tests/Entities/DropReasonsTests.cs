using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class DropReasonsTests
    {
        [TestMethod]
        public void constructor_with_specialProcessingCode_S()
        {
            DropReason dropreason = new Student.Entities.DropReason("C", "Changed my mind", "S");
            Assert.IsTrue(dropreason.DisplayInSelfService);
            Assert.AreEqual(dropreason.Code, "C");
            Assert.AreEqual(dropreason.Description, "Changed my mind");
           
        }
        [TestMethod]
        public void constructor_with_specialProcessingCode_Not_S()
        {
            DropReason dropreason = new Student.Entities.DropReason("C", "Changed my mind", "bb");
            Assert.IsFalse(dropreason.DisplayInSelfService);
            Assert.AreEqual(dropreason.Code, "C");
            Assert.AreEqual(dropreason.Description, "Changed my mind");

        }
        [TestMethod]
        public void constructor_with_specialProcessingCode_Empty()
        {
            DropReason dropreason = new Student.Entities.DropReason("C", "Changed my mind", string.Empty);
            Assert.IsFalse(dropreason.DisplayInSelfService);
            Assert.AreEqual(dropreason.Code, "C");
            Assert.AreEqual(dropreason.Description, "Changed my mind");

        }
        [TestMethod]
        public void constructor_with_specialProcessingCode_Null()
        {
            DropReason dropreason = new Student.Entities.DropReason("C", "Changed my mind",null);
            Assert.IsFalse(dropreason.DisplayInSelfService);
            Assert.AreEqual(dropreason.Code, "C");
            Assert.AreEqual(dropreason.Description, "Changed my mind");

        }
        [TestMethod]
        public void constructor_with_specialProcessingCode_lowecase_s()
        {
            DropReason dropreason = new Student.Entities.DropReason("C", "Changed my mind", "s");
            Assert.IsTrue(dropreason.DisplayInSelfService);
            Assert.AreEqual(dropreason.Code, "C");
            Assert.AreEqual(dropreason.Description, "Changed my mind");

        }
    }
}
