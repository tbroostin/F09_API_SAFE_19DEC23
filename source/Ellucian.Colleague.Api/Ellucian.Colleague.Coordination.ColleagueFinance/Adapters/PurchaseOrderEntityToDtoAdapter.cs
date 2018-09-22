// Copyright 2015 Ellucian Company L.P. and its affiliates.

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
    /// Adapter for mapping the Purchase Order entity into DTOs
    /// </summary>
    public class PurchaseOrderEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrder, Ellucian.Colleague.Dtos.ColleagueFinance.PurchaseOrder>
    {
        public PurchaseOrderEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a purchase order domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Source purchase order domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers</param>
        /// <returns>Purchase Order DTO</returns>
        public PurchaseOrder MapToType(Domain.ColleagueFinance.Entities.PurchaseOrder Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the purchase order level properties
            var purchaseOrderDto = new Dtos.ColleagueFinance.PurchaseOrder();
            purchaseOrderDto.Id = Source.Id;
            purchaseOrderDto.Number = Source.Number;
            purchaseOrderDto.Date = Source.Date;
            purchaseOrderDto.StatusDate = Source.StatusDate;
            purchaseOrderDto.VendorId = Source.VendorId;
            purchaseOrderDto.VendorName = Source.VendorName;
            purchaseOrderDto.Amount = Source.Amount;
            purchaseOrderDto.CurrencyCode = Source.CurrencyCode;
            purchaseOrderDto.DeliveryDate = Source.DeliveryDate;
            purchaseOrderDto.MaintenanceDate = Source.MaintenanceDate;
            purchaseOrderDto.InitiatorName = Source.InitiatorName;
            purchaseOrderDto.RequestorName = Source.RequestorName;
            purchaseOrderDto.ApType = Source.ApType;
            purchaseOrderDto.ShipToCodeName = Source.ShipToCodeName;
            purchaseOrderDto.Comments = Source.Comments;
            purchaseOrderDto.InternalComments = Source.InternalComments;

            purchaseOrderDto.Requisitions = new List<string>();
            foreach (var req in Source.Requisitions)
            {
                purchaseOrderDto.Requisitions.Add(req);
            }

            purchaseOrderDto.Vouchers = new List<string>();
            foreach (var vou in Source.Vouchers)
            {
                purchaseOrderDto.Vouchers.Add(vou);
            }

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Accepted:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Accepted;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Backordered:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Backordered;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Closed:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Closed;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.InProgress:
                    purchaseOrderDto.Status = PurchaseOrderStatus.InProgress;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Invoiced:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Invoiced;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.NotApproved:
                    purchaseOrderDto.Status = PurchaseOrderStatus.NotApproved;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Outstanding:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Paid:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Paid;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Reconciled:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Reconciled;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.PurchaseOrderStatus.Voided:
                    purchaseOrderDto.Status = PurchaseOrderStatus.Voided;
                    break;
            }

            purchaseOrderDto.LineItems = new List<Dtos.ColleagueFinance.LineItem>();
            purchaseOrderDto.Approvers = new List<Dtos.ColleagueFinance.Approver>();

            // Initialize all necessary adapters to convert the descendent elements within the purchase order

            var lineItemDtoAdapter = new LineItemEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemGlDistributionDtoAdapter = new LineItemGlDistributionEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemTaxesDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.LineItemTax, Dtos.ColleagueFinance.LineItemTax>(adapterRegistry, logger);
            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);

            // Convert the purchase order line item domain entities into DTOS
            foreach (var lineItem in Source.LineItems)
            {
                // First convert the properties to DTOs
                var lineItemDto = lineItemDtoAdapter.MapToType(lineItem);

                // Now convert each GL distribution domain entity into a DTO
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    var glDistributionDto = lineItemGlDistributionDtoAdapter.MapToType(glDistribution, glMajorComponentStartPositions);

                    // Add the GL distribution DTOs to the line item DTO
                    lineItemDto.GlDistributions.Add(glDistributionDto);
                }

                // Now convert each line item tax domain entity into a DTO
                foreach (var lineItemTax in lineItem.LineItemTaxes)
                {
                    var lineItemTaxesDto = lineItemTaxesDtoAdapter.MapToType(lineItemTax);

                    // Add the line item taxes DTOs to the line item DTO
                    lineItemDto.LineItemTaxes.Add(lineItemTaxesDto);
                }

                // Add the purchaseOrder line item DTO to the purchaseOrder DTO
                purchaseOrderDto.LineItems.Add(lineItemDto);
            }

            // Convert the purchase order approver domain entities into DTOS
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the purchase order approver DTO to the purchaseOrder DTO
                purchaseOrderDto.Approvers.Add(approverDto);
            }

            return purchaseOrderDto;
        }
    }
}
