//Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class DeductionTypesService : BaseCoordinationService, IDeductionTypesService
    {
        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="referenceDataRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public DeductionTypesService(
            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {
            this._referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all deduction types.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.DeductionType>> GetDeductionTypesAsync(bool bypassCache = false)
        {
            var deductionTypes = new List<Dtos.DeductionType>();

            var deductionTypeEntities = await _referenceDataRepository.GetDeductionTypesAsync(bypassCache);
            if (deductionTypeEntities != null && deductionTypeEntities.Any())
            {
                foreach (var deductionType in deductionTypeEntities)
                {
                    deductionTypes.Add(ConvertDeductionTypeEntityToDto(deductionType));
                }
            }
            return deductionTypes.Any() ? deductionTypes : null;
        }

        /// <summary>
        /// Gets a deduction type by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.DeductionType> GetDeductionTypeByIdAsync(string id)
        {
            try
            {
                var entities = await _referenceDataRepository.GetDeductionTypesAsync(true);

                var entity = entities.FirstOrDefault(i => i.Guid.Equals(id));
                if (entity == null)
                {
                    throw new KeyNotFoundException("Deduction type not found for GUID " + id);
                }
                return ConvertDeductionTypeEntityToDto(entity);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new ColleagueWebApiException("Unknown error getting deduction type.");
            }
        }

        /// <summary>
        /// Gets all deduction types.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.DeductionType2>> GetDeductionTypes2Async(bool bypassCache = false)
        {
            var deductionTypes = new List<Dtos.DeductionType2>();

            var deductionTypeEntities = await _referenceDataRepository.GetDeductionTypes2Async(bypassCache);
            if (deductionTypeEntities != null && deductionTypeEntities.Any())
            {
                foreach (var deductionType in deductionTypeEntities)
                {
                    deductionTypes.Add(await ConvertDeductionTypeEntityToDto2(deductionType, bypassCache));
                }
            }
            return deductionTypes.Any() ? deductionTypes : null;
        }

        /// <summary>
        /// Gets a deduction type by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.DeductionType2> GetDeductionTypeById2Async(string id)
        {
            try
            {
                var entities = await _referenceDataRepository.GetDeductionTypes2Async(true);

                var entity = entities.FirstOrDefault(i => i.Guid.Equals(id));
                if (entity == null)
                {
                    throw new KeyNotFoundException("Deduction type not found for GUID " + id);
                }
                return await ConvertDeductionTypeEntityToDto2(entity, true);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new ColleagueWebApiException("Unknown error getting deduction type.");
            }
        }

        private IEnumerable<Domain.HumanResources.Entities.DeductionCategory> _deductionCategory = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.DeductionCategory>> GetAllDeductionCategoriesAsync(bool bypassCache)
        {
            if (_deductionCategory == null)
            {
                _deductionCategory = await _referenceDataRepository.GetDeductionCategoriesAsync(bypassCache);
            }
            return _deductionCategory;
        }

        private IEnumerable<Domain.HumanResources.Entities.CostCalculationMethod> _costCalculationMethod = null;
        private async Task<IEnumerable<Domain.HumanResources.Entities.CostCalculationMethod>> GetAllCostCalculationMethodsAsync(bool bypassCache)
        {
            if (_costCalculationMethod == null)
            {
                _costCalculationMethod = await _referenceDataRepository.GetCostCalculationMethodsAsync(bypassCache);
            }
            return _costCalculationMethod;
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.DeductionType ConvertDeductionTypeEntityToDto(Domain.HumanResources.Entities.DeductionType source)
        {
            var deductionType = new Dtos.DeductionType();

            deductionType.Id = source.Guid;
            deductionType.Code = source.Code;
            deductionType.Title = source.Description;
            deductionType.Description = null;

            return deductionType;
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private async Task<Dtos.DeductionType2> ConvertDeductionTypeEntityToDto2(Domain.HumanResources.Entities.DeductionType source, bool bypassCache)
        {
            var deductionType = new Dtos.DeductionType2();

            deductionType.Id = source.Guid;
            deductionType.Code = source.Code;
            deductionType.Title = source.Description;
            deductionType.Description = null;
            
            var categoryEntities = await GetAllDeductionCategoriesAsync(bypassCache);
            if (categoryEntities.Any())
            {
                var cateogryEntity = categoryEntities.FirstOrDefault(ep => ep.Code == source.Category);
                if (cateogryEntity != null)
                {
                    deductionType.Category = new GuidObject2(cateogryEntity.Guid);
                }
            }
            
            var ccmEntities = await GetAllCostCalculationMethodsAsync(bypassCache);
            if (!ccmEntities.Any())
            {
                throw new KeyNotFoundException("Cost Calculation Method was not found.");
            }
            else
            {
                var ccmEntity = ccmEntities.FirstOrDefault(ep => ep.Code == source.CostCalculationMethod);
                if (ccmEntity != null)
                {
                    deductionType.CostCalculationMethod = new GuidObject2(ccmEntity.Guid);
                }
                else
                {
                    throw new KeyNotFoundException("Cost Calculation Method was not found.");
                }
            }
            
            deductionType.WithholdingFrequency = source.WithholdingFrequency != null ? new DeductionTypeWithholdingFrequencyDtoProperty() { CyclesPerYear = (decimal) source.WithholdingFrequency } : null;
            
            if ((source.BD_DEFER_TAX_CODES != null && !source.BD_DEFER_TAX_CODES.Any()) && (source.BD_TXABL_TAX_CODES != null && source.BD_TXABL_TAX_CODES.Any()))
            {
                deductionType.TaxApplication = DeductionTypeTaxApplicationType.PostTax;
            }
            else if ((source.BD_DEFER_TAX_CODES != null && source.BD_DEFER_TAX_CODES.Any()) && (source.BD_TXABL_TAX_CODES != null && !source.BD_TXABL_TAX_CODES.Any()))
            {
                deductionType.TaxApplication = DeductionTypeTaxApplicationType.PreTax;
            }

            return deductionType;
        }
    }
}
