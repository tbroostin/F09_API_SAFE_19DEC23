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
    public class TranscriptCategoryTests
    {
        [TestClass]
        public class TranscriptCategory_Constructor
        {
            private string code;
            private string desc;
            private TranscriptCategory transCat;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADM";
                desc = "Admitted";
                transCat = new TranscriptCategory(code, desc);
            }

            [TestMethod]
            public void TranscriptCategory_Code()
            {
                Assert.AreEqual(code, transCat.Code);
            }

            [TestMethod]
            public void TranscriptCategory_Description()
            {
                Assert.AreEqual(desc, transCat.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TranscriptCategory_CodeNullException()
            {
                new TranscriptCategory(null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TranscriptCategoryCodeEmptyException()
            {
                new TranscriptCategory(string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TranscriptCategoryDescEmptyException()
            {
                new TranscriptCategory(code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TranscriptCategory_DescNullException()
            {
                new TranscriptCategory(code, null);
            }

        }
    }
}