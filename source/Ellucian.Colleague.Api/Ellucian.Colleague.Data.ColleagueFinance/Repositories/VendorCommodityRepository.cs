// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// This class implements the IVendorCommoditiesRepository interface
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class VendorCommodityRepository : BaseColleagueRepository, IVendorCommodityRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VendorCommodityRepository"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="logger">The logger.</param>
        public VendorCommodityRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// standard price for given commodity code & vendor
        /// </summary>
        /// <param name="vendorId">vendor id</param>
        /// <param name="commodityCode">commodity code</param>
        /// <returns>VendorCommodity domain entity</returns>
        public async Task<Domain.ColleagueFinance.Entities.VendorCommodity> GetVendorCommodityAsync(string vendorId, string commodityCode)
        {
            if (string.IsNullOrEmpty(vendorId))
            {
                throw new ArgumentNullException("vendorId");
            }

            if (string.IsNullOrEmpty(commodityCode))
            {
                throw new ArgumentNullException("commodityCode");
            }

            string query = string.Format("WITH VENDOR.COMMODITIES.VEND.ID EQ '{0}' AND WITH VENDOR.COMMODITIES.CODE EQ '{1}'", vendorId, commodityCode);
            var filteredByKey = await DataReader.SelectAsync("VENDOR.COMMODITIES", query);
            if (filteredByKey != null && filteredByKey.Any())
            {
                string recordKey = filteredByKey.ToList().FirstOrDefault();
                var vendorCommodity = await DataReader.ReadRecordAsync<DataContracts.VendorCommodities>(recordKey);
                if (vendorCommodity != null)
                {
                    Domain.ColleagueFinance.Entities.VendorCommodity vendorCommoditiesEntity = new Domain.ColleagueFinance.Entities.VendorCommodity(recordKey);
                    vendorCommoditiesEntity.StdPrice = vendorCommodity.VcmStandardPrice;
                    vendorCommoditiesEntity.StdPriceDate = vendorCommodity.VcmStandardPriceDate;
                    return vendorCommoditiesEntity;
                }
            }
            return null;
        }
    }
}
