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
    public class GeographicAreaTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private GeographicAreaTypeCategory type;
        private GeographicAreaType geographicAreaType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "GOV";
            description = "governmental";
            type = GeographicAreaTypeCategory.Governmental;
        }

        [TestClass]
        public class GeographicAreaTypeConstructor : GeographicAreaTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorNullGuid()
            {
                geographicAreaType = new GeographicAreaType(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorEmptyGuid()
            {
                geographicAreaType = new GeographicAreaType(string.Empty, code, description, type);
            }

            [TestMethod]
            public void GeographicAreaTypeConstructorValidGuid()
            {
                geographicAreaType = new GeographicAreaType(guid, code, description, type);
                Assert.AreEqual(guid, geographicAreaType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorNullCode()
            {
                geographicAreaType = new GeographicAreaType(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorEmptyCode()
            {
                geographicAreaType = new GeographicAreaType(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void GeographicAreaTypeConstructorValidCode()
            {
                geographicAreaType = new GeographicAreaType(guid, code, description, type);
                Assert.AreEqual(code, geographicAreaType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorNullDescription()
            {
                geographicAreaType = new GeographicAreaType(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GeographicAreaTypeConstructorEmptyDescription()
            {
                geographicAreaType = new GeographicAreaType(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void GeographicAreaTypeConstructorValidDescription()
            {
                geographicAreaType = new GeographicAreaType(guid, code, description, type);
                Assert.AreEqual(description, geographicAreaType.Description);
            }

            [TestMethod]
            public void GeographicAreaTypeConstructorValidType()
            {
                geographicAreaType = new GeographicAreaType(guid, code, description, type);
                Assert.AreEqual(type, geographicAreaType.GeographicAreaTypeCategory);
            }
        }
    }
}
