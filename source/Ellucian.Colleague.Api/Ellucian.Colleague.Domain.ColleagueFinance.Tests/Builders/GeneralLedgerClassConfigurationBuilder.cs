// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GeneralLedgerClassConfigurationBuilder
    {
        private string classificationName;
        private List<string> expenseClassValues;
        private List<string> revenueClassValues;
        private List<string> assetClassValues;
        private List<string> liabilityClassValues;
        private List<string> fundBalanceClassValues;

        public GeneralLedgerClassConfigurationBuilder()
        {
            expenseClassValues = new List<string>();
            revenueClassValues = new List<string>();
            assetClassValues = new List<string>();
            liabilityClassValues = new List<string>();
            fundBalanceClassValues = new List<string>();
        }

        public GeneralLedgerClassConfigurationBuilder WithClassificationName(string className)
        {
            this.classificationName = className;
            return this;
        }

        public GeneralLedgerClassConfigurationBuilder WithExpenseClassValues(List<string> expenseClassValues)
        {
            this.expenseClassValues.Clear();
            this.expenseClassValues = expenseClassValues;
            return this;
        }

        public GeneralLedgerClassConfigurationBuilder WithRevenueClassValues(List<string> revenueClassValues)
        {
            this.revenueClassValues.Clear();
            this.revenueClassValues = revenueClassValues;
            return this;
        }

        public GeneralLedgerClassConfigurationBuilder WithAssetClassValues(List<string> assetClassValues)
        {
            this.assetClassValues.Clear();
            this.assetClassValues = assetClassValues;
            return this;
        }

        public GeneralLedgerClassConfigurationBuilder WithLiabilityClassValues(List<string> liabilityClassValues)
        {
            this.liabilityClassValues.Clear();
            this.liabilityClassValues = liabilityClassValues;
            return this;
        }

        public GeneralLedgerClassConfigurationBuilder WithFundBalanceClassValues(List<string> fundBalanceClassValues)
        {
            this.fundBalanceClassValues.Clear();
            this.fundBalanceClassValues = fundBalanceClassValues;
            return this;
        }

        public GeneralLedgerClassConfiguration Build()
        {
            return new GeneralLedgerClassConfiguration(this.classificationName,
                this.expenseClassValues, this.revenueClassValues, this.assetClassValues, this.liabilityClassValues, this.fundBalanceClassValues);
        }
    }
}