// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TestResultTests
    {
        TestResult testResult;
        DateTime today;
        TestType category = TestType.Admissions;
        
        [TestInitialize]
        public void Initialize()
        {
            today = DateTime.Today;
            testResult = new TestResult("0000304","1111", "ACT Math", today, category);
            testResult.Score = 3.0001m;
        }

        [TestMethod]
        public void Id()
        {
            Assert.AreEqual("0000304", testResult.StudentId);
        }

        [TestMethod]
        public void Code()
        {
            Assert.AreEqual("1111", testResult.Code);
        }

        [TestMethod]
        public void Description()
        {
            Assert.AreEqual("ACT Math", testResult.Description);
        }

        [TestMethod]
        public void Score()
        {
            Assert.AreEqual(3.0001m, testResult.Score);
        }

        [TestMethod]
        public void DateTaken()
        {
            Assert.AreEqual(today, testResult.DateTaken);
        }

        [TestMethod]
        public void TestCategoryTest()
        {
            Assert.AreEqual(category, testResult.Category);
        }

        [TestMethod]
        public void TestCategoryTest2()
        {
            var testResult2 = new TestResult("0000304", "1111", "ACT Math", today, TestType.Placement);
            Assert.AreEqual(TestType.Placement, testResult2.Category);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IdNullException()
        {
            TestResult newTestResult = new TestResult(null, "1111", "ACT Math", today, category);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CodeNullException()
        {
            TestResult newTestResult = new TestResult("0000304", null, "ACT Math", today, category);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DescriptionNullException()
        {
            TestResult newTestResult = new TestResult("0000304", "1111", null, today, category);
        }
    }
}
