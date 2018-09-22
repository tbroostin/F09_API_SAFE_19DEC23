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
    public class SectionTransferStatusTests
    {
        [TestClass]
        public class SectionTransferStatusConstructor
        {
            private string code;
            private string desc;
            private SectionTransferStatus transferStatus;

            [TestInitialize]
            public void Initialize()
            {
                code = "TS";
                desc = "Transfer Status";
                transferStatus = new SectionTransferStatus(code, desc);
            }

            [TestMethod]
            public void SectionTransferStatusCode()
            {
                Assert.AreEqual(code, transferStatus.Code);
            }

            [TestMethod]
            public void SectionTransferStatusDescription()
            {
                Assert.AreEqual(desc, transferStatus.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTransferStatusCodeNullException()
            {
                new SectionTransferStatus(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SectionTransferStatusDescNullException()
            {
                new SectionTransferStatus(code, null);
            }
        }

        [TestClass]
        public class SectionTransferStatusEquals
        {
            private string code;
            private string desc;
            private SectionTransferStatus SectionTransferStatus1;
            private SectionTransferStatus SectionTransferStatus2;
            private SectionTransferStatus SectionTransferStatus3;

            [TestInitialize]
            public void Initialize()
            {
                code = "TS";
                desc = "Transfer Status";
                SectionTransferStatus1 = new SectionTransferStatus(code, desc);
                SectionTransferStatus2 = new SectionTransferStatus(code, "Transfer Status2");
                SectionTransferStatus3 = new SectionTransferStatus("TS2", desc);
            }

            [TestMethod]
            public void SectionTransferStatusSameCodesEqual()
            {
                Assert.IsTrue(SectionTransferStatus1.Equals(SectionTransferStatus2));
            }

            [TestMethod]
            public void SectionTransferStatusDifferenTSodeNotEqual()
            {
                Assert.IsFalse(SectionTransferStatus1.Equals(SectionTransferStatus3));
            }
        }

        [TestClass]
        public class SectionTransferStatusGetHashCode
        {
            private string code;
            private string desc;
            private SectionTransferStatus SectionTransferStatus1;
            private SectionTransferStatus SectionTransferStatus2;
            private SectionTransferStatus SectionTransferStatus3;

            [TestInitialize]
            public void Initialize()
            {
                code = "TS";
                desc = "Transfer Status";
                SectionTransferStatus1 = new SectionTransferStatus(code, desc);
                SectionTransferStatus2 = new SectionTransferStatus(code, "Transfer Status2");
                SectionTransferStatus3 = new SectionTransferStatus("TS2", desc);
            }

            [TestMethod]
            public void SectionTransferStatusSameCodeHashEqual()
            {
                Assert.AreEqual(SectionTransferStatus1.GetHashCode(), SectionTransferStatus2.GetHashCode());
            }

            [TestMethod]
            public void SectionTransferStatusDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(SectionTransferStatus1.GetHashCode(), SectionTransferStatus3.GetHashCode());
            }
        }
    }
}