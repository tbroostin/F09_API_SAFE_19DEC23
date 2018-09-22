using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CommunicationTests
    {
        public string personId;
        public string code;
        public string instanceDescription;
        public DateTime? assignedDate;
        public string statusCode;
        public DateTime? statusDate;
        public DateTime? actionDate;
        public string commentId;

        public Communication communication;

        [TestClass]
        public class CommunicationConstructorTests : CommunicationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                personId = "0003914";
                code = "FA14AWAC";
                instanceDescription = "instance";
                assignedDate = new DateTime(2015, 1, 1);
                statusCode = "I";
                statusDate = new DateTime(2015, 1, 1);
                actionDate = new DateTime(2015, 1, 30);
                commentId = "220";

                communication = new Communication(personId, code);
            }

            [TestMethod]
            public void PersonIdTest()
            {
                Assert.AreEqual(personId, communication.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PersonIdRequiredTest()
            {
                new Communication(null, code);
            }

            [TestMethod]
            public void CodeTest()
            {
                Assert.AreEqual(code, communication.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CodeRequiredTest()
            {
                new Communication(personId, null);
            }

            [TestMethod]
            public void InstanceDescriptionTest()
            {
                //init to empty string
                Assert.AreEqual(string.Empty, communication.InstanceDescription);

                communication.InstanceDescription = instanceDescription;
                Assert.AreEqual(instanceDescription, communication.InstanceDescription);
            }

            [TestMethod]
            public void AssignedDateTest()
            {
                //init to empty string
                Assert.AreEqual(null, communication.AssignedDate);

                communication.AssignedDate = assignedDate;
                Assert.AreEqual(assignedDate, communication.AssignedDate);
            }

            [TestMethod]
            public void StatusCodeTest()
            {
                //init to empty string
                Assert.AreEqual(string.Empty, communication.StatusCode);

                communication.StatusCode = statusCode;
                Assert.AreEqual(statusCode, communication.StatusCode);
            }

            [TestMethod]
            public void StatusDateTest()
            {
                //init to empty string
                Assert.AreEqual(null, communication.StatusDate);

                communication.StatusDate = statusDate;
                Assert.AreEqual(statusDate, communication.StatusDate);
            }

            [TestMethod]
            public void ActionDateTest()
            {
                //init to empty string
                Assert.AreEqual(null, communication.ActionDate);

                communication.ActionDate = actionDate;
                Assert.AreEqual(actionDate, communication.ActionDate);
            }

            [TestMethod]
            public void CommentIdTest()
            {
                //init to empty string
                Assert.AreEqual(string.Empty, communication.CommentId);

                communication.CommentId = commentId;
                Assert.AreEqual(commentId, communication.CommentId);
            }
        }

        [TestClass]
        public class EqualsTests : CommunicationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                personId = "0003914";
                code = "FA14AWAC";
                instanceDescription = "instance";
                assignedDate = new DateTime(2015, 1, 1);
                statusCode = "I";
                statusDate = new DateTime(2015, 1, 1);
                actionDate = new DateTime(2015, 1, 30);
                commentId = "220";

                communication = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };
            }

            [TestMethod]
            public void NotEqualIfNullTest()
            {
                Assert.IsFalse(communication.Equals(null));
            }

            [TestMethod]
            public void NotEqualIfDiffTypeTest()
            {
                Assert.IsFalse(communication.Equals(new Institution()));
            }

            [TestMethod]
            public void EqualIfSimilarTest()
            {
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };
                Assert.AreEqual(test, communication);
            }

            [TestMethod]
            public void NotEqualIfDiffPersonIdTest()
            {
                var test = new Communication("foo", code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };
                Assert.AreNotEqual(test, communication);
            }

            [TestMethod]
            public void NotEqualIfDiffCodeTest()
            {
                var test = new Communication(personId, "foo")
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };
                Assert.AreNotEqual(test, communication);
            }

            [TestMethod]
            public void NotEqualIfDiffInstanceTest()
            {
                var test = new Communication(personId, code)
                {
                    InstanceDescription = "foo",
                    AssignedDate = assignedDate
                };
                Assert.AreNotEqual(test, communication);
            }

            [TestMethod]
            public void NotEqualIfStatusHasValueAndDiffAssignedDateTest()
            {
                communication.StatusCode = "R";
                var test = new Communication(personId, code)
                {
                    InstanceDescription = "foo",
                    AssignedDate = DateTime.Today.AddDays(1),
                    StatusCode = "W"
                };
                Assert.AreNotEqual(test, communication);
            }

            [TestMethod]
            public void IfStatusCodeIsEmptyEqualIfAlmostSimilarTest()
            {
                communication.StatusCode = "W";
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = null
                };
                Assert.AreEqual(test, communication);
            }

            [TestMethod]
            public void IfThisStatusCodeIsEmptyEqualIfAlmostSimilarTest()
            {
                communication.StatusCode = "";
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = null,
                    StatusCode = "W"
                };
                Assert.AreEqual(test, communication);
            }

            [TestMethod]
            public void EqualIfOneAssignedDateIsTodayTest()
            {
                communication.StatusCode = "W";
                communication.AssignedDate = null;
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = DateTime.Today,
                    StatusCode = "R",
                };
                Assert.AreEqual(test, communication);
            }

            [TestMethod]
            public void EqualIfOtherAssignedDateIsTodayTest()
            {
                communication.StatusCode = "W";
                communication.AssignedDate = DateTime.Today;
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = DateTime.Today.AddDays(1),
                    StatusCode = "R",
                };

                Assert.AreEqual(test, communication);
            }
        }

        [TestClass]
        public class CommunicationSimilarTest : CommunicationTests
        {
            [TestInitialize]
            public void Initialize()
            {
                personId = "0003914";
                code = "FA14AWAC";
                instanceDescription = "instance";
                assignedDate = new DateTime(2015, 1, 1);
                statusCode = "I";
                statusDate = new DateTime(2015, 1, 1);
                actionDate = new DateTime(2015, 1, 30);
                commentId = "220";

                communication = new Communication(personId, code)
                    {
                        InstanceDescription = instanceDescription,
                        AssignedDate = assignedDate
                    };
            }

            [TestMethod]
            public void SimilarTest()
            {
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };

                Assert.IsTrue(test.Similar(communication));
            }

            [TestMethod]
            public void NullCommunicationIsFalseTest()
            {
                Assert.IsFalse(communication.Similar(null));
            }

            [TestMethod]
            public void DiffPersonIdNotSimilarTest()
            {
                var test = new Communication("foobar", code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };

                Assert.IsFalse(test.Similar(communication));
            }

            [TestMethod]
            public void DiffCodeNotSimilarTest()
            {
                var test = new Communication(personId, "foobar")
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = assignedDate
                };

                Assert.IsFalse(test.Similar(communication));
            }

            [TestMethod]
            public void DiffInstanceDescriptionNotSimilarTest()
            {
                var test = new Communication(personId, code)
                {
                    InstanceDescription = "foobar",
                    AssignedDate = assignedDate
                };

                Assert.IsFalse(test.Similar(communication));
            }

            [TestMethod]
            public void DiffAssignedDateNotSimilarTest()
            {
                var test = new Communication(personId, code)
                {
                    InstanceDescription = instanceDescription,
                    AssignedDate = null
                };

                Assert.IsFalse(test.Similar(communication));
            }
        }
    }
}
