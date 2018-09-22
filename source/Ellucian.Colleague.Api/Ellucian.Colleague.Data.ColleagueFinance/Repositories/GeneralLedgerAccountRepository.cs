﻿// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
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
    /// Implements IGeneralLedgerAccountRepository.
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class GeneralLedgerAccountRepository : BaseColleagueRepository, IGeneralLedgerAccountRepository
    {
        /// <summary>
        /// This constructor allows us to instantiate a GL cost center repository object.
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object.</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object.</param>
        /// <param name="logger">Pass in an ILogger object.</param>
        public GeneralLedgerAccountRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Returns descriptions for the supplied general ledger numbers.
        /// </summary>
        /// <param name="generalLedgerAccountIds">Set of GL account ID.</param>
        /// <param name="glAccountStructure">GL account structure used to parse the GL numbers.</param>
        /// <returns>Dictionary of GL account numbers and corresponding descriptions.</returns>
        public async Task<Dictionary<string, string>> GetGlAccountDescriptionsAsync(IEnumerable<string> generalLedgerAccountIds, GeneralLedgerAccountStructure glAccountStructure)
        {
            // Initialize the GL accounts entities
            var glAccountDictionary = new Dictionary<string, string>();

            if (glAccountStructure == null)
            {
                logger.Warn("GL account structure is not set up.");
                return glAccountDictionary;
            }

            if (glAccountStructure.MajorComponents == null || !glAccountStructure.MajorComponents.Any())
            {
                logger.Warn("GL major components are not defined.");
                return glAccountDictionary;
            }

            // Remove any duplicates.
            generalLedgerAccountIds = generalLedgerAccountIds.Distinct().ToList();

            // Initialize the "key" side of the dictionary.
            foreach (var glAccountId in generalLedgerAccountIds)
            {
                glAccountDictionary.Add(glAccountId, string.Empty);
            }

            // Initialize the default major component setting using account structure.
            List<string> displayPieces = new List<string>();

            // Get the "SS" GLNODISP record for GL descriptions in SS.
            var glNumberDisplayContract = await DataReader.ReadRecordAsync<Glnodisp>("SS", true);
            if (glNumberDisplayContract != null && glNumberDisplayContract.DisplayPieces != null && glNumberDisplayContract.DisplayPieces.Any())
            {
                displayPieces = glNumberDisplayContract.DisplayPieces;
            }
            else
            {
                // Otherwise, get the default.
                int skipNumber = glAccountStructure.MajorComponents.Count;
                if (skipNumber < 2)
                {
                    skipNumber = 0;
                }
                else
                {
                    skipNumber = skipNumber - 2;
                }
                List<GeneralLedgerComponent> glComponentsForDescription = glAccountStructure.MajorComponents.Skip(skipNumber).ToList();
                foreach (var glComponent in glComponentsForDescription)
                {
                    switch (glComponent.ComponentType)
                    {
                        case GeneralLedgerComponentType.Function:
                            displayPieces.Add("FC");
                            break;
                        case GeneralLedgerComponentType.Fund:
                            displayPieces.Add("FD");
                            break;
                        case GeneralLedgerComponentType.Location:
                            displayPieces.Add("LO");
                            break;
                        case GeneralLedgerComponentType.Object:
                            displayPieces.Add("OB");
                            break;
                        case GeneralLedgerComponentType.Source:
                            displayPieces.Add("SO");
                            break;
                        case GeneralLedgerComponentType.Unit:
                            displayPieces.Add("UN");
                            break;
                    }
                }
            }

            // Map the major component display pieces into our GL class enumeration.
            Collection<FdDescs> fundDescriptions = null;
            Collection<FcDescs> functionDescriptions = null;
            Collection<ObDescs> objectDescriptions = null;
            Collection<UnDescs> unitDescriptions = null;
            Collection<SoDescs> sourceDescriptions = null;
            Collection<LoDescs> locationDescriptions = null;
            foreach (var majorComponentId in displayPieces)
            {
                switch (majorComponentId.ToUpper())
                {
                    case "FD":
                        // Get the fund descriptions
                        var fundComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Fund);
                        if (fundComponent != null)
                        {
                            var fundIds = generalLedgerAccountIds.Select(x => x.Substring(fundComponent.StartPosition, fundComponent.ComponentLength)).Distinct().ToList();
                            fundDescriptions = await DataReader.BulkReadRecordAsync<FdDescs>(fundIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (fundDescriptions != null)
                        {
                            var fundComponentDescriptions = fundDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.FdDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, fundComponent, fundComponentDescriptions);
                        }

                        break;
                    case "FC":
                        // Get the function descriptions
                        var functionComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Function);
                        if (functionComponent != null)
                        {
                            var functionIds = generalLedgerAccountIds.Select(x => x.Substring(functionComponent.StartPosition, functionComponent.ComponentLength)).Distinct().ToList();
                            functionDescriptions = await DataReader.BulkReadRecordAsync<FcDescs>(functionIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (functionDescriptions != null)
                        {
                            var functionComponentDescriptions = functionDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.FcDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, functionComponent, functionComponentDescriptions);
                        }

                        break;
                    case "OB":
                        // Get the object descriptions
                        var objectComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Object);
                        if (objectComponent != null)
                        {
                            var objectIds = generalLedgerAccountIds.Select(x => x.Substring(objectComponent.StartPosition, objectComponent.ComponentLength)).Distinct().ToList();
                            objectDescriptions = await DataReader.BulkReadRecordAsync<ObDescs>(objectIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (objectDescriptions != null)
                        {
                            var objectComponentDescriptions = objectDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.ObDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, objectComponent, objectComponentDescriptions);
                        }

                        break;
                    case "UN":
                        // Get the unit descriptions
                        var unitComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Unit);
                        if (unitComponent != null)
                        {
                            var unitIds = generalLedgerAccountIds.Select(x => x.Substring(unitComponent.StartPosition, unitComponent.ComponentLength)).Distinct().ToList();
                            unitDescriptions = await DataReader.BulkReadRecordAsync<UnDescs>(unitIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (unitDescriptions != null)
                        {
                            var unitComponentDescriptions = unitDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.UnDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, unitComponent, unitComponentDescriptions);
                        }

                        break;
                    case "SO":
                        // Get the source descriptions
                        var sourceComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Source);
                        if (sourceComponent != null)
                        {
                            var sourceIds = generalLedgerAccountIds.Select(x => x.Substring(sourceComponent.StartPosition, sourceComponent.ComponentLength)).Distinct().ToList();
                            sourceDescriptions = await DataReader.BulkReadRecordAsync<SoDescs>(sourceIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (sourceDescriptions != null)
                        {
                            var sourceComponentDescriptions = sourceDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.SoDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, sourceComponent, sourceComponentDescriptions);
                        }

                        break;
                    case "LO":
                        // Get the location descriptions
                        var locationComponent = glAccountStructure.MajorComponents.FirstOrDefault(x => x.ComponentType == GeneralLedgerComponentType.Location);
                        if (locationComponent != null)
                        {
                            var locationIds = generalLedgerAccountIds.Select(x => x.Substring(locationComponent.StartPosition, locationComponent.ComponentLength)).Distinct().ToList();
                            locationDescriptions = await DataReader.BulkReadRecordAsync<LoDescs>(locationIds.ToArray());
                        }

                        // Assign the object descriptions to the GL accounts
                        if (locationDescriptions != null)
                        {
                            var locationComponentDescriptions = locationDescriptions.Select(x => new ComponentDescription() { Id = x.Recordkey, Description = x.LoDescription }).ToList();
                            UpdateGlAccountDescriptions(ref glAccountDictionary, locationComponent, locationComponentDescriptions);
                        }

                        break;
                    default:
                        break;
                }
            }

            return glAccountDictionary;
        }

        /// <summary>
        /// Retrieves a single general ledger account.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <returns>General ledger account domain entity.</returns>
        public async Task<GeneralLedgerAccount> GetAsync(string generalLedgerAccountId, IEnumerable<string> majorComponentStartPositions)
        {
            if (string.IsNullOrEmpty(generalLedgerAccountId))
            {
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            try
            {
                GetGlAccountDescriptionRequest glAccountDescriptionRequest = new GetGlAccountDescriptionRequest()
                {
                    GlAccountIds = new List<string>() { generalLedgerAccountId },
                    Module = "SS"
                };
                GetGlAccountDescriptionResponse glAccountDescriptionResponse = await transactionInvoker.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(glAccountDescriptionRequest);

                var glDescription = "";
                if (glAccountDescriptionResponse != null
                    && glAccountDescriptionResponse.GlAccountIds != null
                    && glAccountDescriptionResponse.GlDescriptions != null)
                {
                    var glIndex = glAccountDescriptionResponse.GlAccountIds.IndexOf(generalLedgerAccountId);
                    if (glIndex >= 0 && (glIndex < glAccountDescriptionResponse.GlDescriptions.Count))
                    {
                        glDescription = glAccountDescriptionResponse.GlDescriptions[glIndex];
                    }
                }

                return new GeneralLedgerAccount(generalLedgerAccountId, majorComponentStartPositions)
                {
                    Description = glDescription
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validate a GL account. 
        /// If there is a fiscal year, it will also validate it for that year.
        /// </summary>
        /// <param name="generalLedgerAccountId">GL account ID.</param>
        /// <param name="fiscalYear">Optional; General Ledger fiscal year.</param>
        /// <returns>General Ledger account domain entity.</returns>
        public async Task<GlAccountValidationResponse> ValidateGlAccountAsync(string generalLedgerAccountId, string fiscalYear)
        {
            if (string.IsNullOrEmpty(generalLedgerAccountId))
            {
                throw new ArgumentNullException("generalLedgerAccountId", "generalLedgerAccountId is required.");
            }

            try
            {
                GlAccountValidationResponse glAccountValidationResponse = new GlAccountValidationResponse(generalLedgerAccountId);

                // Read the GL account record.
                var glAccountContract = await DataReader.ReadRecordAsync<GlAccts>(generalLedgerAccountId);
                if (glAccountContract == null)
                {
                    glAccountValidationResponse.Status = "failure";
                    glAccountValidationResponse.ErrorMessage = "The GL account does not exist.";
                    return glAccountValidationResponse;
                }

                // Check that the GL account is active.
                if (glAccountContract.GlInactive != "A")
                {
                    glAccountValidationResponse.Status = "failure";
                    glAccountValidationResponse.ErrorMessage = "The GL account is not active.";
                    return glAccountValidationResponse;
                }

                // If a fiscal year is passed in, validate the GL account for the fiscal year.
                if (string.IsNullOrEmpty(fiscalYear))
                {
                    // We are not validating for a fiscal year. The GL account is active.
                    glAccountValidationResponse.Status = "success";
                }
                else
                {
                    if (glAccountContract.MemosEntityAssociation == null)
                    {
                        glAccountValidationResponse.Status = "failure";
                        glAccountValidationResponse.ErrorMessage = "The GL account is not available for the fiscal year.";
                        return glAccountValidationResponse;
                    }

                    var glAccountMemosForFiscalYear = glAccountContract.MemosEntityAssociation.FirstOrDefault(x => x.AvailFundsControllerAssocMember == fiscalYear);

                    // Check that the GL account is available for the fiscal year.
                    if (glAccountMemosForFiscalYear == null)
                    {
                        glAccountValidationResponse.Status = "failure";
                        glAccountValidationResponse.ErrorMessage = "The GL account is not available for the fiscal year.";
                        return glAccountValidationResponse;
                    }
                    else
                    {
                        string glAccountStatus = glAccountMemosForFiscalYear.GlFreezeFlagsAssocMember;

                        // Validate that the GL account status.
                        if ((glAccountStatus == "C") || (glAccountStatus == "F") || (glAccountStatus == "Y"))
                        {
                            glAccountValidationResponse.Status = "failure";

                            switch (glAccountStatus)
                            {
                                case "C":
                                    glAccountValidationResponse.ErrorMessage = "The GL account is closed.";
                                    break;
                                case "F":
                                    glAccountValidationResponse.ErrorMessage = "The GL account is frozen.";
                                    break;
                                case "Y":
                                    glAccountValidationResponse.ErrorMessage = "The GL account is in year-end status.";
                                    break;
                            }
                        }
                        else
                        {
                            // For budget transactions, authorized status is allowed.
                            // If the GL account verifications are successful, obtain the remaining balance.                       
                            glAccountValidationResponse.Status = "success";

                            // Only non-pool GL accounts and umbrellas are allowed in a budget adjustment.
                            // Determine if the GL account is an umbrella or not to obtian the proper reamining balance.
                            // If the GL account is not pooled use the regular amount fields.
                            if (string.IsNullOrEmpty(glAccountMemosForFiscalYear.GlPooledTypeAssocMember))
                            {
                                glAccountValidationResponse.RemainingBalance = glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetPostedAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance += glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlBudgetMemosAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbrancePostedAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlEncumbranceMemosAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlRequisitionMemosAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.GlActualPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualPostedAssocMember.Value : 0m;
                                glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.GlActualMemosAssocMember.HasValue ? glAccountMemosForFiscalYear.GlActualMemosAssocMember.Value : 0m;
                            }
                            else
                            {
                                // If the GL account is an umbrella, use the pool amount fields.
                                if (glAccountMemosForFiscalYear.GlPooledTypeAssocMember.ToUpperInvariant() == "U")
                                {
                                    glAccountValidationResponse.RemainingBalance = glAccountMemosForFiscalYear.FaBudgetPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.FaBudgetPostedAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance += glAccountMemosForFiscalYear.FaBudgetMemoAssocMember.HasValue ? glAccountMemosForFiscalYear.FaBudgetMemoAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.FaEncumbrancePostedAssocMember.HasValue ? glAccountMemosForFiscalYear.FaEncumbrancePostedAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.FaEncumbranceMemoAssocMember.HasValue ? glAccountMemosForFiscalYear.FaEncumbranceMemoAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.FaRequisitionMemoAssocMember.HasValue ? glAccountMemosForFiscalYear.FaRequisitionMemoAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.FaActualPostedAssocMember.HasValue ? glAccountMemosForFiscalYear.FaActualPostedAssocMember.Value : 0m;
                                    glAccountValidationResponse.RemainingBalance -= glAccountMemosForFiscalYear.FaActualMemoAssocMember.HasValue ? glAccountMemosForFiscalYear.FaActualMemoAssocMember.Value : 0m;
                                }
                                else
                                {
                                    // If the GL account is a poolee, throw an error.
                                    glAccountValidationResponse.Status = "failure";
                                    glAccountValidationResponse.ErrorMessage = "A poolee type GL account is not allowed in budget adjustments.";
                                    return glAccountValidationResponse;
                                }
                            }
                        }
                    }
                }

                return glAccountValidationResponse;
            }

            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw;
            }
        }

        #region Private stuff
        private class ComponentDescription
        {
            public string Id { get; set; }
            public string Description { get; set; }
        }

        private void UpdateGlAccountDescriptions(ref Dictionary<string, string> glAccounts, GeneralLedgerComponent glComponent, IEnumerable<ComponentDescription> componentDescriptions)
        {
            foreach (var componentDescription in componentDescriptions)
            {
                var glAccountsWithComponentId = glAccounts.Where(x =>
                    x.Key.Substring(glComponent.StartPosition, glComponent.ComponentLength) == componentDescription.Id).ToList();
                foreach (var glAccountNumber in glAccountsWithComponentId)
                {
                    if (!string.IsNullOrEmpty(glAccountNumber.Value))
                    {
                        glAccounts[glAccountNumber.Key] += " : ";
                    }
                    glAccounts[glAccountNumber.Key] += componentDescription.Description;
                }
            }
        }
        #endregion
    }
}