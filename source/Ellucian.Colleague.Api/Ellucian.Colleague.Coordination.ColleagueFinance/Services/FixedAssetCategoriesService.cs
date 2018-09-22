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
    public class FixedAssetCategoriesService : BaseCoordinationService, IFixedAssetCategoriesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FixedAssetCategoriesService(

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
        /// Gets all fixed-asset-categories
        /// </summary>
        /// <returns>Collection of FixedAssetCategories DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetCategory>> GetFixedAssetCategoriesAsync(bool bypassCache = false)
        {
            var fixedAssetCategoriesCollection = new List<Ellucian.Colleague.Dtos.FixedAssetCategory>();

            var fixedAssetCategoriesEntities = await _referenceDataRepository.GetAssetCategoriesAsync(bypassCache);
            if (fixedAssetCategoriesEntities != null && fixedAssetCategoriesEntities.Any())
            {
                foreach (var fixedAssetCategories in fixedAssetCategoriesEntities)
                {
                    fixedAssetCategoriesCollection.Add(ConvertFixedAssetCategoriesEntityToDto(fixedAssetCategories));
                }
            }
            return fixedAssetCategoriesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FixedAssetCategories from its GUID
        /// </summary>
        /// <returns>FixedAssetCategories DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FixedAssetCategory> GetFixedAssetCategoriesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFixedAssetCategoriesEntityToDto((await _referenceDataRepository.GetAssetCategoriesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("fixed-asset-categories not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("fixed-asset-categories not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AssetCategories domain entity to its corresponding FixedAssetCategories DTO
        /// </summary>
        /// <param name="source">AssetCategories domain entity</param>
        /// <returns>FixedAssetCategories DTO</returns>
        private Ellucian.Colleague.Dtos.FixedAssetCategory ConvertFixedAssetCategoriesEntityToDto(AssetCategories source)
        {
            var fixedAssetCategories = new Ellucian.Colleague.Dtos.FixedAssetCategory();

            fixedAssetCategories.Id = source.Guid;
            fixedAssetCategories.Code = source.Code;
            fixedAssetCategories.Title = source.Description;
            fixedAssetCategories.Description = null;

            return fixedAssetCategories;
        }


    }
}