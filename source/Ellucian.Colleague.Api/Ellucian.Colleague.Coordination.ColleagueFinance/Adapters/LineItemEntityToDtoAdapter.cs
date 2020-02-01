// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using AutoMapper;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping from the Line Item entity to DTO.
    /// </summary>
    public class LineItemEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItem, Ellucian.Colleague.Dtos.ColleagueFinance.LineItem>
    {
        public LineItemEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a line item domain entity into a DTO.
        /// </summary>
        /// <param name="Source">Line item domain entity to be converted.</param>
        /// <returns>Line item DTO.</returns>
        public LineItem MapToType(Domain.ColleagueFinance.Entities.LineItem Source)
        {
            // Copy line item properties.
            var lineItemDto = new LineItem();
            lineItemDto.Id = Source.Id;
            lineItemDto.Description = Source.Description;
            lineItemDto.Quantity = Source.Quantity;
            lineItemDto.Price = Source.Price;
            lineItemDto.ExtendedPrice = Source.ExtendedPrice;
            lineItemDto.UnitOfIssue = Source.UnitOfIssue;
            lineItemDto.VendorPart = Source.VendorPart;
            lineItemDto.ExpectedDeliveryDate = Source.ExpectedDeliveryDate;
            lineItemDto.DesiredDate = Source.DesiredDate;
            lineItemDto.InvoiceNumber = Source.InvoiceNumber;
            lineItemDto.TaxForm = Source.TaxForm;
            lineItemDto.TaxFormCode = Source.TaxFormCode;
            lineItemDto.TaxFormLocation = Source.TaxFormLocation;
            lineItemDto.Comments = Source.Comments;
            lineItemDto.CommodityCode = Source.CommodityCode;
            lineItemDto.FixedAssetsFlag = Source.FixedAssetsFlag;
            lineItemDto.TradeDiscountAmount = Source.TradeDiscountAmount;
            lineItemDto.TradeDiscountPercentage = Source.TradeDiscountPercentage;
            lineItemDto.ReqLineItemTaxCodes = new List<LineItemReqTax>();
            lineItemDto.GlDistributions = new List<LineItemGlDistribution>();
            lineItemDto.LineItemTaxes = new List<LineItemTax>();     

            return lineItemDto;
        }
    }
}

