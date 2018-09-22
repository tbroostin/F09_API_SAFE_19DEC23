// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonFilterTests
    {
        private string guid;
        private string code;
        private string description;
        private PersonFilter personFilter;
       

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "CL";
            description = "Cum Laude";
        }

        [TestClass]
        public class PersonFilterConstructor : PersonFilterTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorNullGuid()
            {
                personFilter = new PersonFilter(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorEmptyGuid()
            {
                personFilter = new PersonFilter(string.Empty, code, description);
            }

            [TestMethod]
            public void PersonFilterConstructorValidGuid()
            {
                personFilter = new PersonFilter(guid, code, description);
                Assert.AreEqual(guid, personFilter.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorNullCode()
            {
                personFilter = new PersonFilter(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorEmptyCode()
            {
                personFilter = new PersonFilter(guid, string.Empty, description);
            }

            [TestMethod]
            public void PersonFilterConstructorValidCode()
            {
                personFilter = new PersonFilter(guid, code, description);
                Assert.AreEqual(code, personFilter.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorNullDescription()
            {
                personFilter = new PersonFilter(guid, code, null);
            }       

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonFilterConstructorEmptyDescription()
            {
                personFilter = new PersonFilter(guid, code, string.Empty);
            }

            [TestMethod]
            public void PersonFilterConstructorValidDescription()
            {
                personFilter = new PersonFilter(guid, code, description);
                Assert.AreEqual(description, personFilter.Description);
            }
           
        }
    }
}
