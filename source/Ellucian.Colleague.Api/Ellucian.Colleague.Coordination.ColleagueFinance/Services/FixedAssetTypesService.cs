//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    [RegisterType]
    public class FixedAssetTypesService : BaseCoordinationService, IFixedAssetTypesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FixedAssetTypesService(

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

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all fixed-asset-types
        /// </summary>
        /// <returns>Collection of FixedAssetTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetType>> GetFixedAssetTypesAsync(bool bypassCache = false)
        {
            var fixedAssetTypesCollection = new List<Ellucian.Colleague.Dtos.FixedAssetType>();

            var fixedAssetTypesEntities = await _referenceDataRepository.GetAssetTypesAsync(bypassCache);
            if (fixedAssetTypesEntities != null && fixedAssetTypesEntities.Any())
            {
                foreach (var fixedAssetTypes in fixedAssetTypesEntities)
                {
                    fixedAssetTypesCollection.Add(ConvertFixedAssetTypesEntityToDto(fixedAssetTypes));
                }
            }
            return fixedAssetTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FixedAssetTypes from its GUID
        /// </summary>
        /// <returns>FixedAssetTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FixedAssetType> GetFixedAssetTypesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFixedAssetTypesEntityToDto((await _referenceDataRepository.GetAssetTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("fixed-asset-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("fixed-asset-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AssetTypes domain entity to its corresponding FixedAssetTypes DTO
        /// </summary>
        /// <param name="source">AssetTypes domain entity</param>
        /// <returns>FixedAssetTypes DTO</returns>
        private Ellucian.Colleague.Dtos.FixedAssetType ConvertFixedAssetTypesEntityToDto(AssetTypes source)
        {
            var fixedAssetTypes = new Ellucian.Colleague.Dtos.FixedAssetType();

            fixedAssetTypes.Id = source.Guid;
            fixedAssetTypes.Code = source.Code;
            fixedAssetTypes.Title = source.Description;
            fixedAssetTypes.Description = null;
            fixedAssetTypes.DepreciationMethod = source.AstpCalcMethod;
            fixedAssetTypes.SalvageValue = source.AstpSalvagePoint;
            fixedAssetTypes.UsefulLife = source.AstpUsefulLife;

            return fixedAssetTypes;
        }
    }
}