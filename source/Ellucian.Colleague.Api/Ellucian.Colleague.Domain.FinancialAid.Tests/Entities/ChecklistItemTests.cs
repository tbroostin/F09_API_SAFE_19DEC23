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
    public class ChecklistItemTests
    {
        public string code;
        public ChecklistItemType type;
        public int sortNumber;
        public string description;

        public ChecklistItem checklistItem;

        public void ChecklistItemTestsInitialize()
        {
            code = "CODE";
            type = ChecklistItemType.CompletedDocuments;
            sortNumber = 3;
            description = "Complete Documents Checklist Item";
        }

        [TestClass]
        public class ChecklistItemConstructorTests : ChecklistItemTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ChecklistItemTestsInitialize();
            }

            [TestMethod]
            public void CodeTest()
            {
                checklistItem = new ChecklistItem(code, sortNumber, description);
                Assert.AreEqual(code, checklistItem.ChecklistItemCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new ChecklistItem("", sortNumber, description);
            }

            [TestMethod]
            public void DescriptionTest()
            {
                checklistItem = new ChecklistItem(code, sortNumber, description);
                Assert.AreEqual(description, checklistItem.Description);
            }

            [TestMethod]
            public void DescriptionNotRequiredTest()
            {
                checklistItem = new ChecklistItem(code, sortNumber, "");
                Assert.AreEqual("", checklistItem.Description);
            }

            [TestMethod]
            public void SortNumberTest()
            {
                checklistItem = new ChecklistItem(code, sortNumber, description);
                Assert.AreEqual(sortNumber, checklistItem.ChecklistSortNumber);
            }

            [TestMethod]
            public void ItemTypeGetSetTest()
            {
                checklistItem = new ChecklistItem(code, sortNumber, description)
                {
                    ChecklistItemType = type
                };

                Assert.AreEqual(type, checklistItem.ChecklistItemType);

            }
        }

        [TestClass]
        public class EqualsAndGetHashCodeTests : ChecklistItemTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ChecklistItemTestsInitialize();
                checklistItem = new ChecklistItem(code, sortNumber, description)
                {
                    ChecklistItemType = type
                };
            }

            [TestMethod]
            public void Equal_CodeEqualTest()
            {
                var test = new ChecklistItem(code, -99, "bar");
                Assert.AreEqual(test, checklistItem);
            }

            [TestMethod]
            public void SameHashCode_CodeEqualTest()
            {
                var test = new ChecklistItem(code, -99, "bar");
                Assert.AreEqual(test.GetHashCode(), checklistItem.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_CodeNotEqualTest()
            {
                var test = new ChecklistItem("foo", -99, description);
                Assert.AreNotEqual(test, checklistItem);
            }

            [TestMethod]
            public void DiffHashCode_CodeNotEqualTest()
            {
                var test = new ChecklistItem("foo", -99, description);
                Assert.AreNotEqual(test.GetHashCode(), checklistItem.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_NullObjectTest()
            {
                Assert.IsFalse(checklistItem.Equals(null));
            }

            [TestMethod]
            public void NotEqual_DiffTypeTest()
            {
                Assert.IsFalse(checklistItem.Equals(ChecklistItemType.ApplicationReview));
            }
        }

    }
}
