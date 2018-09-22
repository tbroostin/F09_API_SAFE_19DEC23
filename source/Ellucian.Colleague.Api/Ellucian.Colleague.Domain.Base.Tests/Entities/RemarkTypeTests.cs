// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RemarkTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private RemarkType remarkType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PER";
            description = "Personal";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorNullGuid()
        {
            remarkType = new RemarkType(null, code, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorEmptyGuid()
        {
            remarkType = new RemarkType(string.Empty, code, description);
        }

        [TestMethod]
        public void RemarkTypeConstructorValidGuid()
        {
            remarkType = new RemarkType(guid, code, description);
            Assert.AreEqual(guid, remarkType.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorNullCode()
        {
            remarkType = new RemarkType(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorEmptyCode()
        {
            remarkType = new RemarkType(guid, string.Empty, description);
        }

        [TestMethod]
        public void RemarkTypeConstructorValidCode()
        {
            remarkType = new RemarkType(guid, code, description);
            Assert.AreEqual(code, remarkType.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorNullDescription()
        {
            remarkType = new RemarkType(guid, code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkTypeConstructorEmptyDescription()
        {
            remarkType = new RemarkType(guid, code, string.Empty);
        }

        [TestMethod]
        public void RemarkTypeConstructorValidDescription()
        {
            remarkType = new RemarkType(guid, code, description);
            Assert.AreEqual(description, remarkType.Description);
        }
    }
}