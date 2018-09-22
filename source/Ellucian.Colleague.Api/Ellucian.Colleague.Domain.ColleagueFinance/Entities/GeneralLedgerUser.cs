// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// This class extends the Person class and builds a GeneralLedgerUser object.
    /// </summary>
    [Serializable]
    public class GeneralLedgerUser : Person
    {
        /// <summary>
        /// All GL accounts to which the user has access.
        /// </summary>
        public ReadOnlyCollection<string> AllAccounts { get; private set; }
        private readonly List<string> allAccounts = new List<string>();

        /// <summary>
        /// Public getter for the list of expense general ledger accounts for which the user has access.
        /// </summary>
        public ReadOnlyCollection<string> ExpenseAccounts { get; private set; }
        private readonly List<string> expenseAccounts = new List<string>();

        /// <summary>
        /// List of revenue GL accounts to which the user has access.
        /// </summary>
        public ReadOnlyCollection<string> RevenueAccounts { get; private set; }
        private readonly List<string> revenueAccounts = new List<string>();

        /// <summary>
        /// Public getter for the private general ledger access variable.
        /// </summary>
        public GlAccessLevel GlAccessLevel { get { return glAccessLevel; } }
        private GlAccessLevel glAccessLevel;

        /// <summary>
        /// This constructor initializes a General Ledger User domain entity.
        /// </summary>
        /// <param name="id">This is the user ID.</param>
        /// <param name="lastName">This is the user last name.</param>
        public GeneralLedgerUser(string id, string lastName)
            : base(id, lastName)
        {
            AllAccounts = allAccounts.AsReadOnly();
            ExpenseAccounts = expenseAccounts.AsReadOnly();
            RevenueAccounts = revenueAccounts.AsReadOnly();
            glAccessLevel = GlAccessLevel.No_Access;
        }

        /// <summary>
        /// Add the list of GL accounts to which the user has access.
        /// </summary>
        /// <param name="allAccountIds"></param>
        public void AddAllAccounts(IEnumerable<string> allAccountIds)
        {
            if (allAccountIds != null)
            {
                allAccounts.AddRange(allAccountIds.Except(allAccounts).Distinct().Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        /// <summary>
        /// This method adds an expense account to the list of expense accounts 
        /// for which the user has access.
        /// </summary>
        /// <param name="expenseAccountIds">This is the expense account ID.</param>
        public void AddExpenseAccounts(IEnumerable<string> expenseAccountIds)
        {
            if (expenseAccountIds != null)
            {
                expenseAccounts.AddRange(expenseAccountIds.Except(expenseAccounts).Distinct().Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        /// <summary>
        /// Adds a set of revenue accounts to the list of revenue accounts for which the user has access.
        /// </summary>
        /// <param name="revenueAccountIds">This is the expense account ID.</param>
        public void AddRevenueAccounts(IEnumerable<string> revenueAccountIds)
        {
            if (revenueAccountIds != null)
            {
                revenueAccounts.AddRange(revenueAccountIds.Except(revenueAccounts).Distinct().Where(x => !string.IsNullOrEmpty(x)));
            }
        }

        /// <summary>
        /// Remove all GL account IDs stored in the AllAccounts list.
        /// </summary>
        public void RemoveAllAccounts()
        {
            allAccounts.Clear();
        }

        /// <summary>
        /// This method removes all expense account IDs from the list of expense account IDs
        /// for which the user has access.
        /// </summary>
        public void RemoveAllExpenseAccounts()
        {
            expenseAccounts.Clear();
        }

        /// <summary>
        /// This method removes all revenue account IDs from the list of revenue account IDs
        /// for which the user has access.
        /// </summary>
        public void RemoveAllRevenueAccounts()
        {
            revenueAccounts.Clear();
        }

        /// <summary>
        /// This method sets the private general ledger access level for the user.
        /// </summary>
        /// <param name="glAccess">GLAccessLevel enumeration value</param>
        public void SetGlAccessLevel(GlAccessLevel glAccess)
        {
            glAccessLevel = glAccess;
        }
    }
}

