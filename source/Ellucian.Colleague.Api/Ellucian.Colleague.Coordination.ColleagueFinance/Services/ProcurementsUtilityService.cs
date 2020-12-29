// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IProcurementsUtilityService interface
    /// </summary>
    [RegisterType]
    public class ProcurementsUtilityService : BaseCoordinationService, IProcurementsUtilityService
    {
        private IVendorsRepository vendorsRepository;
        private ICommodityCodesService commodityCodesService;
        private IVendorCommodityRepository vendorCommoditiesRepository;
        private IColleagueFinanceWebConfigurationsRepository cfWebConfigurationsRepository;


        // This constructor initializes the private attributes
        public ProcurementsUtilityService(
            IVendorsRepository vendorsRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ICommodityCodesService commodityCodesService,
            IVendorCommodityRepository vendorCommoditiesRepository,
            IColleagueFinanceWebConfigurationsRepository cfWebDefaultsRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.vendorsRepository = vendorsRepository;
            this.commodityCodesService = commodityCodesService;
            this.vendorCommoditiesRepository = vendorCommoditiesRepository;
            this.cfWebConfigurationsRepository = cfWebDefaultsRepository;
        }

        /// <summary>
        /// Default Line Item information for given commodity code, vendor & AP type.
        /// </summary>
        /// <param name="commodityCode">commodity code</param>
        /// <param name="VendorId">vendor id</param>
        /// <param name="apType">AP type</param>
        /// <returns>Default Line Item information DTO</returns>
        public async Task<NewLineItemDefaultAdditionalInformation> GetNewLineItemDefaultAdditionalInformation(string commodityCode, string vendorId, string apType)
        {
            var defaultLineItemAdditionalDetails = new Dtos.ColleagueFinance.NewLineItemDefaultAdditionalInformation();

            List<string> taxCodes = new List<string>();
            decimal? defaultStdPrice = null;
            Dtos.ColleagueFinance.ProcurementCommodityCode commodityCodeDto = null;

            var cfWebDefaults = await cfWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            if (!string.IsNullOrEmpty(commodityCode))
            {
                commodityCodeDto = await commodityCodesService.GetCommodityCodeByCodeAsync(commodityCode);
                if (commodityCodeDto != null && !string.IsNullOrEmpty(commodityCodeDto.Code))
                {
                    defaultLineItemAdditionalDetails.FixedAssetFlag = commodityCodeDto.FixedAssetsFlag;
                    defaultLineItemAdditionalDetails.CommodityCode = commodityCodeDto.Code;
                    if (commodityCodeDto.DefaultDescFlag)
                    {
                        defaultLineItemAdditionalDetails.CommodityCodeDesc = commodityCodeDto.Description;
                    }
                    defaultStdPrice = await GetDefaultStdPrice(vendorId, commodityCodeDto);
                }
            }
            
            taxCodes = GetDefaultTaxCodes(apType, commodityCodeDto, cfWebDefaults);
            defaultLineItemAdditionalDetails.StdPrice = defaultStdPrice;
            defaultLineItemAdditionalDetails.TaxCodes = taxCodes;
            if (!string.IsNullOrEmpty(vendorId))
            {
                var vendorTaxFormInfo = await vendorsRepository.GetVendorDefaultTaxFormInfoAsync(vendorId, apType);
                if (vendorTaxFormInfo != null)
                {
                    defaultLineItemAdditionalDetails.TaxForm = vendorTaxFormInfo.TaxForm;
                    defaultLineItemAdditionalDetails.BoxNo = vendorTaxFormInfo.TaxFormBoxCode;
                    defaultLineItemAdditionalDetails.State = vendorTaxFormInfo.TaxFormState;
                }
            }
            return defaultLineItemAdditionalDetails;
        }

        private async Task<decimal?> GetDefaultStdPrice(string vendorId, Dtos.ColleagueFinance.ProcurementCommodityCode commodityCodeDto)
        {
            decimal? defaultStdPrice = null;
            if (commodityCodeDto != null)
            {
                if (!string.IsNullOrEmpty(vendorId) && !string.IsNullOrEmpty(commodityCodeDto.Code))
                {
                    var vendorCommoditiesEntity = await vendorCommoditiesRepository.GetVendorCommodityAsync(vendorId, commodityCodeDto.Code);
                    if(vendorCommoditiesEntity!=null)
                    {
                        defaultStdPrice = vendorCommoditiesEntity.StdPrice;
                    }                    
                }
                defaultStdPrice = defaultStdPrice.HasValue ? defaultStdPrice.Value : commodityCodeDto.Price;
            }
            return defaultStdPrice;
        }

        private List<string> GetDefaultTaxCodes(string apType, Dtos.ColleagueFinance.ProcurementCommodityCode commodityCodeDto, Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration cfWebDefaults)
        {
            List<string> taxCodes = new List<string>();
            if (!string.IsNullOrEmpty(apType))
            {
                if (commodityCodeDto!=null && (commodityCodeDto.TaxCodes != null && commodityCodeDto.TaxCodes.Any()))
                {
                    taxCodes.AddRange(commodityCodeDto.TaxCodes.Distinct());
                }

                if (cfWebDefaults != null && (cfWebDefaults.DefaultTaxCodes != null && cfWebDefaults.DefaultTaxCodes.Any()))
                {
                    taxCodes.AddRange(cfWebDefaults.DefaultTaxCodes.Distinct());
                }
            }
            return taxCodes.Any() ? taxCodes.Distinct().ToList() : taxCodes;
        }
    }
}
