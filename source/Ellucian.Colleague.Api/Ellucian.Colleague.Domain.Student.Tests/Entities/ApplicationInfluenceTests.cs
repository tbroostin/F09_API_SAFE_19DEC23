﻿// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
            private string guid;
            private string desc;
            private ApplicationInfluence applInfl;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ADM";
                desc = "Admitted";
                applInfl = new ApplicationInfluence(guid, code, desc);
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
                new ApplicationInfluence(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluenceCodeEmptyException()
            {
                new ApplicationInfluence(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluenceDescEmptyException()
            {
                new ApplicationInfluence(guid, code, string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplicationInfluence_DescNullException()
            {
                new ApplicationInfluence(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplInfluences_GuidNullException()
            {
                new ApplicationInfluence(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApplInfluencesGuidEmptyException()
            {
                new ApplicationInfluence(string.Empty, code, desc);
            }
        }
    }
}