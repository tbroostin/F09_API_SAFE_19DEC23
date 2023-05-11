// Copyright 2018-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Data.ColleagueFinance.Utilities;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
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
    /// Service for draft budget adjustments.
    /// </summary>
    [RegisterType]
    public class DraftBudgetAdjustmentService : BaseCoordinationService, IDraftBudgetAdjustmentService
    {
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private readonly IDraftBudgetAdjustmentsRepository draftBudgetAdjustmentRepository;

        /// <summary>
        /// Initialize the service.
        /// </summary>
        /// <param name="draftBudgetAdjustmentRepository">Draft budget adjustment repository.</param>
        /// <param name="adapterRegistry">Adapter registry</param>
        /// <param name="currentUserFactory">User factory</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="logger">Logger object</param>
        public DraftBudgetAdjustmentService(IDraftBudgetAdjustmentsRepository draftBudgetAdjustmentRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.draftBudgetAdjustmentRepository = draftBudgetAdjustmentRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
        }

        /// <summary>
        /// Create or update a new budget adjustment.
        /// </summary>
        /// <param name="draftBudgetAdjustmentDto">Draft budget adjustment DTO.</param>
        /// <returns>Budget adjustment DTO</returns>
        public async Task<DraftBudgetAdjustment> SaveDraftBudgetAdjustmentAsync(DraftBudgetAdjustment draftBudgetAdjustmentDto)
        {
            if (draftBudgetAdjustmentDto == null)
            {
                logger.Error("==> Draft budget adjustment is required <==");
                throw new ArgumentNullException("draftBudgetAdjustmentDto", "Draft budget adjustment is required.");
            }

            // Get the account structure configuration.
            var glAccountStructure = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glAccountStructure == null || !glAccountStructure.MajorComponentStartPositions.Any())
            {
                logger.Error("==> Account structure must be defined <==");
                throw new ConfigurationException("Account structure must be defined.");
            }

            // Check the permission code to create or update a draft budget adjustment.
            CheckCreateUpdateBudgetAdjustmentPermission();

            // If this is an update to an existing draft budget adjustment, verify that the user is updating a 
            // draft budget adjustment that they created.
            if (!string.IsNullOrEmpty(draftBudgetAdjustmentDto.Id))
            {
                logger.Debug("==> DraftBudgetAdjustmentService TR date: " + draftBudgetAdjustmentDto.TransactionDate + " <==");
                // Get the draft budget adjustment domain entity.
                var draftBudgetAdjustmentEntity = await draftBudgetAdjustmentRepository.GetAsync(draftBudgetAdjustmentDto.Id);

                // If the draft budget adjustment is null, no verification can take place;
                // throw an exception.
                if (draftBudgetAdjustmentEntity == null)
                {
                    logger.Error("==> Unable to verify person Id for the draft budget adjustment <==");
                    throw new ApplicationException("Unable to verify person Id for the draft budget adjustment.");
                }

                // If the person id that created the draft budget adjustment is not
                //  the same as the current user, throw an exception.
                if (!CurrentUser.IsPerson(draftBudgetAdjustmentEntity.PersonId))
                {
                    logger.Error("==> The draft budget adjustment person ID is not the same as the person ID of the current user <==");
                    throw new PermissionsException("The draft budget adjustment person ID is not the same as the person ID of the current user.");
                }
            }

            // Initialize the draft adjustment line entities.
            var adjustmentLineEntities = new List<Domain.ColleagueFinance.Entities.DraftAdjustmentLine>();
            if (draftBudgetAdjustmentDto.AdjustmentLines != null)
            {
                foreach (var adjustmentLineDto in draftBudgetAdjustmentDto.AdjustmentLines)
                {
                    var draftAdjustmentLine = new Domain.ColleagueFinance.Entities.DraftAdjustmentLine();
                    draftAdjustmentLine.GlNumber = adjustmentLineDto.GlNumber;
                    draftAdjustmentLine.FromAmount = adjustmentLineDto.FromAmount;
                    draftAdjustmentLine.ToAmount = adjustmentLineDto.ToAmount;
                    adjustmentLineEntities.Add(draftAdjustmentLine);
                }
            }

            // Initialize the adjustment object.
            var adjustmentInputEntity = new Domain.ColleagueFinance.Entities.DraftBudgetAdjustment(draftBudgetAdjustmentDto.Reason);
            adjustmentInputEntity.AdjustmentLines = adjustmentLineEntities;
            adjustmentInputEntity.Id = draftBudgetAdjustmentDto.Id;
            adjustmentInputEntity.TransactionDate = draftBudgetAdjustmentDto.TransactionDate;
            adjustmentInputEntity.Comments = draftBudgetAdjustmentDto.Comments;
            adjustmentInputEntity.Initiator = draftBudgetAdjustmentDto.Initiator;

            // Get the adapter for the next approver and assign them to the domain entity.
            var adapterIn = _adapterRegistry.GetAdapter<Dtos.ColleagueFinance.NextApprover, Domain.ColleagueFinance.Entities.NextApprover>();

            adjustmentInputEntity.NextApprovers = new List<Domain.ColleagueFinance.Entities.NextApprover>();
            if (draftBudgetAdjustmentDto.NextApprovers != null)
            {
                foreach (var nextApprover in draftBudgetAdjustmentDto.NextApprovers)
                {
                    var nextApproverDomain = adapterIn.MapToType(nextApprover);
                    adjustmentInputEntity.NextApprovers.Add(nextApproverDomain);
                }
            }

            try
            {
                // Call the repository method to create or update the draft budget adjustment.
                var adjustmentOutputEntity = await draftBudgetAdjustmentRepository.SaveAsync(adjustmentInputEntity, glAccountStructure.MajorComponentStartPositions);

                // Throw an exception if a draft budget adjustment was not returned.
                if (adjustmentOutputEntity == null)
                {
                    logger.Error("==> Draft budget adjustment must not be null <==");
                    throw new ApplicationException("Draft budget adjustment must not be null.");
                }

                // If error exists, throw exception with message.
                if (adjustmentOutputEntity.ErrorMessages != null && adjustmentOutputEntity.ErrorMessages.Count > 0)
                {
                    logger.Error("==>" + String.Join(" <> ", adjustmentOutputEntity.ErrorMessages) + "<==");
                    throw new ApplicationException(String.Join("<>", adjustmentOutputEntity.ErrorMessages));
                }

                // Throw an exception if the returned draft budget adjustment does not have an ID.
                if (string.IsNullOrEmpty(adjustmentOutputEntity.Id))
                {
                    logger.Error("==> Adjustment appears to have succeeded, but no ID was returned <==");
                    throw new ApplicationException("Adjustment appears to have succeeded, but no ID was returned.");
                }

                // Initialize the DTO to return.
                var adjustmentLineDtos = new List<DraftAdjustmentLine>();
                foreach (var adjustmentLineEntity in adjustmentOutputEntity.AdjustmentLines)
                {
                    adjustmentLineDtos.Add(new DraftAdjustmentLine()
                    {
                        GlNumber = adjustmentLineEntity.GlNumber,
                        FromAmount = adjustmentLineEntity.FromAmount,
                        ToAmount = adjustmentLineEntity.ToAmount
                    });
                }

                var adjustmentOutputDto = new DraftBudgetAdjustment()
                {
                    Id = adjustmentOutputEntity.Id,
                    AdjustmentLines = adjustmentLineDtos,
                    Comments = adjustmentOutputEntity.Comments,
                    Initiator = adjustmentOutputEntity.Initiator,
                    Reason = adjustmentOutputEntity.Reason,
                    TransactionDate = adjustmentOutputEntity.TransactionDate
                };

                // Get the adapter for the next approver and assign them to the dto.
                var adapterOut = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, Dtos.ColleagueFinance.NextApprover>();

                adjustmentOutputDto.NextApprovers = new List<NextApprover>();
                if (adjustmentInputEntity.NextApprovers != null)
                {
                    foreach (var nextApprover in adjustmentInputEntity.NextApprovers)
                    {
                        var nextApproverDto = adapterOut.MapToType(nextApprover);
                        adjustmentOutputDto.NextApprovers.Add(nextApproverDto);
                    }
                }

                return adjustmentOutputDto;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Debug(csee, "==> Session expired exception <==");
                throw;
            }
        }

        /// <summary>
        /// Get a draft budget adjustment.
        /// </summary>
        /// <param name="id">A draft budget adjustment ID.</param>
        /// <returns>Budget adjustment DTO</returns>
        public async Task<DraftBudgetAdjustment> GetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Error("==> id is null or empty <==");
                throw new ArgumentNullException("id");
            }

            // Check the permission code to view a draft budget adjustment.
            CheckViewBudgetAdjustmentPermission();

            var draftBudgetAdjustmentEntity = await draftBudgetAdjustmentRepository.GetAsync(id);

            // Initialize the draft budget adjustment dto.
            var draftDto = new DraftBudgetAdjustment();

            // Update the dto with the information from the domain entity.
            if (draftBudgetAdjustmentEntity != null)
            {
                // Only include the entity if the person id that created the 
                // draft budget adjustment is the same as the current user.
                if (!CurrentUser.IsPerson(draftBudgetAdjustmentEntity.PersonId))
                {
                    var message = "==> The current user " + CurrentUser.PersonId + " is not the person " + draftBudgetAdjustmentEntity.PersonId + " that owns the record returned from the repository <==";
                    logger.Error(message);
                    throw new PermissionsException("The draft budget adjustment person ID is not the same as the person ID of the current user.");
                }

                // Convert the domain entity into a DTO.
                var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.DraftBudgetAdjustment, Dtos.ColleagueFinance.DraftBudgetAdjustment>();
                draftDto = adapter.MapToType(draftBudgetAdjustmentEntity);
            }

            return draftDto;
        }

        /// <summary>
        /// Delete a draft budget adjustment.
        /// </summary>
        /// <param name="id">The draft budget adjustment ID to delete.</param>
        /// <returns>Nothing.</returns>
        public async Task DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                logger.Error("==> id is null or empty <==");
                throw new ArgumentNullException("id");
            }

            // Check the permission code to delete a draft budget adjustment.
            CheckDeleteBudgetAdjustmentPermission();

            var draftBudgetAdjustmentEntity = await draftBudgetAdjustmentRepository.GetAsync(id);

            // If the draft budget adjustment is null, no verification can take place;
            // throw an exception.
            if (draftBudgetAdjustmentEntity == null)
            {
                logger.Error("==> Unable to verify person Id for the draft budget adjustment <==");
                throw new ApplicationException("Unable to verify person Id for the draft budget adjustment.");
            }

            // If the person id that created the draft budget adjustment is not
            //  the same as the current user, throw an exception.
            if (!CurrentUser.IsPerson(draftBudgetAdjustmentEntity.PersonId))
            {
                logger.Error("==> The draft budget adjustment person ID is not the same as the person ID of the current user <==");
                throw new PermissionsException("The draft budget adjustment person ID is not the same as the person ID of the current user.");
            }
            else
            {
                try
                {
                    // Proceed with deleting the draft budget adjustment id.
                    await draftBudgetAdjustmentRepository.DeleteAsync(id);
                }
                catch (ColleagueSessionExpiredException csee)
                {
                    logger.Debug(csee, "==> Session expired exception <==");
                    throw;
                }
            }
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
                var message = string.Format("==> {0} does not have permission to view budget adjustments <==", CurrentUser.PersonId);
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
                var message = string.Format("==> {0} does not have permission to create or update budget adjustments <==", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Permission code that allows a DELETE operation on
        /// both a draft budget adjustment and a budget adjutment.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckDeleteBudgetAdjustmentPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.DeleteBudgetAdjustments);

            if (!hasPermission)
            {
                var message = string.Format("==> {0} does not have permission to delete budget adjustments <==", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}