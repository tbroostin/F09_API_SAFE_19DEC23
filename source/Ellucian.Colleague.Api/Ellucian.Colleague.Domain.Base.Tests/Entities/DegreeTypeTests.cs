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
    public class DegreeTypeTests
    {
        private string code;
        private string description;
        private DegreeType type;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            type = new DegreeType(code, description);
        }

        [TestClass]
        public class DegreeTypeConstructor : DegreeTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreeTypeConstructorNullCode()
            {
                type = new DegreeType(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreeTypeConstructorEmptyCode()
            {
                type = new DegreeType(string.Empty, description);
            }

            [TestMethod]
            public void DegreeTypeConstructorValidCode()
            {
                type = new DegreeType(code, description);
                Assert.AreEqual(code, type.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreeTypeConstructorNullDescription()
            {
                type = new DegreeType(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DegreeTypeConstructorEmptyDescription()
            {
                type = new DegreeType(code, string.Empty);
            }

            [TestMethod]
            public void DegreeTypeConstructorValidDescription()
            {
                type = new DegreeType(code, description);
                Assert.AreEqual(description, type.Description);
            }
        }
    }
}
