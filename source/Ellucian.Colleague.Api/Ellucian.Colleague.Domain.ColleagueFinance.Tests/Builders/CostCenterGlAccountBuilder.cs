// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterGlAccountBuilder
    {
        public CostCenterGlAccount GlAccountEntity;
        public string GlAccountNumber;
        public decimal BudgetAmount;
        public decimal ActualAmount;
        public decimal EncumbranceAmount;
        public GlBudgetPoolType PoolType;

        public CostCenterGlAccountBuilder()
        {
            this.GlAccountNumber = "10_00_11_01_0000_51000";
            this.BudgetAmount = 10000m;
            this.ActualAmount = 1000m;
            this.EncumbranceAmount = 150m;
            this.PoolType = GlBudgetPoolType.None;
        }

        public CostCenterGlAccountBuilder WithGlAccountNumber(string glNumber)
        {
            this.GlAccountNumber = glNumber;
            return this;
        }

        public CostCenterGlAccountBuilder WithBudgetAmount(decimal budget)
        {
            this.BudgetAmount = budget;
            return this;
        }

        public CostCenterGlAccountBuilder WithActualAmount(decimal actual)
        {
            this.ActualAmount = actual;
            return this;
        }

        public CostCenterGlAccountBuilder WithEncumbranceAmount(decimal encumbrance)
        {
            this.EncumbranceAmount = encumbrance;
            return this;
        }

        public CostCenterGlAccountBuilder WithPoolType(GlBudgetPoolType type)
        {
            this.PoolType = type;
            return this;
        }

        public CostCenterGlAccount Build()
        {
            this.GlAccountEntity = new CostCenterGlAccount(this.GlAccountNumber, this.PoolType);
            this.GlAccountEntity.BudgetAmount = this.BudgetAmount;
            this.GlAccountEntity.ActualAmount = this.ActualAmount;
            this.GlAccountEntity.EncumbranceAmount = this.EncumbranceAmount;

            return this.GlAccountEntity;
        }
    }
}