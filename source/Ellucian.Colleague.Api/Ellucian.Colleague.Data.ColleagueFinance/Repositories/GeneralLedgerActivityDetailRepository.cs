// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Data.Colleague.DataContracts;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for General Ledger Account activity detail.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy), System.Runtime.InteropServices.GuidAttribute("D1E2A0B4-E0C4-47C3-AA8D-5DA69DF4340D")]
    public class GeneralLedgerActivityDetailRepository : BaseColleagueRepository, IGeneralLedgerActivityDetailRepository
    {
        /// <summary>
        /// Constructor for the General Ledger Account activity detail repository.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public GeneralLedgerActivityDetailRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// This method gets the list of activity detail for actuals and encumbrances for a GL account for a fiscal year.
        /// </summary>
        /// <param name="glAccount">The GL account number.</param>
        /// <param name="fiscalYear">The GL fiscal year.</param>
        /// <param name="costCenterStructure">List of objects with information to build the cost center definitions.</param>
        /// <param name="glClassConfiguration">GL configuration information.</param>
        /// <returns>Returns the list of activity detail posted to the GL account for the fiscal year.</returns>
        public async Task<GlAccountActivityDetail> QueryGlActivityDetailAsync(string glAccount, string fiscalYear,
            CostCenterStructure costCenterStructure, GeneralLedgerClassConfiguration glClassConfiguration)
        {
            var glAccountDomain = new GlAccountActivityDetail(glAccount);

            if (string.IsNullOrEmpty(glAccount))
            {
                throw new ArgumentNullException("glAccount", "A GL account must be specified.");
            }

            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("fiscalYear", "A fiscal year must be specified.");
            }

            try
            {
                // Get the GL.SOURCE.CODES valcode table
                var GlSourceCodesValidationTable = await GetGlSourceCodesAsync();

                // Determine the type of transaction (e.g. Actual, Encumbrance, or Requisition)
                var glTransactionEntities = new List<GlTransaction>();
                ApplValcodesVals GLSourceCode = new ApplValcodesVals();

                // Read the GL.ACCTS record for the GL account to get the actuals and budget pending posting amounts.
                try
                {
                    GlAccts glAccountRecord = await DataReader.ReadRecordAsync<GlAccts>(glAccount);
                    if (glAccountRecord == null)
                        throw new ApplicationException("Missing GL.ACCTS record for GL account " + glAccount);

                    if (glAccountRecord.MemosEntityAssociation != null)
                    {
                        var glAccountFundsAssociation = glAccountRecord.MemosEntityAssociation.FirstOrDefault(x => x != null && x.AvailFundsControllerAssocMember == fiscalYear);
                        if (glAccountFundsAssociation != null)
                        {
                            // Get the budget pending posting amount.
                            glAccountDomain.MemoBudgetAmount = glAccountFundsAssociation.GlBudgetMemosAssocMember.HasValue ? glAccountFundsAssociation.GlBudgetMemosAssocMember.Value : 0m;
                            // Get the actuals pending posting amount.
                            glAccountDomain.MemoActualsAmount = glAccountFundsAssociation.GlActualMemosAssocMember.HasValue ? glAccountFundsAssociation.GlActualMemosAssocMember.Value : 0m;
                        }
                    }
                }
                catch (ApplicationException ae)
                {
                    LogDataError("", "", ae);
                }

                #region Process transactions from GLA.FYR

                // Initialize the file name and criteria.
                var glaFyrFileName = "GLA." + fiscalYear;
                // For alpha GL accounts, avoid the problem with Unidata using A, N and X as wildcards.
                // The criteria has to be like "'11_01_01_00_10005_54N70'..." for it to work.
                var criteria = "@ID LIKE " + "\"'" + glAccount + "'...\"";

                // Select and bulk read the GLA records for the GL account.
                var glaFyrIds = await DataReader.SelectAsync(glaFyrFileName, criteria);
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
                var encDataContract = await DataReader.ReadRecordAsync<EncFyr>(encFyrFileName, glAccount, true);

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
                                string transactionId = glAccount + "*ENC*" + encCounter;

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
                                string transactionId = glAccount + "*ENC*" + encCounter;

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
                #endregion

                // Assign the transactions to the GL account.
                foreach (var transaction in glTransactionEntities)
                {
                    // The list of transactions includes all four types: budget, actuals, encumbrances and requisitions.
                    // Add the budget, actuals and encumbrances separately to obtain totals; requisitions are included in encumbrances.
                    if (transaction.GlTransactionType == GlTransactionType.Budget)
                    {
                        glAccountDomain.BudgetAmount += transaction.Amount;
                    }
                    else if (transaction.GlTransactionType == GlTransactionType.Actual)
                    {
                        glAccountDomain.ActualAmount += transaction.Amount;
                    }
                    else
                    {
                        glAccountDomain.EncumbranceAmount += transaction.Amount;
                    }

                    glAccountDomain.AddGlTransaction(transaction);
                }

                // Remove those encumbrance or Requisition transactions that have a $0.00 amount. 
                // There shouldn't be any but just in case, we do not want to display $0.00.
                glAccountDomain.RemoveZeroDollarEncumbranceTransactions();


                #region Estimated Opening Balance/Closing Year Amount

                GlClass glClass = GlClass.Asset;
                try
                {
                    // Obtain the GL class for the GL account.
                    glClass = GlAccountUtility.GetGlAccountGlClass(glAccountDomain.GlAccountNumber, glClassConfiguration);
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
                    var fiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear.ToString());
                    if (fiscalYearGenLdgrContract != null && !string.IsNullOrEmpty(fiscalYearGenLdgrContract.GenLdgrStatus))
                    {
                        GlsFyr glAccountGlsContract = null;
                        GlsFyr glAccountpreviousGlsContract = null;

                        // If the fiscal year is open, determine the status of the previous fiscal year.
                        if (fiscalYearGenLdgrContract.GenLdgrStatus.ToUpperInvariant() == "O")
                        {
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

                            if (previousFiscalYear != 0)
                            {
                                // Read the previous fiscal year record to check if the status is open or not.
                                var previousFiscalYearGenLdgrContract = await DataReader.ReadRecordAsync<GenLdgr>(previousFiscalYear.ToString());
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
                                            var glsFyrYearId = "GLS." + fiscalYear.ToString();
                                            glAccountGlsContract = await DataReader.ReadRecordAsync<GlsFyr>(glsFyrYearId, glAccountDomain.GlAccountNumber);
                                            glAccountDomain.EstimatedOpeningBalance = glAccountGlsContract.GlsEstimatedOpenBal.HasValue ? glAccountGlsContract.GlsEstimatedOpenBal.Value : 0m;
                                        }

                                        else if (glClass == GlClass.Asset || glClass == GlClass.Liability)
                                        {
                                            // For Asset and Liability accounts only, when the previous fiscal year is open,
                                            // add the open balance and monthly debits and subtract monthly credits from the GLS.previousyear record.
                                            var previousGlsFyrYearId = "GLS." + previousFiscalYear.ToString();
                                            glAccountpreviousGlsContract = await DataReader.ReadRecordAsync<GlsFyr>(previousGlsFyrYearId, glAccountDomain.GlAccountNumber);
                                            if (glAccountpreviousGlsContract != null)
                                            {
                                                glAccountDomain.EstimatedOpeningBalance = glAccountpreviousGlsContract.OpenBal.HasValue ? glAccountpreviousGlsContract.OpenBal.Value : 0m;
                                                var monthlyDebits = glAccountpreviousGlsContract.Mdebits;
                                                if (monthlyDebits != null)
                                                {
                                                    foreach (var debitAmt in monthlyDebits)
                                                    {
                                                        glAccountDomain.EstimatedOpeningBalance += debitAmt.HasValue ? debitAmt.Value : 0m;
                                                    }
                                                }
                                                var monthlyCredits = glAccountpreviousGlsContract.Mcredits;
                                                if (monthlyCredits != null)
                                                {
                                                    foreach (var creditAmt in monthlyCredits)
                                                    {
                                                        glAccountDomain.EstimatedOpeningBalance -= creditAmt.HasValue ? creditAmt.Value : 0m;
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

                        // If the fiscal year is closed, only calculate the closing year amount for fund balance GL accounts.
                        else
                        {
                            if (glClass == GlClass.FundBalance)
                            {
                                // The closing year amount is debits YTD minus credits YTD.
                                var glsFyrYearId = "GLS." + fiscalYear.ToString();
                                glAccountGlsContract = await DataReader.ReadRecordAsync<GlsFyr>(glsFyrYearId, glAccountDomain.GlAccountNumber);
                                glAccountDomain.ClosingYearAmount = glAccountGlsContract.CloseDebits.HasValue ? glAccountGlsContract.CloseDebits.Value : 0m;
                                glAccountDomain.ClosingYearAmount -= glAccountGlsContract.CloseCredits.HasValue ? glAccountGlsContract.CloseCredits.Value : 0m;
                            }
                        }
                    }
                    else
                    {
                        logger.Warn("Unable to determine fiscal year status from GEN.LDGR for fiscal year " + fiscalYear);
                    }
                }
                #endregion

                // Obtain the description for the GL account.
                var glAccountList = new List<string>();
                glAccountList.Add(glAccount);

                GetGlAccountDescriptionRequest descRequest = new GetGlAccountDescriptionRequest()
                {
                    GlAccountIds = glAccountList,
                    Module = "SS"
                };

                GetGlAccountDescriptionResponse descResponse = await transactionInvoker.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(descRequest);

                // The transaction returns the description for each GL account number.
                if (descResponse != null)
                {
                    if (descResponse.GlAccountIds != null && descResponse.GlDescriptions != null && descResponse.GlAccountIds.Any() && descResponse.GlDescriptions.Any())
                    {
                        glAccountDomain.GlAccountDescription = descResponse.GlDescriptions.FirstOrDefault();
                    }
                }

                // Determine the cost center ID and description associated to this GL account.
                string costCenterId = string.Empty;
                string costCenterName = string.Empty;

                if (!string.IsNullOrEmpty(glAccount))
                {
                    foreach (var component in costCenterStructure.CostCenterComponents)
                    {
                        if (component != null)
                        {
                            var componentId = glAccount.Substring(component.StartPosition, component.ComponentLength);
                            costCenterId += componentId;
                            var description = string.Empty;
                            if (component.IsPartOfDescription)
                            {
                                switch (component.ComponentType)
                                {
                                    case GeneralLedgerComponentType.Function:
                                        var fcRecord = await DataReader.ReadRecordAsync<FcDescs>(componentId);
                                        if (fcRecord != null)
                                        {
                                            description = fcRecord.FcDescription;
                                        }
                                        break;
                                    case GeneralLedgerComponentType.Fund:
                                        var fdRecord = await DataReader.ReadRecordAsync<FdDescs>(componentId);
                                        if (fdRecord != null)
                                        {
                                            description = fdRecord.FdDescription;
                                        }
                                        break;
                                    case GeneralLedgerComponentType.Location:
                                        var loRecord = await DataReader.ReadRecordAsync<LoDescs>(componentId);
                                        if (loRecord != null)
                                        {
                                            description = loRecord.LoDescription;
                                        }
                                        break;
                                    case GeneralLedgerComponentType.Object:
                                        var obRecord = await DataReader.ReadRecordAsync<ObDescs>(componentId);
                                        if (obRecord != null)
                                        {
                                            description = obRecord.ObDescription;
                                        }
                                        break;
                                    case GeneralLedgerComponentType.Source:
                                        var soRecord = await DataReader.ReadRecordAsync<SoDescs>(componentId);
                                        if (soRecord != null)
                                        {
                                            description = soRecord.SoDescription;
                                        }
                                        break;
                                    case GeneralLedgerComponentType.Unit:
                                        var unRecord = await DataReader.ReadRecordAsync<UnDescs>(componentId);

                                        if (unRecord != null)
                                        {
                                            description = unRecord.UnDescription;
                                        }
                                        break;
                                };

                                if (!string.IsNullOrEmpty(description))
                                {
                                    if (string.IsNullOrEmpty(costCenterName))
                                    {
                                        costCenterName = description;
                                    }
                                    else
                                    {
                                        costCenterName = costCenterName + " : " + description;
                                    }
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(costCenterName))
                {
                    glAccountDomain.Name = costCenterName;
                }
                else
                {
                    glAccountDomain.Name = "No cost center description available.";
                }

                glAccountDomain.CostCenterId = costCenterId;

                // Get the unit part of the cost center ID associated to this GL account.
                glAccountDomain.UnitId = glAccount.Substring(costCenterStructure.Unit.StartPosition, costCenterStructure.Unit.ComponentLength);
            }
            catch (Exception ex)
            {
                // Log the message and throw the exception
                logger.Error(ex.Message);
            }

            return glAccountDomain;
        }

        #region Private methods

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

        #endregion
    }
}