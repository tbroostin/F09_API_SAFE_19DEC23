// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a financial "cost center" which is a group of cost center subtotals,
    /// each of one contains a group of GL accounts.
    /// </summary>
    [Serializable]
    public class CostCenter
    {
        /// <summary>
        /// The cost center ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The cost center name.
        /// </summary>
        public string Name
        {
            get
            {
                string costCenterName = String.Join(" : ", glComponentDescriptions
                    .Where(x => !string.IsNullOrEmpty(x.Description)).Select(x => x.Description));

                if (string.IsNullOrEmpty(costCenterName))
                    costCenterName = "No cost center description available.";

                return costCenterName;
            }
        }

        /// <summary>
        /// The part of the ID that corresponds to the unit component.
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// GL components used to calculate the cost center name.
        /// </summary>
        public ReadOnlyCollection<GeneralLedgerComponentDescription> GlComponentDescriptions { get; private set; }
        private readonly List<GeneralLedgerComponentDescription> glComponentDescriptions = new List<GeneralLedgerComponentDescription>();

        /// <summary>
        /// List of cost center subtotals that make up the cost center.
        /// </summary>
        public List<CostCenterSubtotal> CostCenterSubtotals = new List<CostCenterSubtotal>();

        /// <summary>
        /// Returns the total budget amount for all of the expense GL accounts included in this cost center.
        /// </summary>
        public decimal TotalBudgetExpenses { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Expense).Sum(x => x.TotalBudget); } }

        /// <summary>
        /// Returns the total encumbrance amount for all of the expense GL accounts included in this cost center.
        /// </summary>
        public decimal TotalEncumbrancesExpenses { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Expense).Sum(x => x.TotalEncumbrances); } }

        /// <summary>
        /// Returns the total actual amount for all of the expense GL accounts included in this cost center.
        /// </summary>
        public decimal TotalActualsExpenses { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Expense).Sum(x => x.TotalActuals); } }

        /// <summary>
        /// Returns the total budget amount for all of the revenue GL accounts included in this cost center.
        /// </summary>
        public decimal TotalBudgetRevenue { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Revenue).Sum(x => x.TotalBudget); } }

        /// <summary>
        /// Returns the total encumbrance amount for all of the revenue GL accounts included in this cost center.
        /// </summary>
        public decimal TotalEncumbrancesRevenue { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Revenue).Sum(x => x.TotalEncumbrances); } }

        /// <summary>
        /// Returns the total actual amount for all of the revenue GL accounts included in this cost center.
        /// </summary>
        public decimal TotalActualsRevenue { get { return CostCenterSubtotals.Where(y => y.GlClass == GlClass.Revenue).Sum(x => x.TotalActuals); } }

        /// <summary>
        /// Constructor that initializes a Cost Center domain entity.
        /// </summary>
        /// <param name="id">The cost center id.</param>
        /// <param name="costCenterSubtotal">A cost center subtotal.</param>
        /// <param name="glAccountsglComponentDescriptions">The list of GL component descriptions that make up this cost center subtotal name.</param>
        public CostCenter(string id, CostCenterSubtotal costCenterSubtotal, IEnumerable<GeneralLedgerComponentDescription> glComponentDescriptions)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is a required field.");
            }

            if (costCenterSubtotal == null)
            {
                throw new ArgumentNullException("costCenterSubtotal", "Cost Center Subtotal is a required field.");
            }

            if (glComponentDescriptions == null)
            {
                throw new ArgumentNullException("glComponentDescriptions", "glComponentDescriptions is a required field.");
            }

            this.id = id;
            this.CostCenterSubtotals.Add(costCenterSubtotal);
            this.glComponentDescriptions = glComponentDescriptions.ToList();
            GlComponentDescriptions = this.glComponentDescriptions.AsReadOnly();
        }

        /// <summary>
        /// Method to add a cost center subtotal to the list of cost center subtotals that make up this cost center.
        /// </summary>
        /// <param name="costCenterSubtotal">A cost center subtotal.</param>
        public void AddCostCenterSubtotal(CostCenterSubtotal costCenterSubtotal)
        {
            if (costCenterSubtotal == null)
            {
                throw new ArgumentNullException("costCenterSubtotal", "The cost center subtotal cannot be null.");
            }

            if (CostCenterSubtotals.Where(x => x.Id == costCenterSubtotal.Id).ToList().Count == 0)
            {
                CostCenterSubtotals.Add(costCenterSubtotal);
            }
        }

        /// <summary>
        /// Method to add a general ledger component description to the list of general ledger component description for the cost center.
        /// </summary>
        /// <param name="glComponent"></param>
        public void AddGeneralLedgerComponent(GeneralLedgerComponentDescription glComponent)
        {
            if (!this.glComponentDescriptions.Where(x => x.Id == glComponent.Id && x.ComponentType == glComponent.ComponentType).Any())
            {
                this.glComponentDescriptions.Add(glComponent);
            }
        }
    }
}
