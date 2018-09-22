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
    /// Adapter for mapping from the requisition entity into DTOs
    /// </summary>
    public class RequisitionEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.Requisition, Ellucian.Colleague.Dtos.ColleagueFinance.Requisition>
    {
        public RequisitionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a requisition domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Source requisition domain entity to be converted</param>
        /// <returns>Requisition DTO</returns>
        public Requisition MapToType(Domain.ColleagueFinance.Entities.Requisition Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the requisition level properties
            var requisitionDto = new Dtos.ColleagueFinance.Requisition();
            requisitionDto.Id = Source.Id;
            requisitionDto.Number = Source.Number;
            requisitionDto.Amount = Source.Amount;
            requisitionDto.ApType = Source.ApType;
            requisitionDto.BlanketPurchaseOrder = Source.BlanketPurchaseOrder;
            requisitionDto.Comments = Source.Comments;
            requisitionDto.CurrencyCode = Source.CurrencyCode;
            requisitionDto.Date = Source.Date;
            requisitionDto.DesiredDate = Source.DesiredDate;
            requisitionDto.InitiatorName = Source.InitiatorName;
            requisitionDto.InternalComments = Source.InternalComments;
            requisitionDto.MaintenanceDate = Source.MaintenanceDate;
            requisitionDto.RequestorName = Source.RequestorName;
            requisitionDto.ShipToCode = Source.ShipToCode;
            requisitionDto.StatusDate = Source.StatusDate;
            requisitionDto.VendorId = Source.VendorId;
            requisitionDto.VendorName = Source.VendorName;

            requisitionDto.PurchaseOrders = new List<string>();
            if ((Source.PurchaseOrders != null) && (Source.PurchaseOrders.Count > 0))
            {
                foreach (var po in Source.PurchaseOrders)
                {
                    requisitionDto.PurchaseOrders.Add(po);
                }
            }

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RequisitionStatus.InProgress:
                    requisitionDto.Status = RequisitionStatus.InProgress;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RequisitionStatus.NotApproved:
                    requisitionDto.Status = RequisitionStatus.NotApproved;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RequisitionStatus.Outstanding:
                    requisitionDto.Status = RequisitionStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RequisitionStatus.PoCreated:
                    requisitionDto.Status = RequisitionStatus.PoCreated;
                    break;
            }

            requisitionDto.LineItems = new List<Dtos.ColleagueFinance.LineItem>();
            requisitionDto.Approvers = new List<Dtos.ColleagueFinance.Approver>();

            // Initialize all necessary adapters to convert the descendent elements within the requisition
            var lineItemDtoAdapter = new LineItemEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemGlDistributionDtoAdapter = new LineItemGlDistributionEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemTaxesDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.LineItemTax, Dtos.ColleagueFinance.LineItemTax>(adapterRegistry, logger);
            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);

            // Convert the requisition line item domain entities into DTOS
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

                // Add the requisition line item DTO to the requisition DTO
                requisitionDto.LineItems.Add(lineItemDto);
            }

            // Convert the requisition approver domain entities into DTOS
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the requisition approver DTO to the requisition DTO
                requisitionDto.Approvers.Add(approverDto);
            }

            return requisitionDto;
        }
    }
}
