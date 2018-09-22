/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class StudentChecklistItemTests
    {
        public string code;
        public ChecklistItemControlStatus controlStatus;
        private string controlStatusCode;

        public StudentChecklistItem studentChecklistItem;

        public void StudentChecklistItemTestsInitialize()
        {
            code = "CODE";
            controlStatus = ChecklistItemControlStatus.RemovedFromChecklist;
            controlStatusCode = "Q";
        }

        /// <summary>
        /// Tests the controller that accepts checklist item's code and its control status as arguments
        /// </summary>
        [TestClass]
        public class StudentChecklistItemConstructorTests : StudentChecklistItemTests
        {
            [TestInitialize]
            public void Inititalize()
            {
                StudentChecklistItemTestsInitialize();
            }

            [TestMethod]
            public void CodeTest()
            {
                studentChecklistItem = new StudentChecklistItem(code, controlStatus);
                Assert.AreEqual(code, studentChecklistItem.Code);
            }

            [TestMethod]
            public void ControlStatusTest()
            {
                studentChecklistItem = new StudentChecklistItem(code, controlStatus);
                Assert.AreEqual(controlStatus, studentChecklistItem.ControlStatus);
            }

            [TestMethod]
            public void ControlStatusGetSetTest()
            {
                studentChecklistItem = new StudentChecklistItem(code, controlStatus);
                studentChecklistItem.ControlStatus = ChecklistItemControlStatus.CompletionRequired;
                Assert.AreEqual(ChecklistItemControlStatus.CompletionRequired, studentChecklistItem.ControlStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new StudentChecklistItem("", controlStatus);
            }
        }

        /// <summary>
        /// Tests the controller that accepts checklist item's code and its control status code as arguments
        /// </summary>
        [TestClass]
        public class StudentChecklistItemConstructorTests2: StudentChecklistItemTests{

            [TestInitialize]
            public void Initialize()
            {
                StudentChecklistItemTestsInitialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullChecklistItemCode_ThrowsArgumentNullExceptionTest()
            {
                new StudentChecklistItem(null, controlStatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoChecklistItemControlStatusCode_ArgumentNullExceptionThrownTest()
            {
                new StudentChecklistItem(code, string.Empty);
            }

            [TestMethod]
            public void StudentChecklistItem_CreatedWithExpectedPropertiesTest()
            {
                var checklist = new StudentChecklistItem(code, controlStatusCode);
                Assert.AreEqual(code, checklist.Code);
                Assert.AreEqual(controlStatusCode, checklist.ControlStatusCode);
            }
            
        }

        [TestClass]
        public class EqualsAndGetHashCodeTests : StudentChecklistItemTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StudentChecklistItemTestsInitialize();
                studentChecklistItem = new StudentChecklistItem(code, controlStatus);
            }

            [TestMethod]
            public void Equals_CodeEqualTest()
            {
                var test = new StudentChecklistItem(code, ChecklistItemControlStatus.CompletionRequired);
                Assert.AreEqual(test, studentChecklistItem);
            }

            [TestMethod]
            public void SameHashCode_CodeEqualTest()
            {
                var test = new StudentChecklistItem(code, ChecklistItemControlStatus.CompletionRequired);
                Assert.AreEqual(test.GetHashCode(), studentChecklistItem.GetHashCode());
            }

            [TestMethod]
            public void NotEquals_CodeNotEqualTest()
            {
                var test = new StudentChecklistItem("foo", controlStatus);
                Assert.AreNotEqual(test, studentChecklistItem);
            }

            [TestMethod]
            public void DiffHashCode_CodeNotEqualTest()
            {
                var test = new StudentChecklistItem("foo", controlStatus);
                Assert.AreNotEqual(test.GetHashCode(), studentChecklistItem.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_NullObjectTest()
            {
                Assert.IsFalse(studentChecklistItem.Equals(null));
            }

            [TestMethod]
            public void NotEqual_DiffTypeTest()
            {
                Assert.IsFalse(studentChecklistItem.Equals(controlStatus));
            }

            [TestMethod]
            public void ToStringMethod_ReturnsExpectedValueTest()
            {
                Assert.AreEqual(code, studentChecklistItem.ToString());
            }
        }
    }
}
