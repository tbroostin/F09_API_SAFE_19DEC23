// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class CostCenterStructureTests
    {
        #region Initialize and Cleanup
        CostCenterStructureBuilder CostCenterStructureBuilder;
        GeneralLedgerComponentBuilder ComponentBuilder;

        [TestInitialize]
        public void Initialize()
        {
            CostCenterStructureBuilder = new CostCenterStructureBuilder();
            ComponentBuilder = new GeneralLedgerComponentBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CostCenterStructureBuilder = null;
            ComponentBuilder = null;
        }
        #endregion

        #region Constructor tests

        [TestMethod]
        public void Constructor_Success()
        {
            var actualEntity = CostCenterStructureBuilder.Build();
            Assert.IsTrue(actualEntity is CostCenterStructure);
            Assert.IsTrue(actualEntity.CostCenterComponents is ReadOnlyCollection<GeneralLedgerComponent>);
            Assert.IsTrue(actualEntity.ObjectComponents is ReadOnlyCollection<GeneralLedgerComponent>);
            Assert.IsTrue(actualEntity.CostCenterComponents.Count == 0);
            Assert.IsTrue(actualEntity.ObjectComponents.Count == 0);
        }

        #endregion

        #region AddCostCenterComponent tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCostCenterComponent_NullArgument()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            glDescriptionInfo.AddCostCenterComponent(null);
        }

        [TestMethod]
        public void AddCostCenterComponent_AddOne()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();
            glDescriptionInfo.AddCostCenterComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.CostCenterComponents.Count);
        }

        [TestMethod]
        public void AddCostCenterComponent_AddTwoDifferentGlComponents()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent1 = ComponentBuilder.WithComponent("FUND").Build();
            var glComponent2 = ComponentBuilder.WithComponent("LOCATION").Build();

            glDescriptionInfo.AddCostCenterComponent(glComponent1);
            Assert.AreEqual(1, glDescriptionInfo.CostCenterComponents.Count);

            glDescriptionInfo.AddCostCenterComponent(glComponent2);
            Assert.AreEqual(2, glDescriptionInfo.CostCenterComponents.Count);
        }

        [TestMethod]
        public void AddCostCenterComponent_AddDuplicateGlComponent()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();

            glDescriptionInfo.AddCostCenterComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.CostCenterComponents.Count);

            glDescriptionInfo.AddCostCenterComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.CostCenterComponents.Count);
        }

        #endregion

        #region AddObjectComponent tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddObjectComponent_NullArgument()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            glDescriptionInfo.AddObjectComponent(null);
        }

        [TestMethod]
        public void AddObjectComponent_AddOne()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();
            glDescriptionInfo.AddObjectComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.ObjectComponents.Count);
        }

        [TestMethod]
        public void AddObjectComponent_AddTwoDifferentGlComponents()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent1 = ComponentBuilder.WithComponent("FUND").Build();
            var glComponent2 = ComponentBuilder.WithComponent("LOCATION").Build();

            glDescriptionInfo.AddObjectComponent(glComponent1);
            Assert.AreEqual(1, glDescriptionInfo.ObjectComponents.Count);

            glDescriptionInfo.AddObjectComponent(glComponent2);
            Assert.AreEqual(2, glDescriptionInfo.ObjectComponents.Count);
        }

        [TestMethod]
        public void AddObjectComponent_AddDuplicateGlComponent()
        {
            var glDescriptionInfo = CostCenterStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();

            glDescriptionInfo.AddObjectComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.ObjectComponents.Count);

            glDescriptionInfo.AddObjectComponent(glComponent);
            Assert.AreEqual(1, glDescriptionInfo.ObjectComponents.Count);
        }

        #endregion
    }
}