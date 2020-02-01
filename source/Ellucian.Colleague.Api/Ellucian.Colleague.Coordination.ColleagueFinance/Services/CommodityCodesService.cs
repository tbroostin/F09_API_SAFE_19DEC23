// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
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

        /// <summary>
        /// Gets all Commodity Codes with descriptions
        /// </summary>
        /// <returns>Collection of Commodity Code</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>> GetAllCommodityCodesAsync()
        {
            var commodityCodeCollection = new List<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode>();

            var commodityCodesEntities = await _cfReferenceDataRepository.GetAllCommodityCodesAsync();
            if (commodityCodesEntities != null && commodityCodesEntities.Any())
            {
                foreach (var commodityCodeEntity in commodityCodesEntities)
                {
                    //convert Commodity Code entity to dto
                    commodityCodeCollection.Add(ConvertCommodityCodesEntityToDto(commodityCodeEntity));
                }
            }
            return commodityCodeCollection;
        }

        /// <summary>
        /// Returns a commodity code
        /// </summary>
        /// <param name="code">commoditycode</param>
        /// <returns>Procurement commodity code</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode> GetCommodityCodeByCodeAsync(string code)
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode commodityCodeDto = new Dtos.ColleagueFinance.ProcurementCommodityCode();
            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code", "code must be specified to get commodity code");

            var commodityCodeEntity = (await _cfReferenceDataRepository.GetCommodityCodeByCodeAsync(code));
            if (commodityCodeEntity == null)
            {
                throw new KeyNotFoundException("Commodity Code is not found.");
            }

            var adapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.ProcurementCommodityCode, Dtos.ColleagueFinance.ProcurementCommodityCode>();

            if (commodityCodeEntity != null)
            {
                commodityCodeDto = adapter.MapToType(commodityCodeEntity);
            }            
            return commodityCodeDto;
        }
        #endregion

        #region Convert method(s)

        /// <summary>
        /// Converts a Commodity Codes domain entity to its corresponding ShipToCodes DTO
        /// </summary>
        /// <param name="source">Commodity Codes domain entity</param>
        /// <returns>Commodity Codes DTO</returns>
        private  Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode ConvertCommodityCodesEntityToDto(Ellucian.Colleague.Domain.ColleagueFinance.Entities.ProcurementCommodityCode source)
        {
            var commodityCode = new Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementCommodityCode();
            if(source != null)
            {
                commodityCode.Code = source.Code;
                commodityCode.Description = source.Description;                
            }
            
            return commodityCode;
        }

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
