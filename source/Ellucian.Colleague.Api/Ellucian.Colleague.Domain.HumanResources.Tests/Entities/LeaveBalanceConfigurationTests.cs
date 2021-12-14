/*Copyright 2021 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class LeaveBalanceConfigurationTests
    {
        public List<string> excludeIds;
        public int? leaveRequestLookbackDays;
        public LeaveBalanceConfiguration configuration;

        [TestInitialize]
        public void Initialize()
        {
            configuration = new LeaveBalanceConfiguration();
        }

        [TestMethod]
        public void ObjectCreatedTest()
        {
            Assert.IsNotNull(configuration);
        }

        [TestMethod]
        public void PropertiesInitializedAsExpectedTest()
        {
            Assert.IsNotNull(configuration.ExcludedLeavePlanIds);
            Assert.IsFalse(configuration.ExcludedLeavePlanIds.Any());
            Assert.IsNull(configuration.LeaveRequestLookbackDays);
        }

        [TestMethod]
        public void ExcludedLeavePlanIds_GetSetTest()
        {
            excludeIds = new List<string>() {"a", "b", "c" };
            configuration.ExcludedLeavePlanIds = excludeIds;
            CollectionAssert.AreEqual(excludeIds, configuration.ExcludedLeavePlanIds);
        }

        [TestMethod]
        public void LeaveRequestLookbackDays_GetSetTest()
        {
            leaveRequestLookbackDays = 5;
            configuration.LeaveRequestLookbackDays = leaveRequestLookbackDays;
            Assert.AreEqual(leaveRequestLookbackDays, configuration.LeaveRequestLookbackDays);
        }
    }
}
