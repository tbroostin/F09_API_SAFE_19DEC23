// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class InstitutionTypeTests
    {
        private string code;
        private string description;
        private string category;
        private InstitutionType institutionType;

        [TestInitialize]
        public void Initialize()
        {
            code = "CO";
            description = "College";
            category = "C";
            institutionType = new InstitutionType(code, description, category);
        }

        [TestClass]
        public class InstitutionTypeConstructor : InstitutionTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionTypeConstructorNullCode()
            {
                institutionType = new InstitutionType(null, description, category);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionTypeConstructorEmptyCode()
            {
                institutionType = new InstitutionType(string.Empty, description, category);
            }

            [TestMethod]
            public void InstitutionTypeConstructorValidCode()
            {
                institutionType = new InstitutionType(code, description, category);
                Assert.AreEqual(code, institutionType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionTypeConstructorNullDescription()
            {
                institutionType = new InstitutionType(code, null, category);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstitutionTypeConstructorEmptyDescription()
            {
                institutionType = new InstitutionType(code, string.Empty, category);
            }

            [TestMethod]
            public void InstitutionTypeConstructorValidDescription()
            {
                institutionType = new InstitutionType(code, description, category);
                Assert.AreEqual(description, institutionType.Description);
            }
        }
    }
}
