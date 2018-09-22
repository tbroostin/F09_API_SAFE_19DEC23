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
    public class AdmittedStatusTests
    {
        [TestClass]
        public class AdmittedStatus_Constructor
        {
            private string code;
            private string desc;
            private string transferFlag;
            private AdmittedStatus admStat;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                transferFlag = "Y";
                admStat = new AdmittedStatus(code, desc, transferFlag);
            }

            [TestMethod]
            public void AdmittedStatus_Code()
            {
                Assert.AreEqual(code, admStat.Code);
            }

            [TestMethod]
            public void AdmittedStatus_Description()
            {
                Assert.AreEqual(desc, admStat.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmittedStatus_CodeNullException()
            {
                new AdmittedStatus(null, desc, transferFlag);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmittedStatusCodeEmptyException()
            {
                new AdmittedStatus(string.Empty, desc, transferFlag);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmittedStatusDescEmptyException()
            {
                new AdmittedStatus(code, string.Empty, transferFlag);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AdmittedStatus_DescNullException()
            {
                new AdmittedStatus(code, null, transferFlag);
            }
        }
    }
}