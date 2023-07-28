// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IFinanceQueryRepository interface.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class FinanceQueryRepository : BaseColleagueRepository, IFinanceQueryRepository
    {

        private const int ThirtyMinuteCacheTimeout = 30;
        private readonly int bulkReadSize;

        /// <summary>
        /// This constructor allows us to instantiate a finance query repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public FinanceQueryRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            bulkReadSize = settings != null && settings.BulkReadSize > 0 ? settings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get a list of GL accounts assigned to the user logged in.
        /// finance query filter criteria is used to filter the GL accounts.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="glAccountStructure">GL Account structure domain entity.</param>
        /// <param name="glClassConfiguration">GL class configuration structure domain entity.</param>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>List of finance query gl account line item entities.</returns>
        public async Task<IEnumerable<FinanceQueryGlAccountLineItem>> GetGLAccountsListAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, FinanceQueryCriteria criteria, string personId)
        {
            List<FinanceQueryGlAccountLineItem> filteredFinanceQueryGlAccountLineItems = new List<FinanceQueryGlAccountLineItem>();

            // Get the fiscal year from the filter criteria.
            var fiscalYear = criteria.FiscalYear;

            if (string.IsNullOrEmpty(fiscalYear))
            {
                LogDataError("fiscalYear", "", fiscalYear);
                return filteredFinanceQueryGlAccountLineItems;
            }

            // Read the GEN.LDGR record for the fiscal year
            var genLdgrDataContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear);

            #region Error checking
            if (generalLedgerUser == null)
            {
                logger.Error("General Ledger User is null. No Finance Query line items to return.");
                return filteredFinanceQueryGlAccountLineItems;
            }

            // If the user does not have any GL accounts assigned, return an empty list of GL object codes.
            if ((generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any()))
            {
                LogDataError("AllAccounts", "", generalLedgerUser.AllAccounts);
                return filteredFinanceQueryGlAccountLineItems;
            }

            if (glAccountStructure == null)
            {
                LogDataError("glAccountStructure", "", glAccountStructure);
                return filteredFinanceQueryGlAccountLineItems;
            }

            if (glClassConfiguration == null)
            {
                LogDataError("glClassConfiguration", "", glClassConfiguration);
                return filteredFinanceQueryGlAccountLineItems;
            }


            if (genLdgrDataContract == null)
            {
                logger.Warn("Missing GEN.LDGR record for ID: " + fiscalYear);
                return filteredFinanceQueryGlAccountLineItems;
            }

            if (string.IsNullOrEmpty(genLdgrDataContract.GenLdgrStatus))
            {
                logger.Warn("GEN.LDGR status is null.");
                return filteredFinanceQueryGlAccountLineItems;
            }
            #endregion

            // all GL accounts for a user has access to.
            string[] allGlAccountsForUser = generalLedgerUser.AllAccounts.ToArray();

            // Limit the list of accounts using the filter component criteria. If the data is not filtered,
            // return the finance query object for all the GL accounts assigned to the user.
            string[] filteredUserGlAccounts = await ApplyFilterCriteria(criteria, allGlAccountsForUser);

            if (filteredUserGlAccounts != null && filteredUserGlAccounts.Any())
            {
                // If the fiscal year is open, get the information from GL.ACCTS
                if (genLdgrDataContract.GenLdgrStatus == "O")
                {
                    #region Get open year amounts from GL.ACCTS
                    // Bulk read the GL Account records. At this point userGlAccounts contains the list
                    // of all GL accounts the user has access to.
                    List<GlAccts> glAccountRecords;
                    if (IsFilterWideOpen(criteria))
                    {
                        glAccountRecords = await GetOrAddToCacheAsync<List<GlAccts>>("FinanceQueryGlAccts" + personId, async () => await BulkReadRecordAsync<GlAccts>(filteredUserGlAccounts), ThirtyMinuteCacheTimeout);
                    }
                    else
                    {
                        glAccountRecords = await BulkReadRecordAsync<GlAccts>(filteredUserGlAccounts);
                    }

                    if (glAccountRecords != null)
                    {
                        filteredFinanceQueryGlAccountLineItems = await ProcessAllGlAccountsForOpenYear(glAccountStructure, glClassConfiguration, fiscalYear, allGlAccountsForUser, filteredUserGlAccounts, glAccountRecords);
                    }


                    #endregion
                }
                else
                {
                    #region Get closed year amounts from GLS.FYR & ENC.FYR

                    // If the fiscal year is closed, obtain the information from GLS.FYR and ENC.FYR.
                    // Bulk read the GLS.FYR records.
                    string glsFyrId = "GLS." + fiscalYear;
                    List<GlsFyr> glsRecords = new List<GlsFyr>();
                    if (IsFilterWideOpen(criteria))
                    {
                        glsRecords = await GetOrAddToCacheAsync<List<GlsFyr>>("FinanceQueryGlsFyr" + personId + criteria.FiscalYear, async () =>
                        {
                            return await BulkReadRecordAsync<GlsFyr>(glsFyrId, filteredUserGlAccounts);
                        }, ThirtyMinuteCacheTimeout);
                    }
                    else
                    {
                        glsRecords = await BulkReadRecordAsync<GlsFyr>(glsFyrId, filteredUserGlAccounts);
                    }

                    List<GlpFyr> glpFyrDataContracts = new List<GlpFyr>();
                    string glpFyrFilename = "GLP." + fiscalYear;
                    var glpFyrIds = await DataReader.SelectAsync(glpFyrFilename, null);

                    if (glpFyrIds != null && glpFyrIds.Any())
                        glpFyrDataContracts = await BulkReadRecordAsync<GlpFyr>(glpFyrFilename, glpFyrIds);

                    //cf web defaults
                    var cfWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");

                    var allGlAccountLineItems = await ProcessAllGlAccountsForClosedYear(glsRecords, glpFyrDataContracts, glsFyrId, glAccountStructure, glClassConfiguration, criteria, personId, fiscalYear, filteredUserGlAccounts, allGlAccountsForUser, cfWebDefaults);
                    filteredFinanceQueryGlAccountLineItems.AddRange(allGlAccountLineItems);

                    #endregion
                }
            }

            #region Remove inactive accounts with no activity

            var glAccountsToBeRemoved = await RemoveInActiveAccountsWithNoActivity(criteria, filteredFinanceQueryGlAccountLineItems, fiscalYear);
            if (glAccountsToBeRemoved.Any())
            {
                foreach (var item in glAccountsToBeRemoved)
                {
                    filteredFinanceQueryGlAccountLineItems.Remove(item);
                }
            }

            #endregion

            return filteredFinanceQueryGlAccountLineItems;
        }

        /// <summary>
        /// Get a list of GL accounts assigned to the user logged in.
        /// finance query filter criteria is used to filter the GL accounts.
        /// </summary>
        /// <param name="generalLedgerUser">General Ledger User domain entity.</param>
        /// <param name="glAccountStructure">GL Account structure domain entity.</param>
        /// <param name="glClassConfiguration">GL class configuration structure domain entity.</param>
        /// <param name="criteria">Finance query filter criteria.</param>
        /// <param name="personId">ID of the user.</param>
        /// <returns>List of finance query account activity entities.</returns>
        public async Task<IEnumerable<FinanceQueryActivityDetail>> GetFinanceQueryActivityDetailAsync(GeneralLedgerUser generalLedgerUser, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, FinanceQueryCriteria criteria, string personId)
        {
            // Note: This method is similar to the QueryGlActivityDetailAsync method in GeneralLedgerActivityDetailRepository
            // but does not all of the same details.
            List<FinanceQueryActivityDetail> financeQueryActivityTransactions = new List<FinanceQueryActivityDetail>();

            // Get the fiscal year from the filter criteria.
            var fiscalYear = criteria.FiscalYear;

            if (string.IsNullOrEmpty(fiscalYear))
            {
                LogDataError("fiscalYear", "", fiscalYear);
                return financeQueryActivityTransactions;
            }

            // Read the GEN.LDGR record for the fiscal year
            var genLdgrDataContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear);

            #region Error checking
            if (generalLedgerUser == null)
            {
                logger.Error("General Ledger User is null. No Finance Query line items to return.");
                return financeQueryActivityTransactions;
            }

            // If the user does not have any GL accounts assigned, return an empty list of GL object codes.
            if ((generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any()))
            {
                LogDataError("AllAccounts", "", generalLedgerUser.AllAccounts);
                return financeQueryActivityTransactions;
            }

            if (glAccountStructure == null)
            {
                LogDataError("glAccountStructure", "", glAccountStructure);
                return financeQueryActivityTransactions;
            }

            if (glClassConfiguration == null)
            {
                LogDataError("glClassConfiguration", "", glClassConfiguration);
                return financeQueryActivityTransactions;
            }


            if (genLdgrDataContract == null)
            {
                logger.Warn("Missing GEN.LDGR record for ID: " + fiscalYear);
                return financeQueryActivityTransactions;
            }

            if (string.IsNullOrEmpty(genLdgrDataContract.GenLdgrStatus))
            {
                logger.Warn("GEN.LDGR status is null.");
                return financeQueryActivityTransactions;
            }
            #endregion

            // all GL accounts for a user has access to.
            string[] allGlAccountsForUser = generalLedgerUser.AllAccounts.ToArray();

            // Limit the list of accounts using the filter component criteria. If the data is not filtered,
            // return the finance query object for all the GL accounts assigned to the user.
            string[] filteredUserGlAccounts = await ApplyFilterCriteria(criteria, allGlAccountsForUser);

            // Get GL activity transactions for the set of filtered GL accounts.
            var filteredUserGlAccountsList = filteredUserGlAccounts.ToList();
            financeQueryActivityTransactions = await QueryGlActivitiesDetailAsync(filteredUserGlAccountsList, fiscalYear, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);
            return financeQueryActivityTransactions;
        }

        /// <summary>
        /// This method gets the list of activity detail for actuals and encumbrances for a set of GL accounts for a fiscal year.
        /// </summary>
        /// <param name="glAccounts">The GL account numbers.</param>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <returns>Returns the list of activity detail posted to the GL account for the fiscal year.</returns>
        private async Task<List<FinanceQueryActivityDetail>> QueryGlActivitiesDetailAsync(List<string> glAccounts, string fiscalYear, GeneralLedgerClassConfiguration glClassConfiguration, IList<string> majorComponentStartPosition)
        {
            List<FinanceQueryActivityDetail> glActivities = new List<FinanceQueryActivityDetail>();

            if (glAccounts == null || !glAccounts.Any())
            {
                throw new ArgumentNullException("glAccounts", "A GL account must be specified.");
            }

            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("fiscalYear", "A fiscal year must be specified.");
            }

            var glAccountDomains = new List<FinanceQueryActivityDetail>();

            // Create variable for containing a list of GL transactions
            var glTransactionEntities = new List<GlTransaction>();

            try
            {
                // Get the GL.SOURCE.CODES valcode table
                var GlSourceCodesValidationTable = await GetGlSourceCodesAsync();

                // Create variable for determining the type of transaction (e.g. Actual, Encumbrance, or Requisition)
                ApplValcodesVals GLSourceCode = new ApplValcodesVals();

                #region Process transactions from GLA.FYR

                // Initialize the file name and criteria.
                var glaFyrFileName = "GLA." + fiscalYear;

                // Get the GL activity detail for the list of glAccounts
                var glaCriteria = "WITH @ID LIKE ";
                var numberAccounts = glAccounts.Count();
                List<string> glaList = new List<string>();
                string selectDuration = "";
                logger.Debug("Number of filtered accounts: " + numberAccounts);

                var watch = new Stopwatch();
                string[] glaFyrIds = null;

                // Select GLA records based on the number of filtered GL accounts.
                if (numberAccounts <= 100)
                {
                    foreach (var glstring in glAccounts)
                    {
                        if (!string.IsNullOrEmpty(glstring))
                        {
                            // For alpha GL accounts, avoid the problem with Unidata using A, N and X as wildcards.
                            // The criteria has to be like "'11_01_01_00_10005_54N70'..." for it to work.
                            glaCriteria = glaCriteria + "\"'" + glstring + "'...\"";
                        }
                    }
                    watch.Start();
                    // Select and bulk read the GLA records for the GL account.
                    glaFyrIds = await DataReader.SelectAsync(glaFyrFileName, glaCriteria);
                    watch.Stop();
                    glaList.AddRange(glaFyrIds);
                    selectDuration = watch.ElapsedMilliseconds.ToString();
                    logger.Debug("GLA selection duration: " + selectDuration);
                    logger.Debug("GLA records selected: " + glaList.Count());
                }
                else
                {
                    int stringCounter = 0;
                    int runningGlaCount = 0;
                    int selectAsyncCount = 0;
                    watch.Start();

                    foreach (var glstring in glAccounts)
                    {
                        if (!string.IsNullOrEmpty(glstring))
                        {
                            // For alpha GL accounts, avoid the problem with Unidata using A, N and X as wildcards.
                            // The criteria has to be like "'11_01_01_00_10005_54N70'..." for it to work.
                            glaCriteria = glaCriteria + "\"'" + glstring + "'...\"";
                            stringCounter += 1;
                            if (stringCounter == 100)
                            {
                                // Select and bulk read the GLA records for the GL account.
                                glaFyrIds = await DataReader.SelectAsync(glaFyrFileName, glaCriteria);
                                glaList.AddRange(glaFyrIds);
                                glaFyrIds = null;
                                runningGlaCount = glaList.Count();
                                stringCounter = 0;
                                glaCriteria = "WITH @ID LIKE ";
                                selectAsyncCount += 1;
                            }
                        }
                    }
                    watch.Stop();
                    selectDuration = watch.ElapsedMilliseconds.ToString();
                    logger.Debug("Number of selection statements: " + selectAsyncCount);
                    logger.Debug("GLA selection duration: " + selectDuration);
                    logger.Debug("GLA records selected: " + runningGlaCount);
                    glaFyrIds = glaList.ToArray();
                }

                // Bulk read the GLA records for the GL account.
                var glaDataContracts = await DataReader.BulkReadRecordAsync<GlaFyr>(glaFyrFileName, glaFyrIds, true);

                foreach (var transaction in glaDataContracts)
                {
                    try
                    {
                        if (transaction != null)
                        {
                            // Check transaction properties for missing data.
                            // Throw an exception if any of the transaction properties are null.
                            string errorPrefix = "GLA." + fiscalYear + " " + transaction.Recordkey + " data contract ";
                            if (string.IsNullOrEmpty(transaction.Recordkey))
                                throw new ApplicationException(errorPrefix + "has no record key.");

                            // Extract the GL account number from the record key.
                            var glNumber = transaction.Recordkey.Split('*').FirstOrDefault();

                            if (string.IsNullOrEmpty(glNumber))
                                throw new ApplicationException(errorPrefix + "has no GL account number.");

                            if (string.IsNullOrEmpty(transaction.GlaSource))
                                throw new ApplicationException(errorPrefix + "has no source code.");

                            if (string.IsNullOrEmpty(transaction.GlaRefNo))
                                throw new ApplicationException(errorPrefix + "has no reference number.");

                            if (!transaction.GlaTrDate.HasValue)
                                throw new ApplicationException(errorPrefix + "has no transaction date.");

                            // Determine the transaction type, create the transaction and add it to the list.
                            GlTransactionType type = GlTransactionType.Actual;
                            GLSourceCode = GlSourceCodesValidationTable.ValsEntityAssociation.Where(x =>
                                x.ValInternalCodeAssocMember == transaction.GlaSource).FirstOrDefault();

                            // GLA.FYR only contains transactions for types budget, actual and encumbrances. 
                            // Requisitions only exist in ENC.FYR.
                            // 1: Actuals
                            // 2: Budget
                            // 3: Encumbrances
                            // 4: Requisitions
                            // handle 1 and 2 only to create a GL transaction if the type if Actual or Budget.
                            // Use ENC to get outstanding encumbrances and requisitions.
                            switch (GLSourceCode.ValActionCode2AssocMember)
                            {
                                case "1":
                                    type = GlTransactionType.Actual;
                                    break;
                                case "2":
                                    type = GlTransactionType.Budget;
                                    break;
                                case "3":
                                    type = GlTransactionType.Encumbrance;
                                    break;
                                case "4":
                                    // Requisitions are not posted to GLA.FYR.
                                    type = GlTransactionType.Requisition;
                                    LogDataError("Requisition activity should not be present in GLA." + fiscalYear, transaction.Recordkey, transaction);
                                    break;
                                default:
                                    // Log an error for bad data
                                    // There should not be any budget or wrong source codes
                                    throw new ApplicationException("Unrecognizable GL Source Code: " + transaction.GlaSource);
                            }

                            // Exclude actuals created in the GL year end process. These have a source code of YE.
                            if (transaction.GlaSource != "YE")
                            {
                                // Only add budget and actuals transactions to the domain entity.
                                if (type == GlTransactionType.Budget || type == GlTransactionType.Actual)
                                {
                                    var glTransactionEntity = new GlTransaction(transaction.Recordkey, type, transaction.GlaSource, glNumber,
                                        (transaction.GlaDebit ?? 0) - (transaction.GlaCredit ?? 0), transaction.GlaRefNo, transaction.GlaTrDate.Value, transaction.GlaDescription);

                                    // Add the transaction to the list.
                                    glTransactionEntities.Add(glTransactionEntity);
                                }
                            }
                        }
                    }

                    catch (ApplicationException ae)
                    {
                        LogDataError("", "", transaction, ae);
                    }
                }

                #endregion

                #region Process transactions from ENC.FYR

                // Initialize the file name and criteria.
                var encFyrFileName = "ENC." + fiscalYear;

                // Bulk read the ENC records for the GL accounts.
                string[] encFyrIds = glAccounts.ToArray();
                var encDataContracts = await DataReader.BulkReadRecordAsync<EncFyr>(encFyrFileName, encFyrIds, true);

                foreach (var encDataContract in encDataContracts)
                {
                    // There may be no outstanding encumbrances for this GL account and so the data contract would be null.
                    if (encDataContract != null)
                    {
                        // Use a counter to create the sequential part of the transaction id to mimic the ids in GLA.
                        int encCounter = 0;

                        // First get all the outstanding encumbrances.
                        foreach (var transaction in encDataContract.EncPoEntityAssociation)
                        {
                            try
                            {
                                if (transaction != null)
                                {
                                    // Check transaction properties for missing data.
                                    // Some documents like recurring vouchers do not have a sequential ID, so do not check for missing ID.
                                    // Throw an exception if any of the transaction properties are null.
                                    string errorPrefix = "ENC." + fiscalYear + " " + encDataContract.Recordkey + " data contract ";

                                    if (string.IsNullOrEmpty(transaction.EncPoNoAssocMember))
                                        throw new ApplicationException(errorPrefix + "for transaction " + transaction.EncPoNoAssocMember + " has no document number.");

                                    if (!transaction.EncPoDateAssocMember.HasValue)
                                        throw new ApplicationException(errorPrefix + "for transaction " + transaction.EncPoNoAssocMember + " has no transaction date.");

                                    // While GLA records have a sequential ID in the key, the ENC record does not.
                                    // Assign an id similar to the GLA records using the counter.
                                    encCounter += 1;
                                    string transactionId = encDataContract.Recordkey + "*ENC*" + encCounter;

                                    var glTransactionEntity = new GlTransaction(transactionId, GlTransactionType.Encumbrance, "EP",
                                        encDataContract.Recordkey, transaction.EncPoAmtAssocMember ?? 0, transaction.EncPoNoAssocMember,
                                        transaction.EncPoDateAssocMember.Value, transaction.EncPoVendorAssocMember);

                                    // If the encumbrance entry has an ID, assign it as the document ID.
                                    if (!string.IsNullOrEmpty(transaction.EncPoIdAssocMember))
                                    {
                                        glTransactionEntity.DocumentId = transaction.EncPoIdAssocMember;
                                    }

                                    // Add the transaction to the list.
                                    glTransactionEntities.Add(glTransactionEntity);
                                }
                            }
                            catch (ApplicationException aex)
                            {
                                LogDataError("", "", transaction, aex);
                            }
                        }

                        // Then get all the outstanding requisitions.
                        foreach (var transaction in encDataContract.EncReqEntityAssociation)
                        {
                            try
                            {
                                if (transaction != null)
                                {
                                    // Check transaction properties for missing data.
                                    // Throw an exception if any of the transaction properties are null.
                                    string errorPrefix = "ENC." + fiscalYear + " " + encDataContract.Recordkey + " data contract ";

                                    if (string.IsNullOrEmpty(transaction.EncReqNoAssocMember))
                                        throw new ApplicationException(errorPrefix + "for transaction " + transaction.EncReqNoAssocMember + " has no requisition number.");

                                    if (!transaction.EncReqDateAssocMember.HasValue)
                                        throw new ApplicationException(errorPrefix + "for transaction " + transaction.EncReqNoAssocMember + " has no transaction date.");

                                    // While GLA records have a sequential ID in the key, the ENC record does not.
                                    // Assign an id similar to the GLA records using the counter.
                                    encCounter += 1;
                                    string transactionId = encDataContract.Recordkey + "*ENC*" + encCounter;

                                    var glTransactionEntity = new GlTransaction(transactionId, GlTransactionType.Requisition, "ER",
                                        encDataContract.Recordkey, transaction.EncReqAmtAssocMember ?? 0, transaction.EncReqNoAssocMember,
                                        transaction.EncReqDateAssocMember.Value, transaction.EncReqVendorAssocMember);

                                    // If the requisition entry has an ID, assign it as the document ID.
                                    if (!string.IsNullOrEmpty(transaction.EncReqIdAssocMember))
                                    {
                                        glTransactionEntity.DocumentId = transaction.EncReqIdAssocMember;
                                    }

                                    // Add the transaction to the list.
                                    glTransactionEntities.Add(glTransactionEntity);
                                }
                            }
                            catch (ApplicationException aex)
                            {
                                LogDataError("", "", transaction, aex);
                            }
                        }
                    }
                }
                #endregion

                // Get the budget pools for the fiscal year
                List<GlpFyr> glpFyrDataContracts = new List<GlpFyr>();
                List<string> umbrellaAccts = new List<string>();
                List<string> pooleeAccts = new List<string>();

                string glpFyrFilename = "GLP." + fiscalYear;
                var glpFyrIds = await DataReader.SelectAsync(glpFyrFilename, null);

                if (glpFyrIds != null && glpFyrIds.Any())
                    glpFyrDataContracts = await BulkReadRecordAsync<GlpFyr>(glpFyrFilename, glpFyrIds);
                if (glpFyrDataContracts != null)
                {
                    umbrellaAccts = glpFyrDataContracts.Select(x => x.Recordkey).ToList();
                    pooleeAccts = glpFyrDataContracts.SelectMany(x => x.GlpPooleeAcctsList).ToList();
                }

                #region Prerequisites for calculating Estimated Opening Balance 

                // Not included in loop as it is one time query
                // Read the GEN.LDGR record to see if the fiscal year is open or not.
                List<string> glsFyrGlANumber = new List<string>();
                List<string> glspreviousFyrGlANumber = new List<string>();
                var fiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear.ToString());

                // Calculate the previous fiscal year.
                int previousFiscalYear = 0;
                try
                {
                    previousFiscalYear = Convert.ToInt32(fiscalYear) - 1;
                }
                catch (Exception)
                {
                    logger.Warn(string.Format("Error converting fiscal year {0} from string to int.", fiscalYear));
                }

                var previousFiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(previousFiscalYear.ToString());

                #endregion

                foreach (var glString in glAccounts)
                {
                    var glAccountDomain = new FinanceQueryActivityDetail(glString);

                    if (umbrellaAccts.Contains(glString))
                    {
                        glAccountDomain.BudgetPoolIndicator = "Umbrella";
                    }
                    else if (pooleeAccts.Contains(glString))
                    {
                        glAccountDomain.BudgetPoolIndicator = "Poolee";
                    }

                    // Assign the transactions to the GL account.
                    foreach (var transaction in glTransactionEntities)
                    {
                        if (transaction.GlAccount == glString)
                        {
                            glAccountDomain.AddGlTransaction(transaction);
                        }
                    }

                    #region Get GL accounts for Estimated Opening Balance/Closing Year Amount

                    GlClass glClass = GlClass.Asset;
                    try
                    {
                        // Obtain the GL class for the GL account.
                        glClass = GlAccountUtility.GetGlAccountGlClass(glAccountDomain.GlAccountNumber, glClassConfiguration, majorComponentStartPosition);
                    }
                    catch (ApplicationException aex)
                    {
                        logger.Warn(aex.Message);
                    }

                    // Revenue and expense do not have opening balance/closing amount so we only need 
                    // to do the following if the GL account is an asset, liability or fund balance type.
                    if (glClass == GlClass.Asset || glClass == GlClass.Liability || glClass == GlClass.FundBalance)
                    {

                        // Read the GEN.LDGR record to see if the fiscal year is open or not.
                        if (fiscalYearGenLdgrContract != null && !string.IsNullOrEmpty(fiscalYearGenLdgrContract.GenLdgrStatus))
                        {

                            // If the fiscal year is open, determine the status of the previous fiscal year.
                            if (fiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                            {

                                if (previousFiscalYear != 0)
                                {
                                    // Read the previous fiscal year record to check if the status is open or not.

                                    if (previousFiscalYearGenLdgrContract != null && !string.IsNullOrEmpty(previousFiscalYearGenLdgrContract.GenLdgrStatus))
                                    {
                                        // If the previous fiscal year is open, calculate estimated opening balances
                                        // for asset, liability and fund balance accounts.
                                        // If the previous fiscal year is not open, do nothing.
                                        if (previousFiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                                        {
                                            if (glClass == GlClass.FundBalance)
                                            {
                                                // For Fund Balance accounts only, the estimated opening balance is the one stored in the GLS.fiscalyear record.
                                                glsFyrGlANumber.Add(glAccountDomain.GlAccountNumber);
                                            }

                                            else if (glClass == GlClass.Asset || glClass == GlClass.Liability)
                                            {
                                                // For Asset and Liability accounts only, when the previous fiscal year is open,
                                                // add the open balance and monthly debits and subtract monthly credits from the GLS.previousyear record.
                                                glspreviousFyrGlANumber.Add(glAccountDomain.GlAccountNumber);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn("GEN.LDGR record or status is invalid for previous fiscal year " + previousFiscalYear);
                                    }
                                }
                            }

                        }
                        else
                        {
                            logger.Warn("Unable to determine fiscal year status from GEN.LDGR for fiscal year " + fiscalYear);
                        }
                    }
                    #endregion

                    // Remove those encumbrance or Requisition transactions that have a $0.00 amount. 
                    // There shouldn't be any but just in case, we do not want to display $0.00.
                    glAccountDomain.RemoveZeroDollarEncumbranceTransactions();

                    // Add the finance query activity detail entity to the list of finance query activity 
                    // domain entities, if the entity has any transactions.
                    // As we need to add all the EOB into considerations, we are considering all the glAccountDomain
                    //if (glAccountDomain.Transactions.Any() )
                    //{
                        glActivities.Add(glAccountDomain);
                    //}

                }

                #region Apply Estimated Opening Balance 

                List<GlsFyr> glAccountGlsContract = new List<GlsFyr>();
                List<GlsFyr> glAccountpreviousGlsContract = new List<GlsFyr>();

                var glsFyrYearId = "GLS." + fiscalYear.ToString();
                glAccountGlsContract = await BulkReadRecordAsync<GlsFyr>(glsFyrYearId, glsFyrGlANumber.ToArray());

                var previousGlsFyrYearId = "GLS." + previousFiscalYear.ToString();
                glAccountpreviousGlsContract = await BulkReadRecordAsync<GlsFyr>(previousGlsFyrYearId, glspreviousFyrGlANumber.ToArray());

                foreach (var glAccts in glActivities)
                {


                    GlClass glClass = GlClass.Asset;
                    try
                    {
                        // Obtain the GL class for the GL account.
                        glClass = GlAccountUtility.GetGlAccountGlClass(glAccts.GlAccountNumber, glClassConfiguration, majorComponentStartPosition);
                    }
                    catch (ApplicationException aex)
                    {
                        logger.Warn(aex.Message);
                    }

                    // Revenue and expense do not have opening balance/closing amount so we only need 
                    // to do the following if the GL account is an asset, liability or fund balance type.
                    if (glClass == GlClass.Asset || glClass == GlClass.Liability || glClass == GlClass.FundBalance)
                    {

                        // Read the GEN.LDGR record to see if the fiscal year is open or not.
                        if (fiscalYearGenLdgrContract != null && !string.IsNullOrEmpty(fiscalYearGenLdgrContract.GenLdgrStatus))
                        {
                            // If the fiscal year is open, determine the status of the previous fiscal year.
                            if (fiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                            {

                                if (previousFiscalYear != 0)
                                {
                                    // Read the previous fiscal year record to check if the status is open or not.

                                    if (previousFiscalYearGenLdgrContract != null && !string.IsNullOrEmpty(previousFiscalYearGenLdgrContract.GenLdgrStatus))
                                    {
                                        // If the previous fiscal year is open, calculate estimated opening balances
                                        // for asset, liability and fund balance accounts.
                                        // If the previous fiscal year is not open, do nothing.
                                        if (previousFiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                                        {
                                            if (glClass == GlClass.FundBalance)
                                            {
                                                // For Fund Balance accounts only, the estimated opening balance is the one stored in the GLS.fiscalyear record.
                                                if (glAccountGlsContract.Exists(x => x.Recordkey == glAccts.GlAccountNumber))
                                                {
                                                    decimal? bal = glAccountGlsContract.Find(x => x.Recordkey == glAccts.GlAccountNumber).GlsEstimatedOpenBal;
                                                    glAccts.EstimatedOpeningBalance = (decimal)(bal.HasValue ? bal : 0m);
                                                }

                                            }

                                            else if (glClass == GlClass.Asset || glClass == GlClass.Liability)
                                            {
                                                // For Asset and Liability accounts only, when the previous fiscal year is open,
                                                // add the open balance and monthly debits and subtract monthly credits from the GLS.previousyear record.

                                                if (glAccountpreviousGlsContract.Exists(x => x.Recordkey == glAccts.GlAccountNumber))
                                                {
                                                    var prevglsContract = glAccountpreviousGlsContract.Find(x => x.Recordkey == glAccts.GlAccountNumber);
                                                    decimal? bal = prevglsContract.OpenBal;
                                                    glAccts.EstimatedOpeningBalance = (decimal)(bal.HasValue ? bal : 0m);

                                                    //glAccountDomain.EstimatedOpeningBalance = glAccountpreviousGlsContract.OpenBal.HasValue ? glAccountpreviousGlsContract.OpenBal.Value : 0m;
                                                    var monthlyDebits = prevglsContract.Mdebits;
                                                    if (monthlyDebits != null)
                                                    {
                                                        foreach (var debitAmt in monthlyDebits)
                                                        {
                                                            glAccts.EstimatedOpeningBalance += debitAmt.HasValue ? debitAmt.Value : 0m;
                                                        }
                                                    }
                                                    var monthlyCredits = prevglsContract.Mcredits;
                                                    if (monthlyCredits != null)
                                                    {
                                                        foreach (var creditAmt in monthlyCredits)
                                                        {
                                                            glAccts.EstimatedOpeningBalance -= creditAmt.HasValue ? creditAmt.Value : 0m;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Warn("GEN.LDGR record or status is invalid for previous fiscal year " + previousFiscalYear);
                                    }
                                }
                            }

                        }
                        else
                        {
                            logger.Warn("Unable to determine fiscal year status from GEN.LDGR for fiscal year " + fiscalYear);
                        }
                    }
                }
                #endregion

                glActivities.RemoveAll(x => x.EstimatedOpeningBalance == 0 && !(x.Transactions != null && x.Transactions.Any()) );
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the message and throw the exception
                logger.Error(ex.Message);
            }

            return glActivities;
        }

        /// <summary>
        /// Get the GL Source Codes from Colleague.
        /// </summary>
        /// <returns>ApplValcodes association of GL Source Codes data.</returns>
        private async Task<ApplValcodes> GetGlSourceCodesAsync()
        {
            var GlSourceCodesValidationTable = new ApplValcodes();
            try
            {
                // Verify that it is populated. If not, throw an error.
                GlSourceCodesValidationTable = await GetOrAddToCacheAsync<ApplValcodes>("GlSourceCodes",
                    async () =>
                    {
                        ApplValcodes GlSourceCodesValTable = await DataReader.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "GL.SOURCE.CODES");
                        if (GlSourceCodesValTable == null)
                            throw new Exception("GL.SOURCE.CODES validation table data is null.");

                        return GlSourceCodesValTable;
                    }, Level1CacheTimeoutValue);

                return GlSourceCodesValidationTable;
            }
            catch (Exception ex)
            {
                LogDataError("CF.VALCODES", "GL.SOURCE.CODES", GlSourceCodesValidationTable, ex);
                throw new Exception("Unable to retrieve GL.SOURCE.CODES validation table from Colleague.");
            }
        }

        #region section for filtering data
        private async Task<string[]> ApplyFilterCriteria(FinanceQueryCriteria criteria, string[] allGlAccountsForUser)
        {
            var filteredUserGlAccounts = allGlAccountsForUser;

            if (filteredUserGlAccounts.Any() && criteria.ComponentCriteria != null && criteria.ComponentCriteria.Any())
            {
                var filteredGlAccounts = filteredUserGlAccounts;
                foreach (var filterComp in criteria.ComponentCriteria)
                {
                    // Set a running limiting list of GL accounts that are
                    // filtered using the filtering criteria for each component.
                    // And, reset the filter criteria strings to null.
                    if (filteredGlAccounts.Any())
                    {
                        var tempFilteredAccounts = filteredGlAccounts;
                        var componentFilterCriteria = BuildFilterCriteria(filterComp);
                        filteredGlAccounts = await DataReader.SelectAsync("GL.ACCTS", tempFilteredAccounts, componentFilterCriteria);
                    }
                }
                filteredUserGlAccounts = filteredGlAccounts;
            }

            if (filteredUserGlAccounts.Any() && criteria.ProjectReferenceNos != null && criteria.ProjectReferenceNos.Any())
            {
                var filteredGlAccounts = filteredUserGlAccounts;
                //append each project ref no in uppercase and quotes
                var projectFilterCriteria = string.Join(" ", criteria.ProjectReferenceNos.Select(x => string.Format("'{0}'", x.ToUpper())));
                projectFilterCriteria = "GL.PRJ.REF.NO EQ " + projectFilterCriteria;
                filteredUserGlAccounts = await DataReader.SelectAsync("GL.ACCTS", filteredGlAccounts, projectFilterCriteria);
            }
            return filteredUserGlAccounts;
        }
        private async Task<List<FinanceQueryGlAccountLineItem>> RemoveInActiveAccountsWithNoActivity(FinanceQueryCriteria criteria, List<FinanceQueryGlAccountLineItem> filteredFinanceQueryGlAccounts, string fiscalYear)
        {
            var glAccountsToBeRemoved = new List<FinanceQueryGlAccountLineItem>();

            if (filteredFinanceQueryGlAccounts == null || !filteredFinanceQueryGlAccounts.Any()) return glAccountsToBeRemoved;

            // Get the non pooled GL account number strings from the FinanceQueryGlAccountLineItems where all of the amounts are zero.
            var possibleGlAccountsToRemove = filteredFinanceQueryGlAccounts.Select(x => x.GlAccount).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0 && a.RequisitionAmount == 0).ToList();

            // Get the poolee GL account number strings from all of the Umbrella lineitems (FinanceQueryGlAccountLineItems) 
            // where all of the amounts are zero.
            var possibleGlAccountPooleesToRemove = filteredFinanceQueryGlAccounts.SelectMany(x => x.Poolees).Where
                (a => a.ActualAmount == 0 && a.BudgetAmount == 0 && a.EncumbranceAmount == 0 && a.RequisitionAmount == 0).ToList();

            // Add the list of poolee accounts to the list of non pooled accounts.
            if (possibleGlAccountPooleesToRemove != null && possibleGlAccountPooleesToRemove.Any())
            {
                possibleGlAccountsToRemove.AddRange(possibleGlAccountPooleesToRemove);
            }

            // If there are any FinanceQueryGlAccountLineItem with all zero amounts, find out if there is any activity for 
            // any of these GL accounts by selecting GLS and ENC records. Also, find out if any of these GL accounts
            // are active by selecting GL.ACCTS that are not currently inactive. Remove these GL account record IDs
            // from the list of possible record IDs to remove.
            if (possibleGlAccountsToRemove != null && possibleGlAccountsToRemove.Any())
            {
                var glAccountsWithZeroBalanceArray = possibleGlAccountsToRemove.Select(x => x.GlAccountNumber).ToArray();
                var inactiveGlAccountsWithNoActivity = glAccountsWithZeroBalanceArray.ToList();

                string activeCriteria = "WITH GL.INACTIVE NE 'I'";
                var activeGlAcctsIds = await DataReader.SelectAsync("GL.ACCTS", glAccountsWithZeroBalanceArray, activeCriteria);
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
                if (criteria != null && !criteria.IncludeActiveAccountsWithNoActivity)
                {
                    glAccountsToRemove.AddRange(activeGlAccountsWithNoActivity);
                }

                // If there are GL account IDs where the account is inactive and has no activity, remove them from the 
                // list of non pooled accounts and from the list of poolee on each line item.
                if (glAccountsToRemove != null && glAccountsToRemove.Any())
                {
                    var glAccountsToBeDeleted = filteredFinanceQueryGlAccounts.Select(x => x.GlAccount).Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber)).ToList();
                    var pooleesGlAccountsToBeDeleted = filteredFinanceQueryGlAccounts.SelectMany(x => x.Poolees).Where(a => inactiveGlAccountsWithNoActivity.Contains(a.GlAccountNumber)).ToList();


                    if (glAccountsToBeDeleted.Any() || pooleesGlAccountsToBeDeleted.Any())
                    {
                        foreach (var glAccountStr in filteredFinanceQueryGlAccounts)
                        {
                            if (inactiveGlAccountsWithNoActivity.Contains(glAccountStr.GlAccountNumber))
                            {
                                glAccountsToBeRemoved.Add(glAccountStr);
                            }
                            if (pooleesGlAccountsToBeDeleted.Any() && glAccountStr.Poolees.Any() && glAccountStr.Poolees.Any(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber)))
                            {
                                glAccountStr.Poolees = glAccountStr.Poolees.Except(glAccountStr.Poolees.Where(z => inactiveGlAccountsWithNoActivity.Contains(z.GlAccountNumber))).ToList();
                            }
                        }
                    }
                }
            }

            return glAccountsToBeRemoved;
        }

        #endregion

        #region section for open year
        private async Task<List<FinanceQueryGlAccountLineItem>> ProcessAllGlAccountsForOpenYear(GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, string fiscalYear, string[] allGlAccountsForUser, string[] filteredUserGlAccounts, List<GlAccts> glAccountRecords)
        {
            List<FinanceQueryGlAccountLineItem> filteredFinanceQueryGlAccounts = new List<FinanceQueryGlAccountLineItem>();
            // Create three unique lists of data contracts for the given fiscal year: umbrellas, poolees, non-pooled
            var umbrellaAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                                                                     && x.MemosEntityAssociation.Where(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && y.GlPooledTypeAssocMember.ToUpper() == "U").Any()).ToList();
            var pooleeAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                                                                   && x.MemosEntityAssociation.Any(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && y.GlPooledTypeAssocMember.ToUpper() == "P")).ToList();
            var nonPooledAccounts = glAccountRecords.Where(x => x != null && x.MemosEntityAssociation != null
                                                                      && x.MemosEntityAssociation.Any(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && string.IsNullOrEmpty(y.GlPooledTypeAssocMember))).ToList();

            var budgetPoolGlAccountLineItems = await ProcessUmbrellaAndPooleesForOpenYear(fiscalYear, umbrellaAccounts, pooleeAccounts, filteredUserGlAccounts, allGlAccountsForUser);
            filteredFinanceQueryGlAccounts.AddRange(budgetPoolGlAccountLineItems);

            var nonPooledGlAccountLineItems = await ProcessNonPooledGlAccountsForOpenYear(fiscalYear, nonPooledAccounts, glAccountStructure, glClassConfiguration);
            filteredFinanceQueryGlAccounts.AddRange(nonPooledGlAccountLineItems);

            return filteredFinanceQueryGlAccounts;
        }
        private async Task<List<FinanceQueryGlAccountLineItem>> ProcessNonPooledGlAccountsForOpenYear(string fiscalYear, List<GlAccts> nonPooledAccounts, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            List<FinanceQueryGlAccountLineItem> nonPooledLineItems = new List<FinanceQueryGlAccountLineItem>();
            FinanceQueryGlAccountLineItemBuilder financeQueryGlAccountLineItemBuilder = new FinanceQueryGlAccountLineItemBuilder();

            // Calculate the previous fiscal year.
            int fiscalYearInt = 0;
            int previousFiscalYear = 0;
            bool parseFiscalYear = int.TryParse(fiscalYear, out fiscalYearInt);

            if (!parseFiscalYear)
            {
                logger.Warn(string.Format("Error converting fiscal year {0} from string to int.", fiscalYear));
            }
            previousFiscalYear = fiscalYearInt - 1;

            // If the previous fiscal year is open then read the GLS records for the gl accounts for the current FY .
            List<GlsFyr> glsContracts = null;
            List<GlsFyr> previousGlsContracts = null;

            if (parseFiscalYear && previousFiscalYear > 0)
            {
                var previousFiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(previousFiscalYear.ToString());
                if (previousFiscalYearGenLdgrContract != null && previousFiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                {
                    var nonPooledGlAccountNumbers = nonPooledAccounts.Select(x => x.Recordkey).ToArray();
                    var glsFyrYearId = "GLS." + fiscalYear.ToString();
                    glsContracts = await BulkReadRecordAsync<GlsFyr>(glsFyrYearId, nonPooledGlAccountNumbers.Where(x => GetGlAccountGlClass(x, glClassConfiguration, glAccountStructure) == GlClass.FundBalance).ToArray());

                    var previousGlsFyrYearId = "GLS." + previousFiscalYear.ToString();
                    previousGlsContracts = await BulkReadRecordAsync<GlsFyr>(previousGlsFyrYearId, nonPooledGlAccountNumbers.Where(x =>
                        GetGlAccountGlClass(x, glClassConfiguration, glAccountStructure) == GlClass.Asset || GetGlAccountGlClass(x, glClassConfiguration, glAccountStructure) == GlClass.Liability).ToArray());
                }
            }

            foreach (var nonPooledAccount in nonPooledAccounts)
            {
                var financeQueryGlAccount = BuildNonPooledAccountForOpenYear(nonPooledAccount, fiscalYear, glAccountStructure, glClassConfiguration, glsContracts, previousGlsContracts);
                // Add the GL account to the list of finance query Gl account.
                if (financeQueryGlAccount != null)
                    financeQueryGlAccountLineItemBuilder.AddNonPooledGlAccount(financeQueryGlAccount);
            }

            if (financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems != null && financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.Any())
                nonPooledLineItems = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.ToList();

            return nonPooledLineItems;

        }
        private async Task<List<FinanceQueryGlAccountLineItem>> ProcessUmbrellaAndPooleesForOpenYear(string fiscalYear, List<GlAccts> umbrellaAccounts, List<GlAccts> pooleeAccounts, string[] filteredUserGlAccounts, string[] userAllGlAccounts)
        {
            List<FinanceQueryGlAccountLineItem> budgetPoolLineItems = new List<FinanceQueryGlAccountLineItem>();
            FinanceQueryGlAccountLineItemBuilder financeQueryGlAccountLineItemBuilder = new FinanceQueryGlAccountLineItemBuilder();

            #region process all umbrella accounts

            foreach (var umbrella in umbrellaAccounts)
            {
                FinanceQueryGlAccount umbrellaAsPooleeGlAccount = null;
                FinanceQueryGlAccount umbrellaGlAccount = BuildUmbrellaAccountForOpenYear(umbrella, fiscalYear, out umbrellaAsPooleeGlAccount);
                // Create umbrella gl account line-item
                if (umbrellaGlAccount != null)
                    financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(umbrellaGlAccount, true);
                //Add poolee acount, if umbrella gl account has direct expenses charged to it.
                if (umbrellaAsPooleeGlAccount != null)
                    financeQueryGlAccountLineItemBuilder.AddPoolee(umbrella.Recordkey, umbrellaAsPooleeGlAccount);

                //Find poolees for this umbrella account.
                List<GlpFyr> glpFyrDataContracts = new List<GlpFyr>();
                string glpFyrFilename = "GLP." + fiscalYear;
                var glpFyrIds = await DataReader.SelectAsync(glpFyrFilename, null);

                if (glpFyrIds != null && glpFyrIds.Any())
                    glpFyrDataContracts = await BulkReadRecordAsync<GlpFyr>(glpFyrFilename, glpFyrIds);

                var umbrellaGlpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.Recordkey == umbrella.Recordkey);

                //if none of the poolee is part of filtered criteria, then add all poolee accounts for which user has access to
                if (umbrellaGlpFyrContract != null && umbrellaGlpFyrContract.GlpPooleeAcctsList.Any() && !filteredUserGlAccounts.Any(x => umbrellaGlpFyrContract.GlpPooleeAcctsList.Contains(x)))
                {
                    var glIdsWithAccess = userAllGlAccounts.Where(x => umbrellaGlpFyrContract.GlpPooleeAcctsList.Contains(x)).ToList();
                    if (glIdsWithAccess.Any())
                    {
                        var pooleeAcctsForThisUmbrella = await BulkReadRecordAsync<GlAccts>(glIdsWithAccess.ToArray());
                        pooleeAcctsForThisUmbrella = pooleeAcctsForThisUmbrella.Where(x => x != null && x.MemosEntityAssociation != null
                                                                                                     && x.MemosEntityAssociation.Any(y => y != null && y.AvailFundsControllerAssocMember == fiscalYear && y.GlPooledTypeAssocMember.ToUpper() == "P")).ToList();
                        if (pooleeAcctsForThisUmbrella != null && pooleeAcctsForThisUmbrella.Any())
                            pooleeAccounts.AddRange(pooleeAcctsForThisUmbrella);
                    }
                }
            }
            #endregion

            #region process all poolee acounts
            foreach (var poolee in pooleeAccounts)
            {
                if (poolee != null)
                {
                    // Figure out the umbrella for this poolee
                    var umbrellaGlAccount = "";
                    var glAccountAmountsUmbrella = poolee.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
                    if (glAccountAmountsUmbrella != null)
                    {
                        umbrellaGlAccount = glAccountAmountsUmbrella.GlBudgetLinkageAssocMember;
                    }

                    // If the GL account has information for the fiscal year passed in, process the amounts.
                    if (glAccountAmountsUmbrella != null)
                    {
                        // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                        var pooleeGlAccount = BuildGlAccountEntityWithAmounts(poolee.Recordkey, GlBudgetPoolType.Poolee, glAccountAmountsUmbrella);

                        // Add the poolee to the appropriate pool.
                        var selectedSummaryPool = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == umbrellaGlAccount);
                        if (selectedSummaryPool != null)
                        {
                            financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaGlAccount, pooleeGlAccount);
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

                            var umbrellaGlAccountAmounts = umbrellaAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

                            // If the GL account has information for the fiscal year passed in, process the amounts.
                            if (umbrellaGlAccountAmounts != null)
                            {
                                // create new gl account line item for umbrella.
                                var newUmbrellaGlAccount = BuildUmbrellaGlAccountEntityWithAmounts(umbrellaAccount.Recordkey, umbrellaGlAccountAmounts);
                                bool isUmbrellaVisible = userAllGlAccounts.Contains(umbrellaAccount.Recordkey);

                                // Create pool for umbrella.
                                financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(newUmbrellaGlAccount, isUmbrellaVisible);
                                financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaGlAccount, pooleeGlAccount);
                            }
                        }
                    }
                }
            }
            #endregion

            if (financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems != null && financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.Any())
                budgetPoolLineItems = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.ToList();

            return budgetPoolLineItems;
        }
        #endregion

        #region section for closed year
        private async Task<List<FinanceQueryGlAccountLineItem>> ProcessAllGlAccountsForClosedYear(List<GlsFyr> glsRecords, List<GlpFyr> glpFyrDataContracts,
            string glsFyrId, GeneralLedgerAccountStructure glAccountStructure,
            GeneralLedgerClassConfiguration glClassConfiguration, FinanceQueryCriteria criteria,
            string personId, string fiscalYear, string[] filteredUserGlAccounts,
            string[] userAllGlAccounts, CfwebDefaults cfWebDefaults)
        {
            List<FinanceQueryGlAccountLineItem> allGlAccountLineItems = new List<FinanceQueryGlAccountLineItem>();
            FinanceQueryGlAccountLineItemBuilder financeQueryGlAccountLineItemBuilder = new FinanceQueryGlAccountLineItemBuilder();

            List<string> pooleesForEncFyr = new List<string>();

            #region Get closed year amounts from GLS.FYR
            if (glsRecords != null)
            {
                // Create three unique lists of data contracts for the given fiscal year: umbrellas, poolees, non-pooled
                var umbrellaAccounts = glsRecords.Where(x => x.GlsPooledType.ToUpper() == "U").ToList();
                var pooleeAccounts = glsRecords.Where(x => x.GlsPooledType.ToUpper() == "P").ToList();
                var nonPooledAccounts = glsRecords.Where(x => string.IsNullOrEmpty(x.GlsPooledType)).ToList();

                #region Process all umbrella accounts
                foreach (var umbrella in umbrellaAccounts)
                {
                    if (umbrella != null)
                    {

                        // Create GL account for umbrella and populate budget, actual and encumbrance amounts.
                        var umbrellaGlAccount = BuildGlAccountEntityWithAmountsForClosedYears(umbrella, GlBudgetPoolType.Umbrella, true, glClassConfiguration, glAccountStructure, cfWebDefaults);

                        financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(umbrellaGlAccount, true);

                        // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                        if ((umbrella.CreditsYtd.HasValue && umbrella.CreditsYtd.Value != 0)
                            || (umbrella.DebitsYtd.HasValue && umbrella.DebitsYtd.Value != 0)
                            || (umbrella.OpenBal.HasValue && umbrella.OpenBal.Value != 0)
                            || (umbrella.EOpenBal.HasValue && umbrella.EOpenBal.Value != 0)
                            || (umbrella.EncumbrancesYtd.HasValue && umbrella.EncumbrancesYtd.Value != 0)
                            || (umbrella.EncumbrancesRelievedYtd.HasValue && umbrella.EncumbrancesRelievedYtd.Value != 0))
                        {
                            var umbrellaPoolee = BuildGlAccountEntityWithAmountsForClosedYears(umbrella, GlBudgetPoolType.Poolee, false, glClassConfiguration, glAccountStructure, cfWebDefaults);
                            financeQueryGlAccountLineItemBuilder.AddPoolee(umbrella.Recordkey, umbrellaPoolee);
                        }


                        var umbrellaGlpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.Recordkey == umbrella.Recordkey);
                        //if none of the poolee is part of filtered criteria, then add all poolee accounts for which user has access to
                        if (umbrellaGlpFyrContract != null && umbrellaGlpFyrContract.GlpPooleeAcctsList.Any() && !filteredUserGlAccounts.Any(x => umbrellaGlpFyrContract.GlpPooleeAcctsList.Contains(x)))
                        {
                            var glIdsWithAccess = userAllGlAccounts.Where(x => umbrellaGlpFyrContract.GlpPooleeAcctsList.Contains(x)).ToArray();
                            if (glIdsWithAccess.Any())
                            {
                                var pooleeAcctsForThisUmbrella = await BulkReadRecordAsync<GlsFyr>(glsFyrId, glIdsWithAccess.ToArray());
                                pooleeAcctsForThisUmbrella = pooleeAcctsForThisUmbrella.Where(x => x.GlsPooledType.ToUpper() == "P").ToList();
                                if (pooleeAcctsForThisUmbrella != null && pooleeAcctsForThisUmbrella.Any())
                                {
                                    pooleeAccounts.AddRange(pooleeAcctsForThisUmbrella);
                                    pooleesForEncFyr.AddRange(pooleeAcctsForThisUmbrella.Select(x => x.Recordkey));
                                }
                            }
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
                            var umbrellaGlAccount = poolee.GlsBudgetLinkage;

                            // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                            var pooleeGlAccount = BuildGlAccountEntityWithAmountsForClosedYears(poolee, GlBudgetPoolType.Poolee, true, glClassConfiguration, glAccountStructure, cfWebDefaults);

                            // Add the poolee to the appropriate pool.
                            var selectedSummaryPool = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == umbrellaGlAccount);
                            if (selectedSummaryPool != null)
                            {
                                financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaGlAccount, pooleeGlAccount);
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

                                // Create the GL account for the umbrella.
                                var newUmbrellaGlAccount = BuildGlAccountEntityWithAmountsForClosedYears(umbrellaAccount, GlBudgetPoolType.Umbrella, true, glClassConfiguration, glAccountStructure, cfWebDefaults);

                                // flag to check if user has access to umbrella.
                                bool isUmbrellaVisible = userAllGlAccounts.Contains(umbrellaAccount.Recordkey);

                                // Create pool for umbrella.
                                financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(newUmbrellaGlAccount, isUmbrellaVisible);
                                financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaGlAccount, pooleeGlAccount);
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
                    // Create GL account for poolee and populate budget, actual and encumbrance amounts.
                    var nonPooledGlAccount = BuildGlAccountEntityWithAmountsForClosedYears(nonPooledAccount, GlBudgetPoolType.None, true, glClassConfiguration, glAccountStructure, cfWebDefaults);
                    // Add the GL account to the list of finance query Gl account.
                    financeQueryGlAccountLineItemBuilder.AddNonPooledGlAccount(nonPooledGlAccount);

                }
                #endregion
            }
            #endregion

            #region Get closed year requisition amounts from ENC.FYR

            var updatedGlAccountsForEncFyr = pooleesForEncFyr != null ? pooleesForEncFyr.ToArray().Concat(filteredUserGlAccounts).ToArray() : filteredUserGlAccounts;
            // Now obtain the requisition amounts from ENC.FYR because they are not stored in GLS.FYR.

            // Bulk read the ENC.FYR records using the list of GL accounts that the user has access to,
            //all accounts that they have access to.
            var encFyrId = "ENC." + fiscalYear;
            List<EncFyr> encRecords = new List<EncFyr>();
            if (IsFilterWideOpen(criteria))
            {
                encRecords = await GetOrAddToCacheAsync("FinanceQueryEncFyr" + personId + criteria.FiscalYear, async () =>
                {
                    return await BulkReadRecordAsync<EncFyr>(encFyrId, filteredUserGlAccounts);
                }, ThirtyMinuteCacheTimeout);
            }
            else
            {
                encRecords = await BulkReadRecordAsync<EncFyr>(encFyrId, updatedGlAccountsForEncFyr);
            }

            // If we do not have any ENC.FYR records, return the list of gl account line items that has been obtained already.
            if (encRecords != null && encRecords.Any())
            {
                // Create a list of GL Account domain entities for each ENC.FYR record that has a requisition amount.
                var requisitionGlAccountsList = BuildRequisitionsGlAccountsForClosedYear(glsRecords, glpFyrDataContracts, encRecords);

                // requisitionGlAccountsList contains a list of finance query gl account domain entities, each containing a requisition amount.
                // We need to add those requisition amounts to the appropriate GL account domain entities such that:
                // - if the GL account belongs to an existing GL line item:
                //      = if the GL account is already in the list of gl account line items, add the requisition amount to its encumbrances.
                //      = if the GL account is not already in the list of gl account line items, add the domain entity to line item list.

                if (requisitionGlAccountsList != null && requisitionGlAccountsList.Any())
                {
                    // Loop through each GL account domain with a requisition amount.
                    foreach (var reqGlAcct in requisitionGlAccountsList)
                    {
                        bool glAccountFound = false;

                        // Loop through each GL account line item.
                        foreach (var acct in financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems)
                        {
                            if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                            {

                                // If the requisition GL account is already included in the list of GL account line items,
                                // add the requisition amount to its encumbrances.
                                if (reqGlAcct.GlAccountNumber == acct.GlAccountNumber)
                                {
                                    acct.GlAccount.RequisitionAmount += reqGlAcct.RequisitionAmount;
                                    glAccountFound = true;
                                }

                            }
                            else if (reqGlAcct.PoolType == GlBudgetPoolType.Poolee || reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                {
                                    if (reqGlAcct.GlAccountNumber == acct.GlAccountNumber)
                                    {
                                        acct.GlAccount.RequisitionAmount += reqGlAcct.RequisitionAmount;

                                        var umbrellaPoolee = acct.Poolees.FirstOrDefault(x => x.GlAccountNumber == reqGlAcct.GlAccountNumber);
                                        if (umbrellaPoolee != null)
                                            umbrellaPoolee.RequisitionAmount += reqGlAcct.RequisitionAmount;
                                        else
                                        {
                                            // Only add the umbrella as a poolee if the user has access to it.
                                            if (filteredUserGlAccounts.Contains(reqGlAcct.GlAccountNumber) && reqGlAcct.RequisitionAmount != 0)
                                            {
                                                // Create a poolee to represent the umbrella and add the encumbrance amount, then add it to the poolees list.
                                                var poolee = new FinanceQueryGlAccount(reqGlAcct.GlAccountNumber, GlBudgetPoolType.Poolee);
                                                poolee.RequisitionAmount = reqGlAcct.RequisitionAmount;
                                                financeQueryGlAccountLineItemBuilder.AddPoolee(acct.GlAccountNumber, poolee);

                                            }
                                        }

                                        glAccountFound = true;
                                    }
                                }
                                else
                                {
                                    var selectedPoolee = acct.Poolees.FirstOrDefault(x => x.GlAccountNumber == reqGlAcct.GlAccountNumber);
                                    if (selectedPoolee != null)
                                    {
                                        selectedPoolee.RequisitionAmount += reqGlAcct.RequisitionAmount;
                                        // Also add the poolee requisition amount to the umbrella requisition amount.
                                        acct.GlAccount.RequisitionAmount += reqGlAcct.RequisitionAmount;
                                        glAccountFound = true;
                                    }
                                }

                            }
                        }

                        // If the GL account is not included in gl account line items , then add the gl account, 
                        // whether they already exist, or we have to create new ones.
                        // This can be a case where a poolee only has an ENC record and no posted activity in any GLS record,
                        // for instance, the poolee only has requisition amounts.
                        if (!glAccountFound)
                        {
                            if (reqGlAcct.PoolType == GlBudgetPoolType.None)
                            {
                                financeQueryGlAccountLineItemBuilder.AddNonPooledGlAccount(reqGlAcct);
                            }
                            else
                            {
                                if (reqGlAcct.PoolType == GlBudgetPoolType.Umbrella)
                                {
                                    bool isUmbrellaVisible = userAllGlAccounts.Contains(reqGlAcct.GlAccountNumber);
                                    financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(reqGlAcct, isUmbrellaVisible);
                                }
                                else
                                {
                                    // Find the umbrella for this poolee from GLP.FYR.
                                    bool poolFound = true;
                                    var umbrellaForThisPooleeGlpFyrRecord = glpFyrDataContracts.FirstOrDefault(x => x.GlpPooleeAcctsList.Contains(reqGlAcct.GlAccountNumber));
                                    if (umbrellaForThisPooleeGlpFyrRecord != null)
                                    {
                                        var umbrellaForThisPoolee = umbrellaForThisPooleeGlpFyrRecord.Recordkey;
                                        // Find if the umbrella for this poolee already has a pool created in any finance query summary,
                                        // or we have to create a new umbrella & poolee for it.

                                        var glAccountForThisUmbrella = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.FirstOrDefault(x => x.GlAccountNumber == umbrellaForThisPoolee);
                                        if (glAccountForThisUmbrella != null)
                                        {
                                            // There is already a GL account line item for this umbrella.
                                            // Get it and add the poolee to it.

                                            if (glAccountForThisUmbrella.Poolees.Any())
                                            {
                                                // grab the poolee acount from umbrella and add requisitions amount.
                                                var pooleeAccount = glAccountForThisUmbrella.Poolees.FirstOrDefault(x => x.GlAccountNumber == umbrellaForThisPoolee);
                                                if (pooleeAccount != null)
                                                {
                                                    // There is a pool for this poolee.
                                                    // Add the poolee to it.
                                                    pooleeAccount.RequisitionAmount += reqGlAcct.RequisitionAmount;
                                                    financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaForThisPoolee, pooleeAccount);
                                                }
                                                else
                                                {
                                                    // There is no umbrella for this poolee.
                                                    //Create a umbrella account for this poolee and add it list of GL account line item.
                                                    poolFound = false;
                                                }
                                            }
                                            else
                                            {
                                                // There is no umbrella for this poolee.
                                                //Create a umbrella account for this poolee and add it list of GL account line item.
                                                poolFound = false;
                                            }
                                        }
                                        else
                                        {
                                            // There is no umbrella for this poolee.
                                            //Create a umbrella account for this poolee and add it list of GL account line item.
                                            poolFound = false;
                                        }
                                    }
                                    else
                                    {
                                        // There is data problem. This is a poolee but it was not found in GLP.FYR
                                        // Log it and ignore this record.
                                        LogDataError("This GL account " + reqGlAcct.GlAccountNumber + " is a poolee but was not found in any GLP." + fiscalYear + " pools.", "", reqGlAcct);
                                    }

                                    if (!poolFound)
                                    {
                                        // add the umbrella gl account to it.
                                        var umbrellaGlAccount = new FinanceQueryGlAccount(umbrellaForThisPooleeGlpFyrRecord.Recordkey, GlBudgetPoolType.Umbrella);

                                        bool isUmbrellaVisible = userAllGlAccounts.Contains(umbrellaForThisPooleeGlpFyrRecord.Recordkey);

                                        // Also add the poolee requisition amount to the umbrella requisition amount.
                                        umbrellaGlAccount.RequisitionAmount += reqGlAcct.RequisitionAmount;

                                        financeQueryGlAccountLineItemBuilder.AddBudgetPoolGlAccount(umbrellaGlAccount, isUmbrellaVisible);
                                        financeQueryGlAccountLineItemBuilder.AddPoolee(umbrellaForThisPooleeGlpFyrRecord.Recordkey, reqGlAcct);

                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            if (financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems != null && financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.Any())
                allGlAccountLineItems = financeQueryGlAccountLineItemBuilder.FinanceQueryGlAccountLineItems.ToList();

            return allGlAccountLineItems;
        }

        #endregion

        #region Domain entities builder
        private List<FinanceQueryGlAccount> BuildRequisitionsGlAccountsForClosedYear(List<GlsFyr> glsRecords, List<GlpFyr> glpFyrDataContracts, List<EncFyr> encRecords)
        {
            List<FinanceQueryGlAccount> requisitionGlAccountsList = new List<FinanceQueryGlAccount>();
            foreach (var reqGlAccount in encRecords)
            {
                // Determine if the GL account is a regular GL account or part of a budget pool.
                // Default the budget pool type to not part of a pool.
                GlBudgetPoolType poolType = GlBudgetPoolType.None;

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
                                }
                                glNumberFoundInGls = true;
                            }
                        }

                        // If this GL account only has an ENC.FYR record and not GLS.FYR, then we have 
                        // to find its umbrella GL account from GL.ACCTS or from GLP.FYR.
                        if (!glNumberFoundInGls)
                        {
                            // Is this GL account an umbrella or poolee?
                            var umbrellaGlpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.Recordkey == reqGlAccount.Recordkey);
                            if (umbrellaGlpFyrContract != null)
                            {
                                poolType = GlBudgetPoolType.Umbrella;
                            }
                            else
                            {
                                var pooleeGlpFyrContract = glpFyrDataContracts.FirstOrDefault(x => x.GlpPooleeAcctsList.Contains(reqGlAccount.Recordkey));
                                if (pooleeGlpFyrContract != null)
                                {
                                    poolType = GlBudgetPoolType.Poolee;
                                }
                            }
                        }


                        var requisitionGlAccountDomain = new FinanceQueryGlAccount(reqGlAccount.Recordkey, poolType);

                        // Obtain the total requisition amount for this GL account.
                        foreach (var amount in reqGlAccount.EncReqAmt)
                            requisitionGlAccountDomain.RequisitionAmount += amount.HasValue ? amount.Value : 0m;

                        requisitionGlAccountsList.Add(requisitionGlAccountDomain);

                    }
                }
            }

            return requisitionGlAccountsList;
        }

        private FinanceQueryGlAccount BuildUmbrellaAccountForOpenYear(GlAccts umbrella, string fiscalYear, out FinanceQueryGlAccount umbrellaAsPooleeGlAccount)
        {
            FinanceQueryGlAccount umbrellaGlAccount = null;
            umbrellaAsPooleeGlAccount = null;
            if (umbrella != null)
            {
                var glAccountAmounts = umbrella.MemosEntityAssociation.Where(x => x.AvailFundsControllerAssocMember == fiscalYear).FirstOrDefault();

                // If the GL account has information for the fiscal year passed in, process the amounts.
                if (glAccountAmounts != null)
                {
                    // Create GL account for umbrella and populate budget, actual and encumbrance amounts.
                    umbrellaGlAccount = BuildUmbrellaGlAccountEntityWithAmounts(umbrella.Recordkey, glAccountAmounts);

                    // If the umbrella has direct expenses charged to it, create a poolee for those amounts.
                    if ((glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue && glAccountAmounts.GlEncumbrancePostedAssocMember.Value != 0)
                        || (glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue && glAccountAmounts.GlEncumbranceMemosAssocMember.Value != 0)
                        || (glAccountAmounts.GlRequisitionMemosAssocMember.HasValue && glAccountAmounts.GlRequisitionMemosAssocMember.Value != 0)
                        || (glAccountAmounts.GlActualPostedAssocMember.HasValue && glAccountAmounts.GlActualPostedAssocMember.Value != 0)
                        || (glAccountAmounts.GlActualMemosAssocMember.HasValue && glAccountAmounts.GlActualMemosAssocMember.Value != 0))
                    {
                        umbrellaAsPooleeGlAccount = BuildGlAccountEntityWithAmounts(umbrella.Recordkey, GlBudgetPoolType.Poolee, glAccountAmounts, false);
                    }

                }
            }

            return umbrellaGlAccount;

        }

        private FinanceQueryGlAccount BuildNonPooledAccountForOpenYear(GlAccts nonPooledAccount, string fiscalYear, GeneralLedgerAccountStructure glAccountStructure, GeneralLedgerClassConfiguration glClassConfiguration, List<GlsFyr> glsContracts, List<GlsFyr> previousGlsContracts)
        {
            FinanceQueryGlAccount financeQueryGlAccount = null;
            // Determine the GL class for the GL account.
            var glClass = GetGlAccountGlClass(nonPooledAccount.Recordkey, glClassConfiguration, glAccountStructure);

            var glAccountAmounts = nonPooledAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

            // If the GL account has information for the fiscal year passed in, process the amounts.
            if (glAccountAmounts != null)
            {
                // Create GL account for poolee and populate budget, actual and encumbrance amounts.                    
                financeQueryGlAccount = BuildGlAccountEntityWithAmounts(nonPooledAccount.Recordkey, GlBudgetPoolType.None, glAccountAmounts);

                // For Fund Balance accounts only, add the estimated opening balance if the previous fiscal year is also open.
                if (glClass == GlClass.FundBalance && glsContracts != null)
                {
                    var nonPooledGlsContract = glsContracts.FirstOrDefault(x => x.Recordkey == financeQueryGlAccount.GlAccountNumber);
                    if (nonPooledGlsContract != null)
                    {
                        financeQueryGlAccount.ActualAmount += nonPooledGlsContract.GlsEstimatedOpenBal.HasValue ? nonPooledGlsContract.GlsEstimatedOpenBal.Value : 0m;
                    }
                }

                // For Asset or Liability accounts, add the estimated opening balance if the previous fiscal year is also open. Add the open balance, monthly
                // debits, and subtract the monthly credits from the previous year GLS record.
                if ((glClass == GlClass.Asset || glClass == GlClass.Liability) && glsContracts != null)
                {
                    var previousGlsContract = previousGlsContracts.FirstOrDefault(x => x.Recordkey == financeQueryGlAccount.GlAccountNumber);
                    if (previousGlsContract != null)
                    {
                        financeQueryGlAccount.ActualAmount += previousGlsContract.OpenBal.HasValue ? previousGlsContract.OpenBal.Value : 0m;
                        var monthlyDebits = previousGlsContract.Mdebits;
                        if (monthlyDebits != null)
                        {
                            foreach (var debitAmt in monthlyDebits)
                            {
                                financeQueryGlAccount.ActualAmount += debitAmt.HasValue ? debitAmt.Value : 0m;
                            }
                        }
                        var monthlyCredits = previousGlsContract.Mcredits;
                        if (monthlyCredits != null)
                        {
                            foreach (var creditAmt in monthlyCredits)
                            {
                                financeQueryGlAccount.ActualAmount -= creditAmt.HasValue ? creditAmt.Value : 0m;
                            }
                        }
                    }
                }
            }
            return financeQueryGlAccount;
        }

        private FinanceQueryGlAccount BuildGlAccountEntityWithAmounts(string recordKey, GlBudgetPoolType poolType, GlAcctsMemos glAccountAmounts, bool includeBudgetAmounts = true)
        {
            var glAccount = new FinanceQueryGlAccount(recordKey, poolType);
            if (includeBudgetAmounts)
            {
                glAccount.BudgetAmount = glAccountAmounts.GlBudgetPostedAssocMember.HasValue ? glAccountAmounts.GlBudgetPostedAssocMember.Value : 0m;
                glAccount.BudgetAmount += glAccountAmounts.GlBudgetMemosAssocMember.HasValue ? glAccountAmounts.GlBudgetMemosAssocMember.Value : 0m;
            }
            glAccount.EncumbranceAmount += glAccountAmounts.GlEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.GlEncumbrancePostedAssocMember.Value : 0m;
            glAccount.EncumbranceAmount += glAccountAmounts.GlEncumbranceMemosAssocMember.HasValue ? glAccountAmounts.GlEncumbranceMemosAssocMember.Value : 0m;
            glAccount.RequisitionAmount += glAccountAmounts.GlRequisitionMemosAssocMember.HasValue ? glAccountAmounts.GlRequisitionMemosAssocMember.Value : 0m;
            glAccount.ActualAmount += glAccountAmounts.GlActualPostedAssocMember.HasValue ? glAccountAmounts.GlActualPostedAssocMember.Value : 0m;
            glAccount.ActualAmount += glAccountAmounts.GlActualMemosAssocMember.HasValue ? glAccountAmounts.GlActualMemosAssocMember.Value : 0m;
            return glAccount;
        }

        private FinanceQueryGlAccount BuildUmbrellaGlAccountEntityWithAmounts(string recordKey, GlAcctsMemos glAccountAmounts)
        {
            var glAccount = new FinanceQueryGlAccount(recordKey, GlBudgetPoolType.Umbrella);
            glAccount.BudgetAmount = glAccountAmounts.FaBudgetPostedAssocMember.HasValue ? glAccountAmounts.FaBudgetPostedAssocMember.Value : 0m;
            glAccount.BudgetAmount += glAccountAmounts.FaBudgetMemoAssocMember.HasValue ? glAccountAmounts.FaBudgetMemoAssocMember.Value : 0m;
            glAccount.EncumbranceAmount += glAccountAmounts.FaEncumbrancePostedAssocMember.HasValue ? glAccountAmounts.FaEncumbrancePostedAssocMember.Value : 0m;
            glAccount.EncumbranceAmount += glAccountAmounts.FaEncumbranceMemoAssocMember.HasValue ? glAccountAmounts.FaEncumbranceMemoAssocMember.Value : 0m;
            glAccount.RequisitionAmount += glAccountAmounts.FaRequisitionMemoAssocMember.HasValue ? glAccountAmounts.FaRequisitionMemoAssocMember.Value : 0m;
            glAccount.ActualAmount += glAccountAmounts.FaActualPostedAssocMember.HasValue ? glAccountAmounts.FaActualPostedAssocMember.Value : 0m;
            glAccount.ActualAmount += glAccountAmounts.FaActualMemoAssocMember.HasValue ? glAccountAmounts.FaActualMemoAssocMember.Value : 0m;
            return glAccount;
        }

        private FinanceQueryGlAccount BuildGlAccountEntityWithAmountsForClosedYears(GlsFyr glsFyr, GlBudgetPoolType poolType, bool includeBudgetAmounts
            , GeneralLedgerClassConfiguration glClassConfiguration
            , GeneralLedgerAccountStructure glAccountStructure
            , CfwebDefaults cfWebDefaults)
        {
            var glAccount = new FinanceQueryGlAccount(glsFyr.Recordkey, poolType);
            if (includeBudgetAmounts)
            {
                glAccount.BudgetAmount = CalculateBudgetForGlAccountInClosedYear(glsFyr);
            }
            glAccount.ActualAmount = poolType == GlBudgetPoolType.Umbrella ? CalculateActualsForUmbrellaInClosedYear(glsFyr) : CalculateActualsForGlAccountInClosedYear(glsFyr, glClassConfiguration, glAccountStructure, cfWebDefaults);
            glAccount.EncumbranceAmount = poolType == GlBudgetPoolType.Umbrella ? CalculateEncumbrancesForUmbrellaInClosedYear(glsFyr) : CalculateEncumbrancesForGlAccountInClosedYear(glsFyr);

            return glAccount;
        }

        private decimal CalculateBudgetForGlAccountInClosedYear(GlsFyr glsContract)
        {
            var budgetAmount = glsContract.BAlocDebitsYtd.HasValue ? glsContract.BAlocDebitsYtd.Value : 0m;
            budgetAmount -= glsContract.BAlocCreditsYtd.HasValue ? glsContract.BAlocCreditsYtd.Value : 0m;

            return budgetAmount;
        }

        private decimal CalculateActualsForGlAccountInClosedYear(GlsFyr glsContract, GeneralLedgerClassConfiguration glClassConfiguration, GeneralLedgerAccountStructure generalLedgerAccountStructure, CfwebDefaults cfWebDefaults)
        {
            var actualsAmount = 0m;
            var glClass = GetGlAccountGlClass(glsContract.Recordkey, glClassConfiguration, generalLedgerAccountStructure);
            if (glClass == GlClass.FundBalance)
            {
                actualsAmount = glsContract.OpenBal.HasValue ? glsContract.OpenBal.Value : 0m;
                bool includeYEClosingAmounts = true;
                if (cfWebDefaults != null)
                {
                    if (!string.IsNullOrEmpty(cfWebDefaults.CfwebFqIncludeYeAmounts) && cfWebDefaults.CfwebFqIncludeYeAmounts.Equals("N"))
                    {
                        includeYEClosingAmounts = false;
                    }
                }
                //include YE closing amounts to fund balance, if CFWEB.FQ.INCLUDE.YE.AMOUNTS is set to "Y" or null
                if (includeYEClosingAmounts)
                {
                    actualsAmount += glsContract.CloseDebits.HasValue ? glsContract.CloseDebits.Value : 0m;
                    actualsAmount -= glsContract.CloseCredits.HasValue ? glsContract.CloseCredits.Value : 0m;
                }


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

        #endregion

        #region General helpers

        private string BuildFilterCriteria(CostCenterComponentQueryCriteria filterComp)
        {
            string componentFilterCriteria = null;
            string valueFilterCriteria = null;
            string rangeFilterCriteria = null;

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
            return componentFilterCriteria;
        }

        async Task<List<T>> BulkReadRecordAsync<T>(string physicalFilename, string[] keys) where T : class, IColleagueEntity
        {
            List<T> result = new List<T>();
            if (keys != null && keys.Count() <= bulkReadSize)
            {
                var records = await DataReader.BulkReadRecordAsync<T>(physicalFilename, keys);
                return records.ToList();
            }

            for (int i = 0; i < keys.Count(); i += bulkReadSize)
            {
                var subList = keys.Skip(i).Take(bulkReadSize);
                var records = await DataReader.BulkReadRecordAsync<T>(physicalFilename, subList.ToArray());
                if (records == null)
                {
                    string errorMessage = string.Format("Unexpected null from bulk read for object {0}", typeof(T).Name);
                    logger.Error(errorMessage);
                }
                else
                {
                    result.AddRange(records);
                }
            }
            return result;
        }

        async Task<List<T>> BulkReadRecordAsync<T>(string[] keys) where T : class, IColleagueEntity
        {
            List<T> result = new List<T>();
            if (keys != null && keys.Count() <= bulkReadSize)
            {
                var records = await DataReader.BulkReadRecordAsync<T>(keys);
                return records.ToList();
            }
            else
            {

                for (int i = 0; i < keys.Count(); i += bulkReadSize)
                {
                    var subList = keys.Skip(i).Take(bulkReadSize);
                    var records = await DataReader.BulkReadRecordAsync<T>(subList.ToArray());
                    if (records == null)
                    {
                        string errorMessage = string.Format("Unexpected null from bulk read for object {0}", typeof(T).Name);
                        logger.Error(errorMessage);
                    }
                    else
                    {
                        result.AddRange(records);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Obtain the GL Class for a GL account
        /// </summary>
        /// <param name="glAccount">A GL account number.</param>
        /// <param name="glClassConfiguration">General Ledger Class configuration.</param>
        private GlClass GetGlAccountGlClass(string glAccount, GeneralLedgerClassConfiguration glClassConfiguration, GeneralLedgerAccountStructure generalLedgerAccountStructure)
        {
            GlClass glClass = new GlClass();
            try
            {
                glClass = GlAccountUtility.GetGlAccountGlClass(glAccount, glClassConfiguration, generalLedgerAccountStructure.MajorComponentStartPositions);
            }
            catch (ArgumentNullException aex)
            {
                logger.Warn(aex.Message);
                glClass = GlClass.Asset;
            }
            catch (ApplicationException)
            {
                logger.Warn("Invalid/unsupported GL class.");
                glClass = GlClass.Asset;
            }
            catch (Exception)
            {
                logger.Warn("Error occurred determining GL class for GL account number.");
                glClass = GlClass.Asset;
            }
            return glClass;
        }

        private bool IsFilterWideOpen(FinanceQueryCriteria criteria)
        {
            return FilterUtility.IsFilterWideOpen(criteria) && (criteria.ProjectReferenceNos == null || !criteria.ProjectReferenceNos.Any());
        }

        #endregion
    }
}
