// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class GlBudgetPoolBuilder
    {
        public GlBudgetPool GlBudgetPoolEntity;
        private CostCenterGlAccount Umbrella;

        private CostCenterGlAccountBuilder glAccountBuilder;

        public GlBudgetPoolBuilder()
        {
            glAccountBuilder = new CostCenterGlAccountBuilder();
            this.Umbrella = glAccountBuilder.WithGlAccountNumber("10_11")
                .WithPoolType(GlBudgetPoolType.Umbrella)
                .WithBudgetAmount(1000m)
                .WithActualAmount(500m)
                .WithEncumbranceAmount(250m).Build();
        }

        public GlBudgetPoolBuilder WithUmbrella(CostCenterGlAccount umbrella)
        {
            this.Umbrella = umbrella;
            return this;
        }

        public GlBudgetPool Build()
        {
            this.GlBudgetPoolEntity = new GlBudgetPool(this.Umbrella);

            return this.GlBudgetPoolEntity;
        }
    }
}