// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Describes a financial General Ledger object code object. 
    /// A GL object code is a major component as defined by each client in their GL Account Structure.
    /// This GL object code will contain all GL accounts for which the user has access,
    /// and that have the same GL object code and GL class.
    /// </summary>
    [Serializable]
    public class GlObjectCode
    {
        /// <summary>
        /// The GL object code ID.
        /// </summary>
        public string Id { get { return id; } }
        private readonly string id;

        /// <summary>
        /// The GL object code name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The GL class as defined by the GL accounts that make up the GL object code.
        /// </summary>
        public GlClass GlClass { get { return glClass; } }
        private readonly GlClass glClass;

        /// <summary>
        /// List of GL account numbers that make up the GL object code.
        /// </summary>
        public List<GlObjectCodeGlAccount> GlAccounts { get; set; }

        /// <summary>
        /// List of GL Budget pools included in the GL object code.
        /// </summary>
        public ReadOnlyCollection<GlObjectCodeBudgetPool> Pools { get; private set; }
        private readonly List<GlObjectCodeBudgetPool> pools = new List<GlObjectCodeBudgetPool>();

        /// <summary>
        /// Returns the total budget amount for all the GL accounts included in this GL object code.
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
        /// Returns the total encumbrance amount for all the GL accounts included in this GL object code.
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
        /// Returns the total actual amount for all the GL accounts included in this GL object code.
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
        /// Constructor that initializes a GL object code using a GL account number.
        /// </summary>
        /// <param name="id">The GL object code ID.</param>
        /// <param name="glAccount">A GL account number.</param>
        /// <param name="glClass">The GL class for GL object code ID.</param>
        public GlObjectCode(string id, GlObjectCodeGlAccount glAccount, GlClass glClass)
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
            this.GlAccounts = new List<GlObjectCodeGlAccount>();

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
                    throw new ApplicationException("Only umbrella accounts can be used to create a Gl Object Code.");
                }

                this.pools.Add(new GlObjectCodeBudgetPool(glAccount));
            }

            Pools = this.pools.AsReadOnly();
        }

        /// <summary>
        /// Initializes a GL object code using a budget pool.
        /// </summary>
        /// <param name="id">The ID of the GL object code.</param>
        /// <param name="budgetPool">Budget pool object.</param>
        /// <param name="glClass">The GL class for GL object code ID.</param>
        public GlObjectCode(string id, GlObjectCodeBudgetPool budgetPool, GlClass glClass)
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

            this.GlAccounts = new List<GlObjectCodeGlAccount>();
            Pools = this.pools.AsReadOnly();
        }

        /// <summary>
        /// Method to add a GL account to the list of GL accounts that make up this GL object code.
        /// </summary>
        /// <param name="glAccount">A GL account number.</param>
        public void AddGlAccount(GlObjectCodeGlAccount glAccount)
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
        /// Method to add a budget pool to the list of pools for this GL object code.
        /// </summary>
        /// <param name="pool">A GL budget pool.</param>
        public void AddBudgetPool(GlObjectCodeBudgetPool pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool", "pool cannot be null.");
            }

            this.pools.Add(pool);
        }
    }
}
