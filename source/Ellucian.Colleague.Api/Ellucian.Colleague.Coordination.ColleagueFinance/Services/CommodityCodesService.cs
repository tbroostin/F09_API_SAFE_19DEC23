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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class CommodityCodesService : BaseCoordinationService, ICommodityCodesService
    {
        private readonly IColleagueFinanceReferenceDataRepository _cfReferenceDataRepository;
        private readonly ILogger _logger;

        public CommodityCodesService(
            IAdapterRegistry adapterRegistry,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IColleagueFinanceReferenceDataRepository cfReferenceDataRepository, 
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _cfReferenceDataRepository = cfReferenceDataRepository;
            _logger = logger;
        }
        #region ICommodityCodesService Members

        /// <summary>
        /// Returns all commodity codes
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CommodityCode>> GetCommodityCodesAsync(bool bypassCache)
        {
            var commodityCodeCollection = new List<Ellucian.Colleague.Dtos.CommodityCode>();

            var commodityCodes = await _cfReferenceDataRepository.GetCommodityCodesAsync(bypassCache);
            if (commodityCodes != null && commodityCodes.Any())
            {
                foreach (var commodityCode in commodityCodes)
                {
                    commodityCodeCollection.Add(ConvertCommodityCodeEntityToDto(commodityCode));
                }
            }
            return commodityCodeCollection;
        }      

        /// <summary>
        /// Returns an commodity code
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.CommodityCode> GetCommodityCodeByIdAsync(string id)
        {
            var commodityCodeEntity = (await _cfReferenceDataRepository.GetCommodityCodesAsync(true)).FirstOrDefault(ac => ac.Guid == id);
            if (commodityCodeEntity == null)
            {
                throw new KeyNotFoundException("Commodity Code is not found.");
            }

            var commodityCode = ConvertCommodityCodeEntityToDto(commodityCodeEntity);
            return commodityCode;
        }
        #endregion

        #region Convert method(s)

        /// <summary>
        /// Converts from CommodityCode entity to CommodityCode dto
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.CommodityCode ConvertCommodityCodeEntityToDto(CommodityCode source)
        {
            Dtos.CommodityCode commodityCode = new Dtos.CommodityCode();
            commodityCode.Id = source.Guid;
            commodityCode.Code = source.Code;
            commodityCode.Title = source.Description;
            commodityCode.Description = string.Empty;
            return commodityCode;
        }

        #endregion
    }
}
