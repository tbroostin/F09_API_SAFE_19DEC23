// Copyright 2017-2020 Ellucian Company L.P. and i affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using System;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    public class TestGeneralLedgerAccountRepository : IGeneralLedgerAccountRepository
    {
        // Create a list of GL account data contracts.
        public List<GlAccts> glAccountRecords = new List<GlAccts>()
        {
            new GlAccts
            {
                Recordkey = "11_00_01_02_ACTIV_50000",
                GlInactive = "A",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = 50000,
                        GlBudgetMemosAssocMember = 500,
                        GlActualPostedAssocMember = 501,
                        GlActualMemosAssocMember = 502,
                        GlEncumbrancePostedAssocMember = 503,
                        GlEncumbranceMemosAssocMember = 504,
                        GlRequisitionMemosAssocMember = 505,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                    AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                    GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                }
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_INCTV_59999",
                GlInactive = "I",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                        GlFreezeFlagsAssocMember = "Y",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "C",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlPooledTypeAssocMember = null

                    },
                }
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_UMBRL_50000",
                GlInactive = "A",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        FaBudgetPostedAssocMember = 66666,
                        FaBudgetMemoAssocMember = 6666,
                        FaActualPostedAssocMember = 667,
                        FaActualMemoAssocMember = 668,
                        FaEncumbrancePostedAssocMember = 669,
                        FaEncumbranceMemoAssocMember = 670,
                        FaRequisitionMemoAssocMember = 671,
                        GlPooledTypeAssocMember = "U"
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        FaBudgetPostedAssocMember = null,
                        FaBudgetMemoAssocMember = null,
                        FaActualPostedAssocMember = null,
                        FaActualMemoAssocMember = null,
                        FaEncumbrancePostedAssocMember = null,
                        FaEncumbranceMemoAssocMember = null,
                        FaRequisitionMemoAssocMember = null,
                        GlPooledTypeAssocMember = "U"
                    },

                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        FaBudgetPostedAssocMember = null,
                        FaBudgetMemoAssocMember = null,
                        FaActualPostedAssocMember = null,
                        FaActualMemoAssocMember = null,
                        FaEncumbrancePostedAssocMember = null,
                        FaEncumbranceMemoAssocMember = null,
                        FaRequisitionMemoAssocMember = null,
                        GlPooledTypeAssocMember = "u"

                    },
                }
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_POOL1_50001",
                GlInactive = "A",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                        GlFreezeFlagsAssocMember = "F",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "C",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                }
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_POOL2_50002",
                GlInactive = "I",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                        GlFreezeFlagsAssocMember = "F",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "C",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = "P"
                    },
                }
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_NOMEM_58888",
                GlInactive = "A",
                MemosEntityAssociation = null
            },
            new GlAccts
            {
                Recordkey = "11_00_01_02_CLOSD_57777",
                GlInactive = "A",
                MemosEntityAssociation = new List<GlAcctsMemos>()
                {
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(2).Year.ToString(),
                        GlFreezeFlagsAssocMember = "F",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.AddYears(1).Year.ToString(),
                        GlFreezeFlagsAssocMember = "Y",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                    new GlAcctsMemos
                    {
                        AvailFundsControllerAssocMember = DateTime.Now.Year.ToString(),
                        GlFreezeFlagsAssocMember = "C",
                        GlBudgetPostedAssocMember = null,
                        GlBudgetMemosAssocMember = null,
                        GlEncumbrancePostedAssocMember = null,
                        GlEncumbranceMemosAssocMember = null,
                        GlRequisitionMemosAssocMember = null,
                        GlActualPostedAssocMember = null,
                        GlActualMemosAssocMember = null,
                        GlPooledTypeAssocMember = null
                    },
                }
            },
        };

        // Create a list of GL account data contracts.
        public List<GlAccountValidationResponse> glAccountValidationResponses = new List<GlAccountValidationResponse>()
        {
            new GlAccountValidationResponse("11_00_01_02_ACTIV_50000")
            {
                Status = "success",
                ErrorMessage = null,
                RemainingBalance = 47985
            },
            new GlAccountValidationResponse("11_00_01_02_INCTV_59999")
            {
                Status = "failure",
                ErrorMessage = "The GL account is not active.",
                RemainingBalance = 0
            },
            new GlAccountValidationResponse("11_00_01_02_UMBRL_50000")
            {
                Status = "success",
                ErrorMessage = null,
                RemainingBalance = 69987
            },
            new GlAccountValidationResponse("11_00_01_02_POOL1_50001")
            {
                Status = "failure",
                ErrorMessage = "A poolee type GL account is not allowed in budget adjustments.",
                RemainingBalance = 0
            },
            new GlAccountValidationResponse("11_00_01_02_NOMEM_58888")
            {
                Status = "failure",
                ErrorMessage = "The GL account is not available for the fiscal year.",
                RemainingBalance = 0
            },
            new GlAccountValidationResponse("11_00_01_02_NOFYR_58888")
            {
                Status = "failure",
                ErrorMessage = "The GL account is not available for the fiscal year.",
                RemainingBalance = 0
            },
        };

        // Create a list of active general ledger account strings.
        public List<string> activeGlAccounts = new List<string>() { "11_01_01_00_00000_75075" };

        /// <summary>
        /// Restricts a list of GL accounts to those that are active.
        /// </summary>
        /// <param name="glAccounts">List of general ledger account strings.</param>
        /// <returns>A list of active general ledger account strings.</returns>
        public async Task<List<string>> GetActiveGeneralLedgerAccounts(List<string> glAccounts)
        {
            return await Task.Run(() => { return activeGlAccounts; });
        }

        /// <summary>
        /// Retrieves a set of general ledger accounts.
        /// </summary>
        /// <param name="generalLedgerAccountIds">Set of GL account ID.</param>
        /// <param name="majorComponentStartPositions">List of positions used to format the GL account ID.</param>
        /// <returns>Set of general ledger account domain entities.</returns>
        public async Task<Dictionary<string, string>> GetGlAccountDescriptionsAsync(IEnumerable<string> generalLedgerAccountIds, GeneralLedgerAccountStructure glAccountStructure)
        {
            var glAccountsList = new Dictionary<string, string>();

            for (int i = 0; i < generalLedgerAccountIds.Count(); i++)
            {
                if (!glAccountsList.ContainsKey(generalLedgerAccountIds.ElementAt(i)))
                {
                    glAccountsList.Add(generalLedgerAccountIds.ElementAt(i), "Description " + i.ToString());
                }
            }

            return await Task.Run(() => { return glAccountsList; });
        }

        /// <summary>
        /// Retrieves a single general ledger account.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <returns>General ledger account domain entity.</returns>
        public async Task<GeneralLedgerAccount> GetAsync(string generalLedgerAccountId, IEnumerable<string> majorComponentStartPositions)
        {
            return await Task.Run(() => { return new GeneralLedgerAccount(generalLedgerAccountId, majorComponentStartPositions) { Description = "Operating fund : General" }; });
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>General Ledger account validation response domain entity.</returns>
        public async Task<GlAccountValidationResponse> ValidateGlAccountAsync(string generalLedgerAccountId, string fiscalYear)
        {
            return await Task.Run(() =>
            {
                return glAccountValidationResponses.Where(x => x.Id == generalLedgerAccountId).FirstOrDefault();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="generalLedgerComponentKeys"></param>
        /// <param name="majorComponentType"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, string>> GetGlComponentDescriptionsByIdsAndComponentTypeAsync(IEnumerable<string> generalLedgerComponentIds, GeneralLedgerComponentType glComponentType)
        {
            var glAccountComponentValuesDictionary = new Dictionary<string, string>();

            for (int i = 0; i < generalLedgerComponentIds.Count(); i++)
            {
                glAccountComponentValuesDictionary.Add(generalLedgerComponentIds.ElementAt(i),"Description-" + generalLedgerComponentIds.ElementAt(i));
            }
            return await Task.Run(() => { return glAccountComponentValuesDictionary; });
        }

        Task<IEnumerable<GlAccount>> IGeneralLedgerAccountRepository.GetUserGeneralLedgerAccountsAsync(IEnumerable<string> glAccounts, GeneralLedgerAccountStructure glAccountStructure)
        {
            throw new NotImplementedException();
        }
    }
}