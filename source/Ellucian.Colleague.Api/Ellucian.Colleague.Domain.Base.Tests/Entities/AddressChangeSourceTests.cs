// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AddressChangeSourceTests
    {
        private string guid;
        private string code;
        private string description;
        private AddressChangeSource addressChangeSource;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PER";
            description = "Personal";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorNullGuid()
        {
            addressChangeSource = new AddressChangeSource(null, code, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorEmptyGuid()
        {
            addressChangeSource = new AddressChangeSource(string.Empty, code, description);
        }

        [TestMethod]
        public void AddressChangeSourceConstructorValidGuid()
        {
            addressChangeSource = new AddressChangeSource(guid, code, description);
            Assert.AreEqual(guid, addressChangeSource.Guid);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorNullCode()
        {
            addressChangeSource = new AddressChangeSource(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorEmptyCode()
        {
            addressChangeSource = new AddressChangeSource(guid, string.Empty, description);
        }

        [TestMethod]
        public void AddressChangeSourceConstructorValidCode()
        {
            addressChangeSource = new AddressChangeSource(guid, code, description);
            Assert.AreEqual(code, addressChangeSource.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorNullDescription()
        {
            addressChangeSource = new AddressChangeSource(guid, code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddressChangeSourceConstructorEmptyDescription()
        {
            addressChangeSource = new AddressChangeSource(guid, code, string.Empty);
        }

        [TestMethod]
        public void AddressChangeSourceConstructorValidDescription()
        {
            addressChangeSource = new AddressChangeSource(guid, code, description);
            Assert.AreEqual(description, addressChangeSource.Description);
        }
    }
}