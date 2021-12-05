// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// A procurement document field
    /// </summary>
    [Serializable]
    public class ProcurementDocumentField : CodeItem
    {
        /// <summary>
        /// Field requirement type
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// The type of field
        /// </summary
        public string Type
        {
            get
            {
                var upperCode = Code.ToUpperInvariant();
                switch (upperCode)
                {
                    case "NEEDED_BY_DATE":
                        return ProcurementDocumentFieldTypes.NeededByDate;
                    case "EXPECTED_DATE":
                        return ProcurementDocumentFieldTypes.ExpectedDate;
                    case "DESIRED_DATE":
                        return ProcurementDocumentFieldTypes.DesiredDate;
                    case "REIMBURSE_MYSELF":
                        return ProcurementDocumentFieldTypes.ReimburseMySelf;
                    case "COMMODITY_CODE":
                        return ProcurementDocumentFieldTypes.CommodityCode;
                    case "VENDOR_PART":
                        return ProcurementDocumentFieldTypes.VendorPart;
                    case "UNIT":
                        return ProcurementDocumentFieldTypes.Unit;
                    case "PROJECT":
                        return ProcurementDocumentFieldTypes.Project;
                    case "PRINTED_COMMENTS":
                        return ProcurementDocumentFieldTypes.PrintedComments;
                    case "INTERNAL_COMMENTS":
                        return ProcurementDocumentFieldTypes.InternalComments;
                    case "TAX_FORM_DETAIL":
                        return ProcurementDocumentFieldTypes.TaxFormDetail;
                    case "TRADE_DISCOUNT_AMOUNT":
                        return ProcurementDocumentFieldTypes.TradeDiscountAmount;
                    case "TRADE_DISCOUNT_PERCENT":
                        return ProcurementDocumentFieldTypes.TradeDiscountPercent;
                    case "FIXED_ASSET":
                        return ProcurementDocumentFieldTypes.FixedAsset;
                    case "LINE_ITM_COMMENT":
                        return ProcurementDocumentFieldTypes.LineItemComments;
                    case "TAX_CODES":
                        return ProcurementDocumentFieldTypes.TaxCodes;
                    default:
                        return ProcurementDocumentFieldTypes.Unknown;
                }
            }
        }

        /// <summary>
        /// Constructor for a procurement document field
        /// </summary>
        /// <param name="code">Code for the field</param>
        /// <param name="desc">Description of the field</param>
        /// <param name="isVisible">Field requirement type</param>
        public ProcurementDocumentField(string code, string desc, bool isVisible)
            : base(code, desc)
        {
            this.IsVisible = isVisible;
        }
    }
}
