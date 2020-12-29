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
    public class IntgPersonEmerPhoneTypesTests
    {
        [TestClass]
        public class IntgPersonEmerPhoneTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "D";
                desc = "Daytime phone number";
                emergencyContactPhoneAvailabilities = new IntgPersonEmerPhoneTypes(guid, code, desc);
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypes_Code()
            {
                Assert.AreEqual(code, emergencyContactPhoneAvailabilities.Code);
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypes_Description()
            {
                Assert.AreEqual(desc, emergencyContactPhoneAvailabilities.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypes_GuidNullException()
            {
                new IntgPersonEmerPhoneTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypes_CodeNullException()
            {
                new IntgPersonEmerPhoneTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypes_DescNullException()
            {
                new IntgPersonEmerPhoneTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypesGuidEmptyException()
            {
                new IntgPersonEmerPhoneTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypesCodeEmptyException()
            {
                new IntgPersonEmerPhoneTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void IntgPersonEmerPhoneTypesDescEmptyException()
            {
                new IntgPersonEmerPhoneTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class IntgPersonEmerPhoneTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities1;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities2;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "D";
                desc = "Daytime phone number";
                emergencyContactPhoneAvailabilities1 = new IntgPersonEmerPhoneTypes(guid, code, desc);
                emergencyContactPhoneAvailabilities2 = new IntgPersonEmerPhoneTypes(guid, code, "Second Year");
                emergencyContactPhoneAvailabilities3 = new IntgPersonEmerPhoneTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypesSameCodesEqual()
            {
                Assert.IsTrue(emergencyContactPhoneAvailabilities1.Equals(emergencyContactPhoneAvailabilities2));
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(emergencyContactPhoneAvailabilities1.Equals(emergencyContactPhoneAvailabilities3));
            }
        }

        [TestClass]
        public class IntgPersonEmerPhoneTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities1;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities2;
            private IntgPersonEmerPhoneTypes emergencyContactPhoneAvailabilities3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "D";
                desc = "Daytime phone number";
                emergencyContactPhoneAvailabilities1 = new IntgPersonEmerPhoneTypes(guid, code, desc);
                emergencyContactPhoneAvailabilities2 = new IntgPersonEmerPhoneTypes(guid, code, "Second Year");
                emergencyContactPhoneAvailabilities3 = new IntgPersonEmerPhoneTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypesSameCodeHashEqual()
            {
                Assert.AreEqual(emergencyContactPhoneAvailabilities1.GetHashCode(), emergencyContactPhoneAvailabilities2.GetHashCode());
            }

            [TestMethod]
            public void IntgPersonEmerPhoneTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(emergencyContactPhoneAvailabilities1.GetHashCode(), emergencyContactPhoneAvailabilities3.GetHashCode());
            }
        }
    }
}
