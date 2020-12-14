// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.InstantEnrollment;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.InstantEnrollment
{
    [TestClass]
    public class AcademicProgramOptionTests
    {
        private string code;
        private string catalog;

        [TestInitialize]
        public void AcademicProgramOptionTests_Initialize()
        {
            code = "CE.DFLT";
            catalog = "2014X";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AcademicProgramOption_null_Code()
        {
            var entity = new AcademicProgramOption(null, catalog);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AcademicProgramOption_empty_Code()
        {
            var entity = new AcademicProgramOption(string.Empty, catalog);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AcademicProgramOption_null_Catalog()
        {
            var entity = new AcademicProgramOption(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AcademicProgramOption_empty_Catalog()
        {
            var entity = new AcademicProgramOption(code, string.Empty);
        }

        [TestMethod]
        public void AcademicProgramOption_valid()
        {
            var entity = new AcademicProgramOption(code, catalog);
            Assert.AreEqual(code, entity.Code);
            Assert.AreEqual(catalog, entity.CatalogCode);
        }
    }
}
