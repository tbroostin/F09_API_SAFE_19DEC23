// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonNameTypeItemTests
    {
        private string guid;
        private string code;
        private string description;
        private PersonNameType type;
        private PersonNameTypeItem personNameTypeItem;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "LEGAL";
            description = "Person's Legal Name";
            type = PersonNameType.Legal;
        }

        [TestClass]
        public class PersonNameTypeItemConstructor : PersonNameTypeItemTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorNullGuid()
            {
                personNameTypeItem = new PersonNameTypeItem(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorEmptyGuid()
            {
                personNameTypeItem = new PersonNameTypeItem(string.Empty, code, description, type);
            }

            [TestMethod]
            public void PersonNameTypeItemConstructorValidGuid()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, description, type);
                Assert.AreEqual(guid, personNameTypeItem.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorNullCode()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorEmptyCode()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void PersonNameTypeItemConstructorValidCode()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, description, type);
                Assert.AreEqual(code, personNameTypeItem.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorNullDescription()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonNameTypeItemConstructorEmptyDescription()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void PersonNameTypeItemConstructorValidDescription()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, description, type);
                Assert.AreEqual(description, personNameTypeItem.Description);
            }

            [TestMethod]
            public void PersonNameTypeItemConstructorValidType()
            {
                personNameTypeItem = new PersonNameTypeItem(guid, code, description, type);
                Assert.AreEqual(type, personNameTypeItem.Type);
            }
        }
    }
}