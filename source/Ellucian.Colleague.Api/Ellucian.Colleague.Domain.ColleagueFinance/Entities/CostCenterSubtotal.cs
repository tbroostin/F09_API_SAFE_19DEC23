// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a financial cost center subtotal as defined by each client, which is a group of GL accounts.
    /// </summary>
    [Serializable]
    public class CostCenterSubtotal
    {
        /// <summary>
        /// The cost center Subtotal ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The cost center subtotal name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The GL class as defined by the GL accounts that make up the cost center subtotal.
        /// </summary>
        public GlClass GlClass { get { return glClass; } }
        private readonly GlClass glClass;

        /// <summary>
        /// True if the cost center subtotal has been defined in Colleague.
        /// False if the cost center subtotal has not been defined.
        /// </summary>
        public bool IsDefined { get; set; }

        /// <summary>
        /// List of GL account numbers that make up the cost center subtotal.
        /// </summary>
        //public ReadOnlyCollection<CostCenterGlAccount> GlAccounts { get; private set; }
        //public List<CostCenterGlAccount> glAccounts = new List<CostCenterGlAccount>();
        public List<CostCenterGlAccount> GlAccounts { get; set; }

        /// <summary>
        /// List of GL Budget pools included in the cost center subtotal.
        /// </summary>
        public ReadOnlyCollection<GlBudgetPool> Pools { get; private set; }
        private readonly List<GlBudgetPool> pools = new List<GlBudgetPool>();

        /// <summary>
        /// Returns the total budget amount for all of the expense GL accounts included in this cost center subtotal.
        /// </summary>
        public decimal TotalBudget
        {
            get
            {
                // Sum the amounts from the non-pooled accounts, the umbrellas, and any direct expenses on the umbrellas
                var budgetAmount = GlAccounts.Sum(x => x.BudgetAmount)
                    + pools.Select(x => x.Umbrella).Sum(x => x.BudgetAmount);

                return budgetAmount;
            }
        }

        /// <summary>
        /// Returns the total encumbrance amount for all of the expense GL accounts included in this cost center subtotal.
        /// </summary>
        public decimal TotalEncumbrances
        {
            get
            {
                // Sum the amounts from the non-pooled accounts, the umbrellas, and any direct expenses on the umbrellas
                var encumbranceAmount = GlAccounts.Sum(x => x.EncumbranceAmount)
                    + pools.Select(x => x.Umbrella).Sum(x => x.EncumbranceAmount);

                return encumbranceAmount;
            }
        }

        /// <summary>
        /// Returns the total actual amount for all of the expense GL accounts included in this cost center subtotal.
        /// </summary>
        public decimal TotalActuals
        {
            get
            {
                // Sum the amounts from the non-pooled accounts, the umbrellas, and any direct expenses on the umbrellas
                var actualAmount = GlAccounts.Sum(x => x.ActualAmount)
                    + pools.Select(x => x.Umbrella).Sum(x => x.ActualAmount);

                return actualAmount;
            }
        }

        /// <summary>
        /// Constructor that initializes a Cost Center subtotal using a GL account number.
        /// </summary>
        /// <param name="id">The cost center subtotal id.</param>
        /// <param name="glAccount">A GL account number.</param>
        /// <param name="glClass">The GL class for cost center subtotal id.</param>
        public CostCenterSubtotal(string id, CostCenterGlAccount glAccount, GlClass glClass)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is a required field.");
            }

            if (glAccount == null)
            {
                throw new ArgumentNullException("glAccount", "GL Account is a required field.");
            }

            this.id = id;
            this.glClass = glClass;
            this.GlAccounts = new List<CostCenterGlAccount>();

            // If the GL account is a not in a pool add it to the GL accounts list,
            // otherwise create a new pool and add the GL account to that pool.
            if (glAccount.PoolType == GlBudgetPoolType.None)
            {
                this.GlAccounts.Add(glAccount);
            }
            else
            {
                if (glAccount.PoolType != GlBudgetPoolType.Umbrella)
                {
                    throw new ApplicationException("Only umbrella accounts can be used to create a subtotal.");
                }

                this.pools.Add(new GlBudgetPool(glAccount));
            }

            //glAccounts = this.glAccounts.AsReadOnly();
            Pools = this.pools.AsReadOnly();
        }

        /// <summary>
        /// Initializes a Cost Center subtotal using a budget pool.
        /// </summary>
        /// <param name="id">The ID of the cost center.</param>
        /// <param name="budgetPool">Budget pool object.</param>
        /// <param name="glClass">The GL class for cost center subtotal id.</param>
        public CostCenterSubtotal(string id, GlBudgetPool budgetPool, GlClass glClass)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "ID is a required field.");
            }

            if (budgetPool == null)
            {
                throw new ArgumentNullException("budgetPool", "budgetPool is a required field");
            }

            this.id = id;
            this.glClass = glClass;
            this.pools.Add(budgetPool);

            //GlAccounts = this.GlAccounts.AsReadOnly();
            this.GlAccounts = new List<CostCenterGlAccount>();
            Pools = this.pools.AsReadOnly();
        }

        /// <summary>
        /// Method to add a GL account to the list of GL accounts that make up this cost center subtotal.
        /// </summary>
        /// <param name="glAccount">A GL account number.</param>
        public void AddGlAccount(CostCenterGlAccount glAccount)
        {
            if (glAccount == null)
            {
                throw new ArgumentNullException("glAccount", "The GL account cannot be null or an empty string.");
            }

            if (glAccount.PoolType != GlBudgetPoolType.None)
            {
                throw new ApplicationException("Only non-pooled accounts can be added to the GlAccounts list.");
            }

            if (!GlAccounts.Where(x => x.GlAccountNumber == glAccount.GlAccountNumber).Any())
                GlAccounts.Add(glAccount);
        }

        /// <summary>
        /// Method to add a budget pool to the list of pools for this cost center subtotal.
        /// </summary>
        /// <param name="pool">A GL budget pool.</param>
        public void AddBudgetPool(GlBudgetPool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool", "pool cannot be null.");
            }

            this.pools.Add(pool);
        }
    }
}
