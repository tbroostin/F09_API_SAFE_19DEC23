/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentChecklistDomainServiceTests
    {
        public TestFinancialAidReferenceDataRepository referenceDataRepository;

        public void StudentChecklistDomainServiceTestsInitialize()
        {
            referenceDataRepository = new TestFinancialAidReferenceDataRepository();
        }

        [TestClass]
        public class BuildStudentFinancialAidChecklistTests : StudentChecklistDomainServiceTests
        {
            public string studentId;
            public string awardYear;
            public List<ChecklistItem> checklistItems;
            private Dictionary<string, string> officeChecklistItems;

            public StudentFinancialAidChecklist actualChecklist
            { get { return StudentChecklistDomainService.BuildStudentFinancialAidChecklist(studentId, awardYear, checklistItems, officeChecklistItems); } }

            [TestInitialize]
            public void Initialize()
            {
                StudentChecklistDomainServiceTestsInitialize();
                studentId = "0003914";
                awardYear = "2015";
                checklistItems = referenceDataRepository.ChecklistItems.ToList();
                officeChecklistItems = new Dictionary<string, string>();
                foreach(var item in checklistItems){
                    officeChecklistItems.Add(item.ChecklistItemCode, "P");
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                studentId = string.Empty;
                var test = actualChecklist;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                awardYear = string.Empty;
                var test = actualChecklist;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ItemsToAddRequiredTest()
            {
                checklistItems = null;
                var test = actualChecklist;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ItemsToAddRequiredValuesTest()
            {
                checklistItems = new List<ChecklistItem>();
                var test = actualChecklist;
            }

            [TestMethod]
            public void StudentIdTest()
            {
                Assert.AreEqual(studentId, actualChecklist.StudentId);
            }

            [TestMethod]
            public void AwardYearTest()
            {
                Assert.AreEqual(awardYear, actualChecklist.AwardYear);
            }

            [TestMethod]
            public void NumChecklistItemsTest()
            {
                Assert.AreEqual(checklistItems.Count, actualChecklist.ChecklistItems.Count);
            }

            [TestMethod]
            public void ItemsToAddAreSortedTest()
            {
                var orderedChecklistItems = checklistItems.OrderBy(c => c.ChecklistSortNumber).ToList();
                for (int i = 0; i < orderedChecklistItems.Count(); i++)
                {
                    Assert.AreEqual(orderedChecklistItems[i].ChecklistItemCode, actualChecklist.ChecklistItems[i].Code);
                }
            }

            [TestMethod]
            public void NullOfficeYearChecklistItems_NoChecklistItemsCretedTest()
            {
                officeChecklistItems = null;
                Assert.IsFalse(actualChecklist.ChecklistItems.Any());
            }

            [TestMethod]
            public void NoOfficeYearChecklistItemsPresent_NoChecklistItemsCretedTest()
            {
                officeChecklistItems = new Dictionary<string,string>();
                Assert.IsFalse(actualChecklist.ChecklistItems.Any());
            }
        }
    }
}
