// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Data.ColleagueFinance.Repositories
{
    /// <summary>
    /// ColleagueFinanceWebConfigurationsRepository
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class ColleagueFinanceWebConfigurationsRepository : BaseColleagueRepository, IColleagueFinanceWebConfigurationsRepository
    {
        /// The constructor to instantiate a Colleague Finance web configurations repository object
        /// </summary>
        /// <param name="cacheProvider">Pass in an ICacheProvider object</param>
        /// <param name="transactionFactory">Pass in an IColleagueTransactionFactory object</param>
        /// <param name="logger">Pass in an ILogger object</param>
        public ColleagueFinanceWebConfigurationsRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }
        /// <summary>
        /// Gets Colleague Finance web configurations
        /// </summary>
        /// <returns></returns>
        public async Task<ColleagueFinanceWebConfiguration> GetColleagueFinanceWebConfigurations()
        {
            ColleagueFinanceWebConfiguration cfWebConfigurationEntity =new ColleagueFinanceWebConfiguration();
            var cfWebDefaults = await DataReader.ReadRecordAsync<CfwebDefaults>("CF.PARMS", "CFWEB.DEFAULTS");
            var purchaseDefaults = await DataReader.ReadRecordAsync<PurDefaults>("CF.PARMS", "PUR.DEFAULTS");
            var attachmentParameters = await DataReader.ReadRecordAsync<CfDocAttachParms>("CF.PARMS", "CF.DOC.ATTACH.PARMS");
            
            if (cfWebDefaults != null)
            {
                cfWebConfigurationEntity = new ColleagueFinanceWebConfiguration();
                cfWebConfigurationEntity.DefaultEmailType = cfWebDefaults.CfwebEmailType;
                cfWebConfigurationEntity.CfWebReqDesiredDays = cfWebDefaults.CfwebReqDesiredDays;
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebReqGlRequired))
                {
                    cfWebConfigurationEntity.CfWebReqGlRequired = cfWebDefaults.CfwebReqGlRequired.ToUpper()=="Y";
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebReqAllowMiscVendor))
                {
                    cfWebConfigurationEntity.CfWebReqAllowMiscVendor = cfWebDefaults.CfwebReqAllowMiscVendor.ToUpper() == "Y"; 
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebPoGlRequired))
                {
                    cfWebConfigurationEntity.CfWebPoGlRequired = cfWebDefaults.CfwebPoGlRequired.ToUpper() == "Y";
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebPoAllowMiscVendor))
                {
                    cfWebConfigurationEntity.CfWebPoAllowMiscVendor = cfWebDefaults.CfwebPoAllowMiscVendor.ToUpper() == "Y";
                }

                if (cfWebDefaults.CfwebTaxCodes != null && cfWebDefaults.CfwebTaxCodes.Any())
                {
                    cfWebConfigurationEntity.DefaultTaxCodes = new List<string>();
                    cfWebConfigurationEntity.DefaultTaxCodes = cfWebDefaults.CfwebTaxCodes;
                }

                VoucherWebConfiguration requestPaymentConfiguration = new VoucherWebConfiguration();
                if(!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrApType))
                {
                    requestPaymentConfiguration.DefaultAPTypeCode = cfWebDefaults.CfwebCkrApType;
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrReqInvoiceNo))
                {
                    requestPaymentConfiguration.IsInvoiceEntryRequired = cfWebDefaults.CfwebCkrReqInvoiceNo.ToUpper() == "Y";
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrAllowMiscVendor))
                {
                    requestPaymentConfiguration.AllowMiscVendor = cfWebDefaults.CfwebCkrAllowMiscVendor.ToUpper() == "Y";
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrGlRequired))
                {
                    requestPaymentConfiguration.GlRequiredForVoucher = cfWebDefaults.CfwebCkrGlRequired.ToUpper() == "Y";
                }
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrApprovalFlag))
                {
                    requestPaymentConfiguration.IsVoucherApprovalNeeded = cfWebDefaults.CfwebCkrApprovalFlag.ToUpper() == "Y" || cfWebDefaults.CfwebCkrApprovalFlag.ToUpper() == "A";
                }
                cfWebConfigurationEntity.RequestPaymentDefaults = requestPaymentConfiguration;

                if (purchaseDefaults!=null)
                {
                    cfWebConfigurationEntity.PurchasingDefaults = new PurchasingDefaults();
                    cfWebConfigurationEntity.PurchasingDefaults.DefaultShipToCode = purchaseDefaults.PurShipToCode;
                    cfWebConfigurationEntity.PurchasingDefaults.IsRequisitionApprovalNeeded = purchaseDefaults.PurReqApprovalNeededFlag.ToUpper() == "Y" || purchaseDefaults.PurReqApprovalNeededFlag.ToUpper() == "A";
                    cfWebConfigurationEntity.PurchasingDefaults.IsPOApprovalNeeded = purchaseDefaults.PurPoApprovalNeededFlag.ToUpper() == "Y" || purchaseDefaults.PurPoApprovalNeededFlag.ToUpper() == "A";
                }

                if(attachmentParameters != null)
                {
                    cfWebConfigurationEntity.VoucherAttachmentCollectionId = attachmentParameters.CfDocAttachVoucherCol;
                }

                cfWebConfigurationEntity.DefaultAPTypeCode = cfWebDefaults.CfwebApType;
            }
            return cfWebConfigurationEntity;
        }
    }
}
