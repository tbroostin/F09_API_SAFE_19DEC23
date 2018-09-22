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
    /// Adapter for mapping from the blanket purchase order entity to the blanket purchase order DTO
    /// </summary>
    public class BlanketPurchaseOrderEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrder, Ellucian.Colleague.Dtos.ColleagueFinance.BlanketPurchaseOrder>
    {
        public BlanketPurchaseOrderEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a blanket purchase order domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Blanket purchase order domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers</param>
        /// <returns>Blanket purchase order DTO</returns>
        public BlanketPurchaseOrder MapToType(Domain.ColleagueFinance.Entities.BlanketPurchaseOrder Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the blanket purchase order level properties
            var bpoDto = new Dtos.ColleagueFinance.BlanketPurchaseOrder();
            bpoDto.Id = Source.Id;
            bpoDto.Number = Source.Number;
            bpoDto.Amount = Source.Amount;
            bpoDto.ApType = Source.ApType;
            bpoDto.Comments = Source.Comments;
            bpoDto.CurrencyCode = Source.CurrencyCode;
            bpoDto.Date = Source.Date;
            bpoDto.Description = Source.Description;
            bpoDto.ExpirationDate = Source.ExpirationDate;
            bpoDto.InitiatorName = Source.InitiatorName;
            bpoDto.InternalComments = Source.InternalComments;
            bpoDto.MaintenanceDate = Source.MaintenanceDate;
            bpoDto.StatusDate = Source.StatusDate;
            bpoDto.VendorId = Source.VendorId;
            bpoDto.VendorName = Source.VendorName;

            bpoDto.Requisitions = new List<string>();
            foreach (var req in Source.Requisitions)
            {
                bpoDto.Requisitions.Add(req);
            }

            bpoDto.Vouchers = new List<string>();
            foreach (var vou in Source.Vouchers)
            {
                bpoDto.Vouchers.Add(vou);
            }

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Closed:
                    bpoDto.Status = BlanketPurchaseOrderStatus.Closed;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.InProgress:
                    bpoDto.Status = BlanketPurchaseOrderStatus.InProgress;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.NotApproved:
                    bpoDto.Status = BlanketPurchaseOrderStatus.NotApproved;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Outstanding:
                    bpoDto.Status = BlanketPurchaseOrderStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderStatus.Voided:
                    bpoDto.Status = BlanketPurchaseOrderStatus.Voided;
                    break;
            }

            bpoDto.Approvers = new List<Approver>();
            bpoDto.GlDistributions = new List<BlanketPurchaseOrderGlDistribution>();

            // Initialize all necessary adapters to convert the descendent elements within the blanket purchase order

            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);
            var glDistributionDtoAdapter = new BlanketPurchaseOrderGlDistributionEntityToDtoAdapter(adapterRegistry, logger);

            // Convert the blanket purchase order approver domain entities into DTOS
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the blanket purchase order approver DTO to the blanket purchase order DTO
                bpoDto.Approvers.Add(approverDto);
            }

            // Convert the blanket purchase order GL Distribution domain entities into DTOS
            foreach (var glDist in Source.GlDistributions)
            {
                var glDistDto = glDistributionDtoAdapter.MapToType(glDist, glMajorComponentStartPositions);

                // Add the blanket purchase order GL Distribution DTO to the blanket purchase order DTO
                bpoDto.GlDistributions.Add(glDistDto);
            }

            return bpoDto;
        }
    }
}
