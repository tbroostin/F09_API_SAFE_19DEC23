// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the ICostCenterRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy), System.Runtime.InteropServices.GuidAttribute("D1E2A0B4-E0C4-47C3-AA8D-5DA69DF4340D")]
    public class CostCenterRepository : BaseColleagueRepository, ICostCenterRepository
    {
        private List<GeneralLedgerComponentDescription> glComponentsToBulkRead;
        private List<string> costCenterSubtotalIdToBulkRead;
        private bool isCostCenterSubtotalDefined;
        private string costCenterId;
        private List<CostCenter> costCenters;
        private bool checkFinancialIndicatorFilter;
        private bool includeGlAccount;
        private List<string> excludedUmbrellaGlAccounts;
        private const int ThirtyMinuteCacheTimeout = 30;

        /// <summary>
        /// This constructor allows us to instantiate a GL cost center repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public CostCenterRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            glComponentsToBulkRead = new List<GeneralLedgerComponentDescription>();
            costCenterSubtotalIdToBulkRead = new List<string>();
            costCenterId = "";
            costCenters = new List<CostCenter>();
            includeGlAccount = false;
            checkFinancialIndicatorFilter = false;
            excludedUmbrellaGlAccounts = new List<string>();
        }

        /// <summary>
        /// This method gets the list of cost centers assigned to the user.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="costCenterStructure">List of objects with information to build the cost center definitions.</param>
        /// <param name="selectedCostCenterId">Null to return all cost centers for the user, or a cost center ID to return just that one.</param>
        /// <param name="glClassConfiguration">General Ledger Class configuration information.</param>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>Returns the list of cost centers assigned to the user or the selected cost center.</returns>
        public async Task<IEnumerable<CostCenter>> GetCostCentersAsync(GeneralLedgerUser generalLedgerUser, CostCenterStructure costCenterStructure,
            GeneralLedgerClassConfiguration glClassConfiguration, string selectedCostCenterId, string fiscalYear, CostCenterQueryCriteria costCenterCriteria,
            string personId)
        {
            #region Error checking
            if (generalLedgerUser == null)
            {
                LogDataError("generalLedgerUser", "", generalLedgerUser);
                return costCenters;
            }

            if (costCenterStructure == null)
            {
                LogDataError("costCenterStructure", "", costCenterStructure);
                return costCenters;
            }

            if (glClassConfiguration == null)
            {
                LogDataError("glClassConfiguration", "", glClassConfiguration);
                return costCenters;
            }

            // If the user does not have any expense or revenue accounts assigned, return an empty list of cost centers.
            if ((generalLedgerUser.ExpenseAccounts == null || !generalLedgerUser.ExpenseAccounts.Any()) &&
                (generalLedgerUser.RevenueAccounts == null || !generalLedgerUser.RevenueAccounts.Any()))
            {
                LogDataError("ExpenseAccounts", "", generalLedgerUser.ExpenseAccounts);
                LogDataError("RevenueAccounts", "", generalLedgerUser.RevenueAccounts);
                return costCenters;
            }

            // Read the GEN.LDGR record for the fiscal year
            var genLdgrDataContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear);

            if (genLdgrDataContract == null)
            {
                logger.Warn("Missing GEN.LDGR record for ID: " + fiscalYear);
                return costCenters;
            }

            if (string.IsNullOrEmpty(genLdgrDataContract.GenLdgrStatus))
            {
                logger.Warn("GEN.LDGR status is null.");
                return costCenters;
            }

            #endregion

            // If no cost center ID is passed in, we want to return information for all cost centers
            // the user has access to. If a cost center ID is passed in, the user has selected one
            // cost center and we want to return information for just that one cost center.

            // Cost Centers are populated with the expense and revenue GL accounts a user has access to.

            // Get the list of expense and revenue GL accounts the user has access to and combine them.
            string[] expenseRevenueGlAccounts = generalLedgerUser.ExpenseAccounts.Union(generalLedgerUser.RevenueAccounts).ToArray();
            string[] allExpenseRevenueAccounts = expenseRevenueGlAccounts;

            // If we are only processing the one selected cost center passed in, then loop
            // through each GL account the user has access to, determine its cost center ID,
            // and then compare it to the specified cost center ID passed into the method.
            // If the GL account belongs to that cost center, add it to a list for bulk read
            // of the appropriate records.
            var selectedCostCenterGlAccounts = new List<string>();

            // Check whether we only want one cost center.
            if (!string.IsNullOrEmpty(selectedCostCenterId))
            {
                // Loop through each expense/revenue GL account the user has access to.
                foreach (var expenseacct in expenseRevenueGlAccounts)
                {
                    // Determine the cost center ID for this expense/revenue GL account
                    var expenseGlAccountCostCenterId = string.Empty;
                    foreach (var component in costCenterStructure.CostCenterComponents)
                    {
                        if (component != null)
                        {
                            var componentId = expenseacct.Substring(component.StartPosition, component.ComponentLength);
                            expenseGlAccountCostCenterId += componentId;
                        }
                    }

                    // If the GL account cost center ID matches the selected one,
                    // add the GL account to the list of GL accounts to process.
                    if (expenseGlAccountCostCenterId == selectedCostCenterId)
                    {
                        selectedCostCenterGlAccounts.Add(expenseacct);
                    }
                }

                expenseRevenueGlAccounts = selectedCostCenterGlAccounts.ToArray();
            }

            #region Apply filter criteria

            // If we have filter component criteria, limit the list of expense/revenue
            // accounts using the filter component criteria.
            if (expenseRevenueGlAccounts.Any())
            {
                if (costCenterCriteria != null)
                {
                    if (costCenterCriteria.ComponentCriteria.Any())
                    {
                        string componentFilterCriteria = null;
                        string valueFilterCriteria = null;
                        string rangeFilterCriteria = null;
                        string[] filteredExpenseRevenueAccounts = expenseRevenueGlAccounts;
                        string[] tempFilteredAccounts;
                        foreach (var filterComp in costCenterCriteria.ComponentCriteria)
                        {
                            // Set a running limiting list of expense/revenue accounts that are
                            // filtered using the filtering criteria for each component.
                            // And, reset the filter criteria strings to null.
                            if (filteredExpenseRevenueAccounts.Any())
                            {
                                tempFilteredAccounts = filteredExpenseRevenueAccounts;
                                valueFilterCriteria = null;
                                rangeFilterCriteria = null;
                                componentFilterCriteria = null;

                                // Set the value filter criteria string from the individual component values.
                                if (filterComp.IndividualComponentValues.Any())
                                {
                                    foreach (var value in filterComp.IndividualComponentValues)
                                    {
                                        valueFilterCriteria = valueFilterCriteria + "'" + value + "' ";
                                    }
                                }

                                // Set the range filter criteria string from the component range values.
                                if (filterComp.RangeComponentValues.Any())
                                {
                                    foreach (var range in filterComp.RangeComponentValues)
                                    {
                                        if (string.IsNullOrEmpty(rangeFilterCriteria))
                                        {
                                            rangeFilterCriteria = rangeFilterCriteria + "(" + filterComp.ComponentName + " GE '"
                                                + range.StartValue + "' AND " + filterComp.ComponentName + " LE '" + range.EndValue + "') ";
                                        }
                                        else
                                        {
                                            rangeFilterCriteria = rangeFilterCriteria + "OR (" + filterComp.ComponentName + " GE '"
                                                + range.StartValue + "' AND " + filterComp.ComponentName + " LE '" + range.EndValue + "') ";
                                        }
                                    }
                                }

                                // Update the value filter criteria string to contain the component name
                                if (!string.IsNullOrEmpty(valueFilterCriteria))
                                {
                                    valueFilterCriteria = filterComp.ComponentName + " EQ " + valueFilterCriteria;
                                }

                                // Set the full component filter criteria string based on the value filter string
                                // and the range value filter string.
                                componentFilterCriteria = valueFilterCriteria;
                                if (!string.IsNullOrEmpty(rangeFilterCriteria))
                                {
                                    if (string.IsNullOrEmpty(componentFilterCriteria))
                                    {
                                        componentFilterCriteria = rangeFilterCriteria;
                                    }
                                    else
                                    {
                                        componentFilterCriteria = componentFilterCriteria + "OR " + rangeFilterCriteria;
                                    }
                                }

                                filteredExpenseRevenueAccounts = await DataReader.SelectAsync("GL.ACCTS", tempFilteredAccounts, componentFilterCriteria);
                            }
                        }
                        expenseRevenueGlAccounts = filteredExpenseRevenueAccounts;
                    }
                }
            }
            #endregion

            // Evaluate whether to check the financial indicator filter options for expense accounts later on.
            if (costCenterCriteria != null)
            {
                if ((costCenterCriteria.FinancialThresholds != null) && (costCenterCriteria.FinancialThresholds.Any()))
                {
                    // If the user has selected all three options no need to check further because that
                    // means include all GL accounts regardless of the financial indicator choices.
                    bool underThreshold = costCenterCriteria.FinancialThresholds.Contains(FinancialThreshold.UnderThreshold);
                    bool nearThreshold = costCenterCriteria.FinancialThresholds.Contains(FinancialThreshold.NearThreshold);
                    bool overThreshold = costCenterCriteria.FinancialThresholds.Contains(FinancialThreshold.OverThreshold);

                    if (underThreshold && nearThreshold && overThreshold == true)
                    {
                        // User has selected all three options.
                        checkFinancialIndicatorFilter = false;
                    }
                    else
                    {
                        // User has selected one or two options.
                        checkFinancialIndicatorFilter = true;
                    }
                }
                else
                {
                    // No financial healther indicator filter options have been checked.
                    checkFinancialIndicatorFilter = false;
                }
            }

            // If the fiscal year is open, get the information from GL.ACCTS
            Collection<GlAccts> glAccountRecords = new Collection<GlAccts>();
            if (genLdgrDataContract.GenLdgrStatus == "O")
            {
                #region Get open year amounts from GL.ACCTS

                // Bulk read the GL Account records. At this point expenseAccounts contains the list of revenue/expense
                // GL accounts the user has access for all cost centers or just for the selected cost center.
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria) && string.IsNullOrEmpty(selectedCostCenterId))
                {
                    glAccountRecords = await GetOrAddToCacheAsync<Collection<GlAccts>>("CostCentersGlAccts" + personId, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<GlAccts>(expenseRevenueGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    glAccountRecords = await DataReader.BulkReadRecordAsync<GlAccts>(expenseRevenueGlAccounts);
                }

                // Create three unique lists of data contracts for the given fiscal year: umbrellas, poolees, non-pooled
                var umbrellaAccounts = new List<GlAccts>();
                var pooleeAccounts = new List<GlAccts>();
                var nonPooledAccounts = new List<GlAccts>();
                if (glAccountRecords != null)
                {
                    umbrellaAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                        && x.MemosEntityAssociation.Where(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && y.GlPooledTypeAssocMember.ToUpper() == "U").Any()).ToList();
                    pooleeAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                        && x.MemosEntityAssociation.Where(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && y.GlPooledTypeAssocMember.ToUpper() == "P").Any()).ToList();
                    nonPooledAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                        && x.MemosEntityAssociation.Where(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && string.IsNullOrEmpty(y.GlPooledTypeAssocMember)).Any()).ToList();
                }

                var glBudgetPoolDomainEntities = new List<GlBudgetPool>();
                var nonPooledGlAccountDomainEntities = new List<CostCenterGlAccount>();

                #region Process all umbrella accounts

                foreach (var umbrella in umbrellaAccounts)
                {
                    if (umbrella != null)
                    {
                        // Determine the GL class for the GL account.
                        var glClass = GetGlAccountGlClass(umbrella.Recordkey, glClassConfiguration);

                        // Only use the financial health indicator filter criteria if the GL account is expense.
                        if (glClass != GlClass.Expense)
                        {
                            includeGlAccount = true;
                        }
                        else
                        {
                            if (checkFinancialIndicatorFilter)
                            {
                                // If specified in the filter criteria, select those expense GL accounts
                                // that meet the financial health indicator values selected by the user.
                                includeGlAccount = FilterUtility.CheckFinancialHealthIndicatorFilterForGlAcct(umbrella, "U", fiscalYear, costCenterCriteria);
                            }
                            else
                            {
                                includeGlAccount = true;
                            }
                        }

                        if (includeGlAccount)
                        {
                            // Determine this GL account cost center ID and the necessary components for its description.
                            var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, umbrella.Recordkey);

                            // Add the necessary components to process for the descriptions to a list for bulk read later.
                            GatherGlDescriptionComponents(glComponentsForCostCenter);

                            var glAccountAmounts = umbrella.MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == fiscalYear).FirstOrDefault();

                            // If the GL account has information for the fiscal year passed in, process the amounts.
                            if (glAccountAmounts != null)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(umbrella.Recordkey))
                                    {
                                        throw new ApplicationException("RecordKey is a required field for GlAccts record.");
                                    }

                                    // Create GL account for umbrella and populate budget, actual and encumbrance amounts.
                                    var umbrellaGlAccount = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                                    umbrellaGlAccount.BudgetAmount = glAccountAmounts.FaBudgetPostedAssocMember.HasValue ? glAccountAmounts.FaBudgetPostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.BudgetAmount += glAccountAmounts.FaBudgetMemoAssocMember.HasValue ? glAccountAmounts.FaBudgetMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaEncumbranceMemoAssocMember.HasValue ? glAccountAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaRequisitionMemoAssocMember.HasValue ? glAccountAmounts.FaRequisitionMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.ActualAmount += glAccountAmounts.FaActualPostedAssocMember.HasValue ? glAccountAmounts.FaActualPostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.ActualAmount += glAccountAmounts.FaActualMemoAssocMember.HasValue ? glAccountAmounts.FaActualMemoAssocMember.Value : 0m;

                                    // Create new pool for umbrella
                                    var newBudgetPool = new GlBudgetPool(umbrellaGlAccount);
                                    newBudgetPool.IsUmbrellaVisible = true;

                                    // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                                    if ((glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue && glAccountAmounts.GlEncumbrancePostedAssocMember.Value != 0)
                                        || (glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue && glAccountAmounts.GlEncumbranceMemosAssocMember.Value != 0)
                                        || (glAccountAmounts.GlRequisitionMemosAssocMember.HasValue && glAccountAmounts.GlRequisitionMemosAssocMember.Value != 0)
                                        || (glAccountAmounts.GlActualPostedAssocMember.HasValue && glAccountAmounts.GlActualPostedAssocMember.Value != 0)
                                        || (glAccountAmounts.GlActualMemosAssocMember.HasValue && glAccountAmounts.GlActualMemosAssocMember.Value != 0))
                                    {
                                        var umbrellaPoolee = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
                                        umbrellaPoolee.EncumbranceAmount += glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                                        umbrellaPoolee.EncumbranceAmount += glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? glAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                                        umbrellaPoolee.EncumbranceAmount += glAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? glAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
                                        umbrellaPoolee.ActualAmount += glAccountAmounts.GlActualPostedAssocMember.HasValue ? glAccountAmounts.GlActualPostedAssocMember.Value : 0m;
                                        umbrellaPoolee.ActualAmount += glAccountAmounts.GlActualMemosAssocMember.HasValue ? glAccountAmounts.GlActualMemosAssocMember.Value : 0m;

                                        newBudgetPool.AddPoolee(umbrellaPoolee);
                                    }

                                    // Add new budget pool to the list of new budget pools
                                    glBudgetPoolDomainEntities.Add(newBudgetPool);
                                }
                                catch (ApplicationException aex)
                                {
                                    LogDataError("", "", umbrella, aex);
                                }
                            }
                        }
                        else
                        {
                            excludedUmbrellaGlAccounts.Add(umbrella.Recordkey);
                        }
                    }
                }
                #endregion

                #region Get poolee from outside of the selected cost center

                if (!string.IsNullOrEmpty(selectedCostCenterId))
                {
                    // If the repository is being limited to a single cost center,
                    // determine if there are poolee from another cost center that
                    // the user has access to, and add the GL.ACCTS data contract
                    // to the list of poolees for the cost center.
                    string[] glpPooleeAccounts = null;
                    List<string> selectedGlpPooleeAccounts = new List<string>();

                    selectedGlpPooleeAccounts = await GetPooleesOutsideCostCenter(generalLedgerUser, selectedCostCenterId, fiscalYear, costCenterStructure, costCenterCriteria);
                    if (selectedGlpPooleeAccounts.Any())
                    {
                        // Bulk read the GL.ACCTS records for the poolee accounts from 
                        // another cost center that the user has access to.
                        glpPooleeAccounts = selectedGlpPooleeAccounts.ToArray();
                        glAccountRecords = await DataReader.BulkReadRecordAsync<GlAccts>(glpPooleeAccounts);
                        foreach (var pooleeAccount in glAccountRecords)
                        {
                            pooleeAccounts.Add(pooleeAccount);
                        }
                    }
                }
                #endregion

                #region Process all poolee accounts

                foreach (var poolee in pooleeAccounts)
                {
                    if (poolee != null)
                    {
                        // Figure out the umbrella for this poolee
                        var umbrellaGlAccount = "";
                        var umbrella = poolee.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
                        if (umbrella != null)
                        {
                            umbrellaGlAccount = umbrella.GlBudgetLinkageAssocMember;
                        }

                        // Only process this poolee if the umbrella has not been excluded.
                        if (!excludedUmbrellaGlAccounts.Contains(umbrellaGlAccount))
                        {
                            // Determine the cost center ID for the umbrella, if we are processing a single cost center.
                            var umbrellaCostCenterId = string.Empty;
                            if (!string.IsNullOrEmpty(selectedCostCenterId))
                            {
                                if (!string.IsNullOrEmpty(umbrellaGlAccount))
                                {
                                    foreach (var component in costCenterStructure.CostCenterComponents)
                                    {
                                        if (component != null)
                                        {
                                            var componentId = umbrellaGlAccount.Substring(component.StartPosition, component.ComponentLength);
                                            umbrellaCostCenterId += componentId;
                                        }
                                    }
                                }
                            }

                            // If we are processing one cost center and the umbrella GL account cost center ID matches the selected one,
                            // or if we are processing all cost centers, process the poolee. If we are processing one cost center and the
                            // umbrella GL account cost center ID does not match the selected on, ignore it.
                            if (string.IsNullOrEmpty(selectedCostCenterId) || umbrellaCostCenterId == selectedCostCenterId)
                            {
                                // Determine this GL account cost center ID and the necessary components for its description.
                                var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, poolee.Recordkey);

                                // Add the necessary components to process for the descriptions to a list for bulk read later.
                                GatherGlDescriptionComponents(glComponentsForCostCenter);

                                var glAccountAmounts = poolee.MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == fiscalYear).FirstOrDefault();

                                // If the GL account has information for the fiscal year passed in, process the amounts.
                                if (glAccountAmounts != null)
                                {
                                    try
                                    {
                                        if (string.IsNullOrEmpty(poolee.Recordkey))
                                        {
                                            throw new ApplicationException("RecordKey is a required field for GlAccts record.");
                                        }

                                        // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                                        var pooleeGlAccount = new CostCenterGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
                                        pooleeGlAccount.BudgetAmount = glAccountAmounts.GlBudgetPostedAssocMember.HasValue ? glAccountAmounts.GlBudgetPostedAssocMember.Value : 0m;
                                        pooleeGlAccount.BudgetAmount += glAccountAmounts.GlBudgetMemosAssocMember.HasValue ? glAccountAmounts.GlBudgetMemosAssocMember.Value : 0m;
                                        pooleeGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                                        pooleeGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? glAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                                        pooleeGlAccount.EncumbranceAmount += glAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? glAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
                                        pooleeGlAccount.ActualAmount += glAccountAmounts.GlActualPostedAssocMember.HasValue ? glAccountAmounts.GlActualPostedAssocMember.Value : 0m;
                                        pooleeGlAccount.ActualAmount += glAccountAmounts.GlActualMemosAssocMember.HasValue ? glAccountAmounts.GlActualMemosAssocMember.Value : 0m;

                                        // Add the poolee to the appropriate pool.
                                        var selectedPool = glBudgetPoolDomainEntities.FirstOrDefault(x => x.Umbrella.GlAccountNumber == umbrellaGlAccount);
                                        if (selectedPool != null)
                                        {
                                            selectedPool.AddPoolee(pooleeGlAccount);
                                        }
                                        else
                                        {
                                            // If the user has access to poolees from a budget pool for which they don't have access
                                            // to the umbrella, we will need to read the GL account for the umbrella to get the budget
                                            // pool amounts. But, we don't have to worry about whether the umbrella has direct expenses
                                            // because the user doesn't have access to see them anyway.

                                            // Read the GL.ACCTS record for the umbrella, and get the amounts for fiscal year.
                                            var umbrellaAccount = await DataReader.ReadRecordAsync<GlAccts>(umbrellaGlAccount);

                                            if (umbrellaAccount == null)
                                            {
                                                throw new ApplicationException("The GL.ACCTS data contract is null for GL number " + umbrellaGlAccount);
                                            }

                                            if (string.IsNullOrEmpty(umbrellaAccount.Recordkey))
                                            {
                                                throw new ApplicationException("RecordKey is a required field for GlAccts record.");
                                            }

                                            // Create the cost center GL account DE for the umbrella.
                                            var newUmbrellaGlAccount = new CostCenterGlAccount(umbrellaGlAccount, GlBudgetPoolType.Umbrella);

                                            // Determine this GL account cost center ID and the necessary components for its description.
                                            var umbrellaGlComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, umbrellaGlAccount);

                                            // Add the necessary components to process for the descriptions to a list for bulk read later.
                                            GatherGlDescriptionComponents(umbrellaGlComponentsForCostCenter);

                                            var umbrellaGlAccountAmounts = umbrellaAccount.MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == fiscalYear).FirstOrDefault();

                                            // If the GL account has information for the fiscal year passed in, process the amounts.
                                            if (umbrellaGlAccountAmounts != null)
                                            {
                                                newUmbrellaGlAccount.BudgetAmount = umbrellaGlAccountAmounts.FaBudgetPostedAssocMember.HasValue ? umbrellaGlAccountAmounts.FaBudgetPostedAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.BudgetAmount += umbrellaGlAccountAmounts.FaBudgetMemoAssocMember.HasValue ? umbrellaGlAccountAmounts.FaBudgetMemoAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.EncumbranceAmount += umbrellaGlAccountAmounts.FaEncumbrancePostedAssocMember.HasValue ? umbrellaGlAccountAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.EncumbranceAmount += umbrellaGlAccountAmounts.FaEncumbranceMemoAssocMember.HasValue ? umbrellaGlAccountAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.EncumbranceAmount += umbrellaGlAccountAmounts.FaRequisitionMemoAssocMember.HasValue ? umbrellaGlAccountAmounts.FaRequisitionMemoAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.ActualAmount += umbrellaGlAccountAmounts.FaActualPostedAssocMember.HasValue ? umbrellaGlAccountAmounts.FaActualPostedAssocMember.Value : 0m;
                                                newUmbrellaGlAccount.ActualAmount += umbrellaGlAccountAmounts.FaActualMemoAssocMember.HasValue ? umbrellaGlAccountAmounts.FaActualMemoAssocMember.Value : 0m;

                                                // Create pool for umbrella.
                                                var newBudgetPool = new GlBudgetPool(newUmbrellaGlAccount);
                                                if (allExpenseRevenueAccounts.Contains(poolee.Recordkey))
                                                {
                                                    // If the user has access to the umbrella, make it visible.
                                                    newBudgetPool.IsUmbrellaVisible = true;
                                                }

                                                // Add poolee to new pool.
                                                newBudgetPool.AddPoolee(pooleeGlAccount);

                                                // Add new budget pool to the list of new budget pools
                                                glBudgetPoolDomainEntities.Add(newBudgetPool);
                                            }
                                        }
                                    }
                                    catch (ApplicationException aex)
                                    {
                                        LogDataError("", "", umbrella, aex);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Process all non-pooled accounts

                foreach (var nonPooledAccount in nonPooledAccounts)
                {
                    // Determine the GL class for the GL account.
                    var glClass = GetGlAccountGlClass(nonPooledAccount.Recordkey, glClassConfiguration);

                    // Only use the financial health indicator filter criteria if the GL account is an expense one.
                    if (glClass != GlClass.Expense)
                    {
                        includeGlAccount = true;
                    }
                    else
                    {
                        if (checkFinancialIndicatorFilter)
                        {
                            // If specified in the filter criteria, select those expense GL accounts
                            // that meet the financial health indicator values selected by the user.
                            includeGlAccount = FilterUtility.CheckFinancialHealthIndicatorFilterForGlAcct(nonPooledAccount, "", fiscalYear, costCenterCriteria);
                        }
                        else
                        {
                            includeGlAccount = true;
                        }
                    }

                    if (includeGlAccount)
                    {
                        // Determine this GL account cost center ID and the necessary components for its description.
                        var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, nonPooledAccount.Recordkey);

                        // Add the necessary components to process for the descriptions to a list for bulk read later.
                        GatherGlDescriptionComponents(glComponentsForCostCenter);

                        var glAccountAmounts = nonPooledAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

                        // If the GL account has information for the fiscal year passed in, process the amounts.
                        if (glAccountAmounts != null)
                        {
                            // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                            var nonPooledGlAccount = new CostCenterGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                            nonPooledGlAccount.BudgetAmount = glAccountAmounts.GlBudgetPostedAssocMember.HasValue ? glAccountAmounts.GlBudgetPostedAssocMember.Value : 0m;
                            nonPooledGlAccount.BudgetAmount += glAccountAmounts.GlBudgetMemosAssocMember.HasValue ? glAccountAmounts.GlBudgetMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? glAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? glAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.ActualAmount += glAccountAmounts.GlActualPostedAssocMember.HasValue ? glAccountAmounts.GlActualPostedAssocMember.Value : 0m;
                            nonPooledGlAccount.ActualAmount += glAccountAmounts.GlActualMemosAssocMember.HasValue ? glAccountAmounts.GlActualMemosAssocMember.Value : 0m;

                            // Add the GL account to the list of non-pooled accounts.
                            nonPooledGlAccountDomainEntities.Add(nonPooledGlAccount);
                        }
                    }
                }
                #endregion

                #region Assign all pools to a cost center and subtotal

                foreach (var budgetPool in glBudgetPoolDomainEntities)
                {
                    AddBudgetPoolToCostCentersList(budgetPool, costCenterStructure, glClassConfiguration);
                }
                #endregion

                #region Assign all non-pooled GL accounts to a cost center and subtotal

                foreach (var nonPooledAccount in nonPooledGlAccountDomainEntities)
                {
                    AddNonPooledAccountToCostCentersList(nonPooledAccount, costCenterStructure, glClassConfiguration);
                }

                #endregion

                #endregion
            }
            else
            {
                #region Get closed year amounts from GLS.FYR

                // If the fiscal year is closed, obtain the information from GLS.FYR and ENC.FYR.
                // Bulk read the GLS.FYR records.
                var glsFyrId = "GLS." + fiscalYear;
                Collection<GlsFyr> glsRecords = new Collection<GlsFyr>();
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria) && string.IsNullOrEmpty(selectedCostCenterId))
                {
                    glsRecords = await GetOrAddToCacheAsync<Collection<GlsFyr>>("CostCentersGlsFyr" + personId + costCenterCriteria.FiscalYear, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrId, expenseRevenueGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    glsRecords = await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrId, expenseRevenueGlAccounts);
                }

                // Create three unique lists of data contracts for the given fiscal year: umbrellas, poolees, non-pooled
                var umbrellaAccounts = new List<GlsFyr>();
                var pooleeAccounts = new List<GlsFyr>();
                var nonPooledAccounts = new List<GlsFyr>();
                if (glsRecords != null)
                {
                    umbrellaAccounts = glsRecords.Where(x => x.GlsPooledType.ToUpper() == "U").ToList();
                    pooleeAccounts = glsRecords.Where(x => x.GlsPooledType.ToUpper() == "P").ToList();
                    nonPooledAccounts = glsRecords.Where(x => string.IsNullOrEmpty(x.GlsPooledType)).ToList();
                }

                var glBudgetPoolDomainEntities = new List<GlBudgetPool>();
                var nonPooledGlAccountDomainEntities = new List<CostCenterGlAccount>();

                #region Process all umbrella accounts

                foreach (var umbrella in umbrellaAccounts)
                {
                    if (umbrella != null)
                    {
                        // Determine the GL class for the GL account.
                        var glClass = GetGlAccountGlClass(umbrella.Recordkey, glClassConfiguration);

                        // Only use the financial health indicator filter criteria if the GL account is expense.
                        if (glClass != GlClass.Expense)
                        {
                            includeGlAccount = true;
                        }
                        else
                        {
                            if (checkFinancialIndicatorFilter)
                            {
                                // If specified in the filter criteria, select those expense GL accounts
                                // that meet the financial health indicator values selected by the user.
                                includeGlAccount = FilterUtility.CheckFinancialHealthIndicatorFilterForGlsFyr(umbrella, "U", costCenterCriteria);
                            }
                            else
                            {
                                includeGlAccount = true;
                            }
                        }

                        if (includeGlAccount)
                        {
                            // Determine this GL account cost center ID and the necessary components for its description.
                            var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, umbrella.Recordkey);

                            // Add the necessary components to process for the descriptions to a list for bulk read later.
                            GatherGlDescriptionComponents(glComponentsForCostCenter);

                            // Create GL account for umbrella and populate budget, actual and encumbrance amounts.
                            var umbrellaGlAccount = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                            umbrellaGlAccount.BudgetAmount += umbrella.BAlocDebitsYtd.HasValue ? umbrella.BAlocDebitsYtd.Value : 0m;
                            umbrellaGlAccount.BudgetAmount -= umbrella.BAlocCreditsYtd.HasValue ? umbrella.BAlocCreditsYtd.Value : 0m;
                            foreach (var amount in umbrella.GlsFaMactuals)
                            {
                                umbrellaGlAccount.ActualAmount += amount.HasValue ? amount.Value : 0m;
                            }

                            foreach (var amount in umbrella.GlsFaMencumbrances)
                            {
                                umbrellaGlAccount.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;
                            }

                            // Create new pool for umbrella
                            var newBudgetPool = new GlBudgetPool(umbrellaGlAccount);
                            newBudgetPool.IsUmbrellaVisible = true;

                            // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                            if ((umbrella.CreditsYtd.HasValue && umbrella.CreditsYtd.Value != 0)
                                || (umbrella.DebitsYtd.HasValue && umbrella.DebitsYtd.Value != 0)
                                || (umbrella.EOpenBal.HasValue && umbrella.EOpenBal.Value != 0)
                                || (umbrella.EncumbrancesYtd.HasValue && umbrella.EncumbrancesYtd.Value != 0)
                                || (umbrella.EncumbrancesRelievedYtd.HasValue && umbrella.EncumbrancesRelievedYtd.Value != 0))
                            {
                                var umbrellaPoolee = new CostCenterGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
                                umbrellaPoolee.EncumbranceAmount += umbrella.EOpenBal.HasValue ? umbrella.EOpenBal.Value : 0m;
                                umbrellaPoolee.EncumbranceAmount += umbrella.EncumbrancesYtd.HasValue ? umbrella.EncumbrancesYtd.Value : 0m;
                                umbrellaPoolee.EncumbranceAmount -= umbrella.EncumbrancesRelievedYtd.HasValue ? umbrella.EncumbrancesRelievedYtd.Value : 0m;
                                umbrellaPoolee.ActualAmount += umbrella.DebitsYtd.HasValue ? umbrella.DebitsYtd.Value : 0m;
                                umbrellaPoolee.ActualAmount -= umbrella.CreditsYtd.HasValue ? umbrella.CreditsYtd.Value : 0m;

                                newBudgetPool.AddPoolee(umbrellaPoolee);
                            }

                            // Add new budget pool to the list of new budget pools
                            glBudgetPoolDomainEntities.Add(newBudgetPool);
                        }
                        else
                        {
                            excludedUmbrellaGlAccounts.Add(umbrella.Recordkey);
                        }
                    }
                }
                #endregion

                #region Get poolee from outside of the selected cost center

                List<string> selectedGlpPooleeAccounts = new List<string>();
                if (!string.IsNullOrEmpty(selectedCostCenterId))
                {
                    // If the repository is being limited to a single cost center,
                    // determine if there are poolee from another cost center that
                    // the user has access to, and add the GLS data contract
                    // to the list of poolees for the cost center.
                    string[] glpPooleeAccounts = null;

                    selectedGlpPooleeAccounts = await GetPooleesOutsideCostCenter(generalLedgerUser, selectedCostCenterId, fiscalYear, costCenterStructure, costCenterCriteria);

                    if (selectedGlpPooleeAccounts.Any())
                    {
                        // Bulk read the GLS records for the poolee accounts from 
                        // another cost center that the user has access to.
                        glpPooleeAccounts = selectedGlpPooleeAccounts.ToArray();
                        glsRecords = await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrId, glpPooleeAccounts);
                        foreach (var pooleeAccount in glsRecords)
                        {
                            pooleeAccounts.Add(pooleeAccount);
                        }
                    }
                }
                #endregion

                #region Process all poolee accounts

                foreach (var poolee in pooleeAccounts)
                {
                    if (poolee != null)
                    {
                        try
                        {
                            // Figure out the umbrella for this poolee
                            var umbrellaGlNumber = poolee.GlsBudgetLinkage;

                            // Only process this poolee if the umbrella has not been excluded.
                            if (!excludedUmbrellaGlAccounts.Contains(umbrellaGlNumber))
                            {
                                // Determine the cost center ID for the umbrella, if we are processing a single cost center.
                                var umbrellaCostCenterId = string.Empty;
                                if (!string.IsNullOrEmpty(selectedCostCenterId))
                                {
                                    if (!string.IsNullOrEmpty(umbrellaGlNumber))
                                    {
                                        foreach (var component in costCenterStructure.CostCenterComponents)
                                        {
                                            if (component != null)
                                            {
                                                var componentId = umbrellaGlNumber.Substring(component.StartPosition, component.ComponentLength);
                                                umbrellaCostCenterId += componentId;
                                            }
                                        }
                                    }
                                }

                                // If we are processing one cost center and the umbrella GL account cost center ID matches the selected one,
                                // or if we are processing all cost centers, process the poolee. If we are processing one cost center and the
                                // umbrella GL account cost center ID does not match the selected on, ignore it.
                                if (string.IsNullOrEmpty(selectedCostCenterId) || umbrellaCostCenterId == selectedCostCenterId)
                                {
                                    // Determine this GL account cost center ID and the necessary components for its description.
                                    var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, poolee.Recordkey);

                                    // Add the necessary components to process for the descriptions to a list for bulk read later.
                                    GatherGlDescriptionComponents(glComponentsForCostCenter);

                                    // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                                    var pooleeGlAccount = new CostCenterGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
                                    pooleeGlAccount.BudgetAmount += poolee.BAlocDebitsYtd.HasValue ? poolee.BAlocDebitsYtd.Value : 0m;
                                    pooleeGlAccount.BudgetAmount -= poolee.BAlocCreditsYtd.HasValue ? poolee.BAlocCreditsYtd.Value : 0m;
                                    pooleeGlAccount.EncumbranceAmount += poolee.EOpenBal.HasValue ? poolee.EOpenBal.Value : 0m;
                                    pooleeGlAccount.EncumbranceAmount += poolee.EncumbrancesYtd.HasValue ? poolee.EncumbrancesYtd.Value : 0m;
                                    pooleeGlAccount.EncumbranceAmount -= poolee.EncumbrancesRelievedYtd.HasValue ? poolee.EncumbrancesRelievedYtd.Value : 0m;
                                    pooleeGlAccount.ActualAmount += poolee.DebitsYtd.HasValue ? poolee.DebitsYtd.Value : 0m;
                                    pooleeGlAccount.ActualAmount -= poolee.CreditsYtd.HasValue ? poolee.CreditsYtd.Value : 0m;

                                    // Add the poolee to the appropriate pool.
                                    var selectedPool = glBudgetPoolDomainEntities.FirstOrDefault(x => x.Umbrella.GlAccountNumber == poolee.GlsBudgetLinkage);
                                    if (selectedPool != null)
                                    {
                                        selectedPool.AddPoolee(pooleeGlAccount);
                                    }
                                    else
                                    {
                                        // If the user has access to poolees from a budget pool for which they don't have access
                                        // to the umbrella, we will need to read the GL account for the umbrella to get the budget
                                        // pool amounts. But, we don't have to worry about whether the umbrella has direct expenses
                                        // because the user doesn't have access to see them anyway.

                                        // Read the GL.ACCTS record for the umbrella, and get the amounts for fiscal year.
                                        var umbrellaAccount = await DataReader.ReadRecordAsync<GlsFyr>(glsFyrId, poolee.GlsBudgetLinkage);

                                        // Perform error checking
                                        if (umbrellaAccount == null)
                                        {
                                            throw new ApplicationException("Umbrella data contract should not be null.");
                                        }

                                        if (string.IsNullOrEmpty(umbrellaAccount.Recordkey))
                                        {
                                            throw new ApplicationException("RecordKey for GLS.FYR data contract is missing.");
                                        }

                                        // Create the cost center GL account DE for the umbrella.
                                        var newUmbrellaGlAccount = new CostCenterGlAccount(umbrellaAccount.Recordkey, GlBudgetPoolType.Umbrella);

                                        // Determine this GL account cost center ID and the necessary components for its description.
                                        var umbrellaGlComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, newUmbrellaGlAccount.GlAccountNumber);

                                        // Add the necessary components to process for the descriptions to a list for bulk read later.
                                        GatherGlDescriptionComponents(umbrellaGlComponentsForCostCenter);

                                        newUmbrellaGlAccount.BudgetAmount += umbrellaAccount.BAlocDebitsYtd.HasValue ? umbrellaAccount.BAlocDebitsYtd.Value : 0m;
                                        newUmbrellaGlAccount.BudgetAmount -= umbrellaAccount.BAlocCreditsYtd.HasValue ? umbrellaAccount.BAlocCreditsYtd.Value : 0m;
                                        foreach (var amount in umbrellaAccount.GlsFaMactuals)
                                        {
                                            newUmbrellaGlAccount.ActualAmount += amount.HasValue ? amount.Value : 0m;
                                        }

                                        foreach (var amount in umbrellaAccount.GlsFaMencumbrances)
                                        {
                                            newUmbrellaGlAccount.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;
                                        }

                                        // Create a new budget pool and add the poolee to it.
                                        var newBudgetPool = new GlBudgetPool(newUmbrellaGlAccount);
                                        if (allExpenseRevenueAccounts.Contains(poolee.Recordkey))
                                        {
                                            // If the user has access to the umbrella, make it visible.
                                            newBudgetPool.IsUmbrellaVisible = true;
                                        }

                                        // Add poolee to new pool.
                                        newBudgetPool.AddPoolee(pooleeGlAccount);

                                        // Add new budget pool to the list of new budget pools
                                        glBudgetPoolDomainEntities.Add(newBudgetPool);
                                    }
                                }
                            }
                        }
                        catch (ApplicationException aex)
                        {
                            LogDataError("", "", poolee, aex);
                        }
                    }
                }
                #endregion

                #region Process all non-pooled accounts

                foreach (var nonPooledAccount in nonPooledAccounts)
                {
                    // Determine the GL class for the GL account.
                    var glClass = GetGlAccountGlClass(nonPooledAccount.Recordkey, glClassConfiguration);

                    // Only use the financial health indicator filter criteria if the GL account is an expense one.
                    if (glClass != GlClass.Expense)
                    {
                        includeGlAccount = true;
                    }
                    else
                    {
                        if (checkFinancialIndicatorFilter)
                        {
                            // If specified in the filter criteria, select those expense GL accounts
                            // that meet the financial health indicator values selected by the user.
                            includeGlAccount = FilterUtility.CheckFinancialHealthIndicatorFilterForGlsFyr(nonPooledAccount, "", costCenterCriteria);
                        }
                        else
                        {
                            includeGlAccount = true;
                        }
                    }

                    if (includeGlAccount)
                    {
                        // Determine this GL account cost center ID and the necessary components for its description.
                        var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, nonPooledAccount.Recordkey);

                        // Add the necessary components to process for the descriptions to a list for bulk read later.
                        GatherGlDescriptionComponents(glComponentsForCostCenter);

                        // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                        var nonPooledGlAccount = new CostCenterGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                        nonPooledGlAccount.BudgetAmount += nonPooledAccount.BAlocDebitsYtd.HasValue ? nonPooledAccount.BAlocDebitsYtd.Value : 0m;
                        nonPooledGlAccount.BudgetAmount -= nonPooledAccount.BAlocCreditsYtd.HasValue ? nonPooledAccount.BAlocCreditsYtd.Value : 0m;
                        nonPooledGlAccount.EncumbranceAmount += nonPooledAccount.EOpenBal.HasValue ? nonPooledAccount.EOpenBal.Value : 0m;
                        nonPooledGlAccount.EncumbranceAmount += nonPooledAccount.EncumbrancesYtd.HasValue ? nonPooledAccount.EncumbrancesYtd.Value : 0m;
                        nonPooledGlAccount.EncumbranceAmount -= nonPooledAccount.EncumbrancesRelievedYtd.HasValue ? nonPooledAccount.EncumbrancesRelievedYtd.Value : 0m;
                        nonPooledGlAccount.ActualAmount += nonPooledAccount.DebitsYtd.HasValue ? nonPooledAccount.DebitsYtd.Value : 0m;
                        nonPooledGlAccount.ActualAmount -= nonPooledAccount.CreditsYtd.HasValue ? nonPooledAccount.CreditsYtd.Value : 0m;

                        // Add the GL account to the list of non-pooled accounts.
                        nonPooledGlAccountDomainEntities.Add(nonPooledGlAccount);
                    }
                }
                #endregion

                #region Assign all pools to a cost center and subtotal

                foreach (var budgetPool in glBudgetPoolDomainEntities)
                {
                    AddBudgetPoolToCostCentersList(budgetPool, costCenterStructure, glClassConfiguration);
                }
                #endregion

                #region Assign all non-pooled GL accounts to a cost center and subtotal

                foreach (var nonPooledAccount in nonPooledGlAccountDomainEntities)
                {
                    AddNonPooledAccountToCostCentersList(nonPooledAccount, costCenterStructure, glClassConfiguration);
                }
                #endregion

                #endregion

                #region Get closed year amounts from ENC.FYR

                // Now obtain the requisition amounts from ENC.FYR because they are not stored in GLS.FYR.

                // Bulk read the ENC.FYR records using the list of GL accounts that the user has access to,
                // either all accounts that they have access to or those that they have access to for a 
                // specified cost center.
                var encFyrId = "ENC." + fiscalYear;
                Collection<EncFyr> encRecords = new Collection<EncFyr>();
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria) && string.IsNullOrEmpty(selectedCostCenterId))
                {
                    encRecords = await GetOrAddToCacheAsync<Collection<EncFyr>>("CostCentersEncFyr" + personId + costCenterCriteria.FiscalYear, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<EncFyr>(encFyrId, expenseRevenueGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    encRecords = await DataReader.BulkReadRecordAsync<EncFyr>(encFyrId, expenseRevenueGlAccounts);
                }

                // If the repository method is being called for a specific cost center, update the list of 
                // ENC.FYR records to process with those poolee accounts that are outside of the
                // specified cost center for budget pools in the cost center that the user has access to.
                if (selectedGlpPooleeAccounts.Any())
                {
                    var encGlpPooleeAccounts = selectedGlpPooleeAccounts.ToArray();
                    var encPooleeRecords = await DataReader.BulkReadRecordAsync<EncFyr>(encFyrId, encGlpPooleeAccounts);
                    foreach (var encPooleeAccount in encPooleeRecords)
                    {
                        encRecords.Add(encPooleeAccount);
                    }
                }

                // If we do not have any ENC.FYR records, return the list of cost centers that has been obtained already.
                if (encRecords != null && encRecords.Any())
                {
                    Collection<GlpFyr> glpFyrDataContracts = new Collection<GlpFyr>();
                    string glpFyrFilename = "GLP." + fiscalYear;
                    var glpFyrIds = await DataReader.SelectAsync(glpFyrFilename, null);

                    if (glpFyrIds.Any())
                        glpFyrDataContracts = await DataReader.BulkReadRecordAsync<GlpFyr>(glpFyrFilename, glpFyrIds);

                    // Create a list of Cost Center GL Account domain entities for each ENC.FYR record that has a requisition amount.
                    var requisitionGlAccountsList = new List<CostCenterGlAccount>();
                    foreach (var reqGlAccount in encRecords)
                    {
                        // Determine if the GL account is a regular GL account or part of a budget pool.
                        // Default the budget pool type to not part of a pool.
                        GlBudgetPoolType poolType = GlBudgetPoolType.None;

                        // Determine the cost center ID for the umbrella, if we are processing a single cost center.
                        var pooleeUmbrella = string.Empty;
                        var pooleeUmbrellaCostCenterId = string.Empty;

                        bool glNumberFoundInGls = false;

                        if (reqGlAccount != null)
                        {
                            // Only process the ENC.FYR record if it contains requisition information.
                            if (reqGlAccount.EncReqEntityAssociation != null)
                            {
                                if (glsRecords != null)
                                {
                                    var glsDataContract = glsRecords.FirstOrDefault(x => x.Recordkey == reqGlAccount.Recordkey);
                                    if (glsDataContract != null)
                                    {
                                        if (glsDataContract.GlsPooledType.ToUpper() == "U")
                                            poolType = GlBudgetPoolType.Umbrella;
                                        else if (glsDataContract.GlsPooledType.ToUpper() == "P")
                                        {
                                            poolType = GlBudgetPoolType.Poolee;
                                            pooleeUmbrella = glsDataContract.GlsBudgetLinkage;
                                        }
                                        glNumberFoundInGls = true;
                                    }
                                }

                                // If this GL account only has an ENC.FYR record and not GLS.FYR, then we have 
                                // to find its umbrella GL account from GL.ACCTS or from GLP.FYR.
                                if (!glNumberFoundInGls)
                                {
                                    // Is this GL account an umbrella or poolee?
                                    var umbrellaglpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.Recordkey == reqGlAccount.Recordkey);
                                    if (umbrellaglpFyrContract != null)
                                        poolType = GlBudgetPoolType.Umbrella;
                                    else
                                    {
                                        var pooleeGlpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.GlpPooleeAcctsList.Contains(reqGlAccount.Recordkey));
                                        if (pooleeGlpFyrContract != null)
                                        {
                                            poolType = GlBudgetPoolType.Poolee;
                                            pooleeUmbrella = pooleeGlpFyrContract.Recordkey;
                                        }
                                    }
                                }

                                // If we are processing a poolee, only process it if the umbrella has not been excluded. Otherwise, create a domain entity
                                // for the requisition GL account.
                                if (!string.IsNullOrEmpty(pooleeUmbrella))
                                {
                                    if (!excludedUmbrellaGlAccounts.Contains(pooleeUmbrella))
                                    {
                                        // If we are processing a single cost center and we have a poolee, determine the cost center
                                        // of the umbrella associated with the poolee.
                                        if (!string.IsNullOrEmpty(selectedCostCenterId))
                                        {
                                            if (!string.IsNullOrEmpty(pooleeUmbrella))
                                            {
                                                foreach (var component in costCenterStructure.CostCenterComponents)
                                                {
                                                    if (component != null)
                                                    {
                                                        var componentId = pooleeUmbrella.Substring(component.StartPosition, component.ComponentLength);
                                                        pooleeUmbrellaCostCenterId += componentId;
                                                    }
                                                }
                                            }
                                        }

                                        // Create the domain entity for the requisition GL account.

                                        // If we are processing one cost center and the umbrella GL account cost center ID matches the selected one,
                                        // or if we are processing all cost centers, process the poolee. If we are processing one cost center and the
                                        // umbrella GL account cost center ID does not match the selected on, ignore it.
                                        if (string.IsNullOrEmpty(selectedCostCenterId) || pooleeUmbrellaCostCenterId == selectedCostCenterId)
                                        {
                                            var requisitionGlAccountDomain = new CostCenterGlAccount(reqGlAccount.Recordkey, poolType);

                                            // Obtain the total requisition amount for this GL account.
                                            foreach (var amount in reqGlAccount.EncReqAmt)
                                                requisitionGlAccountDomain.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;

                                            requisitionGlAccountsList.Add(requisitionGlAccountDomain);
                                        }
                                    }
                                }
                                else
                                {
                                    // Create the domain entity for the non-pooled requisition GL account.
                                    var requisitionGlAccountDomain = new CostCenterGlAccount(reqGlAccount.Recordkey, poolType);

                                    // Obtain the total requisition amount for this GL account.
                                    foreach (var amount in reqGlAccount.EncReqAmt)
                                        requisitionGlAccountDomain.EncumbranceAmount += amount.HasValue ? amount.Value : 0m;

                                    requisitionGlAccountsList.Add(requisitionGlAccountDomain);
                                }
                            }
                        }
                    }

                    // requisitionGlAccountsList contains a list of cost center GL account domain entities, each containing a requisition amount.
                    // We need to add those requisition amounts to the appropriate cost center subtotals and cost centers domain entities such that:
                    // - if the GL account belongs to an existing cost center and one of its cost center subtotals:
                    //      = if the GL account is already in the list of GL accounts for the one cost center subtotal, add the requisition amount to its encumbrances.
                    //      = if the GL account is not already in the list of GL accounts for the one cost center subtotal, add the domain entity to the subtotal cost center.
                    // - if the GL account belongs to an existing cost center, but it belongs to a new cost center subtotal for the cost center:
                    //      = create the new cost center subtotal domain entity and add the gl account to it; add the new subtotal to the cost center.
                    // - if the GL account does not belong to any of the existing cost centers:
                    //      = create a new cost center
                    //      = create a new cost center subtotal and add the GL account entity to it
                    //      = add the new cost center subtotal to the new cost center.

                    if (requisitionGlAccountsList != null && requisitionGlAccountsList.Any())
                    {
                        // Loop through each GL account domain with a requisition amount.
                        foreach (var reqGlAcct in requisitionGlAccountsList)
                        {
                            bool glAccountFound = false;

                            // Find all the GL numbers for the cost centers
                            var allUmbrellaAccounts = costCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.Select(z => z.Umbrella))).ToList();
                            var allPooleeAccounts = costCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.SelectMany(z => z.Poolees))).ToList();
                            var allNonPooledAccounts = costCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.GlAccounts)).ToList();

                            // Loop through each cost center subtotal in the cost center.
                            var allSubtotals = costCenters.SelectMany(x => x.CostCenterSubtotals).ToList();
                            foreach (var subtotal in allSubtotals)
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                                {
                                    // Loop through each GL account in the list that belong to this cost center subtotal.
                                    foreach (var acct in subtotal.GlAccounts)
                                    {
                                        // If the requisition GL account is already included in the cost center subtotal list of
                                        // GL accounts, add the requisition amount to its encumbrances.
                                        if (reqGlAcct.GlAccountNumber == acct.GlAccountNumber)
                                        {
                                            acct.EncumbranceAmount += reqGlAcct.EncumbranceAmount;
                                            glAccountFound = true;
                                        }
                                    }
                                }
                                else if (reqGlAcct.PoolType == GlBudgetPoolType.Poolee || reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                {
                                    foreach (var pool in subtotal.Pools)
                                    {
                                        if (reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                        {
                                            if (reqGlAcct.GlAccountNumber == pool.Umbrella.GlAccountNumber)
                                            {
                                                pool.Umbrella.EncumbranceAmount += reqGlAcct.EncumbranceAmount;

                                                var umbrellaPoolee = pool.Poolees.FirstOrDefault(x => x.GlAccountNumber == reqGlAcct.GlAccountNumber);
                                                if (umbrellaPoolee != null)
                                                    umbrellaPoolee.EncumbranceAmount += reqGlAcct.EncumbranceAmount;
                                                else
                                                {
                                                    // Only add the umbrella as a poolee if the user has access to it.
                                                    if (expenseRevenueGlAccounts.Contains(reqGlAcct.GlAccountNumber) && reqGlAcct.EncumbranceAmount != 0)
                                                    {
                                                        // Create a poolee to represent the umbrella and add the encumbrance amount, then add it to the poolees list.
                                                        var poolee = new CostCenterGlAccount(reqGlAcct.GlAccountNumber, GlBudgetPoolType.Poolee);
                                                        poolee.EncumbranceAmount = reqGlAcct.EncumbranceAmount;

                                                        pool.AddPoolee(poolee);
                                                    }
                                                }

                                                glAccountFound = true;
                                            }
                                        }
                                        else
                                        {
                                            var selectedPoolee = pool.Poolees.FirstOrDefault(x => x.GlAccountNumber == reqGlAcct.GlAccountNumber);
                                            if (selectedPoolee != null)
                                            {
                                                selectedPoolee.EncumbranceAmount += reqGlAcct.EncumbranceAmount;
                                                // Also add the poolee requisition amount to the umbrella requisition amount.
                                                pool.Umbrella.EncumbranceAmount += reqGlAcct.EncumbranceAmount;
                                                glAccountFound = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // If the GL account is not included in any cost center, find the cost center subtotal and cost center for it, 
                            // whether they already exist, or we have to create new ones.
                            // This can be a case where a poolee only has an ENC record and no posted activity in any GLS record,
                            // for instance, the poolee only has requisition amounts.
                            if (!glAccountFound)
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                                {
                                    AddNonPooledAccountToCostCentersList(reqGlAcct, costCenterStructure, glClassConfiguration);
                                }
                                else
                                {
                                    if (reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                    {
                                        var budgetPool = new GlBudgetPool(reqGlAcct);
                                        if (expenseRevenueGlAccounts.Contains(reqGlAcct.GlAccountNumber))
                                        {
                                            // If the user has access to the umbrella, make it visible.
                                            budgetPool.IsUmbrellaVisible = true;
                                        }
                                        AddBudgetPoolToCostCentersList(budgetPool, costCenterStructure, glClassConfiguration);
                                    }
                                    else
                                    {
                                        // Find the umbrella for this poolee from GLP.FYR
                                        var umbrellaForThisPooleeGlpFyrRecord = glpFyrDataContracts.FirstOrDefault(x => x.GlpPooleeAcctsList.Contains(reqGlAcct.GlAccountNumber));
                                        if (umbrellaForThisPooleeGlpFyrRecord != null)
                                        {
                                            var umbrellaAccount = umbrellaForThisPooleeGlpFyrRecord.Recordkey;
                                            var budgetPool = new GlBudgetPool(new CostCenterGlAccount(umbrellaForThisPooleeGlpFyrRecord.Recordkey, GlBudgetPoolType.Umbrella));
                                            budgetPool.AddPoolee(reqGlAcct);
                                            // Also add the poolee requisition amount to the umbrella requisition amount.
                                            budgetPool.Umbrella.EncumbranceAmount += reqGlAcct.EncumbranceAmount;
                                            AddBudgetPoolToCostCentersList(budgetPool, costCenterStructure, glClassConfiguration);
                                        }
                                        else
                                        {
                                            // There is data problem. This is a poolee but it was not found in GLP.FYR
                                            // Log it and ignore this record.
                                            LogDataError("This GL account " + reqGlAcct.GlAccountNumber + " is a poolee but was not found in any GLP." + fiscalYear + " pools.", "", reqGlAcct);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }

            #region Populate component descriptions

            // Bulk read the component descriptions then assign the descriptions.
            // First read the Function descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Function).Any())
            {
                List<string> functionIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Function)
                    .Select(x => x.Id).ToList();
                var fcDescs = await DataReader.BulkReadRecordAsync<FcDescs>(functionIds.ToArray());

                foreach (var fcDesc in fcDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == fcDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Function).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = fcDesc.FcDescription;
                }
            }

            // Now read the Fund descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Fund).Any())
            {
                List<string> fundIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Fund)
                    .Select(x => x.Id).ToList();
                var fdDescs = await DataReader.BulkReadRecordAsync<FdDescs>(fundIds.ToArray());

                foreach (var fdDesc in fdDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == fdDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Fund).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = fdDesc.FdDescription;
                }
            }

            // Now read the Location descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Location).Any())
            {
                List<string> locationIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Location)
                    .Select(x => x.Id).ToList();
                var loDescs = await DataReader.BulkReadRecordAsync<LoDescs>(locationIds.ToArray());

                foreach (var loDesc in loDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == loDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Location).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = loDesc.LoDescription;
                }
            }

            // Now read the Object descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Object).Any())
            {
                List<string> objectIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Object)
                    .Select(x => x.Id).ToList();
                var obDescs = await DataReader.BulkReadRecordAsync<ObDescs>(objectIds.ToArray());

                foreach (var obDesc in obDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == obDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Object).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = obDesc.ObDescription;
                }
            }

            // Now read the Source descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Source).Any())
            {
                List<string> sourceIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Source)
                    .Select(x => x.Id).ToList();
                var soDescs = await DataReader.BulkReadRecordAsync<SoDescs>(sourceIds.ToArray());

                foreach (var soDesc in soDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == soDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Source).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = soDesc.SoDescription;
                }
            }

            // Now read the Unit descriptions
            if (glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Unit).Any())
            {
                List<string> unitIds = glComponentsToBulkRead.Where(x => x.ComponentType == GeneralLedgerComponentType.Unit)
                    .Select(x => x.Id).ToList();
                var unDescs = await DataReader.BulkReadRecordAsync<UnDescs>(unitIds.ToArray());

                foreach (var unDesc in unDescs)
                {
                    var selectedComponent = glComponentsToBulkRead.Where(x => x.Id == unDesc.Recordkey
                        && x.ComponentType == GeneralLedgerComponentType.Unit).ToList().FirstOrDefault();

                    if (selectedComponent != null)
                        selectedComponent.Description = unDesc.UnDescription;
                }
            }

            // Now that the descriptions have been read from Colleague we need to populate the cost center descriptions.
            foreach (var costCenter in costCenters)
            {
                foreach (var componentDescription in costCenter.GlComponentDescriptions)
                {
                    var selectedDescription = glComponentsToBulkRead.Where(x => x.Id == componentDescription.Id && x.ComponentType == componentDescription.ComponentType).FirstOrDefault();

                    if (selectedDescription != null)
                        componentDescription.Description = selectedDescription.Description;
                }
            }

            // Only populate the subtotal description if a specified cost center is passed in.
            // Same for GL account descriptions.
            if (!string.IsNullOrEmpty(selectedCostCenterId))
            {
                // Populate the description for the cost center subtotal.
                // If the subtotal is not defined in Colleague, there is not need to get the description.
                if (costCenterSubtotalIdToBulkRead != null && costCenterSubtotalIdToBulkRead.Any())
                {
                    // We know the subtotal is of a OB type.
                    var subtotalDescs = await DataReader.BulkReadRecordAsync<ObDescs>(costCenterSubtotalIdToBulkRead.ToArray());
                    foreach (var costCenter in costCenters)
                    {
                        foreach (var costCenterSubtotal in costCenter.CostCenterSubtotals)
                        {
                            var unDescription = subtotalDescs.Where(x => x.Recordkey == costCenterSubtotal.Id).FirstOrDefault();
                            if (!string.IsNullOrEmpty(unDescription.ObDescription))
                            {
                                costCenterSubtotal.Name = unDescription.ObDescription;
                            }
                            else
                            {
                                costCenterSubtotal.Name = "No Subtotal Description available";
                            }
                        }
                    }
                }

                // Now for the GL account descriptions.
                var selectedCostCenter = costCenters.FirstOrDefault();
                if (selectedCostCenter != null)
                {
                    var allGlAccountEntities = selectedCostCenter.CostCenterSubtotals.SelectMany(x => x.GlAccounts)
                            .Union(selectedCostCenter.CostCenterSubtotals.SelectMany(x => x.Pools.Select(y => y.Umbrella)))
                            .Union(selectedCostCenter.CostCenterSubtotals.SelectMany(x => x.Pools.SelectMany(y => y.Poolees))).ToList();

                    // Obtain the descriptions for all the GL accounts in the cost center.
                    GetGlAccountDescriptionRequest request = new GetGlAccountDescriptionRequest()
                    {
                        GlAccountIds = allGlAccountEntities.Select(x => x.GlAccountNumber).ToList(),
                        Module = "SS"
                    };

                    GetGlAccountDescriptionResponse response = await transactionInvoker.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(request);

                    // The transaction returns the description for each GL account number.
                    if (response != null)
                    {
                        if (selectedCostCenter != null && response.GlAccountIds != null && response.GlDescriptions != null
                            && response.GlAccountIds.Any() && response.GlDescriptions.Any())
                        {
                            foreach (var glAccount in allGlAccountEntities)
                            {
                                var index = response.GlAccountIds.IndexOf(glAccount.GlAccountNumber);

                                if (index < response.GlDescriptions.Count)
                                {
                                    glAccount.GlAccountDescription = response.GlDescriptions[index];
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Remove inactive accounts with no activity

            // Get the non pooled account GL account number strings from the CostCenterGlAccounts from all of the cost centers
            // where all of the amounts are zero.
            var possibleCostCenterGlAccountsToRemove = costCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.GlAccounts).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0)).ToList();

            // Get the poolee account GL account number strings from the CostCenterGlAccounts from all of the cost centers
            // where all of the amounts are zero.
            var possibleCostCenterGlAccountPooleesToRemove = costCenters.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.SelectMany(z => z.Poolees).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0))).ToList();

            // Add the list of poolee accounts to the list of non pooled accounts.
            if (possibleCostCenterGlAccountPooleesToRemove != null && possibleCostCenterGlAccountPooleesToRemove.Any())
            {
                possibleCostCenterGlAccountsToRemove.AddRange(possibleCostCenterGlAccountPooleesToRemove);
            }

            // If there are any CostCenterGlAccounts with all zero amounts, find out if there is any activity for 
            // any of these GL accounts by selecting GLS and ENC records. Also, find out if any of these GL accounts
            // are active by selecting GL.ACCTS that are not currently inactive. Remove these GL account record IDs
            // from the list of possible record IDs to remove.
            if (possibleCostCenterGlAccountsToRemove != null && possibleCostCenterGlAccountsToRemove.Any())
            {
                var glAccountsWithZeroBalanceArray = possibleCostCenterGlAccountsToRemove.Select(x => x.GlAccountNumber).ToArray();
                var inactiveGlAccountsWithNoActivity = glAccountsWithZeroBalanceArray.ToList();

                string criteria = "WITH GL.INACTIVE NE 'I'";
                var activeGlAcctsIds = await DataReader.SelectAsync("GL.ACCTS", glAccountsWithZeroBalanceArray, criteria);
                if (activeGlAcctsIds != null)
                {
                    // Remove the active GL numbers from the list of GL accounts with zero balances.
                    foreach (var activeString in activeGlAcctsIds)
                    {
                        inactiveGlAccountsWithNoActivity.Remove(activeString);
                    }
                }

                string glsFyrFilename = "GLS." + fiscalYear;
                string glsCriteria = "WITH ACTIVITY.SEQ NE ''";
                var glsAccountsWithNoActivity = new List<string>();
                var glsFyrIdsWithActivity = await DataReader.SelectAsync(glsFyrFilename, glAccountsWithZeroBalanceArray, glsCriteria);
                if (glsFyrIdsWithActivity != null)
                {
                    // Remove the active GL numbers from the list of GL accounts with no activity.
                    foreach (var glsString in glsFyrIdsWithActivity)
                    {
                        inactiveGlAccountsWithNoActivity.Remove(glsString);
                    }

                    // Grab the GL numbers with no activity (not in the 'glsFyrIdsWithActivity' list).
                    glsAccountsWithNoActivity = glAccountsWithZeroBalanceArray.Except(glsFyrIdsWithActivity).ToList();
                }

                string encFyrFilename = "ENC." + fiscalYear;
                var encAccountsWithNoActivity = new List<string>();
                var encFyrIdsWithActivity = await DataReader.SelectAsync(encFyrFilename, glAccountsWithZeroBalanceArray, null);
                if (encFyrIdsWithActivity != null)
                {
                    // Remove the active GL numbers from the list of GL accounts with no activity.
                    foreach (var encString in encFyrIdsWithActivity)
                    {
                        inactiveGlAccountsWithNoActivity.Remove(encString);
                    }

                    encAccountsWithNoActivity = glAccountsWithZeroBalanceArray.Except(encFyrIdsWithActivity).ToList();
                }

                // Get the GL accounts that are both active AND have no activity.
                var activeGlAccountsWithNoActivity = (glsAccountsWithNoActivity.Intersect(encAccountsWithNoActivity)).Intersect(activeGlAcctsIds).ToList();

                // Initialize the list of GL accounts to remove; we always want to remove the inactive accounts with no activity.
                var glAccountsToRemove = inactiveGlAccountsWithNoActivity;

                // If specified in the filter criteria, exclude the active GL accounts with no activity.
                if (costCenterCriteria != null && !costCenterCriteria.IncludeActiveAccountsWithNoActivity)
                {
                    glAccountsToRemove.AddRange(activeGlAccountsWithNoActivity);
                }

                // If there are GL account IDs where the account is inactive and has no activity, remove them from the 
                // list of non pooled accounts and from the list of poolee on each cost center subtotal on each cost center.
                if (glAccountsToRemove.Any())
                {
                    var costCenterGlToBeDeleted = costCenters.SelectMany(x => x.CostCenterSubtotals).SelectMany(y => y.GlAccounts).Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber)).ToList();
                    var costCenterPooleeToBeDeleted = costCenters.SelectMany(x => x.CostCenterSubtotals).SelectMany(y => y.Pools).SelectMany(z => z.Poolees).Where(a => inactiveGlAccountsWithNoActivity.Contains(a.GlAccountNumber)).ToList();

                    foreach (var cc in costCenters)
                    {
                        foreach (var sub in cc.CostCenterSubtotals)
                        {
                            sub.GlAccounts = sub.GlAccounts.Except(sub.GlAccounts.Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber))).ToList();
                            if (costCenterPooleeToBeDeleted.Any())
                            {
                                foreach (var pool in sub.Pools)
                                {
                                    pool.Poolees = pool.Poolees.Except(pool.Poolees.Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber))).ToList();
                                }
                            }
                        }
                    }

                    if (costCenterGlToBeDeleted.Any())
                    {
                        // Remove those cost center subtotals that do not have a list of non pooled accounts and do not
                        // have a list of budget pools.

                        var subtotalsToBeDeleted = costCenters.SelectMany(x => x.CostCenterSubtotals).Where(y => !y.GlAccounts.Any() && !y.Pools.Any()).Select(z => z.Id).ToList();
                        if (subtotalsToBeDeleted.Any())
                        {
                            foreach (var cc in costCenters)
                            {
                                cc.CostCenterSubtotals = cc.CostCenterSubtotals.Except(cc.CostCenterSubtotals.Where(y => !y.GlAccounts.Any() && !y.Pools.Any())).ToList();
                            }
                        }

                        // Remove those cost centers that do not have a list of cost center subtotals.
                        var costCentersToBeDeleted = costCenters.Where(x => !x.CostCenterSubtotals.Any()).Select(y => y.Id).ToList();
                        if (costCentersToBeDeleted.Any())
                        {
                            costCenters = costCenters.Except(costCenters.Where(x => !x.CostCenterSubtotals.Any())).ToList();
                        }
                    }
                }
            }
            #endregion

            return costCenters;
        }

        #region Private methods

        private IEnumerable<GeneralLedgerComponentDescription> DetermineGlComponentsForCostCenter(CostCenterStructure costCenterStructure, string glAccountNumber)
        {
            // Determine which component description objects will be needed to calculate the cost center Id and Name.
            costCenterId = string.Empty;
            var glComponentsForCostCenter = new List<GeneralLedgerComponentDescription>();

            if (!string.IsNullOrEmpty(glAccountNumber))
            {
                foreach (var component in costCenterStructure.CostCenterComponents)
                {
                    if (component != null)
                    {
                        var componentId = glAccountNumber.Substring(component.StartPosition, component.ComponentLength);
                        costCenterId += componentId;

                        // Add the component description object if the component is part of the description and it's not already in the list.
                        if (component.IsPartOfDescription && glComponentsForCostCenter.Where(x => x.Id == componentId && x.ComponentType == component.ComponentType).Count() == 0)
                            glComponentsForCostCenter.Add(new GeneralLedgerComponentDescription(componentId, component.ComponentType));
                    }
                }
            }

            return glComponentsForCostCenter;
        }

        /// <summary>
        /// Add the list of GL components passed in to a master list for bulk read.
        /// </summary>
        /// <param name="glComponentsForCostCenter">List of GL components to add to a master list for buld read.</param>
        private void GatherGlDescriptionComponents(IEnumerable<GeneralLedgerComponentDescription> glComponentsForCostCenter)
        {
            // Add the cost center GL components to the bulk read list.
            if (glComponentsForCostCenter != null)
            {
                foreach (var glComponentDesc in glComponentsForCostCenter)
                {
                    if (glComponentsToBulkRead.Where(x => x.Id == glComponentDesc.Id && x.ComponentType == glComponentDesc.ComponentType).Count() == 0)
                        glComponentsToBulkRead.Add(glComponentDesc);
                }
            }
        }

        private string GetAndSaveCostCenterSubtotalId(CostCenterStructure costCenterStructure, string glNumber)
        {
            // Obtain the GL account digits that determine the cost center subtotal.
            var costCenterSubtotalId = glNumber.Substring(costCenterStructure.CostCenterSubtotal.StartPosition, costCenterStructure.CostCenterSubtotal.ComponentLength);

            // If the cost center subtotal is not defined in Colleague, do not bother getting the description.
            isCostCenterSubtotalDefined = costCenterStructure.IsCostCenterSubtotalDefined;
            if (isCostCenterSubtotalDefined)
            {
                // Get the cost center component that we will need to read for its description.
                costCenterSubtotalIdToBulkRead.Add(costCenterSubtotalId);
            }

            return costCenterSubtotalId;
        }

        private void AddNonPooledAccountToCostCentersList(CostCenterGlAccount glAccount, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            // Get the cost center ID for the umbrella
            var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, glAccount.GlAccountNumber);

            // Get the subtotal ID from the pool umbrella
            var subtotalId = GetAndSaveCostCenterSubtotalId(costCenterStructure, glAccount.GlAccountNumber);

            // Add the cost center to the list of cost centers for the user if it does not exist already.
            // Also add the GL account to the list of GL accounts for the cost center.
            var selectedCostCenter = costCenters.FirstOrDefault(x => x.Id == costCenterId);

            // The cost center already exists so we need to add the GL account to a new or existing subtotal.
            if (selectedCostCenter != null)
            {
                // Check if the cost center contains the subtotal.
                var selectedSubtotal = selectedCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == subtotalId);

                // If the subtotal exists in the cost center add the GL account to the subtotal non-pooled accounts.
                // If the subtotal does not exist in the cost center then create a new subtotal using the GL account
                //    and add that subtotal to the cost center.
                if (selectedSubtotal != null)
                {
                    selectedSubtotal.AddGlAccount(glAccount);
                }
                else
                {
                    // Determine the GL class for the subtotal.
                    var glClass = GetGlAccountGlClass(glAccount.GlAccountNumber, glClassConfiguration);

                    var subtotal = new CostCenterSubtotal(subtotalId, glAccount, glClass);
                    subtotal.IsDefined = isCostCenterSubtotalDefined;
                    selectedCostCenter.AddCostCenterSubtotal(subtotal);
                }
            }
            // The cost center does not exist so we need to add the GL account to a new subtotal then add
            // that subtotal to a new cost center and add that cost center to the cost centers list.
            else
            {
                // Determine the GL class for the subtotal.
                var glClass = GetGlAccountGlClass(glAccount.GlAccountNumber, glClassConfiguration);

                var subtotal = new CostCenterSubtotal(subtotalId, glAccount, glClass);
                subtotal.IsDefined = isCostCenterSubtotalDefined;
                var newCostCenter = new CostCenter(costCenterId, subtotal, glComponentsForCostCenter);

                newCostCenter.UnitId = glAccount.GlAccountNumber.Substring(costCenterStructure.Unit.StartPosition, costCenterStructure.Unit.ComponentLength);
                costCenters.Add(newCostCenter);
            }
        }

        private void AddBudgetPoolToCostCentersList(GlBudgetPool budgetPool, CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            // Get the cost center ID for the umbrella
            var glComponentsForCostCenter = DetermineGlComponentsForCostCenter(costCenterStructure, budgetPool.Umbrella.GlAccountNumber);

            // Get the subtotal ID from the pool umbrella
            var subtotalId = GetAndSaveCostCenterSubtotalId(costCenterStructure, budgetPool.Umbrella.GlAccountNumber);

            // Add the cost center to the list of cost centers for the user if it does not exist already.
            // Also add the GL account to the list of GL accounts for the cost center.
            var selectedCostCenter = costCenters.FirstOrDefault(x => x.Id == costCenterId);
            if (selectedCostCenter != null)
            {
                // If the cost center already contains this subtotal 

                // already exists in the list of subtotals then add the pool to the subtotal, otherwise create
                // a new subtotal with the umbrella and add all poolees to that subtotal pool. Lastly, add the subtotal to the
                // list of subtotals.
                var selectedSubtotal = selectedCostCenter.CostCenterSubtotals.FirstOrDefault(x => x.Id == subtotalId);
                if (selectedSubtotal != null)
                {
                    selectedSubtotal.AddBudgetPool(budgetPool);
                }
                else
                {
                    // Determine the GL class for the subtotal.
                    var glClass = GetGlAccountGlClass(budgetPool.Umbrella.GlAccountNumber, glClassConfiguration);

                    var subtotal = new CostCenterSubtotal(subtotalId, budgetPool, glClass);
                    subtotal.IsDefined = isCostCenterSubtotalDefined;
                    selectedCostCenter.AddCostCenterSubtotal(subtotal);
                }
            }
            else
            {
                // Determine the GL class for the subtotal.
                var glClass = GetGlAccountGlClass(budgetPool.Umbrella.GlAccountNumber, glClassConfiguration);

                var subtotal = new CostCenterSubtotal(subtotalId, budgetPool, glClass);
                subtotal.IsDefined = isCostCenterSubtotalDefined;
                var newCostCenter = new CostCenter(costCenterId, subtotal, glComponentsForCostCenter);

                newCostCenter.UnitId = budgetPool.Umbrella.GlAccountNumber.Substring(costCenterStructure.Unit.StartPosition, costCenterStructure.Unit.ComponentLength);
                costCenters.Add(newCostCenter);
            }
        }

        private async Task<List<string>> GetPooleesOutsideCostCenter(GeneralLedgerUser generalLedgerUser, string selectedCostCenterId, string fiscalYear, CostCenterStructure costCenterStructure, CostCenterQueryCriteria costCenterCriteria)
        {
            // Determine whether there are any poolees for which the user has access that
            // are part of a budget pool for a specified cost center.

            string[] glpAccounts = null;
            Collection<GlpFyr> glpFyrDataContracts = new Collection<GlpFyr>();
            List<string> selectedGlpAccounts = new List<string>();
            List<string> outsidePooleeAccounts = new List<string>();

            // Select all of the budget pools for the fiscal year.
            string glpFyrFilename = "GLP." + fiscalYear;
            var glpFyrIds = await DataReader.SelectAsync(glpFyrFilename, null);

            if (glpFyrIds.Any())
            {
                // glpFyrIds contains the list of all the umbrella GL accounts for the fiscal year.
                // If the financial health indicator has been applied, remove from
                // glpFyrIds all the umbrellas that have been previously excluded.
                if (excludedUmbrellaGlAccounts.Any())
                {
                    List<string> filteredGlpIds = new List<string>();
                    foreach (var glpId in glpFyrIds)
                    {
                        if (!excludedUmbrellaGlAccounts.Contains(glpId))
                        {
                            filteredGlpIds.Add(glpId);
                        }
                    }
                    glpFyrIds = filteredGlpIds.ToArray();
                }
            }

            if (glpFyrIds.Any())
            {
                if (!string.IsNullOrEmpty(selectedCostCenterId))
                {
                    // Loop through the budget pool records for the fiscal year and
                    // determine those that are included in the specified cost center.
                    foreach (var acct in glpFyrIds)
                    {
                        // Determine the cost center ID for this expense/revenue GL account
                        var accountCostCenterId = string.Empty;
                        foreach (var component in costCenterStructure.CostCenterComponents)
                        {
                            if (component != null)
                            {
                                var componentId = acct.Substring(component.StartPosition, component.ComponentLength);
                                accountCostCenterId += componentId;
                            }
                        }

                        // Add the budget pool ID to a list of budget pool IDs
                        // for the specified cost center.
                        if (accountCostCenterId == selectedCostCenterId)
                        {
                            selectedGlpAccounts.Add(acct);
                        }
                    }
                }

                // If there are any budget pools for the cost center, read the budget pool, and
                // determine whether any of the poolees are from another cost center and the user
                // has access to.
                if (selectedGlpAccounts.Any())
                {
                    glpAccounts = selectedGlpAccounts.ToArray();
                    glpFyrDataContracts = await DataReader.BulkReadRecordAsync<GlpFyr>(glpFyrFilename, glpAccounts);
                    foreach (var glp in glpFyrDataContracts)
                    {
                        foreach (var poolee in glp.GlpPooleeAcctsList)
                        {
                            // Determine the cost center ID for this expense/revenue GL account
                            var accountCostCenterId = string.Empty;
                            foreach (var component in costCenterStructure.CostCenterComponents)
                            {
                                if (component != null)
                                {
                                    var componentId = poolee.Substring(component.StartPosition, component.ComponentLength);
                                    accountCostCenterId += componentId;
                                }
                            }

                            // If the poolee GL account cost center ID does not match the selected
                            // one, add the GL account to the list of GL accounts to process, if the
                            // user has access to it.
                            if (accountCostCenterId != selectedCostCenterId)
                            {
                                // check to see if the user has access to the poolee
                                if (generalLedgerUser.ExpenseAccounts.Contains(poolee))
                                {
                                    outsidePooleeAccounts.Add(poolee);
                                }
                            }
                        }
                    }

                    // filter the poolees that are outside the selected cost center by the selection criteria.
                    if (outsidePooleeAccounts.Any())
                    {
                        if (costCenterCriteria != null)
                        {
                            if (costCenterCriteria.ComponentCriteria.Any())
                            {
                                string componentFilterCriteria = null;
                                string valueFilterCriteria = null;
                                string rangeFilterCriteria = null;
                                string[] filteredPooleeAccounts = outsidePooleeAccounts.ToArray();
                                string[] tempFilteredAccounts;
                                foreach (var filterComp in costCenterCriteria.ComponentCriteria)
                                {
                                    // Set a running limiting list of expense/revenue accounts that are
                                    // filtered using the filtering criteria for each component.
                                    // And, reset the filter criteria strings to null.
                                    if (filteredPooleeAccounts.Any())
                                    {
                                        tempFilteredAccounts = filteredPooleeAccounts;
                                        valueFilterCriteria = null;
                                        rangeFilterCriteria = null;
                                        componentFilterCriteria = null;

                                        // Set the value filter criteria string from the individual component values.
                                        if (filterComp.IndividualComponentValues.Any())
                                        {
                                            foreach (var value in filterComp.IndividualComponentValues)
                                            {
                                                valueFilterCriteria = valueFilterCriteria + "'" + value + "' ";
                                            }
                                        }

                                        // Set the range filter criteria string from the component range values.
                                        if (filterComp.RangeComponentValues.Any())
                                        {
                                            foreach (var range in filterComp.RangeComponentValues)
                                            {
                                                if (string.IsNullOrEmpty(rangeFilterCriteria))
                                                {
                                                    rangeFilterCriteria = rangeFilterCriteria + "(" + filterComp.ComponentName + " GE '"
                                                        + range.StartValue + "' AND " + filterComp.ComponentName + " LE '" + range.EndValue + "') ";
                                                }
                                                else
                                                {
                                                    rangeFilterCriteria = rangeFilterCriteria + "OR (" + filterComp.ComponentName + " GE '"
                                                        + range.StartValue + "' AND " + filterComp.ComponentName + " LE '" + range.EndValue + "') ";
                                                }
                                            }
                                        }

                                        // Update the value filter criteria string to contain the component name
                                        if (!string.IsNullOrEmpty(valueFilterCriteria))
                                        {
                                            valueFilterCriteria = filterComp.ComponentName + " EQ " + valueFilterCriteria;
                                        }

                                        // Set the full component filter criteria string based on the value filter string
                                        // and the range value filter string.
                                        componentFilterCriteria = valueFilterCriteria;
                                        if (!string.IsNullOrEmpty(rangeFilterCriteria))
                                        {
                                            if (string.IsNullOrEmpty(componentFilterCriteria))
                                            {
                                                componentFilterCriteria = rangeFilterCriteria;
                                            }
                                            else
                                            {
                                                componentFilterCriteria = componentFilterCriteria + "OR " + rangeFilterCriteria;
                                            }
                                        }

                                        filteredPooleeAccounts = await DataReader.SelectAsync("GL.ACCTS", tempFilteredAccounts, componentFilterCriteria);
                                    }
                                }
                                outsidePooleeAccounts = filteredPooleeAccounts.ToList();
                            }
                        }
                    }
                }
            }
            return outsidePooleeAccounts;
        }

        /// <summary>
        /// Obtain the GL Class for a GL account
        /// </summary>
        /// <param name="glAccountr">A GL account number.</param>
        /// <param name="glClassConfiguration">General Ledger Class configuration.</param>
        private GlClass GetGlAccountGlClass(string glAccount, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            GlClass glClass = new GlClass();
            try
            {
                string glAccountGlClass = glAccount.Substring(glClassConfiguration.GlClassStartPosition, glClassConfiguration.GlClassLength);
                if (!string.IsNullOrEmpty(glAccountGlClass))
                {
                    if (glClassConfiguration.ExpenseClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Expense;
                    }
                    else if (glClassConfiguration.RevenueClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Revenue;
                    }
                    else if (glClassConfiguration.AssetClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Asset;
                    }
                    else if (glClassConfiguration.LiabilityClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.Liability;
                    }
                    else if (glClassConfiguration.FundBalanceClassValues.Contains(glAccountGlClass))
                    {
                        glClass = GlClass.FundBalance;
                    }
                    else
                    {
                        throw new ApplicationException("Invalid glClass for GL account: " + glAccount);
                    }
                }
                else
                {
                    throw new ApplicationException("Missing glClass for GL account: " + glAccount);
                }

                return glClass;
            }
            catch (ApplicationException aex)
            {
                logger.Warn("Invalid/unsupported GL class.");
                glClass = GlClass.Asset;
            }
            catch (Exception ex)
            {
                logger.Warn("Error occurred determining GL class for GL account number.");
                glClass = GlClass.Asset;
            }

            return glClass;
        }

        #endregion
    }
}