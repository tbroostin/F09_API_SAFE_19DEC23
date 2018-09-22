// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class JobChangeReasonTests
    {
        private string guid;
        private string code;
        private string description;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "code";
            description = "description";
        }

        [TestMethod]
        public void JobChangeReasonConstructorTest()
        {
            var jobChangeReason = new JobChangeReason(guid, code, description);
            Assert.AreEqual(code, jobChangeReason.Code);
            Assert.AreEqual(description, jobChangeReason.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JobChangeReasonConstructorNullCodeTest()
        {
            new JobChangeReason(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JobChangeReasonConstructorNullDescriptionTest()
        {
            new JobChangeReason(guid, code, null);
        }
    }
}
