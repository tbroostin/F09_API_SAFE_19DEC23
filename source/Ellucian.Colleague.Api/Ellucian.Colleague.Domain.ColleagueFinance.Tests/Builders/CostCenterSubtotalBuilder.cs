// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterSubtotalBuilder
    {
        private string id;
        private CostCenterGlAccount GlAccountEntity;
        private GlBudgetPool BudgetPoolEntity;
        private GlClass glClass;
        private CostCenterGlAccountBuilder glAccountBuilderObject = new CostCenterGlAccountBuilder();
        private GlBudgetPoolBuilder budgetPoolBuilderObject = new GlBudgetPoolBuilder();

        public CostCenterSubtotalBuilder()
        {
        }

        public CostCenterSubtotalBuilder WithId(string id)
        {
            this.id = id;
            return this;
        }

        public CostCenterSubtotalBuilder WithGlAccount(CostCenterGlAccount glAccount)
        {
            this.GlAccountEntity = glAccount;
            return this;
        }

        public CostCenterSubtotalBuilder WithBudgetPool(GlBudgetPool budgetPool)
        {
            this.BudgetPoolEntity = budgetPool;
            return this;
        }

        public CostCenterSubtotalBuilder WithGlClass(GlClass glClass)
        {
            this.glClass = glClass;
            return this;
        }

        public CostCenterSubtotal BuildWithGlAccount()
        {
            return new CostCenterSubtotal(this.id, this.GlAccountEntity, this.glClass);
        }

        public CostCenterSubtotal BuildWithBudgetPool()
        {
            return new CostCenterSubtotal(this.id, this.BudgetPoolEntity, this.glClass);
        }
    }
}