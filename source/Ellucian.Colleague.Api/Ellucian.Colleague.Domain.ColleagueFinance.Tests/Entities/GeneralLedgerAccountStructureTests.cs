// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class GeneralLedgerAccountStructureTests
    {
        #region Initialize and Cleanup
        GeneralLedgerAccountStructureBuilder AccountStructureBuilder;
        GeneralLedgerComponentBuilder ComponentBuilder;
        private string glComponentName;

        [TestInitialize]
        public void Initialize()
        {
            AccountStructureBuilder = new GeneralLedgerAccountStructureBuilder();
            ComponentBuilder = new GeneralLedgerComponentBuilder();
        }

        [TestCleanup]
        public void Cleanup()
        {
            AccountStructureBuilder = null;
            ComponentBuilder = null;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Constructor_Success()
        {
            var actualEntity = AccountStructureBuilder.Build();
            Assert.IsTrue(actualEntity is GeneralLedgerAccountStructure);
            Assert.IsTrue(actualEntity.MajorComponents is ReadOnlyCollection<GeneralLedgerComponent>);
            Assert.IsTrue(actualEntity.MajorComponents.Count == 0);

            Assert.IsTrue(actualEntity.MajorComponentStartPositions is ReadOnlyCollection<string>);
            Assert.IsTrue(actualEntity.MajorComponentStartPositions.Count() == 0);

            Assert.IsTrue(actualEntity.Subcomponents.Count == 0);
            Assert.IsTrue(actualEntity.Subcomponents.Count() == 0);
        }
        #endregion

        #region AddMajorComponent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddMajorComponent_NullArgument()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            glAccountStructure.AddMajorComponent(null);
        }

        [TestMethod]
        public void AddMajorComponent_AddOne()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();
            glAccountStructure.AddMajorComponent(glComponent);
            Assert.AreEqual(1, glAccountStructure.MajorComponents.Count);
        }

        [TestMethod]
        public void AddMajorComponent_AddTwoDifferentGlComponents()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            var glComponent1 = ComponentBuilder.WithComponent("PROGRAM").Build();
            var glComponent2 = ComponentBuilder.WithComponent("OBJECT").Build();

            glAccountStructure.AddMajorComponent(glComponent1);
            Assert.AreEqual(1, glAccountStructure.MajorComponents.Count);

            glAccountStructure.AddMajorComponent(glComponent2);
            Assert.AreEqual(2, glAccountStructure.MajorComponents.Count);
        }

        [TestMethod]
        public void AddMajorComponent_AddDuplicateGlComponent()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            var glComponent = ComponentBuilder.Build();

            glAccountStructure.AddMajorComponent(glComponent);
            Assert.AreEqual(1, glAccountStructure.MajorComponents.Count);

            glAccountStructure.AddMajorComponent(glComponent);
            Assert.AreEqual(1, glAccountStructure.MajorComponents.Count);
        }
        #endregion

        #region AddSubcomponent
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddSubcomponent_NullArgument()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            glAccountStructure.AddSubcomponent(null);
        }

        [TestMethod]
        public void AddSubcomponent_AddOne()
        {
            var glAccountStructure = AccountStructureBuilder.Build();
            this.glComponentName = "GL.CLASS";
            glAccountStructure.AddSubcomponent(BuildGlComponent());

            Assert.AreEqual(1, glAccountStructure.Subcomponents.Count);
            Assert.AreEqual(this.glComponentName, glAccountStructure.Subcomponents[0].ComponentName);
        }

        [TestMethod]
        public void AddSubcomponent_AddTwoDifferentGlComponents()
        {
            var glAccountStructure = AccountStructureBuilder.Build();

            this.glComponentName = "GL.CLASS";
            glAccountStructure.AddSubcomponent(BuildGlComponent());
            Assert.AreEqual(1, glAccountStructure.Subcomponents.Count);
            Assert.AreEqual(this.glComponentName, glAccountStructure.Subcomponents[0].ComponentName);

            this.glComponentName = "GL.SUBCLASS";
            glAccountStructure.AddSubcomponent(BuildGlComponent());
            Assert.AreEqual(2, glAccountStructure.Subcomponents.Count);
            Assert.AreEqual(this.glComponentName, glAccountStructure.Subcomponents[1].ComponentName);
        }

        [TestMethod]
        public void AddSubcomponent_AddDuplicateGlComponent()
        {
            var glAccountStructure = AccountStructureBuilder.Build();

            this.glComponentName = "GL.CLASS";
            glAccountStructure.AddSubcomponent(BuildGlComponent());
            Assert.AreEqual(1, glAccountStructure.Subcomponents.Count);
            Assert.AreEqual(this.glComponentName, glAccountStructure.Subcomponents[0].ComponentName);

            // Add the same component again and confirm that the original component is remains
            glAccountStructure.AddSubcomponent(BuildGlComponent());
            Assert.AreEqual(1, glAccountStructure.Subcomponents.Count);
            Assert.AreEqual(this.glComponentName, glAccountStructure.Subcomponents[0].ComponentName);
        }
        #endregion

        #region SetMajorComponentStartPositions
        [TestMethod]
        public void SetMajorComponentStartPositions_Success()
        {
            var startPositions = new List<string>() { "3", "5", "7", "12", "16" };
            var actualEntity = AccountStructureBuilder.Build();
            actualEntity.SetMajorComponentStartPositions(startPositions);
            Assert.AreEqual(startPositions.Count(), actualEntity.MajorComponentStartPositions.Count(), "List counts should match.");

            for (var i = 0; i < startPositions.Count(); i++)
            {
                var seedPosition = startPositions[i];
                Assert.AreEqual(startPositions[i], actualEntity.MajorComponentStartPositions[i], "Start positions should match.");
            }
        }

        [TestMethod]
        public void SetMajorComponentStartPositions_CallTwice()
        {
            // Set the start positions and confirm that everything was added correctly.
            var startPositions1 = new List<string>() { "3", "5", "7", "12", "16" };
            var actualEntity = AccountStructureBuilder.Build();
            actualEntity.SetMajorComponentStartPositions(startPositions1);
            Assert.AreEqual(startPositions1.Count(), actualEntity.MajorComponentStartPositions.Count(), "List counts should match.");

            for (var i = 0; i < startPositions1.Count(); i++)
            {
                var seedPosition = startPositions1[i];
                Assert.AreEqual(startPositions1[i], actualEntity.MajorComponentStartPositions[i], "Start positions should match.");
            }

            // Set the start positions with a new list and confirm that the old information was discarded.
            var startPositions2 = new List<string>() { "2", "6", "9", "15" };
            actualEntity.SetMajorComponentStartPositions(startPositions2);
            Assert.AreEqual(startPositions2.Count(), actualEntity.MajorComponentStartPositions.Count(), "List counts should match.");

            for (var i = 0; i < startPositions2.Count(); i++)
            {
                var seedPosition = startPositions1[i];
                Assert.AreEqual(startPositions2[i], actualEntity.MajorComponentStartPositions[i], "Start positions should match.");
            }
        }
        #endregion

        #region Builders
        public GeneralLedgerComponent BuildGlComponent()
        {
            return new GeneralLedgerComponent(this.glComponentName, false, GeneralLedgerComponentType.Object, "19", "1");
        }
        #endregion
    }
}