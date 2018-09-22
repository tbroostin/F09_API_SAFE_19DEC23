using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TopicCodeTests
    {
        [TestClass]
        public class TopicCodeConstructor
        {
            private string code;
            private string desc;
            private string guid;

            private TopicCode topicCode;

            [TestInitialize]
            public void Initialize()
            {
                code = "TC";
                desc = "Topic Code";
                guid = "00000000-0000-0000-0000-000000000001";
                topicCode = new TopicCode(guid, code, desc);
            }

            [TestMethod]
            public void TopicCodeCode()
            {
                Assert.AreEqual(code, topicCode.Code);
            }

            [TestMethod]
            public void TopicCodeDescription()
            {
                Assert.AreEqual(desc, topicCode.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TopicCodeCodeNullException()
            {
                new TopicCode(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TopicCodeDescNullException()
            {
                new TopicCode(guid, code, null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TopicCodeGuidNullException()
            {
                new TopicCode(null, code, desc);
            }
        }

        [TestClass]
        public class TopicCodeEquals
        {
            private string code;
            private string desc;
            private string guid;
            private TopicCode topicCode1;
            private TopicCode topicCode2;
            private TopicCode topicCode3;

            [TestInitialize]
            public void Initialize()
            {
                code = "TC";
                desc = "Topic Code";
                guid = "00000000-0000-0000-0000-000000000000";
                topicCode1 = new TopicCode(guid, code, desc);
                topicCode2 = new TopicCode(guid, code, "Topic Code2");
                topicCode3 = new TopicCode(guid, "TC2", desc);
            }

            [TestMethod]
            public void TopicCodeSameCodesEqual()
            {
                Assert.IsTrue(topicCode1.Equals(topicCode2));
            }

            [TestMethod]
            public void TopicCodeDifferentCodeNotEqual()
            {
                Assert.IsFalse(topicCode1.Equals(topicCode3));
            }
        }

        [TestClass]
        public class TopicCodeGetHashCode
        {
            private string code;
            private string desc;
            private string guid;
            private TopicCode topicCode1;
            private TopicCode topicCode2;
            private TopicCode topicCode3;

            [TestInitialize]
            public void Initialize()
            {
                code = "TC";
                desc = "Topic Code";
                guid = "00000000-0000-0000-0000-000000000000";
                topicCode1 = new TopicCode(guid, code, desc);
                topicCode2 = new TopicCode(guid, code, "Topic Code2");
                topicCode3 = new TopicCode(guid, "TC2", desc);
            }

            [TestMethod]
            public void TopicCodeSameCodeHashEqual()
            {
                Assert.AreEqual(topicCode1.GetHashCode(), topicCode2.GetHashCode());
            }

            [TestMethod]
            public void TopicCodeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(topicCode1.GetHashCode(), topicCode3.GetHashCode());
            }
        }
    }
}