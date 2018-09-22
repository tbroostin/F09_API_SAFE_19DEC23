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
    public class StudentLoadTests
    {
        [TestClass]
        public class StudentLoad_Constructor
        {
            private string code;
            private string desc;
            private StudentLoad load;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                load = new StudentLoad(code, desc);
            }

            [TestMethod]
            public void StudentLoad_Code()
            {
                Assert.AreEqual(code, load.Code);
            }

            [TestMethod]
            public void StudentLoad_Description()
            {
                Assert.AreEqual(desc, load.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentLoad_CodeNullException()
            {
                new StudentLoad(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentLoadCodeEmptyException()
            {
                new StudentLoad(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentLoadDescEmptyException()
            {
                new StudentLoad(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentLoad_DescNullException()
            {
                new StudentLoad(code, null);
            }

        }
    }
}