//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class AccountingStringSubcomponentsService : BaseCoordinationService, IAccountingStringSubcomponentsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public AccountingStringSubcomponentsService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        public IEnumerable<AcctStructureIntg> _acctStructureIntgRecords { get; set; }
        private async Task<IEnumerable<AcctStructureIntg>> GetAcctStructureIntg(bool bypassCache)
        {
            if (_acctStructureIntgRecords == null)
            {
                _acctStructureIntgRecords = await _referenceDataRepository.GetAcctStructureIntgAsync(bypassCache);
            }
            return _acctStructureIntgRecords;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all accounting-string-subcomponents
        /// </summary>
        /// <returns>Collection of AccountingStringSubcomponents DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponents>> GetAccountingStringSubcomponentsAsync(bool bypassCache = false)
        {
            CheckViewAccountingStringsPermission();

            var accountingStringSubcomponentsCollection = new List<Ellucian.Colleague.Dtos.AccountingStringSubcomponents>();

            var accountingStringSubcomponentsEntities = await GetAcctStructureIntg(bypassCache);
            if (accountingStringSubcomponentsEntities != null && accountingStringSubcomponentsEntities.Any())
            {
                foreach (var accountingStringSubcomponents in accountingStringSubcomponentsEntities)
                {
                    accountingStringSubcomponentsCollection.Add(ConvertAccountingStringSubcomponentsEntityToDto(accountingStringSubcomponents));
                }
            }
            return accountingStringSubcomponentsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AccountingStringSubcomponents from its GUID
        /// </summary>
        /// <returns>AccountingStringSubcomponents DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringSubcomponents> GetAccountingStringSubcomponentsByGuidAsync(string guid, bool bypassCache = true)
        {
            CheckViewAccountingStringsPermission();

            try
            {
                return ConvertAccountingStringSubcomponentsEntityToDto((await GetAcctStructureIntg(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-subcomponents not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-subcomponents not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AcctStructureIntg domain entity to its corresponding AccountingStringSubcomponents DTO
        /// </summary>
        /// <param name="source">AcctStructureIntg domain entity</param>
        /// <returns>AccountingStringSubcomponents DTO</returns>
        private Ellucian.Colleague.Dtos.AccountingStringSubcomponents ConvertAccountingStringSubcomponentsEntityToDto(AcctStructureIntg source)
        {
            var accountingStringSubcomponents = new Ellucian.Colleague.Dtos.AccountingStringSubcomponents();

            accountingStringSubcomponents.Id = source.Guid;
            accountingStringSubcomponents.Title = source.Title;
            if (!string.IsNullOrEmpty(source.Type))
            {
                switch (source.Type)
                {
                    case "FD":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Fund;
                        break;
                    case "FC":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Function;
                        break;
                    case "OB":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Object;
                        break;
                    case "UN":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Unit;
                        break;
                    case "SO":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Source;
                        break;
                    case "LO":
                        accountingStringSubcomponents.Type = AccountingStringSubcomponentsType.Location;
                        break;
                    default:
                        break;

                }
            }
            if (_acctStructureIntgRecords != null && _acctStructureIntgRecords.Any() && !string.IsNullOrEmpty(source.ParentSubComponent))
            {
                var parentCat = _acctStructureIntgRecords.FirstOrDefault(par => par.Id == source.ParentSubComponent);
                if (parentCat != null && !string.IsNullOrEmpty(parentCat.Guid))
                    accountingStringSubcomponents.ParentSubcomponent = new GuidObject2 { Id = parentCat.Guid };
            }

            return accountingStringSubcomponents;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view ColleagueFinance accounting-string-subcomponents.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewAccountingStringsPermission()
        {
            bool hasPermission = HasPermission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewAccountingStrings);
            // User is not allowed to read accounting strings without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException(string.Format("User {0} does not have permission to view accounting string information.", CurrentUser.UserId));
            }
        }

    }
}
