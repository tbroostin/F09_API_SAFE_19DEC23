// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class BookOptionTests
    {
        [TestClass]
        public class BookOption_Constructor
        {
            private string code;
            private string desc;
            private BookOption bookOpt;

            [TestInitialize]
            public void Initialize()
            {
                code = "R";
                desc = "Required";
                bookOpt = new BookOption(code, desc, true);
            }

            [TestMethod]
            public void BookOption_Code()
            {
                Assert.AreEqual(code, bookOpt.Code);
            }

            [TestMethod]
            public void BookOption_Description()
            {
                Assert.AreEqual(desc, bookOpt.Description);
            }

            [TestMethod]
            public void BookOption_IsRequired_True()
            {
                Assert.IsTrue(bookOpt.IsRequired);
            }

            [TestMethod]
            public void BookOption_IsRequired_False()
            {
                var newBookOpt = new BookOption(code, desc, false);
                Assert.IsFalse(newBookOpt.IsRequired);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BookOption_CodeNullException()
            {
                new BookOption(null, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BookOptionCodeEmptyException()
            {
                new BookOption(string.Empty, desc, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BookOptionDescEmptyException()
            {
                new BookOption(code, string.Empty, true);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void BookOption_DescNullException()
            {
                new BookOption(code, null, true);
            }

        }
    }
}
