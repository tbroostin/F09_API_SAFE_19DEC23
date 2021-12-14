// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// Repository for GL Account balances.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy), System.Runtime.InteropServices.GuidAttribute("D1E2A0B4-E0C4-47C3-AA8D-5DA69DF4340D")]
    public class GlAccountBalancesRepository : BaseColleagueRepository, IGlAccountBalancesRepository
    {
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;

        /// <summary>
        /// Constructor for the GL Account balances repository.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public GlAccountBalancesRepository(IGeneralLedgerAccountRepository generalLedgerAccountRepository, ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
        }

        /// <summary>
        /// This method gets the list of gl account balances for a fiscal year.
        /// </summary>
        /// <param name="glAccounts">List of GL accounts in internal format</param>
        /// <param name="fiscalYear">General Ledger fiscal year.</param>
        /// <param name="generalLedgerUser">General Ledger User record.</param>
        /// <param name="glAccountStructure">General Ledger Account Structure.</param>
        /// <param name="glClassConfiguration">General Ledger Class Configuration</param>
        /// <returns>Returns the list of GL account balances domain entities for the fiscal year.</returns>
        public async Task<IEnumerable<GlAccountBalances>> QueryGlAccountBalancesAsync(List<string> glAccounts, string fiscalYear,
            GeneralLedgerUser generalLedgerUser,
            GeneralLedgerAccountStructure glAccountStructure,
            GeneralLedgerClassConfiguration glClassConfiguration)
        {
            List<GlAccountBalances> glAccountBalancesEntities = new List<GlAccountBalances>();

            if (string.IsNullOrEmpty(fiscalYear))
            {
                throw new ArgumentNullException("fiscalYear", "A fiscal year must be specified.");
            }

            // These arguments should contain a value.
            if (glAccounts == null || !glAccounts.Any())
            {
                throw new ArgumentNullException("glAccounts", "GL accounts must be specified.");
            }

            if (glAccountStructure == null)
            {
                LogDataError("glAccountStructure", "", glAccountStructure);
                return glAccountBalancesEntities;
            }

            if (generalLedgerUser == null)
            {
                logger.Error("General Ledger User is null. No GL Account Balances to return.");
                return glAccountBalancesEntities;
            }

            // If the user does not have any GL accounts assigned, return an empty list of GL balances.
            if (generalLedgerUser.AllAccounts == null || !generalLedgerUser.AllAccounts.Any())
            {
                LogDataError("AllAccounts", "", generalLedgerUser.AllAccounts);
                return glAccountBalancesEntities;
            }

            // Read the GEN.LDGR record for the fiscal year
            var genLdgrDataContract = await DataReader.ReadRecordAsync<GenLdgr>(fiscalYear);
            if (genLdgrDataContract == null)
            {
                logger.Warn("Missing GEN.LDGR record for ID: " + fiscalYear);
                return glAccountBalancesEntities;
            }

            if (string.IsNullOrEmpty(genLdgrDataContract.GenLdgrStatus))
            {
                logger.Warn("GEN.LDGR status is null.");
                return glAccountBalancesEntities;
            }

            if (genLdgrDataContract.GenLdgrStatus != "O")
            {
                string errorMessage = string.Format("{0} is a closed ledger.", fiscalYear);
                throw new ApplicationException(errorMessage);
            }

            var filteredGlAccounts = generalLedgerUser.AllAccounts.Where(x => glAccounts.Contains(x));

            // Also validate that the user still has access to the GL account list.
            if (filteredGlAccounts == null || !filteredGlAccounts.Any())
            {
                throw new PermissionsException("You do not have access to the requested GL accounts " + glAccounts);
            }

            var filteredUserGlAccounts = filteredGlAccounts.ToArray();
            //Read GL Account records.
            var glAccountContracts = await DataReader.BulkReadRecordAsync<GlAccts>(filteredUserGlAccounts);

            //get gl account description
            var glAccountDescriptionsDictionary = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(filteredUserGlAccounts, glAccountStructure);

            foreach (var glAccount in glAccounts)
            {
                var hasAccess = generalLedgerUser.AllAccounts.Contains(glAccount);
                var glAccountContract = glAccountContracts.FirstOrDefault(x => x.Recordkey == glAccount);
                var glAccountBalance = await GetGlAccountBalance(glAccount, fiscalYear, generalLedgerUser, glAccountStructure, glAccountContract, glAccountDescriptionsDictionary, hasAccess);
                glAccountBalancesEntities.Add(glAccountBalance);
            }
            return glAccountBalancesEntities;
        }

        private async Task<GlAccountBalances> GetGlAccountBalance(string glAccount, string fiscalYear,
            GeneralLedgerUser generalLedgerUser,
            GeneralLedgerAccountStructure glAccountStructure,
            GlAccts glAccountContract,
            Dictionary<string, string> glAccountDescriptionsDictionary,
            bool hasAccess)
        {
            try
            {
                GlAccountBalances glAccountBalance = new GlAccountBalances(glAccount, glAccountStructure.MajorComponentStartPositions.ToList());                
                if (!hasAccess)
                {
                    glAccountBalance.ErrorMessage = "You do not have access to this GL Account.";
                    return glAccountBalance;
                }
                if (!generalLedgerUser.ExpenseAccounts.Contains(glAccount))
                {
                    glAccountBalance.ErrorMessage = "The GL account is not an expense type.";
                    return glAccountBalance;
                }
                if (glAccountContract == null)
                {
                    glAccountBalance.ErrorMessage = "The GL account does not exist.";
                    return glAccountBalance;
                }
                // Check that the GL account is active.
                if (glAccountContract.GlInactive != "A")
                {
                    glAccountBalance.ErrorMessage = "The GL account is not active.";
                    return glAccountBalance;
                }
                if (glAccountContract.MemosEntityAssociation == null)
                {
                    glAccountBalance.ErrorMessage = "The GL account is not available for the fiscal year.";
                    return glAccountBalance;
                }

                // Check that the GL account is available for the fiscal year.
                var glAccountMemosForFiscalYear = glAccountContract.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

                if (glAccountMemosForFiscalYear == null)
                {
                    glAccountBalance.ErrorMessage = "The GL account is not available for the fiscal year.";
                    return glAccountBalance;
                }
                else
                {
                    string glAccountStatus = glAccountMemosForFiscalYear.GlFreezeFlagsAssocMember;

                    switch (glAccountStatus)
                    {
                        case "C":
                            glAccountBalance.ErrorMessage = "The GL account is closed.";
                            break;
                        case "F":
                            glAccountBalance.ErrorMessage = "The GL account is frozen.";
                            break;
                        case "Y":
                            glAccountBalance.ErrorMessage = "The GL account is in year-end status.";
                            break;
                        default:
                            await PopulateGlAccountBalances(glAccount, fiscalYear, glAccountStructure, glAccountDescriptionsDictionary, glAccountBalance, glAccountMemosForFiscalYear);
                            break;
                    }
                }
                return glAccountBalance;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        #region Private methods
        private static string GetGlAccountDescription(string glAccountNumber, Dictionary<string, string> glAccountDescriptionsDictionary)
        {
            string description = "";
            if (!string.IsNullOrEmpty(glAccountNumber))
            {
                glAccountDescriptionsDictionary.TryGetValue(glAccountNumber, out description);
            }
            return description ?? string.Empty;
        }

        private static void PopulateGlAmountFromFaFields(GlAccountBalances glAccountBalance, GlAcctsMemos glAccountMemos)
        {
            glAccountBalance.BudgetAmount = glAccountMemos.FaBudgetPostedAssocMember.HasValue ? glAccountMemos.FaBudgetPostedAssocMember.Value : 0m;
            glAccountBalance.BudgetAmount += glAccountMemos.FaBudgetMemoAssocMember.HasValue ? glAccountMemos.FaBudgetMemoAssocMember.Value : 0m;
            glAccountBalance.EncumbranceAmount = glAccountMemos.FaEncumbrancePostedAssocMember.HasValue ? glAccountMemos.FaEncumbrancePostedAssocMember.Value : 0m;
            glAccountBalance.EncumbranceAmount += glAccountMemos.FaEncumbranceMemoAssocMember.HasValue ? glAccountMemos.FaEncumbranceMemoAssocMember.Value : 0m;
            glAccountBalance.RequisitionAmount = glAccountMemos.FaRequisitionMemoAssocMember.HasValue ? glAccountMemos.FaRequisitionMemoAssocMember.Value : 0m;
            glAccountBalance.ActualAmount = glAccountMemos.FaActualPostedAssocMember.HasValue ? glAccountMemos.FaActualPostedAssocMember.Value : 0m;
            glAccountBalance.ActualAmount += glAccountMemos.FaActualMemoAssocMember.HasValue ? glAccountMemos.FaActualMemoAssocMember.Value : 0m;
        }
        private async Task PopulateGlAccountBalances(string glAccount, string fiscalYear, GeneralLedgerAccountStructure glAccountStructure, Dictionary<string, string> glAccountDescriptionsDictionary, GlAccountBalances glAccountBalance, GlAcctsMemos glAccountMemosForFiscalYear)
        {            
            glAccountBalance.GlAccountDescription = GetGlAccountDescription(glAccount, glAccountDescriptionsDictionary);
            glAccountBalance.IsPooleeAccount = false;
            if (string.IsNullOrEmpty(glAccountMemosForFiscalYear.GlPooledTypeAssocMember))
            {
                glAccountBalance.BudgetAmount = glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.Value : 0m;
                glAccountBalance.BudgetAmount += glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.Value : 0m;
                glAccountBalance.EncumbranceAmount = glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.Value : 0m;
                glAccountBalance.EncumbranceAmount += glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.Value : 0m;
                glAccountBalance.RequisitionAmount = glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.Value : 0m;
                glAccountBalance.ActualAmount = glAccountMemosForFiscalYear.GlActualPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualPostedAssocMember.Value : 0m;
                glAccountBalance.ActualAmount += glAccountMemosForFiscalYear.GlActualMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualMemosAssocMember.Value : 0m;
            }
            //poolee gl accounts should be assigned with corresponding umbrella gl account amounts
            else
            {
                if (glAccountMemosForFiscalYear.GlPooledTypeAssocMember.ToUpperInvariant() == "U")
                {
                    PopulateGlAmountFromFaFields(glAccountBalance, glAccountMemosForFiscalYear);
                }
                else if (glAccountMemosForFiscalYear.GlPooledTypeAssocMember.ToUpperInvariant() == "P")
                {
                    // get umbrella for this poolee
                    var umbrellaGlAccount = glAccountMemosForFiscalYear.GlBudgetLinkageAssocMember;
                    // Read the GL.ACCTS record for the umbrella, and get the amounts for fiscal year.
                    var umbrellaAccount = await DataReader.ReadRecordAsync<GlAccts>(umbrellaGlAccount);

                    if (umbrellaAccount == null)
                    {
                        glAccountBalance.ErrorMessage = string.Format("Invalid Umbrella account for poolee account {0}.", glAccount);
                    }

                    if (string.IsNullOrEmpty(umbrellaAccount.Recordkey))
                    {
                        glAccountBalance.ErrorMessage = string.Format("Invalid Umbrella account for poolee account {0}.", glAccount);
                    }

                    //var hasAccessToUmbrellaAccount = generalLedgerUser.ExpenseAccounts.Contains(umbrellaGlAccount);
                    glAccountBalance.IsPooleeAccount = true;
                    GlAccount umbrellaGlAccountEntity = new GlAccount(umbrellaGlAccount);
                    glAccountBalance.UmbrellaGlAccount = umbrellaGlAccountEntity.GetFormattedGlAccount(glAccountStructure.MajorComponentStartPositions);

                    var umbrellaGlAccountAmounts = umbrellaAccount.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);
                    if (umbrellaGlAccountAmounts != null)
                    {
                        PopulateGlAmountFromFaFields(glAccountBalance, umbrellaGlAccountAmounts);
                    }
                }
            }
        }
        #endregion
    }
}
