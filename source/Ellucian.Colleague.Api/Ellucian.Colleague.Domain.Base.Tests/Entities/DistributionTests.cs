// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class DistributionTests
    {
        private string code, description;
        private Distribution result;

        [TestInitialize]
        public void Initialize()
        {
            code = "BANK";
            description = "Main bank account";
        }

        [TestMethod]
        public void Distribution_Constructor_CodeValid()
        {
            result = new Distribution(code, description);
            Assert.AreEqual(code, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distribution_Constructor_CodeNull()
        {
            result = new Distribution(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distribution_Constructor_CodeEmpty()
        {
            result = new Distribution(string.Empty, description);
        }

        [TestMethod]
        public void Distribution_Constructor_DescriptionValid()
        {
            result = new Distribution(code, description);
            Assert.AreEqual(description, result.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distribution_Constructor_DescriptionNull()
        {
            result = new Distribution(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Distribution_Constructor_DescriptionEmpty()
        {
            result = new Distribution(code, string.Empty);
        }
    }
}