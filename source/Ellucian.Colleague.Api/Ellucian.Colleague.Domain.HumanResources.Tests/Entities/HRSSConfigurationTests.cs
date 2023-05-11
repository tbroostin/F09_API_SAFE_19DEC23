/* Copyright 2022 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class HRSSConfigurationTests
    {
        public HRSSConfiguration Configuration
        {
            get
            {
                return new HRSSConfiguration();
            }
        }

        [TestMethod]
        public void DefaultPropertiesAreSetTest()
        {
            Assert.AreEqual(string.Empty, Configuration.HrssDisplayNameHierarchy);
            
        }
    }
}
