using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentLoanHistoryTests
    {
        [TestClass]
        public class StudentLoanHistoryConstructor
        {
            private string SchoolCode;
            private int TotalLoanAmount;

            private StudentLoanHistory studentLoanHistory;

            [TestInitialize]
            public void Initialize()
            {
                SchoolCode = "00391500";
                TotalLoanAmount = 6848;
             
                studentLoanHistory = new StudentLoanHistory(SchoolCode);
                studentLoanHistory.AddToTotalLoanAmount(TotalLoanAmount);
            }

            [TestMethod]
            public void SchoolCodeEqualsTest()
            {
                Assert.AreEqual(SchoolCode, studentLoanHistory.OpeId);
            }

            [TestMethod]
            public void TotalLoanAmountEqualsTest()
            {
                Assert.AreEqual(TotalLoanAmount, studentLoanHistory.TotalLoanAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SchoolCodeNullExceptionTest()
            {
                new StudentLoanHistory(null);
            }

        }
    }
}
