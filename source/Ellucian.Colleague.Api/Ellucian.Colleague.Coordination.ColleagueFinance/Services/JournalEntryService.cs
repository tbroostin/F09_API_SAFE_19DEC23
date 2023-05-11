// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Implements the IJournalEntryService
    /// </summary>
    [RegisterType]
    public class JournalEntryService : BaseCoordinationService, IJournalEntryService
    {
        private IJournalEntryRepository journalEntryRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IApprovalConfigurationRepository approvalConfigurationRepository;

        // Constructor to initialize the private attributes
        public JournalEntryService(IJournalEntryRepository journalEntryRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IApprovalConfigurationRepository approvalConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.journalEntryRepository = journalEntryRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.approvalConfigurationRepository = approvalConfigurationRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified journal entry
        /// </summary>
        /// <param name="id">The journal entry number</param>
        /// <returns>Journal Entry DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.JournalEntry> GetJournalEntryAsync(string id)
        {
            // Check the permission code to view a journal entry.
            CheckViewJournalEntryPermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // If the user has access to all the GL accounts through the GL access role,
            // don't bother adding any additional GL accounts they may have access through
            // approval roles.
            IEnumerable<string> allAccessAndApprovalAccounts = new List<string>();
            if (generalLedgerUser.GlAccessLevel != GlAccessLevel.Full_Access)
            {
                ApprovalConfiguration approvalConfiguration = null;
                try
                {
                    approvalConfiguration = await approvalConfigurationRepository.GetApprovalConfigurationAsync();
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Approval Roles are not configured.");
                }

                // Check if journal entries use approval roles.
                if (approvalConfiguration != null && approvalConfiguration.JournalEntriesUseApprovalRoles)
                {
                    // Use the approval roles to see if they have access to additional GL accounts.
                    allAccessAndApprovalAccounts = await generalLedgerUserRepository.GetGlUserApprovalAndGlAccessAccountsAsync(CurrentUser.PersonId, generalLedgerUser.AllAccounts);
                }
                else
                {
                    // If approval roles aren't being used, just use the GL Access
                    allAccessAndApprovalAccounts = generalLedgerUser.AllAccounts;
                }

                // If the user has access to some GL accounts via approval 
                // roles, change the GL access level to possible access.
                if (allAccessAndApprovalAccounts != null && allAccessAndApprovalAccounts.Any())
                {
                    generalLedgerUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
                }
            }

            // Get the journal entry domain entity from the repository
            var journalEntryDomainEntity = await journalEntryRepository.GetJournalEntryAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, allAccessAndApprovalAccounts);

            if (journalEntryDomainEntity == null)
            {
                throw new ArgumentNullException("journalEntryDomainEntity", "journalEntryDomainEntity cannot be null.");
            }

            // Convert the journal entry and all its child objects into DTOs.
            var journalEntryDtoAdapter = new JournalEntryEntityToDtoAdapter(_adapterRegistry, logger);
            var journalEntryDto = journalEntryDtoAdapter.MapToType(journalEntryDomainEntity, glConfiguration.MajorComponentStartPositions);

            if (journalEntryDto.Items == null || journalEntryDto.Items.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access journal entry.");
            }

            return journalEntryDto;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a journal entry.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewJournalEntryPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewJournalEntry);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view journal entries.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}
