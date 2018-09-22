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
    public class ExternalTranscriptStatusTests
    {
        [TestClass]
        public class ExternalTranscriptStatus_Constructor
        {
            private string code;
            private string desc;
            private ExternalTranscriptStatus etStat;

            [TestInitialize]
            public void Initialize()
            {
                code = "A";
                desc = "Accepted";
                etStat = new ExternalTranscriptStatus(code, desc);
            }

            [TestMethod]
            public void ExternalTranscriptStatus_Code()
            {
                Assert.AreEqual(code, etStat.Code);
            }

            [TestMethod]
            public void ExternalTranscriptStatus_Description()
            {
                Assert.AreEqual(desc, etStat.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalTranscriptStatus_CodeNullException()
            {
                new ExternalTranscriptStatus(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalTranscriptStatusCodeEmptyException()
            {
                new ExternalTranscriptStatus(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalTranscriptStatusDescEmptyException()
            {
                new ExternalTranscriptStatus(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ExternalTranscriptStatus_DescNullException()
            {
                new ExternalTranscriptStatus(code, null);
            }

        }
    }
}