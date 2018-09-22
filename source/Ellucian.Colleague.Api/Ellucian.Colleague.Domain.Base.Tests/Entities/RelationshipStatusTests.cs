//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
    public class RelationshipStatusTests
    {
        [TestClass]
        public class RelationshipStatusConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private RelationshipStatus relationshipStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                relationshipStatuses = new RelationshipStatus(guid, code, desc);
            }

            [TestMethod]
            public void RelationshipStatus_Code()
            {
                Assert.AreEqual(code, relationshipStatuses.Code);
            }

            [TestMethod]
            public void RelationshipStatus_Description()
            {
                Assert.AreEqual(desc, relationshipStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatus_GuidNullException()
            {
                new RelationshipStatus(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatus_CodeNullException()
            {
                new RelationshipStatus(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatus_DescNullException()
            {
                new RelationshipStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatusGuidEmptyException()
            {
                new RelationshipStatus(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatusCodeEmptyException()
            {
                new RelationshipStatus(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RelationshipStatusDescEmptyException()
            {
                new RelationshipStatus(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class RelationshipStatus_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private RelationshipStatus relationshipStatuses1;
            private RelationshipStatus relationshipStatuses2;
            private RelationshipStatus relationshipStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                relationshipStatuses1 = new RelationshipStatus(guid, code, desc);
                relationshipStatuses2 = new RelationshipStatus(guid, code, "Second Year");
                relationshipStatuses3 = new RelationshipStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void RelationshipStatusSameCodesEqual()
            {
                Assert.IsTrue(relationshipStatuses1.Equals(relationshipStatuses2));
            }

            [TestMethod]
            public void RelationshipStatusDifferentCodeNotEqual()
            {
                Assert.IsFalse(relationshipStatuses1.Equals(relationshipStatuses3));
            }
        }

        [TestClass]
        public class RelationshipStatus_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private RelationshipStatus relationshipStatuses1;
            private RelationshipStatus relationshipStatuses2;
            private RelationshipStatus relationshipStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                relationshipStatuses1 = new RelationshipStatus(guid, code, desc);
                relationshipStatuses2 = new RelationshipStatus(guid, code, "Second Year");
                relationshipStatuses3 = new RelationshipStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void RelationshipStatusSameCodeHashEqual()
            {
                Assert.AreEqual(relationshipStatuses1.GetHashCode(), relationshipStatuses2.GetHashCode());
            }

            [TestMethod]
            public void RelationshipStatusDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(relationshipStatuses1.GetHashCode(), relationshipStatuses3.GetHashCode());
            }
        }
    }
}
