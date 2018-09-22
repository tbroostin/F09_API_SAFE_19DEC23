/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Person Position Wage Item
    /// </summary>
    [Serializable]
    public class PersonPositionWageItem
    {
        public string recordkey;

        public string FundingSourceId { get; set; }
        public string HourlyOrSalary { get; set; }
        public string Et { get; set; }
        public string PayRate { get; set; }
        public Decimal? RegularHours { get; set; }
        public Decimal? PeriodPay { get; set; }
        public Decimal? PercentDistribution { get; set; }
        public Decimal? ExpenseAmount { get; set; }
        public Decimal? AccrualAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public string Grade { get; set; }
        public string Step { get; set; }
        public DateTime? EndDate { get; set; }
        public List<PpwgAccountingStringAllocation> AccountingStringAllocation { get; set; }

        public PersonPositionWageItem() { }
        public PersonPositionWageItem(
            string fundingSourceId,
            string hourlyOrSalary,
            string et,
            string payRate,
            Decimal? regularHours,
            Decimal? periodPay,
            Decimal? percentDistribution,
            Decimal? expenseAmount,
            Decimal? accrualAmount)
        {
            FundingSourceId = fundingSourceId;
            HourlyOrSalary = hourlyOrSalary;
            Et = et;
            PayRate = payRate;
            RegularHours = regularHours;
            PeriodPay = periodPay;
            PercentDistribution = percentDistribution;
            ExpenseAmount = expenseAmount;
            AccrualAmount = accrualAmount;
        }

        public PersonPositionWageItem(string recordkey)
        {
            this.recordkey = recordkey;
        }
    }

    [Serializable]
    public class PpwgAccountingStringAllocation
    {
        public string GlNumber { get; set; }
        public string PpwgProjectsId { get; set; }
        public string PpwgPrjItemId { get; set; }
        public Decimal? GlPercentDistribution { get; set; }
    }
}