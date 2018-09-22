using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentLoanLimitationTests
    {
        [TestClass]
        public class StudentLoanLimiationConstructorTests
        {
            private string awardYear;
            private string studentId;

            private int subsidizedMaximumAmount;
            private int unsubsidizedMaximumAmount;
            private int gradPlusMaximumAmount;

            private StudentLoanLimitation studentLoanLimitation;

            [TestInitialize]
            public void Initialize()
            {
                awardYear = "2013";
                studentId = "0003914";
                subsidizedMaximumAmount = 2000;
                unsubsidizedMaximumAmount = 7500;
                gradPlusMaximumAmount = 3500;

                studentLoanLimitation = new StudentLoanLimitation(awardYear, studentId);
                studentLoanLimitation.SubsidizedMaximumAmount = subsidizedMaximumAmount;
                studentLoanLimitation.UnsubsidizedMaximumAmount = unsubsidizedMaximumAmount;
                studentLoanLimitation.GradPlusMaximumAmount = gradPlusMaximumAmount;
            }

            [TestMethod]
            public void AwardYearEqualTest()
            {
                Assert.AreEqual(awardYear, studentLoanLimitation.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearNullExceptionTest()
            {
                new StudentLoanLimitation(null, studentId);
            }

            [TestMethod]
            public void StudentIdEqualTest()
            {
                Assert.AreEqual(studentId, studentLoanLimitation.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdNullExceptionTest()
            {
                new StudentLoanLimitation(awardYear, null);
            }

            [TestMethod]
            public void SubMaxAmountEqualTest()
            {
                Assert.AreEqual(subsidizedMaximumAmount, studentLoanLimitation.SubsidizedMaximumAmount);
            }

            [TestMethod]
            public void UnsubMaxAmountEqualTest()
            {
                Assert.AreEqual(unsubsidizedMaximumAmount, studentLoanLimitation.UnsubsidizedMaximumAmount);
            }

            [TestMethod]
            public void GradPlusMaxAmountEqualTest()
            {
                Assert.AreEqual(gradPlusMaximumAmount, studentLoanLimitation.GradPlusMaximumAmount);
            }
        }
    }
}
