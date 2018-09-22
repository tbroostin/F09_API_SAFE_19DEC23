using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SessionTests
    {
        [TestClass]
        public class SessionConstructor
        {
            private Session s;

            [TestInitialize]
            public void Initialize()
            {
                s = new Session("Fall");
            }

            [TestCleanup]
            public void CleanUp()
            {
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SessionConstructor_ExceptionIfNullName()
            {
                new Session(null);
            }

            [TestMethod]
            public void SessionName()
            {
                Assert.AreEqual("Fall", s.Name);
            }

        }

        [TestClass]
        public class SessionEquals
        {
            private Session s1;
            private Session s2;
            private Session s3;
            [TestInitialize]
            public void Initialize()
            {
                s1 = new Session("Fall");
                s2 = new Session("Fall");
                s3 = new Session("Spring");
            }
            [TestMethod]
            public void SessionSameNameEqual()
            {
                Assert.IsTrue(s1.Equals(s2));
            }
            [TestMethod]
            public void SessionDifferentNameNotEqual()
            {
                Assert.IsFalse(s1.Equals(s3));
            }
            [TestMethod]
            public void SessionSameNameHashEqual()
            {
                Assert.AreEqual(s1.GetHashCode(), s2.GetHashCode());
            }
            [TestMethod]
            public void SessionDifferentNameHashNotEqual()
            {
                Assert.AreNotEqual(s1.GetHashCode(), s3.GetHashCode());
            }
        }
    }
}
