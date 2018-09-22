// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterComponentRangeQueryCriteriaBuilder
    {
        public CostCenterComponentRangeQueryCriteria CostCenterComponentRangeQueryCriteriaEntity;
        public string StartValue;
        public string EndValue;

        public CostCenterComponentRangeQueryCriteriaBuilder()
        {
            this.StartValue = "10";
            this.EndValue = "25";
        }

        public CostCenterComponentRangeQueryCriteriaBuilder WithStartAndEndValue(string startValue, string endValue)
        {
            this.StartValue = startValue;
            this.EndValue = endValue;
            return this;
        }

        public CostCenterComponentRangeQueryCriteria Build()
        {
            this.CostCenterComponentRangeQueryCriteriaEntity = new CostCenterComponentRangeQueryCriteria(this.StartValue, this.EndValue);
            return this.CostCenterComponentRangeQueryCriteriaEntity;
        }
    }
}