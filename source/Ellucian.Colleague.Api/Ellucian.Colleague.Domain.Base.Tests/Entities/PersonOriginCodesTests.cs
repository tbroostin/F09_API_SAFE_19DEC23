//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonOriginCodesTests
    {
        [TestClass]
        public class PersonOriginCodesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private PersonOriginCodes personSources;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personSources = new PersonOriginCodes(guid, code, desc);
            }

            [TestMethod]
            public void PersonOriginCodes_Code()
            {
                Assert.AreEqual(code, personSources.Code);
            }

            [TestMethod]
            public void PersonOriginCodes_Description()
            {
                Assert.AreEqual(desc, personSources.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodes_GuidNullException()
            {
                new PersonOriginCodes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodes_CodeNullException()
            {
                new PersonOriginCodes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodes_DescNullException()
            {
                new PersonOriginCodes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodesGuidEmptyException()
            {
                new PersonOriginCodes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodesCodeEmptyException()
            {
                new PersonOriginCodes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonOriginCodesDescEmptyException()
            {
                new PersonOriginCodes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class PersonOriginCodes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private PersonOriginCodes personSources1;
            private PersonOriginCodes personSources2;
            private PersonOriginCodes personSources3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personSources1 = new PersonOriginCodes(guid, code, desc);
                personSources2 = new PersonOriginCodes(guid, code, "Second Year");
                personSources3 = new PersonOriginCodes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PersonOriginCodesSameCodesEqual()
            {
                Assert.IsTrue(personSources1.Equals(personSources2));
            }

            [TestMethod]
            public void PersonOriginCodesDifferentCodeNotEqual()
            {
                Assert.IsFalse(personSources1.Equals(personSources3));
            }
        }

        [TestClass]
        public class PersonOriginCodes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private PersonOriginCodes personSources1;
            private PersonOriginCodes personSources2;
            private PersonOriginCodes personSources3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                personSources1 = new PersonOriginCodes(guid, code, desc);
                personSources2 = new PersonOriginCodes(guid, code, "Second Year");
                personSources3 = new PersonOriginCodes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void PersonOriginCodesSameCodeHashEqual()
            {
                Assert.AreEqual(personSources1.GetHashCode(), personSources2.GetHashCode());
            }

            [TestMethod]
            public void PersonOriginCodesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(personSources1.GetHashCode(), personSources3.GetHashCode());
            }
        }
    }
}
