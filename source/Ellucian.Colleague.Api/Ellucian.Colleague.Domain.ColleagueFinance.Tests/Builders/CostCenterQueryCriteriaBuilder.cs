// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Builders
{
    public class CostCenterQueryCriteriaBuilder
    {
        public CostCenterQueryCriteria CostCenterQueryCriteriaEntity;
        public string FiscalYear;
        public List<CostCenterComponentQueryCriteria> CostCenterComponentQueryCriteriaEntities;
        
        private CostCenterComponentQueryCriteriaBuilder CostCenterComponentQueryCriteriaBuilderObject = new CostCenterComponentQueryCriteriaBuilder();


        public CostCenterQueryCriteriaBuilder()
        {
            this.FiscalYear = "2016";
            this.CostCenterComponentQueryCriteriaEntities = new List<CostCenterComponentQueryCriteria>();
        }

        public CostCenterQueryCriteriaBuilder WithComponentQueryCriteria(IEnumerable<CostCenterComponentQueryCriteria> componentCriteria)
        {
            this.CostCenterComponentQueryCriteriaEntities = null;
            if (componentCriteria == null)
                this.CostCenterComponentQueryCriteriaEntities = null;
            else
                this.CostCenterComponentQueryCriteriaEntities = componentCriteria.ToList();
            return this;
        }

        public CostCenterQueryCriteria Build()
        {
            this.CostCenterQueryCriteriaEntity = new CostCenterQueryCriteria(this.CostCenterComponentQueryCriteriaEntities);
            return this.CostCenterQueryCriteriaEntity;
        }
    }
}