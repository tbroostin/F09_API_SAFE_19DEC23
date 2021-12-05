// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance
{
    /// <summary>
    /// Defines the various types of procurement document fields
    /// </summary>
    [Serializable]
    public static class ProcurementDocumentFieldTypes
    {
        #region Fields used in Procurement Create / Modify
        
        //Needed by date
        public const string NeededByDate = "NEEDED_BY_DATE";

        //Needed by date
        public const string ExpectedDate = "EXPECTED_DATE";

        //Desired date
        public const string DesiredDate = "DESIRED_DATE";

        //Reimburse Myself
        public const string ReimburseMySelf = "REIMBURSE_MYSELF";

        //Commodity code
        public const string CommodityCode = "COMMODITY_CODE";

        //Vendor part
        public const string VendorPart = "VENDOR_PART";

        //Unit
        public const string Unit = "UNIT";

        //Project
        public const string Project = "PROJECT";

        //Printed comments
        public const string PrintedComments = "PRINTED_COMMENTS";

        //Internal comments
        public const string InternalComments = "INTERNAL_COMMENTS";

        //Tax form fields (Tax form, Box No & State)
        public const string TaxFormDetail = "TAX_FORM_DETAIL";

        //Trade discount amount
        public const string TradeDiscountAmount = "TRADE_DISCOUNT_AMOUNT";

        //Trade discount percent
        public const string TradeDiscountPercent = "TRADE_DISCOUNT_PERCENT";

        //Fixed asset
        public const string FixedAsset = "FIXED_ASSET";

        //Line item comments
        public const string LineItemComments = "LINE_ITM_COMMENT";

        //Tax Code fields
        public const string TaxCodes = "TAX_CODES";

        public const string Unknown = "UNKNOWN";

        #endregion        
    }
}
