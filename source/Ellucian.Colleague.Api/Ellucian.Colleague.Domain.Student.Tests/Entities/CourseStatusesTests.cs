// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CourseStatusesTests
    {
        [TestClass]
        public class CourseStatuses_Constructor
        {
            private string guid;
            private string code;
            private string desc;
            private CourseStatuses csItem;

            [TestInitialize]
            public void Initialize()
            {
                guid = GetGuid();
                code = "A";
                desc = "Active";
                csItem = new CourseStatuses(guid, code, desc);
            }

            [TestMethod]
            public void CourseStatuses_Guid()
            {
                Assert.AreEqual(guid, csItem.Guid);
            }

            [TestMethod]
            public void CourseStatuses_Code()
            {
                Assert.AreEqual(code, csItem.Code);
            }

            [TestMethod]
            public void CourseStatuses_Description()
            {
                Assert.AreEqual(desc, csItem.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatuses_GuidNullException()
            {
                new CourseStatuses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatuses_GuidEmptyException()
            {
                new CourseStatuses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatuses_CodeNullException()
            {
                new CourseStatuses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatusesCodeEmptyException()
            {
                new CourseStatuses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatusesDescEmptyException()
            {
                new CourseStatuses(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CourseStatuses_DescNullException()
            {
                new CourseStatuses(guid, code, null);
            }

        }

        private static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}