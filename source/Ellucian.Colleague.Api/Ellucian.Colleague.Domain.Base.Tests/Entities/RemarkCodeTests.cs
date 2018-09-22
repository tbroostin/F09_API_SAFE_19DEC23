// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RemarkCodeTests
    {
        private string guid;
        private string code;
        private string description;
        private RemarkCode remarkCode;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "FA";
            description = "Family";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorNullGuid()
        {
            remarkCode = new RemarkCode(null, code, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorEmptyGuid()
        {
            remarkCode = new RemarkCode(string.Empty, code, description);
        }

        [TestMethod]
        public void RemarkCodeConstructorValidGuid()
        {
            remarkCode = new RemarkCode(guid, code, description);
            Assert.AreEqual(guid, remarkCode.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorNullCode()
        {
            remarkCode = new RemarkCode(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorEmptyCode()
        {
            remarkCode = new RemarkCode(guid, string.Empty, description);
        }

        [TestMethod]
        public void RemarkCodeConstructorValidCode()
        {
            remarkCode = new RemarkCode(guid, code, description);
            Assert.AreEqual(code, remarkCode.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorNullDescription()
        {
            remarkCode = new RemarkCode(guid, code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemarkCodeConstructorEmptyDescription()
        {
            remarkCode = new RemarkCode(guid, code, string.Empty);
        }

        [TestMethod]
        public void RemarkCodeConstructorValidDescription()
        {
            remarkCode = new RemarkCode(guid, code, description);
            Assert.AreEqual(description, remarkCode.Description);
        }
    }
}