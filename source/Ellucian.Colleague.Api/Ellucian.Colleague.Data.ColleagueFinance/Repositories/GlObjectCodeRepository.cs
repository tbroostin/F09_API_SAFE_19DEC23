// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
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
    /// This class implements the IGlObjectCodeRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy), System.Runtime.InteropServices.GuidAttribute("D1E2A0B4-E0C4-47C3-AA8D-5DA69DF4340D")]
    public class GlObjectCodeRepository : BaseColleagueRepository, IGlObjectCodeRepository
    {
        private List<GlObjectCode> glObjectCodes;
        private GeneralLedgerComponent objectMajorComponent;
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
        public GlObjectCodeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            objectMajorComponent = null;
            glObjectCodes = new List<GlObjectCode>();
            includeGlAccount = false;
            checkFinancialIndicatorFilter = false;
            excludedUmbrellaGlAccounts = new List<string>();
        }

        /// <summary>
        /// This method gets the list of requested GL object codes assigned to the user.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="glAccountStructure">List of objects with information about the GL account structure.</param>
        /// <param name="glClassConfiguration">General Ledger Class configuration information.</param>
        /// <param name="criteria">Cost center filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>Returns the list of GL object codes assigned to the user for the selected criteria.</returns>
        public async Task<IEnumerable<GlObjectCode>> GetGlObjectCodesAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure,
            GeneralLedgerClassConfiguration glClassConfiguration, CostCenterQueryCriteria costCenterCriteria, string personId)
        {
            // Get the fiscal year from the filter criteria.
            var fiscalYear = costCenterCriteria.FiscalYear;

            if (string.IsNullOrEmpty(fiscalYear))
            {
                LogDataError("fiscalYear", "", fiscalYear);
                return glObjectCodes;
            }

            // Read the GEN.LDGR record for the fiscal year
            var genLdgrDataContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear);

            #region Error checking
            if (generalLedgerUser == null)
            {
                LogDataError("generalLedgerUser", "", generalLedgerUser);
                return glObjectCodes;
            }

            if (glAccountStructure == null)
            {
                LogDataError("glAccountStructure", "", glAccountStructure);
                return glObjectCodes;
            }

            if (glClassConfiguration == null)
            {
                LogDataError("glClassConfiguration", "", glClassConfiguration);
                return glObjectCodes;
            }

            // The object code view works off all five types of GL accounts, not just revenue and expense.
            // If the user does not have any GL accounts assigned, return an empty list of GL object codes.
            if ((generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any()))
            {
                LogDataError("AllAccounts", "", generalLedgerUser.AllAccounts);
                return glObjectCodes;
            }

            if (genLdgrDataContract == null)
            {
                logger.Warn("Missing GEN.LDGR record for ID: " + fiscalYear);
                return glObjectCodes;
            }

            if (string.IsNullOrEmpty(genLdgrDataContract.GenLdgrStatus))
            {
                logger.Warn("GEN.LDGR status is null.");
                return glObjectCodes;
            }
            #endregion

            // Initialize the object major component.
            objectMajorComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);

            // GL object codes are populated with all five types of GL accounts a user has access to.
            string[] userGlAccounts = generalLedgerUser.AllAccounts.ToArray();

            #region Apply filter criteria
            // Limit the list of accounts using the filter component criteria. If the data is not filtered,
            // return the GL object codes for all the GL accounts assigned to the user.
            if (userGlAccounts.Any())
            {
                if (costCenterCriteria != null)
                {
                    if (costCenterCriteria.ComponentCriteria.Any())
                    {
                        string componentFilterCriteria = null;
                        string valueFilterCriteria = null;
                        string rangeFilterCriteria = null;
                        string[] filteredGLAccounts = userGlAccounts;
                        string[] tempFilteredAccounts;
                        foreach (var filterComp in costCenterCriteria.ComponentCriteria)
                        {
                            // Set a running limiting list of GL accounts that are
                            // filtered using the filtering criteria for each component.
                            // And, reset the filter criteria strings to null.
                            if (filteredGLAccounts.Any())
                            {
                                tempFilteredAccounts = filteredGLAccounts;
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

                                filteredGLAccounts = await DataReader.SelectAsync("GL.ACCTS", tempFilteredAccounts, componentFilterCriteria);
                            }
                        }
                        userGlAccounts = filteredGLAccounts;
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
                // Bulk read the GL Account records. At this point userGlAccounts contains the list
                // of all GL accounts the user has access to.
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria))
                {
                    glAccountRecords = await GetOrAddToCacheAsync<Collection<GlAccts>>("GlObjectCodesGlAccts" + personId, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<GlAccts>(userGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    glAccountRecords = await DataReader.BulkReadRecordAsync<GlAccts>(userGlAccounts);
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

                var glBudgetPoolDomainEntities = new List<GlObjectCodeBudgetPool>();
                var nonPooledGlAccountDomainEntities = new List<GlObjectCodeGlAccount>();

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
                                    var umbrellaGlAccount = new GlObjectCodeGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                                    umbrellaGlAccount.BudgetAmount = glAccountAmounts.FaBudgetPostedAssocMember.HasValue ? glAccountAmounts.FaBudgetPostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.BudgetAmount += glAccountAmounts.FaBudgetMemoAssocMember.HasValue ? glAccountAmounts.FaBudgetMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaEncumbranceMemoAssocMember.HasValue ? glAccountAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.EncumbranceAmount += glAccountAmounts.FaRequisitionMemoAssocMember.HasValue ? glAccountAmounts.FaRequisitionMemoAssocMember.Value : 0m;
                                    umbrellaGlAccount.ActualAmount += glAccountAmounts.FaActualPostedAssocMember.HasValue ? glAccountAmounts.FaActualPostedAssocMember.Value : 0m;
                                    umbrellaGlAccount.ActualAmount += glAccountAmounts.FaActualMemoAssocMember.HasValue ? glAccountAmounts.FaActualMemoAssocMember.Value : 0m;

                                    // Create new pool for umbrella
                                    var newBudgetPool = new GlObjectCodeBudgetPool(umbrellaGlAccount);
                                    newBudgetPool.IsUmbrellaVisible = true;

                                    // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                                    if ((glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue && glAccountAmounts.GlEncumbrancePostedAssocMember.Value != 0)
                                        || (glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue && glAccountAmounts.GlEncumbranceMemosAssocMember.Value != 0)
                                        || (glAccountAmounts.GlRequisitionMemosAssocMember.HasValue && glAccountAmounts.GlRequisitionMemosAssocMember.Value != 0)
                                        || (glAccountAmounts.GlActualPostedAssocMember.HasValue && glAccountAmounts.GlActualPostedAssocMember.Value != 0)
                                        || (glAccountAmounts.GlActualMemosAssocMember.HasValue && glAccountAmounts.GlActualMemosAssocMember.Value != 0))
                                    {
                                        var umbrellaPoolee = new GlObjectCodeGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
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
                                    var pooleeGlAccount = new GlObjectCodeGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
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
                                        var newUmbrellaGlAccount = new GlObjectCodeGlAccount(umbrellaGlAccount, GlBudgetPoolType.Umbrella);

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
                                            var newBudgetPool = new GlObjectCodeBudgetPool(newUmbrellaGlAccount);

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
                #endregion

                #region Process all non-pooled accounts
                // Calculate the previous fiscal year.
                int previousFiscalYear = 0;
                try
                {
                    previousFiscalYear = Convert.ToInt32(fiscalYear) - 1;
                }
                catch (Exception ex)
                {
                    logger.Warn(string.Format("Error converting fiscal year {0} from string to int.", fiscalYear));
                }

                // If the previous fiscal year is open then read the GLS records for the non-pooled accounts for the current FY .
                Collection<GlsFyr> nonPooledGlsContracts = null;
                Collection<GlsFyr> previousNonPooledGlsContracts = null;

                if (previousFiscalYear != 0)
                {
                    var previousFiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(previousFiscalYear.ToString());
                    if (previousFiscalYearGenLdgrContract != null && previousFiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                    {
                        var nonPooledGlAccountNumbers = nonPooledAccounts.Select(x => x.Recordkey).ToArray();
                        var glsFyrYearId = "GLS." + fiscalYear.ToString();
                        nonPooledGlsContracts = await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrYearId, nonPooledGlAccountNumbers.Where(x => GetGlAccountGlClass(x, glClassConfiguration) == GlClass.FundBalance).ToArray());

                        var previousGlsFyrYearId = "GLS." + previousFiscalYear.ToString();
                        previousNonPooledGlsContracts = await DataReader.BulkReadRecordAsync<GlsFyr>(previousGlsFyrYearId, nonPooledGlAccountNumbers.Where(x =>
                            GetGlAccountGlClass(x, glClassConfiguration) == GlClass.Asset || GetGlAccountGlClass(x, glClassConfiguration) == GlClass.Liability).ToArray());
                    }
                }

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
                        var glAccountAmounts = nonPooledAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

                        // If the GL account has information for the fiscal year passed in, process the amounts.
                        if (glAccountAmounts != null)
                        {
                            // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                            var nonPooledGlAccount = new GlObjectCodeGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                            nonPooledGlAccount.BudgetAmount = glAccountAmounts.GlBudgetPostedAssocMember.HasValue ? glAccountAmounts.GlBudgetPostedAssocMember.Value : 0m;
                            nonPooledGlAccount.BudgetAmount += glAccountAmounts.GlBudgetMemosAssocMember.HasValue ? glAccountAmounts.GlBudgetMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? glAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.EncumbranceAmount += glAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? glAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
                            nonPooledGlAccount.ActualAmount += glAccountAmounts.GlActualPostedAssocMember.HasValue ? glAccountAmounts.GlActualPostedAssocMember.Value : 0m;
                            nonPooledGlAccount.ActualAmount += glAccountAmounts.GlActualMemosAssocMember.HasValue ? glAccountAmounts.GlActualMemosAssocMember.Value : 0m;

                            // For Fund Balance accounts only, add the estimated opening balance if the previous fiscal year is also open.
                            if (glClass == GlClass.FundBalance && nonPooledGlsContracts != null)
                            {
                                var nonPooledGlsContract = nonPooledGlsContracts.FirstOrDefault(x => x.Recordkey == nonPooledGlAccount.GlAccountNumber);
                                if (nonPooledGlsContract != null)
                                {
                                    nonPooledGlAccount.ActualAmount += nonPooledGlsContract.GlsEstimatedOpenBal.HasValue ? nonPooledGlsContract.GlsEstimatedOpenBal.Value : 0m;
                                }
                            }

                            // For Asset or Liability accounts, add the estimated opening balance if the previous fiscal year is also open. Add the open balance, monthly
                            // debits, and subtract the monthly credits from the previous year GLS record.
                            if ((glClass == GlClass.Asset || glClass == GlClass.Liability) && nonPooledGlsContracts != null)
                            {
                                var previousNonPooledGlsContract = previousNonPooledGlsContracts.FirstOrDefault(x => x.Recordkey == nonPooledGlAccount.GlAccountNumber);
                                if (previousNonPooledGlsContract != null)
                                {
                                    nonPooledGlAccount.ActualAmount += previousNonPooledGlsContract.OpenBal.HasValue ? previousNonPooledGlsContract.OpenBal.Value : 0m;
                                    var monthlyDebits = previousNonPooledGlsContract.Mdebits;
                                    if (monthlyDebits != null)
                                    {
                                        foreach (var debitAmt in monthlyDebits)
                                        {
                                            nonPooledGlAccount.ActualAmount += debitAmt.HasValue ? debitAmt.Value : 0m;
                                        }
                                    }
                                    var monthlyCredits = previousNonPooledGlsContract.Mcredits;
                                    if (monthlyCredits != null)
                                    {
                                        foreach (var creditAmt in monthlyCredits)
                                        {
                                            nonPooledGlAccount.ActualAmount -= creditAmt.HasValue ? creditAmt.Value : 0m;
                                        }
                                    }
                                }
                            }

                            // Add the GL account to the list of non-pooled accounts.
                            nonPooledGlAccountDomainEntities.Add(nonPooledGlAccount);
                        }
                    }
                }
                #endregion

                #region Assign all pools to a cost center and subtotal
                foreach (var budgetPool in glBudgetPoolDomainEntities)
                {
                    AddBudgetPoolToGlObjectCodeList(budgetPool, glClassConfiguration);
                }
                #endregion

                #region Assign all non-pooled GL accounts to a cost center and subtotal
                foreach (var nonPooledAccount in nonPooledGlAccountDomainEntities)
                {
                    AddNonPooledAccountToGlObjectCodeList(nonPooledAccount, glClassConfiguration);
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
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria))
                {
                    glsRecords = await GetOrAddToCacheAsync<Collection<GlsFyr>>("GlObjectCodesGlsFyr" + personId + costCenterCriteria.FiscalYear, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrId, userGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    glsRecords = await DataReader.BulkReadRecordAsync<GlsFyr>(glsFyrId, userGlAccounts);
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

                var glBudgetPoolDomainEntities = new List<GlObjectCodeBudgetPool>();
                var nonPooledGlAccountDomainEntities = new List<GlObjectCodeGlAccount>();

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
                            // Create GL account for umbrella and populate budget, actual and encumbrance amounts.
                            var umbrellaGlAccount = new GlObjectCodeGlAccount(umbrella.Recordkey, GlBudgetPoolType.Umbrella);
                            umbrellaGlAccount.BudgetAmount = CalculateBudgetForGlAccountInClosedYear(umbrella);
                            umbrellaGlAccount.ActualAmount = CalculateActualsForUmbrellaInClosedYear(umbrella);
                            umbrellaGlAccount.EncumbranceAmount = CalculateEncumbrancesForUmbrellaInClosedYear(umbrella);

                            // Create new pool for umbrella
                            var newBudgetPool = new GlObjectCodeBudgetPool(umbrellaGlAccount);
                            newBudgetPool.IsUmbrellaVisible = true;

                            // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                            if ((umbrella.CreditsYtd.HasValue && umbrella.CreditsYtd.Value != 0)
                                || (umbrella.DebitsYtd.HasValue && umbrella.DebitsYtd.Value != 0)
                                || (umbrella.OpenBal.HasValue && umbrella.OpenBal.Value != 0)
                                || (umbrella.EOpenBal.HasValue && umbrella.EOpenBal.Value != 0)
                                || (umbrella.EncumbrancesYtd.HasValue && umbrella.EncumbrancesYtd.Value != 0)
                                || (umbrella.EncumbrancesRelievedYtd.HasValue && umbrella.EncumbrancesRelievedYtd.Value != 0))
                            {
                                var umbrellaPoolee = new GlObjectCodeGlAccount(umbrella.Recordkey, GlBudgetPoolType.Poolee);
                                umbrellaPoolee.EncumbranceAmount = CalculateEncumbrancesForGlAccountInClosedYear(umbrella);
                                umbrellaPoolee.ActualAmount = CalculateActualsForGlAccountInClosedYear(umbrella, glClassConfiguration);

                                newBudgetPool.AddPoolee(umbrellaPoolee);
                            }

                            // Add new budget pool to the list of new budget pools
                            glBudgetPoolDomainEntities.Add(newBudgetPool);
                        }
                    }
                    else
                    {
                        excludedUmbrellaGlAccounts.Add(umbrella.Recordkey);
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

                                // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                                var pooleeGlAccount = new GlObjectCodeGlAccount(poolee.Recordkey, GlBudgetPoolType.Poolee);
                                pooleeGlAccount.BudgetAmount = CalculateBudgetForGlAccountInClosedYear(poolee);
                                pooleeGlAccount.ActualAmount = CalculateActualsForGlAccountInClosedYear(poolee, glClassConfiguration);
                                pooleeGlAccount.EncumbranceAmount = CalculateEncumbrancesForGlAccountInClosedYear(poolee);

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
                                    var newUmbrellaGlAccount = new GlObjectCodeGlAccount(umbrellaAccount.Recordkey, GlBudgetPoolType.Umbrella);
                                    newUmbrellaGlAccount.BudgetAmount = CalculateBudgetForGlAccountInClosedYear(umbrellaAccount);
                                    newUmbrellaGlAccount.ActualAmount = CalculateActualsForUmbrellaInClosedYear(umbrellaAccount);
                                    newUmbrellaGlAccount.EncumbranceAmount = CalculateEncumbrancesForUmbrellaInClosedYear(umbrellaAccount);

                                    // Create a new budget pool and add the poolee to it.
                                    var newBudgetPool = new GlObjectCodeBudgetPool(newUmbrellaGlAccount);
                                    newBudgetPool.AddPoolee(pooleeGlAccount);

                                    // Add new budget pool to the list of new budget pools
                                    glBudgetPoolDomainEntities.Add(newBudgetPool);
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
                        // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                        var nonPooledGlAccount = new GlObjectCodeGlAccount(nonPooledAccount.Recordkey, GlBudgetPoolType.None);
                        nonPooledGlAccount.BudgetAmount = CalculateBudgetForGlAccountInClosedYear(nonPooledAccount);
                        nonPooledGlAccount.EncumbranceAmount = CalculateEncumbrancesForGlAccountInClosedYear(nonPooledAccount);
                        nonPooledGlAccount.ActualAmount = CalculateActualsForGlAccountInClosedYear(nonPooledAccount, glClassConfiguration);

                        // Add the GL account to the list of non-pooled accounts.
                        nonPooledGlAccountDomainEntities.Add(nonPooledGlAccount);
                    }
                }
                #endregion

                #region Assign all pools to a cost center and subtotal
                foreach (var budgetPool in glBudgetPoolDomainEntities)
                {
                    AddBudgetPoolToGlObjectCodeList(budgetPool, glClassConfiguration);
                }
                #endregion

                #region Assign all non-pooled GL accounts to a cost center and subtotal
                foreach (var nonPooledAccount in nonPooledGlAccountDomainEntities)
                {
                    AddNonPooledAccountToGlObjectCodeList(nonPooledAccount, glClassConfiguration);
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
                if (FilterUtility.IsFilterWideOpen(costCenterCriteria))
                {
                    encRecords = await GetOrAddToCacheAsync<Collection<EncFyr>>("GlObjectCodesEncFyr" + personId + costCenterCriteria.FiscalYear, async () =>
                    {
                        return await DataReader.BulkReadRecordAsync<EncFyr>(encFyrId, userGlAccounts);
                    }, ThirtyMinuteCacheTimeout);
                }
                else
                {
                    encRecords = await DataReader.BulkReadRecordAsync<EncFyr>(encFyrId, userGlAccounts);
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
                    var requisitionGlAccountsList = new List<GlObjectCodeGlAccount>();
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

                                // Only process this poolee if the umbrella has not been excluded.
                                if (!excludedUmbrellaGlAccounts.Contains(pooleeUmbrella))
                                {
                                    var requisitionGlAccountDomain = new GlObjectCodeGlAccount(reqGlAccount.Recordkey, poolType);

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
                            //var allUmbrellaAccounts = glObjectCodes.SelectMany(x => x.CostCenterSubtotals.SelectMany(y => y.Pools.Select(z => z.Umbrella))).ToList();
                            var allUmbrellaAccounts = glObjectCodes.SelectMany(x => x.Pools.Select(z => z.Umbrella)).ToList();
                            var allPooleeAccounts = glObjectCodes.SelectMany(x => x.Pools.SelectMany(z => z.Poolees)).ToList();
                            var allNonPooledAccounts = glObjectCodes.SelectMany(x => x.GlAccounts).ToList();

                            // Loop through each cost center subtotal in the cost center.
                            //var allSubtotals = glObjectCodes.SelectMany(x => x.CostCenterSubtotals).ToList();
                            foreach (var objectCode in glObjectCodes)
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                                {
                                    // Loop through each GL account in the list that belong to this cost center subtotal.
                                    foreach (var acct in objectCode.GlAccounts)
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
                                    foreach (var pool in objectCode.Pools)
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
                                                    if (userGlAccounts.Contains(reqGlAcct.GlAccountNumber) && reqGlAcct.EncumbranceAmount != 0)
                                                    {
                                                        // Create a poolee to represent the umbrella and add the encumbrance amount, then add it to the poolees list.
                                                        var poolee = new GlObjectCodeGlAccount(reqGlAcct.GlAccountNumber, GlBudgetPoolType.Poolee);
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
                                                glAccountFound = true;
                                            }
                                        }
                                    }
                                }
                            }

                            // If the GL account is not included in any cost center, find the cost center subtotal and cost center for it, 
                            // whether they already exist, or we have to create new ones.
                            if (!glAccountFound)
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                                {
                                    AddNonPooledAccountToGlObjectCodeList(reqGlAcct, glClassConfiguration);
                                }
                                else
                                {
                                    if (reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                    {
                                        var budgetPool = new GlObjectCodeBudgetPool(reqGlAcct);
                                        AddBudgetPoolToGlObjectCodeList(budgetPool, glClassConfiguration);
                                    }
                                    else
                                    {
                                        // Find the umbrella from GLP.FYR
                                        var selectedPool = glpFyrDataContracts.FirstOrDefault(x => x.GlpPooleeAcctsList.Contains(reqGlAcct.GlAccountNumber));
                                        if (selectedPool != null)
                                        {
                                            var umbrellaAccount = selectedPool.Recordkey;
                                            var budgetPool = new GlObjectCodeBudgetPool(new GlObjectCodeGlAccount(selectedPool.Recordkey, GlBudgetPoolType.Umbrella));
                                            budgetPool.AddPoolee(reqGlAcct);
                                            AddBudgetPoolToGlObjectCodeList(budgetPool, glClassConfiguration);
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
            // Populate the object descriptions with the info from Colleague.
            var obDescs = await DataReader.BulkReadRecordAsync<ObDescs>(glObjectCodes.Select(x => x.Id).ToArray());

            foreach (var glObjectCode in glObjectCodes)
            {
                var obDesc = obDescs.FirstOrDefault(x => x.Recordkey == glObjectCode.Id);
                if (obDesc != null)
                {
                    glObjectCode.Name = obDesc.ObDescription;
                }
            }
            #endregion

            #region Remove inactive accounts with no activity

            // Get the non pooled account GL account number strings from the CostCenterGlAccounts from all of the cost centers
            // where all of the amounts are zero.
            var possibleCostCenterGlAccountsToRemove = glObjectCodes.SelectMany(x => x.GlAccounts).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0).ToList();

            // Get the poolee account GL account number strings from the CostCenterGlAccounts from all of the cost centers
            // where all of the amounts are zero.
            var possibleCostCenterGlAccountPooleesToRemove = glObjectCodes.SelectMany(x => x.Pools.SelectMany(y => y.Poolees).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0)).ToList();

            // Add the list of poolee accounts to the list of non pooled accounts.
            if (possibleCostCenterGlAccountPooleesToRemove != null)
            {
                possibleCostCenterGlAccountsToRemove.AddRange(possibleCostCenterGlAccountPooleesToRemove);
            }

            // If there are any CostCenterGlAccounts with all zero amounts, find out if there is any activity for 
            // any of these GL accounts by selecting GLS and ENC records. Also, find out if any of these GL accounts
            // are active by selecting GL.ACCTS that are not currently inactive. Remove these GL account record IDs
            // from the list of possible record IDs to remove.
            if (possibleCostCenterGlAccountsToRemove != null)
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
                    var costCenterGlToBeDeleted = glObjectCodes.SelectMany(x => x.GlAccounts).Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber)).ToList();
                    var costCenterPooleeToBeDeleted = glObjectCodes.SelectMany(x => x.Pools).SelectMany(z => z.Poolees).Where(a => inactiveGlAccountsWithNoActivity.Contains(a.GlAccountNumber)).ToList();

                    foreach (var sub in glObjectCodes)
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

                    if (costCenterGlToBeDeleted.Any())
                    {
                        glObjectCodes = glObjectCodes.Except(glObjectCodes.Where(x => !x.GlAccounts.Any() && !x.Pools.Any())).ToList();
                    }
                }
            }
            #endregion

            return glObjectCodes;
        }

        #region Private methods

        private void AddNonPooledAccountToGlObjectCodeList(GlObjectCodeGlAccount glAccount, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            // Get the object ID of the GL account.
            var glAccountObjectCode = glAccount.GlAccountNumber.Substring(objectMajorComponent.StartPosition, objectMajorComponent.ComponentLength);

            // Add the budget pool to the GL Object, if the GL Object does not exist create it, then add the budget pool to it.
            var selectedGlObjectCode = glObjectCodes.FirstOrDefault(x => x.Id == glAccountObjectCode);
            if (selectedGlObjectCode != null)
            {
                selectedGlObjectCode.AddGlAccount(glAccount);
            }
            else
            {
                var glClass = GetGlAccountGlClass(glAccount.GlAccountNumber, glClassConfiguration);
                glObjectCodes.Add(new GlObjectCode(glAccountObjectCode, glAccount, glClass));
            }
        }

        private void AddBudgetPoolToGlObjectCodeList(GlObjectCodeBudgetPool budgetPool, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            // Get the object ID of the umbrella account.
            var umbrellaObjectCode = budgetPool.Umbrella.GlAccountNumber.Substring(objectMajorComponent.StartPosition, objectMajorComponent.ComponentLength);

            // Add the budget pool to the GL Object, if the GL Object does not exist create it, then add the budget pool to it.
            var selectedGlObjectCode = glObjectCodes.FirstOrDefault(x => x.Id == umbrellaObjectCode);
            if (selectedGlObjectCode != null)
            {
                selectedGlObjectCode.AddBudgetPool(budgetPool);
            }
            else
            {
                var glClass = GetGlAccountGlClass(budgetPool.Umbrella.GlAccountNumber, glClassConfiguration);
                glObjectCodes.Add(new GlObjectCode(umbrellaObjectCode, budgetPool, glClass));
            }
        }

        private decimal CalculateBudgetForGlAccountInClosedYear(GlsFyr glsContract)
        {
            var budgetAmount = glsContract.BAlocDebitsYtd.HasValue ? glsContract.BAlocDebitsYtd.Value : 0m;
            budgetAmount -= glsContract.BAlocCreditsYtd.HasValue ? glsContract.BAlocCreditsYtd.Value : 0m;

            return budgetAmount;
        }

        private decimal CalculateActualsForGlAccountInClosedYear(GlsFyr glsContract, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            var actualsAmount = 0m;
            var glClass = GetGlAccountGlClass(glsContract.Recordkey, glClassConfiguration);
            if (glClass == GlClass.FundBalance)
            {
                actualsAmount = glsContract.OpenBal.HasValue ? glsContract.OpenBal.Value : 0m;
                actualsAmount += glsContract.CloseDebits.HasValue ? glsContract.CloseDebits.Value : 0m;
                actualsAmount -= glsContract.CloseCredits.HasValue ? glsContract.CloseCredits.Value : 0m;

                var closedMonthlyDebits = glsContract.Mdebits;
                if (closedMonthlyDebits != null)
                {
                    foreach (var closedDebitAmt in closedMonthlyDebits)
                    {
                        actualsAmount += closedDebitAmt.HasValue ? closedDebitAmt.Value : 0m;
                    }
                }
                var closedMonthlyCredits = glsContract.Mcredits;
                if (closedMonthlyCredits != null)
                {
                    foreach (var closedCreditAmt in closedMonthlyCredits)
                    {
                        actualsAmount -= closedCreditAmt.HasValue ? closedCreditAmt.Value : 0m;
                    }
                }
            }
            else
            {
                actualsAmount = glsContract.OpenBal.HasValue ? glsContract.OpenBal.Value : 0m;
                actualsAmount += glsContract.DebitsYtd.HasValue ? glsContract.DebitsYtd.Value : 0m;
                actualsAmount -= glsContract.CreditsYtd.HasValue ? glsContract.CreditsYtd.Value : 0m;
            }


            return actualsAmount;
        }

        private decimal CalculateEncumbrancesForGlAccountInClosedYear(GlsFyr glsContract)
        {
            var encumbranceAmount = glsContract.EOpenBal.HasValue ? glsContract.EOpenBal.Value : 0m;
            encumbranceAmount += glsContract.EncumbrancesYtd.HasValue ? glsContract.EncumbrancesYtd.Value : 0m;
            encumbranceAmount -= glsContract.EncumbrancesRelievedYtd.HasValue ? glsContract.EncumbrancesRelievedYtd.Value : 0m;

            return encumbranceAmount;
        }

        private decimal CalculateActualsForUmbrellaInClosedYear(GlsFyr glsContract)
        {
            var actualsAmount = 0m;
            foreach (var amount in glsContract.GlsFaMactuals)
            {
                actualsAmount += amount.HasValue ? amount.Value : 0m;
            }

            return actualsAmount;
        }

        private decimal CalculateEncumbrancesForUmbrellaInClosedYear(GlsFyr glsContract)
        {
            var encumbranceAmount = 0m;
            foreach (var amount in glsContract.GlsFaMencumbrances)
            {
                encumbranceAmount += amount.HasValue ? amount.Value : 0m;
            }

            return encumbranceAmount;
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