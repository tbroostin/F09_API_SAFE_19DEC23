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
    public class InterestTests
    {
       private string guid;
        private string code;
        private string description;
        private string interestType;
        private Interest interest;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            interestType= "AR";
            code = "HIS";
            description = "Hispanic/Latino";
         }

        [TestClass]
        public class InterestConstructor : InterestTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestConstructorNullCode()
            {
                interest = new Interest(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestConstructorEmptyCode()
            {
                interest = new Interest(guid, string.Empty, description);
            }

            [TestMethod]
            public void InterestConstructorValidCode()
            {
                interest = new Interest(guid, code, description);
                Assert.AreEqual(code, interest.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestConstructorNullDescription()
            {
                interest = new Interest(guid,code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void InterestConstructorEmptyDescription()
            {
                interest = new Interest(guid, code, string.Empty);
            }

            [TestMethod]
            public void InterestConstructorValidDescription()
            {
                interest = new Interest(guid, code, description);
                Assert.AreEqual(description, interest.Description);
            }
        }
       
    }
}
