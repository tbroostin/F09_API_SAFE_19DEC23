/*Copyright 2015 Ellucian Company L.P. and its affiliates*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class AwardLetterHistoryItemTests
    {
        private string id;
        private DateTime createdDate;
        private AwardLetterHistoryItem awardLetterHistoryItem;

        [TestInitialize]
        public void Initialize()
        {
            id = "235";
            createdDate = new DateTime(2015, 08, 09);
            awardLetterHistoryItem = new AwardLetterHistoryItem(id, createdDate);
        }

        [TestCleanup]
        public void Cleanup()
        {
            awardLetterHistoryItem = null;
        }

        [TestMethod]
        public void AwardLetterHistoryItemInitializedTest()
        {
            Assert.IsNotNull(awardLetterHistoryItem);
            Assert.IsNotNull(id);
            Assert.IsNotNull(createdDate);
        }
        
        [TestMethod]
        public void AwardLetterHistoryItemId_EqualsExpectedTest()
        {
            Assert.AreEqual(id, awardLetterHistoryItem.Id);
        }

        [TestMethod]
        public void AwardLetterHistoryItemCreatedDate_EqualsExpectedTest()
        {
            Assert.AreEqual(createdDate, awardLetterHistoryItem.CreatedDate);
        }

        [TestMethod]
        [ExpectedException (typeof(ArgumentNullException))]
        public void AwardLetterHistoryItemIdIsNull_ExceptionThrownTest()
        {
            awardLetterHistoryItem = new AwardLetterHistoryItem(null, createdDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AwardLetterHistoryItemCreatedDateIsNull_ExceptionThrownTest()
        {
            awardLetterHistoryItem = new AwardLetterHistoryItem(id, null);
        }
    }
}
