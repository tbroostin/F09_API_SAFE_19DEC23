//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class AccountingStringSubcomponentValuesService : BaseCoordinationService, IAccountingStringSubcomponentValuesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public AccountingStringSubcomponentValuesService(

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


        private IEnumerable<AcctStructureIntg> _acctStructIntg;
        private async Task<IEnumerable<AcctStructureIntg>> GetAcctStructureIntgAsync(bool bypassCache)
        {
            if (_acctStructIntg != null)
                return _acctStructIntg;
            else
            {
                _acctStructIntg = await _referenceDataRepository.GetAcctStructureIntgAsync(bypassCache);
                return _acctStructIntg;
            }

        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all accounting-string-subcomponent-values
        /// </summary>
        /// <returns>Collection of AccountingStringSubcomponentValues DTO objects</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues>, int>> GetAccountingStringSubcomponentValuesAsync(int offset, int limit, bool bypassCache = false)
        {
            CheckViewAccountingStringsPermission();

            try
            {
                var accountingStringSubcomponentValuesCollection = new List<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues>();
                var accountingStringSubcomponentValuesEntities = await _referenceDataRepository.GetAccountingStringSubcomponentValuesAsync(offset, limit, bypassCache);
                if (accountingStringSubcomponentValuesEntities != null && accountingStringSubcomponentValuesEntities.Item1 != null && accountingStringSubcomponentValuesEntities.Item1.Any())
                {
                    foreach (var accountingStringSubcomponentValues in accountingStringSubcomponentValuesEntities.Item1)
                    {
                        accountingStringSubcomponentValuesCollection.Add(await ConvertAccountingStringSubcomponentValuesEntityToDto(accountingStringSubcomponentValues, bypassCache));
                    }
                }
                return new Tuple<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues>, int>(accountingStringSubcomponentValuesCollection, accountingStringSubcomponentValuesEntities.Item2);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AccountingStringSubcomponentValues from its GUID
        /// </summary>
        /// <returns>AccountingStringSubcomponentValues DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues> GetAccountingStringSubcomponentValuesByGuidAsync(string guid, bool bypassCache = true)
        {
            CheckViewAccountingStringsPermission();

            try
            {
                return await ConvertAccountingStringSubcomponentValuesEntityToDto(await _referenceDataRepository.GetAccountingStringSubcomponentValuesByGuidAsync(guid), bypassCache);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("accounting-string-subcomponent-values not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("accounting-string-subcomponent-values not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FdDescs domain entity to its corresponding AccountingStringSubcomponentValues DTO
        /// </summary>
        /// <param name="source">FdDescs domain entity</param>
        /// <returns>AccountingStringSubcomponentValues DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues> ConvertAccountingStringSubcomponentValuesEntityToDto(Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues source, bool bypassCache)
        {
            var accountingStringSubcomponentValues = new Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues();

            accountingStringSubcomponentValues.Id = source.Guid;
            accountingStringSubcomponentValues.Code = source.Code;
            accountingStringSubcomponentValues.Title = source.Description;
            if (!string.IsNullOrEmpty(source.Explanation))
                accountingStringSubcomponentValues.Description = source.Explanation;
            var acctStringComponents = await GetAcctStructureIntgAsync(bypassCache);
            if (acctStringComponents != null && acctStringComponents.Any())
            {
                var subcomponent = acctStringComponents.FirstOrDefault(intg => intg.Type.Equals(source.Type, StringComparison.OrdinalIgnoreCase) && intg.Length == source.Code.Length);
                if (subcomponent != null)
                {
                    accountingStringSubcomponentValues.Subcomponent = new Dtos.GuidObject2(subcomponent.Guid);
                    //now find parent subcomponents
                    if (!string.IsNullOrEmpty(subcomponent.ParentSubComponent))
                    {
                        var parComponent = acctStringComponents.FirstOrDefault(intg => intg.Id == subcomponent.ParentSubComponent);
                        if (parComponent != null)
                        {
                            try
                            {
                                var parEntityGuid = await _referenceDataRepository.GetGuidFromEntityInfoAsync(parComponent.FileName, source.Code.Substring(0, parComponent.Length.GetValueOrDefault()));
                                if (!string.IsNullOrEmpty(parEntityGuid))
                                    accountingStringSubcomponentValues.ParentSubcomponent = new Dtos.GuidObject2(parEntityGuid);
                                else
                                    throw new KeyNotFoundException(string.Concat("GL subcomponent ", parComponent.Title, " does not exist. Entity ='", parComponent.FileName, "' Record Id ='", source.Code.Substring(0, parComponent.Length.GetValueOrDefault()), "'"));
                            }
                            catch
                            {
                                throw new ArgumentException(string.Concat("GL subcomponent ", parComponent.Title, " does not exist. Entity ='", parComponent.FileName, "' Record Id ='", source.Code.Substring(0, parComponent.Length.GetValueOrDefault()), "'"));
                            }
                        }
                    }
                }
            }
            return accountingStringSubcomponentValues;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view ColleagueFinance accounting-string-subcomponent-values.
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
