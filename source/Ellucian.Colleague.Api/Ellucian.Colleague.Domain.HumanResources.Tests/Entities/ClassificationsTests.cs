//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResoures.Tests.Entities
{
    [TestClass]
    public class ClassificationsTests
    {
        [TestClass]
        public class ClassificationsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private Classifications positionClassifications;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                positionClassifications = new Classifications(guid, code, desc);
            }

            [TestMethod]
            public void Classifications_Code()
            {
                Assert.AreEqual(code, positionClassifications.Code);
            }

            [TestMethod]
            public void Classifications_Description()
            {
                Assert.AreEqual(desc, positionClassifications.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Classifications_GuidNullException()
            {
                new Classifications(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Classifications_CodeNullException()
            {
                new Classifications(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Classifications_DescNullException()
            {
                new Classifications(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ClassificationsGuidEmptyException()
            {
                new Classifications(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ClassificationsCodeEmptyException()
            {
                new Classifications(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ClassificationsDescEmptyException()
            {
                new Classifications(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class Classifications_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private Classifications positionClassifications1;
            private Classifications positionClassifications2;
            private Classifications positionClassifications3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                positionClassifications1 = new Classifications(guid, code, desc);
                positionClassifications2 = new Classifications(guid, code, "Second Year");
                positionClassifications3 = new Classifications(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ClassificationsSameCodesEqual()
            {
                Assert.IsTrue(positionClassifications1.Equals(positionClassifications2));
            }

            [TestMethod]
            public void ClassificationsDifferentCodeNotEqual()
            {
                Assert.IsFalse(positionClassifications1.Equals(positionClassifications3));
            }
        }

        [TestClass]
        public class Classifications_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private Classifications positionClassifications1;
            private Classifications positionClassifications2;
            private Classifications positionClassifications3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                positionClassifications1 = new Classifications(guid, code, desc);
                positionClassifications2 = new Classifications(guid, code, "Second Year");
                positionClassifications3 = new Classifications(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ClassificationsSameCodeHashEqual()
            {
                Assert.AreEqual(positionClassifications1.GetHashCode(), positionClassifications2.GetHashCode());
            }

            [TestMethod]
            public void ClassificationsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(positionClassifications1.GetHashCode(), positionClassifications3.GetHashCode());
            }
        }
    }
}