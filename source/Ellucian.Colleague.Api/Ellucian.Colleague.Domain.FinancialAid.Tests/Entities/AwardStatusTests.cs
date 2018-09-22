using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    /// <summary>
    /// Tests the AwardStatus domain object - extending CodeItem
    /// </summary>
    [TestClass]
    public class AwardStatusTests
    {
        /// <summary>
        /// Tests the constructor of the AwardStatus class
        /// CodeItem tests apply in addition to extra Category attribute
        /// </summary>
        [TestClass]
        public class AwardStatusConstructor
        {
            private string code;
            private string description;
            private AwardStatusCategory category;

            private AwardStatus awardStatus;

            [TestInitialize]
            public void Initialize()
            {
                code = "A";
                description = "Accepted";
                category = AwardStatusCategory.Accepted;

                awardStatus = new AwardStatus(code, description, category);
            }

            /// <summary>
            /// Test that the constructor set the code correctly
            /// </summary>
            [TestMethod]
            public void AwardStatusCodeEqual()
            {
                Assert.AreEqual(code, awardStatus.Code);
            }

            /// <summary>
            /// Test that the constructor set the description correctly
            /// </summary>
            [TestMethod]
            public void AwardStatusDescriptionEqual()
            {
                Assert.AreEqual(description, awardStatus.Description);
            }

            /// <summary>
            /// Test that the constructor set the category correctly
            /// </summary>            
            [TestMethod]
            public void AwardStatusCategoryEqual()
            {
                Assert.AreEqual(category, awardStatus.Category);
            }

            /// <summary>
            /// Test that the constructor throws an exception when a null
            /// value is passed in for the code.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardStatusCodeNullException()
            {
                new AwardStatus(null, description, category);
            }

            /// <summary>
            /// Test that the constructor throws an exception when a null
            /// value is passed in for the description.
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardStatusDescriptionNullException()
            {
                new AwardStatus(code, null, category);
            }

            /// <summary>
            /// Test that the Public Accessor Get and Set methods work correctly.
            /// Get -> should return value of private attribute
            /// Set -> should sets value of private attribute to input value
            /// </summary>
            [TestMethod]
            public void AwardCategoryGetSet()
            {
                awardStatus.Category = AwardStatusCategory.Pending;
                Assert.AreEqual(awardStatus.Category, AwardStatusCategory.Pending);
            }
        }

        /// <summary>
        /// Tests the Equals method (inherited from CodeItem) of the AwardStatus class
        /// Two AwardStatuses are equal when their Codes are equal. 
        /// </summary>
        [TestClass]
        public class AwardStatusEquals
        {
            private string code1;
            private string desc1;
            private AwardStatusCategory cat1;

            private string code2;
            private string desc2;
            private AwardStatusCategory cat2;

            /// <summary>
            /// Test award status 1 is defined by code1, desc1, and cat1
            /// </summary>
            private AwardStatus awardStatus1;

            /// <summary>
            /// Test award status 2 has the same code, but different description than award status 1
            /// </summary>
            private AwardStatus awardStatus2;

            /// <summary>
            /// Test award status 3 has the same code, but different category as award status 1
            /// </summary>
            private AwardStatus awardStatus3;

            /// <summary>
            /// Test award status 4 has a different code, but same description and category as award status 1
            /// </summary>
            private AwardStatus awardStatus4;

            [TestInitialize]
            public void Initialize()
            {
                code1 = "A";
                desc1 = "Accepted";
                cat1 = AwardStatusCategory.Accepted;              

                code2 = "X";
                desc2 = "Reviewed";
                cat2 = AwardStatusCategory.Pending;

                awardStatus1 = new AwardStatus(code1, desc1, cat1);
                awardStatus2 = new AwardStatus(code1, desc2, cat1);
                awardStatus3 = new AwardStatus(code1, desc1, cat2);
                awardStatus4 = new AwardStatus(code2, desc1, cat1);
            }

            /// <summary>
            /// 
            /// </summary>
            [TestMethod]
            public void AwardStatusSameCodeEqual()
            {
                Assert.IsTrue(awardStatus1.Equals(awardStatus2));
                Assert.IsTrue(awardStatus2.Equals(awardStatus3));
            }

            /// <summary>
            /// 
            /// </summary>
            [TestMethod]
            public void AwardStatusDifferentCodesNotEqual()
            {
                Assert.IsFalse(awardStatus1.Equals(awardStatus4));
            }


        }
    }
}
