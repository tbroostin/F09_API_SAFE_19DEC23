using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class WaitlistStatusTests
    {
        string code = "A";
        string desc = "Active";
        WaitlistStatus status = WaitlistStatus.WaitingToEnroll;
        WaitlistStatusCode result;

        [TestInitialize]
        public void Initialize()
        {
            result = new WaitlistStatusCode(code, desc, status);
        }

        [TestMethod]
        public void WaitlistStatus_Constructor_Code()
        {
            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitlistStatus_Constructor_CodeNull()
        {
            result = new WaitlistStatusCode(null, desc, status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitlistStatus_Constructor_CodeEmpty()
        {
            result = new WaitlistStatusCode(string.Empty, desc, status);
        }

        [TestMethod]
        public void WaitlistStatus_Constructor_Description()
        {
            Assert.AreEqual(desc, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitlistStatus_Constructor_DescriptionNull()
        {
            result = new WaitlistStatusCode(code, null, status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitlistStatus_Constructor_DescriptionEmpty()
        {
            result = new WaitlistStatusCode(code, string.Empty, status);
        }

        [TestMethod]
        public void WaitlistStatus_Constructor_Status()
        {
            Assert.AreEqual(status, result.Status);
        }
    }
}
