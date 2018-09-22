// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class RepositoryTestsTemplate : BaseRepositorySetup
    {
        #region Initialize and Cleanup
        private VoucherRepository actualRepository;
        private Vouchers voucherDataContract;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();
            actualRepository = new VoucherRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            // Set up the data contract(s)
            voucherDataContract = new Vouchers() { Recordkey = "V0000001" };

            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
        }
        #endregion

        #region Tests
        [TestMethod]
        public void TestMethod()
        {
            // Method body
        }
        #endregion

        #region Private methods
        private void InitializeMockStatements()
        {
            // Using voucher for the example in the template.
            //dataReaderMock.Setup(acc => acc.ReadRecordAsync<Vouchers>(It.IsAny<string>(), true)).Returns(() =>
            //{
            //    return Task.FromResult(voucherDataContract);
            //});

            //transManagerMock.Setup(tio => tio.ExecuteAsync<GetGlAccountDescriptionRequest, GetGlAccountDescriptionResponse>(It.IsAny<GetGlAccountDescriptionRequest>())).Returns(() =>
            //{
            //    return Task.FromResult(glAccountsDescriptionResponse);
            //});
        }
        #endregion
    }
}