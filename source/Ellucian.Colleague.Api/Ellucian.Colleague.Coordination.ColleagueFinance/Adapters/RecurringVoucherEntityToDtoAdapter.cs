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
    /// Adapter for mapping from the recurring voucher entity to the recurring voucher DTO
    /// </summary>
    public class RecurringVoucherEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucher, Ellucian.Colleague.Dtos.ColleagueFinance.RecurringVoucher>
    {
        public RecurringVoucherEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a recurring voucher domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Recurring voucher domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions to format GL numbers</param>
        /// <returns>Recurring Voucher DTO</returns>
        public RecurringVoucher MapToType(Domain.ColleagueFinance.Entities.RecurringVoucher Source)
        {
            // Copy the recurring voucher properties
            var recurringVoucherDto = new Dtos.ColleagueFinance.RecurringVoucher();
            recurringVoucherDto.RecurringVoucherId = Source.Id;
            recurringVoucherDto.VendorId = Source.VendorId;
            recurringVoucherDto.VendorName = Source.VendorName;
            recurringVoucherDto.Amount = Source.Amount;
            recurringVoucherDto.Date = Source.Date;
            recurringVoucherDto.MaintenanceDate = Source.MaintenanceDate;
            recurringVoucherDto.InvoiceNumber = Source.InvoiceNumber;
            recurringVoucherDto.InvoiceDate = Source.InvoiceDate;
            recurringVoucherDto.Comments = Source.Comments;
            recurringVoucherDto.StatusDate = Source.StatusDate;
            recurringVoucherDto.ApType = Source.ApType;
            recurringVoucherDto.CurrencyCode = Source.CurrencyCode;
            recurringVoucherDto.TotalScheduleAmountInLocalCurrency = Source.TotalScheduleAmountInLocalCurrency;
            recurringVoucherDto.TotalScheduleTaxAmountInLocalCurrency = Source.TotalScheduleTaxAmountInLocalCurrency;

            // Translate the domain status into the DTO status
            switch (Source.Status)
            {
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucherStatus.Cancelled:
                    recurringVoucherDto.Status = Dtos.ColleagueFinance.RecurringVoucherStatus.Cancelled;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucherStatus.Closed:
                    recurringVoucherDto.Status = Dtos.ColleagueFinance.RecurringVoucherStatus.Closed;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucherStatus.NotApproved:
                    recurringVoucherDto.Status = Dtos.ColleagueFinance.RecurringVoucherStatus.NotApproved;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucherStatus.Outstanding:
                    recurringVoucherDto.Status = Dtos.ColleagueFinance.RecurringVoucherStatus.Outstanding;
                    break;
                case Ellucian.Colleague.Domain.ColleagueFinance.Entities.RecurringVoucherStatus.Voided:
                    recurringVoucherDto.Status = Dtos.ColleagueFinance.RecurringVoucherStatus.Voided;
                    break;
            }

            recurringVoucherDto.Approvers = new List<Dtos.ColleagueFinance.Approver>();

            // Initialize all necessary adapters to convert the descendent elements within the recurring voucher
            var approverDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>(adapterRegistry, logger);
            var scheduleDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.RecurringVoucherSchedule, Dtos.ColleagueFinance.RecurringVoucherSchedule>(adapterRegistry, logger);

            // Convert the recurring voucher approver domain entities into DTOS
            foreach (var approver in Source.Approvers)
            {
                var approverDto = approverDtoAdapter.MapToType(approver);

                // Add the recurring voucher approver DTO to the recurring voucher DTO
                recurringVoucherDto.Approvers.Add(approverDto);
            }

            recurringVoucherDto.Schedules = new List<RecurringVoucherSchedule>();

            // Convert the recurring voucher schedule domain entities into DTOs
            foreach (var schedule in Source.Schedules)
            {

                var scheduleDto = scheduleDtoAdapter.MapToType(schedule);
                // Add the recurring voucher schedule DTO to the recurring voucher DTO
                recurringVoucherDto.Schedules.Add(scheduleDto);
            }

            return recurringVoucherDto;
        }
    }
}
