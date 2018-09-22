// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

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

        // Constructor to initialize the private attributes
        public JournalEntryService(IJournalEntryRepository journalEntryRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.journalEntryRepository = journalEntryRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
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

            // Get the journal entry domain entity from the repository
            var journalEntryDomainEntity = await journalEntryRepository.GetJournalEntryAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

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
