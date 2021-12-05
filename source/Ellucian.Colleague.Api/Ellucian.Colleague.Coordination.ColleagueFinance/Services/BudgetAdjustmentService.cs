// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Service for budget adjustments.
    /// </summary>
    [RegisterType]
    public class BudgetAdjustmentService : BaseCoordinationService, IBudgetAdjustmentService
    {
        private readonly IBudgetAdjustmentsRepository budgetAdjustmentRepository;
        private readonly IGeneralLedgerConfigurationRepository configurationRepository;
        private readonly IApproverRepository approverRepository;
        private readonly IStaffRepository staffRepository;
        private readonly IGeneralLedgerUserRepository generalLedgerUserRepository;

        /// <summary>
        /// Initialize the service.
        /// </summary>
        /// <param name="budgetAdjustmentRepository">Budget adjustment repository.</param>
        /// <param name="configurationRepository">GL configuration repository.</param>
        /// <param name="approverRepository">Approver repository.</param>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="currentUserFactory">User factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger object</param>
        public BudgetAdjustmentService(IBudgetAdjustmentsRepository budgetAdjustmentRepository, IGeneralLedgerConfigurationRepository configurationRepository, IGeneralLedgerUserRepository generalLedgerUserRepository,
            IApproverRepository approverRepository, IStaffRepository staffRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.budgetAdjustmentRepository = budgetAdjustmentRepository;
            this.configurationRepository = configurationRepository;
            this.approverRepository = approverRepository;
            this.staffRepository = staffRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
        }

        /// <summary>
        /// Create a new budget adjustment.
        /// </summary>
        /// <param name="budgetAdjustmentDto">Budget adjustment DTO.</param>
        /// <returns>Budget adjustment DTO</returns>
        public async Task<BudgetAdjustment> CreateBudgetAdjustmentAsync(BudgetAdjustment budgetAdjustmentDto)
        {
            if (budgetAdjustmentDto == null)
            {
                throw new ArgumentNullException("budgetAdjustmentDto", "Budget adjustment is required.");
            }

            // Check the permission code to create or update a budget adjustment, whether it is a draft or not.
            CheckCreateUpdateBudgetAdjustmentPermission();

            #region Get configuration data

            var exclusions = await configurationRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            if (exclusions == null)
            {
                throw new ApplicationException("Error retrieving budget adjustment exclusions.");
            }

            var glClassConfiguration = await configurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            var costCenterStructure = await this.configurationRepository.GetCostCenterStructureAsync();
            if (costCenterStructure == null || !costCenterStructure.CostCenterComponents.Any())
            {
                throw new ConfigurationException("Cost center structure not defined.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await configurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null || !glAccountStructure.MajorComponentStartPositions.Any())
            {
                throw new ConfigurationException("Account structure must be defined.");
            }

            #endregion

            // Initialize the adjustment line entities.
            var adjustmentLineEntities = new List<Domain.ColleagueFinance.Entities.AdjustmentLine>();
            foreach (var adjustmentLineDto in budgetAdjustmentDto.AdjustmentLines)
            {
                try
                {
                    adjustmentLineEntities.Add(new Domain.ColleagueFinance.Entities.AdjustmentLine(adjustmentLineDto.GlNumber, adjustmentLineDto.FromAmount, adjustmentLineDto.ToAmount));
                }
                catch (ArgumentNullException anex)
                {
                    logger.Error(anex, anex.Message);
                }
                catch (ArgumentException aex)
                {
                    logger.Error(aex, aex.Message);
                }
            }

            // Initialize the adjustment object.
            var adjustmentInputEntity = new Domain.ColleagueFinance.Entities.BudgetAdjustment(budgetAdjustmentDto.TransactionDate, budgetAdjustmentDto.Reason, CurrentUser.PersonId, adjustmentLineEntities);
            adjustmentInputEntity.Comments = budgetAdjustmentDto.Comments;
            adjustmentInputEntity.Initiator = budgetAdjustmentDto.Initiator;
            adjustmentInputEntity.DraftBudgetAdjustmentId = budgetAdjustmentDto.DraftBudgetAdjustmentId;

            // Get the adapter for the next approver and assign them to the domain entity.
            var adapterIn = _adapterRegistry.GetAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>();

            adjustmentInputEntity.NextApprovers = new List<Domain.ColleagueFinance.Entities.NextApprover>();
            if (adjustmentInputEntity.NextApprovers != null && budgetAdjustmentDto.NextApprovers != null)
            {
                foreach (var nextApprover in budgetAdjustmentDto.NextApprovers)
                {
                    var nextApproverDomain = adapterIn.MapToType(nextApprover);
                    adjustmentInputEntity.NextApprovers.Add(nextApproverDomain);
                }
            }

            // See if any of the GL accounts in the adjustment are not allowed.
            var restrictionMessages = GlAccountUtility.EvaluateExclusionsForBudgetAdjustment(adjustmentInputEntity.AdjustmentLines.Select(x => x.GlNumber).ToList(), exclusions, glAccountStructure.MajorComponentStartPositions);
            if (restrictionMessages != null && restrictionMessages.Any())
            {
                throw new ApplicationException(String.Join("<>", restrictionMessages));
            }

            // Validate only expense type GL accounts are included
            foreach (var adjustmentLine in adjustmentLineEntities)
            {
                if (adjustmentLine != null)
                {
                    var glClass = GlAccountUtility.GetGlAccountGlClass(adjustmentLine.GlNumber, glClassConfiguration, glAccountStructure.MajorComponentStartPositions);
                    if (glClass != Domain.ColleagueFinance.Entities.GlClass.Expense)
                    {
                        throw new ApplicationException("Only expense type GL accounts are allowed in a Budget Adjustment.");
                    }
                }
            }

            // Obtain the parameter that determines if the same cost center is required.
            var budgetAdjustmentParameters = await this.configurationRepository.GetBudgetAdjustmentParametersAsync();

            // If it is true, evaluate the cost centers.
            if (budgetAdjustmentParameters.SameCostCenterRequired)
            {
                // Make sure we're dealing with only a single cost center.
                var costCenterIds = adjustmentInputEntity.AdjustmentLines.Select(x => GlAccountUtility.GetCostCenterId(x.GlNumber, costCenterStructure, glAccountStructure.MajorComponentStartPositions)).Distinct().ToList();
                if (costCenterIds.Count() > 1)
                {
                    throw new ApplicationException("GL accounts must be from the same cost center.");
                }
            }

            var adjustmentOutputEntity = await budgetAdjustmentRepository.CreateAsync(adjustmentInputEntity, glAccountStructure.MajorComponentStartPositions);

            if (adjustmentOutputEntity == null)
            {
                throw new ApplicationException("Budget adjustment must not be null.");
            }

            // If error exists, throw exception with message.
            if (adjustmentOutputEntity.ErrorMessages != null && adjustmentOutputEntity.ErrorMessages.Count > 0)
            {
                throw new ApplicationException(String.Join("<>", adjustmentOutputEntity.ErrorMessages));
            }

            if (string.IsNullOrEmpty(adjustmentOutputEntity.Id))
            {
                throw new ApplicationException("Adjustment appears to have succeeded, but no ID was returned.");
            }

            // Initialize the DTO to return.
            var adjustmentLineDtos = new List<AdjustmentLine>();
            foreach (var adjustmentLineEntity in adjustmentOutputEntity.AdjustmentLines)
            {
                adjustmentLineDtos.Add(new AdjustmentLine()
                {
                    GlNumber = adjustmentLineEntity.GlNumber,
                    FromAmount = adjustmentLineEntity.FromAmount,
                    ToAmount = adjustmentLineEntity.ToAmount
                });
            }

            var adjustmentOutputDto = new BudgetAdjustment()
            {
                Id = adjustmentOutputEntity.Id,
                AdjustmentLines = adjustmentLineDtos,
                Comments = adjustmentOutputEntity.Comments,
                Initiator = adjustmentOutputEntity.Initiator,
                Reason = adjustmentOutputEntity.Reason,
                TransactionDate = adjustmentOutputEntity.TransactionDate,
                DraftDeletionSuccessfulOrUnnecessary = adjustmentOutputEntity.DraftDeletionSuccessfulOrUnnecessary,
            };

            // Get the adapter for the next approver and assign them to the dto.
            var adapterOut = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>();

            adjustmentOutputDto.NextApprovers = new List<NextApprover>();
            if (adjustmentOutputDto.NextApprovers != null && adjustmentInputEntity.NextApprovers != null)
                foreach (var nextApprover in adjustmentInputEntity.NextApprovers)
                {
                    var nextApproverDto = adapterOut.MapToType(nextApprover);
                    adjustmentOutputDto.NextApprovers.Add(nextApproverDto);
                }

            return adjustmentOutputDto;
        }

        /// <summary>
        /// Update an existing budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to be updated.</param>
        /// <param name="budgetAdjustmentDto">The new budget adjustment data.</param>
        /// <returns>The updated budget adjustment data.</returns>
        public async Task<BudgetAdjustment> UpdateBudgetAdjustmentAsync(string id, BudgetAdjustment budgetAdjustmentDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (budgetAdjustmentDto == null)
            {
                throw new ArgumentNullException("budgetAdjustmentDto", "Budget adjustment is required.");
            }

            // Check the permission code to create or update a budget adjustment, whether it is a draft or not.
            CheckCreateUpdateBudgetAdjustmentPermission();

            #region Get configuration data
            var exclusions = await configurationRepository.GetBudgetAdjustmentAccountExclusionsAsync();
            if (exclusions == null)
            {
                throw new ApplicationException("Error retrieving budget adjustment exclusions.");
            }

            var glClassConfiguration = await configurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ApplicationException("Error retrieving GL class configuration.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await configurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null || !glAccountStructure.MajorComponentStartPositions.Any())
            {
                throw new ConfigurationException("Account structure must be defined.");
            }

            var costCenterStructure = await this.configurationRepository.GetCostCenterStructureAsync();
            if (costCenterStructure == null || !costCenterStructure.CostCenterComponents.Any())
            {
                throw new ConfigurationException("Cost center structure not defined.");
            }
            #endregion

            var budgetAdjustmentEntityToUpdate = await budgetAdjustmentRepository.GetBudgetAdjustmentAsync(id);

            // Updates are only permitted for existing budget adjustments that are in "Not Approved" status.
            if (budgetAdjustmentEntityToUpdate != null && budgetAdjustmentEntityToUpdate.Status != Domain.ColleagueFinance.Entities.BudgetEntryStatus.Complete)
            {
                // Get the adapter for the next approver and assign them to the domain entity.
                var adapterIn = _adapterRegistry.GetAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>();

                // Update the list of next approvers because that is all we are allowed to write that's different from the database.
                budgetAdjustmentEntityToUpdate.NextApprovers = new List<Domain.ColleagueFinance.Entities.NextApprover>();
                if (budgetAdjustmentEntityToUpdate.NextApprovers != null && budgetAdjustmentDto.NextApprovers != null)
                {
                    foreach (var nextApprover in budgetAdjustmentDto.NextApprovers)
                    {
                        var nextApproverDomain = adapterIn.MapToType(nextApprover);
                        budgetAdjustmentEntityToUpdate.NextApprovers.Add(nextApproverDomain);
                    }
                }

                var adjustmentOutputEntity = await budgetAdjustmentRepository.UpdateAsync(id, budgetAdjustmentEntityToUpdate, glAccountStructure.MajorComponentStartPositions);

                if (adjustmentOutputEntity == null)
                {
                    throw new ApplicationException("Budget adjustment must not be null.");
                }

                // If error exists, throw exception with message.
                if (adjustmentOutputEntity.ErrorMessages != null && adjustmentOutputEntity.ErrorMessages.Count > 0)
                {
                    throw new ApplicationException(String.Join("<>", adjustmentOutputEntity.ErrorMessages));
                }

                if (string.IsNullOrEmpty(adjustmentOutputEntity.Id))
                {
                    throw new ApplicationException("Budget Adjustment update appears to have succeeded, but no ID was returned.");
                }

                // Initialize the DTO to return.
                var adjustmentLineDtos = new List<AdjustmentLine>();
                if (adjustmentOutputEntity.AdjustmentLines != null)
                {
                    foreach (var adjustmentLineEntity in adjustmentOutputEntity.AdjustmentLines)
                    {
                        adjustmentLineDtos.Add(new AdjustmentLine()
                        {
                            GlNumber = adjustmentLineEntity.GlNumber,
                            FromAmount = adjustmentLineEntity.FromAmount,
                            ToAmount = adjustmentLineEntity.ToAmount
                        });
                    }
                }

                var adjustmentOutputDto = new BudgetAdjustment()
                {
                    Id = adjustmentOutputEntity.Id,
                    AdjustmentLines = adjustmentLineDtos,
                    Comments = adjustmentOutputEntity.Comments,
                    Initiator = adjustmentOutputEntity.Initiator,
                    Reason = adjustmentOutputEntity.Reason,
                    TransactionDate = adjustmentOutputEntity.TransactionDate,
                    DraftDeletionSuccessfulOrUnnecessary = adjustmentOutputEntity.DraftDeletionSuccessfulOrUnnecessary,
                };

                // Get the adapter for the next approver and assign them to the dto.
                var adapterOut = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>();

                adjustmentOutputDto.NextApprovers = new List<NextApprover>();
                if (adjustmentOutputDto.NextApprovers != null && budgetAdjustmentEntityToUpdate.NextApprovers != null)
                {
                    foreach (var nextApprover in budgetAdjustmentEntityToUpdate.NextApprovers)
                    {
                        var nextApproverDto = adapterOut.MapToType(nextApprover);
                        adjustmentOutputDto.NextApprovers.Add(nextApproverDto);
                    }
                }

                // Get the adapter for the approver and assign them to the dto.
                var approverAdapterOut = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>();

                adjustmentOutputDto.Approvers = new List<Approver>();
                if (adjustmentOutputDto.Approvers != null && budgetAdjustmentEntityToUpdate.Approvers != null)
                {
                    foreach (var approver in budgetAdjustmentEntityToUpdate.Approvers)
                    {
                        var approverDto = approverAdapterOut.MapToType(approver);
                        adjustmentOutputDto.Approvers.Add(approverDto);
                    }
                }
                return adjustmentOutputDto;
            }
            else
            {
                throw new ApplicationException("Invalid update request.");
            }
        }

        /// <summary>
        /// Get a budget adjustment by its id. It is invoked by the initiator 
        /// when they click to see or update a budget adjustment.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to retrieve.</param>
        /// <returns>A budget adjustment domain entity.</returns>
        /// <exception><see cref="PermissionsException">KeyNotFoundExceptionPermissionsException</see></exception>
        public async Task<BudgetAdjustment> GetBudgetAdjustmentAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Check the permission code to view a budget adjustment, whether it is a draft or not.
            CheckViewBudgetAdjustmentPermission();

            var budgetAdjustment = await budgetAdjustmentRepository.GetBudgetAdjustmentAsync(id);
            var budgetAdjustmentDto = new BudgetAdjustment();

            if (budgetAdjustment != null)
            {
                // Validate that the current user is the same person ID as the one obtained from the repository.
                if (!CurrentUser.IsPerson(budgetAdjustment.PersonId))
                {
                    var message = "The current user " + CurrentUser.PersonId + " is not the person that owns the record returned from the repository";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }

                // Convert the domain entity into a DTO.
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>();
                budgetAdjustmentDto = adapter.MapToType(budgetAdjustment);

                // Run validation if it is not a complete budget adjustment (which may return errors in the Validation Results property of the DTO).
                if (budgetAdjustment.Status != Domain.ColleagueFinance.Entities.BudgetEntryStatus.Complete)
                {
                    await ValidateBudgetAdjustmentAsync(budgetAdjustment, budgetAdjustmentDto);
                }
            }

            return budgetAdjustmentDto;
        }

        /// <summary>
        /// Gets a budget adjustment that is awaiting approval by its id. It is invoked
        /// by an approver when they retrieve a budget adjustment to see it or approve it.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to retrieve.</param>
        /// <returns>A budget adjustment DTO.</returns>
        public async Task<BudgetAdjustment> GetBudgetAdjustmentPendingApprovalDetailAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            // Check the permission code to view pending approval budget adjustments.
            CheckViewBudgetAdjustmentPendingApprovalPermission();

            // Get the approver/login id for the current user.
            var currentUserStaffLoginId = string.Empty;
            try
            {
                currentUserStaffLoginId = await staffRepository.GetStaffLoginIdForPersonAsync(CurrentUser.PersonId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Could not locate Staff Login ID for person ID: " + CurrentUser.PersonId);
                throw new PermissionsException("Could not find Staff information for the user.");
            }

            var budgetAdjustment = await budgetAdjustmentRepository.GetBudgetAdjustmentAsync(id);
            var budgetAdjustmentDto = new BudgetAdjustment();

            if (budgetAdjustment != null)
            {
                // Validate that the current user is listed as a "next approver" on the entity obtained from the repository.
                var nextApproverMatchesCurrentUser = false;
                if (budgetAdjustment.NextApprovers != null)
                {
                    foreach (var nextApprover in budgetAdjustment.NextApprovers)
                    {
                        if (currentUserStaffLoginId == nextApprover.NextApproverId)
                        {
                            nextApproverMatchesCurrentUser = true;
                        }
                    }
                }

                if (!nextApproverMatchesCurrentUser)
                {
                    var message = "The current user " + CurrentUser.PersonId + " is not a next approver for the record returned from the repository";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }

                // Convert the domain entity into a DTO.
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustment, Dtos.ColleagueFinance.BudgetAdjustment>();
                budgetAdjustmentDto = adapter.MapToType(budgetAdjustment);

                // Run validation (which will also update the Validation Results property of the DTO).
                await ValidateBudgetAdjustmentAsync(budgetAdjustment, budgetAdjustmentDto);

                // Get the account structure configuration.
                var glAccountStructure = await configurationRepository.GetAccountStructureAsync();
                if (glAccountStructure == null)
                {
                    throw new ConfigurationException("Account structure must be defined.");
                }

                var glClassConfiguration = await configurationRepository.GetClassConfigurationAsync();
                if (glClassConfiguration == null)
                {
                    throw new ApplicationException("Error retrieving GL class configuration.");
                }

                // Get the ID for the person who is logged in, and use the ID to get his list of assigned expense and revenue GL accounts.
                var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glAccountStructure.FullAccessRole, glClassConfiguration);
                if (generalLedgerUser == null)
                {
                    throw new ApplicationException("GL user must be defined.");
                }

                if (budgetAdjustmentDto != null)
                {

                    var costCenterStructure = await configurationRepository.GetCostCenterStructureAsync();
                    if (costCenterStructure == null)
                    {
                        throw new ConfigurationException("costCenterStructure can not be null.");
                    }

                    List<string> costCenterIds = new List<string>();

                    // Strip out GL accounts that the approver does not have access to based on their GL User access.
                    var expenseAndRevenueAccounts = generalLedgerUser.RevenueAccounts.Union(generalLedgerUser.ExpenseAccounts);

                    foreach (var adjustmentLine in budgetAdjustmentDto.AdjustmentLines)
                    {
                        costCenterIds.Add(GlAccountUtility.GetCostCenterId(adjustmentLine.GlNumber, costCenterStructure, glAccountStructure.MajorComponentStartPositions));

                        if (!expenseAndRevenueAccounts.Contains(adjustmentLine.GlNumber))
                        {
                            adjustmentLine.Hidden = true;
                            adjustmentLine.FromAmount = 0m;
                            adjustmentLine.ToAmount = 0m;
                            adjustmentLine.GlNumber = string.Empty;
                        }
                        else
                        {
                            adjustmentLine.Hidden = false;
                        }
                    }
                    budgetAdjustmentDto.CostCenterCount = costCenterIds.Distinct().Count();
                }

            }

            return budgetAdjustmentDto;
        }

        /// <summary>
        /// Approve a budget adjustment by the current user.
        /// </summary>
        /// <param name="id">The ID of the budget adjustment to approve.</param>
        /// <param name="budgetAdjustmentApprovalDto">The budget adjustment approval data.</param>        
        /// <returns>The same budget adjustment approval DTO passed in.</returns>
        public async Task<Dtos.ColleagueFinance.BudgetAdjustmentApproval> PostBudgetAdjustmentApprovalAsync(string id, Dtos.ColleagueFinance.BudgetAdjustmentApproval budgetAdjustmentApprovalDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (budgetAdjustmentApprovalDto == null)
            {
                throw new ArgumentNullException("budgetAdjustmentApprovalDto", "Budget adjustment approval is required.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await configurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null || !glAccountStructure.MajorComponentStartPositions.Any())
            {
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Check the permission code to view pending approval budget adjustments.
            CheckViewBudgetAdjustmentPendingApprovalPermission();

            // Get the approver/login id for the current user.
            var currentUserStaffLoginId = string.Empty;
            try
            {
                currentUserStaffLoginId = await staffRepository.GetStaffLoginIdForPersonAsync(CurrentUser.PersonId);
            }
            catch (Exception e)
            {
                logger.Error(e, "Could not locate Staff Login ID for person ID: " + CurrentUser.PersonId);
                throw new ApplicationException("Could not find Staff information for the user.");
            }

            // Get the budget adjustment to approve, add the approval information and post it.
            var budgetAdjustmentEntity = await budgetAdjustmentRepository.GetBudgetAdjustmentAsync(id);

            // Updates are only permitted for existing budget adjustments that are in "Not Approved" Status.
            // Check the budget adjustment that was retrieved from the database.
            if (budgetAdjustmentEntity == null)
            {
                logger.Error("Could not retrieve the budget adjustment: " + id);
                throw new ApplicationException("Could not retrieve the budget adjustment.");
            }

            if (budgetAdjustmentEntity.Status != Domain.ColleagueFinance.Entities.BudgetEntryStatus.NotApproved)
            {
                var message = "The budget adjustment does not have a not approved status.";
                logger.Error(message);
                throw new NotApprovedStatusException(message);
            }

            // Throw an exception is the current user has already approved this budget adjustment.
            if (budgetAdjustmentEntity.Approvers != null)
            {
                if (budgetAdjustmentEntity.Approvers.Any())
                {
                    int approverPosition = budgetAdjustmentEntity.Approvers.FindIndex(x => x.ApproverId == currentUserStaffLoginId);
                    if (approverPosition >= 0)
                    {
                        var message = "You have already approved this budget adjustment.";
                        logger.Error(message);
                        throw new AlreadyApprovedByUserException(message);
                    }
                }
            }

            // Validate that the current user is still listed as a "next approver" on the entity obtained from the repository.
            if (budgetAdjustmentEntity.NextApprovers == null || !budgetAdjustmentEntity.NextApprovers.Any())
            {
                var message = "You are no longer a next approver in this budget adjustment.";
                logger.Error(message);
                throw new PermissionsException(message);
            }
            else
            {
                int nextApproverPosition = budgetAdjustmentEntity.NextApprovers.FindIndex(x => x.NextApproverId == currentUserStaffLoginId);
                if (nextApproverPosition < 0)
                {
                    var message = "You are no longer a next approver in this budget adjustment.";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
            }

            // Validate the budget adjustment before posting it.
            // If there are any validation errors, throw an exception.
            var validationMessages = await budgetAdjustmentRepository.ValidateBudgetAdjustmentAsync(budgetAdjustmentEntity, glAccountStructure.MajorComponentStartPositions);
            if (validationMessages != null && validationMessages.Any())
            {
                var message = "The budget adjustment fails validation before posting.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            // Update the list of approvers with the current user's approval.
            var currentApprover = new Domain.ColleagueFinance.Entities.Approver(currentUserStaffLoginId);
            currentApprover.ApprovalDate = DateTime.Today;
            if (budgetAdjustmentEntity.Approvers != null)
            {
                budgetAdjustmentEntity.Approvers.Add(currentApprover);
            }

            // Remove the current user's approval from the list of next approvers.
            int pos = budgetAdjustmentEntity.NextApprovers.FindIndex(x => x.NextApproverId == currentUserStaffLoginId);
            budgetAdjustmentEntity.NextApprovers.RemoveAt(pos);

            // Append the current user's approval name, date, time and added comments to the previously entered comments.
            // Insert an empty line to separate the new comments.
            if (!string.IsNullOrEmpty(budgetAdjustmentEntity.Comments))
            {
                budgetAdjustmentEntity.Comments += DmiString._VM;
            }

            // Get the approver name from the approval ID and append it. 
            // Otherwise append the approver ID. 
            // Make sure the approver name is not longer than 30 because otherwise
            // it will get split when storing in the database.
            string approverName = await approverRepository.GetApproverNameForIdAsync(currentUserStaffLoginId);
            string commentsToAdd = "";

            if (!string.IsNullOrEmpty(approverName))
            {
                if (approverName.Trim().Length > 30)
                {
                    approverName.Substring(0, 30);
                }
                commentsToAdd = approverName;
            }
            else
            {
                commentsToAdd = currentUserStaffLoginId;
            }
            // Append the Date and Time. Count the space in betwen name and date.
            if ((commentsToAdd.Length + DateTime.Now.ToString().Length) > 31)
            {
                commentsToAdd += " " + DateTime.Now.ToString();
            }
            else
            {
                commentsToAdd += DmiString._VM + DateTime.Now.ToString();
            }

            // The dto comments only contains any comments added by the user in this transaction.
            if (!string.IsNullOrEmpty(budgetAdjustmentApprovalDto.Comments))
            {
                commentsToAdd += DmiString._VM + budgetAdjustmentApprovalDto.Comments;
            }

            // We may have line break characters in the existing comments. Split them out and add each line separately
            // to preserve any line-to-line formatting the user entered. Note that these characters could be
            // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
            // any of the new line or carriage returns to the same thing, and then split the string on that.
            string _VM = Convert.ToString(DmiString._VM);
            string alternateNewLineCharacter = "\r\n";
            string newLineCharacter = "\n";
            string carriageReturnCharacter = "\r";
            if (!string.IsNullOrWhiteSpace(budgetAdjustmentEntity.Comments))
            {
                budgetAdjustmentEntity.Comments = budgetAdjustmentEntity.Comments.Replace(alternateNewLineCharacter, _VM);
                budgetAdjustmentEntity.Comments = budgetAdjustmentEntity.Comments.Replace(newLineCharacter, _VM);
                budgetAdjustmentEntity.Comments = budgetAdjustmentEntity.Comments.Replace(carriageReturnCharacter, _VM);

                budgetAdjustmentEntity.Comments += _VM + commentsToAdd;
            }
            else
            {
                budgetAdjustmentEntity.Comments = commentsToAdd;

            }

            // Approve the budget adjustment. The current user approval may not be enough to approve 
            // this budget adjustment, so it may not go to a complete status.
            var budgetAdjustmentOutputEntity = await budgetAdjustmentRepository.UpdateAsync(id, budgetAdjustmentEntity, glAccountStructure.MajorComponentStartPositions);

            if (budgetAdjustmentOutputEntity == null)
            {
                var message = "Budget adjustment must not be null.";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            return budgetAdjustmentApprovalDto;
        }

        /// <summary>
        /// Get the list of budget adjustments summary for the current user.
        /// </summary>
        /// <returns>List of budget adjustment summary DTOs for the specified user.</returns>
        public async Task<IEnumerable<BudgetAdjustmentSummary>> GetBudgetAdjustmentsSummaryAsync()
        {
            // Check the permission code to view a budget adjustment, whether it is a draft or not.
            CheckViewBudgetAdjustmentPermission();

            // Obtain the list of draft and non-draft budget adjustments for the current user.
            var budgetAdjustmentSummaryEntities = await budgetAdjustmentRepository.GetBudgetAdjustmentsSummaryAsync(CurrentUser.PersonId);

            // Initialize the budget adjustment summary dtos.
            var budgetAdjustmentSummaryDtos = new List<BudgetAdjustmentSummary>();

            // Update the dtos with the information from the domain entities.
            if (budgetAdjustmentSummaryEntities != null && budgetAdjustmentSummaryEntities.Any())
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentSummary, Dtos.ColleagueFinance.BudgetAdjustmentSummary>();
                foreach (var adjustmentSummary in budgetAdjustmentSummaryEntities)
                {
                    if (adjustmentSummary != null)
                    {
                        // Validate that the current user is the same person ID as the one obtained from the repository.
                        if (CurrentUser.IsPerson(adjustmentSummary.PersonId))
                        {
                            // Convert the domain entity into a DTO.
                            var summaryDto = adapter.MapToType(adjustmentSummary);

                            // Add the dto to the list of budget adjustment summary DTOs.
                            budgetAdjustmentSummaryDtos.Add(summaryDto);
                        }
                        else
                        {
                            var message = "The current user " + CurrentUser.PersonId + " is not the person that owns the record returned from the repository";
                            logger.Error(message);
                        }
                    }
                }
            }

            return budgetAdjustmentSummaryDtos;
        }

        /// <summary>
        /// Get the list of budget adjustments summary to approve for the current user.
        /// </summary>
        /// <returns>List of budget adjustment summary DTOs for the specified user.</returns>
        public async Task<IEnumerable<BudgetAdjustmentPendingApprovalSummary>> GetBudgetAdjustmentsPendingApprovalSummaryAsync()
        {
            // Check the permission code to view budget adjustments pending approval.
            CheckViewBudgetAdjustmentPendingApprovalPermission();

            // Obtain the list of budget adjustments pending approval for the current user.
            var budgetAdjustmentSummaryEntities = await budgetAdjustmentRepository.GetBudgetAdjustmentsPendingApprovalSummaryAsync(CurrentUser.PersonId);

            // Initialize the budget adjustment summary dtos.
            var budgetAdjustmentSummaryDtos = new List<BudgetAdjustmentPendingApprovalSummary>();

            // Update the dtos with the information from the domain entities.
            if (budgetAdjustmentSummaryEntities != null && budgetAdjustmentSummaryEntities.Any())
            {
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.BudgetAdjustmentPendingApprovalSummary, Dtos.ColleagueFinance.BudgetAdjustmentPendingApprovalSummary>();
                foreach (var adjustmentSummary in budgetAdjustmentSummaryEntities)
                {
                    if (adjustmentSummary != null)
                    {
                        // Convert the domain entity into a DTO.
                        var summaryDto = adapter.MapToType(adjustmentSummary);

                        // Add the dto to the list of budget adjustment summary DTOs.
                        budgetAdjustmentSummaryDtos.Add(summaryDto);

                    }
                }
            }

            return budgetAdjustmentSummaryDtos;
        }

        /// <summary>
        /// Permission code that allows a READ operation on both 
        /// a draft budget adjustment and a budget adjutment.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBudgetAdjustmentPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBudgetAdjustments);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view budget adjustments.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a CREATE and UPDATE operation
        /// on both a draft budget adjustment and a budget adjutment.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateUpdateBudgetAdjustmentPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to create or update budget adjustments.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows an UPDATE operation to approve those
        /// budget adjustments that are pending approval.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBudgetAdjustmentPendingApprovalPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBudgetAdjustmentsPendingApproval);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view budget adjustments pending approval.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Evaluate the budget adjustment and update the Validation Results property on the DTO.
        /// </summary>
        /// <param name="budgetAdjustmentDto">The budget adjustment DTO to validate.</param>
        /// <returns>void - updates the Validation Results property on the DTO.</returns>
        private async Task ValidateBudgetAdjustmentAsync(Domain.ColleagueFinance.Entities.BudgetAdjustment budgetAdjustmentEntity, BudgetAdjustment budgetAdjustmentDto)
        {
            if (budgetAdjustmentDto != null)
            {
                // Get the account structure configuration.
                var glAccountStructure = await configurationRepository.GetAccountStructureAsync();
                if (glAccountStructure == null)
                {
                    throw new ConfigurationException("Account structure must be defined.");
                }

                budgetAdjustmentDto.ValidationResults = new List<string>();

                var validationMessages = await budgetAdjustmentRepository.ValidateBudgetAdjustmentAsync(budgetAdjustmentEntity, glAccountStructure.MajorComponentStartPositions);
                if (validationMessages != null && validationMessages.Any())
                {
                    budgetAdjustmentDto.ValidationResults.AddRange(validationMessages);
                }
            }
        }
    }
}