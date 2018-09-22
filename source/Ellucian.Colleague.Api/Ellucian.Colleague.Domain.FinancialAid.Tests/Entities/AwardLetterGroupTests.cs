/*Copyright 2014 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardLetterGroupTests
    {
        private string title;
        private int sequenceNumber;
        private GroupType groupType;

        private AwardLetterGroup awardLetterGroup;

        [TestInitialize]
        public void Initialize()
        {
            title = "Group1";
            sequenceNumber = 0;
            groupType = GroupType.AwardCategories;

            awardLetterGroup = new AwardLetterGroup(title, sequenceNumber, groupType);
        }

        /// <summary>
        /// Tests if we get a correct number of the class properties
        /// </summary>
        [TestMethod]
        public void NumberOfPropertiesTest_AwardLetterGroup()
        {
            var properties = typeof(AwardLetterGroup).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Assert.AreEqual(4, properties.Length);
        }

        /// <summary>
        /// Test if the title in the created group equals the passed one
        /// </summary>
        [TestMethod]
        public void TitleEqualsTest_AwardLetterGroup()
        {
            Assert.AreEqual(title, awardLetterGroup.Title);
        }

        /// <summary>
        /// Test if the sequence number in the created group equals the passed one
        /// </summary>
        [TestMethod]
        public void SequenceNumberEqualsTest_AwardLetterGroup()
        {
            Assert.AreEqual(sequenceNumber, awardLetterGroup.SequenceNumber);
        }

        /// <summary>
        /// Tests if the group type of the created group equals the one passed
        /// </summary>
        [TestMethod]
        public void GroupTypeEqualsTest_AwardLetterGroup()
        {
            Assert.AreEqual(groupType, awardLetterGroup.GroupType);
        }

        /// <summary>
        /// Tests if the members property is not null when the group is created
        /// </summary>
        [TestMethod]
        public void MembersPropertyIsNotNullTest_AwardLetterGroup()
        {
            Assert.IsNotNull(awardLetterGroup.Members);
        }

        /// <summary>
        /// Tests if the created members property is initialized as an empty list
        /// </summary>
        [TestMethod]
        public void MembersPropertyInitializedToEmptyListTest_AwardLetterGroup()
        {
            Assert.IsTrue(awardLetterGroup.Members.Count == 0);
        }

        /// <summary>
        /// Test if ArgumentNullException is thrown when passing the group title as null
        /// </summary>
        [TestMethod]
        public void NullTitleDoesNotThrowExceptionTest_AwardLetterGroup()
        {
            title = null;
            awardLetterGroup = new AwardLetterGroup(title, sequenceNumber, groupType);

            Assert.AreEqual(null, awardLetterGroup.Title);
        }

        /// <summary>
        /// Tets if ArgumentException is thrown when creating a group with a sequence number that is less
        /// than 0
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentException))]
        public void SequenceNumberLessThanZeroThrowsArgumentExceptionTest_AwardLetterGroup()
        {
            sequenceNumber = -1;
            new AwardLetterGroup(title, sequenceNumber, groupType);
        }

        /// <summary>
        /// Test if ArgumentNullException is thrown when trying to add a member - empty string
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void AddEmptyStringMemberTest_AwardLetterGroup()
        {
            awardLetterGroup.AddGroupMember("");
        }

        /// <summary>
        /// Test if ArgumentNullException is trown when trying to pass null to the method
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void AddNullMemberTest_AwardLetterGroup()
        {
            awardLetterGroup.AddGroupMember(null);
        }

        /// <summary>
        /// Test if a member gets added to the award letter group
        /// </summary>
        [TestMethod]
        public void AddMemberToGroupTest_AwardLetterGroup()
        {
            Assert.IsTrue(awardLetterGroup.AddGroupMember("member1"));
        }

        /// <summary>
        /// Test if a duplicate member does not get added to the group
        /// </summary>
        [TestMethod]
        public void AddDuplicateMemberToGroup_AwardLetterGroup()
        {
            awardLetterGroup.AddGroupMember("member2");
            Assert.IsFalse(awardLetterGroup.AddGroupMember("member2"));
        }

        /// <summary>
        /// Tests if passing null to the RemoveGroupMember method 
        /// throws an ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void RemoveNullMemberTest_AwardLetterGroup()
        {
            awardLetterGroup.RemoveGroupMember(null);
        }

        /// <summary>
        /// Tests if passing an empty string to the RemoveGroupMember
        /// throws an ArgumentNullException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveEmptyStringMemberTest_AwardLetterGroup()
        {
            awardLetterGroup.RemoveGroupMember("");
        }

        /// <summary>
        /// Tests if a member gets removed from the group
        /// </summary>
        [TestMethod]
        public void RemoveMemberFromGroupTest_AwardLetterGroup()
        {
            awardLetterGroup.AddGroupMember("member1");
            Assert.IsTrue(awardLetterGroup.RemoveGroupMember("member1"));
        }

        /// <summary>
        /// Tests if trying to remove a member from the group which does not
        /// contain that member returns false
        /// </summary>
        [TestMethod]
        public void RemoveMemberNotContainedInGroupTest_AwardLetterGroup()
        {
            Assert.IsFalse(awardLetterGroup.RemoveGroupMember("m3"));
        }
    }
}
