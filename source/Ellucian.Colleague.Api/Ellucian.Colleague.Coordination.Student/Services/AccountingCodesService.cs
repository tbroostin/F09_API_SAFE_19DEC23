// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AccountingCodesService : BaseCoordinationService, IAccountingCodesService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;

        public AccountingCodesService(IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger) : 
            base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }
        #region IAccountingCodesService Members

        /// <summary>
        /// Returns all accounting codes.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode>> GetAccountingCodesAsync(bool bypassCache)
        {
            var accountingCodeCollection = new List<Ellucian.Colleague.Dtos.AccountingCode>();

            var accountingCodes = await _studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache);
            if (accountingCodes != null && accountingCodes.Any())
            {
                foreach (var accountingCode in accountingCodes)
                {
                    accountingCodeCollection.Add(ConvertAccountingCodeEntityToDto(accountingCode));
                }
            }
            return accountingCodeCollection;
        }

        /// <summary>
        /// Gets all accountig codes.
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AccountingCode2>> GetAccountingCodes2Async(AccountingCodeCategoryDtoProperty criteria, bool bypassCache)
        {
            List<Dtos.AccountingCode2> accountingDtoCodes = new List<AccountingCode2>();

            if (criteria == null)
            {
                await ConvertAccountingCodeEntitiesToDtoAsync(accountingDtoCodes, await ARCodesAsync(bypassCache), bypassCache);

                ConvertAccountReceivableTypeEntitiesToDto(accountingDtoCodes, await AccountRecievableTypeCodesAsync(bypassCache));

                ConvertDepositTypeEntitiesToDto(accountingDtoCodes, await AccountDepositTypeCodesAsync(bypassCache));

                ConvertDistributionEntitiesToDto(accountingDtoCodes, await DistributionAsync(bypassCache));
            }
            else
            {
                // if we have a detail.id only, then make sure its a valid AR.CATEGORIES
                if ((criteria.Detail != null) && (!string.IsNullOrEmpty(criteria.Detail.Id)))
                {
                    if (criteria.AccountingCodeCategory == AccountingCodeCategoryType.NotSet)
                    {
                        var arCodes = await ARCodesAsync(bypassCache);
                        if ((arCodes != null) && (arCodes.Any(x => x.Guid.Equals(criteria.Detail.Id, StringComparison.OrdinalIgnoreCase))));
                        {
                            criteria.AccountingCodeCategory = AccountingCodeCategoryType.AccountsReceivableCode;
                        }
                    }
                    if (criteria.AccountingCodeCategory != AccountingCodeCategoryType.AccountsReceivableCode)
                    {
                        return new List<AccountingCode2>();
                    }
                }
                switch (criteria.AccountingCodeCategory)
                {                    
                    case AccountingCodeCategoryType.DepositType:
                        ConvertDepositTypeEntitiesToDto(accountingDtoCodes, await AccountDepositTypeCodesAsync(bypassCache));
                        break;
                    case AccountingCodeCategoryType.DistributionCode:
                        ConvertDistributionEntitiesToDto(accountingDtoCodes, await DistributionAsync(bypassCache));
                        break;
                    case AccountingCodeCategoryType.AccountsReceivableType:
                        ConvertAccountReceivableTypeEntitiesToDto(accountingDtoCodes, await AccountRecievableTypeCodesAsync(bypassCache));
                        break;
                    case AccountingCodeCategoryType.AccountsReceivableCode:
                        await ConvertAccountingCodeEntitiesToDtoAsync(accountingDtoCodes, await ARCodesAsync(bypassCache), bypassCache);
                        if (accountingDtoCodes != null && accountingDtoCodes.Any() && 
                            criteria != null && criteria.Detail!= null && !string.IsNullOrEmpty(criteria.Detail.Id))
                        {
                            var acctCodes = accountingDtoCodes.Where(i => i.AccountingCodeCategory.Detail!= null && !string.IsNullOrEmpty(i.AccountingCodeCategory.Detail.Id) &&
                                                                          i.AccountingCodeCategory.Detail.Id.Equals(criteria.Detail.Id, StringComparison.OrdinalIgnoreCase));
                            return acctCodes;
                        }
                        break;
                    case AccountingCodeCategoryType.NotSet:
                    case AccountingCodeCategoryType.Charge:
                    case AccountingCodeCategoryType.Payment:
                    default:
                        return new List<AccountingCode2>();
                }
            }

            return accountingDtoCodes;
        }

        /// <summary>
        /// Returns an accounting code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.AccountingCode> GetAccountingCodeByIdAsync(string id)
        {
            var accountingCodeEntity = (await _studentReferenceDataRepository.GetAccountingCodesAsync(true)).FirstOrDefault(ac => ac.Guid == id);
            if (accountingCodeEntity == null)
            {
                throw new KeyNotFoundException(string.Format("AR Code not found for Id {0}.", id));
            }

            var accountingCode = ConvertAccountingCodeEntityToDto(accountingCodeEntity);
            return accountingCode;
        }

        /// <summary>
        /// Gets accounting code by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<AccountingCode2> GetAccountingCode2ByIdAsync(string id, bool bypassCache)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("The id required.");
            }
            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(id);
                if (lookupResult == null) throw new KeyNotFoundException();

                switch (lookupResult.Entity.ToUpper())
                {
                    case "AR.CODES":
                        var arCode = (await ARCodesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                        var arCodeDto = ConvertEntity2ToDto(arCode, AccountingCodeCategoryType.AccountsReceivableCode);
                        if (!string.IsNullOrEmpty(arCode.ArCategoryCode))
                        {
                            arCodeDto.AccountingCodeCategory.Detail = await ConvertCodeToGuidItemAsync(arCode.ArCategoryCode, bypassCache);
                        }
                        return arCodeDto;
                    case "AR.TYPES":
                        var arType = (await AccountRecievableTypeCodesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                        return ConvertEntity2ToDto(arType, AccountingCodeCategoryType.AccountsReceivableType);
                    case "AR.DEPOSIT.TYPES":
                        var arDepositType = (await AccountDepositTypeCodesAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                        return ConvertEntity2ToDto(arDepositType, AccountingCodeCategoryType.DepositType);
                    case "DISTRIBUTION":
                        var arDistribution = (await DistributionAsync(bypassCache)).FirstOrDefault(i => i.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));
                        return ConvertEntity2ToDto(arDistribution, AccountingCodeCategoryType.DistributionCode);
                    default:
                        throw new KeyNotFoundException();
                }
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException(string.Format("No accounting code was found for guid {0}.", id));
            }



        }

        #endregion

        #region Convert method(s)

        /// <summary>
        /// Converts from AccountingCode entity to AccountingCode dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.AccountingCode ConvertAccountingCodeEntityToDto(Domain.Student.Entities.AccountingCode source)
        {
            Dtos.AccountingCode accountingCode = new Dtos.AccountingCode();
            accountingCode.Id = source.Guid;
            accountingCode.Code = source.Code;
            accountingCode.Title = source.Description;
            accountingCode.Description = string.Empty;
            return accountingCode;
        }

        /// <summary>
        /// Converts from AccountingCode entity to AccountingCode dto
        /// </summary>
        /// <param name="source"></param>
        /// <param name="accountingCodeCategoryType"></param>
        /// <returns></returns>
        private Dtos.AccountingCode2 ConvertEntity2ToDto(Domain.Entities.GuidCodeItem source, AccountingCodeCategoryType accountingCodeCategoryType)
        {
            Dtos.AccountingCode2 accountingCode = new Dtos.AccountingCode2();
            accountingCode.Id = source.Guid;
            accountingCode.Code = source.Code;
            accountingCode.Title = source.Description;
            accountingCode.Description = null;
            accountingCode.AccountingCodeCategory = new AccountingCodeCategoryDtoProperty()
            {
                AccountingCodeCategory = accountingCodeCategoryType,
            };
            return accountingCode;
        }

        /// <summary>
        /// Converts entity to acoounting code category.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="accountingDtoCodes"></param>
        /// <param name="accountingCodeEntities"></param>
        /// <returns></returns>
        private async Task ConvertAccountingCodeEntitiesToDtoAsync(List<AccountingCode2> accountingDtoCodes, IEnumerable<Domain.Student.Entities.AccountingCode> accountingCodeEntities, bool bypassCache)
        {
            foreach (var entity in accountingCodeEntities)
            {
                Dtos.AccountingCode2 acctCode = ConvertEntity2ToDto(entity, AccountingCodeCategoryType.AccountsReceivableCode);
                if (acctCode.AccountingCodeCategory != null)
                {
                    acctCode.AccountingCodeCategory.Detail = await ConvertCodeToGuidItemAsync(entity.ArCategoryCode, bypassCache);
                }
                accountingDtoCodes.Add(acctCode);
            }
        }

        /// <summary>
        /// Converts account recievable type entity to dto.
        /// </summary>
        /// <param name="accountingDtoCodes"></param>
        /// <param name="arTypeEntities"></param>
        private void ConvertAccountReceivableTypeEntitiesToDto(List<AccountingCode2> accountingDtoCodes, IEnumerable<Domain.Student.Entities.AccountReceivableType> arTypeEntities)
        {
            foreach (var entity in arTypeEntities)
            {
                Dtos.AccountingCode2 acctCode = ConvertEntity2ToDto(entity, AccountingCodeCategoryType.AccountsReceivableType);
                accountingDtoCodes.Add(acctCode);
            }
        }

        /// <summary>
        /// Converts deposit type entity to dto.
        /// </summary>
        /// <param name="accountingDtoCodes"></param>
        /// <param name="arDepositTypes"></param>
        private void ConvertDepositTypeEntitiesToDto(List<AccountingCode2> accountingDtoCodes, IEnumerable<AccountReceivableDepositType> arDepositTypes)
        {
            foreach (var entity in arDepositTypes)
            {
                Dtos.AccountingCode2 acctCode = ConvertEntity2ToDto(entity, AccountingCodeCategoryType.DepositType);
                accountingDtoCodes.Add(acctCode);
            }
        }

        /// <summary>
        /// Converts distribution entity to dto.
        /// </summary>
        /// <param name="accountingDtoCodes"></param>
        /// <param name="distributionEntities"></param>
        private void ConvertDistributionEntitiesToDto(List<AccountingCode2> accountingDtoCodes, IEnumerable<Domain.Base.Entities.Distribution2> distributionEntities)
        {
            foreach (var entity in distributionEntities)
            {
                Dtos.AccountingCode2 acctCode = ConvertEntity2ToDto(entity, AccountingCodeCategoryType.DistributionCode);
                accountingDtoCodes.Add(acctCode);
            }
        }

        /// <summary>
        /// Gets guid object for ar category code
        /// </summary>
        /// <param name="arCategoryCode"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        private async Task<GuidObject2> ConvertCodeToGuidItemAsync(string arCategoryCode, bool bypassCache)
        {
            if (string.IsNullOrEmpty(arCategoryCode)) return null;

            var arCatCode = (await ARCategoriesAsync(bypassCache)).FirstOrDefault(i => i.Code.Equals(arCategoryCode, StringComparison.OrdinalIgnoreCase));
            if (arCatCode == null)
            {
                throw new KeyNotFoundException(string.Format("Accounting category code not found for code: {0}.", arCategoryCode));
            }
            return new GuidObject2(arCatCode.Guid);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ARCategories
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        IEnumerable<ArCategory> arCategories = null;
        private async Task<IEnumerable<ArCategory>> ARCategoriesAsync(bool bypassCache)
        {
            if (arCategories == null)
            {
                arCategories = await _studentReferenceDataRepository.GetArCategoriesAsync(bypassCache);
            }
            return arCategories;
        }

        /// <summary>
        /// ARCodesAsync
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        IEnumerable<Domain.Student.Entities.AccountingCode> arCodes = null;
        private async Task<IEnumerable<Domain.Student.Entities.AccountingCode>> ARCodesAsync(bool bypassCache)
        {
            if (arCodes == null)
            {
                arCodes = await _studentReferenceDataRepository.GetAccountingCodesAsync(bypassCache);
            }
            return arCodes;
        }

        /// <summary>
        /// AccountRecievableTypeCodesAsync
        /// </summary>
        IEnumerable<Domain.Student.Entities.AccountReceivableType> arRecievableTypeCodes = null;
        private async Task<IEnumerable<Domain.Student.Entities.AccountReceivableType>> AccountRecievableTypeCodesAsync(bool bypassCache)
        {
            if (arRecievableTypeCodes == null)
            {
                arRecievableTypeCodes = await _studentReferenceDataRepository.GetAccountReceivableTypesAsync(bypassCache);
            }
            return arRecievableTypeCodes;
        }

        /// <summary>
        /// AccountDepositTypeCodesAsync
        /// </summary>
        IEnumerable<Domain.Student.Entities.AccountReceivableDepositType> arDepositTypeCodes = null;
        private async Task<IEnumerable<Domain.Student.Entities.AccountReceivableDepositType>> AccountDepositTypeCodesAsync(bool bypassCache)
        {
            if (arDepositTypeCodes == null)
            {
                arDepositTypeCodes = await _studentReferenceDataRepository.GetAccountReceivableDepositTypesAsync(bypassCache);
            }
            return arDepositTypeCodes;
        }

        /// <summary>
        /// DistributionAsync
        /// </summary>
        IEnumerable<Domain.Base.Entities.Distribution2> distributionCodes = null;
        private async Task<IEnumerable<Domain.Base.Entities.Distribution2>> DistributionAsync(bool bypassCache)
        {
            if (distributionCodes == null)
            {
                distributionCodes = await _studentReferenceDataRepository.GetDistributionsAsync(bypassCache);
            }
            return distributionCodes;
        }

        #endregion
    }
}
