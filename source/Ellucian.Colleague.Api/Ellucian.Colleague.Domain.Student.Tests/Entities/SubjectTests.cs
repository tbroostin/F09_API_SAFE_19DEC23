using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SubjectTests
    {
        [TestClass]
        public class SubjectConstructor
        {
            private string guid;
            private string code;
            private string description;
            private Subject subject;
            private bool showInCourseSearch;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "MATH";
                description = "Mathematics";
                showInCourseSearch = true;

                // Asserts are based off this constructor statement, unless another constructor is used in the test method
                subject = new Subject(guid, code, description, showInCourseSearch);
            }

            [TestCleanup]
            public void CleanUp()
            {

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectGuidExceptionIfNull()
            {
                new Subject(null, code, "junk", showInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectGuidExceptionIfEmpty()
            {
                new Subject(string.Empty, code, "junk", showInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectCodeExceptionIfNull()
            {
                new Subject(guid, null, "junk", showInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectDescriptionExceptionIfNull()
            {
                new Subject(guid, "junk", null, showInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectCodeExceptionIfEmpty()
            {
                new Subject(guid, string.Empty, "junk", showInCourseSearch);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SubjectDescriptionExceptionIfEmpty()
            {
                new Subject(guid, "junk", string.Empty, showInCourseSearch);
            }

            [TestMethod]
            public void SubjectGuid()
            {
                Assert.AreEqual(guid, subject.Guid);
            }

            [TestMethod]
            public void SubjectCode()
            {
                Assert.AreEqual(code, subject.Code);
            }

            [TestMethod]
            public void SubjectDescription()
            {
                Assert.AreEqual(description, subject.Description);
            }

            [TestMethod]
            public void ShowInCourseSearch()
            {
                Assert.AreEqual(showInCourseSearch, subject.ShowInCourseSearch);
            }

        }

        [TestClass]
        public class SubjectEquals
        {
            private string guid;
            private string code;
            private string description;
            private Subject s1;
            private Subject s2;
            private Subject s3;
            private Subject s4;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "FOO";
                description = "Foobar";
                s1 = new Subject(guid, code, description, true);
                s2 = new Subject(guid, code, "junk", true);
                s3 = new Subject(Guid.NewGuid().ToString(), "junk", description, false);
                s4 = new Subject(guid, code, description, false);
            }

            [TestMethod]
            public void SubjectSameCodesEqual()
            {
                Assert.IsTrue(s1.Equals(s4));
            }

            [TestMethod]
            public void SubjectDifferentCodeNotEqual()
            {
                Assert.IsFalse(s1.Equals(s3));
            }
        }

        [TestClass]
        public class SubjectGetHashCode
        {
            private string guid;
            private string code;
            private string description;
            private Subject s1;
            private Subject s2;
            private Subject s3;
            private Subject s4;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "FOO";
                description = "Foobar";
                s1 = new Subject(guid, code, description, true);
                s2 = new Subject(guid, code, "junk", false);
                s3 = new Subject(Guid.NewGuid().ToString(), "junk", description, true);
                s4 = new Subject(guid, code, description, false);
            }

            [TestMethod]
            public void SubjectSameCodeHashEqual()
            {
                Assert.AreEqual(s1.GetHashCode(), s4.GetHashCode());
            }

            [TestMethod]
            public void SubjectDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(s1.GetHashCode(), s3.GetHashCode());
            }
        }
    }
}

