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
    public class ApplicationInfluenceTests
    {
        [TestClass]
        public class ApplicationInfluence_Constructor
        {
            private string code;
            private string desc;
            private ApplicationInfluence applInfl;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                applInfl = new ApplicationInfluence(code, desc);
            }

            [TestMethod]
            public void ApplicationInfluence_Code()
            {
                Assert.AreEqual(code, applInfl.Code);
            }

            [TestMethod]
            public void ApplicationInfluence_Description()
            {
                Assert.AreEqual(desc, applInfl.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluence_CodeNullException()
            {
                new ApplicationInfluence(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluenceCodeEmptyException()
            {
                new ApplicationInfluence(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluenceDescEmptyException()
            {
                new ApplicationInfluence(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluence_DescNullException()
            {
                new ApplicationInfluence(code, null);
            }

        }
    }
}