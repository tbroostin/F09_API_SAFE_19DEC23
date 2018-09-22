/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
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
    /// Test class for AwardLetterGroup2 entity
    /// </summary>
    [TestClass]
    public class AwardLetterGroup2Tests
    {
        private AwardLetterGroup2 awardLetteGroup;
        private string groupName;
        private int groupNumber;
        private GroupType groupType;

        [TestInitialize]
        public void Initialize()
        {
            groupName = "GroupName";
            groupNumber = 1;
            groupType = GroupType.AwardCategories;

            awardLetteGroup = new AwardLetterGroup2(groupName, groupNumber, groupType);
        }

        [TestCleanup]
        public void Cleanup()
        {
            awardLetteGroup = null;
        }

        /// <summary>
        /// Tests if award letter group object is initialized and 
        /// all its properties are not null
        /// </summary>
        [TestMethod]
        public void AwardLetterGroupInitializedTest()
        {
            Assert.IsNotNull(awardLetteGroup);
            Assert.IsNotNull(awardLetteGroup.GroupName);
            Assert.IsNotNull(awardLetteGroup.GroupNumber);
            Assert.IsNotNull(awardLetteGroup.GroupType);
        }

        /// <summary>
        /// Tests if group name was set to the expected value
        /// </summary>
        [TestMethod]
        public void GroupName_EqualsExpectedTest()
        {
            Assert.AreEqual(groupName, awardLetteGroup.GroupName);
        }

        /// <summary>
        /// Tests if group number was set to the expected value
        /// </summary>
        [TestMethod]
        public void GroupNumber_EqualsExpectedTest()
        {
            Assert.AreEqual(groupNumber, awardLetteGroup.GroupNumber);
        }

        /// <summary>
        /// Tests if group type was set to the expected value
        /// </summary>
        [TestMethod]
        public void GroupType_EqualsExpectedTest()
        {
            Assert.AreEqual(groupType, awardLetteGroup.GroupType);
        }

        /// <summary>
        /// Tests if the ArgumentException isthrown if the specified groupNumber
        /// is invalid (lt 0)
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentException))]
        public void InvalidGroupNumber_ThrowsExceptionTest()
        {
            new AwardLetterGroup2(string.Empty, -2, GroupType.AwardCategories);
        }
    }
}
