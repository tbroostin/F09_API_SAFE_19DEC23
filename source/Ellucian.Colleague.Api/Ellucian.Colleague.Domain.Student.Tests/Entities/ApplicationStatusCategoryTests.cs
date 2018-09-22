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
    public class ApplicationStatusCategoryTests
    {
        [TestClass]
        public class ApplicationStatusCategory_Constructor
        {
            private string code;
            private string desc;
            private ApplicationStatusCategory applStatCat;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                applStatCat = new ApplicationStatusCategory(code, desc);
            }

            [TestMethod]
            public void ApplicationStatusCategory_Code()
            {
                Assert.AreEqual(code, applStatCat.Code);
            }

            [TestMethod]
            public void ApplicationStatusCategory_Description()
            {
                Assert.AreEqual(desc, applStatCat.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationStatusCategory_CodeNullException()
            {
                new ApplicationStatusCategory(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationStatusCategoryCodeEmptyException()
            {
                new ApplicationStatusCategory(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationStatusCategoryDescEmptyException()
            {
                new ApplicationStatusCategory(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationStatusCategory_DescNullException()
            {
                new ApplicationInfluence(code, null);
            }

        }
    }
}