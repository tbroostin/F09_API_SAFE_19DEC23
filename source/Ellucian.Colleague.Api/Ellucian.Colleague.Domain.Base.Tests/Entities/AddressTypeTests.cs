// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class AddressTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private AddressTypeCategory addressTypeCategory;
        private AddressType2 addressType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "PER";
            description = "Personal";
            addressTypeCategory = AddressTypeCategory.Home;
        }

        [TestClass]
        public class AddressTypeConstructor : AddressTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorNullGuid()
            {
                addressType = new AddressType2(null, code, description, addressTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorEmptyGuid()
            {
                addressType = new AddressType2(string.Empty, code, description, addressTypeCategory);
            }

            [TestMethod]
            public void AddressTypeConstructorValidGuid()
            {
                addressType = new AddressType2(guid, code, description, addressTypeCategory);
                Assert.AreEqual(guid, addressType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorNullCode()
            {
                addressType = new AddressType2(guid, null, description, addressTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorEmptyCode()
            {
                addressType = new AddressType2(guid, string.Empty, description, addressTypeCategory);
            }

            [TestMethod]
            public void AddressTypeConstructorValidCode()
            {
                addressType = new AddressType2(guid, code, description, addressTypeCategory);
                Assert.AreEqual(code, addressType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorNullDescription()
            {
                addressType = new AddressType2(guid, code, null, addressTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AddressTypeConstructorEmptyDescription()
            {
                addressType = new AddressType2(guid, code, string.Empty, addressTypeCategory);
            }

            [TestMethod]
            public void AddressTypeConstructorValidDescription()
            {
                addressType = new AddressType2(guid, code, description, addressTypeCategory);
                Assert.AreEqual(description, addressType.Description);
            }

            [TestMethod]
            public void AddressTypeConstructorValidAddressTypeCategory()
            {
                addressType = new AddressType2(guid, code, description, addressTypeCategory);
                Assert.AreEqual(addressTypeCategory, addressType.AddressTypeCategory);
            }
        }
    }
}
