//Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentNsldsInformationTests
    {
        private string studentId;
        private decimal pellUsedAmount;

        private StudentNsldsInformation studentNsldsInfo;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0004791";
            pellUsedAmount = 2500;

            studentNsldsInfo = new StudentNsldsInformation(studentId, pellUsedAmount);
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(studentNsldsInfo);
        }

        [TestMethod]
        public void StudentId_EqualsExpectedTest()
        {
            Assert.AreEqual(studentId, studentNsldsInfo.StudentId);
        }

        [TestMethod]
        public void PellUsedAmount_EqualsExpectedTest()
        {
            Assert.AreEqual(pellUsedAmount, studentNsldsInfo.PellLifetimeEligibilityUsedPercentage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullStudentId_ArgumentNullExceptionThrownTest()
        {
            new StudentNsldsInformation(null, pellUsedAmount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NegativeUsedPellAmount_ThrowsArgumentExceptionTest()
        {
            new StudentNsldsInformation(studentId, -7);
        }

        [TestMethod]
        public void NullPellUsedAmount_NoExceptionThrownTest()
        {
            bool exceptionThrown = false;
            try
            {
                new StudentNsldsInformation(studentId, null);
            }
            catch { exceptionThrown = true; }
            Assert.IsFalse(exceptionThrown);
        }
    }
}
