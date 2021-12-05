/*Copyright 2020 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{

    [TestClass]
    public class VendorCommodityRepositoryTests : BaseRepositorySetup
    {
        private Mock<IColleagueTransactionFactory> _iColleagueTransactionFactoryMock;
        private Mock<IColleagueTransactionInvoker> _iColleagueTransactionInvokerMock;
        private Mock<ILogger> _iLoggerMock;
        private Mock<IColleagueDataReader> _dataReaderMock;

        private VendorCommodityRepository vendorCommodityRepository;
        Ellucian.Colleague.Data.ColleagueFinance.DataContracts.VendorCommodities vendorCommodityCommodtiyDataContract;
        private Ellucian.Colleague.Domain.ColleagueFinance.Entities.VendorCommodity vendorCommodityEntity;
        
        private string vendorId;
        private string commodityCode;
        string expectedRecordKey;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            _iColleagueTransactionFactoryMock = new Mock<IColleagueTransactionFactory>();
            _iColleagueTransactionInvokerMock = new Mock<IColleagueTransactionInvoker>();
            _iLoggerMock = new Mock<ILogger>();
            _dataReaderMock = new Mock<IColleagueDataReader>();
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetDataReader())
                .Returns(_dataReaderMock.Object);
            _iColleagueTransactionFactoryMock.Setup(transFac => transFac.GetTransactionInvoker())
                .Returns(transManager);

            vendorId = "0000190";
            commodityCode = "10900";
            expectedRecordKey = vendorId + "*" + commodityCode;
            vendorCommodityEntity = new VendorCommodity(expectedRecordKey)
            {
                StdPrice = 10,
                StdPriceDate = DateTime.Today.Date
            };
            vendorCommodityCommodtiyDataContract = new VendorCommodities()
            {
                Recordkey = expectedRecordKey,
                VcmStandardPrice = 10,
                VcmStandardPriceDate = DateTime.Today.Date
            };

            var ids = new List<string>() { expectedRecordKey };
            string query = string.Format("WITH VENDOR.COMMODITIES.VEND.ID EQ '{0}' AND WITH VENDOR.COMMODITIES.CODE EQ '{1}'", vendorId, commodityCode);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDOR.COMMODITIES", query)).ReturnsAsync(ids.ToArray());
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<VendorCommodities>(expectedRecordKey, true)).ReturnsAsync(vendorCommodityCommodtiyDataContract);

            vendorCommodityRepository = new VendorCommodityRepository(cacheProviderMock.Object, _iColleagueTransactionFactoryMock.Object, _iLoggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _iColleagueTransactionFactoryMock = null;
            _iColleagueTransactionInvokerMock = null;
            _iLoggerMock = null;
            _dataReaderMock = null;
            apiSettings = null;
            vendorCommodityRepository = null;
            transManager = null;
        }
        
        #region GetVendorCommodityAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VendorCommodityRepository_GetVendorCommodityAsync_ArgumentNullException()
        {
            var actual = await vendorCommodityRepository.GetVendorCommodityAsync(null, null);
        }

        [TestMethod]        
        public async Task VendorCommodityRepository_GetVendorCommodityAsync_SelectAsyncReturnsNull()
        {
            string query = string.Format("WITH VENDOR.COMMODITIES.VEND.ID EQ '{0}' AND WITH VENDOR.COMMODITIES.CODE EQ '{1}'", vendorId, commodityCode);
            _dataReaderMock.Setup(repo => repo.SelectAsync("VENDOR.COMMODITIES", query)).ReturnsAsync(() => null);
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<VendorCommodities>(expectedRecordKey, true)).ReturnsAsync(vendorCommodityCommodtiyDataContract);

            var result = await vendorCommodityRepository.GetVendorCommodityAsync(vendorId, commodityCode);
            Assert.IsNull(result);
        }

        [TestMethod]        
        public async Task VendorCommodityRepository_GetVendorCommodityAsync_ReadRecordAsyncReturnsNull()
        {
            _dataReaderMock.Setup(repo => repo.ReadRecordAsync<VendorCommodities>(expectedRecordKey, true)).ReturnsAsync(() => null);
            var result = await vendorCommodityRepository.GetVendorCommodityAsync(vendorId, commodityCode);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task VendorCommodityRepository_GetVendorCommodityAsync_ReturnsSearchResultSuccess()
        {
            var result = await vendorCommodityRepository.GetVendorCommodityAsync(vendorId, commodityCode);

            Assert.AreEqual(result.Id, vendorCommodityEntity.Id);
            Assert.AreEqual(result.StdPrice, vendorCommodityEntity.StdPrice);
            Assert.AreEqual(result.StdPriceDate, vendorCommodityEntity.StdPriceDate);
        }

        #endregion
       
    }
}
