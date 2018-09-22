// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GeneralLedgerFiscalYearConfigurationBuilder
    {
        public GeneralLedgerFiscalYearConfiguration FiscalYearConfiguration;
        public int StartMonth;
        public string CurrentYear;
        public int FiscalMonth;

        public GeneralLedgerFiscalYearConfigurationBuilder()
        {
            this.StartMonth = 6;
            this.CurrentYear = "2016";
            this.FiscalMonth = 1;
        }

        public GeneralLedgerFiscalYearConfigurationBuilder WithStartMonth(int startMonth)
        {
            this.StartMonth = startMonth;
            return this;
        }

        public GeneralLedgerFiscalYearConfigurationBuilder WithCurrentYear(string currentYear)
        {
            this.CurrentYear = currentYear;
            return this;
        }

        public GeneralLedgerFiscalYearConfigurationBuilder WithFiscalMonth(int fiscalMonth)
        {
            this.FiscalMonth = fiscalMonth;
            return this;
        }

        public GeneralLedgerFiscalYearConfiguration Build()
        {
            this.FiscalYearConfiguration = new GeneralLedgerFiscalYearConfiguration(this.StartMonth, this.CurrentYear, this.FiscalMonth, 11, "Y");
            return this.FiscalYearConfiguration;
        }
    }
}
