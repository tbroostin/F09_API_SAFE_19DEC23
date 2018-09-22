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
    public class ProspectSourceTests
    {
        private string code;
        private string description;
        private ProspectSource prospectSource;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            prospectSource = new ProspectSource(code, description);
        }

        [TestClass]
        public class ProspectSourceConstructor : ProspectSourceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProspectSourceConstructorNullCode()
            {
                prospectSource = new ProspectSource(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProspectSourceConstructorEmptyCode()
            {
                prospectSource = new ProspectSource(string.Empty, description);
            }

            [TestMethod]
            public void ProspectSourceConstructorValidCode()
            {
                prospectSource = new ProspectSource(code, description);
                Assert.AreEqual(code, prospectSource.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProspectSourceConstructorNullDescription()
            {
                prospectSource = new ProspectSource(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProspectSourceConstructorEmptyDescription()
            {
                prospectSource = new ProspectSource(code, string.Empty);
            }

            [TestMethod]
            public void ProspectSourceConstructorValidDescription()
            {
                prospectSource = new ProspectSource(code, description);
                Assert.AreEqual(description, prospectSource.Description);
            }
        }
    }
}
