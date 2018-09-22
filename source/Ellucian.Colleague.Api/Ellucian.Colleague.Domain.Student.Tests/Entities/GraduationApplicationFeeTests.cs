// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class GraduationApplicationFeeTests
    {
        [TestClass]
        public class GraduationApplicationFee_Constructor
        {
            private string studentId;
            private string programCode;
            private decimal? amount;
            private string distribution;
            private GraduationApplicationFee appFee;

            [TestInitialize]
            public void Initialize()
            {
                studentId = "StudentId";
                programCode = "ProgramCode";
                amount = 25m;
                distribution = "DIST";
            }

            [TestMethod]
            public void GraduationApplicationFee_PropertiesUpdated()
            {
                appFee = new GraduationApplicationFee(studentId, programCode, amount, distribution);
                Assert.AreEqual(studentId, appFee.StudentId);
                Assert.AreEqual(programCode, appFee.ProgramCode);
                Assert.AreEqual(amount, appFee.Amount);
                Assert.AreEqual(distribution, appFee.PaymentDistributionCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationFee_NullStudentId()
            {
                appFee = new GraduationApplicationFee(null, programCode, amount, distribution);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationFee_EmptyStudentId()
            {
                appFee = new GraduationApplicationFee(string.Empty, programCode, amount, distribution);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationFee_NullProgramCode()
            {
                appFee = new GraduationApplicationFee(studentId, null, amount, distribution);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GraduationApplicationFee_EmptyProgramCode()
            {
                appFee = new GraduationApplicationFee(studentId, string.Empty, amount, distribution);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void GraduationApplicationFee_NegativeAmount()
            {
                appFee = new GraduationApplicationFee(studentId, programCode, -50m, distribution);
            }

            [TestMethod]
            public void GraduationApplicationFee_EmptyAmountAndDistributionSuccess()
            {
                appFee = new GraduationApplicationFee(studentId, programCode, null, null);
                Assert.AreEqual(studentId, appFee.StudentId);
                Assert.AreEqual(programCode, appFee.ProgramCode);
                Assert.IsNull(appFee.Amount);
                Assert.IsNull(appFee.PaymentDistributionCode);
            }
        }
    }
}
