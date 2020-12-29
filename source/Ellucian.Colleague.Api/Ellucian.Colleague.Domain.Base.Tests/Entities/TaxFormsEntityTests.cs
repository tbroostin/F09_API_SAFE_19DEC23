//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
     [TestClass]
    public class TaxFormsTests
    {
        [TestClass]
        public class TaxFormsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string box;
            private TaxForms2 taxForms;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxForms = new TaxForms2(guid, code, desc, box);
            }

            [TestMethod]
            public void TaxForms_Code()
            {
                Assert.AreEqual(code, taxForms.Code);
            }

            [TestMethod]
            public void TaxForms_Description()
            {
                Assert.AreEqual(desc, taxForms.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxForms_GuidNullException()
            {
                new TaxForms2(null, code, desc, box);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxForms_CodeNullException()
            {
                new TaxForms2(guid, null, desc, box);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxForms_DescNullException()
            {
                new TaxForms2(guid, code, null, box);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxFormsGuidEmptyException()
            {
                new TaxForms2(string.Empty, code, desc, box);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxFormsCodeEmptyException()
            {
                new TaxForms2(guid, string.Empty, desc, box);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TaxFormsDescEmptyException()
            {
                new TaxForms2(guid, code, string.Empty, box);
            }

        }

        [TestClass]
        public class TaxForms_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private string box;
            private TaxForms2 taxForms1;
            private TaxForms2 taxForms2;
            private TaxForms2 taxForms3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxForms1 = new TaxForms2(guid, code, desc, box);
                taxForms2 = new TaxForms2(guid, code, "Second Year", box);
                taxForms3 = new TaxForms2(Guid.NewGuid().ToString(), "200", desc, box);
            }

            [TestMethod]
            public void TaxFormsSameCodesEqual()
            {
                Assert.IsTrue(taxForms1.Equals(taxForms2));
            }

            [TestMethod]
            public void TaxFormsDifferentCodeNotEqual()
            {
                Assert.IsFalse(taxForms1.Equals(taxForms3));
            }
        }

        [TestClass]
        public class TaxForms_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string box;
            private TaxForms2 taxForms1;
            private TaxForms2 taxForms2;
            private TaxForms2 taxForms3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                taxForms1 = new TaxForms2(guid, code, desc, box);
                taxForms2 = new TaxForms2(guid, code, "Second Year", box);
                taxForms3 = new TaxForms2(Guid.NewGuid().ToString(), "200", desc, box);
            }

            [TestMethod]
            public void TaxFormsSameCodeHashEqual()
            {
                Assert.AreEqual(taxForms1.GetHashCode(), taxForms2.GetHashCode());
            }

            [TestMethod]
            public void TaxFormsDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(taxForms1.GetHashCode(), taxForms3.GetHashCode());
            }
        }
    }
}
