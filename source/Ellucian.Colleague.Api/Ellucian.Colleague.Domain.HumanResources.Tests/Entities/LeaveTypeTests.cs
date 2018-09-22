//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class LeaveTypesTests
    {
        [TestClass]
        public class LeaveTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private LeaveType leaveTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                leaveTypes = new LeaveType(guid, code, desc);
            }

            [TestMethod]
            public void LeaveTypes_Code()
            {
                Assert.AreEqual(code, leaveTypes.Code);
            }

            [TestMethod]
            public void LeaveTypes_Description()
            {
                Assert.AreEqual(desc, leaveTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypes_GuidNullException()
            {
                new LeaveType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypes_CodeNullException()
            {
                new LeaveType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypes_DescNullException()
            {
                new LeaveType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypesGuidEmptyException()
            {
                new LeaveType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypesCodeEmptyException()
            {
                new LeaveType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveTypesDescEmptyException()
            {
                new LeaveType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class LeaveTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private LeaveType leaveTypes1;
            private LeaveType leaveTypes2;
            private LeaveType leaveTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                leaveTypes1 = new LeaveType(guid, code, desc);
                leaveTypes2 = new LeaveType(guid, code, "Second Year");
                leaveTypes3 = new LeaveType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void LeaveTypesSameCodesEqual()
            {
                Assert.IsTrue(leaveTypes1.Equals(leaveTypes2));
            }

            [TestMethod]
            public void LeaveTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(leaveTypes1.Equals(leaveTypes3));
            }
        }

        [TestClass]
        public class LeaveTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private LeaveType leaveTypes1;
            private LeaveType leaveTypes2;
            private LeaveType leaveTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                leaveTypes1 = new LeaveType(guid, code, desc);
                leaveTypes2 = new LeaveType(guid, code, "Second Year");
                leaveTypes3 = new LeaveType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void LeaveTypesSameCodeHashEqual()
            {
                Assert.AreEqual(leaveTypes1.GetHashCode(), leaveTypes2.GetHashCode());
            }

            [TestMethod]
            public void LeaveTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(leaveTypes1.GetHashCode(), leaveTypes3.GetHashCode());
            }
        }
    }
}
