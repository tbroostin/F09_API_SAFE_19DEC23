﻿// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ColleagueFinanceWebConfigurationsRepositoryTests : BaseRepositorySetup
    {
        protected ColleagueFinanceWebConfigurationsRepository colleagueFinanceWebConfigurationsRepository;
        protected TestColleagueFinanceWebConfigurationsRepository expectedRepository;
        private CfwebDefaults cfWebDefaultsDataContract;
        private PurDefaults purDefaultsDataContract;
        private CfDocAttachParms cfDocAttachParmsDataContract;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            expectedRepository = new TestColleagueFinanceWebConfigurationsRepository();
            colleagueFinanceWebConfigurationsRepository = BuildMockColleagueFinanceWebConfigurationsRepository();
            this.cfWebDefaultsDataContract = new CfwebDefaults();
            this.cfWebDefaultsDataContract.CfwebEmailType = "PRI";
            this.cfWebDefaultsDataContract.CfwebApType = "AP";
            this.cfWebDefaultsDataContract.CfwebReqGlRequired = "Y";
            this.cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "Y";
            this.cfWebDefaultsDataContract.CfwebReqDesiredDays = 7;
            this.cfWebDefaultsDataContract.CfwebPoGlRequired = "Y";
            this.cfWebDefaultsDataContract.CfwebPoAllowMiscVendor = "Y";
            this.cfWebDefaultsDataContract.CfwebCkrApprovalFlag = "Y";
            this.cfWebDefaultsDataContract.CfwebCkrAllowMiscVendor = "Y";
            this.cfWebDefaultsDataContract.CfwebCkrApType = "AP2";
            this.cfWebDefaultsDataContract.CfwebCkrGlRequired = "Y";
            this.cfWebDefaultsDataContract.CfwebCkrReqInvoiceNo = "N";
            this.cfWebDefaultsDataContract.CfwebTaxCodes = new List<string> { "GS", "PS", "FL1" };
            this.cfWebDefaultsDataContract.CfwebApTypes = new List<string> { "AP", "AP2", "AP3" };
            this.cfWebDefaultsDataContract.CfwebCkrApTypes = new List<string> { "CAD", "EUR", "AP" };
            this.purDefaultsDataContract = new PurDefaults();
            this.purDefaultsDataContract.PurShipToCode = "MC";
            this.purDefaultsDataContract.PurReqApprovalNeededFlag = "N";
            this.purDefaultsDataContract.PurPoApprovalNeededFlag = "A";
            this.cfDocAttachParmsDataContract = new CfDocAttachParms()
            {
                CfDocAttachVoucherCol = "VOUCHERS",
                CfDocAttachPoCol = "PORDERS",
                CfDocAttachReqCol = "REQS",
                CfDocAttachVoucherReqd = "Y",
                CfDocAttachPoReqd = "Y",
                CfDocAttachReqReqd = "Y",
                Recordkey = "101"
            };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            colleagueFinanceWebConfigurationsRepository = null;
            expectedRepository = null;
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidDefaultEmailType()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.DefaultEmailType);
            Assert.AreEqual(cfWebDefaultsExpected.Result.DefaultEmailType, cfWebDefaultsActual.Result.DefaultEmailType);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidDefaultEmailType()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.DefaultEmailType = "";
            cfWebDefaultsDataContract.CfwebEmailType = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.DefaultEmailType, string.Empty);
            Assert.AreEqual(cfWebDefaultsExpected.Result.DefaultEmailType, cfWebDefaultsActual.Result.DefaultEmailType);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidDefaultAPTypeCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.DefaultAPTypeCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.DefaultAPTypeCode, cfWebDefaultsActual.Result.DefaultAPTypeCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidDefaultAPTypeCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsDataContract.CfwebApType = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.DefaultAPTypeCode, null);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidDefaultShipToCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode, cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidDefaultShipToCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode = "";
            purDefaultsDataContract.PurShipToCode = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.DefaultShipToCode, cfWebDefaultsActual.Result.PurchasingDefaults.DefaultShipToCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqGlRequired_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqGlRequired = false;
            cfWebDefaultsDataContract.CfwebReqGlRequired = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebReqGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqGlRequired = false;
            cfWebDefaultsDataContract.CfwebReqGlRequired = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebReqGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqGlRequired, cfWebDefaultsActual.Result.CfWebReqGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebReqAllowMiscVendor_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebReqAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebReqAllowMiscVendor = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqAllowMiscVendor, cfWebDefaultsActual.Result.CfWebReqAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfWebReqDesiredDays()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebReqDesiredDays);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqDesiredDays, cfWebDefaultsActual.Result.CfWebReqDesiredDays);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfWebReqDesiredDays()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebReqDesiredDays = null;
            cfWebDefaultsDataContract.CfwebReqDesiredDays = null;
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNull(cfWebDefaultsActual.Result.CfWebReqDesiredDays);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebReqDesiredDays, cfWebDefaultsActual.Result.CfWebReqDesiredDays);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebPoGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebPoGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoGlRequired, cfWebDefaultsActual.Result.CfWebPoGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebPoGlRequired_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebPoGlRequired = false;
            cfWebDefaultsDataContract.CfwebPoGlRequired = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebPoGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoGlRequired, cfWebDefaultsActual.Result.CfWebPoGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebPoGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebPoGlRequired = false;
            cfWebDefaultsDataContract.CfwebPoGlRequired = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebPoGlRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoGlRequired, cfWebDefaultsActual.Result.CfWebPoGlRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebPoAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoAllowMiscVendor, cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebPoAllowMiscVendor_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebPoAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebPoAllowMiscVendor = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoAllowMiscVendor, cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebPoAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.CfWebPoAllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebPoAllowMiscVendor = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.CfWebPoAllowMiscVendor, cfWebDefaultsActual.Result.CfWebPoAllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebCkrGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.GlRequiredForVoucher, cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebCkrGlRequired_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.RequestPaymentDefaults.GlRequiredForVoucher = false;
            cfWebDefaultsDataContract.CfwebCkrGlRequired = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.GlRequiredForVoucher, cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebCkrGlRequired()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.RequestPaymentDefaults.GlRequiredForVoucher = false;
            cfWebDefaultsDataContract.CfwebCkrGlRequired = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.GlRequiredForVoucher, cfWebDefaultsActual.Result.RequestPaymentDefaults.GlRequiredForVoucher);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebCkrAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.AllowMiscVendor, cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidCfwebCkrAllowMiscVendor_No()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.RequestPaymentDefaults.AllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebCkrAllowMiscVendor = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.AllowMiscVendor, cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidCfwebCkrAllowMiscVendor()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.RequestPaymentDefaults.AllowMiscVendor = false;
            cfWebDefaultsDataContract.CfwebCkrAllowMiscVendor = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsFalse(cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.AllowMiscVendor, cfWebDefaultsActual.Result.RequestPaymentDefaults.AllowMiscVendor);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidVoucherDefaultAPTypeCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RequestPaymentDefaults.DefaultAPTypeCode);
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.DefaultAPTypeCode, cfWebDefaultsActual.Result.RequestPaymentDefaults.DefaultAPTypeCode);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidVoucherDefaultAPTypeCode()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsDataContract.CfwebCkrApType = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.RequestPaymentDefaults.DefaultAPTypeCode, null);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidRequisitionApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.IsRequisitionApprovalNeeded, cfWebDefaultsActual.Result.PurchasingDefaults.IsRequisitionApprovalNeeded);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidRequisitionApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.PurchasingDefaults.IsRequisitionApprovalNeeded = false;
            cfWebDefaultsDataContract.CfwebCkrApprovalFlag = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.IsRequisitionApprovalNeeded, cfWebDefaultsActual.Result.PurchasingDefaults.IsRequisitionApprovalNeeded);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidPoApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.IsPOApprovalNeeded, cfWebDefaultsActual.Result.PurchasingDefaults.IsPOApprovalNeeded);
        }

        public void GetColleagueFinanceWebConfigurations_InvalidPoApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.PurchasingDefaults.IsPOApprovalNeeded = false;
            cfWebDefaultsDataContract.CfwebCkrApprovalFlag = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.PurchasingDefaults.IsPOApprovalNeeded, cfWebDefaultsActual.Result.PurchasingDefaults.IsPOApprovalNeeded);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ValidVoucherApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.IsVoucherApprovalNeeded, cfWebDefaultsActual.Result.RequestPaymentDefaults.IsVoucherApprovalNeeded);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_InvalidVoucherApprovalNeeded()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.RequestPaymentDefaults.IsVoucherApprovalNeeded = false;
            cfWebDefaultsDataContract.CfwebCkrApprovalFlag = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.IsVoucherApprovalNeeded, cfWebDefaultsActual.Result.RequestPaymentDefaults.IsVoucherApprovalNeeded);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_CfDocAttachParmsIsUnAssigned()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS", true)).Returns(() =>
            {
                return Task.FromResult(new CfDocAttachParms() { });
            });
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(null, cfWebDefaultsActual.Result.VoucherAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.PurchaseOrderAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.RequisitionAttachmentCollectionId);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_CfDocAttachParm_VouchersValid()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS", true)).Returns(() =>
            {
                return Task.FromResult(new CfDocAttachParms()
                {
                    CfDocAttachVoucherCol = "VOUCHERS"
                });
            });
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual("VOUCHERS", cfWebDefaultsActual.Result.VoucherAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.PurchaseOrderAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.RequisitionAttachmentCollectionId);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_VouAttachmentRequiredIsY()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsTrue(cfWebDefaultsActual.Result.AreVoucherAttachmentsRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreVoucherAttachmentsRequired, cfWebDefaultsActual.Result.AreVoucherAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_VouAttachmentRequiredIsN()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.AreVoucherAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachVoucherReqd = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreVoucherAttachmentsRequired, cfWebDefaultsActual.Result.AreVoucherAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_VouAttachmentRequiredIsEmpty()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.AreVoucherAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachVoucherReqd = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreVoucherAttachmentsRequired, cfWebDefaultsActual.Result.AreVoucherAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_CfDocAttachParm_PurchaseOrdersValid()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS", true)).Returns(() =>
            {
                return Task.FromResult(new CfDocAttachParms()
                {
                    CfDocAttachPoCol = "PORDERS"
                });
            });
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual("PORDERS", cfWebDefaultsActual.Result.PurchaseOrderAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.VoucherAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.RequisitionAttachmentCollectionId);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_PoAttachmentRequiredIsY()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsTrue(cfWebDefaultsActual.Result.ArePurchaseOrderAttachmentsRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.ArePurchaseOrderAttachmentsRequired, cfWebDefaultsActual.Result.ArePurchaseOrderAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_PoAttachmentRequiredIsN()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.ArePurchaseOrderAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachPoReqd = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.ArePurchaseOrderAttachmentsRequired, cfWebDefaultsActual.Result.ArePurchaseOrderAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_PoAttachmentRequiredIsEmpty()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.ArePurchaseOrderAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachPoReqd = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.ArePurchaseOrderAttachmentsRequired, cfWebDefaultsActual.Result.ArePurchaseOrderAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_CfDocAttachParm_RequisitionsValid()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS", true)).Returns(() =>
            {
                return Task.FromResult(new CfDocAttachParms()
                {
                    CfDocAttachReqCol = "REQUISITIONS"
                });
            });
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual("REQUISITIONS", cfWebDefaultsActual.Result.RequisitionAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.VoucherAttachmentCollectionId);
            Assert.AreEqual(null, cfWebDefaultsActual.Result.PurchaseOrderAttachmentCollectionId);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ReqAttachmentRequiredIsY()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsTrue(cfWebDefaultsActual.Result.AreRequisitionAttachmentsRequired);
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreRequisitionAttachmentsRequired, cfWebDefaultsActual.Result.AreRequisitionAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ReqAttachmentRequiredIsN()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.AreRequisitionAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachReqReqd = "N";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreRequisitionAttachmentsRequired, cfWebDefaultsActual.Result.AreRequisitionAttachmentsRequired);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_ReqAttachmentRequiredIsEmpty()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            cfWebDefaultsExpected.Result.AreRequisitionAttachmentsRequired = false;
            cfDocAttachParmsDataContract.CfDocAttachReqReqd = "";
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsExpected.Result.AreRequisitionAttachmentsRequired, cfWebDefaultsActual.Result.AreRequisitionAttachmentsRequired);
        }
        //
        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_Valid_PUWP_RestrictToAPTypeCodes()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RestrictToListedApTypeCodes);
            CollectionAssert.AreEqual(cfWebDefaultsExpected.Result.RestrictToListedApTypeCodes.ToList(), cfWebDefaultsActual.Result.RestrictToListedApTypeCodes.ToList());
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_Empty_PUWP_RestrictToAPTypeCodes()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            this.cfWebDefaultsDataContract.CfwebApTypes = new List<string> { };
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.RestrictToListedApTypeCodes, null);
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_Valid_RPYP_RestrictToAPTypeCodes()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.IsNotNull(cfWebDefaultsActual.Result.RestrictToListedApTypeCodes);
            CollectionAssert.AreEqual(cfWebDefaultsExpected.Result.RequestPaymentDefaults.RestrictToListedApTypeCodes.ToList(), cfWebDefaultsActual.Result.RequestPaymentDefaults.RestrictToListedApTypeCodes.ToList());
        }

        [TestMethod]
        public void GetColleagueFinanceWebConfigurations_Empty_RPYP_RestrictToAPTypeCodes()
        {
            var cfWebDefaultsExpected = expectedRepository.GetColleagueFinanceWebConfigurations();
            this.cfWebDefaultsDataContract.CfwebCkrApTypes = new List<string> { };
            var cfWebDefaultsActual = colleagueFinanceWebConfigurationsRepository.GetColleagueFinanceWebConfigurations();
            Assert.AreEqual(cfWebDefaultsActual.Result.RequestPaymentDefaults.RestrictToListedApTypeCodes, null);
        }

        private ColleagueFinanceWebConfigurationsRepository BuildMockColleagueFinanceWebConfigurationsRepository()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(cfWebDefaultsDataContract);
            });
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.PurDefaults>("CF.PARMS", "PUR.DEFAULTS", true)).Returns(() =>
            {
                return Task.FromResult(purDefaultsDataContract);
            });

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS", true)).Returns(() =>
            {
                return Task.FromResult(cfDocAttachParmsDataContract);
            });

            ColleagueFinanceWebConfigurationsRepository repository = new ColleagueFinanceWebConfigurationsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return repository;
        }

    }
}
