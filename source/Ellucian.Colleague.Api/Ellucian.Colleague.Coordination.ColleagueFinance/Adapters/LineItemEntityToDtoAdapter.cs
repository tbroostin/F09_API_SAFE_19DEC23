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
            
            // Translate the domain status into the DTO status
            switch (Source.LineItemStatus)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Accepted:
                    lineItemDto.LineItemStatus = LineItemStatus.Accepted;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Backordered:
                    lineItemDto.LineItemStatus = LineItemStatus.Backordered;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Closed:
                    lineItemDto.LineItemStatus = LineItemStatus.Closed;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Invoiced:
                    lineItemDto.LineItemStatus = LineItemStatus.Invoiced;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Outstanding:
                    lineItemDto.LineItemStatus = LineItemStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Paid:
                    lineItemDto.LineItemStatus = LineItemStatus.Paid;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Reconciled:
                    lineItemDto.LineItemStatus = LineItemStatus.Reconciled;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Voided:
                    lineItemDto.LineItemStatus = LineItemStatus.Voided;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemStatus.Hold:
                    lineItemDto.LineItemStatus = LineItemStatus.Hold;
                    break;
                default:
                    {
                        // if we get here, we have no status associated to the line item
                        lineItemDto.LineItemStatus = LineItemStatus.None;                       
                        break;
                    }
            }

            return lineItemDto;
        }
    }
}

