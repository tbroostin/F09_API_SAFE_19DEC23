// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SubTestResultTests
    {
        [TestClass]
        public class SubTestResult_Constructor
        {
            private string code;
            private string desc;
            private DateTime taken;
            private SubTestResult subTestRes;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                taken = DateTime.Today.AddDays(-7);
                subTestRes = new SubTestResult(code, desc, taken);
                subTestRes.Score = 4.3334m;
            }

            [TestMethod]
            public void SubTestResult_Code()
            {
                Assert.AreEqual(code, subTestRes.Code);
            }

            [TestMethod]
            public void SubTestResult_Description()
            {
                Assert.AreEqual(desc, subTestRes.Description);
            }

            [TestMethod]
            public void SubTestResult_Score()
            {
                Assert.AreEqual(4.3334m, subTestRes.Score);
            }

            [TestMethod]
            public void SubTestResult_DateTaken()
            {
                Assert.AreEqual(taken, subTestRes.DateTaken);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubTestResult_CodeNullException()
            {
                new SubTestResult(null, desc, taken);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubTestResultCodeEmptyException()
            {
                new SubTestResult(string.Empty, desc, taken);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubTestResultDescEmptyException()
            {
                new SubTestResult(code, string.Empty, taken);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubTestResult_DescNullException()
            {
                new SubTestResult(code, null, taken);
            }

        }
    }
}