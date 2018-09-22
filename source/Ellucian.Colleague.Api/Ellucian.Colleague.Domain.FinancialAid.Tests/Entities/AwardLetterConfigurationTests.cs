//Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Test class for AwardLetterConfiguration entity
    /// </summary>
    [TestClass]
    public class AwardLetterConfigurationTests
    {
        private AwardLetterConfiguration awardLetterConfiguration;
        private string id;
        private bool isNeedBlockActive;
        private bool isContactBlockActive;
        private bool isHousingBlockActive;
        private string paragraphSpacing;
        private string awardTableTitle;
        private string awardTotalTitle;


        [TestInitialize]
        public void Initialize()
        {
            id = "foo";
            isNeedBlockActive = true;
            isContactBlockActive = true;
            isHousingBlockActive = true;
            paragraphSpacing = "2";
            awardTableTitle = "Awards";
            awardTotalTitle = "Total amount";

            awardLetterConfiguration = new AwardLetterConfiguration(id);
        }

        [TestCleanup]
        public void Cleanup()
        {
            awardLetterConfiguration = null;
        }

        /// <summary>
        /// Constructor test
        /// </summary>
        [TestMethod]
        public void AwardLetterConfigurationInitializedTest()
        {
            Assert.IsNotNull(awardLetterConfiguration);
            Assert.IsNotNull(awardLetterConfiguration.Id);
            Assert.AreEqual(id, awardLetterConfiguration.Id);
            Assert.IsFalse(awardLetterConfiguration.IsContactBlockActive);
            Assert.IsFalse(awardLetterConfiguration.IsHousingBlockActive);
            Assert.IsFalse(awardLetterConfiguration.IsNeedBlockActive);
            Assert.IsNull(awardLetterConfiguration.ParagraphSpacing);
            Assert.IsNull(awardLetterConfiguration.AwardTableTitle);
            Assert.IsNull(awardLetterConfiguration.AwardTotalTitle);
            Assert.IsNotNull(awardLetterConfiguration.AwardCategoriesGroups);
            Assert.IsNotNull(awardLetterConfiguration.AwardPeriodsGroups);
        }

        /// <summary>
        /// Constructor test with null id argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullIdPassed_ExceptionThrownTest()
        {
            awardLetterConfiguration = new AwardLetterConfiguration(null);
        }

        /// <summary>
        /// Constructor test with empty string id argument
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyStringIdPassed_ExceptionThrownTest()
        {
            awardLetterConfiguration = new AwardLetterConfiguration(string.Empty);
        }

        /// <summary>
        /// Tests if IsContactBlockActive gets set to the expected value and can be accessed
        /// </summary>
        [TestMethod]
        public void IsContactBlockActive_GetSetTest()
        {
            awardLetterConfiguration.IsContactBlockActive = isContactBlockActive;
            Assert.AreEqual(isContactBlockActive, awardLetterConfiguration.IsContactBlockActive);
        }

        /// <summary>
        /// Tests if IsNeedBlockActive gets set to the expected value and can be accessed
        /// </summary>
        [TestMethod]
        public void IsNeedBlockActive_GetSetTest()
        {
            awardLetterConfiguration.IsNeedBlockActive = isNeedBlockActive;
            Assert.AreEqual(isNeedBlockActive, awardLetterConfiguration.IsNeedBlockActive);
        }

        /// <summary>
        /// Tests if IsHousingBlockSctive gets set to the expected value and can be accessed
        /// </summary>
        [TestMethod]
        public void IsHousingBlockActive_GetSetTest()
        {
            awardLetterConfiguration.IsHousingBlockActive = isHousingBlockActive;
            Assert.AreEqual(isHousingBlockActive, awardLetterConfiguration.IsHousingBlockActive);
        }

        /// <summary>
        /// Tests if ParagraphSpacing gets set to the expected value and can be accessed
        /// </summary>
        [TestMethod]
        public void ParagraphSpacing_GetSetTest()
        {
            awardLetterConfiguration.ParagraphSpacing = paragraphSpacing;
            Assert.AreEqual(paragraphSpacing, awardLetterConfiguration.ParagraphSpacing);
        }

        /// <summary>
        /// Tests if AwardTableTitle gets set to the correct value and can be accessed
        /// </summary>
        [TestMethod]
        public void AwardTableTitle_GetSetTest()
        {
            awardLetterConfiguration.AwardTableTitle = awardTableTitle;
            Assert.AreEqual(awardTableTitle, awardLetterConfiguration.AwardTableTitle);
        }

        /// <summary>
        /// Tests if AwardTotalTitle gets set to the expected value and can be accessed
        /// </summary>
        [TestMethod]
        public void AwardTotalTitle_GetSetTest()
        {
            awardLetterConfiguration.AwardTotalTitle = awardTotalTitle;
            Assert.AreEqual(awardTotalTitle, awardLetterConfiguration.AwardTotalTitle);
        }

        /// <summary>
        /// Tests if AddAwardCategoryGroup method adds an AwardCategoryGroup to the collection, and that 
        /// the added group properties match expected ones
        /// </summary>
        [TestMethod]
        public void AddAwardCategoryGroup_AddsAwardCategoryGroupTest()
        {
            Assert.IsFalse(awardLetterConfiguration.AwardCategoriesGroups.Any());
            awardLetterConfiguration.AddAwardCategoryGroup("cat1", 5, GroupType.AwardCategories);

            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Count == 1);
            Assert.AreEqual("cat1", awardLetterConfiguration.AwardCategoriesGroups.First().GroupName);
            Assert.AreEqual(5, awardLetterConfiguration.AwardCategoriesGroups.First().GroupNumber);
            Assert.AreEqual(GroupType.AwardCategories, awardLetterConfiguration.AwardCategoriesGroups.First().GroupType);
        }

        /// <summary>
        /// Tests if AddAwardCategoryGroup method throws an ArgumentException if the provided groupNumber is
        /// invalid (lt 0)
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentException))]
        public void AddAwardCategoryGroup_InvalidGroupNumberThrowsExceptionTest()
        {
            awardLetterConfiguration.AddAwardCategoryGroup(string.Empty, -1, GroupType.AwardCategories);
        }

        /// <summary>
        /// Tests if AddAwardCategoryGroup throws an ArgumentException if invalid
        /// GroupType is provided
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddAwardCategoryGroup_InvalidGroupTypeThrowsExceptionTest()
        {
            awardLetterConfiguration.AddAwardCategoryGroup(string.Empty, 1, GroupType.AwardPeriodColumn);
        }

        /// <summary>
        /// Tests if attempt to add another award category group with same group number
        /// does not result in the addition of the group and returns false
        /// </summary>
        [TestMethod]
        public void AddAwardCategoryGroup_ExistentGroupNumberReturnsFalseTest()
        {
            awardLetterConfiguration.AddAwardCategoryGroup("award category", 1, GroupType.AwardCategories);
            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Count == 1);

            Assert.IsFalse(awardLetterConfiguration.AddAwardCategoryGroup("award category1", 1, GroupType.AwardCategories));
            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Count == 1);
        }

        /// <summary>
        /// Tests if AddAwardPeriodColumnGroupMethod adds an award period
        /// column group to the collection and that the added group's properties match the expected ones
        /// </summary>
        [TestMethod]
        public void AddAwardPeriodColumnGroup_AddsAwardPeriodColumnGroupTest()
        {
            Assert.IsFalse(awardLetterConfiguration.AwardPeriodsGroups.Any());
            awardLetterConfiguration.AddAwardPeriodColumnGroup("period1", 10, GroupType.AwardPeriodColumn);

            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Count == 1);
            Assert.AreEqual("period1", awardLetterConfiguration.AwardPeriodsGroups.First().GroupName);
            Assert.AreEqual(10, awardLetterConfiguration.AwardPeriodsGroups.First().GroupNumber);
            Assert.AreEqual(GroupType.AwardPeriodColumn, awardLetterConfiguration.AwardPeriodsGroups.First().GroupType);
        }

        /// <summary>
        /// Tests if AddAwardPeriodColumnGroup throws an ArgumentException if invalid group number
        /// is provided (lt 0)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddAwardPeriodColumnGroup_InvalidGroupNumberThrowsExceptionTest()
        {
            awardLetterConfiguration.AddAwardPeriodColumnGroup(string.Empty, -1, GroupType.AwardPeriodColumn);
        }

        /// <summary>
        /// Tests if AddAwardPeriodColumnGroup method throws an ArgumentException if an invalid GroupType
        /// is passed in
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddAwardPeriodColumnGroup_InvalidGroupTypeThrowsExceptionTest()
        {
            awardLetterConfiguration.AddAwardPeriodColumnGroup(string.Empty, 1, GroupType.AwardCategories);
        }

        /// <summary>
        /// Tests if attempt to add another award period column group with same group number
        /// does not result in the addition of the group and returns false
        /// </summary>
        [TestMethod]
        public void AddAwardPeriodColumnGroup_ExistentGroupNumberReturnsFalseTest()
        {
            awardLetterConfiguration.AddAwardPeriodColumnGroup("awardPeriod", 1, GroupType.AwardPeriodColumn);
            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Count == 1);

            Assert.IsFalse(awardLetterConfiguration.AddAwardPeriodColumnGroup("anotherAwardPeriod", 1, GroupType.AwardPeriodColumn));
            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Count == 1);
        }

        /// <summary>
        /// Tests if RemoveAwardCategoryGroup removes the group with the specified group number
        /// from the collection and returns true
        /// </summary>
        [TestMethod]
        public void RemoveAwardCategoryGroup_RemovesAwardCategoryGroupTest()
        {
            awardLetterConfiguration.AddAwardCategoryGroup("foo", 2, GroupType.AwardCategories);
            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Any());

            Assert.IsTrue(awardLetterConfiguration.RemoveAwardCategoryGroup(2));
            Assert.IsFalse(awardLetterConfiguration.AwardCategoriesGroups.Any());
        }

        /// <summary>
        /// Tests if RemoveAwardCategoryGroup method returns false and does not remove
        /// any groups from the collection if none of the groups in the collection has
        /// the specified group number
        /// </summary>
        [TestMethod]
        public void RemoveAwardCategoryGroup_InvalidGroupReturnsFalseTest()
        {
            awardLetterConfiguration.AddAwardCategoryGroup("foo", 2, GroupType.AwardCategories);
            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Count == 1);

            Assert.IsFalse(awardLetterConfiguration.RemoveAwardCategoryGroup(0));
            Assert.IsTrue(awardLetterConfiguration.AwardCategoriesGroups.Count == 1);
        }

        /// <summary>
        /// Tests if RemoveAwardCategoryGroup returns false if there are no groups to remove from
        /// </summary>
        [TestMethod]
        public void RemoveAwardCategoryGroup_NoGroupsAvailableReturnsFalseTest()
        {
            Assert.IsFalse(awardLetterConfiguration.AwardCategoriesGroups.Any());
            Assert.IsFalse(awardLetterConfiguration.RemoveAwardCategoryGroup(0));
        }

        /// <summary>
        /// Tests if RemoveAwardPeriodColumnGroupMethod removes an award period column group
        /// from the colection and returns true
        /// </summary>
        [TestMethod]
        public void RemoveAwardPeriodColumnGroup_RemovesAwardPeriodColumnGroupTest()
        {
            awardLetterConfiguration.AddAwardPeriodColumnGroup("foo", 2, GroupType.AwardPeriodColumn);
            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Any());

            Assert.IsTrue(awardLetterConfiguration.RemoveAwardPeriodColumnGroup(2));
            Assert.IsFalse(awardLetterConfiguration.AwardPeriodsGroups.Any());
        }

        /// <summary>
        /// Tests if RemoveAwardPeriodColumnGroup method returns false and does not remove any groups
        /// if the provided group number does not match any in the collection
        /// </summary>
        [TestMethod]
        public void RemoveAwardPeriodColumnGroup_InvalidGroupReturnsFalseTest()
        {
            awardLetterConfiguration.AddAwardPeriodColumnGroup("foo", 2, GroupType.AwardPeriodColumn);
            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Count == 1);

            Assert.IsFalse(awardLetterConfiguration.RemoveAwardPeriodColumnGroup(0));
            Assert.IsTrue(awardLetterConfiguration.AwardPeriodsGroups.Count == 1);
        }

        /// <summary>
        /// Tests if RemoveAwardPeriodColumnGroup method returns false if there ar no groups to
        /// remove from
        /// </summary>
        [TestMethod]
        public void RemoveAwardPeriodColumnGroup_NoGroupsAvailableReturnsFalseTest()
        {
            Assert.IsFalse(awardLetterConfiguration.AwardPeriodsGroups.Any());
            Assert.IsFalse(awardLetterConfiguration.RemoveAwardPeriodColumnGroup(0));
        }
        
    }
}
