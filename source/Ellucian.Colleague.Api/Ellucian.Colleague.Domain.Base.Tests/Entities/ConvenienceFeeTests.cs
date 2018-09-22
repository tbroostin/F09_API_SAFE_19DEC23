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
    public class ConvenienceFeeTests
    {
        private string code;
        private string description;
        private ConvenienceFee convFee;

        [TestInitialize]
        public void Initialize()
        {
            code = "HIS";
            description = "Hispanic/Latino";
            convFee = new ConvenienceFee(code, description);
        }

        [TestClass]
        public class ConvenienceFeeConstructor : ConvenienceFeeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConvenienceFeeConstructorNullCode()
            {
                convFee = new ConvenienceFee(null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConvenienceFeeConstructorEmptyCode()
            {
                convFee = new ConvenienceFee(string.Empty, description);
            }

            [TestMethod]
            public void ConvenienceFeeConstructorValidCode()
            {
                convFee = new ConvenienceFee(code, description);
                Assert.AreEqual(code, convFee.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConvenienceFeeConstructorNullDescription()
            {
                convFee = new ConvenienceFee(code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ConvenienceFeeConstructorEmptyDescription()
            {
                convFee = new ConvenienceFee(code, string.Empty);
            }

            [TestMethod]
            public void ConvenienceFeeConstructorValidDescription()
            {
                convFee = new ConvenienceFee(code, description);
                Assert.AreEqual(description, convFee.Description);
            }
        }
    }
}
