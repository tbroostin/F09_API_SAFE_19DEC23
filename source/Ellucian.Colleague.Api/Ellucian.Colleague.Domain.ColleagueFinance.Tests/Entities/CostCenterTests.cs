// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

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
    public class CostCenterTests
    {
        #region Initialize and Cleanup
        private CostCenterBuilder CostCenterBuilderObject;
        private CostCenterSubtotalBuilder CostCenterSubtotalObject;
        private CostCenterGlAccountBuilder GlAccountBuilderObject;
        private GeneralLedgerComponentDescriptionBuilder GlComponentDescriptionBuilder;
        private CostCenterGlAccount glAccount1;
        private CostCenterGlAccount glAccount2;
        private CostCenterSubtotal subtotal1;
        private CostCenterSubtotal subtotal2;
        private List<GeneralLedgerComponentDescription> glComps1;
        private List<GeneralLedgerComponentDescription> glComps2;

        [TestInitialize]
        public void Initialize()
        {
            CostCenterBuilderObject = new CostCenterBuilder();
            CostCenterSubtotalObject = new CostCenterSubtotalBuilder();
            GlAccountBuilderObject = new CostCenterGlAccountBuilder();
            GlComponentDescriptionBuilder = new GeneralLedgerComponentDescriptionBuilder();

            // Initialize new GlAccount objects
            glAccount1 = GlAccountBuilderObject
                .WithGlAccountNumber("10_00_52001")
                .WithBudgetAmount(5839m)
                .WithActualAmount(123m)
                .WithEncumbranceAmount(50m).Build();
            glAccount2 = GlAccountBuilderObject
                .WithGlAccountNumber("10_00_52002")
                .WithBudgetAmount(94815m)
                .WithActualAmount(658m)
                .WithEncumbranceAmount(5120m).Build();

            // Initialize GL component descriptions
            glComps1 = new List<GeneralLedgerComponentDescription>();
            var glComp1 = GlComponentDescriptionBuilder.WithId("10")
                .WithComponentType(GeneralLedgerComponentType.Location).Build();
            glComp1.Description = "Main Campus";
            glComps1.Add(glComp1);

            glComps2 = new List<GeneralLedgerComponentDescription>();
            var glComp2 = GlComponentDescriptionBuilder.WithId("20")
                .WithComponentType(GeneralLedgerComponentType.Location).Build();
            glComp2.Description = "West Campus";
            glComps2.Add(glComp2);

            // Initialize new subtotal objects
            subtotal1 = CostCenterSubtotalObject
                .WithId("0000")
                .WithGlAccount(glAccount1).BuildWithGlAccount();

            subtotal2 = CostCenterSubtotalObject
                .WithId("6530")
                .WithGlAccount(glAccount2).BuildWithGlAccount();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CostCenterBuilderObject = null;
            GlAccountBuilderObject = null;
            GlComponentDescriptionBuilder = null;
            glAccount1 = null;
            glAccount2 = null;
            subtotal1 = null;
            subtotal2 = null;
            glComps1 = null;
            glComps2 = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public void CostCenterConstructor_Success()
        {
            var glComponentDescriptions = new List<GeneralLedgerComponentDescription>()
            {
                new GeneralLedgerComponentDescription("10", GeneralLedgerComponentType.Location),
                new GeneralLedgerComponentDescription("11", GeneralLedgerComponentType.Fund),
            };
            var costCenter = CostCenterBuilderObject.WithGlComponentDescriptions(glComponentDescriptions).Build();

            Assert.IsTrue(costCenter.CostCenterSubtotals.Count == 1);
            Assert.AreEqual(CostCenterBuilderObject.Id, costCenter.Id);
            Assert.IsTrue(costCenter.GlComponentDescriptions is ReadOnlyCollection<GeneralLedgerComponentDescription>);

            Assert.AreEqual(glComponentDescriptions.Count, costCenter.GlComponentDescriptions.Count);
            foreach (var componentDescription in glComponentDescriptions)
            {
                var selectedComponentDescriptions = costCenter.GlComponentDescriptions.Where(x => x.Id == componentDescription.Id && x.ComponentType == componentDescription.ComponentType).ToList();
                Assert.AreEqual(1, selectedComponentDescriptions.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterConstructor_NullId()
        {
            var costCenter = CostCenterBuilderObject.WithId(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterConstructor_EmptyId()
        {
            var costCenter = CostCenterBuilderObject.WithId("").Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CostCenterConstructor_NullSubtotal()
        {
            var costCenter = CostCenterBuilderObject.WithSubtotal(null).Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullGlComponentDescriptions()
        {
            var costCenterEntity = CostCenterBuilderObject.WithGlComponentDescriptions(null).Build();
        }
        #endregion

        #region Id and Name tests
        [TestMethod]
        public void IdAndName()
        {
            var costCenterEntity = CostCenterBuilderObject.Build();

            // Add GL components
            var glComponents = new List<GeneralLedgerComponentDescription>();
            var glComponent = GlComponentDescriptionBuilder.WithId("10")
                .WithComponentType(GeneralLedgerComponentType.Location).Build();
            glComponent.Description = "Main Campus";
            costCenterEntity.AddGeneralLedgerComponent(glComponent);
            glComponents.Add(glComponent);

            glComponent = GlComponentDescriptionBuilder.WithId("01")
                .WithComponentType(GeneralLedgerComponentType.Fund).Build();
            glComponent.Description = "Operating Fund";
            costCenterEntity.AddGeneralLedgerComponent(glComponent);
            glComponents.Add(glComponent);

            var expectedName = string.Empty;
            foreach (var component in glComponents)
            {
                // Append the delimeter if the name has already been initialized then append the next part of the name.
                if (!string.IsNullOrEmpty(expectedName))
                    expectedName += " : ";

                expectedName += component.Description;
            }
            Assert.AreEqual(expectedName, costCenterEntity.Name);
        }

        [TestMethod]
        public void NoName_NoComponentDescriptions()
        {
            var costCenterEntity = CostCenterBuilderObject.WithGlComponentDescriptions(new List<GeneralLedgerComponentDescription>()).Build();
            Assert.AreEqual("No cost center description available.", costCenterEntity.Name);
        }

        [TestMethod]
        public void NoName_ComponentDescriptionsAreNull()
        {
            var componentDescriptions = new List<GeneralLedgerComponentDescription>()
            {
                new GeneralLedgerComponentDescription("01", GeneralLedgerComponentType.Location),
                new GeneralLedgerComponentDescription("0010", GeneralLedgerComponentType.Unit)
            };

            var costCenterEntity = CostCenterBuilderObject.WithGlComponentDescriptions(componentDescriptions).Build();
            Assert.AreEqual("No cost center description available.", costCenterEntity.Name);
        }
        #endregion

        #region AddCostCenterSubtotal
        [TestMethod]
        public void AddCostCenterSubtotal_Success()
        {
            var costCenter = CostCenterBuilderObject.Build();

            CostCenterGlAccount glAcct1 = new CostCenterGlAccount("1065305308001", GlBudgetPoolType.None);
            var costCenterSubtotal = new CostCenterSubtotal("6530", glAcct1, GlClass.Expense);
            Assert.IsTrue(costCenter.CostCenterSubtotals.Count == 1);

            CostCenterGlAccount glAcct2 = new CostCenterGlAccount("1000005308001", GlBudgetPoolType.None);
            costCenter.AddCostCenterSubtotal(new CostCenterSubtotal("0000", glAcct2, GlClass.Expense));
            Assert.IsTrue(costCenter.CostCenterSubtotals.Count == 2);
        }

        [TestMethod]
        public void AddCostCenterSubtotal_DuplicateSubtotal()
        {
            CostCenterGlAccount glAcct1 = new CostCenterGlAccount("1000005308001", GlBudgetPoolType.None);
            // build the GL component descriptions
            var glComps1 = new List<GeneralLedgerComponentDescription>();
            var glComp1 = GlComponentDescriptionBuilder.WithId("10")
                .WithComponentType(GeneralLedgerComponentType.Location).Build();
            glComp1.Description = "Main Campus";
            glComps1.Add(glComp1);

            CostCenterSubtotal subtotal = new CostCenterSubtotal("0000", glAcct1, GlClass.Expense);
            var costCenter = CostCenterBuilderObject.WithSubtotal(subtotal).Build();
            Assert.IsTrue(costCenter.CostCenterSubtotals.Count == 1);

            costCenter.AddCostCenterSubtotal(subtotal);
            Assert.IsTrue(costCenter.CostCenterSubtotals.Count == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCostCenterSubtotal_NullSubtotal()
        {
            var costCenter = CostCenterBuilderObject.Build();
            costCenter.AddCostCenterSubtotal(null);
        }

        #endregion

        #region TotalBudget
        [TestMethod]
        public void TotalBudget_OnlyOneSubtotal()
        {
            var costCenter = CostCenterBuilderObject.Build();
            Assert.AreEqual(CostCenterBuilderObject.CostCenterSubtotalEntity.TotalBudget, costCenter.TotalBudgetExpenses);
        }

        [TestMethod]
        public void TotalBudget_MultipleSubtotals()
        {
            // Create the cost center object.
            var actualCostCenter = CostCenterBuilderObject.Build();

            // Add the subtotals to the "expected" builder object
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal1);
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal2);

            // Add the subtotals objects to the actual cost center object
            actualCostCenter.AddCostCenterSubtotal(subtotal1);
            actualCostCenter.AddCostCenterSubtotal(subtotal2);

            Assert.AreEqual(CostCenterBuilderObject.CostCenterEntity.CostCenterSubtotals.Sum(x => x.TotalBudget), actualCostCenter.TotalBudgetExpenses);
        }
        #endregion

        #region TotalEncumbrances
        [TestMethod]
        public void TotalEncumbrances_OnlyOneSubtotal()
        {
            var costCenter = CostCenterBuilderObject.Build();
            Assert.AreEqual(CostCenterBuilderObject.CostCenterSubtotalEntity.TotalEncumbrances, costCenter.TotalEncumbrancesExpenses);
        }

        [TestMethod]
        public void TotalEncumbrances_MultipleSubtotals()
        {
            // Create the cost center object.
            var actualCostCenter = CostCenterBuilderObject.Build();

            // Add the GlAccounts to the "expected" builder object
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal1);
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal2);

            // Add the GlAccount objects to the actual cost center object
            actualCostCenter.AddCostCenterSubtotal(subtotal1);
            actualCostCenter.AddCostCenterSubtotal(subtotal2);

            Assert.AreEqual(CostCenterBuilderObject.CostCenterEntity.CostCenterSubtotals.Sum(x => x.TotalEncumbrances), actualCostCenter.TotalEncumbrancesExpenses);
        }
        #endregion

        #region TotalActuals
        [TestMethod]
        public void TotalActuals_OnlyOneSubtotal()
        {
            var costCenter = CostCenterBuilderObject.Build();
            Assert.AreEqual(CostCenterBuilderObject.CostCenterSubtotalEntity.TotalActuals, costCenter.TotalActualsExpenses);
        }

        [TestMethod]
        public void TotalActuals_MultipleSubtotals()
        {
            // Create the cost center object.
            var actualCostCenter = CostCenterBuilderObject.Build();

            // Add the GlAccounts to the "expected" builder object
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal1);
            CostCenterBuilderObject.CostCenterEntity.AddCostCenterSubtotal(subtotal2);

            // Add the GlAccount objects to the actual cost center object
            actualCostCenter.AddCostCenterSubtotal(subtotal1);
            actualCostCenter.AddCostCenterSubtotal(subtotal2);

            Assert.AreEqual(CostCenterBuilderObject.CostCenterEntity.CostCenterSubtotals.Sum(x => x.TotalActuals), actualCostCenter.TotalActualsExpenses);
        }
        #endregion
    }
}