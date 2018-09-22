using System;
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class RegistrationEligibilityTermTests
    {
        private string code;
        private DateTime? checkAgain;
        private RegistrationEligibilityTermStatus status;
        private string messages;


        private RegistrationEligibilityTerm regEliTerm;

        [TestInitialize]
        public void Initialize()
        {
            code = "2013/FA";
            checkAgain = DateTime.Today;
            status = RegistrationEligibilityTermStatus.Open;
            messages = "Messages";
            regEliTerm = new RegistrationEligibilityTerm(code, true, true);
            regEliTerm.AnticipatedTimeForAdds = checkAgain;
            regEliTerm.Status = status;
            regEliTerm.Message = messages;
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_HasCode()
        {
            Assert.AreEqual(code, regEliTerm.TermCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RegistrationEligibilityTerm_ThrowsExceptionIfCodeNull()
        {
            RegistrationEligibilityTerm regEligibilityTerm = new RegistrationEligibilityTerm(null, true, true);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_AddCheckAgainOnPresent()
        {
            Assert.AreEqual(checkAgain, regEliTerm.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_AddCheckAgainOnNullable()
        {
            RegistrationEligibilityTerm term = new RegistrationEligibilityTerm(code, true, true);
            Assert.AreEqual(null, term.AnticipatedTimeForAdds);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_CheckPriorityTrue()
        {
            RegistrationEligibilityTerm term = new RegistrationEligibilityTerm(code, true, true);
            Assert.IsTrue(term.CheckPriority);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_PriorityOverridableFalse()
        {
            RegistrationEligibilityTerm term = new RegistrationEligibilityTerm(code, true, false);
            Assert.IsFalse(term.PriorityOverridable);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_Equals_Equal()
        {
            RegistrationEligibilityTerm term1 = new RegistrationEligibilityTerm(code, true, false);
            RegistrationEligibilityTerm term2 = new RegistrationEligibilityTerm(code, true, false);
            var result = term1.Equals(term2);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_Equals_NotEqual()
        {
            RegistrationEligibilityTerm term1 = new RegistrationEligibilityTerm(code, true, false);
            RegistrationEligibilityTerm term2 = new RegistrationEligibilityTerm("not me", true, false);
            var result = term1.Equals(term2);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_Equals_NullObject_NotEqual()
        {
            RegistrationEligibilityTerm term1 = new RegistrationEligibilityTerm(code, true, false);
            RegistrationEligibilityTerm term2 = null;
            var result = term1.Equals(term2);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RegistrationEligibilityTerm_Equals_BadCast_NotEqual()
        {
            RegistrationEligibilityTerm term1 = new RegistrationEligibilityTerm(code, true, false);
            string term2 = "foo";
            var result = term1.Equals(term2);
            Assert.IsFalse(result);
        }
    }
}
