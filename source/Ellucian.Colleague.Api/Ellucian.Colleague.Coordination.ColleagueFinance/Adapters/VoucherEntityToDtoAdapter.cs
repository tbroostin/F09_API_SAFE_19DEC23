// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

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
    /// Adapter for mapping from the voucher entity to the voucher DTO.
    /// </summary>
    public class VoucherEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.Voucher, Ellucian.Colleague.Dtos.ColleagueFinance.Voucher>
    {
        public VoucherEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a voucher domain entity and all of its descendent objects into DTOs.
        /// </summary>
        /// <param name="Source">Voucher domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL numbers.</param>
        /// <returns>Voucher DTO.</returns>
        public Voucher MapToType(Domain.ColleagueFinance.Entities.Voucher Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            if (string.IsNullOrEmpty(Source.InvoiceNumber))
            {
                throw new ApplicationException("Invoice number is a required field.");
            }
            if (!Source.InvoiceDate.HasValue)
            {
                throw new ApplicationException("Invoice date is a required field.");
            }

            var voucherDto = new Dtos.ColleagueFinance.Voucher();

            // Copy the voucher-level properties.
            voucherDto.VoucherId = Source.Id;
            voucherDto.VendorId = Source.VendorId;
            voucherDto.VendorName = Source.VendorName;
            voucherDto.Amount = Source.Amount;
            voucherDto.Date = Source.Date;
            voucherDto.DueDate = Source.DueDate;
            voucherDto.MaintenanceDate = Source.MaintenanceDate;
            voucherDto.InvoiceNumber = Source.InvoiceNumber;
            voucherDto.InvoiceDate = Source.InvoiceDate.Value;
            voucherDto.CheckNumber = Source.CheckNumber;
            voucherDto.CheckDate = Source.CheckDate;
            voucherDto.Comments = Source.Comments;
            voucherDto.CurrencyCode = Source.CurrencyCode;

            voucherDto.PurchaseOrderId = Source.PurchaseOrderId;
            voucherDto.BlanketPurchaseOrderId = Source.BlanketPurchaseOrderId;
            voucherDto.RecurringVoucherId = Source.RecurringVoucherId;

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.InProgress:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.InProgress;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.NotApproved:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.NotApproved;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.Outstanding:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.Paid:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.Paid;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.Reconciled:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.Reconciled;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.Voided:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.Voided;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherStatus.Cancelled:
                    voucherDto.Status = Dtos.ColleagueFinance.VoucherStatus.Cancelled;
                    break;
            }

            voucherDto.ApType = Source.ApType;

            voucherDto.LineItems = new List<Dtos.ColleagueFinance.LineItem>();
            voucherDto.Approvers = new List<Dtos.ColleagueFinance.Approver>();

            // Initialize all necessary adapters to convert the descendent elements within the voucher.

            var lineItemDtoAdapter = new LineItemEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemGlDistributionDtoAdapter = new LineItemGlDistributionEntityToDtoAdapter(adapterRegistry, logger);
            var lineItemTaxesDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.LineItemTax, Dtos.ColleagueFinance.LineItemTax>(adapterRegistry, logger);
            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);

            // Convert the voucher line item domain entities into DTOS.
            foreach (var lineItem in Source.LineItems)
            {
                // First convert the properties to DTOs.
                var lineItemDto = lineItemDtoAdapter.MapToType(lineItem);

                // Now convert each GL distribution domain entity into a DTO.
                foreach (var glDistribution in lineItem.GlDistributions)
                {
                    var glDistributionDto = lineItemGlDistributionDtoAdapter.MapToType(glDistribution, glMajorComponentStartPositions);

                    // Add the GL distribution DTOs to the line item DTO.
                    lineItemDto.GlDistributions.Add(glDistributionDto);
                }

                // Now convert each line item tax domain entity into a DTO.
                foreach (var lineItemTax in lineItem.LineItemTaxes)
                {
                    var lineItemTaxesDto = lineItemTaxesDtoAdapter.MapToType(lineItemTax);

                    // Add the line item taxes DTOs to the line item DTO.
                    lineItemDto.LineItemTaxes.Add(lineItemTaxesDto);
                }

                // Add the voucher line item DTO to the voucher DTO.
                voucherDto.LineItems.Add(lineItemDto);
            }

            // Convert the voucher approver domain entities into DTOS.
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the voucher approver DTO to the voucher DTO.
                voucherDto.Approvers.Add(approverDto);
            }

            return voucherDto;
        }

    }
}
