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
    public class StudentFinancialAidChecklistTests
    {
        public string studentId;
        public string awardYear;
        public List<StudentChecklistItem> checklistItems;

        public StudentFinancialAidChecklist studentChecklist;

        public void StudentFinancialAidChecklistTestsInitialize()
        {
            studentId = "0003914";
            awardYear = "2015";
            checklistItems = new List<StudentChecklistItem>()
            {
                new StudentChecklistItem("DOC", ChecklistItemControlStatus.CompletionRequired),
                new StudentChecklistItem("LTR", ChecklistItemControlStatus.CompletionRequiredLater),
                new StudentChecklistItem("AWD", ChecklistItemControlStatus.RemovedFromChecklist)
            };
        }

        [TestClass]
        public class StudentFinancialAidChecklistConstructorTests : StudentFinancialAidChecklistTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StudentFinancialAidChecklistTestsInitialize();
            }

            [TestMethod]
            public void StudentIdTest()
            {
                studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear);
                Assert.AreEqual(studentId, studentChecklist.StudentId);
            }

            [TestMethod]
            public void AwardYearTest()
            {
                studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear);
                Assert.AreEqual(awardYear, studentChecklist.AwardYear);
            }

            [TestMethod]
            public void ChecklistItemsInitializedTest()
            {
                studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear);
                Assert.IsNotNull(studentChecklist.ChecklistItems);
                Assert.IsFalse(studentChecklist.ChecklistItems.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                new StudentFinancialAidChecklist(null, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                new StudentFinancialAidChecklist(studentId, null);
            }

            [TestMethod]
            public void ChecklistItemsGetSetTest()
            {
                studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear)
                {
                    ChecklistItems = checklistItems
                };
                CollectionAssert.AreEqual(checklistItems, studentChecklist.ChecklistItems);
            }           
        }

        [TestClass]
        public class EqualsAndGetHashCodeTests : StudentFinancialAidChecklistTests
        {
            [TestInitialize]
            public void Initialize()
            {
                StudentFinancialAidChecklistTestsInitialize();
                studentChecklist = new StudentFinancialAidChecklist(studentId, awardYear);
            }

            [TestMethod]
            public void Equal_StudentIdAndAwardYearEqualTest()
            {
                var test = new StudentFinancialAidChecklist(studentId, awardYear);
                Assert.AreEqual(test, studentChecklist);
            }

            [TestMethod]
            public void SameHashCode_StudentIdAndAwardYearEqualTest()
            {
                var test = new StudentFinancialAidChecklist(studentId, awardYear);
                Assert.AreEqual(test.GetHashCode(), studentChecklist.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_StudentIdNotEqualTest()
            {
                var test = new StudentFinancialAidChecklist("foobar", awardYear);
                Assert.AreNotEqual(test, studentChecklist);
            }

            [TestMethod]
            public void DiffHashCode_StudentIdNotEqualTest()
            {
                var test = new StudentFinancialAidChecklist("foobar", awardYear);
                Assert.AreNotEqual(test.GetHashCode(), studentChecklist.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_AwardYearNotEqualTest()
            {
                var test = new StudentFinancialAidChecklist(studentId, "foobar");
                Assert.AreNotEqual(test, studentChecklist);
            }

            [TestMethod]
            public void DiffHashCode_AwardYearNotEqualTest()
            {
                var test = new StudentFinancialAidChecklist(studentId, "foobar");
                Assert.AreNotEqual(test.GetHashCode(), studentChecklist.GetHashCode());
            }

            [TestMethod]
            public void NotEqual_NullObjectTest()
            {
                Assert.IsFalse(studentChecklist.Equals(null));
            }

            [TestMethod]
            public void NotEqual_DiffObjectTest()
            {
                Assert.IsFalse(studentChecklist.Equals(awardYear));
            }
        }
    }
}
