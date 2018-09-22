// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class CommodityUnitTypesService : BaseCoordinationService, ICommodityUnitTypesService
    {
        private readonly IColleagueFinanceReferenceDataRepository _cfReferenceDataRepository;
        private readonly ILogger _logger;

        public CommodityUnitTypesService(
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            IColleagueFinanceReferenceDataRepository cfReferenceDataRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)

        {
            _cfReferenceDataRepository = cfReferenceDataRepository;
            _logger = logger;
        }
        #region ICommodityUnitTypesService Members

        /// <summary>
        /// Returns all commodity unit types
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityUnitType>> GetCommodityUnitTypesAsync(bool bypassCache)
        {
            var commodityUnitTypeCollection = new List<Ellucian.Colleague.Dtos.CommodityUnitType>();

            var commodityUnitTypes = await _cfReferenceDataRepository.GetCommodityUnitTypesAsync(bypassCache);
            if (commodityUnitTypes != null && commodityUnitTypes.Any())
            {
                foreach (var commodityUnitType in commodityUnitTypes)
                {
                    commodityUnitTypeCollection.Add(ConvertCommodityUnitTypeEntityToDto(commodityUnitType));
                }
            }
            return commodityUnitTypeCollection;
        }      

        /// <summary>
        /// Returns an commodity unit type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.CommodityUnitType> GetCommodityUnitTypeByIdAsync(string id)
        {
            var commodityUnitTypeEntity = (await _cfReferenceDataRepository.GetCommodityUnitTypesAsync(true)).FirstOrDefault(ct => ct.Guid == id);
            if (commodityUnitTypeEntity == null)
            {
                throw new KeyNotFoundException("Commodity Unit Type is not found.");
            }

            var commodityUnitType = ConvertCommodityUnitTypeEntityToDto(commodityUnitTypeEntity);
            return commodityUnitType;
        }
        #endregion

        #region Convert method(s)

        /// <summary>
        /// Converts from CommodityUnitType entity to CommodityUnitType dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.CommodityUnitType ConvertCommodityUnitTypeEntityToDto(CommodityUnitType source)
        {
            Dtos.CommodityUnitType commodityUnitType = new Dtos.CommodityUnitType();
            commodityUnitType.Id = source.Guid;
            commodityUnitType.Code = source.Code;
            commodityUnitType.Title = source.Description;
            commodityUnitType.Description = string.Empty;
            return commodityUnitType;
        }

        #endregion
    }
}
