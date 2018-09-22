// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterComponentQueryCriteriaBuilder
    {
        public CostCenterComponentQueryCriteria CostCenterComponentQueryCriteriaEntity;
        public string ComponentName;
        public List<string> IndividualComponentValues;
        public List<CostCenterComponentRangeQueryCriteria> RangeComponentValues;

        public CostCenterComponentQueryCriteriaBuilder()
        {
            this.ComponentName = "Fund";
            this.IndividualComponentValues = new List<string>();
            this.RangeComponentValues = new List<CostCenterComponentRangeQueryCriteria>();
        }

        public CostCenterComponentQueryCriteriaBuilder WithComponentName(string componentName)
        {
            this.ComponentName = componentName;
            return this;
        }

        public CostCenterComponentQueryCriteria Build()
        {
            this.CostCenterComponentQueryCriteriaEntity = new CostCenterComponentQueryCriteria(this.ComponentName);
            return this.CostCenterComponentQueryCriteriaEntity;
        }
    }
}