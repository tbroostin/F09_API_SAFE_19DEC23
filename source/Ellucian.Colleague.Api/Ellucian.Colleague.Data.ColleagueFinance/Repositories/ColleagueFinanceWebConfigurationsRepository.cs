// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

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
            ColleagueFinanceWebConfiguration cfWebConfigurationEntity = new ColleagueFinanceWebConfiguration();
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
                    cfWebConfigurationEntity.CfWebReqGlRequired = cfWebDefaults.CfwebReqGlRequired.ToUpper() == "Y";
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

                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebProcShowGlBalance))
                {
                    cfWebConfigurationEntity.DisplayGlBalance = cfWebDefaults.CfwebProcShowGlBalance.ToUpper() == "Y";
                }

                VoucherWebConfiguration requestPaymentConfiguration = new VoucherWebConfiguration();
                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebCkrApType))
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
                if(cfWebDefaults.CfwebCkrApTypes != null && cfWebDefaults.CfwebCkrApTypes.Any())
                {
                    requestPaymentConfiguration.RestrictToListedApTypeCodes = cfWebDefaults.CfwebCkrApTypes;
                }

                cfWebConfigurationEntity.RequestPaymentDefaults = requestPaymentConfiguration;

                if (purchaseDefaults != null)
                {
                    cfWebConfigurationEntity.PurchasingDefaults = new PurchasingDefaults();
                    cfWebConfigurationEntity.PurchasingDefaults.DefaultShipToCode = purchaseDefaults.PurShipToCode;
                    cfWebConfigurationEntity.PurchasingDefaults.IsRequisitionApprovalNeeded = purchaseDefaults.PurReqApprovalNeededFlag.ToUpper() == "Y" || purchaseDefaults.PurReqApprovalNeededFlag.ToUpper() == "A";
                    cfWebConfigurationEntity.PurchasingDefaults.IsPOApprovalNeeded = purchaseDefaults.PurPoApprovalNeededFlag.ToUpper() == "Y" || purchaseDefaults.PurPoApprovalNeededFlag.ToUpper() == "A";
                }

                if (attachmentParameters != null)
                {
                    cfWebConfigurationEntity.VoucherAttachmentCollectionId = attachmentParameters.CfDocAttachVoucherCol;
                    cfWebConfigurationEntity.PurchaseOrderAttachmentCollectionId = attachmentParameters.CfDocAttachPoCol;
                    cfWebConfigurationEntity.RequisitionAttachmentCollectionId = attachmentParameters.CfDocAttachReqCol;
                    cfWebConfigurationEntity.AreVoucherAttachmentsRequired = attachmentParameters.CfDocAttachVoucherReqd != null && attachmentParameters.CfDocAttachVoucherReqd.ToUpper() == "Y";
                    cfWebConfigurationEntity.ArePurchaseOrderAttachmentsRequired = attachmentParameters.CfDocAttachPoReqd != null && attachmentParameters.CfDocAttachPoReqd.ToUpper() == "Y";
                    cfWebConfigurationEntity.AreRequisitionAttachmentsRequired = attachmentParameters.CfDocAttachReqReqd != null && attachmentParameters.CfDocAttachReqReqd.ToUpper() == "Y";
                }

                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebApType))
                {
                    cfWebConfigurationEntity.DefaultAPTypeCode = cfWebDefaults.CfwebApType;
                }
                if (cfWebDefaults.CfwebApTypes != null && cfWebDefaults.CfwebApTypes.Any())
                {
                    cfWebConfigurationEntity.RestrictToListedApTypeCodes = cfWebDefaults.CfwebApTypes;
                }

                if (!string.IsNullOrEmpty(cfWebDefaults.CfwebAllowNonExpGlAcc))
                {
                    cfWebConfigurationEntity.AllowNonExpenseGlAccounts = cfWebDefaults.CfwebAllowNonExpGlAcc.ToUpper() == "Y";
                }

                //Populate document field display requirements set in PDWP
                GetProcurementDocumentFieldConfigurations(cfWebDefaults, cfWebConfigurationEntity);
                logger.Debug("Constructed cfWebConfigurationEntity:");

                logger.Debug("cfWebConfigurationEntity.DefaultEmailType ==>" + cfWebConfigurationEntity.DefaultEmailType);
                logger.Debug("cfWebConfigurationEntity.CfWebReqDesiredDays ==>" + cfWebConfigurationEntity.CfWebReqDesiredDays);
                logger.Debug("cfWebConfigurationEntity.CfWebReqGlRequired ==>" + cfWebConfigurationEntity.CfWebReqGlRequired);
                logger.Debug("cfWebConfigurationEntity.CfWebReqAllowMiscVendor ==>" + cfWebConfigurationEntity.CfWebReqAllowMiscVendor);
                logger.Debug("cfWebConfigurationEntity.CfWebPoGlRequired ==>" + cfWebConfigurationEntity.CfWebPoGlRequired);
                logger.Debug("cfWebConfigurationEntity.CfWebPoAllowMiscVendor ==>" + cfWebConfigurationEntity.CfWebPoAllowMiscVendor);
                logger.Debug("cfWebConfigurationEntity.DefaultTaxCodes ==>" + cfWebConfigurationEntity.DefaultTaxCodes);
                logger.Debug("cfWebConfigurationEntity.DisplayGlBalance ==>" + cfWebConfigurationEntity.DisplayGlBalance);

                logger.Debug("cfWebConfigurationEntity.RequestPaymentDefaults ==>" + cfWebConfigurationEntity.RequestPaymentDefaults);

                logger.Debug("cfWebConfigurationEntity.PurchasingDefaults.DefaultShipToCode ==>" + cfWebConfigurationEntity.PurchasingDefaults.DefaultShipToCode);
                logger.Debug("cfWebConfigurationEntity.PurchasingDefaults.IsRequisitionApprovalNeeded ==>" + cfWebConfigurationEntity.PurchasingDefaults.IsRequisitionApprovalNeeded);
                logger.Debug("cfWebConfigurationEntity.PurchasingDefaults.IsPOApprovalNeeded ==>" + cfWebConfigurationEntity.PurchasingDefaults.IsPOApprovalNeeded);

                logger.Debug("cfWebConfigurationEntity.VoucherAttachmentCollectionId ==>" + cfWebConfigurationEntity.VoucherAttachmentCollectionId);
                logger.Debug("cfWebConfigurationEntity.PurchaseOrderAttachmentCollectionId ==>" + cfWebConfigurationEntity.PurchaseOrderAttachmentCollectionId);
                logger.Debug("cfWebConfigurationEntity.RequisitionAttachmentCollectionId ==>" + cfWebConfigurationEntity.RequisitionAttachmentCollectionId);
                logger.Debug("cfWebConfigurationEntity.AreVoucherAttachmentsRequired ==>" + cfWebConfigurationEntity.AreVoucherAttachmentsRequired);
                logger.Debug("cfWebConfigurationEntity.ArePurchaseOrderAttachmentsRequired ==>" + cfWebConfigurationEntity.ArePurchaseOrderAttachmentsRequired);
                logger.Debug("cfWebConfigurationEntity.AreRequisitionAttachmentsRequired ==>" + cfWebConfigurationEntity.AreRequisitionAttachmentsRequired);

                logger.Debug("cfWebConfigurationEntity.DefaultAPTypeCode ==>" + cfWebConfigurationEntity.DefaultAPTypeCode);
                logger.Debug("cfWebConfigurationEntity.AllowNonExpenseGlAccounts ==>" + cfWebConfigurationEntity.AllowNonExpenseGlAccounts);
                logger.Debug("cfWebConfigurationEntity.RestrictToListedApTypeCodes ==>" + cfWebConfigurationEntity.RestrictToListedApTypeCodes);

                logger.Debug("cfWebConfigurationEntity.RequisitionFieldRequirements ==>" + cfWebConfigurationEntity.RequisitionFieldRequirements);
                logger.Debug("cfWebConfigurationEntity.PurchaseOrderFieldRequirements ==>" + cfWebConfigurationEntity.PurchaseOrderFieldRequirements);
                logger.Debug("cfWebConfigurationEntity.VoucherFieldRequirements ==>" + cfWebConfigurationEntity.VoucherFieldRequirements);
            }
            return cfWebConfigurationEntity;
        }

        /// <summary>
        /// Gets the flag that indicates if Justification Notes should be displayed to the user.
        /// </summary>
        /// <returns>Boolean: true = display Justification Notes, false = do not display Justification Notes.</returns>
        public async Task<bool> GetShowJustificationNotesFlagAsync()
        {
            try
            {
                // Get parameter value
                var budgetDevDefaults = await DataReader.ReadRecordAsync<BudgetDevDefaults>("CF.PARMS", "BUDGET.DEV.DEFAULTS");

                if (budgetDevDefaults != null && !string.IsNullOrWhiteSpace(budgetDevDefaults.BudDevShowNotes) &&
                    budgetDevDefaults.BudDevShowNotes.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
                return false;
            }
            catch(Exception e)
            {
                logger.Error(e, "Error accessing BUDGET.DEV.DEFAULTS file.");
                return false;
            }
        }

        /// <summary>
        /// Gets Procurement document field configurations
        /// </summary>
        /// <returns></returns>
        private void GetProcurementDocumentFieldConfigurations(CfwebDefaults cfWebDefaults, ColleagueFinanceWebConfiguration cfWebConfigurationEntity)
        {
            List<ProcurementDocumentField> requisitionFields = new List<ProcurementDocumentField>();
            List<ProcurementDocumentField> purchaseOrderFields = new List<ProcurementDocumentField>();
            List<ProcurementDocumentField> voucherFields = new List<ProcurementDocumentField>();

            if (cfWebDefaults != null && cfWebDefaults.CfwebProcDocFieldsEntityAssociation != null && cfWebDefaults.CfwebProcDocFieldsEntityAssociation.Any())
            {
                foreach (var field in cfWebDefaults.CfwebProcDocFieldsEntityAssociation)
                {
                    if (!string.IsNullOrEmpty(field.CfwebProcDocElementAssocMember) && !string.IsNullOrEmpty(field.CfwebProcDocElementAssocMember))
                    {
                        if (!string.IsNullOrEmpty(field.CfwebProcDocReqApplicablAssocMember) && field.CfwebProcDocReqApplicablAssocMember.ToUpper() == "Y")
                        {
                            requisitionFields.Add(new ProcurementDocumentField(field.CfwebProcDocElementAssocMember, field.CfwebProcDocDescAssocMember,
                                             ConvertToFieldDisplayRequirement(field.CfwebProcDocReqRqmtAssocMember)));
                        }

                        if (!string.IsNullOrEmpty(field.CfwebProcDocPoApplicablAssocMember) && field.CfwebProcDocPoApplicablAssocMember.ToUpper() == "Y")
                        {
                            purchaseOrderFields.Add(new ProcurementDocumentField(field.CfwebProcDocElementAssocMember, field.CfwebProcDocDescAssocMember,
                                             ConvertToFieldDisplayRequirement(field.CfwebProcDocPoRqmtAssocMember)));
                        }

                        if (!string.IsNullOrEmpty(field.CfwebProcDocVouApplicablAssocMember) && field.CfwebProcDocVouApplicablAssocMember.ToUpper() == "Y")
                        {
                            voucherFields.Add(new ProcurementDocumentField(field.CfwebProcDocElementAssocMember, field.CfwebProcDocDescAssocMember,
                                             ConvertToFieldDisplayRequirement(field.CfwebProcDocVouRqmtAssocMember)));
                        }
                    }
                }
            }
            cfWebConfigurationEntity.RequisitionFieldRequirements = requisitionFields;
            cfWebConfigurationEntity.PurchaseOrderFieldRequirements = purchaseOrderFields;
            cfWebConfigurationEntity.VoucherFieldRequirements = voucherFields;            
        }

        private bool ConvertToFieldDisplayRequirement(string requirement)
        {
            return !string.IsNullOrEmpty(requirement) && requirement.ToUpper() == "D";
        }
    }
}
