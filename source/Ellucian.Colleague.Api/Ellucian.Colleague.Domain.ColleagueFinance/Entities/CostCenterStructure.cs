// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Contains the information for how a GL cost center is structure from the GL account components.
    /// </summary>
    [Serializable]
    public class CostCenterStructure
    {
        /// <summary>
        /// List of cost center GL components.
        /// </summary>
        public ReadOnlyCollection<GeneralLedgerComponent> CostCenterComponents { get; private set; }
        private readonly List<GeneralLedgerComponent> costCenterComponents = new List<GeneralLedgerComponent>();

        /// <summary>
        /// List of object GL components.
        /// </summary>
        public ReadOnlyCollection<GeneralLedgerComponent> ObjectComponents { get; private set; }
        private readonly List<GeneralLedgerComponent> objectComponents = new List<GeneralLedgerComponent>();

        /// <summary>
        /// True if the cost center subtotal has been defined in Colleague.
        /// False if the cost center subtotal has not been defined.
        /// </summary>
        public bool IsCostCenterSubtotalDefined { get; set; }

        /// <summary>
        /// The GL subcomponent that the cost centers are going to use for subtotals.
        /// </summary>
        public GeneralLedgerComponent CostCenterSubtotal { get; set; }

        /// <summary>
        /// The GL component that corresponds to the UN type.
        /// </summary>
        public GeneralLedgerComponent Unit { get; set; }

        /// <summary>
        /// Constructor to initialize the cost center structure object.
        /// </summary>
        public CostCenterStructure()
        {
            CostCenterComponents = costCenterComponents.AsReadOnly();
            ObjectComponents = objectComponents.AsReadOnly();
        }

        /// <summary>
        /// Add a cost center GL component.
        /// </summary>
        /// <param name="glComponent">GlComponent object</param>
        public void AddCostCenterComponent(GeneralLedgerComponent glComponent)
        {
            if (glComponent == null)
                throw new ArgumentNullException("glComponent", "glComponent must have a value.");

            if (this.costCenterComponents.Where(x => x.ComponentName == glComponent.ComponentName).ToList().Count == 0)
                this.costCenterComponents.Add(glComponent);
        }

        /// <summary>
        /// Add an object GL component.
        /// </summary>
        /// <param name="glComponent">GlComponent object</param>
        public void AddObjectComponent(GeneralLedgerComponent glComponent)
        {
            if (glComponent == null)
                throw new ArgumentNullException("glComponent", "glComponent must have a value.");

            if (this.objectComponents.Where(x => x.ComponentName == glComponent.ComponentName).ToList().Count == 0)
                this.objectComponents.Add(glComponent);
        }
    }
}