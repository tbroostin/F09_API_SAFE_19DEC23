// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class IntentToWithdrawCodeTests
    {
        string id;
        string code;
        string description;

        [TestInitialize]
        public void IntentToWithdrawCodeTests_Initialize()
        {
            id = "1";
            code = "Yes";
            description = "I intent to withdraw from the institution.";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntentToWithdrawCode_null_id()
        {
            var entity = new IntentToWithdrawCode(null, code, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntentToWithdrawCode_null_code()
        {
            var entity = new IntentToWithdrawCode(id, null, description);
        }

        [TestMethod]
        public void IntentToWithdrawCode_valid()
        {
            var entity = new IntentToWithdrawCode(id, code, description);
            Assert.AreEqual(id, entity.Id);
            Assert.AreEqual(code, entity.Code);
            Assert.AreEqual(description, entity.Description);
        }
    }
}
