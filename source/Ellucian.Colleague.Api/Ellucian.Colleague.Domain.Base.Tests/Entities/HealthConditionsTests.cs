﻿// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class HealthConditionsTests
    {
        [TestClass]
        public class HealthConditionsConstructor
        {
            private string code;
            private string desc;
            private HealthConditions bldgType;

            [TestInitialize]
            public void Initialize()
            {
                code = "ADMIN";
                desc = "Administrative";
                bldgType = new HealthConditions(code, desc);
            }

            [TestMethod]
            public void TypeCode()
            {
                Assert.AreEqual(bldgType.Code, code);
            }

            [TestMethod]
            public void TypeDescription()
            {
                Assert.AreEqual(bldgType.Description, desc);
            }
        }
    }
}
