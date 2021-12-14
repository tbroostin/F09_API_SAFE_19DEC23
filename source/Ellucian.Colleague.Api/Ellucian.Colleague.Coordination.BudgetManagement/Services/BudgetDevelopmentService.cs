// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.BudgetManagement.Adapters;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    /// <summary>
    /// Provider for Budget Development configuration services.
    /// </summary>
    [RegisterType]
    public class BudgetDevelopmentService : BaseCoordinationService, IBudgetDevelopmentService
    {
        private IBudgetDevelopmentRepository budgetDevelopmentRepository;
        private IBudgetDevelopmentConfigurationRepository budgetConfigurationRepository;
        private IGeneralLedgerAccountRepository generalLedgerAccountRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;

        // Constructor for the Budget Development service.
        public BudgetDevelopmentService(IBudgetDevelopmentRepository budgetDevelopmentRepository,
            IBudgetDevelopmentConfigurationRepository budgetConfigurationRepository,
            IGeneralLedgerAccountRepository generalLedgerAccountRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.budgetDevelopmentRepository = budgetDevelopmentRepository;
            this.budgetConfigurationRepository = budgetConfigurationRepository;
            this.generalLedgerAccountRepository = generalLedgerAccountRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
        }

        /// <summary>
        /// Get the working budget.
        /// </summary>
        /// <param name="criteria">Working budget filter query criteria.</param>
        /// <returns>Working budget DTO with a set of filtered budget line items.</returns>
        public async Task<WorkingBudget2> QueryWorkingBudget2Async(WorkingBudgetQueryCriteria criteria)
        {
            // The query criteria can be empty, but it cannot be null.
            if (criteria == null)
            {
                logger.Debug("==> Filter component criteria (criteria) is null. <==");
                throw new ArgumentNullException("criteria", "Filter component criteria must be specified.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                logger.Debug("==> GL account structure (glAccountStructure) is null. <==");
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                logger.Debug("==> GL class configuration (glClassConfiguration) is null. <==");
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            // Initialize the DTO that is to be returned.
            var workingBudgetDto = new WorkingBudget2();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            logger.Debug(string.Format("==> Working Budget ID (After GetBudgetDevelopmentConfigurationAsync) {0}. <==", buDevConfigurationEntity.BudgetId));

            // If there is a working budget defined and a person ID defined for the current user, then get the working
            // budget information for the current user, and if there information for the user convert it into a DTO.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                // Convert the filter criteria DTO into a domain entity, and pass it into the repository.
                var workingBudgetCriteriaAdapter = new WorkingBudgetQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
                var queryCriteriaEntity = workingBudgetCriteriaAdapter.MapToType(criteria);

                // Get the working budget information for the current user.
                var workingBudgetEntity = await budgetDevelopmentRepository.GetBudgetDevelopmentWorkingBudget2Async(buDevConfigurationEntity.BudgetId,
                    buDevConfigurationEntity.BudgetConfigurationComparables, queryCriteriaEntity, CurrentUser.PersonId, glAccountStructure, queryCriteriaEntity.StartLineItem, queryCriteriaEntity.LineItemCount);

                if (workingBudgetEntity != null)
                {
                    // Assign the GL account descriptions to the budget line item entities.
                    var allBudgetLineItemEntities = workingBudgetEntity.LineItems.Where(x => x.BudgetLineItem != null).Select(x => x.BudgetLineItem);

                    // Get the descriptions for the GL accounts returned from the object repository.
                    var glAccountDescriptions = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(allBudgetLineItemEntities.Select(x => x.BudgetAccountId).ToList(), glAccountStructure);

                    foreach (var budgetLineItem in allBudgetLineItemEntities)
                    {
                        if (budgetLineItem != null)
                        {
                            // Get the GL account description.
                            string description = "";
                            if (!string.IsNullOrEmpty(budgetLineItem.BudgetAccountId))
                            {
                                glAccountDescriptions.TryGetValue(budgetLineItem.BudgetAccountId, out description);
                            }
                            budgetLineItem.BudgetAccountDescription = description ?? string.Empty;

                            // Obtain the budget line item GL class (revenue or expense).
                            var lineItemGlClass = GlAccountUtility.GetGlAccountGlClass(budgetLineItem.BudgetAccountId, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);

                            // Assign the budget line item account GL class.
                            // GlClass is an enum, so it'll never be null.
                            budgetLineItem.GlClass = lineItemGlClass;
                        }
                    }

                    // Populate the GL component/subcomponent description on any GL type subtotal line.

                    // Obtain the component/subcomponent names that are used for subtotals from the criteria.
                    if (criteria.SortSubtotalComponentQueryCriteria != null && criteria.SortSubtotalComponentQueryCriteria.Any())
                    {
                        var subtotalNamesFromCriteria = criteria.SortSubtotalComponentQueryCriteria.Where(sb => sb.IsDisplaySubTotal == true && sb.SubtotalType == "GL").Select(sb => sb.SubtotalName);
                        if (subtotalNamesFromCriteria != null && subtotalNamesFromCriteria.Any())
                        {
                            // Loop through the subtotal names in the criteria.
                            foreach (var subtotalName in subtotalNamesFromCriteria)
                            {
                                // Get all the subtotal line items for that subtotal name. There maybe not be any on this one page that is returned to the user.
                                var subtotalNameLineItems = workingBudgetEntity.LineItems.Where(st => st.SubtotalLineItem != null && st.SubtotalLineItem.SubtotalName.ToUpperInvariant() == subtotalName.ToUpperInvariant()).Select(st => st.SubtotalLineItem);
                                if (subtotalNameLineItems != null && subtotalNameLineItems.Any())
                                {
                                    // Get the component that matches the name from the GL account structure to obtain the component type.
                                    var subtotalLineComponent = glAccountStructure.Subcomponents.Where(sc => sc.ComponentName.ToUpperInvariant() == subtotalName.ToUpperInvariant()).FirstOrDefault();
                                    List<string> glComponentIds = new List<string>();

                                    // Loop through each subtotal line item for this subtotal name.
                                    foreach (var subtotalLine in subtotalNameLineItems)
                                    {
                                        // Put all the IDs for this component name in a list.
                                        glComponentIds.Add(subtotalLine.SubtotalValue);
                                    }

                                    if (glComponentIds.Count() > 0)
                                    {
                                        List<string> uniqueGlComponentIds = glComponentIds.Distinct().ToList();

                                        // Use the component type to know what file to read in the repository method.
                                        // This method returns a dictionary of ID, description strings.
                                        var subtotalNameIdsDescriptionDictionary = await generalLedgerAccountRepository.GetGlComponentDescriptionsByIdsAndComponentTypeAsync(uniqueGlComponentIds, subtotalLineComponent.ComponentType);
                                        if (subtotalNameIdsDescriptionDictionary.Any())
                                        {
                                            // Loop again through each of the subtotal line items for this subtotal name.
                                            foreach (var subtotalLine in subtotalNameLineItems)
                                            {
                                                string description = string.Empty;
                                                // If the subtotal line value matches an entry, populate the description.
                                                if (subtotalNameIdsDescriptionDictionary.TryGetValue(subtotalLine.SubtotalValue, out description))
                                                {
                                                    subtotalLine.SubtotalDescription = description;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Debug("==> subtotalNamesFromCriteria is null or empty. <==");
                        }
                    }
                    else
                    {
                        logger.Debug("==> criteria.SortSubtotalComponentQueryCriteria is null or empty. <==");
                    }

                    // define an adapter and convert the working budget domain entity to the working budget DTO.
                    var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget2, Dtos.BudgetManagement.WorkingBudget2>();

                    workingBudgetDto = adapter.MapToType(workingBudgetEntity);
                }
                else
                {
                    logger.Debug("==> workingBudgetEntity (After GetBudgetDevelopmentWorkingBudget2Async) is null. <==");
                }
            }
            else
            {
                logger.Debug("==> Either buDevConfigurationEntity.BudgetId or CurrentUser.PersonId is null or empty. <==");
            }

            return workingBudgetDto;
        }

        /// <summary>
        /// Updates the working budget for a user.
        /// </summary>
        /// <param name="budgetLineItems">A list of budget line items for which the working budget amount will be updated.</param>
        /// <returns>A list of budget line items that have been updated for the working budget.</returns>
        public async Task<List<BudgetLineItem>> UpdateBudgetDevelopmentWorkingBudgetAsync(List<BudgetLineItem> budgetLineItems)
        {
            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            // Initialize the DTO that is to be returned.
            var budgetLineItemsDto = new List<BudgetLineItem>();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            // If there is a working budget defined on BDVP and there a person ID defined for the current user, then try
            // to update the budget line items that have been passed into the method, and then return the list of budget
            // line item DTOs that have been updated.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                List<string> glUpdateList = new List<string>();
                List<long?> newAmountUpdateList = new List<long?>();
                List<string> justificationNotes = new List<string>();

                // Determine which line items exists and if the budget line item is one that the user
                // is allowed to update based on whether they are the budget officer for the budget line item.
                List<string> lineItemAccounts = budgetLineItems.Select(x => x.BudgetAccountId).ToList();
                var workingBudgetLineItemEntities = await budgetDevelopmentRepository.GetBudgetDevelopmentBudgetLineItemsAsync(buDevConfigurationEntity.BudgetId, CurrentUser.PersonId, lineItemAccounts);
                if (workingBudgetLineItemEntities != null && workingBudgetLineItemEntities.Any())
                {
                    var validLineItemAccounts = workingBudgetLineItemEntities.Select(x => x.BudgetAccountId);
                    if (validLineItemAccounts != null && validLineItemAccounts.Any())
                    {
                        foreach (var lineItem in budgetLineItems)
                        {
                            if (validLineItemAccounts.Contains(lineItem.BudgetAccountId))
                            {
                                glUpdateList.Add(lineItem.BudgetAccountId);
                                newAmountUpdateList.Add(lineItem.WorkingAmount);
                                justificationNotes.Add(lineItem.JustificationNotes);
                            }
                        }
                    }
                }

                if (glUpdateList.Any())
                {
                    var updatedBudgetLineItemEntities = await budgetDevelopmentRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(CurrentUser.PersonId, buDevConfigurationEntity.BudgetId, glUpdateList, newAmountUpdateList, justificationNotes);

                    if (updatedBudgetLineItemEntities != null)
                    {
                        foreach (var entity in updatedBudgetLineItemEntities)
                        {
                            if (entity != null)
                            {
                                // define an adapter and convert the budget line item domain entity to the budget line item DTO.
                                var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>();

                                var budgetLineItemDto = adapter.MapToType(entity);

                                budgetLineItemsDto.Add(budgetLineItemDto);
                            }
                        }
                    }
                }
            }
            return budgetLineItemsDto;
        }

        /// <summary>
        /// Get the budget officers for the working budget.
        /// </summary>
        /// <returns>List of budget officer DTOs.</returns>
        public async Task<List<BudgetOfficer>> GetBudgetDevelopmentBudgetOfficersAsync()
        {
            // Initialize the DTO that is to be returned.
            var workingBudgetBudgetOfficerDto = new List<BudgetOfficer>();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            // If there is a working budget defined and a person ID defined for the current user, then get the working
            // budget information for the current user, and if there information for the user convert it into a DTO.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                // Get the budget officers for the working budget for the current user.
                var workingBudgetBudgetOfficerEntities = await budgetDevelopmentRepository.GetBudgetDevelopmentBudgetOfficersAsync(buDevConfigurationEntity.BudgetId, CurrentUser.PersonId);

                if (workingBudgetBudgetOfficerEntities != null && workingBudgetBudgetOfficerEntities.Any())
                {
                    // define an adapter and convert the working budget domain entity to the working budget DTO.
                    var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>();
                    foreach (var ofcrEntity in workingBudgetBudgetOfficerEntities)
                    {
                        var budgetOfficerDto = adapter.MapToType(ofcrEntity);
                        workingBudgetBudgetOfficerDto.Add(budgetOfficerDto);
                    }
                }
            }

            return workingBudgetBudgetOfficerDto;
        }

        /// <summary>
        /// Get the reporting units for the user in the working budget.
        /// </summary>
        /// <returns>List of budget reporting unit DTOs.</returns>
        public async Task<List<BudgetReportingUnit>> GetBudgetDevelopmentReportingUnitsAsync()
        {
            // Initialize the DTO that is to be returned.
            List<BudgetReportingUnit> workingBudgetReportingUnitDtos = new List<BudgetReportingUnit>();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            // If there is a working budget defined and a person ID defined for the current user, then get the working
            // budget information for the current user, and if there information for the user convert it into a DTO.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                // Get the reporting units for the current user for the working budget.
                var workingBudgetReportingUnitEntities = await budgetDevelopmentRepository.GetBudgetDevelopmentReportingUnitsAsync(buDevConfigurationEntity.BudgetId, CurrentUser.PersonId);

                if (workingBudgetReportingUnitEntities != null && workingBudgetReportingUnitEntities.Any())
                {
                    // define an adapter and convert the working budget domain entity to the working budget DTO.
                    var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.BudgetReportingUnit, Dtos.BudgetManagement.BudgetReportingUnit>();
                    foreach (var unitEntity in workingBudgetReportingUnitEntities)
                    {
                        var reportingUnitDto = adapter.MapToType(unitEntity);
                        workingBudgetReportingUnitDtos.Add(reportingUnitDto);
                    }
                }
            }

            return workingBudgetReportingUnitDtos;
        }




        //////////////////////////////////////////////////////
        //                                                  //
        //               DEPRECATED / OBSOLETE              //
        //                                                  //
        //////////////////////////////////////////////////////


        #region DEPRECATED / OBSOLETE

        /// <summary>
        /// Get the working budget.
        /// </summary>
        /// <param name="startPosition">Start position of the budget line items to return.</param>
        /// <param name="recordCount">Number of budget line items to return.</param>
        /// <returns>Working budget DTO with a set of budget line items.</returns>
        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(int startPosition, int recordCount)
        {
            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            // Initialize the DTO that is to be returned.
            var workingBudgetDto = new WorkingBudget();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            // If there is a working budget defined and a person ID defined for the current user, then get the working
            // budget information for the current user, and if there information for the user convert it into a DTO.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                // Get the working budget information for the current user.
                var workingBudgetEntity = await budgetDevelopmentRepository.GetBudgetDevelopmentWorkingBudgetAsync(buDevConfigurationEntity.BudgetId, buDevConfigurationEntity.BudgetConfigurationComparables, null, CurrentUser.PersonId, glAccountStructure.MajorComponentStartPositions, startPosition, recordCount);

                if (workingBudgetEntity != null)
                {
                    // Assign the GL account descriptions to the budget line item entities.
                    var allBudgetLineItemEntities = workingBudgetEntity.BudgetLineItems;

                    // Get the descriptions for the GL accounts returned from the object repository.
                    var glAccountDescriptions = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(allBudgetLineItemEntities.Select(x => x.BudgetAccountId).ToList(), glAccountStructure);

                    foreach (var budgetLineItem in allBudgetLineItemEntities)
                    {
                        if (budgetLineItem != null)
                        {
                            // Get the GL number description.
                            string description = "";
                            if (!string.IsNullOrEmpty(budgetLineItem.BudgetAccountId))
                            {
                                glAccountDescriptions.TryGetValue(budgetLineItem.BudgetAccountId, out description);
                            }
                            budgetLineItem.BudgetAccountDescription = description ?? string.Empty;

                            // Reverse the budget line item base amount, working amount and comparable amounts, if the budget line item account is a revenue account.
                            var lineItemGlClass = GlAccountUtility.GetGlAccountGlClass(budgetLineItem.BudgetAccountId, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);
                            if (lineItemGlClass == Domain.ColleagueFinance.Entities.GlClass.Revenue)
                            {
                                budgetLineItem.BaseBudgetAmount = -budgetLineItem.BaseBudgetAmount;
                                budgetLineItem.WorkingAmount = -budgetLineItem.WorkingAmount;
                                foreach (var comp in budgetLineItem.BudgetComparables)
                                {
                                    comp.ComparableAmount = -comp.ComparableAmount;
                                }
                            }
                        }
                    }

                    // define an adapter and convert the working budget domain entity to the working budget DTO.
                    var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>();

                    workingBudgetDto = adapter.MapToType(workingBudgetEntity);
                }
            }

            return workingBudgetDto;
        }

        /// <summary>
        /// Get the working budget.
        /// </summary>
        /// <param name="criteria">Working budget filter query criteria.</param>
        /// <returns>Working budget DTO with a set of filtered budget line items.</returns>
        [Obsolete("Obsolete as of Colleague Web API 1.25. Use QueryWorkingBudget2Async")]
        public async Task<WorkingBudget> QueryWorkingBudgetAsync(WorkingBudgetQueryCriteria criteria)
        {
            // The query criteria can be empty, but it cannot be null.
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "Filter component criteria must be specified.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null)
            {
                throw new ApplicationException("GL account structure is not set up.");
            }

            // Get the GL class configuration.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            // Initialize the DTO that is to be returned.
            var workingBudgetDto = new WorkingBudget();

            // Get the working budget defined on BDVP in Colleague.
            var buDevConfigurationEntity = await budgetConfigurationRepository.GetBudgetDevelopmentConfigurationAsync();

            // If there is a working budget defined and a person ID defined for the current user, then get the working
            // budget information for the current user, and if there information for the user convert it into a DTO.
            if (!string.IsNullOrEmpty(buDevConfigurationEntity.BudgetId) && !string.IsNullOrEmpty(CurrentUser.PersonId))
            {
                // Convert the filter criteria DTO into a domain entity, and pass it into the repository.
                var workingBudgetCriteriaAdapter = new WorkingBudgetQueryCriteriaDtoToEntityAdapter(_adapterRegistry, logger);
                var queryCriteriaEntity = workingBudgetCriteriaAdapter.MapToType(criteria);

                // Get the working budget information for the current user.
                var workingBudgetEntity = await budgetDevelopmentRepository.GetBudgetDevelopmentWorkingBudgetAsync(buDevConfigurationEntity.BudgetId,
                    buDevConfigurationEntity.BudgetConfigurationComparables, queryCriteriaEntity, CurrentUser.PersonId, glAccountStructure.MajorComponentStartPositions, queryCriteriaEntity.StartLineItem, queryCriteriaEntity.LineItemCount);

                if (workingBudgetEntity != null)
                {
                    // Assign the GL account descriptions to the budget line item entities.
                    var allBudgetLineItemEntities = workingBudgetEntity.BudgetLineItems;

                    // Get the descriptions for the GL accounts returned from the object repository.
                    var glAccountDescriptions = await generalLedgerAccountRepository.GetGlAccountDescriptionsAsync(allBudgetLineItemEntities.Select(x => x.BudgetAccountId).ToList(), glAccountStructure);

                    foreach (var budgetLineItem in allBudgetLineItemEntities)
                    {
                        if (budgetLineItem != null)
                        {
                            // Get the GL number description.
                            string description = "";
                            if (!string.IsNullOrEmpty(budgetLineItem.BudgetAccountId))
                            {
                                glAccountDescriptions.TryGetValue(budgetLineItem.BudgetAccountId, out description);
                            }
                            budgetLineItem.BudgetAccountDescription = description ?? string.Empty;

                            // Reverse the budget line item base amount, working amount and comparable amounts, if the budget line item account is a revenue account.
                            var lineItemGlClass = GlAccountUtility.GetGlAccountGlClass(budgetLineItem.BudgetAccountId, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);

                            // Assign the budget line item account GL class.
                            // GlClass is an enum, so it'll never be null.
                            budgetLineItem.GlClass = lineItemGlClass;
                            if (lineItemGlClass == Domain.ColleagueFinance.Entities.GlClass.Revenue)
                            {
                                budgetLineItem.BaseBudgetAmount = -budgetLineItem.BaseBudgetAmount;
                                budgetLineItem.WorkingAmount = -budgetLineItem.WorkingAmount;
                                foreach (var comp in budgetLineItem.BudgetComparables)
                                {
                                    comp.ComparableAmount = -comp.ComparableAmount;
                                }
                            }
                        }
                    }

                    // define an adapter and convert the working budget domain entity to the working budget DTO.
                    var adapter = _adapterRegistry.GetAdapter<Domain.BudgetManagement.Entities.WorkingBudget, Dtos.BudgetManagement.WorkingBudget>();

                    workingBudgetDto = adapter.MapToType(workingBudgetEntity);
                }
            }

            return workingBudgetDto;
        }

        #endregion

    }
}
