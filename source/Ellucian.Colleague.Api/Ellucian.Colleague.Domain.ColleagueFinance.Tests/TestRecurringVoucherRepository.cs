// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of vouchers
    /// </summary>
    public class TestRecurringVoucherRepository : IRecurringVoucherRepository
    {
        public RecurringVoucher recurringVoucher;

        public RcVouchers RcVouchers = new RcVouchers()
        {
            Recordkey = "RV1000000",
            RcvStatus = new List<string>() { "O" },
            RcvStatusDate = new List<DateTime?>() { new DateTime(2015, 04, 01) },
            RcvVendor = null,
            RcvVenName = new List<string>() { "Susty Corporation" },
            RcvExchangeRate = 1.05m,
            RcvExchangeRateDate = new DateTime(2015, 01, 01),
            RcvDate = new DateTime(2015, 04, 01),
            RcvDefaultInvoiceDate = new DateTime(2015, 04, 01),
            RcvDefaultInvoiceNo = "test rc voucher",
            RcvApType = "AP",
            RcvComments = "Nice RC voucher",
            RcvMaintGlTranDate = new DateTime(2015, 04, 01),
            RcvAuthorizations = new List<string>() { "AJK", "GTT" },
            RcvAuthorizationDates = new List<DateTime?>() { new DateTime(2015, 04, 01), new DateTime(2015, 04, 01) },
            RcvNextApprovalIds = new List<string>() { "TGL" },
            RcvItemsId = new List<string>() { "1" },
            RcvCurrencyCode = null,
            RcvRcvsId = new List<string>() { "1", "2", "3" },
        };

        public Collection<RcVouSchedules> RcVoucherSchedules = new Collection<RcVouSchedules>()
        {
            new RcVouSchedules()
            {
                RcvsScheduleDate = new DateTime(2015, 01, 01),
                RcvsItemsTotal = 100.55m,
                RcvsItemsTaxTotal = 10.11m,
                RcvsVouId = "V0000001",
            }
        };

        public Collection<Opers> Opers;

        public Collection<Items> Items;

        public TestRecurringVoucherRepository() 
        {
            CreateRecurringVoucherDomainEntity(RecurringVoucherStatus.Outstanding);

            Opers = new Collection<Opers>()
            {
                new Opers()
                {
                    // "AJK"
                    Recordkey = this.RcVouchers.RcvAuthorizations[0], SysUserName = "Andy Kleehammer"
                },
                new Opers()
                {
                    // "GTT"
                    Recordkey = this.RcVouchers.RcvAuthorizations[1], SysUserName = "Gary Thorne"
                },
                new Opers()
                {
                    // "TGL"
                    Recordkey = this.RcVouchers.RcvNextApprovalIds[0], SysUserName = "Teresa Longerbeam"
                }

            };

            Items = new Collection<Items>()
            {
                new Items()
                {
                    // "1"
                    Recordkey = "1",   
                    ItmDesc = new List<string>() {"Line item 1"},
                    ItmVouGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    VouchGlEntityAssociation = new List<ItemsVouchGl>() { new ItemsVouchGl() { ItmVouGlNoAssocMember = "1000005308001" } },
                    ItmVouGlAmt = new List<decimal?>() {50m, 150m},
                    ItmVouGlForeignAmt = new List<decimal?>() {100m, 300m},
                    ItmVouLineGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    ItmVouGlTaxAmt = new List<decimal?>() {5m, 15m},
                    ItmVouGlForeignTaxAmt = new List<decimal?>() {10m, 30m}
                },
                new Items()
                {
                    // "2"
                    Recordkey = "2",
                    ItmDesc = new List<string>() {"Line item 2"},
                    ItmVouGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    ItmVouGlAmt = new List<decimal?>() {40m, 60m},
                    ItmVouGlForeignAmt = new List<decimal?>() {80m, 120m},
                    ItmVouLineGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    ItmVouGlTaxAmt = new List<decimal?>() {4m, 6m},
                    ItmVouGlForeignTaxAmt = new List<decimal?>() {8m, 12m}
                },
                new Items()
                {
                    // "3"
                    Recordkey = "3",
                    ItmDesc = new List<string>() {"Line item 3"},
                    ItmVouGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    ItmVouGlAmt = new List<decimal?>() {null, null},
                    ItmVouGlForeignAmt = new List<decimal?>() {null, null},
                    ItmVouLineGlNo = new List<string>() {"1000005308001", "1010005308001"},
                    ItmVouGlTaxAmt = new List<decimal?>() {null, null},
                    ItmVouGlForeignTaxAmt = new List<decimal?>() {null, null}
                }
            };
        }

        public RecurringVoucher CreateRecurringVoucherDomainEntity(RecurringVoucherStatus status)
        {
            var scheduleBuilder = new RecurringVoucherScheduleBuilder();

            recurringVoucher = new RecurringVoucher("RV0001000", new DateTime(2015, 04, 01), status, new DateTime(2015, 04, 01), "Susty Corporation", "Invoice 1", new DateTime(2015, 04, 01));
            recurringVoucher.ApType = "AP";
            recurringVoucher.Comments = "nice recurring voucher.";
            recurringVoucher.MaintenanceDate = new DateTime(2015, 04, 01);
            recurringVoucher.VendorId = "0003949";
            recurringVoucher.CurrencyCode = "CAD";
            recurringVoucher.ExchangeRate = 1.05m;
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddSchedule(scheduleBuilder.Build());
            recurringVoucher.AddApprover(new Approver("AJK") { ApprovalDate = DateTime.Now });
            recurringVoucher.AddApprover(new Approver("TGL") { ApprovalDate = DateTime.Now });
            recurringVoucher.AddApprover(new Approver("GTT") { ApprovalDate = DateTime.Now });
            recurringVoucher.AddApprover(new Approver("JTS"));
            recurringVoucher.AddLineItem(new LineItem("1", "line 1", 0, 0, 0));
            return recurringVoucher;
        }

        public async Task<RecurringVoucher> GetRecurringVoucherAsync(string recurringVoucherId, GlAccessLevel glAccessLevel, IEnumerable<string> glAccessAccounts)
        {
            if (!string.IsNullOrEmpty(recurringVoucherId))
            {
                return await Task.Run(() => this.recurringVoucher);
            }
            else
            {
                return null;
            }
        }

        public RcVouchers GetRcVouchersDataContract()
        {
            this.RcVouchers.buildAssociations();
            return this.RcVouchers;
        }

        public Collection<Items> GetItemsDataContract()
        {
            foreach (var item in Items)
            {
                item.buildAssociations();
            }
            return this.Items;

        }

    }
}
