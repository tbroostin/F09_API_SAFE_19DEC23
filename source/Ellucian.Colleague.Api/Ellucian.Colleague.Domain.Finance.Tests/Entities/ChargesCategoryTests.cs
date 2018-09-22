// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class ChargesCategoryTests
    {
        static ChargesCategory cc = new ChargesCategory();

        [TestMethod]
        public void ChargesCategory_FeeGroups()
        {
            CollectionAssert.AreEqual(new List<FeeType>(), cc.FeeGroups);
        }

        [TestMethod]
        public void ChargesCategory_TuitionBySectionGroups()
        {
            CollectionAssert.AreEqual(new List<TuitionBySectionType>(), cc.TuitionBySectionGroups);
        }

        [TestMethod]
        public void ChargesCategory_OtherGroups()
        {
            CollectionAssert.AreEqual(new List<OtherType>(), cc.OtherGroups);
        }

        [TestMethod]
        public void ChargesCategory_Miscellaneous()
        {
            Assert.AreEqual(new OtherType().DisplayOrder, cc.Miscellaneous.DisplayOrder);
            Assert.AreEqual(new OtherType().Name, cc.Miscellaneous.Name);
            CollectionAssert.AreEqual(new OtherType().OtherCharges, cc.Miscellaneous.OtherCharges);
        }

        [TestMethod]
        public void ChargesCategory_RoomAndBoardGroups()
        {
            CollectionAssert.AreEqual(new List<RoomAndBoardType>(), cc.RoomAndBoardGroups);
        }

        [TestMethod]
        public void ChargesCategory_TuitionByTotalGroups()
        {
            CollectionAssert.AreEqual(new List<TuitionByTotalType>(), cc.TuitionByTotalGroups);
        }
    }
}
