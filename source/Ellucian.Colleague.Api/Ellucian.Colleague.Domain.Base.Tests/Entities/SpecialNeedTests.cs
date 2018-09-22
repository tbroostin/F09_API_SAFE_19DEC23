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
    public class SpecialNeedTests
    {
        [TestClass]
        public class SpecialNeedConstructor
        {
            private string code;
            private string desc;
            private SpecialNeed sn;

            [TestInitialize]
            public void Initialize()
            {
                code = "DIV";
                desc = "SpecialNeed";
                sn = new SpecialNeed(code, desc);
            }

            [TestMethod]
            public void SpecialNeedCode()
            {
                Assert.AreEqual(sn.Code, code);
            }

            [TestMethod]
            public void SpecialNeedDescription()
            {
                Assert.AreEqual(sn.Description, desc);
            }
        }
    }
}
