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
    public class AcademicHonorTests
    {
        private string guid;
        private string code;
        private string description;
        private OtherHonor otherHonor;
       

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "CL";
            description = "Cum Laude";
        }

        [TestClass]
        public class AcademicHonorConstructor : AcademicHonorTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullGuid()
            {
                otherHonor = new OtherHonor(null, code, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyGuid()
            {
                otherHonor = new OtherHonor(string.Empty, code, description);
            }

            [TestMethod]
            public void OtherHonorConstructorValidGuid()
            {
                otherHonor = new OtherHonor(guid, code, description);
                Assert.AreEqual(guid, otherHonor.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullCode()
            {
                otherHonor = new OtherHonor(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyCode()
            {
                otherHonor = new OtherHonor(guid, string.Empty, description);
            }

            [TestMethod]
            public void OtherHonorConstructorValidCode()
            {
                otherHonor = new OtherHonor(guid, code, description);
                Assert.AreEqual(code, otherHonor.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorNullDescription()
            {
                otherHonor = new OtherHonor(guid, code, null);
            }       

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void OtherHonorConstructorEmptyDescription()
            {
                otherHonor = new OtherHonor(guid, code, string.Empty);
            }

            [TestMethod]
            public void OtherHonorConstructorValidDescription()
            {
                otherHonor = new OtherHonor(guid, code, description);
                Assert.AreEqual(description, otherHonor.Description);
            }
           
        }
    }
}
