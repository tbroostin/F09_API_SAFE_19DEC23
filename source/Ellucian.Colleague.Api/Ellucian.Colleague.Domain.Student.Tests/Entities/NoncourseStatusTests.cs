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
    public class NoncourseStatusTests
    {
        [TestClass]
        public class NoncourseStatus_Constructor
        {
            private string code;
            private string desc;
            private NoncourseStatusType type;
            private NoncourseStatus ncStat;

            [TestInitialize]
            public void Initialize()
            {
                code = "A";
                desc = "Active";
                type = NoncourseStatusType.Accepted;
                ncStat = new NoncourseStatus(code, desc, type);
            }

            [TestMethod]
            public void NoncourseStatus_Code()
            {
                Assert.AreEqual(code, ncStat.Code);
            }

            [TestMethod]
            public void NoncourseStatus_Description()
            {
                Assert.AreEqual(desc, ncStat.Description);
            }

            [TestMethod]
            public void NoncourseStatus_StatusType()
            {
                Assert.AreEqual(type, ncStat.StatusType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoncourseStatus_CodeNullException()
            {
                new NoncourseStatus(null, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoncourseStatusCodeEmptyException()
            {
                new NoncourseStatus(string.Empty, desc, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoncourseStatus_DescNullException()
            {
                new NoncourseStatus(code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NoncourseStatusDescEmptyException()
            {
                new NoncourseStatus(code, string.Empty, type);
            }
        }
    }
}