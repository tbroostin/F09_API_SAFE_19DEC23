// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class InstructionalPlatformTests
    {
        private string guid;
        private string code;
        private string description;
        private InstructionalPlatform instructionalPlatform;
       

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "CL";
            description = "Cum Laude";
        }

        [TestClass]
        public class InstructionalPlatformConstructor : InstructionalPlatformTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorNullGuid()
            {
                instructionalPlatform = new InstructionalPlatform(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorEmptyGuid()
            {
                instructionalPlatform = new InstructionalPlatform(string.Empty, code, description);
            }

            [TestMethod]
            public void InstructionalPlatformConstructorValidGuid()
            {
                instructionalPlatform = new InstructionalPlatform(guid, code, description);
                Assert.AreEqual(guid, instructionalPlatform.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorNullCode()
            {
                instructionalPlatform = new InstructionalPlatform(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorEmptyCode()
            {
                instructionalPlatform = new InstructionalPlatform(guid, string.Empty, description);
            }

            [TestMethod]
            public void InstructionalPlatformConstructorValidCode()
            {
                instructionalPlatform = new InstructionalPlatform(guid, code, description);
                Assert.AreEqual(code, instructionalPlatform.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorNullDescription()
            {
                instructionalPlatform = new InstructionalPlatform(guid, code, null);
            }       

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InstructionalPlatformConstructorEmptyDescription()
            {
                instructionalPlatform = new InstructionalPlatform(guid, code, string.Empty);
            }

            [TestMethod]
            public void InstructionalPlatformConstructorValidDescription()
            {
                instructionalPlatform = new InstructionalPlatform(guid, code, description);
                Assert.AreEqual(description, instructionalPlatform.Description);
            }
           
        }
    }
}
