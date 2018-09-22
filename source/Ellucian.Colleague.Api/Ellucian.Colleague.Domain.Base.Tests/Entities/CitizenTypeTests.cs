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
    public class CitizenTypeTests
    {
        private string code;
        private string description;
        private CitizenType type;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            type = new CitizenType(code, description);
        }

        [TestClass]
        public class CitizenTypeConstructor : CitizenTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenTypeConstructorNullCode()
            {
                type = new CitizenType(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenTypeConstructorEmptyCode()
            {
                type = new CitizenType(string.Empty, description);
            }

            [TestMethod]
            public void CitizenTypeConstructorValidCode()
            {
                type = new CitizenType(code, description); 
                Assert.AreEqual(code, type.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenTypeConstructorNullDescription()
            {
                type = new CitizenType(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CitizenTypeConstructorEmptyDescription()
            {
                type = new CitizenType(code, string.Empty);
            }

            [TestMethod]
            public void CitizenTypeConstructorValidDescription()
            {
                type = new CitizenType(code, description); 
                Assert.AreEqual(description, type.Description);
            }
        }
    }
}
